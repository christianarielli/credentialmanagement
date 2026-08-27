using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace CredentialManagement
{
    [Obsolete("Use WindowsCredentialsPrompt instead.")]
    public class VistaPrompt : BaseCredentialsPrompt
    {
        private string? _domain;

        public VistaPrompt()
        {
            Title = "Please provide credentials";
        }

        public string? Domain
        {
            get
            {
                CheckNotDisposed();
                return _domain;
            }
            set
            {
                CheckNotDisposed();
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _domain = value;
            }
        }

        public override bool ShowSaveCheckBox
        {
            get
            {
                CheckNotDisposed();
                return ((int)NativeMethods.WINVISTA_CREDUI_FLAGS.CREDUIWIN_CHECKBOX & DialogFlags) != 0;
            }
            set
            {
                CheckNotDisposed();
                AddFlag(value, (int)NativeMethods.WINVISTA_CREDUI_FLAGS.CREDUIWIN_CHECKBOX);
            }
        }

        public override bool GenericCredentials
        {
            get
            {
                CheckNotDisposed();
                return ((int)NativeMethods.WINVISTA_CREDUI_FLAGS.CREDUIWIN_GENERIC & DialogFlags) != 0;
            }
            set
            {
                CheckNotDisposed();
                AddFlag(value, (int)NativeMethods.WINVISTA_CREDUI_FLAGS.CREDUIWIN_GENERIC);
            }
        }

        public override DialogResult ShowDialog(IntPtr owner)
        {
            CheckNotDisposed();

            if (string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(Message))
            {
                throw new InvalidOperationException("Title or Message must be set.");
            }

            if (!IsWindowsVistaOrHigher)
            {
                throw new PlatformNotSupportedException("The Windows credential prompt requires Windows Vista or later.");
            }

            IntPtr inputBuffer = IntPtr.Zero;
            int inputBufferSize = 0;
            IntPtr outputBuffer = IntPtr.Zero;
            uint outputBufferSize = 0;
            StringBuilder usernameBuffer = new StringBuilder(NativeMethods.CREDUI_MAX_USERNAME_LENGTH);
            StringBuilder passwordBuffer = new StringBuilder(NativeMethods.CREDUI_MAX_PASSWORD_LENGTH);
            StringBuilder domainBuffer = new StringBuilder(NativeMethods.CREDUI_MAX_USERNAME_LENGTH);

            try
            {
                string username = Username;
                string password = Password;
                if (!string.IsNullOrEmpty(username) || !string.IsNullOrEmpty(password))
                {
                    PackInputCredentials(username, password, out inputBuffer, out inputBufferSize);
                }

                uint authenticationPackage = 0;
                bool saveChecked = SaveChecked;
                NativeMethods.CREDUI_INFO credUi = CreateCREDUI_INFO(owner);
                NativeMethods.CredUIReturnCodes result = NativeMethods.CredUIPromptForWindowsCredentials(
                    ref credUi,
                    ErrorCode,
                    ref authenticationPackage,
                    inputBuffer,
                    (uint)inputBufferSize,
                    out outputBuffer,
                    out outputBufferSize,
                    ref saveChecked,
                    DialogFlags);

                SaveChecked = saveChecked;
                if (result == NativeMethods.CredUIReturnCodes.ERROR_CANCELLED)
                {
                    return DialogResult.Cancel;
                }

                if (result != NativeMethods.CredUIReturnCodes.NO_ERROR)
                {
                    throw new Win32Exception((int)result);
                }

                int usernameLength = usernameBuffer.Capacity;
                int passwordLength = passwordBuffer.Capacity;
                int domainLength = domainBuffer.Capacity;
                if (!NativeMethods.CredUnPackAuthenticationBuffer(
                        0,
                        outputBuffer,
                        outputBufferSize,
                        usernameBuffer,
                        ref usernameLength,
                        domainBuffer,
                        ref domainLength,
                        passwordBuffer,
                        ref passwordLength))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                }

                Username = usernameBuffer.ToString();
                Password = passwordBuffer.ToString();
                _domain = domainBuffer.ToString();
                return DialogResult.OK;
            }
            finally
            {
                NativeMethods.ClearStringBuilder(passwordBuffer);
                NativeMethods.ZeroAndFreeCoTaskMem(inputBuffer, inputBufferSize);
                NativeMethods.ZeroAndFreeCoTaskMem(outputBuffer, checked((int)outputBufferSize));
            }
        }

        private static bool IsWindowsVistaOrHigher
        {
            get
            {
                OperatingSystem operatingSystem = Environment.OSVersion;
                return operatingSystem.Platform == PlatformID.Win32NT && operatingSystem.Version.Major >= 6;
            }
        }

        private static void PackInputCredentials(
            string username,
            string password,
            out IntPtr inputBuffer,
            out int inputBufferSize)
        {
            inputBuffer = IntPtr.Zero;
            inputBufferSize = 0;

            if (NativeMethods.CredPackAuthenticationBuffer(
                    0,
                    username,
                    password,
                    IntPtr.Zero,
                    ref inputBufferSize))
            {
                return;
            }

            int error = Marshal.GetLastWin32Error();
            if (error != (int)NativeMethods.CredUIReturnCodes.ERROR_INSUFFICIENT_BUFFER)
            {
                throw new Win32Exception(error, "The supplied credentials could not be packed.");
            }

            inputBuffer = Marshal.AllocCoTaskMem(inputBufferSize);
            if (!NativeMethods.CredPackAuthenticationBuffer(
                    0,
                    username,
                    password,
                    inputBuffer,
                    ref inputBufferSize))
            {
                error = Marshal.GetLastWin32Error();
                NativeMethods.ZeroAndFreeCoTaskMem(inputBuffer, inputBufferSize);
                inputBuffer = IntPtr.Zero;
                inputBufferSize = 0;
                throw new Win32Exception(error, "The supplied credentials could not be packed.");
            }
        }
    }
}
