# MIDIFlux Release Notes

This folder contains detailed release notes for each version of MIDIFlux.

## Naming Convention

Release notes files should be named: `v{version}.md`

Examples:
- `v0.9.0.md` - Release notes for version 0.9.0
- `v1.0.0.md` - Release notes for version 1.0.0
- `v1.0.0-alpha.md` - Release notes for version 1.0.0-alpha

## Automated Release Process

When you push a git tag (e.g., `v0.9.0`), the GitHub Actions workflow will:

1. Look for a matching release notes file in this folder
2. If found, use those detailed notes for the GitHub release
3. If not found, generate generic release notes
4. Automatically append installation instructions
5. Create the GitHub release with the executable attached

## Creating a New Release

1. Create a release notes file in this folder (e.g., `v0.9.0.md`)
2. Write detailed release notes including:
   - Major features
   - New actions or capabilities
   - Bug fixes
   - Breaking changes (if any)
   - Acknowledgments
3. Commit and push the release notes
4. Create and push a git tag: `git tag -a v0.9.0 -m "Release v0.9.0"`
5. Push the tag: `git push origin v0.9.0`
6. GitHub Actions will handle the rest!

## Release Notes Template

See `v0.9.0.md` for a comprehensive example of well-structured release notes.

Key sections to include:
- **Major Features** - Highlight the big additions
- **New Actions** - List any new action types
- **GUI Improvements** - User-facing enhancements
- **Technical Improvements** - Under-the-hood changes
- **Bug Fixes** - Issues resolved
- **Breaking Changes** - Anything that breaks compatibility
- **Requirements** - System requirements
- **Resources** - Links to docs, Discord, GitHub

## Available Releases

- [v0.9.0](v0.9.0.md) - MCP Server Integration and Enhanced Actions

