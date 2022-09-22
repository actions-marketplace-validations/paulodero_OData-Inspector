using Microsoft.OData.Edm;
using OData.Schema.Validation.Models;
using System.IO.Compression;

namespace OData.Schema.Validation.Utils
{
    public class GitUtilities
    {

        public Logger Logger;

        public GitUtilities(Logger logger)
        {
            Logger = logger;
        }

        public async Task<Dictionary<string, ModelContainer>> GetSchemasFromBranch(string repoName, string branchName)
        {
            var branchDownloadUrl = $"https://github.com/{repoName}/archive/refs/heads/{branchName}.zip";
            var client = new HttpClient();
            var file = await client.GetStreamAsync(branchDownloadUrl);
           
            return ExtractSchemasFromZip(file);
        }

        public Dictionary<string, ModelContainer> ExtractSchemasFromZip(Stream memStream)
        {
            var schemaFiles = new Dictionary<string, ModelContainer>();

            using (var readArchive = new ZipArchive(memStream, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in readArchive.Entries)
                {
                    // Need to account for tags.txt
                    if (entry.Name.EndsWith(".csdl"))
                    {
                        using Stream entryStream = entry.Open();
                        EdmModelParser edmModelParser = new EdmModelParser(Logger);
                        IEdmModel model = edmModelParser.ParseEdmModel(entryStream);
                        var csdl = StreamToString(entry.Open());
                        schemaFiles.Add(Path.GetFileNameWithoutExtension(entry.Name), new ModelContainer(model, csdl));
                    }
                }
            }

            return schemaFiles;
        }

        public static string StreamToString(Stream stream)
        {
            using (StreamReader streamReader = new StreamReader(stream))
            {
               return streamReader.ReadToEnd();
            }
        }
        private static HttpClient GetGithubHttpClient()
        {
            return new HttpClient
            {
                BaseAddress = new Uri("https://api.github.com"),
                DefaultRequestHeaders =
            {
                // NOTE: You'll have to set up Authentication tokens in real use scenario
                // NOTE: as without it you're subject to harsh rate limits.
                {"User-Agent", "Github-API-Test"}
            }
            };
        }
    }
}
