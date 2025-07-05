using LibBundle3.Nodes;
using Newtonsoft.Json;
using System.IO;
using System;

namespace PoeFixer;

public class FileExtractor
{
    public LibBundle3.Index index;

    public const string extractJsonPath = "paths_to_extract.json";

    public FileExtractor(LibBundle3.Index index)
    {
        this.index = index ?? throw new ArgumentNullException(nameof(index));
    }

    /// <summary>
    /// Extracts vanilla assets.
    /// </summary>
    /// <returns>Number of files extracted.</returns>
    public int ExtractFiles()
    {
        try
        {
            // Validate JSON file exists
            if (!File.Exists(extractJsonPath))
            {
                App.LogError($"Extract paths file not found: {extractJsonPath}");
                return 0;
            }

            string cachePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "extractedassets", "");

            // Clean up existing cache directory
            if (Directory.Exists(cachePath))
            {
                Directory.Delete(cachePath, true);
            }

            // Read and parse JSON
            string jsonContent = File.ReadAllText(extractJsonPath);
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                App.LogError("Extract paths file is empty");
                return 0;
            }

            PathData? pathData = JsonConvert.DeserializeObject<PathData>(jsonContent);
            if (pathData?.paths == null || pathData.paths.Length == 0)
            {
                App.LogError("No paths found in extract configuration");
                return 0;
            }

            int totalCount = 0;

            foreach (string path in pathData.paths)
            {
                try
                {
                    if (string.IsNullOrEmpty(path))
                    {
                        continue;
                    }

                    bool found = index.TryFindNode(path, out ITreeNode? node);
                    if (!found || node == null)
                    {
                        App.LogError($"Node not found in index: {path}");
                        continue;
                    }

                    string directory = Path.GetDirectoryName(path) ?? "";
                    string fullOutputPath = Path.Combine(cachePath, directory);

                    // Ensure output directory exists
                    if (!Directory.Exists(fullOutputPath))
                    {
                        Directory.CreateDirectory(fullOutputPath);
                    }

                    int count = LibBundle3.Index.ExtractParallel(node, fullOutputPath);
                    totalCount += count;
                }
                catch (Exception ex)
                {
                    App.LogException(ex, $"Error extracting path: {path}");
                    // Continue with other paths
                }
            }

            return totalCount;
        }
        catch (JsonException ex)
        {
            App.LogException(ex, "Error parsing extract paths JSON");
            return 0;
        }
        catch (Exception ex)
        {
            App.LogException(ex, "Error in ExtractFiles");
            return 0;
        }
    }
}