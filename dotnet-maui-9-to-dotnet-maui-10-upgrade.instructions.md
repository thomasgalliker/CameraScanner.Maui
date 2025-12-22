# .NET MAUI 9 to .NET MAUI 10 Upgrade Instructions

This document provides instructions for migrating a .NET MAUI 9 project to .NET MAUI 10.

## Prerequisites

- .NET 10 SDK installed (version 10.0.101 or later)
- MAUI workload for .NET 10 installed

## Installation

Install the required workloads:

```bash
dotnet workload install maui-android android
```

For macOS (iOS development):
```bash
dotnet workload install maui-android maui-ios android ios
```

## Migration Steps

### 1. Update Target Frameworks

Update the `TargetFrameworks` property in your `.csproj` files:

**Before (net9.0):**
```xml
<TargetFrameworks>net9.0;net9.0-android;net9.0-ios18.0</TargetFrameworks>
```

**After (net10.0):**
```xml
<TargetFrameworks>net10.0;net10.0-android;net10.0-ios</TargetFrameworks>
```

**Note:** The iOS target framework no longer includes a version suffix in .NET 10.

### 2. Update MAUI Version

Update the `MauiVersion` property:

**Before:**
```xml
<MauiVersion Condition="$(TargetFramework.Contains('net9.0'))">9.0.0</MauiVersion>
```

**After:**
```xml
<MauiVersion Condition="$(TargetFramework.Contains('net10.0'))">10.0.1</MauiVersion>
```

### 3. Update Supported OS Platform Versions

Update any conditional platform version checks:

**Before:**
```xml
<SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios' and $(TargetFramework.Contains('net9.0'))">12.2</SupportedOSPlatformVersion>
```

**After:**
```xml
<SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios' and $(TargetFramework.Contains('net10.0'))">12.2</SupportedOSPlatformVersion>
```

### 4. Update .NET SDK Version in CI/CD

For Azure Pipelines, update the `UseDotNet` task:

**Before:**
```yaml
- task: UseDotNet@2
  displayName: 'Use .NET 9.0.x'
  inputs:
    version: 9.0.x
```

**After:**
```yaml
- task: UseDotNet@2
  displayName: 'Use .NET 10.0.x'
  inputs:
    version: 10.0.x
```

### 5. Update NuGet Packages

Update Microsoft.Extensions packages and other .NET version-specific packages:

```xml
<!-- Before -->
<PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="9.0.7" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="9.0.7" />

<!-- After -->
<PackageReference Include="Microsoft.Extensions.Logging.Debug" Version="10.0.0" />
<PackageReference Include="Microsoft.Extensions.Logging.Abstractions" Version="10.0.0" />
```

### 6. Update AndroidX Packages (If Needed)

Some AndroidX packages may need version updates to resolve dependency conflicts:

```xml
<PackageReference Include="Xamarin.AndroidX.Activity.Ktx" Version="1.10.1.3" />
<PackageReference Include="Xamarin.AndroidX.Collection.Ktx" Version="1.5.0.3" />
<PackageReference Include="Xamarin.AndroidX.Fragment.Ktx" Version="1.8.8.1" />
```

### 7. Add Platform Conditional Builds (Optional)

For cross-platform development environments, you may want to conditionally exclude iOS on non-macOS platforms:

```xml
<PropertyGroup>
  <TargetFrameworks Condition="'$(OS)' != 'Unix' or $([MSBuild]::IsOSPlatform('OSX'))">net10.0;net10.0-android;net10.0-ios</TargetFrameworks>
  <TargetFrameworks Condition="'$(OS)' == 'Unix' and !$([MSBuild]::IsOSPlatform('OSX'))">net10.0;net10.0-android</TargetFrameworks>
</PropertyGroup>
```

## Building the Project

### Restore NuGet Packages

```bash
dotnet restore
```

### Build Debug Configuration

```bash
dotnet build --configuration Debug
```

### Build Release Configuration

```bash
dotnet build --configuration Release
```

### Build Specific Target Framework

```bash
dotnet build /p:TargetFramework=net10.0-android /p:Configuration=Release
```

## Publishing

### Android (AAB)

```bash
dotnet publish -f net10.0-android -c Release /p:AndroidPackageFormat=aab
```

### Android (APK)

```bash
dotnet publish -f net10.0-android -c Release /p:AndroidPackageFormat=apk
```

### iOS (requires macOS)

```bash
dotnet publish -f net10.0-ios -c Release
```

## Testing

Run unit tests to ensure functionality after migration:

```bash
dotnet test --configuration Debug
```

## Known Issues and Warnings

### AndroidX Package Version Warnings

You may see warnings about AndroidX package version constraints:

```
warning NU1608: Detected package version outside of dependency constraint
```

These warnings are generally safe to ignore as long as the build succeeds. They occur when transitive dependencies are resolved to newer versions than explicitly specified.

### Application.MainPage Obsolete Warnings

.NET MAUI 10 has deprecated `Application.MainPage` in favor of `Windows[0].Page`. Update your code if you see these warnings:

**Before:**
```csharp
var page = Application.Current.MainPage;
```

**After:**
```csharp
var page = Application.Current.Windows[0].Page;
```

## Verification Checklist

After migration, verify the following:

- [ ] Project restores without errors
- [ ] Project builds in Debug mode without errors
- [ ] Project builds in Release mode without errors
- [ ] All unit tests pass
- [ ] Android app can be published (if applicable)
- [ ] iOS app can be published on macOS (if applicable)
- [ ] App runs correctly on target platforms

## Additional Resources

- [.NET 10 Release Notes](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-10)
- [.NET MAUI 10 Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [Migrating to .NET 10](https://learn.microsoft.com/en-us/dotnet/core/migration/)

## Troubleshooting

### Build Fails with Missing Workload

If you see errors about missing workloads:

```bash
dotnet workload restore
```

### Android AAR Download Failures

If building for Android fails with AAR download errors, ensure you have network access to `dl.google.com`. This is typically required for the first build to download Android dependencies.

### iOS Build Requires macOS

iOS builds require a macOS environment with Xcode installed. iOS builds cannot be performed on Windows or Linux.
