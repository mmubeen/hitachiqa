
namespace HitachiQA.UnitTests
{
    [TestClass]
    public class FieldAutoDetectorTest
    {
        [TestMethod]
        public void IsParsedToBool()
        {
            var detector = FieldAutoDetector.parseStrIntoBool("true");
            detector.Should().BeTrue();

        }
    }
}
