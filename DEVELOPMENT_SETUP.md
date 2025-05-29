# MIDIFlux Development Setup

## AppData Directory Access

During development, you often need to access MIDIFlux configuration files that are stored in `%AppData%\MIDIFlux`. To make this easier, we've created directory junctions in the solution directory.

### Available Junction

- **`appdata-midiflux/`** → `%AppData%\MIDIFlux\` (complete MIDIFlux AppData directory)

### Usage

You can now access AppData files directly from the solution directory:

```bash
# View all AppData files
dir appdata-midiflux

# Edit a profile file
code appdata-midiflux/profiles/rolf.json

# View logs
dir appdata-midiflux/Logs
```

### Setting Up Junctions (for new developers)

If you're setting up a new development environment, create these junctions:

```powershell
# Navigate to solution directory
cd "path\to\MIDIFlux"

# Create junction to MIDIFlux AppData directory
cmd /c "mklink /J appdata-midiflux %AppData%\MIDIFlux"
```

### Notes

- **Junctions vs Symbolic Links**: We use junctions because they don't require administrator privileges
- **Git Ignore**: These junctions are ignored by Git (see `.gitignore`)
- **Cross-Platform**: On Linux/Mac, use symbolic links instead: `ln -s ~/.config/MIDIFlux appdata-midiflux`

### Usage Examples

```powershell
# Edit your profile directly
code appdata-midiflux/profiles/rolf.json

# View application logs
dir appdata-midiflux/Logs

# Check current settings
code appdata-midiflux/appsettings.json

# Browse example configurations
dir appdata-midiflux/profiles/examples
```

### Benefits

1. **Easy Access**: Edit AppData files directly from your IDE
2. **No Path Issues**: No need to navigate to complex AppData paths
3. **Development Workflow**: Test configuration changes without copying files
4. **Debugging**: Easy access to logs and current configuration

### File Structure

```
MIDIFlux/
├── src/                    # Source code
└── appdata-midiflux/      # → %AppData%\MIDIFlux\ (junction)
    ├── profiles/          # User profile files
    │   ├── examples/      # Example profile configurations
    │   ├── rolf.json      # User profiles
    │   └── *.json         # Other user profiles
    ├── Logs/              # Application logs
    ├── appsettings.json   # Application settings
    └── current.json       # Currently loaded profile
```
