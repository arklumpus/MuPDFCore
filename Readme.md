# MuPDFCore: Multiplatform .NET bindings for MuPDF

<img src="icon.svg" width="256" align="right">

[![License: AGPL v3](https://img.shields.io/badge/License-AGPL_v3-blue.svg)](https://www.gnu.org/licenses/agpl-3.0)
[![Version](https://img.shields.io/nuget/v/MuPDFCore)](https://nuget.org/packages/MuPDFCore)

__MuPDFCore__ is a set of multiplatform .NET bindings for [MuPDF](https://mupdf.com/). It can render PDF, XPS, EPUB and other formats to raster images returned either as raw bytes, or as image files in multiple formats (including PNG, JPEG, and PSD). It also supports multithreading.

It also includes __MuPDFCore.MuPDFRenderer__, an Avalonia control to display documents compatible with MuPDFCore in Avalonia windows (with multithreaded rendering).

The library is released under the [AGPLv3](https://www.gnu.org/licenses/agpl-3.0.html) licence.

## Getting started

The MuPDFCore library targets .NET Standard 2.0, thus it can be used in projects that target .NET Standard 2.0+, .NET Core 2.0+, .NET 5.0+, .NET Framework 4.6.1 ([note](#netFrameworkNote)) and possibly others. MuPDFCore includes a pre-compiled native library, which currently supports the following platforms:

* Windows x86 (32 bit) `win-x86`
* Windows x64 (64 bit) `win-x64`
* Windows arm64 (ARM 64 bit) `win-arm64`
* Linux x64 (64 bit)
    * glibc-based `linux-x64`
    * musl-based `linux-musl-x64`
* Linux arm64/aarch64 (ARM 64 bit)
    * glibc-based `linux-arm64`
    * musl-based `linux-musl-arm64` (see [note](#muslNote))
* macOS Intel x86_64 (64 bit) `osx-x64`
* macOS Apple silicon (ARM 64 bit) `osx-arm64`

To use the library in your project, you should install the [MuPDFCore NuGet package](https://www.nuget.org/packages/MuPDFCore/) and/or the [MuPDFCore.PDFRenderer NuGet package](https://www.nuget.org/packages/MuPDFCore.MuPDFRenderer/). When you publish a program that uses MuPDFCore, the correct native library for the target architecture will automatically be copied to the build folder (but see the [note](#netFrameworkNote) for .NET Framework).

**Note**: you should make sure that end users on **Windows** install the [Microsoft Visual C++ Redistributable for Visual Studio 2015, 2017, 2019 and 2022](https://docs.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-160#visual-studio-2015-2017-2019-and-2022) for their platform, otherwise they will get an error message stating that `MuPDFWrapper.dll` could not be loaded because a module was not found.

<a name="muslNote"></a>**Note** for **`musl`-based Linux arm64**: I could not find a way to ensure that the linux-musl-arm64 native artifact overwrites the linux-arm64 (`glibc`) artifact. As a result, when you publish a project that uses MuPDFCore targeting linux-musl-arm64, you will find _two_ native assets in the build directory (`MuPDFWrapper.so`, which is the `musl` artifact, and `libMuPDFWrapper.so`, which is the `glibc` artifact). Everything will work fine out of the box (because the name of the `musl` artifact has higher priority), but you may want to delete `libMuPDFWrapper.so` in order to reduce size. You can use e.g. a post-build target to do this.

## Usage

### Documentation

You can find detailed descriptions of how to use MuPDFCore and some code examples in the [MuPDFCore wiki](https://github.com/arklumpus/MuPDFCore/wiki). Interactive documentation for the library API can be accessed from the [documentation website](https://arklumpus.github.io/MuPDFCore/). A [PDF reference manual](https://arklumpus.github.io/MuPDFCore/MuPDFCore.pdf) is also available.

### Minimal working example

The following example shows the bare minimum code necessary to render a page of a PDF document to a PNG image using MuPDFCore:

```CSharp
//Initialise the MuPDF context. This is needed to open or create documents.
using MuPDFContext ctx = new MuPDFContext();

//Open a PDF document
using MuPDFDocument document = new MuPDFDocument(ctx, "path/to/document.pdf");

//Page index (page 0 is the first page of the document)
int pageIndex = 0;

//Zoom level, converting document units into pixels. For a PDF document, a 1x zoom level corresponds to a
//72dpi resolution.
double zoomLevel = 1;

//Save the first page as a PNG image with transparency, at a 1x zoom level (1pt = 1px).
document.SaveImage(pageIndex, zoomLevel, PixelFormats.RGBA, "path/to/output.png", RasterOutputFileTypes.PNG);
```

Look at the [wiki](https://github.com/arklumpus/MuPDFCore/wiki) for more information.

### Examples

The [Demo](https://github.com/arklumpus/MuPDFCore/tree/master/Demo) folder in the repository contains some examples of how the library can be used to extract pages from a PDF or XPS document, render them to a raster image, or combine them in a new document

The [PDFViewerDemo](https://github.com/arklumpus/MuPDFCore/tree/master/PDFViewerDemo) folder contains a complete (though minimal) example of a PDF viewer program built around the `MuPDFCore.MuPDFRenderer.PDFRenderer` control.

Note that these examples intentionally avoid any error handling code: in a production setting, you should typically make sure that calls to MuPDFCore library functions are within a `try...catch` block to handle any resulting `MuPDFException`s.

## Building from source

Building the MuPDFCore library from source requires the following steps:

1. Building the `libmupdf` native library
2. Building the `MuPDFWrapper` native library
3. Creating the `MuPDFCore.NativeAssets.xxx-yyy` native assets NuGet packages
4. Creating the `MuPDFCore` library NuGet package

Starting from MuPDFCore 1.8.0, the native assets are split into their own NuGet packages, on which the main MuPDFCore package depends. Aside from reducing the size of individual packages, this means that if you are making changes that do not affect the native assets, you can skip steps 1-3 and go straight to step 4.

Steps 1 and 2 need to be performed on all of Windows, macOS and Linux, and on the various possible architectures (x86, x64 and arm64 for Windows, x64/Intel and arm64/Apple for macOS, and x64 and arm64 for Linux, both glibc and musl - no cross-compiling)! Otherwise, some native assets will be missing and it will not be possible to build the NuGet packages in step 3.

### 1. Building libmupdf

Starting from version 2.0.0, MuPDFCore is based on a fork of [MuPDF](https://mupdf.com), providing some bugfixes/new features and simplifying the build process.

You can access this fork (currently based on MuPDF 1.25.2) from [arklumpus/mupdf](https://github.com/arklumpus/mupdf). You will need to follow the instructions in that repository to compile the library on Windows, macOS and Linux. You need the following files:

* From Windows (x86, x64, arm64):
    * libmupdf.lib

* From macOS (Intel - x64, Apple silicon - arm64):
    * libmupdf.a
    * libmupdf-third.a

* From Linux (x64, arm64):
    * libmupdf.a
    * libmupdf-third.a

Note that the files from macOS and Linux are different, despite sharing the same name.

For convenience, these compiled files are included in the [`native/MuPDFWrapper/lib` folder](https://github.com/arklumpus/MuPDFCore/tree/master/native/MuPDFWrapper/lib) of this repository.

### 2. Building MuPDFWrapper

Once you have the required static library files, you should download the MuPDFCore source code (just clone this repository) and place the library files in the appropriate subdirectories in the `native/MuPDFWrapper/lib/` folder (for Linux x64, copy the library built against `glibc` to the `linux-x64` folder, and the library built against `musl` to the `linux-musl-x64` folder, and do the same for Linux arm64).

To compile `MuPDFWrapper` you will need [CMake](https://cmake.org/) (version 3.8 or higher) and (on Windows) [Ninja](https://ninja-build.org/).

On Windows, the easiest way to get all the required tools is probably to install [Visual Studio](https://visualstudio.microsoft.com/it/). By selecting the "Desktop development with C++" workload you should get everything you need.

On macOS, you will need to install at least the Command-Line Tools for Xcode (if necessary, you should be prompted to do this while you perform the following steps) and CMake.

Once you have everything at the ready, you will have to build MuPDFWrapper on the nine platforms.

<details>
<summary><strong>Build instructions</strong></summary>

#### Windows (x86 and x64)

1. <p>Assuming you have installed Visual Studio, you should open the "<strong>x64</strong> Native Tools Command Prompt for VS" or the "<strong>x86</strong> Native Tools Command Prompt for VS" (you should be able to find these in the Start menu). Take care to open the version corresponding to the architecture you are building for, otherwise you will not be able to compile the library. A normal command prompt will not work, either.</p>
    <p><strong>Note 1</strong>: you <strong>must</strong> build the library on two separate systems, one running a 32-bit version of Windows and the other running a 64-bit version. If you try to build the x86 library on an x64 system, the system will probably build a 64-bit library and place it in the 32-bit output folder, which will just make things very confusing.</p>
    <p><strong>Note 2 for Windows x86</strong>: for some reason, Visual Studio might install the 64-bit version of CMake and Ninja, even though you are on a 32-bit machine. If this happens, you will have to manually install the 32-bit CMake and compile a 32-bit version of Ninja. You will notice if this is an issue because the 64-bit programs will refuse to run.</p>
2. `CD` to the directory where you have downloaded the MuPDFCore source code.
3. `CD` into the `native` directory.
4. Type `build`. This will start the `build.cmd` batch script that will delete any previous build and compile the library.

After this finishes, you should find a file named `MuPDFWrapper.dll` in the `native/out/build/win-x64/MuPDFWrapper/` directory or in the `native/out/build/win-x86/MuPDFWrapper/` directory. Leave it there.

#### Windows (arm64)

1. Locate the batch file that sets up the developer command prompt environment. You can do this by finding the "Developer Command Prompt for VS" link in the start menu, then clicking on `Open file location`, opening the properties of the link and looking at the `Target`. This could be e.g. `C:\Program Files\Microsoft Visual Studio\2022\Preview\Common7\Tools\VsDevCmd.bat`.
2. Open a normal command prompt and invoke the batch script with the `-arch=arm64 -host_arch=x86` arguments (add quotes if there are spaces in the path to the batch script), e.g.:
    ```
    "C:\Program Files\Microsoft Visual Studio\2022\Preview\Common7\Tools\VsDevCmd.bat" -arch=arm64 -host_arch=x86
    ```
3. `CD` to the directory where you have downloaded the MuPDFCore source code.
4. `CD` into the `native` directory.
5. Type `build`. This will start the `build.cmd` batch script that will delete any previous build and compile the library.

After this finishes, you should find a file named `MuPDFWrapper.dll` in the `native/out/build/win-arm64/MuPDFWrapper/` directory. Leave it there.

#### macOS and Linux

1. Assuming you have everything ready, open a terminal in the folder where you have downloaded the MuPDFCore source code.
2. `cd` into the `native` directory.
3. Type `chmod +x build.sh`.
4. Type `./build.sh`. This will delete any previous build and compile the library.

After this finishes, you should find a file named `libMuPDFWrapper.dylib` in the `native/out/build/mac-x64/MuPDFWrapper/` directory (on macOS running on an Intel x64 processor) or in the `native/out/build/mac-arm64/MuPDFWrapper/` directory (on macOS running on an Apple silicon arm64 processor), and a file named `libMuPDFWrapper.so` in the `native/out/build/linux-XXX/MuPDFWrapper/` directory (on Linux - where `XXX` can be `x64`, `arm64`, `musl-x64`, or `musl-arm64`). Leave it there.

</details>

### 3. Creating the native assets MuPDFCore NuGet packages

Once you have the `MuPDFWrapper.dll` (3x), `libMuPDFWrapper.dylib` (2x) and `libMuPDFWrapper.so` (4x) files, make sure they are in the correct folders (`native/out/build/xxx-yyy/MuPDFWrapper/`), __all on the same machine__.

To create the native assets NuGet packages, you will need the [.NET Core 2.0 SDK or higher](https://dotnet.microsoft.com/download/dotnet/current) for your platform. Once you have installed it and have everything ready, open a terminal in the folder where you have downloaded the MuPDFCore source code and type:

```
BuildNativeAssets
```

This will create the NuGet packages in the `MuPDFCore.NativeAssets/NuGetPackages` folder. Once the script finishes, this folder should contain 9 files. Make sure you add this folder as a local NuGet source.

### 4. Creating the MuPDFCore NuGet package

If you have made updates to the native assets, make sure to use the appropriate version numbers in `MuPDFCore/MuPDFCore.csproj`. Then, to create the main MuPDFCore NuGet package, open a terminal in the folder where you have downloaded the MuPDFCore source code and type:

```
cd MuPDFCore
dotnet pack -c Release
```

This will create a NuGet package in `MuPDFCore/bin/Release`. You can install this package on your projects by adding a local NuGet source.

### 5. Running tests

To verify that everything is working correctly, you should build the MuPDFCore test suite and run it on all platforms. To build the test suite, you will need the [.NET 7 SDK or higher](https://dotnet.microsoft.com/download/dotnet/current). You will also need to have enabled the [Windows Subsystem for Linux (WSL)](https://docs.microsoft.com/en-us/windows/wsl/install).

To build the test suite:

1. Make sure that you have changed the version of the MuPDFCore NuGet package so that it is higher than the latest version of MuPDFCore in the NuGet repository (you should use a pre-release suffix, e.g. `1.4.0-a1` to avoid future headaches with new versions of MuPDFCore). This is set in line 9 of the `MuPDFCore/MuPDFCore.csproj` file.
2. Add the `MuPDFCore/bin/Release` folder to your local NuGet repositories (you can do this e.g. in Visual Studio).
3. If you have not done so already, create the MuPDFCore NuGet package following step 4 above.
4. Update line 76 of the `Tests/Tests.csproj` project file so that it refers to the version of the MuPDFCore package you have just created.

These steps ensure that you are testing the right version of MuPDFCore (i.e. your freshly built copy) and not something else that may have been cached.

Now, open a Windows command line in the folder where you have downloaded the MuPDFCore source code, type `BuildTests` and press `Enter`. This will create a number of files in the `Release\MuPDFCoreTests` folder, where each file is an archive containing the tests for a certain platform and architecture:

* `MuPDFCoreTests-linux-x64.tar.gz` contains the tests for Linux environments using `glibc` on x64 processors.
* `MuPDFCoreTests-linux-arm64.tar.gz` contains the tests for Linux environments using `glibc` on arm64 processors.
* `MuPDFCoreTests-linux-musl-x64.tar.gz` contains the tests for Linux environments using `musl` on x64 processors.
* `MuPDFCoreTests-linux-musl-arm64.tar.gz` contains the tests for Linux environments using `musl` on arm64 processors.
* `MuPDFCoreTests-mac-x64.tar.gz` contains the tests for macOS environments on Intel processors.
* `MuPDFCoreTests-mac-arm64.tar.gz` contains the tests for macOS environments on Apple silicon processors.
* `MuPDFCoreTests-win-x64.tar.gz` contains the tests for Windows environments on x64 processors.
* `MuPDFCoreTests-win-x86.tar.gz` contains the tests for Windows environments on x86 processors.
* `MuPDFCoreTests-win-arm64.tar.gz` contains the tests for Windows environments on arm64 processors.

To run the tests, copy each archive to a machine running the corresponding operating system, and extract it (note: on Windows, the default zip file manager may struggle when extracting the text file with non-latin characters; you may need to manually extract this file, use [7-Zip](https://www.7-zip.org/), or use the `unzip` command from within the WSL). Then:

#### Windows
* Open a command prompt and `CD` into the folder where you have extracted the contents of the test archive.
* Enter the command `MuPDFCoreTestHost` (this will run the test program).

#### macOS and Linux
* Open a terminal and `cd` into the folder where you have extracted the contents of the test archive.
* Enter the command `chmod +x MuPDFCoreTestHost` (this will add the executable flag to the test program).
* Enter the command `./MuPDFCoreTestHost` (this will run the test program).
* On macOS, depending on your security settings, you may get a message saying `zsh: killed` when you try to run the program. To address this, you need to sign the executable, e.g. by running `codesign --timestamp --sign <certificate> MuPDFCoreTestHost`, where `<certificate>` is the name of a code signing certificate in your keychain (e.g. `Developer ID Application: John Smith`). After this, you can try again to run the test program with `./MuPDFCoreTestHost`.

The test suite will start; it will print the name of each test, followed by a green `  Succeeded  ` or a red `  Failed  ` depending on the test result. If everything went correctly, all tests should succeed.

When all the tests have been run, the program will print a summary showing how many tests have succeeded (if any) and how many have failed (if any). If any tests have failed, a list of these will be printed, and then they will be run again one at a time, waiting for a key press before running each test (this makes it easier to follow what is going on). If you wish to kill the test process early, you can do so with `CTRL+C`.

## Note about MuPDFCore and .NET Framework <a name="netFrameworkNote"></a>

If you wish to use MuPDFCore in a .NET Framework project, you will need to manually copy the native MuPDFWrapper library for the platform you are using to the executable directory (this is done automatically if you target .NET/.NET core).

One way to obtain the appropriate library files is:

1. Manually download the appropriate native assets NuGet package from the table below. Note that AnyCPU builds on Windows need the `win-x86` native asset.
2. Rename the `.nupkg` file so that it has a `.zip` extension.
3. Extract the zip file.
4. Within the extracted folder, the library files are in the `runtimes/xxx/native/` folder, where `xxx` is `linux-x64`, `linux-arm64`, `linux-musl-x64`, `linux-musl-arm64`, `osx-x64`, `osx-arm64`, `win-x64`, `win-x86` or `win-arm64`, depending on the platform you are using.
5. The file you need to copy should be called `MuPDFWrapper.dll` on Windows, `libMuPDFWrapper.so` or `MuPDFWrapper.so` on Linux, and `libMuPDFWrapper.dylib` on macOS.

Make sure you copy the appropriate file to the same folder as the executable!

<table align="center">
    <thead>
        <tr>
            <td><strong>OS</strong></td>
            <td colspan=2><strong>Platform</strong></td>
            <td><strong>NuGet package</strong></td>
        </tr>
    </thead>
    <tbody>
        <tr>
            <td rowspan=3>Windows</td>
            <td colspan=2>x86</td>
            <td><a href="https://www.nuget.org/api/v2/package/MuPDFCore.NativeAssets.Win-x86/">win-x86</a></td>
        </tr>
        <tr>
            <td colspan=2>x64</td>
            <td><a href="https://www.nuget.org/api/v2/package/MuPDFCore.NativeAssets.Win-x64/">win-x64</a></td>
        </tr>
        <tr>
            <td colspan=2>arm64</td>
            <td><a href="https://www.nuget.org/api/v2/package/MuPDFCore.NativeAssets.Win-arm64/">win-arm64</a></td>
        </tr>
        <tr>
            <td rowspan=4>Linux</td>
            <td rowspan=2>x64</td>
            <td>glibc</td>
            <td><a href="https://www.nuget.org/api/v2/package/MuPDFCore.NativeAssets.Linux-x64/">linux-x64</a></td>
        </tr>
        <tr>
            <td>musl</td>
            <td><a href="https://www.nuget.org/api/v2/package/MuPDFCore.NativeAssets.Linux-musl-x64/">linux-musl-x64</a></td>
        </tr>
        <tr>
            <td rowspan=2>arm64</td>
            <td>glibc</td>
            <td><a href="https://www.nuget.org/api/v2/package/MuPDFCore.NativeAssets.Linux-arm64/">linux-arm64</a></td>
        </tr>
        <tr>
            <td>musl</td>
            <td><a href="https://www.nuget.org/api/v2/package/MuPDFCore.NativeAssets.Linux-musl-arm64/">linux-musl-arm64</a></td>
        </tr>
        <tr>
            <td rowspan=2>macOS</td>
            <td colspan=2>x64 (Intel)</td>
            <td><a href="https://www.nuget.org/api/v2/package/MuPDFCore.NativeAssets.Mac-x64/">osx-x64</a></td>
        </tr>
        <tr>
            <td colspan=2>arm64 (Apple Silicon)</td>
            <td><a href="https://www.nuget.org/api/v2/package/MuPDFCore.NativeAssets.Mac-arm64/">osx-arm64</a></td>
        </tr>
    </tbody>
</table>