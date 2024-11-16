using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using Windows.Storage;
using WinRT;

namespace MicaForEveryone.App.Helpers;

public sealed partial class WinRTFileSystemWatcher : IDisposable
{
    internal sealed partial class WinRTInternals
    {
        [Flags]
        internal enum HANDLE_ACCESS_OPTIONS
        {
            HAO_NONE = 0,
            HAO_READ_ATTRIBUTES = 0x80,
            HAO_READ = 0x120089,
            HAO_WRITE = 0x120116,
            HAO_DELETE = 0x10000
        }

        [Flags]
        internal enum HANDLE_SHARING_OPTIONS
        {
            HSO_SHARE_NONE = 0,
            HSO_SHARE_READ = 0x1,
            HSO_SHARE_WRITE = 0x2,
            HSO_SHARE_DELETE = 0x4
        }

        [Flags]
        internal enum HANDLE_OPTIONS : uint
        {
            HO_NONE = 0,
            HO_OPEN_REQUIRING_OPLOCK = 0x40000,
            HO_DELETE_ON_CLOSE = 0x4000000,
            HO_SEQUENTIAL_SCAN = 0x8000000,
            HO_RANDOM_ACCESS = 0x10000000,
            HO_NO_BUFFERING = 0x20000000,
            HO_OVERLAPPED = 0x40000000,
            HO_WRITE_THROUGH = 0x80000000
        }

        [GeneratedComInterface]
        [Guid("5ca296b2-2c25-4d22-b785-b885c8201e6a")]
        internal partial interface IStorageItemHandleAccess
        {
            IntPtr Create(HANDLE_ACCESS_OPTIONS accessOptions, HANDLE_SHARING_OPTIONS sharingOptions, HANDLE_OPTIONS options, IntPtr oplockBreakingHandler);
        }
    }

    private sealed class AsyncReadState
    {
        internal AsyncReadState(byte[] buffer, SafeFileHandle handle, ThreadPoolBoundHandle binding, WinRTFileSystemWatcher parent)
        {
            Buffer = buffer;
            DirectoryHandle = handle;
            ThreadPoolBinding = binding;
            WeakWatcher = new WeakReference<WinRTFileSystemWatcher>(parent);
        }

        internal byte[] Buffer { get; }
        internal SafeFileHandle DirectoryHandle { get; }
        internal ThreadPoolBoundHandle ThreadPoolBinding { get; }
        internal PreAllocatedOverlapped? PreAllocatedOverlapped { get; set; }
        internal WeakReference<WinRTFileSystemWatcher> WeakWatcher { get; }
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private readonly struct FILE_NOTIFY_INFORMATION
    {
        internal readonly uint NextEntryOffset;
        internal readonly FileAction Action;

        // The size of FileName portion of the record, in bytes. The value does not include the terminating null character.
        internal readonly uint FileNameLength;

        // A variable-length field that contains the file name. This field is part of Windows SDK definition of this structure.
        // It is intentionally omitted in the managed definition given how it is used.
        // internal readonly fixed char FileName[1];
    }

    public enum FileAction : uint
    {
        FILE_ACTION_ADDED = 0x00000001,
        FILE_ACTION_REMOVED = 0x00000002,
        FILE_ACTION_MODIFIED = 0x00000003,
        FILE_ACTION_RENAMED_OLD_NAME = 0x00000004,
        FILE_ACTION_RENAMED_NEW_NAME = 0x00000005
    }

    [Flags]
    public enum NotifyFilters : uint
    {
        FileName = 0x00000001,
        DirectoryName = 0x00000002,
        Attributes = 0x00000004,
        Size = 0x00000008,
        LastWrite = 0x00000010,
        LastAccess = 0x00000020,
        CreationTime = 0x00000040,
        Security = 0x00000100,
    }

