using HitachiQA.Helpers;
using Telerik.JustMock;

namespace HitachiQA.UnitTests.Helpers
{
    [TestClass]
    public class SQLTests
    {
        SQL SQL { get; init; }

        public SQLTests()
        {
            SQL = Mock.Create<SQL>();
        }
        [TestMethod]
        public void BuildExecuteQueryCommandTest_SouldContainEachParameterInTheCommand()
        {
            // Arrange
            var query = "SELECT * FROM Table WHERE Column1 = @key1 AND Column2 = @key2 AND Column3 = @key3";
            (string key, dynamic value)[] parameters = new (string, dynamic)[]
            {
                ("@key1", 123),
                ("@key2", "TestValue"),
                ("key3", "TestValue2"),
            };

            //act
            var command = SQL.BuildExecuteQueryCommand(ref query, ref parameters);

            //assert
            command.CommandText.Should().Be(query);
            command.Parameters.Count.Should().Be(parameters.Length);
            foreach (var param in parameters)
            {
                var paramName = param.Item1.StartsWith("@") ? param.Item1 : "@" + param.Item1;

                command.Parameters[paramName].Value.Should().Be(param.Item2);
            }
        }

        [TestMethod]
        public void ProcessListParameterValues_ShouldGenerateKeyPerValue()
        {
            // Arrange
            var query = "SELECT * FROM Table WHERE Column IN @key";
            (string key, dynamic value)[] parameters = new (string, dynamic)[]
            {
                ("@key", new List<int> { 1, 2, 3 })
            };

            //act
            SQL.ProcessListParameterValues(ref query, ref parameters);

            //assert
            query.Should().Be("SELECT * FROM Table WHERE Column IN @key0, @key1, @key2");
            parameters.Should().NotContain(("@key", new List<int> { 1, 2, 3 }));
            parameters.Should().Contain(("@key0", 1));
            parameters.Should().Contain(("@key1", 2));
            parameters.Should().Contain(("@key2", 3));
        }
    }
}
