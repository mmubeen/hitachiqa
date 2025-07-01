using HitachiQA;
using HitachiQA.Helpers;

namespace Fabrics.Test.StepDefinitions
{
    [Binding]
    public class FabricDataValidationStepDefinitions
    {
        private readonly FabricSQL _fabricSql;

        public FabricDataValidationStepDefinitions(FabricSQL fabric)
        {
            _fabricSql = fabric;
        }

        [Given("We connect to the Fabric Lakehouse and query the records")]
        public void GivenWeConnectToTheFabricLakehouseAndQueryTheRecords()
        {
            Log.Info("Connecting to Microsoft Fabric and querying records...");

            string query = "SELECT TOP 10 * FROM Lakehouse_Gold.dw.new_client_dimension";
            var results = _fabricSql.ExecuteQueryAsync(query).Result;

            if (results == null || !results.Any())
                throw new Exception("No records returned from Fabric Lakehouse query.");

            Log.Info($"Query returned {results.Count} records.");
        }
    }
}
