// This is a prototype tool that allows for import of sample data to an Azure Search index

using System;
using System.Configuration;
using System.IO;
using System.Net.Http;

namespace AzureSearchBackupRestore
{
    class Program
    {
        private static string TargetSearchServiceName = ConfigurationManager.AppSettings["TargetSearchServiceName"];
        private static string TargetSearchServiceApiKey = ConfigurationManager.AppSettings["TargetSearchServiceApiKey"];
        private static HttpClient HttpClient;
        private static Uri ServiceUri;
        private static string SchemaDataPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "NYCJobsWeb", "Schema_and_Data"));

        static void Main(string[] args)
        {
            try
            {
                ServiceUri = new Uri("https://" + TargetSearchServiceName + ".search.windows.net");
                HttpClient = new HttpClient();
                HttpClient.DefaultRequestHeaders.Add("api-key", TargetSearchServiceApiKey);

                // Re-create and import content to target indexes
                LaunchImportProcess("zipcodes");
                LaunchImportProcess("nycjobs");

                Console.WriteLine("NOTE: For really large indexes it may take some time to index all content.\r\n");
                Console.WriteLine("Press any key to continue.\r\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                Console.WriteLine("Did you remember to set your TArgetSearchServiceName and TargetSearchServiceApiKey in the app.config?\r\n");
            }
            Console.ReadLine();
        }

        private static void LaunchImportProcess(string IndexName)
        {
            // Re-create and import content to target index
            Console.WriteLine("Deleting " + IndexName + " index...");
            DeleteIndex(IndexName);
            Console.WriteLine("Creating " + IndexName + " index...");
            CreateTargetIndex(IndexName);
            Console.WriteLine("Uploading data to " + IndexName + "...");
            ImportFromJSON(IndexName);
        }

        private static void DeleteIndex(string IndexName)
        {
            // Delete the index if it exists
            try
            {
                try
                {
                    Uri uri = new Uri(ServiceUri, "/indexes/" + IndexName);
                    HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(HttpClient, HttpMethod.Delete, uri);
                    response.EnsureSuccessStatusCode();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: {0}", ex.Message.ToString());
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting index: {0}\r\n", ex.Message);
            }
        }

        static void CreateTargetIndex(string IndexName)
        {
            // Use the schema file to create a copy of this index
            // I like using REST here since I can just take the response as-is
            if (!Directory.Exists(SchemaDataPath))
            {
                throw new DirectoryNotFoundException("Schema and data folder not found at: " + SchemaDataPath);
            }
            string schemaPath = Path.Combine(SchemaDataPath, IndexName + ".schema");
            string json = File.ReadAllText(schemaPath);
            try
            {
                Uri uri = new Uri(ServiceUri, "/indexes");
                HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(HttpClient, HttpMethod.Post, uri, json);
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message.ToString());
            }

        }

        static void ImportFromJSON(string IndexName)
        {
            // Take JSON file and import this as-is to target index
            try
            {
                if (!Directory.Exists(SchemaDataPath))
                {
                    throw new DirectoryNotFoundException("Schema and data folder not found at: " + SchemaDataPath);
                }
                foreach (string fileName in Directory.GetFiles(SchemaDataPath, IndexName + "*.json"))
                {
                    Console.WriteLine("Uploading documents from file {0}", fileName);
                    string json = File.ReadAllText(fileName);
                    Uri uri = new Uri(ServiceUri, "/indexes/"+ IndexName + "/docs/index");
                    HttpResponseMessage response = AzureSearchHelper.SendSearchRequest(HttpClient, HttpMethod.Post, uri, json);
                    response.EnsureSuccessStatusCode();
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message.ToString());
            }
        }
    }
}
