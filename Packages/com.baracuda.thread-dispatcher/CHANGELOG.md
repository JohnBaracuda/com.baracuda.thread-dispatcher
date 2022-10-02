# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [3.1.0]
### Changed
- Created a unity Package and moved plugin contents. 

## [3.0.1]
### Added
- Added additional xml documentation.
- Created two standalone DLLs for net472 and netstandart2.0
- Added optional source code in a compressed file.

### Fixed
- Fixed a IL2CPP build issue caused by a incompatible DLL file.

## [3.0.0]
### Changed
- Code is now contained in a dll and no longer pure source code.
- Source code is still included in a .zip archive.
- Update cycles are no longer controller via preprocessor defies but by new Properties.

### Removed
- Removed obsolete code.
- Removed bloated features for simplicity.
- Removed Tick & Post update cycles.
- Removed experimental features MEC & ValueTask API.
- Removed script order settings.

## [2.0.1]
### Fixed
- Fixed an issue that prevented the cancellation of the dispatchers RuntimeToken when exiting playmode.

## [2.0.0]
### Added
- Added functionality to dispatch and await the execution of a Task. More
- Added functionality to dispatch and await the execution of a  Task<TResult>. More
- Added functionality to await the Completion of a dispatched Coroutine. More
- Added exception sensitive Coroutines.
- MEC Coroutines can now be disptached. This feature requires some manual setup and its API must be unlocked unsing the #define EXPERIMENTAL_ENABLE_MEC. More

### Changed
- Changed API to dispatch and await the start of a Coroutine, making old API obsolete.
- Updated online and offline documentation.
- Updated demo scene & examples.
- The dispatcher scene object is no longer initialized on load, but instead when needed.
