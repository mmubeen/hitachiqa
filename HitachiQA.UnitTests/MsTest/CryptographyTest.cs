using HitachiQA.Helpers;

namespace HitachiQA.UnitTests
{
    [TestClass]
    public class CryptographyTest
    {
        [TestMethod]
        public void IsStringEncrypted()
        {
            string theString = "test12";
            string encrypt = Cryptography.Encrypt(theString);
            encrypt.Should().Be("IvkLxQ7k+mp5OVoKenFqJw==");
        }

        [TestMethod]
        public void IsStringDecrypted()
        {
            string encrypt = "IvkLxQ7k+mp5OVoKenFqJw==";
            string decrypted = Cryptography.Decrypt(encrypt);
            decrypted.Should().Be("test12");
        } 
    }
}
