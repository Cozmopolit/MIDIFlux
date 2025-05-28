# ActionFactory Service Dependency Issue

## Problem Description

The current ActionFactory implementation has a critical design flaw that prevents proper round-trip serialization of actions between JSON configuration and runtime objects.

### Current Issue

1. **ActionFactory requires services** (like `ActionStateManager`) to create action instances
2. **Services are not available in GUI context** (ProfileEditorControl)
3. **Workaround created display-only actions** that can't be converted back to configuration
4. **Round-trip conversion fails** - saving a profile loses existing mappings, only keeping newly added ones

### Root Cause

The ActionFactory tries to inject services during action construction:

```csharp
// In ActionFactory.CreateAction()
var action = new KeyPressReleaseAction(
    config.VirtualKeyCode,
    GetRequiredService<ActionStateManager>() // ❌ Fails in GUI context
);
```

This creates two incompatible contexts:
- **Runtime context**: Services available, actions fully functional
- **GUI context**: No services available, can't create real actions

## Proposed Solution

**Key Insight**: Most action constructors already accept optional service dependencies (e.g., `ActionStateManager? actionStateManager = null`). The real problem is that ActionFactory calls `GetRequiredService<ActionStateManager>()` instead of passing `null` when services aren't available.

### Simple Solution

**Make ActionFactory context-aware: services are required in runtime context, optional in GUI context.**

This prevents silent failures during normal application operations while allowing GUI operations to work without services.

### Implementation Approach

#### 1. Add Context-Aware Service Resolution to ActionFactory

```csharp
public class ActionFactory
{
    private readonly IServiceProvider? _serviceProvider;
    private readonly bool _isGuiContext;

    // Runtime constructor - services required
    public ActionFactory(ILogger logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _isGuiContext = false;
    }

    // GUI constructor - services intentionally not available
    public static ActionFactory CreateForGui(ILogger logger)
    {
        return new ActionFactory(logger, null, isGuiContext: true);
    }

    private ActionFactory(ILogger logger, IServiceProvider? serviceProvider, bool isGuiContext)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _isGuiContext = isGuiContext;
    }

    /// <summary>
    /// Gets an optional service - null in GUI context, required in runtime context.
    /// </summary>
    private T? GetOptionalService<T>() where T : class
    {
        if (_isGuiContext)
        {
            return null; // Services intentionally not available in GUI
        }

        // In runtime context, services should be available
        return _serviceProvider!.GetRequiredService<T>();
    }
}
```

#### 2. Update ActionFactory CreateAction Method

```csharp
public IAction CreateAction(ActionConfig config)
{
    return config switch
    {
        // Actions with optional services - pass null when services unavailable
        KeyPressReleaseConfig keyConfig => new Simple.KeyPressReleaseAction(keyConfig, GetOptionalService<ActionStateManager>()),
        KeyDownConfig keyDownConfig => new Simple.KeyDownAction(keyDownConfig, GetOptionalService<ActionStateManager>()),
        KeyUpConfig keyUpConfig => new Simple.KeyUpAction(keyUpConfig, GetOptionalService<ActionStateManager>()),
        KeyToggleConfig keyToggleConfig => new Simple.KeyToggleAction(keyToggleConfig, GetOptionalService<ActionStateManager>()),

        // Actions that don't need services - no changes
        MouseClickConfig mouseClickConfig => new Simple.MouseClickAction(mouseClickConfig),
        MouseScrollConfig mouseScrollConfig => new Simple.MouseScrollAction(mouseScrollConfig),
        CommandExecutionConfig cmdConfig => new Simple.CommandExecutionAction(cmdConfig),
        DelayConfig delayConfig => new Simple.DelayAction(delayConfig),

        // Actions requiring services - throw clear error when services unavailable
        SetStateConfig setStateConfig => new Stateful.SetStateAction(setStateConfig, GetRequiredService<ActionStateManager>()),
        MidiOutputConfig midiOutputConfig => new Simple.MidiOutputAction(midiOutputConfig, GetRequiredService<MidiManager>()),

        // Complex actions - handle based on service requirements
        SequenceConfig seqConfig => new Complex.SequenceAction(seqConfig, this),
        ConditionalConfig condConfig => new Complex.ConditionalAction(condConfig, this),
        AlternatingActionConfig altConfig => new Complex.AlternatingAction(altConfig, GetRequiredService<ActionStateManager>(), this),
        StateConditionalConfig stateCondConfig => new Stateful.StateConditionalAction(stateCondConfig, GetRequiredService<ActionStateManager>(), this),

        _ => throw new NotSupportedException($"Action config type {config.GetType().Name} is not supported")
    };
}
```

