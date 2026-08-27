# Contributing

Contributions should preserve the Windows-only API contract and support both target frameworks unless a major-version change explicitly removes `net48`.

Before submitting a change, run:

```powershell
dotnet restore src/CredentialManagement.sln --locked-mode
dotnet build src/CredentialManagement.sln --configuration Release --no-restore -warnaserror
dotnet test src/CredentialManagement.sln --configuration Release --no-build --no-restore
```

Tests that open a native credential dialog must use the `Interactive` category. Credential-store integration tests must use unique targets and remove every created credential in a `finally` block.

Do not commit passwords, exported credentials, private package feeds, generated packages, or local test results.
