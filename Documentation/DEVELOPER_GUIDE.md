# Developer Guide

Technical documentation for developers working on MIDIFlux or contributing to the project.

## Building from Source

### Prerequisites
- **Operating System**: Windows 10/11
- **.NET SDK**: .NET 8.0 SDK
- **IDE**: Visual Studio 2022 or VS Code (recommended)
- **Git**: For version control

### Build Steps

```bash
# Clone the repository
git clone https://github.com/Cozmopolit/MIDIFlux.git
cd MIDIFlux

# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release

# Run the application
dotnet run --project src\MIDIFlux.App
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Project Architecture

### Solution Structure

```
MIDIFlux/
├── src/
│   ├── MIDIFlux.Core/          # Core library (MIDI handling, actions)
│   ├── MIDIFlux.App/           # Windows Forms application
│   ├── MIDIFlux.GUI/           # GUI components library
│   └── MIDIFlux.Core.Tests/    # Unit and integration tests
├── Documentation/              # Project documentation
└── .github/                    # GitHub Actions workflows
```

### Core Components

#### MIDIFlux.Core
- **MIDI Processing**: Device management, event handling
- **Action System**: All action types and execution logic
- **State Management**: Profile-scoped state system
- **Configuration**: JSON profile loading and validation

#### MIDIFlux.App  
- **System Tray**: Main application interface
- **Device Management**: MIDI device discovery and connection
- **Profile Management**: Loading and switching profiles

#### MIDIFlux.GUI
- **Configuration Editor**: Visual profile editing
- **Action Configuration**: Type-specific parameter editors
- **Device Selection**: MIDI device and channel selection

### Key Architectural Patterns

#### Action System
- **Interface-based**: All actions implement `IAction`
- **Reflection-based Discovery**: Automatic action type registration
- **Strongly-typed Parameters**: Type-safe configuration with validation
- **Hot Path Optimization**: Pre-compiled actions for performance

#### MIDI Event Processing
```
MIDI Hardware → NAudio → MidiDeviceManager → ProfileManager → Action Execution
```

1. **Hardware Events**: Raw MIDI from devices via NAudio
2. **Event Normalization**: Convert to internal `MidiEvent` format
3. **O(1) Lookup**: Pre-computed dictionary for action matching
4. **Async Execution**: Actions execute asynchronously for performance

#### State Management
- **Profile-scoped**: States cleared on profile changes
- **Thread-safe**: Concurrent dictionary for real-time access
- **User + Internal**: Custom states + automatic key/controller tracking

## Development Setup

### AppData Directory Access

Create a junction for easy access to configuration files:

```powershell
# Navigate to solution directory
cd "path\to\MIDIFlux"

# Create junction to MIDIFlux AppData directory
cmd /c "mklink /J appdata-midiflux %AppData%\MIDIFlux"
```

### Development Workflow

```bash
# Edit configuration files directly
code appdata-midiflux/profiles/your-profile.json

# View application logs
dir appdata-midiflux/Logs

# Check current settings
code appdata-midiflux/appsettings.json

# Browse example configurations
dir appdata-midiflux/profiles/examples
```

### File Locations
- **Profiles**: `%AppData%\MIDIFlux\profiles\`
- **Examples**: `%AppData%\MIDIFlux\profiles\examples\`
- **Logs**: `%AppData%\MIDIFlux\Logs\`
- **Settings**: `%AppData%\MIDIFlux\appsettings.json`

## Testing

### Test Structure
- **Unit Tests**: Core logic and action implementations
- **Integration Tests**: MIDI processing and profile loading
- **Performance Tests**: Action execution timing and memory usage

### Test Categories
- **Action Tests**: Verify all action types work correctly
- **State Tests**: State management and concurrency
- **Configuration Tests**: Profile validation and loading
- **MIDI Tests**: Event processing and device management

### Running Specific Tests

```bash
# Run specific test category
dotnet test --filter Category=Actions

# Run specific test class
dotnet test --filter ClassName=MidiActionEngineTests

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"

# Run tests for specific project
dotnet test src/MIDIFlux.Core.Tests
```

### Viewing Coverage Reports

After running tests with coverage collection, you can generate HTML reports:

```bash
# Install the report generator tool (one-time setup)
dotnet tool install -g dotnet-reportgenerator-globaltool

