using Fabrics.Test.Entities;
using HitachiQA;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Fabrics.Test.Orchestrators.ExpectedProviders
{
    /// <summary>
    /// This provider defines how we'll get the data out of github to then use it to compare with another workspace
    /// </summary>
    class GithubExpectedProvider : IExpectedProvider
    {
        private readonly IConfiguration _config;
        public List<EntityInfo> EntitiyInfos { get; init; }

        public GithubExpectedProvider(IConfiguration config)
        {
            _config = config;
            EntitiyInfos = LoadEntityTypesFromZip(config);
        }

        private List<EntityInfo> LoadEntityTypesFromZip(IConfiguration config)
        {
            var sourcePath = config.GetVariable("fabric:github:source-path");
            var zipPath = config.GetVariable("fabric:github:zip-path");
            zipPath = GetFullPath(zipPath);
            
            var entityInfos = new List<EntityInfo>();
            
            if (!File.Exists(zipPath))
            {
                throw new FileNotFoundException($"Zip file not found at: {zipPath}");
            }
            
            using (var archive = ZipFile.OpenRead(zipPath))
            {
                // First pass: collect all file paths grouped by directory
                var filesByDirectory = new Dictionary<string, List<string>>();
                
                foreach (var entry in archive.Entries)
                {
                    if (!entry.FullName.EndsWith("/")) // Skip directories
                    {
                        var directory = Path.GetDirectoryName(entry.FullName) ?? "";
                        if (!filesByDirectory.ContainsKey(directory))
                        {
                            filesByDirectory[directory] = new List<string>();
                        }
                        filesByDirectory[directory].Add(entry.FullName);
                    }
                }
                
                // Second pass: process .platform files
                var platformEntries = archive.Entries
                     .Where(entry => entry.Name.EndsWith(".platform", StringComparison.OrdinalIgnoreCase))
                    .Where(entry => {
                        var path = entry.FullName.Replace('\\', '/').Replace("//", "/");
                        var sourcePathNormalized = sourcePath.Replace('\\', '/').Replace("//", "/").TrimStart('/');
                        return path.Contains("/" + sourcePathNormalized + "/", StringComparison.OrdinalIgnoreCase) ||
                               path.StartsWith(sourcePathNormalized + "/", StringComparison.OrdinalIgnoreCase);
                    })
                    .ToList();

                foreach (var entry in platformEntries)
                {
                    try
                    {
                        using (var stream = entry.Open())
                        using (var reader = new StreamReader(stream))
                        {
                            var jsonContent = reader.ReadToEnd();
                            
                            if (string.IsNullOrWhiteSpace(jsonContent))
                                continue;

                            var platformContent = JObject.Parse(jsonContent);
                            
                            if (platformContent != null)
                            {
                                var parentDirectory = Path.GetDirectoryName(entry.FullName) ?? "";
                                var allFilesInDir = filesByDirectory.GetValueOrDefault(parentDirectory, new List<string>());
                                
                                var entityInfo = new EntityInfo
                                {
                                    DisplayName = platformContent["metadata"].Value<string>("displayName") ?? "",
                                    Type = platformContent["metadata"].Value<string>("type") ?? "",
                                    FilePath = entry.FullName,
                                    ParentDirectory = parentDirectory,
                                    AllFilesInDirectory = allFilesInDir.ToList()
                                };
                                
                                entityInfos.Add(entityInfo);
                            }
                        }
                    }
                    catch (JsonException ex)
                    {
                        // Log the error but continue processing other files
                        Console.WriteLine($"Failed to parse JSON from {entry.FullName}: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        // Log any other errors
                        Console.WriteLine($"Error processing {entry.FullName}: {ex.Message}");
                    }
                }
            }
            
            return entityInfos;
        }


        public string GetFullPath(string path)
        {
            if (Path.IsPathRooted(path))
            {
                // Already a full path
                return path;
            }
            else
            {
                // Partial path - combine with application base directory
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                return Path.Combine(basePath, path);
            }
        }
    }
}
