# MuPDFCore: Multiplatform .NET Core bindings for MuPDF

<img src="icon.svg" width="256" align="right">

__MuPDFCore__ is a set of multiplatform .NET Core bindings for [MuPDF](https://mupdf.com/). It can render PDF, XPS, EPUB and other formats to raster images returned either as raw bytes, or as image files in multiple formats (including PNG and PSD). It also supports multithreading.

It also includes __MuPDFCore.MuPDFRenderer__, an Avalonia control to display documents compatible with MuPDFCore in Avalonia windows (with multithreaded rendering).

The library is released under the [AGPLv3](https://www.gnu.org/licenses/agpl-3.0.html) licence.

## Getting started

The MuPDFCore library targets .NET Standard 2.0, thus it can be used in projects that target .NET Standard 2.0+, .NET Core 2.0+, .NET Framework 4.6.1 and possibly others. MuPDFCore includes a pre-compiled native library, thus projects using it can only run on Windows, macOS and Linux x64 operating systems.

To use the library in your project, you should install the [MuPDFCore NuGet package](https://www.nuget.org/packages/MuPDFCore/) and/or the [MuPDFCore.PDFRenderer NuGet package](https://www.nuget.org/packages/MuPDFCore.MuPDFRenderer/).

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


    //The code in this for-loop is not really part of MuPDFCore - it just shows an example of using SixLabors.ImageSharp to "stitch" the tiles up and produce the full image.
    for (int i = 0; i < document.Pages.Count; i++)
    {
        //Create a new (empty) image to hold the whole page.
        SixLabors.ImageSharp.Image renderedPage = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgb24>(renderedPageSizes[i].Width, renderedPageSizes[i].Height);

        //Draw each tile onto the image.
        for (int j = 0; j < renderers[i].ThreadCount; j++)
        {
            ReadOnlySpan<byte> imageData;

            //By using unsafe code, we can avoid having to marshal the image data around.
            unsafe
            {
                //Create a new ReadOnlySpan that reads the unmanaged memory where the image data is located.
                imageData = new ReadOnlySpan<byte>((void*)destinations[i][j], tileBounds[i][j].Height * tileBounds[i][j].Width * 3);
                
            }

            //Load the image data in the tile by using the ReadOnlySpan.
            SixLabors.ImageSharp.Image tile = SixLabors.ImageSharp.Image.LoadPixelData<SixLabors.ImageSharp.PixelFormats.Rgb24>(imageData, tileBounds[i][j].Width, tileBounds[i][j].Height);

            //Draw the tile on the main image page.
            renderedPage.Mutate(x => x.DrawImage(tile, new SixLabors.ImageSharp.Point(tileBounds[i][j].X0, tileBounds[i][j].Y0), 1));

            //Release the resources held by the tile.
            tile.Dispose();
        }

        //Save the full page as a JPG image.
        using (FileStream fs = new FileStream("page" + i.ToString() + ".jpg", FileMode.Create))
        {
            renderedPage.SaveAsJpeg(fs);
        }

        //Release the resources held by the image.
        renderedPage.Dispose();
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

Steps 1 and 2 need to be performed on all of Windows, macOS and Linux (no cross-compiling)! Otherwise, some native assets will be missing and it will not be possible to build the NuGet package.

### 1. Building libmupdf

You can download the open-source (GNU AGPL) MuPDF source code from [here](https://mupdf.com/downloads/index.html). You will need to uncompress the source file and compile the library on Windows, macOS and Linux. You need the following files:

* From Windows:
    * libmupdf.lib
    * libthirdparty.lib

* From macOS:
    * libmupdf.a
    * libmupdf-third.a

* From Linux:
    * libmupdf.a
    * libmupdf-third.a

Note that the files from macOS and Linux are different, despite sharing the same name.

Depending on your system, on Linux and/or macOS you may need to enable the `-fPIC` compiler option to generate library files that can be included in the MuPDFWrapper shared library, otherwise a later step may fail. You can do this in multiple ways, e.g. by opening the `Makefile` included in the MuPDF source and adding `-fPIC` at the end of the line specifying `CFLAGS` (line 23 in the MuPDF 1.17.0 source).

For convenience, these compiled files for MuPDF 1.17.0 are included in the [`native/MuPDFWrapper/lib` folder](https://github.com/arklumpus/MuPDFCore/tree/master/native/MuPDFWrapper/lib) of this repository.

### 2. Building MuPDFWrapper

Once you have the required static library files, you should download the MuPDFCore source code: [MuPDFCore-1.0.0.tar.gz](https://github.com/arklumpus/MuPDFCore/archive/v1.0.0.tar.gz) (or clone the repository) and place the library files in the appropriate subdirectories in the `native/MuPDFWrapper/lib/` folder.

To compile `MuPDFWrapper` you will need [CMake](https://cmake.org/) and (on Windows) [Ninja](https://ninja-build.org/).

On Windows, the easiest way to get all the required tools is probably to install [Visual Studio](https://visualstudio.microsoft.com/it/). By selecting the "Desktop development with C++" you should get everything you need.

On macOS, you will need to install at least the Command-Line Tools for Xcode (if necessary, you should be prompted to do this while you perform the following steps) and CMake.

Once you have everything at the ready, you will have to build MuPDFWrapper on the three platforms.

#### Windows

1. Assuming you have installed Visual Studio, you should open the "__x64__ Native Tools Command Prompt for VS" (you should be able to find this in the Start menu). Take care to open the x64 version, otherwise you will not be able to compile the library. A normal command propmpt will not work, either.
2. `CD` to the directory where you have downloaded the MuPDFCore source code.
3. `CD` into the `native` directory.
4. Type `build`. This will start the `build.cmd` batch script that will delete any previous build and compile the library.

After this finishes, you should find a file named `MuPDFWrapper.dll` in the `native/out/build/win-x64/MuPDFWrapper/` directory. Leave it there.

#### macOS and Linux

1. Assuming you have everything ready, open a terminal in the folder where you have downloaded the MuPDFCore source code.
2. `cd` into the `native` directory.
3. Type `chmod +x build.sh`.
4. Type `./build.sh`. This will delete any previous build and compile the library.

After this finishes, you should find a file named `libMuPDFWrapper.dylib` in the `native/out/build/mac-x64/MuPDFWrapper/` directory (on macOS) and a file named `libMuPDFWrapper.so` in the `native/out/build/linux-x64/MuPDFWrapper/` directory (on Linux). Leave it there.

### 3. Creating the MuPDFCore NuGet package

Once you have the `MuPDFWrapper.dll`, `libMuPDFWrapper.dylib` and `libMuPDFWrapper.so` files, make sure they are in the correct folders (`native/out/build/xxx-x64/MuPDFWrapper/`), __all on the same machine__.

To create the MuPDFCore NuGet package, you will need the [.NET Core 2.0 SDK or higher](https://dotnet.microsoft.com/download/dotnet/current) for your platform. Once you have installed it and have everything ready, open a terminal in the folder where you have downloaded the MuPDFCore source code and type:

```
cd MuPDFCore
dotnet pack -c Release
```

This will create a NuGet package in `MuPDFCore/bin/Release`. You can install this package on your projects by adding a local NuGet source.
