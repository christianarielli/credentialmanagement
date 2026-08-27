using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CredentialManagement.Test
{
    [TestClass]
    public class ApiCompatibilityTests
    {
        [TestMethod]
        public void WindowsCredentialsPrompt_ImplementsPublicInterface()
        {
            using (ICredentialsPrompt prompt = new WindowsCredentialsPrompt())
            {
                Assert.IsNotNull(prompt);
            }
        }
    }
}
