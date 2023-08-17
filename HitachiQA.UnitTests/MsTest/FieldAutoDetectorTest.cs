
namespace HitachiQA.UnitTests
{
    [TestClass]
    public class FieldAutoDetectorTest
    {
        [TestMethod]
        public void IsParsedToBoolTrue()
        {
            // Arrange
            string[] strings = { "1", "yes", "true", "check" };
            foreach (var thing in strings)
            {
                // Act
                var detector = FieldAutoDetector.parseStrIntoBool(thing);
                // Assert
                detector.Should().BeTrue();
            }
        }

        [TestMethod]
        public void IsParsedToBoolFalse()
        {
            string[] strings = { "0", "no", "false", "uncheck" };
            foreach (var thing in strings)
            {
                var detector = FieldAutoDetector.parseStrIntoBool(thing);
                detector.Should().BeFalse();
            }
        }
    }
}
