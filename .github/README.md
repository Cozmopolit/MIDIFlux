# GitHub Workflows for MIDIFlux

This directory contains GitHub Actions workflows for automated building, testing, and releasing of MIDIFlux.

## Workflows

### 1. Continuous Integration (`ci.yml`)

**Triggers:**
- Push to `main` or `develop` branches
- Pull requests to `main` or `develop` branches

**What it does:**
- Builds the solution in both Debug and Release configurations
- Runs all unit tests with code coverage
- Tests the publish process to ensure executables can be created
- Uploads test results as artifacts

**Purpose:** Ensures code quality and that the project builds correctly on every change.

### 2. Build and Release (`build.yml`)

**Triggers:**
- Push to `main` or `develop` branches (builds only)
- Push of version tags (e.g., `v1.0.0`) - creates releases
- Manual workflow dispatch with optional release creation

**What it does:**
- Builds and tests the solution
- Creates a portable, self-contained Windows executable
- For tagged versions: Creates a GitHub release with the executable
- Uploads build artifacts for all builds

**Purpose:** Creates distributable releases and provides build artifacts for testing.

## Creating a Release

### Automatic Release (Recommended)

1. **Ensure your code is ready for release**
2. **Create and push a version tag:**
   ```bash
   git tag v1.0.0
   git push origin v1.0.0
   ```
3. **The workflow will automatically:**
   - Build and test the code
   - Create a portable executable
   - Create a GitHub release
   - Upload the executable as a release asset

### Manual Release

1. **Go to the Actions tab in your GitHub repository**
2. **Select "Build and Release" workflow**
3. **Click "Run workflow"**
4. **Check "Create a release" option**
5. **Click "Run workflow"**

## Version Naming Convention

- **Release versions:** `v1.0.0`, `v1.2.3`, etc. (semantic versioning)
- **Pre-release versions:** `v1.0.0-beta.1`, `v1.0.0-rc.1`, etc.
- **Development builds:** `0.0.0-dev-{git-sha}` (automatic)

## Build Artifacts

### Release Builds
- **Executable name:** `MIDIFlux-v{version}-win-x64.exe`
- **Type:** Self-contained, single-file executable
- **Target:** Windows x64
- **Dependencies:** All .NET dependencies included

### Development Builds
- **Available as GitHub Actions artifacts**
- **Retention:** 30 days for releases, 7 days for CI builds
- **Access:** Download from the Actions tab

## Build Configuration

The workflows use these key settings:

```yaml
# .NET version
DOTNET_VERSION: '8.0.x'

# Publish settings for portable executable
--runtime win-x64
--self-contained true
-p:PublishSingleFile=true
-p:PublishTrimmed=false
-p:IncludeNativeLibrariesForSelfExtract=true
```

## Troubleshooting

### Build Failures

1. **Check the Actions tab** for detailed error logs
2. **Common issues:**
   - Missing dependencies (check `.csproj` files)
   - Test failures (run tests locally first)
   - Version conflicts (ensure consistent package versions)

### Release Issues

1. **Tag not triggering release:**
   - Ensure tag follows `v*` pattern (e.g., `v1.0.0`)
   - Check that the tag was pushed to the repository

2. **Release creation fails:**
   - Verify GitHub token permissions
   - Check for existing releases with the same tag

### Local Testing

Before creating a release, test the build process locally:

```powershell
# Build and test
dotnet build MIDIFlux.sln --configuration Release
dotnet test src/MIDIFlux.Core.Tests --configuration Release

# Test publish process
dotnet publish src/MIDIFlux.App/MIDIFlux.App.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  --output ./local-publish `
  -p:PublishSingleFile=true
```

## Customization

### Adding New Triggers

Edit the `on:` section in the workflow files:

```yaml
on:
  push:
    branches: [ main, develop, feature/* ]  # Add feature branches
  schedule:
    - cron: '0 2 * * 1'  # Weekly builds on Monday 2 AM
```

### Changing Build Settings

Modify the publish command in `build.yml`:

```yaml
- name: Build portable executable
  run: |
    dotnet publish ${{ env.PROJECT_PATH }} `
      --configuration Release `
      --runtime win-x64 `
      -p:PublishSingleFile=true `
      -p:PublishTrimmed=true `  # Enable trimming for smaller size
      # Add other publish options as needed
```

### Adding Multiple Platforms

Create additional jobs for different platforms:

```yaml
jobs:
  build-windows:
    runs-on: windows-latest
    # Windows build steps
  
  build-linux:
    runs-on: ubuntu-latest
    # Linux build steps (if supported in future)
```

## Security Notes

- **GitHub token:** Workflows use `GITHUB_TOKEN` automatically provided by GitHub
- **Secrets:** No additional secrets required for basic functionality
- **Permissions:** Workflows have read/write access to repository content and releases

## Monitoring

- **Build status:** Check the repository's main page for build badges
- **Failed builds:** GitHub will send email notifications for failures
- **Artifacts:** Available in the Actions tab for 7-30 days depending on type
