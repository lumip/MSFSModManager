# Mod Manager for Microsoft Flight Simulator (2020)

This project aims to provide an easy and unified way of installing and updating community mods for MS Flight Simulator from various sources.

## Current state:

    - Command line interface only (a graphical user interface will be added later)
    - Lists all installed mods/packages
    - Installing from GitHub releases or branches possible
    - Checks and downloads dependencies for packages during installation (if a package source for the dependency is known, installation will abort if dependencies cannot be satisfied)
    - Uninstalling packages possible

### Roadmap

    - Adding GUI application for easier usability
    - Supporting popular mod database websites as package sources

### Known issues / missing features

    - dependency resolution only considers packages to be installed and does not check whether updating dependencies might break installed packages

## CLI Usage

The command line interface executable is built from the `MSFSModManager.CLI` project as `fsmodm`.  It currently supports the following commands

### Listing installed packages

```
fsmodm list [--includeOfficial] [--filterType <PackageType>]
```

Lists all currently installed community packages. Optional arguments are:
- `includeOfficial`: List also official asobo packages and their version numbers.
- `filterType`: Show only packages of the given type.

### Add/Remove installation source for a package

```
fsmodm add-source <PackageId> <SourceURL> [<SourceOptions>]
fsmodm remove-source <PackageId>
```

Adds/removes a source from which packages can be installed. `PackageId` is a user-chosen identifier for the package (and will be used as the folder name for the package in the Community mod folder of MSFS). `SourceURL` specifies the URL from which the package (and package metadata) are obtained and `SourceOptions` are additional arguments depending on the source type.

Current source types are:

#### Github releases

Obtains packages from the 'Releases' page of a Github repository. In this case, `SourceURL` is `https://github.com/<Author>/<Repository>`.
As an optional `SourceOption` the user can supply a regular expression that is used to select the correct archive file to download from the release.

Example: `fsmodm add-source B78XH https://github.com/Heavy-Division/B78XH B78XH-v.*\\d-wo.zip` registers a source for releases of the [B78XH mod](https://github.com/Heavy-Division/B78XH/) but in the variant without optional content.

#### Github branches

Obtains packages from a development branch of a Github repository. In this case, `SourceUrl` is `https://github.com/<Author>/<Repository>@<BranchName>`.
This source type does not have any `SourceOption`s.

Example: `fsmodm add-source B78XH https://github.com/Heavy-Division/B78XH@main` registers a source for the latest development state in the `main` branch of the B78XH mod.

**Note**: Installing from github branches only works if the branch contains a correct package manifest file (`manifest.json`) and will install only the contents of the folder containing the manifest file (including all subdirectories).

#### Local ZIP file

Uses a local ZIP file of a package as a package installation source. In this case, `SourceURL` is a local file path.

Example: `fsmodm add-source B78XH Downloads\B78XH-v0.1.12.zip`.

**Note**: This will automatically locate the relative path of the `manifest.json` file within the archive and install
all files under that path during package installation, i.e., it will ignore all files in the archive that are not under
the same directory as the manifest file and remove path prefixes.

### Install/Uninstall a package

```
fsmodm install <PackageId>
fsmodm uninstall <PackageId>
```

Installs/removes a package (mod). This requires an installation source to have been added for the package identified by `PackageId`.
Installation will always select to last compatible available version of the package. If the package is already installed,
it will be replaced if a newer version is found.

### Install package without storing package source

```
fsmodm install <PackageId> <SourceURL> [<SourceOptions>]
```

Installs a package (mod) from the given installation source without persisting information about the source.

### Update all packages

```
fsmodm update
```

Updates all packages for which installation sources have been added to the latest compatible available version.

### List available package versions

```
fsmodm show-available <PackageId>
```

Shows all versions that are available for the package `PackageId`. This requires an installation source to have been added for the package.

### Export list of installed packages and sources

```
fsmodm export [--onlyWithSource] [--ignoreVersion]
```

Prints information on all installed community packages in JSON format, including the package id and version as well as details on the package source, if one was added for a package. Optional arguments are:
- `onlyWithSource`: Print only information for packages for which a package source was added.
- `ignoreVersion`: Print only package names and source information but skip version information.
