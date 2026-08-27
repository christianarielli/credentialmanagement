using System;
using System.Runtime.InteropServices;
using System.Security;

namespace CredentialManagement
{
    public abstract class BaseCredentialsPrompt : ICredentialsPrompt
    {
        private bool _disposed;
        private string _username;
        private SecureString _password;
        private bool _saveChecked;
        private string _message;
        private string _title;
        private int _errorCode;
        private int _dialogFlags;

        public bool SaveChecked
        {
            get
            {
                CheckNotDisposed();
                return _saveChecked;
            }
            set
            {
                CheckNotDisposed();
                _saveChecked = value;
            }
        }

        public string Message
        {
            get
            {
                CheckNotDisposed();
                return _message;
            }
            set
            {
                CheckNotDisposed();
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (value.Length > NativeMethods.CREDUI_MAX_MESSAGE_LENGTH)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _message = value;
            }
        }

        public string Title
        {
            get
            {
                CheckNotDisposed();
                return _title;
            }
            set
            {
                CheckNotDisposed();
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (value.Length > NativeMethods.CREDUI_MAX_CAPTION_LENGTH)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                _title = value;
            }
        }

        public string Username
        {
            get
            {
                CheckNotDisposed();
                return _username ?? string.Empty;
            }
            set
            {
                CheckNotDisposed();
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (value.Length > NativeMethods.CREDUI_MAX_USERNAME_LENGTH)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

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
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                if (value.Length > NativeMethods.CREDUI_MAX_PASSWORD_LENGTH)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                ReplacePassword(SecureStringHelper.CreateSecureString(value));
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

        public int ErrorCode
        {
            get
            {
                CheckNotDisposed();
                return _errorCode;
            }
            set
            {
                CheckNotDisposed();
                _errorCode = value;
            }
        }

        public abstract bool ShowSaveCheckBox { get; set; }

        public abstract bool GenericCredentials { get; set; }

        protected int DialogFlags => _dialogFlags;

        public virtual DialogResult ShowDialog()
        {
            return ShowDialog(IntPtr.Zero);
        }

        public abstract DialogResult ShowDialog(IntPtr owner);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void AddFlag(bool add, int flag)
        {
            if (add)
            {
                _dialogFlags |= flag;
            }
            else
            {
                _dialogFlags &= ~flag;
            }
        }

        protected virtual NativeMethods.CREDUI_INFO CreateCREDUI_INFO(IntPtr owner)
        {
            return new NativeMethods.CREDUI_INFO
            {
                cbSize = Marshal.SizeOf(typeof(NativeMethods.CREDUI_INFO)),
                hwndParent = owner,
                pszCaptionText = Title,
                pszMessageText = Message
            };
        }

        protected void CheckNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing && _password != null)
            {
                _password.Dispose();
                _password = null;
            }

            _disposed = true;
        }

        private void ReplacePassword(SecureString password)
        {
            if (_password != null)
            {
                _password.Dispose();
            }

            _password = password;
        }
    }
}
