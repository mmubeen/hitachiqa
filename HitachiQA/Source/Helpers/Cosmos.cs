using Microsoft.Azure.Cosmos;

namespace HitachiQA.Helpers
{
    public class Cosmos : CosmosClient
    {
        public readonly string DatabaseName;
        public Cosmos(string uri, string apiKey, string databaseName) : base(uri, apiKey)
        {
            this.DatabaseName = databaseName;
        }

        public async Task<List<dynamic>> GetQuery(string containerA, string queryA)
        {
            var database = this.GetDatabase(DatabaseName);
            var container = database.GetContainer(containerA);
            List<dynamic> result = new List<dynamic>();
            using (FeedIterator<dynamic> feedIterator = container.GetItemQueryIterator<dynamic>(queryA))
            {
                while (feedIterator.HasMoreResults)
                {
                    FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
                    foreach (var item in response)
                    {
                        result.Add(item);
                    }
                }
            }
            return result;
        }

        public void setProperty(string containerA, string identifierQuery,  string key, dynamic value)
        {
            var database = this.GetDatabase(DatabaseName);
            var container = database.GetContainer(containerA);

            Log.Debug(containerA);

            List<dynamic> recordsToUpdate = GetQuery(containerA, identifierQuery).Result;

            foreach(var record in recordsToUpdate)
            {
               record[key] = value;
                _ = container.ReplaceItemAsync(record, (string)record.id).Result;
            }
        }
    }
}
