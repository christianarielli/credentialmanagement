using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace CredentialManagement
{
    public class Credential : IDisposable
    {
        private const int MaxCredentialBlobSize = 5 * 512;

        private bool _disposed;
        private CredentialType _type;
        private string _target;
        private SecureString _password;
        private string _username;
        private string _description;
        private DateTime _lastWriteTime;
        private PersistanceType _persistanceType;

        public Credential()
            : this(null)
        {
        }

        public Credential(string username)
            : this(username, null)
        {
        }

        public Credential(string username, string password)
            : this(username, password, null)
        {
        }

        public Credential(string username, string password, string target)
            : this(username, password, target, CredentialType.Generic)
        {
        }

        public Credential(string username, string password, string target, CredentialType type)
        {
            Username = username;
            Password = password;
            Target = target;
            Type = type;
            PersistanceType = PersistanceType.Session;
            _lastWriteTime = DateTime.MinValue;
        }

        public string Username
        {
            get
            {
                CheckNotDisposed();
                return _username;
            }
            set
            {
                CheckNotDisposed();
                _username = value;
            }
        }

        public string Password
        {
            get
            {
                using (SecureString password = SecurePassword)
                {
                    return SecureStringHelper.CreateString(password);
                }
            }
            set
            {
                CheckNotDisposed();
                ReplacePassword(SecureStringHelper.CreateSecureString(value ?? string.Empty));
            }
        }

        public SecureString SecurePassword
        {
            get
            {
                CheckNotDisposed();
                return _password == null ? new SecureString() : _password.Copy();
            }
            set
            {
                CheckNotDisposed();
                ReplacePassword(value == null ? new SecureString() : value.Copy());
            }
        }

        public string Target
        {
            get
            {
                CheckNotDisposed();
                return _target;
            }
            set
            {
                CheckNotDisposed();
                _target = value;
            }
        }

        public string Description
        {
            get
            {
                CheckNotDisposed();
                return _description;
            }
            set
            {
                CheckNotDisposed();
                _description = value;
            }
        }

        public DateTime LastWriteTime => LastWriteTimeUtc.ToLocalTime();

        public DateTime LastWriteTimeUtc
        {
            get
            {
                CheckNotDisposed();
                return _lastWriteTime;
            }
            private set => _lastWriteTime = value;
        }

        public CredentialType Type
        {
            get
            {
                CheckNotDisposed();
                return _type;
            }
            set
            {
                CheckNotDisposed();
                _type = value;
            }
        }

        public PersistanceType PersistanceType
        {
            get
            {
                CheckNotDisposed();
                return _persistanceType;
            }
            set
            {
                CheckNotDisposed();
                _persistanceType = value;
            }
        }

        public bool Save()
        {
            CheckNotDisposed();

            string password = Password;
            byte[] passwordBytes = Encoding.Unicode.GetBytes(password);
            if (passwordBytes.Length > MaxCredentialBlobSize)
            {
                Array.Clear(passwordBytes, 0, passwordBytes.Length);
                throw new ArgumentOutOfRangeException(nameof(Password), "The credential blob must not exceed 2560 bytes.");
            }

            IntPtr passwordPointer = IntPtr.Zero;
            try
            {
                if (passwordBytes.Length > 0)
                {
                    passwordPointer = Marshal.StringToCoTaskMemUni(password);
                }

                NativeMethods.CREDENTIAL credential = new NativeMethods.CREDENTIAL
                {
                    TargetName = Target,
                    UserName = Username,
                    CredentialBlob = passwordPointer,
                    CredentialBlobSize = passwordBytes.Length,
                    Comment = Description,
                    Type = (int)Type,
                    Persist = (int)PersistanceType
                };

                bool result = NativeMethods.CredWrite(ref credential, 0);
                if (result)
                {
                    LastWriteTimeUtc = DateTime.UtcNow;
                }

                return result;
            }
            finally
            {
                Array.Clear(passwordBytes, 0, passwordBytes.Length);
                if (passwordPointer != IntPtr.Zero)
                {
                    Marshal.ZeroFreeCoTaskMemUnicode(passwordPointer);
                }
            }
        }

        public bool Delete()
        {
            CheckNotDisposed();

            if (string.IsNullOrEmpty(Target))
            {
                throw new InvalidOperationException("Target must be specified to delete a credential.");
            }

            return NativeMethods.CredDelete(Target, Type, 0);
        }

        public bool Load()
        {
            CheckNotDisposed();

            if (string.IsNullOrEmpty(Target))
            {
                throw new InvalidOperationException("Target must be specified to load a credential.");
            }

            IntPtr credentialPointer;
            if (!NativeMethods.CredRead(Target, Type, 0, out credentialPointer))
            {
                return false;
            }

            using (NativeMethods.CriticalCredentialHandle credentialHandle =
                   new NativeMethods.CriticalCredentialHandle(credentialPointer))
            {
                LoadInternal(credentialHandle.GetCredential());
            }

            return true;
        }

        public bool Exists()
        {
            CheckNotDisposed();

            if (string.IsNullOrEmpty(Target))
            {
                throw new InvalidOperationException("Target must be specified to check existence of a credential.");
            }

            using (Credential existing = new Credential { Target = Target, Type = Type })
            {
                return existing.Load();
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            if (_password != null)
            {
                _password.Dispose();
                _password = null;
            }

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        internal void LoadInternal(NativeMethods.CREDENTIAL credential)
        {
            Username = credential.UserName;
            Password = credential.CredentialBlobSize > 0
                ? Marshal.PtrToStringUni(credential.CredentialBlob, credential.CredentialBlobSize / sizeof(char))
                : string.Empty;
            Target = credential.TargetName;
            Type = (CredentialType)credential.Type;
            PersistanceType = (PersistanceType)credential.Persist;
            Description = credential.Comment;
            LastWriteTimeUtc = DateTime.FromFileTimeUtc(credential.LastWritten);
        }

        private void ReplacePassword(SecureString password)
        {
            if (_password != null)
            {
                _password.Dispose();
            }

            _password = password;
        }

        private void CheckNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(Credential));
            }
        }
    }
}
