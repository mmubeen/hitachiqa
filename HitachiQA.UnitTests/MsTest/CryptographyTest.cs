using HitachiQA.Helpers;

namespace HitachiQA.UnitTests
{
    [TestClass]
    public class CryptographyTest
    {
        [TestMethod]
        public void IsStringEncrypted()
        {
            // Arrange
            string theString = "test12";
            // Act
            string encrypt = Cryptography.Encrypt(theString);
            // Assert
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
