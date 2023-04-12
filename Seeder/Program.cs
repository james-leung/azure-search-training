using Bogus;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using AzureSearchTraining.Deploy;

namespace AzureSearchTraining.Seeder
{
    public sealed class Program
    {
        public static async Task Main(string[] args)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            IConfigurationRoot configuration = builder.Build();

            var connectionString = configuration["CosmosDbConnectionString"];

            CosmosClient cosmosClient = new CosmosClient(
                connectionString,
                new CosmosClientOptions()
                {
                    ApplicationRegion = Regions.UKSouth,
                });

            var database = cosmosClient.GetDatabase("myDatabase");
            var container = database.GetContainer("myContainer");

            var carFaker = new Faker<Car>()
                .RuleFor(c => c.Id, f => f.Random.Guid().ToString())
                .RuleFor(c => c.Name, f => f.Vehicle.Manufacturer() + " " + f.Vehicle.Model() + " " + f.Vehicle.Type() + " " + f.Date.Past(15, DateTime.Now).Year)
                .RuleFor(c => c.Description, f => f.Lorem.Sentence(2, 30))
                .RuleFor(c => c.Price, f => f.Random.Number(100, 10000))
                .RuleFor(c => c.Color, f => f.Commerce.Color());

            // Seed cars into Cosmos DB
            var cars = carFaker.Generate(100);
            foreach (var car in cars)
            {
                await container.CreateItemAsync(car);
            }
        }
    }
}