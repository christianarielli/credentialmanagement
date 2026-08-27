using System;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CredentialManagement.Test
{
    [TestClass]
    public class CredentialErrorTests
    {
        [TestMethod]
        public void LoadOrThrow_PreservesNativeErrorCode()
        {
            string target = "CredentialManagement.Tests.Missing." + Guid.NewGuid().ToString("N");
            using (Credential credential = new Credential { Target = target })
            {
                Win32Exception exception =
                    Assert.ThrowsExactly<Win32Exception>(() => credential.LoadOrThrow());

                Assert.AreEqual(1168, exception.NativeErrorCode);
            }
        }

        [TestMethod]
        public void DeleteOrThrow_PreservesNativeErrorCode()
        {
            string target = "CredentialManagement.Tests.Missing." + Guid.NewGuid().ToString("N");
            using (Credential credential = new Credential { Target = target })
            {
                Win32Exception exception =
                    Assert.ThrowsExactly<Win32Exception>(() => credential.DeleteOrThrow());

                Assert.AreEqual(1168, exception.NativeErrorCode);
            }
        }

        [TestMethod]
        public void Save_ValidatesCredentialBlobSizeInBytes()
        {
            string oversizedPassword = new string('x', 1281);
            using (Credential credential = new Credential("user", oversizedPassword, "unused"))
            {
                Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => credential.Save());
            }
        }

        [TestMethod]
        public void CredentialSet_LoadOrThrow_TreatsNoMatchesAsEmptyResult()
        {
            string target = "CredentialManagement.Tests.Missing." + Guid.NewGuid().ToString("N");
            using (CredentialSet credentials = new CredentialSet(target))
            {
                Assert.AreSame(credentials, credentials.LoadOrThrow());
                Assert.AreEqual(0, credentials.Count);
            }
        }
    }
}