# Generate HTML coverage report
reportgenerator -reports:TestResults/*/coverage.cobertura.xml -targetdir:TestResults/CoverageReport

# Open the report
start TestResults/CoverageReport/index.html
```

## Contributing Guidelines

### Code Style
- **C# Conventions**: Follow standard C# naming conventions
- **Async/Await**: Use async patterns for I/O operations
- **Immutable Types**: Prefer immutable data structures
- **Dependency Injection**: Use DI for service dependencies

### Action Development

#### Creating New Actions

1. **Implement IAction Interface**:
```csharp
public class MyCustomAction : IAction
{
    public async ValueTask ExecuteAsync(int midiValue)
    {
        // Implementation
    }
}
```

2. **Create Parameters Class**:
```csharp
public class MyCustomActionParameters
{
    public string RequiredParameter { get; set; }
    public int OptionalParameter { get; set; } = 42;
}
```

3. **Automatic Discovery**: No registration needed - reflection handles it

#### Action Guidelines
- **Performance**: Keep hot path actions lightweight
- **Error Handling**: Handle exceptions gracefully
- **State Management**: Use provided state system for persistence
- **Validation**: Validate parameters in constructor

### Pull Request Process

1. **Fork and Branch**: Create feature branch from `develop`
2. **Implement Changes**: Follow coding standards and patterns
3. **Add Tests**: Include unit tests for new functionality
4. **Update Documentation**: Update relevant documentation
5. **Submit PR**: Target `develop` branch with clear description

### Release Process

#### GitHub Actions
- **CI/CD**: Automated building and testing on push/PR
- **Release Builds**: Automatic executable generation on tags
- **Artifacts**: Development builds available as GitHub artifacts

#### Version Management
- **Alpha Releases**: Current development status
- **Semantic Versioning**: Major.Minor.Patch format
- **Breaking Changes**: Documented in release notes

## Technical Details

### Performance Optimizations
- **O(1) Action Lookup**: Pre-computed mapping keys
- **Minimal Allocations**: Object reuse in hot paths
- **Async Execution**: Non-blocking action execution
- **State Caching**: Efficient state access patterns

### Memory Management
- **Action Pre-compilation**: Created at profile load time
- **State Cleanup**: Automatic cleanup on profile changes
- **Event Object Reuse**: Minimize garbage collection
- **Resource Disposal**: Proper cleanup of MIDI resources

### Error Handling
- **Graceful Degradation**: Continue on device disconnection
- **Action Isolation**: Failed actions don't affect others
- **Comprehensive Logging**: Detailed error information
- **Configuration Validation**: Early error detection

## Dependencies

### Core Dependencies
- **NAudio**: MIDI device access and audio playback
- **ViGEm.NET**: Xbox controller emulation (optional)
- **Newtonsoft.Json**: Configuration serialization
- **.NET 8.0**: Runtime and framework

### Development Dependencies
- **xUnit**: Unit testing framework
- **Moq**: Mocking framework for tests
- **Coverlet**: Code coverage analysis
- **FluentAssertions**: Readable test assertions

## Debugging

### Common Issues
- **MIDI Device Access**: Check device permissions and drivers
- **Action Execution**: Enable debug logging for detailed traces
- **Configuration Errors**: Validate JSON syntax and required properties
- **State Issues**: Monitor state changes in logs

### Debugging Tools
- **Visual Studio Debugger**: Full debugging support
- **Application Logs**: Detailed execution traces
- **MIDI Monitor**: External tools for MIDI event inspection
- **Performance Profiler**: Memory and CPU usage analysis

## Future Development

### Planned Features
- **Plugin System**: External action type loading
- **Advanced GUI**: Enhanced configuration editor
- **MIDI Recording**: Event recording and playback
- **Performance Metrics**: Built-in monitoring

### Architecture Evolution
- **Microservices**: Potential service-based architecture
- **Cross-Platform**: .NET Core compatibility exploration
- **Cloud Integration**: Remote configuration management
- **Real-time Collaboration**: Multi-user configuration editing

---

For user documentation, see [Getting Started](GETTING_STARTED.md) and [Action Reference](ACTION_REFERENCE.md).
