# ViGEm Status and Compatibility

## End-of-Life Status

As of August 2023, the ViGEm project has been officially retired due to trademark conflicts. The project's creator, Nefarius (Benjamin Höglinger-Stelzer), has announced that the ViGEm Bus Driver and related libraries will no longer receive updates.

**Important Notes:**
- Existing installations will continue to work
- No new features or bug fixes will be provided
- The domain `vigem.org` has been transferred to another owner as of December 2023

For more information, see the [official End-of-Life statement](https://docs.nefarius.at/projects/ViGEm/End-of-Life/).

## Compatibility with MIDIFlux

MIDIFlux's game controller functionality relies on the ViGEm Bus Driver and the Nefarius.ViGEm.Client NuGet package. The following versions are known to work with MIDIFlux:

### ViGEm Bus Driver
- Version 1.17.333 or newer is recommended
- Version 1.16.116 (Windows 7/8/8.1 version) is also compatible

### Nefarius.ViGEm.Client NuGet Package
- Version 1.17.178 or newer is recommended
- MIDIFlux currently uses version 1.21.256

## Installation

If you don't have the ViGEm Bus Driver installed, game controller features in MIDIFlux will be automatically disabled with appropriate warnings in the log.

To install the ViGEm Bus Driver:
1. Download the latest version from [GitHub](https://github.com/nefarius/ViGEmBus/releases/latest)
2. Run the installer as administrator
3. Restart your computer

## Future Plans

Given the retired status of ViGEm, MIDIFlux may explore alternative solutions for game controller emulation in the future. Potential options include:

1. Developing a custom driver specifically for MIDIFlux
2. Integrating with alternative controller emulation libraries
3. Using Windows native APIs for input simulation (with more limited functionality)

Any transition to a new solution will be carefully planned to minimize disruption for existing users.

## Troubleshooting

If you encounter issues with the game controller functionality:

1. Verify that the ViGEm Bus Driver is installed correctly
2. Check the MIDIFlux logs for any ViGEm-related errors
3. Try reinstalling the ViGEm Bus Driver
4. Ensure no other applications are conflicting with ViGEm

## References

- [ViGEm GitHub Repository](https://github.com/nefarius/ViGEmBus)
- [Nefarius.ViGEm.Client NuGet Package](https://www.nuget.org/packages/Nefarius.ViGEm.Client)
- [ViGEm End-of-Life Documentation](https://docs.nefarius.at/projects/ViGEm/End-of-Life/)

