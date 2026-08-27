using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CredentialManagement
{
    public class CredentialSet : List<Credential>, IDisposable
    {
        private bool _disposed;

        public CredentialSet()
        {
        }

        public CredentialSet(string target)
        {
            if (string.IsNullOrEmpty(target))
            {
                throw new ArgumentNullException(nameof(target));
            }

            Target = target;
        }

        public string Target { get; set; }

        public CredentialSet Load()
        {
            CheckNotDisposed();
            LoadInternal();
            return this;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            DisposeCredentials();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private void LoadInternal()
        {
            DisposeCredentials();

            uint count;
            IntPtr credentialsPointer;
            bool result = NativeMethods.CredEnumerateW(Target, 0, out count, out credentialsPointer);
            if (!result)
            {
                Trace.WriteLine(new Win32Exception(Marshal.GetLastWin32Error()));
                return;
            }

            using (NativeMethods.CriticalCredentialHandle credentialsHandle =
                   new NativeMethods.CriticalCredentialHandle(credentialsPointer))
            {
                IntPtr basePointer = credentialsHandle.GetRawHandle();
                for (int index = 0; index < count; index++)
                {
                    IntPtr credentialPointer = Marshal.ReadIntPtr(basePointer, index * IntPtr.Size);
                    NativeMethods.CREDENTIAL nativeCredential =
                        Marshal.PtrToStructure<NativeMethods.CREDENTIAL>(credentialPointer);

                    Credential credential = new Credential();
                    credential.LoadInternal(nativeCredential);
                    Add(credential);
                }
            }
        }

        private void DisposeCredentials()
        {
            foreach (Credential credential in this)
            {
                credential.Dispose();
            }

            Clear();
        }

        private void CheckNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(CredentialSet));
            }
        }
    }
}
