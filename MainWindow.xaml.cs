using LibBundledGGPK3;
using Microsoft.Win32;
using System.Diagnostics;
using System.Windows;
using System;
using System.IO;

namespace PoeFixer;

public partial class MainWindow : Window
{
    public string? GGPKPath { get; set; }

    public MainWindow()
    {
        InitializeComponent();
    }

    public void EmitToConsole(string line)
    {
        try
        {
            // Ensure we're on the UI thread
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => EmitToConsole(line));
                return;
            }

            Console.Text += line + "\n";
            Console.ScrollToEnd();
        }
        catch (Exception ex)
        {
            App.LogException(ex, "Error in EmitToConsole");
        }
    }

    private void RestoreExtractedAssets(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(GGPKPath))
            {
                EmitToConsole("GGPK is not selected.");
                return;
            }

            if (!File.Exists(GGPKPath))
            {
                EmitToConsole($"GGPK file not found: {GGPKPath}");
                return;
            }

            EmitToConsole("Starting asset restoration...");

            // Check if ggpk extension is .ggpk.
            if (GGPKPath.EndsWith(".ggpk"))
            {
                using BundledGGPK ggpk = new(GGPKPath);
                PatchManager manager = new(ggpk.Index, this);
                int count = manager.RestoreExtractedAssets();
                EmitToConsole($"{count} assets restored successfully.");
            }
            else if (GGPKPath.EndsWith(".bin"))
            {
                using LibBundle3.Index index = new(GGPKPath);
                PatchManager manager = new(index, this);
                int count = manager.RestoreExtractedAssets();
                EmitToConsole($"{count} assets restored successfully.");
            }
            else
            {
                EmitToConsole("Invalid file format. Please select a .ggpk or .bin file.");
            }
        }
        catch (Exception ex)
        {
            App.LogException(ex, "Error in RestoreExtractedAssets");
            EmitToConsole($"Error restoring assets: {ex.Message}");
        }
    }

    private void ExtractVanillaAssets(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(GGPKPath))
            {
                EmitToConsole("GGPK is not selected.");
                return;
            }

            if (!File.Exists(GGPKPath))
            {
                EmitToConsole($"GGPK file not found: {GGPKPath}");
                return;
            }

            EmitToConsole("Starting asset extraction...");

            if (GGPKPath.EndsWith(".ggpk"))
            {
                using BundledGGPK ggpk = new(GGPKPath);
                FileExtractor extractor = new(ggpk.Index);
                int count = extractor.ExtractFiles();
                EmitToConsole($"{count} assets extracted successfully.");
            }
            else if (GGPKPath.EndsWith(".bin"))
            {
                using LibBundle3.Index index = new(GGPKPath);
                FileExtractor extractor = new(index);
                int count = extractor.ExtractFiles();
                EmitToConsole($"{count} assets extracted successfully.");
            }
            else
            {
                EmitToConsole("Invalid file format. Please select a .ggpk or .bin file.");
            }
        }
        catch (Exception ex)
        {
            App.LogException(ex, "Error in ExtractVanillaAssets");
            EmitToConsole($"Error extracting assets: {ex.Message}");
        }
    }

    private void SelectGGPK(object sender, RoutedEventArgs e)
    {
        try
        {
            // Open file dialogue to select either a .ggpk or .bin file.
            OpenFileDialog dlg = new()
            {
                DefaultExt = ".ggpk",
                Filter = "GGPK Files (*.ggpk, *.bin)|*.ggpk;*.bin",
                CheckFileExists = true,
                Title = "Select GGPK or BIN file"
            };

            if (dlg.ShowDialog() == true)
            {
                GGPKPath = dlg.FileName;
                EmitToConsole($"GGPK selected: {GGPKPath}");
                
                // Validate the selected file
                if (!File.Exists(GGPKPath))
                {
                    EmitToConsole("Warning: Selected file does not exist.");
                    GGPKPath = null;
                }
            }
        }
        catch (Exception ex)
        {
            App.LogException(ex, "Error in SelectGGPK");
            EmitToConsole($"Error selecting GGPK file: {ex.Message}");
        }
    }

    private void PatchGGPK(object sender, RoutedEventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(GGPKPath))
            {
                EmitToConsole("GGPK is not selected.");
                return;
            }

            if (!File.Exists(GGPKPath))
            {
                EmitToConsole($"GGPK file not found: {GGPKPath}");
                return;
            }

            EmitToConsole("Patching GGPK...");
            Stopwatch sw = Stopwatch.StartNew();

            if (GGPKPath.EndsWith(".ggpk"))
            {
                using BundledGGPK ggpk = new(GGPKPath);
                PatchManager manager = new(ggpk.Index, this);
                int count = manager.Patch();
                EmitToConsole($"{count} assets patched successfully.");
            }
            else if (GGPKPath.EndsWith(".bin"))
            {
                using LibBundle3.Index index = new(GGPKPath);
                PatchManager manager = new(index, this);
                int count = manager.Patch();
                EmitToConsole($"{count} assets patched successfully.");
            }
            else
            {
                EmitToConsole("Invalid file format. Please select a .ggpk or .bin file.");
                return;
            }

            sw.Stop();
            EmitToConsole($"GGPK patched in {(int)sw.Elapsed.TotalMilliseconds}ms.");
        }
        catch (Exception ex)
        {
            App.LogException(ex, "Error in PatchGGPK");
            EmitToConsole($"Error patching GGPK: {ex.Message}");
        }
    }
}