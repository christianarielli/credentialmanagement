using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CredentialManagement.Test
{
    [TestClass]
    public class WindowsCredentialsPromptTests
    {
        [TestMethod]
        public void Defaults_AreSuitableForModernWindowsPrompt()
        {
            using (WindowsCredentialsPrompt prompt = new WindowsCredentialsPrompt())
            {
                Assert.AreEqual("Please provide credentials", prompt.Title);
                Assert.AreEqual(string.Empty, prompt.Username);
                Assert.IsFalse(prompt.GenericCredentials);
                Assert.IsFalse(prompt.ShowSaveCheckBox);
            }
        }

        [TestMethod]
        public void Validation_RejectsInvalidValues()
        {
            using (WindowsCredentialsPrompt prompt = new WindowsCredentialsPrompt())
            {
                Assert.ThrowsExactly<ArgumentNullException>(() => prompt.Username = null);
                Assert.ThrowsExactly<ArgumentNullException>(() => prompt.Password = null);
                Assert.ThrowsExactly<ArgumentNullException>(() => prompt.Message = null);
                Assert.ThrowsExactly<ArgumentNullException>(() => prompt.Title = null);
                Assert.ThrowsExactly<ArgumentOutOfRangeException>(
                    () => prompt.Password = new string('x', NativeMethods.CREDUI_MAX_PASSWORD_LENGTH + 1));
            }
        }

        [TestMethod]
        public void DisposedPrompt_RejectsFurtherAccess()
        {
            WindowsCredentialsPrompt prompt = new WindowsCredentialsPrompt();
            prompt.Password = "temporary";
            prompt.Dispose();
            Assert.ThrowsExactly<ObjectDisposedException>(() => prompt.Username = "user");
        }
    }
}
