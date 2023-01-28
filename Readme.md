# MuPDFCore: Multiplatform .NET bindings for MuPDF

<img src="icon.svg" width="256" align="right">

[![License: AGPL v3](https://img.shields.io/badge/License-AGPL_v3-blue.svg)](https://www.gnu.org/licenses/agpl-3.0)
[![Version](https://img.shields.io/nuget/v/MuPDFCore)](https://nuget.org/packages/MuPDFCore)

__MuPDFCore__ is a set of multiplatform .NET bindings for [MuPDF](https://mupdf.com/). It can render PDF, XPS, EPUB and other formats to raster images returned either as raw bytes, or as image files in multiple formats (including PNG and PSD). It also supports multithreading.

It also includes __MuPDFCore.MuPDFRenderer__, an Avalonia control to display documents compatible with MuPDFCore in Avalonia windows (with multithreaded rendering).

The library is released under the [AGPLv3](https://www.gnu.org/licenses/agpl-3.0.html) licence.

## Getting started

The MuPDFCore library targets .NET Standard 2.0, thus it can be used in projects that target .NET Standard 2.0+, .NET Core 2.0+, .NET 5.0+, .NET Framework 4.6.1 ([note](#netFrameworkNote)) and possibly others. MuPDFCore includes a pre-compiled native library, which currently supports the following platforms:

* Windows x86 (32 bit)
* Windows x64 (64 bit)
* Windows arm64 (ARM 64 bit)
* Linux x64 (64 bit)
* Linux arm64/aarch64 (ARM 64 bit)
* macOS Intel x86_64 (64 bit)
* macOS Apple silicon (ARM 64 bit)

To use the library in your project, you should install the [MuPDFCore NuGet package](https://www.nuget.org/packages/MuPDFCore/) and/or the [MuPDFCore.PDFRenderer NuGet package](https://www.nuget.org/packages/MuPDFCore.MuPDFRenderer/). When you publish a program that uses MuPDFCore, the correct native library for the target architecture will automatically be copied to the build folder (but see the [note](#netFrameworkNote) for .NET Framework).

**Note**: you should make sure that end users on Windows install the [Microsoft Visual C++ Redistributable for Visual Studio 2015, 2017, 2019 and 2022](https://docs.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-160#visual-studio-2015-2017-2019-and-2022) for their platform, otherwise they will get an error message stating that `MuPDFWrapper.dll` could not be loaded because a module was not found.

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
3. Creating the `MuPDFCore` library NuGet package

Steps 1 and 2 need to be performed on all of Windows, macOS and Linux, and on the various possible architectures (x86, x64 and arm64 for Windows, x64/Intel and arm64/Apple for macOS, and x64 and arm64 for Linux - no cross-compiling)! Otherwise, some native assets will be missing and it will not be possible to build the NuGet package.

### 1. Building libmupdf

You can download the open-source (GNU AGPL) MuPDF source code from [here](https://mupdf.com/downloads/index.html). You will need to uncompress the source file and compile the library on Windows, macOS and Linux. You need the following files:

* From Windows (x86, x64, arm64):
    * libmupdf.lib

* From macOS (Intel - x64, Apple silicon - arm64):
    * libmupdf.a
    * libmupdf-third.a

* From Linux (x64, arm64):
    * libmupdf.a
    * libmupdf-third.a

Note that the files from macOS and Linux are different, despite sharing the same name.

For convenience, these compiled files for MuPDF 1.21.1 are included in the [`native/MuPDFWrapper/lib` folder](https://github.com/arklumpus/MuPDFCore/tree/master/native/MuPDFWrapper/lib) of this repository.

#### Tips for compiling MuPDF 1.21.1:

* On all platforms:
    * You do not need to follow the instructions in `thirdparty/tesseract.txt`, as in this version the _leptonica_ and _tesseract_ libraries are already included in the source archive.
	* Delete or comment line 316 in `source/fitz/output.c` (the `fz_throw` invocation within the `buffer_seek` method - this should leave the `buffer_seek` method empty). This line throws an exception when a seek operation on a buffer is attempted. The problem is that this makes it impossible to render a document as a PSD image in memory, because the `fz_write_pixmap_as_psd` method performs a few seek operations. By removing this line, we turn buffer seeks into no-ops; this doesn't seem to have catastrophic side-effects and the PSD documents produced in this way appear to be fine.

* On Windows (x64):
    * Open the `platform/win32/mupdf.sln` solution in Visual Studio. You should get a prompt to retarget your projects. Accept the default settings (latest Windows SDK and v143 of the tools).
    * Select the `ReleaseTesseract` configuration and `x64` architecture. Select every project in the solution except `javaviewer` and `javaviewerlib` and right-click to open the project properties. Go to `C/C++` > `Code Generation` and set the `Runtime Library` to `Multi-threaded DLL (/MD)`. Save everything (`CTRL+SHIFT+S`) and close Visual Studio.
    * Now, open the `x64 Native Tools Command Prompt for VS`, move to the folder with the solution file, and build it using `msbuild mupdf.sln`
    * Then, build again using `msbuild mupdf.sln /p:Configuration=Release`. Ignore the compilation errors.
    * Finally, build again using `msbuild mupdf.sln /p:Configuration=ReleaseTesseract`.
    * This may still show some errors, but should produce the `libmupdf.lib` file that is required in the `x64/ReleaseTesseract` folder (the file should be ~497MB in size).

* On Windows (x86):
    * You will have to use Visual Studio 2019, as Visual Studio 2022 is not supported on x86 platforms.
    * Open the `platform/win32/mupdf.sln` solution in Visual Studio and select the `ReleaseTesseract` configuration and `Win32` architecture. Select every project in the solution except `javaviewer` and `javaviewerlib` and right-click to open the project properties. Go to `C/C++` > `Code Generation` and set the `Runtime Library` to `Multi-threaded DLL (/MD)`. Save everything (`CTRL+SHIFT+S`) and close Visual Studio.
    * Now, open the `x86 Native Tools Command Prompt for VS`, move to the folder with the solution file, and build it using `msbuild mupdf.sln /p:Platform=Win32`
    * Then, build again using `msbuild mupdf.sln /p:Configuration=Release /p:Platform=Win32`. Ignore the compilation errors.
    * Finally, build again using `msbuild mupdf.sln /p:Configuration=ReleaseTesseract /p:Platform=Win32`.
    * This may still show some errors, but should produce the `libmupdf.lib` file that is required in the `ReleaseTesseract` folder (the file should be ~415MB in size).

* On Windows (arm64)
    
    This is going to be a bit more complicated, because it appears that MuPDF is not meant to be built on ARM. These instructions will assume that you are building MuPDF on an ARM machine.
    
    First of all, make sure that you have installed Visual Studio 2022 and have selected the C++ ARM64 build tools component of the "Desktop development with C++" workload.
    
    * Download and extract the MuPDF source code and follow the instructions for all platforms above.
    * Add ` || defined(_M_ARM64)` at the end of line 16 in `scripts/tesseract/endianness.h`.
    * Open the file `thirdparty/openjpeg/src/lib/openjp2/ht_dec.c` and add the following after line 57 ([source](https://github.com/ngtcp2/ngtcp2/commit/576f2d470cdf3da7b15348f4f609e4c7a0070499)):
        ```C
        unsigned int __popcnt(unsigned int x) {
            unsigned int c = 0;
            for (; x; ++c) {
                x &= x - 1;
            }
            return c;
        }
        ```
    * Now we need to edit a few files in the `thirdparty/tesseract/src/arch` folder.
        * Comment or delete lines 149-177 (inclusive) in `simddetect.cpp`. You should now have an empty block between `#  elif defined(_WIN32)` and `#else`. Also comment or delete lines 198-220 (inclusive) and 237-260 (inclusive).
        * Comment or delete lines 20-22 (inclusive) in `dotproductsse.cpp`. Replace the whole body of the `DotProductSSE` method (lines 30-76) with `return DotProductNative(u, v, n);`.
        * Comment or delete lines 20-21 (inclusive) in `dotproductavx.cpp`. Replace the whole body of the `DotProductAVX` method (lines 29-54) with `return DotProductNative(u, v, n);`.
        * Comment or delete lines 20-21 (inclusive) in `dotproductfma.cpp`. Replace the whole body of the `DotProductFMA` method (lines 29-52) with `return DotProductNative(u, v, n);`.
        * Delete the contents of `thirdparty/tesseract/src/arch/intsimdmatrixavx2.cpp` and `thirdparty/tesseract/src/arch/intsimdmatrixsse.cpp` (do not delete the files, just their contents).
        * Comment or delete lines 120-121 (inclusive) in `intsimdmatrix.h`

    * Open the `platform/win32/mupdf.sln` solution in Visual Studio. You should get a prompt to retarget your projects. Accept the default settings (latest Windows SDK and v143 of the tools).
    * In Visual Studio, click on the "Configuration Manager" item from the "Build" menu. In the new window, click on the drop down menu for the "Active solution platform" and select `<New...>`. In this new dialog, select the `ARM64` platform and choose to copy the settings from `x64`. Leave the `Create new project platforms` option enabled and click on `OK` (this may take some time).
    * Close the Configuration Manager and select the `ReleaseTesseract` configuration and `ARM64` architecture. Select every project in the solution except `javaviewer` and `javaviewerlib` and right-click to open the project properties. Go to `C/C++` > `Code Generation` and set the `Runtime Library` to `Multi-threaded DLL (/MD)`.
    * Open the properties for the `libpkcs7` project, go to `C/C++` > `Preprocessor` and remove `HAVE_LIBCRYPTO` from the `Preprocessor Definitions`. Then go to `Librarian` > `General` and remove `libcrypto.lib` from the `Additional Dependencies`.
    * Save everything (`CTRL+SHIFT+S`) and close Visual Studio.
    * Create a new folder `platform/win32/Release`. Now, the problem is that the `bin2coff` script included with MuPDF cannot create `obj` files for ARM64 (only for x86 and x64). Since I could not find a version that can do this, I [translated the source code of bin2coff to C# and added this option myself](https://github.com/arklumpus/bin2coff). You can download an ARM64 `bin2coff.exe` from [here](https://github.com/arklumpus/bin2coff/releases/latest/download/win-arm64.zip); place it in the `Release` folder that you have just created.
    * Open the `Developer Command Prompt for VS`, move to the folder with the solution file (`platform/win32`), and build it using `msbuild mupdf.sln /p:Configuration=ReleaseTesseract`. Some compilation errors may occur towards the end, but they should not matter.
    * After a while, this should produce `libmupdf.lib` in the `ARM64/ReleaseTesseract` folder (the file should be ~500MB in size).

* On Linux (x64):
    * Edit the `Makefile`, adding the `-fPIC` compiler option at the end of line 24 (which specifies the `CFLAGS`).
    * Comment line 218 in `include/mupdf/fitz/config.h`.
    * Make sure that you are using a recent enough version of GCC (version 7.3.1 seems to be enough).
    * Compile by running `USE_TESSERACT=yes make HAVE_X11=no HAVE_GLUT=no` (this builds just the command-line libraries and tools, and enables OCR through the included Tesseract library).

* On Linux (arm64):
    * Edit the `Makefile`, adding the `-fPIC` compiler option at the end of line 24 (which specifies the `CFLAGS`).
    * Delete or comment line 218 in `thirdparty/tesseract/src/arch/simddetect.cpp`.
    * Make sure that you are using a recent enough version of GCC (version 7.3.1 seems to be enough).
    * Compile by running `USE_TESSERACT=yes make HAVE_X11=no HAVE_GLUT=no` (this builds just the command-line libraries and tools, and enables OCR through the included Tesseract library).

* On macOS (Intel - x64):
    * Edit the `Makefile`, adding the `-fPIC` compiler option at the end of line 24 (which specifies the `CFLAGS`). Also add the `-std=c++11` option at the end of line 58 (which specifies the `CXX_CMD`).
    * Compile by running `USE_TESSERACT=yes make` (this enables OCR through the included Tesseract library).

* On macOS (Apple silicon - arm64)
    * Edit the `Makefile`, adding the `-fPIC` compiler options at the end of line 24 (which specifies the `CFLAGS`). Also add the `-std=c++11` option at the end of line 58 (which specifies the `CXX_CMD`).
	* Delete or comment line 218 in `thirdparty/tesseract/src/arch/simddetect.cpp`.
    * Compile by running `USE_TESSERACT=yes make` (this enables OCR through the included Tesseract library).

### 2. Building MuPDFWrapper

Once you have the required static library files, you should download the MuPDFCore source code (just clone this repository) and place the library files in the appropriate subdirectories in the `native/MuPDFWrapper/lib/` folder.

To compile `MuPDFWrapper` you will need [CMake](https://cmake.org/) (version 3.8 or higher) and (on Windows) [Ninja](https://ninja-build.org/).

On Windows, the easiest way to get all the required tools is probably to install [Visual Studio](https://visualstudio.microsoft.com/it/). By selecting the "Desktop development with C++" workload you should get everything you need.

On macOS, you will need to install at least the Command-Line Tools for Xcode (if necessary, you should be prompted to do this while you perform the following steps) and CMake.

Once you have everything at the ready, you will have to build MuPDFWrapper on the seven platforms.

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

After this finishes, you should find a file named `libMuPDFWrapper.dylib` in the `native/out/build/mac-x64/MuPDFWrapper/` directory (on macOS running on an Intel x64 processor) or in the `native/out/build/mac-arm64/MuPDFWrapper/` directory (on macOS running on an Apple silicon arm64 processor), and a file named `libMuPDFWrapper.so` in the `native/out/build/linux-x64/MuPDFWrapper/` directory (on Linux). Leave it there.

### 3. Creating the MuPDFCore NuGet package

Once you have the `MuPDFWrapper.dll`, `libMuPDFWrapper.dylib` and `libMuPDFWrapper.so` files, make sure they are in the correct folders (`native/out/build/xxx-yyy/MuPDFWrapper/`), __all on the same machine__.

To create the MuPDFCore NuGet package, you will need the [.NET Core 2.0 SDK or higher](https://dotnet.microsoft.com/download/dotnet/current) for your platform. Once you have installed it and have everything ready, open a terminal in the folder where you have downloaded the MuPDFCore source code and type:

```
cd MuPDFCore
dotnet pack -c Release
```

This will create a NuGet package in `MuPDFCore/bin/Release`. You can install this package on your projects by adding a local NuGet source.

### 4. Running tests

To verify that everything is working correctly, you should build the MuPDFCore test suite and run it on all platforms. To build the test suite, you will need the [.NET 6 SDK or higher](https://dotnet.microsoft.com/download/dotnet/current). You will also need to have enabled the [Windows Subsystem for Linux](https://docs.microsoft.com/en-us/windows/wsl/install).

To build the test suite:

1. Make sure that you have changed the version of the MuPDFCore NuGet package so that it is higher than the latest version of MuPDFCore in the NuGet repository (you should use a pre-release suffix, e.g. `1.4.0-a1` to avoid future headaches with new versions of MuPDFCore). This is set in line 9 of the `MuPDFCore/MuPDFCore.csproj` file.
2. Add the `MuPDFCore/bin/Release` folder to your local NuGet repositories (you can do this e.g. in Visual Studio).
3. If you have not done so already, create the MuPDFCore NuGet package following step 3 above.
4. Update line 56 of the `Tests/Tests.csproj` project file so that it refers to the version of the MuPDFCore package you have just created.

These steps ensure that you are testing the right version of MuPDFCore (i.e. your freshly built copy) and not something else that may have been cached.

Now, open a Windows command line in the folder where you have downloaded the MuPDFCore source code, type `BuildTests` and press `Enter`. This will create a number of files in the `Release\MuPDFCoreTests` folder, where each file is an archive containing the tests for a certain platform and architecture:

* `MuPDFCoreTests-linux-x64.tar.gz` contains the tests for Linux environments on x64 processors.
* `MuPDFCoreTests-linux-arm64.tar.gz` contains the tests for Linux environments on arm64 processors.
* `MuPDFCoreTests-mac-x64.tar.gz` contains the tests for macOS environments on Intel processors.
* `MuPDFCoreTests-mac-arm64.tar.gz` contains the tests for macOS environments on Apple silicon processors.
* `MuPDFCoreTests-win-x64.tar.gz` contains the tests for Windows environments on x64 processors.
* `MuPDFCoreTests-win-x86.tar.gz` contains the tests for Windows environments on x86 processors.

To run the tests, copy each archive to a machine running the corresponding operating system, and extract it (note: on Windows, the default zip file manager may struggle when extracting the text file with non-latin characters; you may need to manually extract this file). Then:

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

1. Manually download the NuGet package for [MuPDFCore](https://www.nuget.org/packages/MuPDFCore/) (click on the "Download package" link on the right).
2. Rename the `.nupkg` file so that it has a `.zip` extension.
3. Extract the zip file.
4. Within the extracted folder, the library files are in the `runtimes/xxx/native/` folder, where `xxx` is `linux-x64`, `linux-arm64`, `osx-x64`, `osx-arm64`, `win-x64`, `win-x86` or `win-arm64`, depending on the platform you are using.

Make sure you copy the appropriate file to the same folder as the executable!
