// This is a prototype tool that allows for import of sample data to an Azure Search index

using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net.Http;

namespace AzureSearchBackupRestore
{
    class Program
    {
        private static readonly string TargetSearchServiceName = ConfigurationManager.AppSettings["TargetSearchServiceName"];
        private static readonly string TargetSearchServiceApiKey = ConfigurationManager.AppSettings["TargetSearchServiceApiKey"];
        private static HttpClient HttpClient;
        private static Uri ServiceUri;

        static void Main(string[] args)
        {
            try
            {
                ServiceUri = new Uri($"https://{TargetSearchServiceName}.search.windows.net", UriKind.Absolute);
                HttpClient = new HttpClient();
                HttpClient.DefaultRequestHeaders.Add("api-key", TargetSearchServiceApiKey);

                LaunchImportProcess("zipcodes");
                LaunchImportProcess("nycjobs");

                Console.WriteLine("NOTE: For really large indexes it may take some time to index all content.\r\n");
                Console.WriteLine("Press any key to continue.\r\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                Console.WriteLine("Did you remember to set your TargetSearchServiceName and TargetSearchServiceApiKey in the app.config?\r\n");
            }

            Console.ReadLine();
        }

        private static void LaunchImportProcess(string indexName)
        {
            Console.WriteLine("Deleting " + indexName + " index...");
            DeleteIndex(indexName);
            Console.WriteLine("Creating " + indexName + " index...");
            CreateTargetIndex(indexName);
            Console.WriteLine("Uploading data to " + indexName + "...");
            ImportFromJson(indexName);
        }

        private static void DeleteIndex(string indexName)
        {
            try
            {
                try
                {
                    Uri uri = new Uri(ServiceUri, "/indexes/" + indexName);
                    HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(HttpClient, HttpMethod.Delete, uri);
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: {0}", ex.Message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting index: {0}\r\n", ex.Message);
            }
        }

        private static void CreateTargetIndex(string indexName)
        {
            string json = File.ReadAllText(Path.Combine(GetSchemaDataDirectory(), indexName + ".schema"));
            try
            {
                Uri uri = new Uri(ServiceUri, "/indexes");
                HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(HttpClient, HttpMethod.Post, uri, json);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }
        }

        private static void ImportFromJson(string indexName)
        {
            try
            {
                foreach (string fileName in Directory.GetFiles(GetSchemaDataDirectory(), indexName + "*.json"))
                {
                    Console.WriteLine("Uploading documents from file {0}", fileName);
                    string json = File.ReadAllText(fileName);
                    Uri uri = new Uri(ServiceUri, "/indexes/" + indexName + "/docs/index");
                    HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(HttpClient, HttpMethod.Post, uri, json);
                    response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }
        }

        private static string GetSchemaDataDirectory()
        {
            DirectoryInfo currentDirectory = new DirectoryInfo(AppContext.BaseDirectory);

            while (currentDirectory != null)
            {
                string candidate = Path.Combine(currentDirectory.FullName, "NYCJobsWeb", "Schema_and_Data");
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                currentDirectory = currentDirectory.Parent;
            }

            throw new DirectoryNotFoundException("Unable to locate the NYCJobsWeb/Schema_and_Data directory.");
        }
    }
}