    [LibraryImport("kernel32")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static unsafe partial bool ReadDirectoryChangesW(SafeFileHandle hDirectory, byte[] lpBuffer, uint nBufferLength, [MarshalAs(UnmanagedType.Bool)] bool bWatchSubtree, NotifyFilters dwNotifyFilter, uint* lpBytesReturned, NativeOverlapped* lpOverlapped, delegate* unmanaged<uint, uint, NativeOverlapped*, void> lpCompletionRoutine);

    private SafeFileHandle? DirectoryHandle;

    private NotifyFilters filters;

    private bool includeSubdirectories;

    public WinRTFileSystemWatcher(
        StorageFolder folder, 
        NotifyFilters filters = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes | NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.CreationTime | NotifyFilters.Security,
        bool includeSubdirectories = true)
    {
        this.filters = filters;
        this.includeSubdirectories = includeSubdirectories;

        var folderHandleAccess = (WinRTInternals.IStorageItemHandleAccess)(object)folder;
        IntPtr folderHandle = folderHandleAccess.Create(WinRTInternals.HANDLE_ACCESS_OPTIONS.HAO_READ, WinRTInternals.HANDLE_SHARING_OPTIONS.HSO_SHARE_READ | WinRTInternals.HANDLE_SHARING_OPTIONS.HSO_SHARE_WRITE | WinRTInternals.HANDLE_SHARING_OPTIONS.HSO_SHARE_DELETE, WinRTInternals.HANDLE_OPTIONS.HO_OVERLAPPED, IntPtr.Zero);
        DirectoryHandle = new(folderHandle, true);
        byte[] buffer = new byte[8192];

        AsyncReadState state = new AsyncReadState(buffer, DirectoryHandle, ThreadPoolBoundHandle.BindHandle(DirectoryHandle), this);
        unsafe
        {
            state.PreAllocatedOverlapped = new PreAllocatedOverlapped((errorCode, numBytes, overlappedPointer) =>
            {
                AsyncReadState state = (AsyncReadState)ThreadPoolBoundHandle.GetNativeOverlappedState(overlappedPointer)!;
                state.ThreadPoolBinding.FreeNativeOverlapped(overlappedPointer);
                if (state.WeakWatcher.TryGetTarget(out WinRTFileSystemWatcher? watcher))
                {
                    watcher.ReadDirectoryChangesCallback(errorCode, numBytes, state);
                }
            }, state, state.Buffer);
        }
        Monitor(state);
    }

    private unsafe void Monitor(AsyncReadState state)
    {
        NativeOverlapped* overlappedPointer = state.ThreadPoolBinding.AllocateNativeOverlapped(state.PreAllocatedOverlapped!);
        bool continueExecuting = ReadDirectoryChangesW(
            state.DirectoryHandle,
            state.Buffer, // the buffer is kept pinned for the duration of the sync and async operation by the PreAllocatedOverlapped
            (uint)state.Buffer.Length,
            includeSubdirectories,
            filters,
            null,
            overlappedPointer,
            null);
    }

    private unsafe void ReadDirectoryChangesCallback(uint errorCode, uint numBytes, AsyncReadState state)
    {
        ReadOnlySpan<byte> buffer = state.Buffer.AsSpan().Slice(0, (int)numBytes);

        if (state.DirectoryHandle.IsInvalid)
            return;

        if (errorCode != 0)
        {
            return;
        }

        if (numBytes == 0)
        {
            return;
        }

        ReadOnlySpan<char> oldName = ReadOnlySpan<char>.Empty;

        // Parse each event from the buffer and notify appropriate delegates
        while (true)
        {
            // Validate the data we received isn't truncated. This can happen if we are watching files over
            // the network with a poor connection (https://github.com/dotnet/runtime/issues/40412).
            if (sizeof(FILE_NOTIFY_INFORMATION) > (uint)buffer.Length)
            {
                // This is defensive check. We do not expect to receive truncated data under normal circumstances.
                break;
            }
            ref readonly FILE_NOTIFY_INFORMATION info = ref MemoryMarshal.AsRef<FILE_NOTIFY_INFORMATION>(buffer);

            // Validate the data we received isn't truncated.
            if (info.FileNameLength > (uint)buffer.Length - sizeof(FILE_NOTIFY_INFORMATION))
            {
                // This is defensive check. We do not expect to receive truncated data under normal circumstances.
                break;
            }
            ReadOnlySpan<char> fileName = MemoryMarshal.Cast<byte, char>(
                buffer.Slice(sizeof(FILE_NOTIFY_INFORMATION), (int)info.FileNameLength));

            Changed?.Invoke(this, info.Action, fileName.ToString());

            if (info.NextEntryOffset == 0)
            {
                break;
            }

            // Validate the data we received isn't truncated.
            if (info.NextEntryOffset > (uint)buffer.Length)
            {
                // This is defensive check. We do not expect to receive truncated data under normal circumstances.
                break;
            }

            buffer = buffer.Slice((int)info.NextEntryOffset);
        }

        Monitor(state);
    }

    public void Dispose()
    {
        if (DirectoryHandle is { IsInvalid: false, IsClosed: false })
        {
            SafeFileHandle handle = DirectoryHandle;
            DirectoryHandle = null;
            handle.Dispose();
        }
    }

    public delegate void DirectoryChangeEventHandler(WinRTFileSystemWatcher watcher, FileAction action, string fileName);

    public event DirectoryChangeEventHandler? Changed;
}