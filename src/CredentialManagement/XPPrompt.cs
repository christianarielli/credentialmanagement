using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace CredentialManagement
{
    public class XPPrompt : BaseCredentialsPrompt
    {

        string _target;
        Bitmap _banner;

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
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException("value");
                }
                _target = value;
            }
        }
        public Bitmap Banner
        {
            get
            {
                CheckNotDisposed();
                return _banner;
            }
            set
            {
                CheckNotDisposed();
                if (null != _banner)
                {
                    _banner.Dispose();
                }
                _banner = value;
            }
        }

        public bool CompleteUsername
        {
            get
            {
                CheckNotDisposed();
                return 0 != ((int)NativeMethods.WINXP_CREDUI_FLAGS.COMPLETE_USERNAME & DialogFlags);
            }
            set
            {
                CheckNotDisposed();
                AddFlag(value, (int)NativeMethods.WINXP_CREDUI_FLAGS.COMPLETE_USERNAME);
            }
        }
        public bool DoNotPersist
        {
            get
            {
                CheckNotDisposed();
                return 0 != ((int)NativeMethods.WINXP_CREDUI_FLAGS.DO_NOT_PERSIST & DialogFlags);
            }
            set
            {
                CheckNotDisposed();
                AddFlag(value, (int)NativeMethods.WINXP_CREDUI_FLAGS.DO_NOT_PERSIST);
            }
        }
        public bool ExcludeCertificates
        {
            get
            {
                CheckNotDisposed();
                return 0 != ((int)NativeMethods.WINXP_CREDUI_FLAGS.EXCLUDE_CERTIFICATES & DialogFlags);
            }
            set
            {
                CheckNotDisposed();
                AddFlag(value, (int)NativeMethods.WINXP_CREDUI_FLAGS.EXCLUDE_CERTIFICATES);
            }
        }
        public bool ExpectConfirmation
        {
            get
            {
                CheckNotDisposed();
                return 0 != ((int)NativeMethods.WINXP_CREDUI_FLAGS.EXPECT_CONFIRMATION & DialogFlags);
            }
            set
            {
                CheckNotDisposed();
                AddFlag(value, (int)NativeMethods.WINXP_CREDUI_FLAGS.EXPECT_CONFIRMATION);
            }
        }
        public bool IncorrectPassword
        {
            get
            {
                CheckNotDisposed();
                return 0 != ((int)NativeMethods.WINXP_CREDUI_FLAGS.INCORRECT_PASSWORD & DialogFlags);
            }
            set
            {
                CheckNotDisposed();
                AddFlag(value, (int)NativeMethods.WINXP_CREDUI_FLAGS.INCORRECT_PASSWORD);
            }
        }
        public bool Persist
        {
            get
            {
                CheckNotDisposed();
                return 0 != ((int)NativeMethods.WINXP_CREDUI_FLAGS.PERSIST & DialogFlags);
            }
            set
            {
                CheckNotDisposed();
                AddFlag(value, (int)NativeMethods.WINXP_CREDUI_FLAGS.PERSIST);
            }
        }
        public bool RequestAdministrator
        {
            get
            {
                CheckNotDisposed();
                return 0 != ((int)NativeMethods.WINXP_CREDUI_FLAGS.REQUEST_ADMINISTRATOR & DialogFlags);
            }
            set
            {
                CheckNotDisposed();
                AddFlag(value, (int)NativeMethods.WINXP_CREDUI_FLAGS.REQUEST_ADMINISTRATOR);
            }
        }
        public bool RequireCertificate
        {
            get
            {
                CheckNotDisposed();
                return 0 != ((int)NativeMethods.WINXP_CREDUI_FLAGS.REQUIRE_CERTIFICATE & DialogFlags);
            }
            set
            {
                CheckNotDisposed();
                AddFlag(value, (int)NativeMethods.WINXP_CREDUI_FLAGS.REQUIRE_CERTIFICATE);
            }
        }
        public bool RequireSmartCard
        {
            get
            {
                CheckNotDisposed();
                return 0 != ((int)NativeMethods.WINXP_CREDUI_FLAGS.REQUIRE_SMARTCARD & DialogFlags);
            }
            set
            {
                CheckNotDisposed();
                AddFlag(value, (int)NativeMethods.WINXP_CREDUI_FLAGS.REQUIRE_SMARTCARD);
            }
        }
        public bool UsernameReadOnly
        {
            get
            {
                CheckNotDisposed();
                return 0 != ((int)NativeMethods.WINXP_CREDUI_FLAGS.KEEP_USERNAME & DialogFlags);
            }
            set
            {
                CheckNotDisposed();
                AddFlag(value, (int)NativeMethods.WINXP_CREDUI_FLAGS.KEEP_USERNAME);
            }
        }
        public bool ValidateUsername
        {
            get
            {
                CheckNotDisposed();
                return 0 != ((int)NativeMethods.WINXP_CREDUI_FLAGS.VALIDATE_USERNAME & DialogFlags);
            }
            set
            {
                CheckNotDisposed();
                AddFlag(value, (int)NativeMethods.WINXP_CREDUI_FLAGS.VALIDATE_USERNAME);
            }
        }
        public override bool ShowSaveCheckBox
        {
            get
            {
                CheckNotDisposed();
                return 0 != ((int)NativeMethods.WINXP_CREDUI_FLAGS.SHOW_SAVE_CHECK_BOX & DialogFlags);
            }
            set
            {
                CheckNotDisposed();
                AddFlag(value, (int)NativeMethods.WINXP_CREDUI_FLAGS.SHOW_SAVE_CHECK_BOX);
            }
        }
        public override bool GenericCredentials
        {
            get
            {
                CheckNotDisposed();
                return 0 != ((int)NativeMethods.WINXP_CREDUI_FLAGS.GENERIC_CREDENTIALS & DialogFlags);
            }
            set
            {
                CheckNotDisposed();
                AddFlag(value, (int)NativeMethods.WINXP_CREDUI_FLAGS.GENERIC_CREDENTIALS);
            }
        }
        public bool AlwaysShowUI
        {
            get
            {
                CheckNotDisposed();
                return 0 != ((int)NativeMethods.WINXP_CREDUI_FLAGS.ALWAYS_SHOW_UI & DialogFlags);
            }
            set
            {
                CheckNotDisposed();
                AddFlag(value, (int)NativeMethods.WINXP_CREDUI_FLAGS.ALWAYS_SHOW_UI);
            }
        }

        protected override NativeMethods.CREDUI_INFO CreateCREDUI_INFO(IntPtr owner)
        {
            NativeMethods.CREDUI_INFO info = base.CreateCREDUI_INFO(owner);
            info.hbmBanner = null == Banner ? IntPtr.Zero : Banner.GetHbitmap();
            return info;
        }
        public override DialogResult ShowDialog(IntPtr owner)
        {
            CheckNotDisposed();

            if (string.IsNullOrEmpty(Target))
            {
                throw new InvalidOperationException("Target must always be specified.");
            }

            if (AlwaysShowUI && !GenericCredentials)
            {
                throw new InvalidOperationException("AlwaysShowUI must be specified with GenericCredentials property.");
            }

            NativeMethods.CREDUI_INFO credUI = CreateCREDUI_INFO(owner);
            StringBuilder usernameBuffer = new StringBuilder(NativeMethods.CREDUI_MAX_USERNAME_LENGTH);
            StringBuilder passwordBuffer = new StringBuilder(NativeMethods.CREDUI_MAX_PASSWORD_LENGTH);
            bool persist = SaveChecked;

            try
            {
                NativeMethods.CredUIReturnCodes result = NativeMethods.CredUIPromptForCredentials(
                    ref credUI,
                    Target,
                    IntPtr.Zero,
                    ErrorCode,
                    usernameBuffer,
                    usernameBuffer.Capacity,
                    passwordBuffer,
                    passwordBuffer.Capacity,
                    ref persist,
                    DialogFlags);

                SaveChecked = persist;
                if (result == NativeMethods.CredUIReturnCodes.ERROR_CANCELLED)
                {
                    return DialogResult.Cancel;
                }

                if (result != NativeMethods.CredUIReturnCodes.NO_ERROR)
                {
                    throw new Win32Exception((int)result);
                }

                Username = usernameBuffer.ToString();
                Password = passwordBuffer.ToString();
                return DialogResult.OK;
            }
            finally
            {
                NativeMethods.ClearStringBuilder(passwordBuffer);
                if (credUI.hbmBanner != IntPtr.Zero)
                {
                    NativeMethods.DeleteObject(credUI.hbmBanner);
                    credUI.hbmBanner = IntPtr.Zero;
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _banner != null)
            {
                _banner.Dispose();
                _banner = null;
            }

            base.Dispose(disposing);
        }
    }
}
