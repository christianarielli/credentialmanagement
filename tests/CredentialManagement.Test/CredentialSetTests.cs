using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CredentialManagement.Test
{
    [TestClass]
    public class CredentialSetTests
    {
        [TestMethod]
        public void Constructor_RequiresNonEmptyTargetWhenSpecified()
        {
            using (CredentialSet credentials = new CredentialSet("target"))
            {
                Assert.AreEqual("target", credentials.Target);
            }

            Assert.ThrowsExactly<ArgumentNullException>(() => new CredentialSet(string.Empty));
        }

        [TestMethod]
        public void FilteredLoad_ReturnsOnlyMatchingCredentialWithoutDuplicates()
        {
            string target = "CredentialManagement.Tests.Set." + Guid.NewGuid().ToString("N");
            using (Credential saved = new Credential("filter-user", "filter-password", target))
            {
                try
                {
                    saved.SaveOrThrow();
                    using (CredentialSet credentials = new CredentialSet(target))
                    {
                        Assert.AreSame(credentials, credentials.LoadOrThrow());
                        Assert.AreEqual(1, credentials.Count);
                        Assert.AreEqual(target, credentials[0].Target);

                        credentials.LoadOrThrow();
                        Assert.AreEqual(1, credentials.Count);
                    }
                }
                finally
                {
                    if (saved.Exists())
                    {
                        saved.DeleteOrThrow();
                    }
                }
            }
        }

        [TestMethod]
        public void DisposedSet_RejectsFurtherLoads()
        {
            CredentialSet credentials = new CredentialSet();
            credentials.Dispose();
            Assert.ThrowsExactly<ObjectDisposedException>(() => credentials.Load());
        }
    }
}
