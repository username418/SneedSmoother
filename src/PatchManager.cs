using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Windows.Controls;
using System;

namespace PoeFixer;

public class PatchManager
{
    public HashSet<string> patchedFiles = [];

    // Index ambiguous between steam and normal client.
    public LibBundle3.Index index;

    // Paths ending with "/".
    public string CachePath { get; set; }
    public string ModifiedCachePath { get; set; }

    public Dictionary<string, bool> bools = [];
    public Dictionary<string, float> floats = [];

    public MainWindow window;

    public PatchManager(LibBundle3.Index index, MainWindow window)
    {
        this.index = index ?? throw new ArgumentNullException(nameof(index));
        this.window = window ?? throw new ArgumentNullException(nameof(window));
        
        CachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extractedassets", "");
        ModifiedCachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "modifiedassets", "");
    }

    public int RestoreExtractedAssets()
    {
        try
        {
            if (!Directory.Exists(CachePath))
            {
                window.EmitToConsole($"Cache directory not found: {CachePath}");
                return 0;
            }

            var patchZipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "patch.zip");
            
            // Clean up existing patch file
            if (File.Exists(patchZipPath))
            {
                File.Delete(patchZipPath);
            }

            // Create zip archive
            ZipFile.CreateFromDirectory(CachePath, patchZipPath);
            
            using var archive = ZipFile.OpenRead(patchZipPath);
            int count = LibBundle3.Index.Replace(index, archive.Entries);
            
            // Clean up patch file
            if (File.Exists(patchZipPath))
            {
                File.Delete(patchZipPath);
            }
            
            return count;
        }
        catch (Exception ex)
        {
            App.LogException(ex, "Error in RestoreExtractedAssets");
            window.EmitToConsole($"Error restoring assets: {ex.Message}");
            return 0;
        }
    }

    public void CollectSettings()
    {
        try
        {
            bools.Clear();
            floats.Clear();

            // Find every named checkbox in the window.
            foreach (Control control in window.mainGrid.Children)
            {
                if (control is CheckBox checkbox && !string.IsNullOrEmpty(checkbox.Name))
                {
                    bools[checkbox.Name] = checkbox.IsChecked == true;
                }

                if (control is Slider slider && !string.IsNullOrEmpty(slider.Name))
                {
                    floats[slider.Name] = (float)slider.Value;
                }
            }
        }
        catch (Exception ex)
        {
            App.LogException(ex, "Error in CollectSettings");
            window.EmitToConsole($"Error collecting settings: {ex.Message}");
        }
    }

    /// <summary>
    /// Main patch method.
    /// </summary>
    public int Patch()
    {
        try
        {
            // Get every class implementing the IPatch interface.
            Type[] patchTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => x.GetInterfaces().Contains(typeof(IPatch)))
                .ToArray();

            if (patchTypes.Length == 0)
            {
                window.EmitToConsole("No patch types found.");
                return 0;
            }

            CollectSettings();

            // Instantiate every patch, only keep enabled ones.
            List<IPatch> enabledPatches = new List<IPatch>();
            
            foreach (var patchType in patchTypes)
            {
                try
                {
                    var patch = (IPatch)Activator.CreateInstance(patchType)!;
                    if (patch.ShouldPatch(bools, floats))
                    {
                        enabledPatches.Add(patch);
                    }
                }
                catch (Exception ex)
                {
                    App.LogException(ex, $"Error creating patch instance: {patchType.Name}");
                    window.EmitToConsole($"Error creating patch {patchType.Name}: {ex.Message}");
                }
            }

            if (enabledPatches.Count == 0)
            {
                window.EmitToConsole("No patches enabled.");
                return 0;
            }

            // Ensure cache directory exists
            if (!Directory.Exists(CachePath))
            {
                window.EmitToConsole($"Cache directory not found: {CachePath}");
                return 0;
            }

            // Delete and recreate modified file directory.
            if (Directory.Exists(ModifiedCachePath))
            {
                Directory.Delete(ModifiedCachePath, true);
            }
            Directory.CreateDirectory(ModifiedCachePath);

            // Modify files.
            foreach (IPatch patch in enabledPatches)
            {
                try
                {
                    Stopwatch stopWatch = Stopwatch.StartNew();

                    foreach (string file in patch.FilesToPatch)
                    {
                        TryModifyFile(file, patch);
                    }

                    foreach (string directory in patch.DirectoriesToPatch)
                    {
                        string[] extensions = patch.Extension.Split('|');

                        foreach (string extension in extensions)
                        {
                            if (!string.IsNullOrEmpty(extension))
                            {
                                ModifyDirectory(directory, extension, patch);
                            }
                        }
                    }

                    stopWatch.Stop();
                    window.EmitToConsole($"{patch.GetType().Name} patched in {(int)stopWatch.Elapsed.TotalMilliseconds}ms.");
                }
                catch (Exception ex)
                {
                    App.LogException(ex, $"Error applying patch: {patch.GetType().Name}");
                    window.EmitToConsole($"Error applying patch {patch.GetType().Name}: {ex.Message}");
                }
            }

            // Create final patch zip
            var patchZipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "patch.zip");
            
            if (File.Exists(patchZipPath))
            {
                File.Delete(patchZipPath);
            }

            if (!Directory.Exists(ModifiedCachePath) || !Directory.EnumerateFileSystemEntries(ModifiedCachePath).Any())
            {
                window.EmitToConsole("No modified assets to apply.");
                return 0;
            }

            ZipFile.CreateFromDirectory(ModifiedCachePath, patchZipPath);
            
            using var archive = ZipFile.OpenRead(patchZipPath);
            int count = LibBundle3.Index.Replace(index, archive.Entries);
            
            // Keep patch.zip for debugging - don't delete it
            return count;
        }
        catch (Exception ex)
        {
            App.LogException(ex, "Error in Patch");
            window.EmitToConsole($"Error applying patches: {ex.Message}");
            return 0;
        }
    }

    public void ModifyDirectory(string path, string extension, IPatch patch)
    {
        try
        {
            var searchPath = Path.Combine(CachePath, path);
            
            if (!Directory.Exists(searchPath))
            {
                window.EmitToConsole($"Directory not found: {searchPath}");
                return;
            }

            IEnumerable<string> files = Directory.EnumerateFiles(searchPath, extension, SearchOption.AllDirectories);

            foreach (string file in files)
            {
                ModifyFile(file, patch);
            }
        }
        catch (Exception ex)
        {
            App.LogException(ex, $"Error in ModifyDirectory: {path}");
            window.EmitToConsole($"Error modifying directory {path}: {ex.Message}");
        }
    }

    public void TryModifyFile(string path, IPatch patch)
    {
        try
        {
            var fullPath = Path.Combine(CachePath, path);
            if (File.Exists(fullPath))
            {
                ModifyFile(fullPath, patch);
            }
            else
            {
                window.EmitToConsole($"File not found: {fullPath}");
            }
        }
        catch (Exception ex)
        {
            App.LogException(ex, $"Error in TryModifyFile: {path}");
            window.EmitToConsole($"Error trying to modify file {path}: {ex.Message}");
        }
    }

    public void ModifyFile(string path, IPatch patch)
    {
        try
        {
            if (!File.Exists(path))
            {
                window.EmitToConsole($"File not found: {path}");
                return;
            }

            bool patchModifiedAsset = patchedFiles.Contains(path);

            // Grab this file from the modified cache if it was modified already.
            string currentPath = patchModifiedAsset ? path.Replace(CachePath, ModifiedCachePath) : path;

            string text = File.ReadAllText(currentPath);

            string? modifiedText = patch.PatchFile(text);
            if (modifiedText == null) return;

            // Determine output path
            string outputPath = patchModifiedAsset ? currentPath : path.Replace(CachePath, ModifiedCachePath);

            // Ensure output directory exists
            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            // Choose encoding based on file extension
            Encoding encoding = Path.GetExtension(outputPath).ToLower() == ".hlsl" ? Encoding.ASCII : Encoding.Unicode;
            
            File.WriteAllText(outputPath, modifiedText, encoding);

            if (!patchModifiedAsset)
            {
                patchedFiles.Add(path);
            }
        }
        catch (Exception ex)
        {
            App.LogException(ex, $"Error in ModifyFile: {path}");
            window.EmitToConsole($"Error modifying file {path}: {ex.Message}");
        }
    }
}