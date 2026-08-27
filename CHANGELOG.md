# Changelog

All notable changes are documented in this file.

## 2.0.0 - 2026-08-27

### Added

- .NET 10 Windows target with Windows 10 build 17763 as the minimum target platform.
- .NET Framework 4.8 transition target.
- Correctly spelled `PersistenceType` API with a compatibility alias for `PersistanceType`.
- `WindowsCredentialsPrompt` and public `ICredentialsPrompt` APIs.
- `SaveOrThrow`, `LoadOrThrow`, `DeleteOrThrow`, and `CredentialSet.LoadOrThrow` methods.
- Current Windows credential types for generic certificates and extended domain credentials.
- Nullable annotations, modern analyzers, XML documentation, SourceLink, symbol packages, and deterministic builds.
- Windows 2022/2025, x86, and x64 CI verification.

### Changed

- Converted library and tests to SDK-style projects.
- Migrated tests to MSTest 4 and isolated interactive dialog tests.
- Credential integration tests now use unique targets and guaranteed cleanup.
- Repeated `CredentialSet.Load` calls replace previous results instead of appending duplicates.
- Native error codes can be surfaced as `Win32Exception` values.

### Fixed

- Native password memory allocated by `Credential.Save` is overwritten and freed.
- Credential and prompt `SecureString` instances are disposed correctly.
- Credential enumeration memory is owned by a single safe handle and released on exceptions.
- Vista-style prompt input/output buffers are released on every code path.
- XP-style prompt GDI bitmap handles are released.
- Credential blob limits are validated in bytes rather than characters.
- Credential UI return codes are interpreted directly instead of reading an unrelated last-error value.

### Deprecated

- `PersistanceType` and `Credential.PersistanceType`.
- `VistaPrompt` and `XPPrompt` in favor of `WindowsCredentialsPrompt`.

## 1.0.2

- Original .NET Framework 3.5 implementation imported from CodePlex.
