using System;
using System.ComponentModel;
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
        private string? _target;
        private SecureString? _password;
        private string? _username;
        private string? _description;
        private DateTime _lastWriteTime;
        private PersistenceType _persistenceType;

        public Credential()
            : this(null)
        {
        }

        public Credential(string? username)
            : this(username, null)
        {
        }

        public Credential(string? username, string? password)
            : this(username, password, null)
        {
        }

        public Credential(string? username, string? password, string? target)
            : this(username, password, target, CredentialType.Generic)
        {
        }

        public Credential(string? username, string? password, string? target, CredentialType type)
        {
            Username = username;
            Password = password;
            Target = target;
            Type = type;
            PersistenceType = PersistenceType.Session;
            _lastWriteTime = DateTime.MinValue;
        }

        public string? Username
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

        public string? Password
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

        public string? Target
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

        public string? Description
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

        public PersistenceType PersistenceType
        {
            get
            {
                CheckNotDisposed();
                return _persistenceType;
            }
            set
            {
                CheckNotDisposed();
                _persistenceType = value;
            }
        }

        [Obsolete("Use PersistenceType instead.")]
        public PersistanceType PersistanceType
        {
            get => (PersistanceType)PersistenceType;
            set => PersistenceType = (PersistenceType)value;
        }

        public bool Save()
        {
            int ignoredError;
            return SaveCore(out ignoredError);
        }

        public void SaveOrThrow()
        {
            int error;
            if (!SaveCore(out error))
            {
                throw new Win32Exception(error);
            }
        }

        public bool Delete()
        {
            int ignoredError;
            return DeleteCore(out ignoredError);
        }

        public void DeleteOrThrow()
        {
            int error;
            if (!DeleteCore(out error))
            {
                throw new Win32Exception(error);
            }
        }

        public bool Load()
        {
            int ignoredError;
            return LoadCore(out ignoredError);
        }

        public void LoadOrThrow()
        {
            int error;
            if (!LoadCore(out error))
            {
                throw new Win32Exception(error);
            }
        }

        private bool SaveCore(out int error)
        {
            CheckNotDisposed();

            string password = Password ?? string.Empty;
            byte[] passwordBytes = Encoding.Unicode.GetBytes(password);
            if (passwordBytes.Length > MaxCredentialBlobSize)
            {
                Array.Clear(passwordBytes, 0, passwordBytes.Length);
                throw new InvalidOperationException("The credential blob must not exceed 2560 bytes.");
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
                    Persist = (int)PersistenceType
                };

                bool result = NativeMethods.CredWrite(ref credential, 0);
                error = result ? 0 : Marshal.GetLastWin32Error();
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

        private bool DeleteCore(out int error)
        {
            CheckNotDisposed();

            string? target = Target;
            if (target == null || target.Length == 0)
            {
                throw new InvalidOperationException("Target must be specified to delete a credential.");
            }

            bool result = NativeMethods.CredDelete(target, Type, 0);
            error = result ? 0 : Marshal.GetLastWin32Error();
            return result;
        }

        private bool LoadCore(out int error)
        {
            CheckNotDisposed();

            string? target = Target;
            if (target == null || target.Length == 0)
            {
                throw new InvalidOperationException("Target must be specified to load a credential.");
            }

            IntPtr credentialPointer;
            if (!NativeMethods.CredRead(target, Type, 0, out credentialPointer))
            {
                error = Marshal.GetLastWin32Error();
                return false;
            }

            error = 0;

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
                ? Marshal.PtrToStringUni(credential.CredentialBlob, credential.CredentialBlobSize / sizeof(char)) ?? string.Empty
                : string.Empty;
            Target = credential.TargetName;
            Type = (CredentialType)credential.Type;
            PersistenceType = (PersistenceType)credential.Persist;
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
