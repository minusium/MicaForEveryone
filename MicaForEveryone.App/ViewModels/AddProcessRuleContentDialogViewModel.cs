using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MicaForEveryone.CoreUI;
using MicaForEveryone.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MicaForEveryone.App.ViewModels;

public partial class AddProcessRuleContentDialogViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAddButtonEnabled))]
    public partial string ProcessName { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<string> Recommendations { get; set; }

    public bool IsAddButtonEnabled => !string.IsNullOrWhiteSpace(ProcessName);

    private readonly ISettingsService _settingsService;

    public AddProcessRuleContentDialogViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        ProcessName = string.Empty;
        Recommendations = new ObservableCollection<string>();
    }

    public void RequestSuggestions()
    {
        if (string.IsNullOrWhiteSpace(ProcessName))
        {
            Recommendations.Clear();
            return;
        }

        IEnumerable<string> newRecommendations = Process
            .GetProcesses()
            .Select(f => f.ProcessName)
            .Where(f => f.StartsWith(ProcessName, System.StringComparison.CurrentCultureIgnoreCase))
            .Distinct();
        IEnumerable<string> toRemove = Recommendations.Except(newRecommendations).ToArray();
        IEnumerable<string> toAdd = newRecommendations.Except(Recommendations).ToArray();

        foreach (string recommendationToRemove in toRemove)
        {
            Recommendations.Remove(recommendationToRemove);
        }

        foreach (string recommendationToAdd in toAdd)
        {
            Recommendations.Add(recommendationToAdd);
        }
    }

    [RelayCommand]
    private async Task AddRuleAsync()
    {
        _settingsService.Settings!.Rules.Insert(1, new ProcessRule() { ProcessName = ProcessName });
        await _settingsService.SaveAsync();
    }
}
