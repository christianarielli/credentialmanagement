using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CredentialManagement.Test
{
    [TestClass]
    public class CredentialTests
    {
        [TestMethod]
        public void Constructors_CreateCredential()
        {
            using (Credential empty = new Credential())
            using (Credential username = new Credential("username"))
            using (Credential password = new Credential("username", "password"))
            using (Credential target = new Credential("username", "password", "target"))
            {
                Assert.IsNotNull(empty);
                Assert.AreEqual("username", username.Username);
                Assert.AreEqual("password", password.Password);
                Assert.AreEqual("target", target.Target);
            }
        }

        [TestMethod]
        public void DisposedCredential_RejectsFurtherAccess()
        {
            Credential credential = new Credential { Password = "password" };
            credential.Dispose();
            Assert.ThrowsExactly<ObjectDisposedException>(() => credential.Username = "username");
        }

        [TestMethod]
        public void SaveLoadExistsDelete_RoundTripsUnicodeCredential()
        {
            string target = CreateTarget();
            using (Credential saved = new Credential("domain\\üser", "pässwörd🔐", target))
            {
                saved.PersistenceType = PersistenceType.Session;
                try
                {
                    saved.SaveOrThrow();
                    Assert.IsTrue(saved.Exists());

                    using (Credential loaded = new Credential { Target = target })
                    {
                        loaded.LoadOrThrow();
                        Assert.AreEqual(saved.Username, loaded.Username);
                        Assert.AreEqual(saved.Password, loaded.Password);
                        Assert.AreEqual(target, loaded.Target);
                        Assert.AreEqual(PersistenceType.Session, loaded.PersistenceType);
                    }
                }
                finally
                {
                    if (saved.Exists())
                    {
                        saved.DeleteOrThrow();
                    }
                }

                Assert.IsFalse(saved.Exists());
            }
        }

        private static string CreateTarget()
        {
            return "CredentialManagement.Tests." + Guid.NewGuid().ToString("N");
        }
    }
}
