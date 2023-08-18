
using Microsoft.VisualBasic;

namespace HitachiQA.UnitTests
{
    [TestClass]
    public class FieldAutoDetectorTest
    {
        [TestMethod]
        public void IsParsedToBool()
        {
            string[] positive = { "1", "yes", "true", "check" };
            string[] negative = { "0", "no", "false", "uncheck" };
            foreach (var thing in positive)
            {
                var detector = FieldAutoDetector.parseStrIntoBool(thing);
                detector.Should().BeTrue();
            }

            foreach (var thing in negative)
            {
                var detector = FieldAutoDetector.parseStrIntoBool(thing);
                detector.Should().BeFalse();
            }
        }
    }
}
