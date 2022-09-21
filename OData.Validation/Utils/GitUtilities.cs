using Microsoft.OData.Edm;
using System.IO.Compression;

namespace OData.Schema.Validation.Utils
{
    public class GitUtilities
    {

        public static async Task<Dictionary<string, IEdmModel>> GetSchemasFromBranch(string userName, string branchName)
        {
            var branchDownloadUrl = $"https://github.com/{userName}/OData-Sample-Schema/archive/refs/heads/{branchName}.zip";
            HttpClient client = new();
            var file = await client.GetStreamAsync(branchDownloadUrl);
            return ExtractSchemasFromZip(file);
        }

        public static Dictionary<string, IEdmModel> ExtractSchemasFromZip(Stream memStream)
        {
            var schemaFiles = new Dictionary<string, IEdmModel>();

            using (var readArchive = new ZipArchive(memStream, ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in readArchive.Entries)
                {
                    // Need to account for tags.txt
                    if (entry.Name.EndsWith(".csdl"))
                    {
                        using Stream entryStream = entry.Open();
                        IEdmModel model = EdmModelParser.ParseEdmModel(entryStream);
                        schemaFiles.Add(Path.GetFileNameWithoutExtension(entry.Name), model);
                    }
                }
            }

            return schemaFiles;
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
