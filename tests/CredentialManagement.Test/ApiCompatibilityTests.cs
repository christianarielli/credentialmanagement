using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CredentialManagement.Test
{
    [TestClass]
    public class ApiCompatibilityTests
    {
        [TestMethod]
        public void PersistenceType_UsesWindowsCredentialValues()
        {
            Assert.AreEqual(1U, (uint)PersistenceType.Session);
            Assert.AreEqual(2U, (uint)PersistenceType.LocalComputer);
            Assert.AreEqual(3U, (uint)PersistenceType.Enterprise);
        }

        [TestMethod]
        public void WindowsCredentialsPrompt_ImplementsPublicInterface()
        {
            using (ICredentialsPrompt prompt = new WindowsCredentialsPrompt())
            {
                Assert.IsNotNull(prompt);
            }
        }

        [TestMethod]
        public void CredentialType_IncludesCurrentWindowsTypes()
        {
            Assert.AreEqual(5U, (uint)CredentialType.GenericCertificate);
            Assert.AreEqual(6U, (uint)CredentialType.DomainExtended);
        }
    }
}
