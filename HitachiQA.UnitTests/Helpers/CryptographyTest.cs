using HitachiQA.Helpers;

namespace HitachiQA.UnitTests.Helpers
{
    [TestClass]
    public class CryptographyTest
    {
        [TestMethod]
        public void IsStringEncrypted()
        {
            string theString = "test12$#@";
            string encrypt = Cryptography.Encrypt(theString);
            string decrypt = Cryptography.Decrypt(encrypt);
            decrypt.Should().Be("test12$#@");
        }
    }
}
