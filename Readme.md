# MuPDFCore: Multiplatform .NET Core bindings for MuPDF

<img src="icon.svg" width="256" align="right">

__MuPDFCore__ is a set of multiplatform .NET Core bindings for [MuPDF](https://mupdf.com/). It can render PDF, XPS, EPUB and other formats to raster images returned either as raw bytes, or as image files in multiple formats (including PNG and PSD). It also supports multithreading.

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
* macOS Apple silicon (ARM 64 bit, without support for the OCR functions)

To use the library in your project, you should install the [MuPDFCore NuGet package](https://www.nuget.org/packages/MuPDFCore/) and/or the [MuPDFCore.PDFRenderer NuGet package](https://www.nuget.org/packages/MuPDFCore.MuPDFRenderer/). When you publish a program that uses MuPDFCore, the correct native library for the target architecture will automatically be copied to the build folder (but see the [note](#netFrameworkNote) for .NET Framework).

**Note**: you should make sure that end users on Windows install the [Microsoft Visual C++ Redistributable for Visual Studio 2015, 2017, 2019 and 2022](https://docs.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist?view=msvc-160#visual-studio-2015-2017-2019-and-2022) for their platform, otherwise they will get an error message stating that `MuPDFWrapper.dll` could not be loaded because a module was not found.

## Usage

### Documentation

Interactive documentation for the library can be accessed from the [documentation website](https://arklumpus.github.io/MuPDFCore/). A [PDF reference manual](https://arklumpus.github.io/MuPDFCore/MuPDFCore.pdf) is also available.

### Examples

The [Demo](https://github.com/arklumpus/MuPDFCore/tree/master/Demo) folder in the repository contains some examples of how the library can be used to extract pages from a PDF or XPS document, render them to a raster image, or combine them in a new document

The [PDFViewerDemo](https://github.com/arklumpus/MuPDFCore/tree/master/PDFViewerDemo) folder contains a complete (though minimal) example of a PDF viewer program built around the `MuPDFCore.MuPDFRenderer.PDFRenderer` control.

Note that these examples intentionally avoid any error handling code: in a production setting, you should typically make sure that calls to MuPDFCore library functions are within a `try...catch` block to handle any resulting `MuPDFException`s.

### MuPDFCore library

The first step when using MuPDFCore is to create a `MuPDFCore.MuPDFContext` object that is used internally by the MuPDF library to store various things:

```Csharp
    MuPDFContext context = new MuPDFContext();
```

This object is `IDisposable`, therefore you should always call the `Dispose()` method on it once you are done with it (or, better yet, wrap it in a `using` directive). In most instances, you will only need one instance of `MuPDFContext` for your whole application.

Amongst other things, MuPDF uses this context to store a cache of "assets" (e.g. images or fonts) that have been used while rendering documents and that may be needed in future. This requires some memory: by default, the maximum size of this cache store is 256MB; however, if you want to restrict how much memory can be used, you can alter this by providing a `long` value to constructor, indicating the size in bites for the store. A value of `0` means that the store can grow up to an unlimited size. Furthermore, you can clear the cache completely by using the `MuPDFContext.ClearCache` method, or partially by using the `MuPDFContext.ShrinkCache` method.

Once you have obtained a `MuPDFContext`, you can use it to open a `MuPDFDocument`. A document can be opened from a file on disk:

```Csharp
    MuPDFDocument document = new MuPDFDocument(context, "path/to/file");
```

Or from a `byte[]` array (in this case, you will have to specify the format of the document):

```Csharp
    byte[] data;

    ...

    MuPDFDocument document = new MuPDFDocument(context, data, InputFileTypes.PDF);
```

Or from a `MemoryStream` (in this case too, you will have to specify the format of the document):

```Csharp
    MemoryStream stream;
    
    ...
    
    MuPDFDocument document = new MuPDFDocument(context, ref stream, InputFileTypes.PDF);
```

The `MemoryStream` is passed with the `ref` keyword to indicate that the `MuPDFDocument` will take care of appropriately disposing it once it finishes using it.

A `MuPDFDocument` is also `IDisposable` and should be properly disposed of to avoid memory leaks.

__Important note__: the constructor taking a `byte[]` and the one taking a `MemoryStream` will not copy the data bytes before sending them to the native MuPDF library functions. Rather, they will _pin them in place_. This is a __bad thing__ because it will mess up with the Garbage Collector's management of memory. Therefore, this is only suitable for short-lived objects. If you need to initialise a long-lived document object from memory, you should first copy the data to unmanaged memory and then use one of the constructors that take an `IntPtr` parameter, e.g.:

```Csharp
    byte[] data;

    ...
    
    //Allocate enough unmanaged memory
    IntPtr ptr = Marshal.AllocHGlobal(data.Length);
    
    //Copy the byte array to unmanaged memory
    Marshal.Copy(data, 0, ptr, data.Length);

    //Wrap the pointer in an IDisposable
    IDisposable dispIntPtr = new DisposableIntPtr(ptr);

    //Create the document
    MuPDFDocument document = new MuPDFDocument(ctx, ptr, data.Length, InputFileTypes.PDF, ref dispIntPtr);

```

The `DisposableIntPtr` class is a wrapper around a pointer that calls `Marshal.FreeHGlobal` on it once it is disposed. Passing it as the final optional parameter of `MuPDFDocument` constructor (again by reference, to indicate that the document takes ownership of the object) makes sure that the memory is properly freed once the document is disposed.

After having obtained a document, you can do many things with it: for example, you can render a page and save the results to a file on disk, or you can collect multiple pages and combine them in a new document. Code to do this can be found in the [`Program.cs`](https://github.com/arklumpus/MuPDFCore/blob/master/Demo/Program.cs) file of the Demo project.

Furthermore, you can render a page directly to memory:

```Csharp
    byte[] pixelData = document.Render(0, 1, PixelFormats.RGBA);
```

This method renders page 0 (i.e. the first page of the document) at a 1x resolution (1pt in the document is equivalent to 1px in the image), preserving alpha (transparency) information, and returns the image as an array of the bytes that constitute the pixel data (four bytes per pixel). A variation of this method allows you to supply a rectangular region of the page that you would like to render, rather than the whole page.

Alternatively, if you already know where the image data should be put (e.g. because you are using some kind of graphics library that lets you manipulate the pixel data of its images), you can use the methods that take an `IntPtr` destination:

```Csharp
    IntPtr destination;

    ...

    document.Render(0, 1, PixelFormats.RGBA, destination);
```

In this case, __you have to make sure that there is enough memory to hold the resulting image__! Otherwise, an `AccessViolationException` will occur and your program will usually fail catastrophically. Since it may sometimes be hard to determine how much memory a particular image will need (especially because of subtle differences in the rounding routines, which can cause images to be 1px larger or shorter than expected), the `GetRenderedSize` method is provided, which returns the number of bytes that will be needed to render a certain page. For example:

```Csharp
    //Get the number of bytes that will be necessary to hold the rendered page at the given resolution.
    int sizeInBytes = document.GetRenderedSize(0, 1, PixelFormats.RGBA);

    //Allocate an appropriate amount of memory.
    IntPtr destination = Marshal.AllocHGlobal(sizeInBytes);

    //Again, we use a DisposableIntPtr to make sure that we are freeing the memory when we are done with it.
    using (DisposableIntPtr holder = new DisposableIntPtr(destination))
    {
        //Make sure that all the parameters match those of the call to GetRenderedSize, or the size of the
        //resulting image may be different than expected! Even a translation of 1px could have catastrophic
        //consequences.
        document.Render(0, 1, PixelFormats.RGBA, destination);
    }

```

Finally, __none of these methods are inherently thread-safe__! E.g. you cannot render multiple pages of the same document (nor multiple regions of a single page) by simply performing multiple calls to `MuPDFDocument.Render` in parallel. For multi-threaded operation, you must instead use a `MuPDFMultiThreadedPageRender`. You can obtain one from a document:

```Csharp
    MuPDFMultiThreadedPageRenderer renderer = document.GetMultiThreadedRenderer(0, 2);
```

This method obtains an object that can be used to render the first page of the document using two threads. By using the `Render` method of this object, the page can be rendered. The page will be rendered to a number of separate tiles equal to the number of threads, which will then be your responsibility to appropriately "stitch up" (e.g. if you want to display them on screen, you could just place them appropriately). The size of each tile (and the position it should occupy) can be computed by using the `Split` method of the `RoundedSize` struct.

Furthermore, multiple `MuPDFMultiThreadedPageRenderer`s can be used in parallel, which makes it possible e.g. to render every page in the document at the same time (while also using multiple threads to render each page). The following example will render all the pages in a document at the same time in RGBA format at a 1.5x zoom, using 2 threads for each page:

```Csharp
    //Create a MuPDFContext with a using statement, so that it gets disposed at the right time.
    using MuPDFContext context = new MuPDFContext();

    //Open the document also with a using statement.
    using MuPDFDocument document = new MuPDFDocument(context, "path/to/file.pdf");

    //Create arrays to hold the objects for the various pages

    //Renderers: one per page
    MuPDFMultiThreadedPageRenderer[] renderers = new MuPDFMultiThreadedPageRenderer[document.Pages.Count];

    //Page size: one per page
    RoundedSize[] renderedPageSizes = new RoundedSize[document.Pages.Count];

    //Boundaries of the tiles that make up each page: one array per page, with one element per thread
    RoundedRectangle[][] tileBounds = new RoundedRectangle[document.Pages.Count][];

    //Addresses of the memory areas where the image data of the tiles will be stored: one array per page, with one element per thread
    IntPtr[][] destinations = new IntPtr[document.Pages.Count][];

    //Cycle through the pages in the document to initialise everything
    for (int i = 0; i < document.Pages.Count; i++)
    {
        //Initialise the renderer for the current page, using two threads (total number of threads: number of pages x 2
        renderers[i] = document.GetMultiThreadedRenderer(i, 2);

        //Determine the boundaries of the page when it is rendered with a 1.5x zoom factor
        RoundedRectangle roundedBounds = document.Pages[i].Bounds.Round(1.5);
        renderedPageSizes[i] = new RoundedSize(roundedBounds.Width, roundedBounds.Height);

        //Determine the boundaries of each tile by splitting the total size of the page by the number of threads.
        tileBounds[i] = renderedPageSizes[i].Split(renderers[i].ThreadCount);

        destinations[i] = new IntPtr[renderers[i].ThreadCount];
        for (int j = 0; j < renderers[i].ThreadCount; j++)
        {
            //Allocate the required memory for the j-th tile of the i-th page.
            //Since we will be rendering with a 24-bit-per-pixel format, the required memory in bytes is height x width x 3.
            destinations[i][j] = Marshal.AllocHGlobal(tileBounds[i][j].Height * tileBounds[i][j].Width * 3);
        }
    }

    //Start the actual rendering operations in parallel.
    Parallel.For(0, document.Pages.Count, i =>
    {
        renderers[i].Render(renderedPageSizes[i], document.Pages[i].Bounds, destinations[i], PixelFormats.RGB);
    });


    //The code in this for-loop is not really part of MuPDFCore - it just shows an example of using VectSharp to "stitch" the tiles up and produce the full image.
    for (int i = 0; i < document.Pages.Count; i++)
    {
        //Create a new (empty) image to hold the whole page.
        VectSharp.Page renderedPage = new VectSharp.Page(renderedPageSizes[i].Width, renderedPageSizes[i].Height);

        //Draw each tile onto the image.
        for (int j = 0; j < renderers[i].ThreadCount; j++)
        {
            //Create a raster image object containing the pixel data. Yay, we do not need to copy/marshal anything!
            VectSharp.RasterImage tile = new VectSharp.RasterImage(destinations[i][j], tileBounds[i][j].Width, tileBounds[i][j].Height, false, false);

            //Draw the tile on the main image page.
            renderedPage.Graphics.DrawRasterImage(tileBounds[i][j].X0, tileBounds[i][j].Y0, tile);
        }

        //Save the full page as a PNG image.
        renderedPage.SaveAsPNG("page" + i.ToString() + ".png");
    }

    //Clean-up code.
    for (int i = 0; i < document.Pages.Count; i++)
    {
        //Release the allocated memory.
        for (int j = 0; j < renderers[i].ThreadCount; j++)
        {
            Marshal.FreeHGlobal(destinations[i][j]);
        }

        //Release the renderer (if you skip this, the quiescent renderer's threads will not be stopped, and your application will never exit!
        renderers[i].Dispose();
    }
```

### Structured text representation

The `GetStructuredTextPage` method of the `MuPDFDocument` class makes it possible to obtain a "structured text" representation of each page of the document. This consists of a `MuPDFStructuredTextPage` object, which is a collection of 0 or more `MuPDFStructuredTextBlock`s.

Each `MuPDFStructuredTextBlock` either represents an image or a block of text, typically a paragraph (though there is no guarantee that this is the case). `MuPDFStructuredTextBlock`s are themselves collections of `MuPDFStructuredTextLine`s, and each line is a collection of `MuPDFStructuredTextCharacter`s (in the case of a block representing an image, it will contain a single line with a single character).

`MuPDFStructuredTextBlock`s and `MuPDFStructuredTextLine`s have a `BoundingBox` property that defines a rectangle (in page units) that bounds the contents of the block/line in the page. Similarly, `MuPDFStructuredTextCharacter`s have a `BoundingQuad` (rather than being a `Rectangle`, this is a `Quad`, i.e. a quadrilater defined by its four vertices, which may or may not be a rectangle). These can be used e.g. to highlight regions of text in the page.

The `MuPDFStructuredTextPage` also has methods to determine which character contains or is closest to a specified point (useful, for example, to determine on which character the user clicked), to obtain a list of shapes that encompass a specified range of text, and to perform text searches using regular expressions.

The order of the blocks in the page (which affects the definition of a "range" of text and search operations) is the same as returned by the underlying MuPDF library, which is taken from the order the text is drawn in the source file, so may not be accurate. They can be reordered using the `Array.Sort` method on the `StructuredTextBlocks` array contained in the `MuPDFStructuredTextPage` (lines within blocks and characters within lines can be likewise reordered).

### Optical Character Recognition (OCR) using Tesseract

MuPDF 1.18+ (embedded in MuPDFCore 1.3.0+) adds support for OCR using the [Tesseract](https://github.com/tesseract-ocr/tesseract) library. To access this feature in MuPDFCore, you can use one of the overloads of `GetStructuredTextPage` that takes a `TesseractLanguage` argument specifying the language to use for the OCR. This will run the OCR and return a `MuPDFStructuredTextPage` containing the character information obtained by Tesseract, which can be used normally. Depending on the model being used, the OCR step can take a relatively long time; therefore, the `MuPDFDocument` class also implements a `GetStructuredTextPageAsync` method, which does the same thing in an asynchronous way.

Objects of the `TesseractLanguage` class contain information used to locate the trained language model file that is used by Tesseract. Normally, when using Tesseract, you would have to ensure that the trained language model files are available on the user's computer; however, this class implements some "clever" logic to download the necessary files on demand.

In general, MuPDF provides Tesseract with a "language name" (e.g. `"eng"`). Tesseract then looks for a file called `eng.traineddata` either in the folder specified by the `TESSDATA_PREFIX` environment variable, or, if the variable is not defined, in a subfolder of the current working directory called `tessdata`. MuPDFCore manipulates the value of `TESSDATA_PREFIX` (at the process level) and the language name in order to specify the language file.

The `TesseractLanguage` class has multiple constructors:

* `TesseractLanguage(string prefix, string language)`: this constructor is used to directly specify the value of `TESSDATA_PREFIX` and the language name. The library does not process these in any way. If `prefix` is `null`, the value of `TESSDATA_PREFIX` is not changed, and Tesseract uses the system value.

* `TesseractLanguage(string fileName)`: with this constructor, you can directly specify the path to a trained language model file. You can obtain such a file from [the tessdata_fast repository](https://github.com/tesseract-ocr/tessdata_fast) or from [the tessdata_best repository](https://github.com/tesseract-ocr/tessdata_best). If the file does not have a `.traineddata` extension, it will be copied in a temporary location.

* `TesseractLanguage(Fast language, bool useAnyCached = false)` \
    `TesseractLanguage(FastScript language, bool useAnyCached = false)` \
    `TesseractLanguage(Best language, bool useAnyCached = false)` \
    `TesseractLanguage(BestScript language, bool useAnyCached = false)`
    
    With these constructors, you can specify a language from the list of available languages defined in the `TesseractLanguage.Fast`, `TesseractLanguage.FastScript`, `TesseractLanguage.Best`, and `TesseractLanguage.BestScript` enums.
    
    MuPDFCore will then look for the trained model file corresponding to the selected language, relative to the _path of the executable_, in a folder called `tessdata/fast` and then in a folder called `fast` (or `best`, depending on the overload; for the overloads taking a script name, it looks in `tessdata/fast/script` or `fast/script` instead).
    
    If the language file is not found in either of these folders, it then looks for it in a subfolder called `tessdata/fast` in `Environment.SpecialFolder.LocalApplicationData`. If the optional argument `useAnyCached` is `true`, it also looks for the language file in the same folder as the executable, and then in the `best` (or `fast`) subfolders. In this case, for example, if the language file for `TesseractLanguage.Fast.Eng` is not available, but the file for `TesseractLanguage.Best.Eng` is available, the latter will be used.

    Finally, if the language file could not be found in any of the possible paths, MuPDFCore will download it from the appropriate repository and place it in the appropriate subfolder of the `tessdata` folder in `Environment.SpecialFolder.LocalApplicationData`. The file will then be reused as necessary.

    The `TESSDATA_PREFIX` and language name will then be set accordingly to where the file was located.
    
    This means that if you use one of these constructors you do not have to worry about the language files being installed in the right place; as long as the user has an Internet connection, the library will download the language files as necessary.

**Note**: the Tesseract OCR is not supported on macOS on Apple silicon, because I could not find a way to compile the native MuPDF library with Tesseract on this platform (can you help?). If you try to use any OCR method in an app published with target `osx-arm64`, you will get an exception (you can catch this and fail gracefully). If you need to use the OCR functions on macOS, you should publish with target `osx-x64` and rely on Rosetta 2 to run your program on Apple silicon Macs.

### MuPDFCore.MuPDFRenderer control

To use the `PDFRenderer` control in an Avalonia application, first of all you need to add it to you Avalonia `Window`, e.g. in the XAML:

```XML
    <Window xmlns="https://github.com/avaloniaui"
            ...
            xmlns:mupdf="clr-namespace:MuPDFCore.MuPDFRenderer;assembly=MuPDFCore.MuPDFRenderer"
            Opened="WindowOpened"
            ... >
        <mupdf:PDFRenderer Name="MuPDFRenderer" />
    </Window>
```

You then need to initialise it from the backing code, e.g. in a `WindowOpened` event:

```Csharp
    private void WindowOpened(object sender, EventArgs e)
    {
        this.FindControl<PDFRenderer>("MuPDFRenderer").Initialize("path/to/file.pdf");
    }
```

This way, the renderer will start showing the first page of the specified document, using a number of rendering threads that is decided based on the number of processors in the computer. There are many other ways to initialise a PDFRenderer, so make sure to look at the [documentation](https://arklumpus.github.io/MuPDFCore/) to see the other possibilities!

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

For convenience, these compiled files for MuPDF 1.19.0 are included in the [`native/MuPDFWrapper/lib` folder](https://github.com/arklumpus/MuPDFCore/tree/master/native/MuPDFWrapper/lib) of this repository.

#### Tips for compiling MuPDF 1.19.0:

* On all platforms:
    * You do not need to follow the instructions in `thirdparty/tesseract.txt`, as in this version the _leptonica_ and _tesseract_ libraries are already included in the source archive.
    * Delete or comment line 1082 in `source/fitz/ocr-device.c` (the one reading `fz_save_pixmap_as_png(ctx, ocr->pixmap, "ass.png");`). This line creates a file called `ass.png` when running the OCR process. This may be useful for debugging, but may have the unintended consequence of overwriting a file with same name, or cause a runtime error if the user does not have write permissions.
	* Delete or comment line 316 in `source/fitz/output.c` (the `fz_throw` invocation within the `buffer_seek` method - this should leave the `buffer_seek` method empty). This line throws an exception when a seek operation on a buffer is attempted. The problem is that this makes it impossible to render a document as a PSD image in memory, because the `fz_write_pixmap_as_psd` method performs a few seek operations. By removing this line, we turn buffer seeks into no-ops; this doesn't seem to have catastrophic side-effects and the PSD documents produced in this way appear to be fine.

* On Windows (x64):
    * Open the `platform/win32/mupdf.sln` solution in Visual Studio and select the `ReleaseTesseract` configuration and `x64` architecture. Right-click on each project, to open its properties, then go to `C/C++` > `Code Generation` and set the `Runtime Library` to `Multi-threaded DLL (/MD)` (ignore any project for which this option is not available). Save everything (`CTRL+SHIFT+S`) and close Visual Studio.
    * Now, open the `x64 Native Tools Command Prompt for VS`, move to the folder with the solution file, and build it using `msbuild mupdf.sln`
    * Then, build again using `msbuild mupdf.sln /p:Configuration=Release`. Ignore the compilation errors.
    * Finally, build again using `msbuild mupdf.sln /p:Configuration=ReleaseTesseract`.
    * This may still show some errors, but should produce the `libmupdf.lib` file that is required in the `x64/ReleaseTesseract` folder (the file should be ~383MB in size).

* On Windows (x86):
    * Open the `platform/win32/mupdf.sln` solution in Visual Studio and select the `ReleaseTesseract` configuration and `Win32` architecture. Right-click on each project, to open its properties, then go to `C/C++` > `Code Generation` and set the `Runtime Library` to `Multi-threaded DLL (/MD)` (ignore any project for which this option is not available). Save everything (`CTRL+SHIFT+S`) and close Visual Studio.
    * Now, open the `x86 Native Tools Command Prompt for VS`, move to the folder with the solution file, and build it using `msbuild mupdf.sln /p:Platform=Win32`
    * Then, build again using `msbuild mupdf.sln /p:Configuration=Release /p:Platform=Win32`. Ignore the compilation errors.
    * Finally, build again using `msbuild mupdf.sln /p:Configuration=ReleaseTesseract /p:Platform=Win32`.
    * This may still show some errors, but should produce the `libmupdf.lib` file that is required in the `ReleaseTesseract` folder (the file should be ~362MB in size).

* On Windows (arm64)
    
    This is going to be a bit more complicated, because it appears that MuPDF is not meant to be built on ARM. These instructions will assume that you are building MuPDF on an ARM machine.
    
    First of all, make sure that you have installed Visual Studio 2022 and have selected the C++ ARM64 build tools component of the "Desktop development with C++" workload.
    
    **Note**: When you install Visual Studio on an ARM machine, it will complain that this is not supported and will be slow. Ignore that warning.

    * Download and extract the MuPDF source code and follow the instructions for all platforms above.
    * Add ` || defined(_M_ARM64)` at the end of line 16 in `scripts/tesseract/endianness.h`.
    * Now we need to edit a few files in the `thirdparty/tesseract/src/arch` folder.
        * Comment or delete lines 149-177 (inclusive) in `simddetect.cpp`. You should now have an empty block between `#  elif defined(_WIN32)` and `#else`. Also comment or delete lines 198-220 (inclusive) and 237-260 (inclusive).
        * Comment or delete lines 20-22 (inclusive) in `dotproductsse.cpp`. Replace the whole body of the `DotProductSSE` method (lines 30-76) with `return DotProductNative(u, v, n);`.
        * Comment or delete lines 20-21 (inclusive) in `dotproductavx.cpp`. Replace the whole body of the `DotProductAVX` method (lines 29-54) with `return DotProductNative(u, v, n);`.
        * Comment or delete lines 20-21 (inclusive) in `dotproductfma.cpp`. Replace the whole body of the `DotProductFMA` method (lines 29-52) with `return DotProductNative(u, v, n);`.
        * Delete the contents of `thirdparty/tesseract/src/arch/intsimdmatrixavx2.cpp` and `thirdparty/tesseract/src/arch/intsimdmatrixsse.cpp` (do not delete the files, just their contents).
        * Comment or delete lines 120-121 (inclusive) in `intsimdmatrix.h`

    * Open the `platform/win32/mupdf.sln` solution in Visual Studio. You should get a prompt to retarget your projects. Accept the default settings (latest Windows SDK and v143 of the tools).
    * In Visual Studio, click on the "Configuration Manager" item from the "Build" menu. In the new window, click on the drop down menu for the "Active solution platform" and select `<New...>`. In this new dialog, select the `ARM64` platform and choose to copy the settings from `x64`. Leave the `Create new project platforms` option enabled and click on `OK` (this may take some time).
    * Close the Configuration Manager and select the `ReleaseTesseract` configuration and `ARM64` architecture. Right-click on each project, to open its properties, then go to `C/C++` > `Code Generation` and set the `Runtime Library` to `Multi-threaded DLL (/MD)` (ignore any project for which this option is not available).
    * Open the properties for the `libpkcs7` project, go to `C/C++` > `Preprocessor` and remove `HAVE_LIBCRYPTO` from the `Preprocessor Definitions`. Then go to `Librarian` > `General` and remove `libcrypto.lib` from the `Additional Dependencies`.
    * Save everything (`CTRL+SHIFT+S`) and close Visual Studio.
    * Create a new folder `platform/win32/Release`. Now, the problem is that the `bin2coff` script included with MuPDF cannot create `obj` files for ARM64 (only for x86 and x64). Since I could not find a version that can do this, I [translated the source code of bin2coff to C# and added this option myself](https://github.com/arklumpus/bin2coff). You can download an ARM64 `bin2coff.exe` from [here](https://github.com/arklumpus/bin2coff/releases/latest/download/win-arm64.zip); place it in the `Release` folder that you have just created.
    * Open the `Developer Command Prompt for VS`, move to the folder with the solution file (`platform/win32`), and build it using `msbuild mupdf.sln /p:Configuration=ReleaseTesseract`.
    * After a while, this should produce `libmupdf.lib` in the `ARM64/ReleaseTesseract` folder (the file should be ~388MB in size).

* On Linux (x64):
    * Edit the `Makefile`, adding the `-fPIC` compiler option at the end of line 24 (which specifies the `CFLAGS`).
    * Make sure that you are using a recent enough version of GCC (version 7.3.1 seems to be enough).
    * Compile by running `USE_TESSERACT=yes make HAVE_X11=no HAVE_GLUT=no` (this builds just the command-line libraries and tools, and enables OCR through the included Tesseract library).

* On Linux (arm64):
    * Edit the `Makefile`, adding the `-fPIC` compiler option at the end of line 24 (which specifies the `CFLAGS`).
    * Delete or comment line 218 in `thirdparty/tesseract/src/arch/simddetect.cpp`.
    * Make sure that you are using a recent enough version of GCC (version 7.3.1 seems to be enough).
    * Compile by running `USE_TESSERACT=yes make HAVE_X11=no HAVE_GLUT=no` (this builds just the command-line libraries and tools, and enables OCR through the included Tesseract library).

* On macOS (Intel - x64):
    * Edit the `Makefile`, adding the `-fPIC` compiler option at the end of line 24 (which specifies the `CFLAGS`). Also add the `-std=c++11` option at the end of line 58 (which specifies the CXX_CMD).
    * Compile by running `USE_TESSERACT=yes make` (this enables OCR through the included Tesseract library).

* On macOS (Apple silicon - arm64)
    * Edit the `Makefile`, adding the `-fPIC` compiler options at the end of line 24 (which specifies the `CFLAGS`). Also add the `-std=c++11` option at the end of line 58 (which specifies the CXX_CMD).
    * Compile by running `make` (this disables OCR, unfortunately - if you find a way to compile MuPDF with OCR support on Apple silicon, let me know).

### 2. Building MuPDFWrapper

Once you have the required static library files, you should download the MuPDFCore source code: [MuPDFCore-1.3.0.tar.gz](https://github.com/arklumpus/MuPDFCore/archive/v1.3.0.tar.gz) (or clone the repository) and place the library files in the appropriate subdirectories in the `native/MuPDFWrapper/lib/` folder.

To compile `MuPDFWrapper` you will need [CMake](https://cmake.org/) (version 3.8 or higher) and (on Windows) [Ninja](https://ninja-build.org/).

On Windows, the easiest way to get all the required tools is probably to install [Visual Studio](https://visualstudio.microsoft.com/it/). By selecting the "Desktop development with C++" workload you should get everything you need.

On macOS, you will need to install at least the Command-Line Tools for Xcode (if necessary, you should be prompted to do this while you perform the following steps) and CMake.

Once you have everything at the ready, you will have to build MuPDFWrapper on the seven platforms.

#### Windows (x86 and x64)

1. <p>Assuming you have installed Visual Studio, you should open the "<strong>x64</strong> Native Tools Command Prompt for VS" or the "<strong>x86</strong> Native Tools Command Prompt for VS" (you should be able to find these in the Start menu). Take care to open the version corresponding to the architecture you are building for, otherwise you will not be able to compile the library. A normal command prompt will not work, either.</p>
    <p><strong>Note 1</strong>: you <strong>must</strong> build the library on two separate systems, one running a 32-bit version of Windows and the other running a 64-bit version. If you try to build the x86 library on an x64 system, the system will probably build a 64-bit library and place it in the 32-bit output folder, which will just make things very confusing.</p>
    <p><strong>Note 2 for Windows x86</strong>: for some reason, Visual Studio might install the 64-bit version of CMake and Ninja, even though you are on a 32-bit machine. If this happens, you will have to manually install the 32-bit CMake and compile a 32-bit version of Ninja (which also requires Python to be installed). You will notice if this is an issue because the 64-bit programs will refuse to run.</p>
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
4. Update line 50 of the `Tests/Tests.csproj` project file so that it refers to the version of the MuPDFCore package you have just created.

These steps ensure that you are testing the right version of MuPDFCore (i.e. your freshly built copy) and not something else that may have been cached.

Now, open a windows command line in the folder where you have downloaded the MuPDFCore source code, type `BuildTests` and press `Enter`. This will create a number of files in the `Release\MuPDFCoreTests` folder, where each file is an archive containing the tests for a certain platform and architecture:

* `MuPDFCoreTests-linux-x64.tar.gz` contains the tests for Linux environments on x64 processors.
* `MuPDFCoreTests-linux-arm64.tar.gz` contains the tests for Linux environments on arm64 processors.
* `MuPDFCoreTests-mac-x64.tar.gz` contains the tests for macOS environments on Intel processors.
* `MuPDFCoreTests-mac-arm64.tar.gz` contains the tests for macOS environments on Apple silicon processors.
* `MuPDFCoreTests-win-x64.tar.gz` contains the tests for Windows environments on x64 processors.
* `MuPDFCoreTests-win-x86.tar.gz` contains the tests for Windows environments on x86 processors.

To run the tests, copy each archive to a machine running the corresponding operating system, and extract it. Then:

#### Windows
* Open a command prompt and `CD` into the folder where you have extracted the contents of the test archive.
* Enter the command `MuPDFCoreTestHost` (this will run the test program).

#### macOS and Linux
* Open a terminal and `cd` into the folder where you have extracted the contents of the test archive.
* Enter the command `chmod +x MuPDFCoreTestHost` (this will add the executable flag to the test program).
* Enter the command `./MuPDFCoreTestHost` (this will run the test program).
* On macOS, depending on your security settings, you may get a message saying `zsh: killed` when you try to run the program. To address this, you need to sign the executable, e.g. by running `codesign --timestamp --sign <certificate> MuPDFCoreTestHost`, where `<certificate>` is the name of a code signing certificate in your keychain (e.g. `Developer ID Application: John Smith`). After this, you can try again to run the test program with `./MuPDFCoreTestHost`.

The test suite will start; it will print the name of each test, followed by a green `  Succeeded  ` or a red `  Failed  ` depending on the test result. If everything went correctly, all tests should succeed (except for the 5 OCR tests on Apple silicon Macs).

When all the tests have been run, the program will print a summary showing how many tests have succeeded (if any) and how many have failed (if any). If any tests have failed, a list of these will be printed, and then they will be run again one at a time, waiting for a key press before running each test (this makes it easier to follow what is going on). If you wish to kill the test process early, you can do so with `CTRL+C`.

## Note about MuPDFCore and .NET Framework <a name="netFrameworkNote"></a>

If you wish to use MuPDFCore in a .NET Framework project, you will need to manually copy the native MuPDFWrapper library for the platform you are using to the executable directory (this is done automatically if you target .NET/.NET core).

One way to obtain the appropriate library files is:

1. Manually download the NuGet package for [MuPDFCore](https://www.nuget.org/packages/MuPDFCore/) (click on the "Download package" link on the right).
2. Rename the `.nupkg` file so that it has a `.zip` extension.
3. Extract the zip file.
4. Within the extracted folder, the library files are in the `runtimes/xxx/native/` folder, where `xxx` is `linux-x64`, `linux-arm64`, `osx-x64`, `osx-arm64`, `win-x64`, `win-x86` or `win-arm64`, depending on the platform you are using.

Make sure you copy the appropriate file to the same folder as the executable!
