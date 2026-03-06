using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GalaxyClassifierCapstone.Core.Interfaces;
using GalaxyClassifierCapstone.Core.Models;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace GalaxyClassifierCapstone.ViewModels;

public partial class EvaluationViewModel : ObservableObject
{
    private readonly ISnackbarService _snackbarService;
    private readonly IClassificationService _classificationService;

    [ObservableProperty] private string _datasetPath = string.Empty;
    [ObservableProperty] private bool _isEvaluating;

    [ObservableProperty] private double _microAccuracy;
    [ObservableProperty] private double _macroAccuracy;
    [ObservableProperty] private double _logLoss;

    [ObservableProperty] private ObservableCollection<ConfusionRow> _confusionRows = new();
    [ObservableProperty] private ObservableCollection<string> _confusionColumnHeaders = new();

    private EvaluationResult? _lastEvaluation;

    [ObservableProperty] private ObservableCollection<TopMistakeItem> _topMistakes = new();

    public EvaluationViewModel(ISnackbarService snackbarService, IClassificationService classificationService)
    {
        _snackbarService = snackbarService;
        _classificationService = classificationService;
    }

    [RelayCommand]
    private void BrowseDatasetFolder()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Select Evaluation Dataset Folder"
        };

        if (dialog.ShowDialog() == true)
        {
            DatasetPath = dialog.FolderName;
        }
    }

    [RelayCommand(CanExecute = nameof(CanEvaluate))]
    private async Task Evaluate()
    {
        if (!_classificationService.IsModelLoaded())
        {
            ShowSnackbar("Evaluation", "No model loaded. Train or load a model first.", ControlAppearance.Caution);
            return;
        }

        if (string.IsNullOrWhiteSpace(DatasetPath) || !Directory.Exists(DatasetPath))
        {
            ShowSnackbar("Evaluation", "Dataset folder not found.", ControlAppearance.Danger);
            return;
        }

        IsEvaluating = true;

        try
        {
            var result = await _classificationService.EvaluateAsync(DatasetPath);
            Apply(result);

            ShowSnackbar("Evaluation", "Evaluation complete.", ControlAppearance.Success);
        }
        catch (Exception ex)
        {
            ShowSnackbar("Evaluation", $"Evaluation failed: {ex.Message}", ControlAppearance.Danger);
        }
        finally
        {
            IsEvaluating = false;
        }
    }

    private bool CanEvaluate() => !IsEvaluating;

    [RelayCommand(CanExecute = nameof(CanExportMisclassifications))]
    private async Task ExportMisclassifications()
    {
        if (_lastEvaluation == null)
        {
            ShowSnackbar("Export", "Run evaluation first.", ControlAppearance.Info);
            return;
        }

        if (_lastEvaluation.Misclassifications.Length == 0)
        {
            ShowSnackbar("Export", "No misclassifications to export.", ControlAppearance.Info);
            return;
        }

        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Select export folder"
        };

        if (dialog.ShowDialog() != true)
            return;

        var exportRoot = Path.Combine(dialog.FolderName, "misclassified");

        try
        {
            await Task.Run(() =>
            {
                Directory.CreateDirectory(exportRoot);

                foreach (var m in _lastEvaluation.Misclassifications)
                {
                    if (string.IsNullOrWhiteSpace(m.ImagePath) || !File.Exists(m.ImagePath))
                        continue;

                    var trueLabel = SanitizePathPart(m.TrueLabel);
                    var predLabel = SanitizePathPart(m.PredictedLabel);

                    var destDir = Path.Combine(exportRoot, trueLabel, predLabel);
                    Directory.CreateDirectory(destDir);

                    var fileName = Path.GetFileName(m.ImagePath);
                    var destPath = Path.Combine(destDir, fileName);

                    // Avoid overwriting if collisions happen.
                    if (File.Exists(destPath))
                    {
                        var name = Path.GetFileNameWithoutExtension(fileName);
                        var ext = Path.GetExtension(fileName);
                        destPath = Path.Combine(destDir, $"{name}_{Guid.NewGuid():N}{ext}");
                    }

                    File.Copy(m.ImagePath, destPath, overwrite: false);
                }
            });

            ShowSnackbar("Export", $"Exported to: {exportRoot}", ControlAppearance.Success);
        }
        catch (Exception ex)
        {
            ShowSnackbar("Export", $"Export failed: {ex.Message}", ControlAppearance.Danger);
        }
    }

    private bool CanExportMisclassifications() => !IsEvaluating && _lastEvaluation != null;

    partial void OnIsEvaluatingChanged(bool value)
    {
        EvaluateCommand.NotifyCanExecuteChanged();
        ExportMisclassificationsCommand.NotifyCanExecuteChanged();
    }

    private void Apply(EvaluationResult result)
    {
        _lastEvaluation = result;

        MicroAccuracy = result.MicroAccuracy;
        MacroAccuracy = result.MacroAccuracy;
        LogLoss = result.LogLoss;

        ConfusionColumnHeaders = new ObservableCollection<string>(result.Labels);

        var rows = new ObservableCollection<ConfusionRow>();
        for (var i = 0; i < result.Labels.Length; i++)
        {
            var row = new ConfusionRow
            {
                ActualLabel = result.Labels[i]
            };

            var counts = (i < result.ConfusionMatrix.Length) ? result.ConfusionMatrix[i] : Array.Empty<int>();
            row.Counts = new ObservableCollection<int>(counts);
            rows.Add(row);
        }

        ConfusionRows = rows;

        TopMistakes = new ObservableCollection<TopMistakeItem>(
            result.Misclassifications
                .GroupBy(m => new { m.TrueLabel, m.PredictedLabel })
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => new TopMistakeItem
                {
                    TrueLabel = g.Key.TrueLabel,
                    PredictedLabel = g.Key.PredictedLabel,
                    Count = g.Count(),
                    AvgConfidence = g.Average(x => x.Confidence)
                }));

        ExportMisclassificationsCommand.NotifyCanExecuteChanged();
    }

    private static string SanitizePathPart(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "unknown";

        foreach (var c in Path.GetInvalidFileNameChars())
            value = value.Replace(c, '_');

        return value.Trim();
    }

    private void ShowSnackbar(string title, string message, ControlAppearance appearance)
    {
        _snackbarService.Show(title, message, appearance,
            new SymbolIcon { Symbol = SymbolRegular.Info28 }, TimeSpan.FromSeconds(4));
    }
}

public partial class ConfusionRow : ObservableObject
{
    [ObservableProperty] private string _actualLabel = string.Empty;
    [ObservableProperty] private ObservableCollection<int> _counts = new();
}

public sealed class TopMistakeItem
{
    public string TrueLabel { get; set; } = string.Empty;
    public string PredictedLabel { get; set; } = string.Empty;
    public int Count { get; set; }
    public double AvgConfidence { get; set; }
}
