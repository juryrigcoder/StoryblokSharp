# Changelog

All notable changes to this project will be documented in this file.

## [0.0.9] - 2025-01-18

### Fixed
- Add missing checkout step in release workflow to enable CHANGELOG.md access in releases

## [0.0.8] - 2025-01-18

### Added
- Automated release process using GitHub Actions
- Automated changelog generation
- NuGet package publishing to GitHub Packages
- Integrated automated testing in CI pipeline
- Release notes automation through release.yml configuration

### Changed
- Updated build workflow to extract version from csproj
- Standardized release naming convention

### Infrastructure
- Added GitHub Actions workflow for building, testing, and releasing
- Added release.yml for automated release notes categorization
- Set up automated NuGet package generation

## [0.0.7] - 2025-01-18

### Added
- Initial release of StoryblokSharp
- Basic Storyblok API integration for .NET
- Memory caching support
- Basic content fetching capabilities