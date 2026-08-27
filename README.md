# CredentialManagement

CredentialManagement is a Windows-only .NET library for storing generic credentials in Windows Credential Manager and displaying the native Windows credential-provider dialog.

Version 2 modernizes the original .NET Framework 3.5 library for current Windows and .NET releases while retaining a .NET Framework 4.8 compatibility target.

## Platform support

| Target | Intended use |
| --- | --- |
| `net10.0-windows10.0.17763` | Primary target for Windows 11 and supported Windows 10 1809/LTSC environments |
| `net48` | Transition target for existing .NET Framework applications |

Windows 10 Home and Pro reached end of support in October 2025. The library can target Windows 10 build 17763 or later, but production deployments should use an operating-system edition still supported by Microsoft. Windows 11 is the recommended client platform.

## Install

```powershell
dotnet add package CredentialManagement --version 2.0.0
```

## Store, read, and delete a credential

```csharp
using CredentialManagement;

const string target = "com.example.application";

using (var credential = new Credential("user@example.com", "secret", target))
{
    credential.PersistenceType = PersistenceType.LocalComputer;
    credential.SaveOrThrow();
}

using (var credential = new Credential { Target = target })
{
    credential.LoadOrThrow();
    Console.WriteLine(credential.Username);

    // Use the secret only for as long as necessary.
    string? password = credential.Password;

    credential.DeleteOrThrow();
}
```

`Save()`, `Load()`, and `Delete()` remain available for compatibility and return `false` on a native failure. New code should normally use the `OrThrow` variants so that `Win32Exception.NativeErrorCode` is preserved.

## Check or enumerate credentials

```csharp
using var credential = new Credential
{
    Target = "com.example.application",
    Type = CredentialType.Generic
};

if (credential.Exists())
{
    credential.LoadOrThrow();
}

using var credentials = new CredentialSet("com.example.*");
credentials.LoadOrThrow();

foreach (Credential item in credentials)
{
    Console.WriteLine(item.Target);
}
```

Always use an application-specific target or filter. Avoid enumerating the entire user credential store unless the application explicitly needs to do so.

## Show the Windows credential dialog

```csharp
using var prompt = new WindowsCredentialsPrompt
{
    Title = "Sign in",
    Message = "Enter the credentials for the service.",
    GenericCredentials = true,
    ShowSaveCheckBox = true
};

if (prompt.ShowDialog() == DialogResult.OK)
{
    Console.WriteLine(prompt.Username);
    string password = prompt.Password;
}
```

The dialog returns credentials to the process; it does not save them automatically. The application decides whether and how to persist them.

## Security notes

- Windows Credential Manager stores credentials for the current Windows logon session and user.
- Native credential and dialog buffers are overwritten before being released.
- `Credential`, `CredentialSet`, and prompt instances are disposable and should always be used in `using` statements.
- The legacy `SecurePassword` API remains for compatibility. Modern .NET does not recommend `SecureString` as a general security boundary; prefer leaving credentials in Windows Credential Manager and minimize the time a plaintext password exists in process memory.
- Do not place passwords in logs, exception messages, command-line arguments, or configuration files.

## Migrating from 1.x

| 1.x API | 2.0 API |
| --- | --- |
| .NET Framework 3.5 project | `net10.0-windows10.0.17763` or transitional `net48` |
| `PersistanceType` | `PersistenceType` |
| `credential.PersistanceType` | `credential.PersistenceType` |
| `VistaPrompt` | `WindowsCredentialsPrompt` |
| `XPPrompt` | `WindowsCredentialsPrompt` |
| `Save()`, `Load()`, `Delete()` | Prefer `SaveOrThrow()`, `LoadOrThrow()`, `DeleteOrThrow()` |

The misspelled persistence enum/property and the older prompt classes remain available with `[Obsolete]` annotations to support staged migrations.

## Build and test

Install the SDK selected by `global.json`, then run from the repository root:

```powershell
dotnet restore src/CredentialManagement.sln --locked-mode
dotnet build src/CredentialManagement.sln --configuration Release --no-restore -warnaserror
dotnet test src/CredentialManagement.sln --configuration Release --no-build --no-restore
dotnet pack src/CredentialManagement/CredentialManagement.csproj --configuration Release --no-build --no-restore
```

The default runsettings exclude tests that open interactive Windows dialogs. Run them manually from an interactive desktop when required:

```powershell
dotnet test tests/CredentialManagement.Test/CredentialManagement.Test.csproj `
  --filter "TestCategory=Interactive" `
  --settings tests/test.runsettings
```

Continuous integration verifies Windows Server 2022 and 2025 plus x86 and x64 builds.

## License

CredentialManagement is licensed under the [Apache License 2.0](LICENSE).
