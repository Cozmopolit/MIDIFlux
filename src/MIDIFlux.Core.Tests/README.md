# MIDIFlux.Core.Tests

This project contains unit and integration tests for the MIDIFlux Core library, focusing on the hot path MIDI processing pipeline and action system.

## Test Structure

### Infrastructure
- **`TestBase`**: Base class providing common test setup with dependency injection and mock services
- **`ActionTestBase`**: Specialized base class for action testing with parameter manipulation utilities
- **`MockMidiHardwareAdapter`**: Mock implementation of `IMidiHardwareAdapter` for controllable MIDI event simulation

### Test Categories

#### Core Processing Tests (`Processing/`)
- **`MidiActionEngineTests`**: Tests for the main MIDI processing pipeline
  - MIDI event processing and action execution
  - Error handling and async behavior
  - Performance characteristics

#### Action System Tests (`Actions/`)
- **`ActionMappingRegistryTests`**: Tests for action lookup and registry operations
  - Exact matching, wildcard matching, and performance
  - Registry updates and atomic swaps
- **`StatefulActionTests`**: Tests for stateful actions (StateSet, StateIncrease, StateDecrease, StateConditional)
  - State manipulation and conditional logic
  - Parameter validation and execution
- **`ActionParameterTests`**: Tests for the parameter system
  - Parameter validation, serialization, and type handling
- **`ActionTypeRegistryTests`**: Tests for action discovery and instantiation
  - Type registration and instance creation

#### State Management Tests (`State/`)
- **`ActionStateManagerTests`**: Tests for state management operations
  - Thread-safe state operations
  - State initialization and cleanup
  - User-defined vs internal states

#### Integration Tests (`Integration/`)
- **`MidiProcessingIntegrationTests`**: End-to-end tests of the complete MIDI processing pipeline
  - Full MIDI input to action execution flow
  - Multi-device and wildcard device handling
  - Stateful action chains and profile state initialization

### Utilities
- **`MidiEventBuilder`**: Builder pattern for creating test MIDI events
- **`TestAction`**: Test action implementation that tracks execution without side effects

## Running Tests

### Command Line
```bash
# Run all tests
dotnet test src/MIDIFlux.Core.Tests

# Run specific test class
dotnet test src/MIDIFlux.Core.Tests --filter "ClassName=MidiActionEngineTests"

# Run tests with detailed output
dotnet test src/MIDIFlux.Core.Tests --verbosity normal

# Run tests with coverage (requires coverlet)
dotnet test src/MIDIFlux.Core.Tests --collect:"XPlat Code Coverage"
```

### Visual Studio
- Open Test Explorer (Test → Test Explorer)
- Build the solution to discover tests
- Run individual tests or test groups

## Test Coverage

### What We Test
- **Core MIDI processing pipeline**: Event conversion, action lookup, and execution
- **Action system**: Parameter validation, state management, and execution logic
- **State management**: Thread-safe operations, initialization, and cleanup
- **Configuration handling**: Profile loading and device configuration
- **Error handling**: Invalid inputs, missing parameters, and exception scenarios

### What We Don't Test
- **Hardware interactions**: Actual MIDI devices, keyboard/mouse simulation
- **External dependencies**: File system operations, shell command execution
- **GUI components**: User interface interactions and dialogs
- **Real-time performance**: Actual MIDI hardware timing and latency

## Mock Strategy

### MockMidiHardwareAdapter
- Provides controllable MIDI event generation
- Simulates device connections and disconnections
- Tracks sent MIDI output commands
- No actual hardware dependencies

### TestAction
- Tracks execution history without side effects
- Supports all input categories for comprehensive testing
- Provides execution count and MIDI value tracking

### Service Mocking
- Uses real implementations for core services (ActionStateManager, etc.)
- Mocks only hardware-dependent services (IMidiHardwareAdapter)
- Maintains realistic service interactions

## Test Patterns

### Arrange-Act-Assert
All tests follow the AAA pattern for clarity and consistency.

### Fluent Assertions
Uses FluentAssertions for readable and expressive test assertions.

### Theory Tests
Uses xUnit Theory tests for parameterized testing of similar scenarios.

### Async Testing
Properly handles async action execution with appropriate timeouts.

## Adding New Tests

### For New Actions
1. Create tests in `Actions/` directory
2. Inherit from `ActionTestBase`
3. Test parameter validation, execution, and input categories
4. Add integration tests if the action has complex behavior

### For Core Components
1. Create tests in appropriate directory (`Processing/`, `State/`, etc.)
2. Inherit from `TestBase`
3. Focus on public API and error conditions
4. Mock external dependencies appropriately

### For Integration Scenarios
1. Add tests to `Integration/` directory
2. Test complete workflows and component interactions
3. Use realistic configurations and data flows

## Dependencies

- **xUnit**: Test framework
- **FluentAssertions**: Assertion library
- **Moq**: Mocking framework (for future use)
- **Microsoft.Extensions.DependencyInjection**: Dependency injection for test setup
- **Microsoft.Extensions.Logging**: Logging infrastructure for tests

## Performance Considerations

Tests are designed to run quickly while providing comprehensive coverage:
- Mock implementations avoid I/O operations
- Async tests use appropriate timeouts
- State is reset between tests for isolation
- No actual hardware or external process dependencies
