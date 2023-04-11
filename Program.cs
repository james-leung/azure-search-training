using System;
using System.Net;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest.Azure;

namespace AzureSearchTraining
{
    public sealed class Program
    {
        public static async Task Main(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();

            SearchIndexClient indexClient = new SearchIndexClient(new Uri(configuration["SearchServiceEndPoint"]), new AzureKeyCredential(configuration["SearchServiceAdminApiKey"]));
            SearchIndexerClient indexerClient = new SearchIndexerClient(new Uri(configuration["SearchServiceEndPoint"]), new AzureKeyCredential(configuration["SearchServiceAdminApiKey"]));

            Console.WriteLine("Creating index...");
            FieldBuilder fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(Car));
            var searchIndex = new SearchIndex("cars-search-idx", searchFields);

            CleanupSearchIndexClientResources(indexClient, searchIndex);

            indexClient.CreateOrUpdateIndex(searchIndex);

            Console.WriteLine("Creating data source...");

            var dataSource =
                new SearchIndexerDataSourceConnection(
                    "cars-ds",
                    SearchIndexerDataSourceType.CosmosDb,
                    configuration["CosmosDbConnectionString"],
                    new SearchIndexerDataContainer("myContainer"));

            indexerClient.CreateOrUpdateDataSourceConnection(dataSource);

            Console.WriteLine("Creating Azure indexer...");

            var schedule = new IndexingSchedule(TimeSpan.FromDays(1))
            {
                StartTime = DateTimeOffset.Now
            };

            var parameters = new IndexingParameters()
            {
                BatchSize = 100,
                MaxFailedItems = 0,
                MaxFailedItemsPerBatch = 0
            };

            var indexer = new SearchIndexer("cars-idxr", dataSource.Name, searchIndex.Name)
            {
                Description = "Data indexer",
                Schedule = schedule,
                Parameters = parameters,
            };

            CleanupSearchIndexerClientResources(indexerClient, indexer);

            await indexerClient.CreateOrUpdateIndexerAsync(indexer);

            Console.WriteLine("Running Azure indexer...");

            try
            {
                await indexerClient.RunIndexerAsync(indexer.Name);
            }
            catch (CloudException e) when (e.Response.StatusCode == (HttpStatusCode)429)
            {
                Console.WriteLine("Failed to run indexer: {0}", e.Response.Content);
            }

            Console.WriteLine("Waiting for indexing...\n");
            System.Threading.Thread.Sleep(5000);

            await CheckIndexerStatus(indexerClient, indexer);
        }

        private async static Task CheckIndexerStatus(SearchIndexerClient indexerClient, SearchIndexer indexer)
        {
            string indexerName = "cars-idxr";
            SearchIndexerStatus execInfo = await indexerClient.GetIndexerStatusAsync(indexerName);

            Console.WriteLine("Indexer has run {0} times.", execInfo.ExecutionHistory.Count);
            Console.WriteLine("Indexer Status: " + execInfo.Status.ToString());

            IndexerExecutionResult result = execInfo.LastResult;

            Console.WriteLine("Latest run");
            Console.WriteLine("Run Status: {0}", result.Status.ToString());
            Console.WriteLine("Total Documents: {0}, Failed: {1}", result.ItemCount, result.FailedItemCount);

            TimeSpan elapsed = result.EndTime.Value - result.StartTime.Value;
            Console.WriteLine("StartTime: {0:T}, EndTime: {1:T}, Elapsed: {2:t}", result.StartTime.Value, result.EndTime.Value, elapsed);

            string errorMsg = result.ErrorMessage ?? "none";
            Console.WriteLine("ErrorMessage: {0}", errorMsg);
            Console.WriteLine(" Document Errors: {0}, Warnings: {1}\n", result.Errors.Count, result.Warnings.Count);
        }

        private static void CleanupSearchIndexClientResources(SearchIndexClient indexClient, SearchIndex index)
        {
            try
            {
                if (indexClient.GetIndex(index.Name) != null)
                {
                    indexClient.DeleteIndex(index.Name);
                }
            }
            catch (RequestFailedException e) when (e.Status == 404)
            {
                //if exception occurred and status is "Not Found", this is working as expected
                Console.WriteLine("Failed to find index and this is because it doesn't exist.");
            }
        }

        private static void CleanupSearchIndexerClientResources(SearchIndexerClient indexerClient, SearchIndexer indexer)
        {
            try
            {
                if (indexerClient.GetIndexer(indexer.Name) != null)
                {
                    indexerClient.ResetIndexer(indexer.Name);
                }
            }
            catch (RequestFailedException e) when (e.Status == 404)
            {
                //if exception occurred and status is "Not Found", this is working as expected
                Console.WriteLine("Failed to find indexer and this is because it doesn't exist.");
            }
        }
    }
}