#### 3. Context-Specific Factory Creation

**Runtime Context (services required):**
```csharp
var factory = new ActionFactory(logger, serviceProvider); // Throws if serviceProvider is null
var actions = factory.CreateAction(config); // Fully functional
```

**GUI Context (services intentionally unavailable):**
```csharp
var factory = ActionFactory.CreateForGui(logger); // Explicit GUI context
var actions = factory.CreateAction(config); // Display/serialization only
```

## Benefits

1. **Unified Action System**: Same action classes used everywhere
2. **Clean Round-Trip**: Actions serialize to/from JSON without loss
3. **No Workarounds**: No need for display-only action wrappers
4. **Minimal Changes**: Leverages existing optional service pattern
5. **Fail Fast**: Runtime context throws immediately if services are missing
6. **No Silent Failures**: Clear distinction between runtime and GUI contexts
7. **Simple Implementation**: Just a boolean flag and factory method

## Simplified Implementation Plan

### Phase 1: Analyze Current Service Dependencies

• **Actions with optional services** (already compatible):
  - KeyPressReleaseAction, KeyDownAction, KeyUpAction, KeyToggleAction
  - MouseClickAction, MouseScrollAction, CommandExecutionAction, DelayAction
  - GameControllerButtonAction, GameControllerAxisAction

• **Actions requiring services** (need clear error handling):
  - SetStateAction (requires ActionStateManager)
  - MidiOutputAction (requires MidiManager)
  - AlternatingAction (requires ActionStateManager)
  - StateConditionalAction (requires ActionStateManager)

• **Complex actions** (pass ActionFactory, handle service requirements internally):
  - SequenceAction, ConditionalAction

### Phase 2: Update ActionFactory Only

• Add `CreateForGui()` static factory method
• Add private constructor with `isGuiContext` parameter
• Add `GetOptionalService<T>()` method with context-aware behavior
• Update CreateAction switch statement to use GetOptionalService for actions with optional dependencies
• Keep GetRequiredService for actions that truly require services
• No action constructor changes needed - they already handle null services

### Phase 3: Fix ProfileEditorControl

• Remove ConfigurationPreservingAction class completely
• Remove _originalMappingConfigs dictionary and related logic
• Update LoadConfiguration to use ActionConfigurationLoader with service-free ActionFactory
• Simplify CreateDisplayMappingFromConfig to use real actions via ActionFactory
• Update SaveConfiguration to use standard ConvertFromMappings method
• Remove ConvertNewMappingToConfig and all extraction methods
• Remove FormatSysExPattern duplicate (use the one in ActionConfigurationLoader)

### Phase 4: Update ActionConfigurationLoader (if needed)

• Verify ActionConfigurationLoader works with service-free ActionFactory
• Ensure ConvertFromMappings works with both service-enabled and service-free actions
• No changes should be needed since it already uses ActionFactory

### Phase 5: Update Service Registration

• Verify ActionFactory registration in DI container includes IServiceProvider
• Update ProfileEditorControl to use `ActionFactory.CreateForGui(logger)`
• Check that runtime contexts (MidiProcessingService) create ActionFactory with services

### Phase 6: Testing and Validation

• Test profile loading in runtime context (should work as before)
• Test profile editor loading (should show all mappings)
• Test adding new mappings in profile editor
• Test saving profiles (should preserve all existing mappings)
• Test round-trip: load profile → edit → save → reload (should be identical)
• Verify actions execute properly in runtime context
• Verify actions requiring services throw clear exceptions when executed without services in GUI context

### Phase 7: Clean Up and Documentation

• Update code comments to reflect the simplified approach
• Ensure ActionFactory documentation explains optional vs required service behavior
• Review error handling and exception messages for clarity

## Files to Modify

- `src/MIDIFlux.Core/Actions/ActionFactory.cs` - Add GetOptionalService method and update CreateAction
- `src/MIDIFlux.GUI/Controls/ProfileEditor/ProfileEditorControl.cs` - Remove ConfigurationPreservingAction workarounds
- `src/MIDIFlux.Core/Configuration/ActionConfigurationLoader.cs` - Verify compatibility (likely no changes needed)

## Expected Outcome

After implementation:
- Profile editor shows all existing mappings correctly using real action instances
- Adding new mappings preserves existing ones
- Saving profiles maintains all mappings without data loss
- Same action classes work in both runtime and GUI contexts
- Clean, maintainable codebase without complex workarounds
- Minimal code changes leveraging existing optional service pattern
