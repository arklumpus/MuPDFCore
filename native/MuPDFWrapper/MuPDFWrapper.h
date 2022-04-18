#pragma once

//Exit codes.
enum
{
	ERR_CANNOT_CREATE_CONTEXT = 129,
	ERR_CANNOT_REGISTER_HANDLERS = 130,
	ERR_CANNOT_OPEN_FILE = 131,
	ERR_CANNOT_COUNT_PAGES = 132,
	ERR_CANNOT_RENDER = 134,
	ERR_CANNOT_OPEN_STREAM = 135,
	ERR_CANNOT_LOAD_PAGE = 136,
	ERR_CANNOT_COMPUTE_BOUNDS = 137,
	ERR_CANNOT_INIT_MUTEX = 138,
	ERR_CANNOT_CLONE_CONTEXT = 139,
	ERR_CANNOT_SAVE = 140,
	ERR_CANNOT_CREATE_BUFFER = 141,
	ERR_CANNOT_CREATE_WRITER = 142,
	ERR_CANNOT_CLOSE_DOCUMENT = 143,
	ERR_CANNOT_CREATE_PAGE = 144,
	ERR_CANNOT_POPULATE_PAGE = 145
};

//Output raster image formats.
enum
{
	OUT_PNM = 0,
	OUT_PAM = 1,
	OUT_PNG = 2,
	OUT_PSD = 3
};

//Output document formats.
enum
{
	OUT_DOC_PDF = 0,
	OUT_DOC_SVG = 1,
	OUT_DOC_CBZ = 2,
	OUT_DOC_DOCX = 3,
	OUT_DOC_ODT = 4,
	OUT_DOC_HTML = 5,
	OUT_DOC_XHTML = 6
};

//Colour formats
enum
{
	COLOR_RGB = 0,
	COLOR_RGBA = 1,
	COLOR_BGR = 2,
	COLOR_BGRA = 3
};


//Macros to define the exported functions.
#define BUILDING_DLL 1
#define PTW32_STATIC_LIB 1

#if defined _WIN32 || defined __CYGWIN__ || defined __MINGW32__
#ifdef BUILDING_DLL
#ifdef __GNUC__
#define DLL_PUBLIC __attribute__ ((dllexport))
#else
#define DLL_PUBLIC __declspec(dllexport) // Note: actually gcc seems to also supports this syntax.
#endif
#else
#ifdef __GNUC__
#define DLL_PUBLIC __attribute__ ((dllimport))
#else
#define DLL_PUBLIC __declspec(dllimport) // Note: actually gcc seems to also supports this syntax.
#endif
#endif
#define DLL_LOCAL
#else
#if __GNUC__ >= 4
#define DLL_PUBLIC __attribute__ ((visibility ("default")))
#define DLL_LOCAL  __attribute__ ((visibility ("hidden")))
#else
#define DLL_PUBLIC
#define DLL_LOCAL
#endif
#endif

//A structure to hold the mutexes used by the locking mechanism. An array might have been a better choice, but this is more easily manageable.
struct mutex_holder
{
	std::mutex mutex0;
	std::mutex mutex1;
	std::mutex mutex2;
	std::mutex mutex3;
};

mutex_holder global_mutex;

//Copied here from store.c
typedef struct fz_item
{
	void* key;
	fz_storable* val;
	size_t size;
	struct fz_item* next;
	struct fz_item* prev;
	fz_store* store;
	const fz_store_type* type;
} fz_item;

//Copied here from store.c
/* Every entry in fz_store is protected by the alloc lock */
struct fz_store
{
	int refs;

	/* Every item in the store is kept in a doubly linked list, ordered
	 * by usage (so LRU entries are at the end). */
	fz_item* head;
	fz_item* tail;

	/* We have a hash table that allows to quickly find a subset of the
	 * entries (those whose keys are indirect objects). */
	fz_hash_table* hash;

	/* We keep track of the size of the store, and keep it below max. */
	size_t max;
	size_t size;

	int defer_reap_count;
	int needs_reaping;
	int scavenging;
};

//Exported methods
extern "C"
{
	/// <summary>
	/// Get the contents of a structured text character.
	/// </summary>
	/// <param name="character">The address of the character.</param>
	/// <param name="out_c">Unicode code point of the character.</param>
	/// <param name="out_color">An sRGB hex representation of the colour of the character.</param>
	/// <param name="out_origin_x">The x coordinate of the baseline origin of the character.</param>
	/// <param name="out_origin_y">The y coordinate of the baseline origin of the character.</param>
	/// <param name="out_size">The size in points of the character.</param>
	/// <param name="out_ll_x">The x coordinate of the lower left corner of the bounding quad.</param>
	/// <param name="out_ll_y">The y coordinate of the lower left corner of the bounding quad.</param>
	/// <param name="out_ul_x">The x coordinate of the upper left corner of the bounding quad.</param>
	/// <param name="out_ul_y">The y coordinate of the upper left corner of the bounding quad.</param>
	/// <param name="out_ur_x">The x coordinate of the upper right corner of the bounding quad.</param>
	/// <param name="out_ur_y">The y coordinate of the upper right corner of the bounding quad.</param>
	/// <param name="out_lr_x">The x coordinate of the lower right corner of the bounding quad.</param>
	/// <param name="out_lr_y">The y coordinate of the lower right corner of the bounding quad.</param>
	/// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
	DLL_PUBLIC int GetStructuredTextChar(fz_stext_char* character, int* out_c, int* out_color, float* out_origin_x, float* out_origin_y, float* out_size, float* out_ll_x, float* out_ll_y, float* out_ul_x, float* out_ul_y, float* out_ur_x, float* out_ur_y, float* out_lr_x, float* out_lr_y);

	/// <summary>
	/// Get an array of structured text characters from a structured text line.
	/// </summary>
	/// <param name="line">The structured text line from which the characters should be extracted.</param>
	/// <param name="out_chars">An array of pointers to the structured text characters.</param>
	/// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
	DLL_PUBLIC int GetStructuredTextChars(fz_stext_line* line, fz_stext_char** out_chars);

	/// <summary>
	/// Get the contents of a structured text line.
	/// </summary>
	/// <param name="line">The address of the line.</param>
	/// <param name="out_wmode">An integer equivalent to <see cref="MuPDFStructuredTextLine"/> representing the writing mode of the line.</param>
	/// <param name="out_x0">The left coordinate in page units of the bounding box of the line.</param>
	/// <param name="out_y0">The top coordinate in page units of the bounding box of the line.</param>
	/// <param name="out_x1">The right coordinate in page units of the bounding box of the line.</param>
	/// <param name="out_y1">The bottom coordinate in page units of the bounding box of the line.</param>
	/// <param name="out_x">The x component of the normalised direction of the baseline.</param>
	/// <param name="out_y">The y component of the normalised direction of the baseline.</param>
	/// <param name="out_char_count">The number of characters in the line.</param>
	/// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
	DLL_PUBLIC int GetStructuredTextLine(fz_stext_line* line, int* out_wmode, float* out_x0, float* out_y0, float* out_x1, float* out_y1, float* out_x, float* out_y, int* out_char_count);

	/// <summary>
	/// Get an array of structured text lines from a structured text block.
	/// </summary>
	/// <param name="block">The structured text block from which the lines should be extracted.</param>
	/// <param name="out_lines">An array of pointers to the structured text lines.</param>
	/// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
	DLL_PUBLIC int GetStructuredTextLines(fz_stext_block* block, fz_stext_line** out_lines);

	/// <summary>
	/// Get the contents of a structured text block.
	/// </summary>
	/// <param name="block">The address of the block.</param>
	/// <param name="out_type">An integer equivalent to <see cref="MuPDFStructuredTextBlock.Types"/> representing the type of the block.</param>
	/// <param name="out_x0">The left coordinate in page units of the bounding box of the block.</param>
	/// <param name="out_y0">The top coordinate in page units of the bounding box of the block.</param>
	/// <param name="out_x1">The right coordinate in page units of the bounding box of the block.</param>
	/// <param name="out_y1">The bottom coordinate in page units of the bounding box of the block.</param>
	/// <param name="out_line_count">The number of lines in the block.</param>
	/// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
	DLL_PUBLIC int GetStructuredTextBlock(fz_stext_block* block, int* out_type, float* out_x0, float* out_y0, float* out_x1, float* out_y1, int* out_line_count);

	/// <summary>
	/// Get an array of structured text blocks from a structured text page.
	/// </summary>
	/// <param name="page">The structured text page from which the blocks should be extracted.</param>
	/// <param name="out_blocks">An array of pointers to the structured text blocks.</param>
	/// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
	DLL_PUBLIC int GetStructuredTextBlocks(fz_stext_page* page, fz_stext_block** out_blocks);

	/// <summary>
	/// Get a structured text representation of a display list, using the Tesseract OCR engine.
	/// </summary>
	/// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
	/// <param name="list">The display list whose structured text representation is sought.</param>
	/// <param name="out_page">The address of the structured text page.</param>
	/// <param name="out_stext_block_count">The number of structured text blocks in the page.</param>
	/// <param name="zoom">How much the specified region should be scaled when rendering. This determines the size in pixels of the image that is passed to Tesseract.</param>
	/// <param name="x0">The left coordinate in page units of the region of the display list that should be analysed.</param>
	/// <param name="y0">The top coordinate in page units of the region of the display list that should be analysed.</param>
	/// <param name="x1">The right coordinate in page units of the region of the display list that should be analysed.</param>
	/// <param name="y1">The bottom coordinate in page units of the region of the display list that should be analysed.</param>
	/// <param name="prefix">A string value that will be used as an argument for the <c>putenv</c> function. If this is <see langword="null"/>, the <c>putenv</c> function is not invoked. Usually used to set the value of the <c>TESSDATA_PREFIX</c> environment variable.</param>
	/// <param name="language">The name of the language model file to use for the OCR.</param>
	/// <param name="callback">A progress callback function. This function will be called with an integer parameter ranging from 0 to 100 to indicate OCR progress, and should return 0 to continue or 1 to abort the OCR process.</param>
	/// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
	DLL_PUBLIC int GetStructuredTextPageWithOCR(fz_context* ctx, fz_display_list* list, fz_stext_page** out_page, int* out_stext_block_count, float zoom, float x0, float y0, float x1, float y1, char* prefix, char* language, int __stdcall callback(int));

	/// <summary>
	/// Get a structured text representation of a display list.
	/// </summary>
	/// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
	/// <param name="list">The display list whose structured text representation is sought.</param>
	/// <param name="out_page">The address of the structured text page.</param>
	/// <param name="out_stext_block_count">The number of structured text blocks in the page.</param>
	/// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
	DLL_PUBLIC int GetStructuredTextPage(fz_context* ctx, fz_display_list* list, fz_stext_page** out_page, int* out_stext_block_count);

	/// <summary>
	/// Free a native structured text page and its associated resources.
	/// </summary>
	/// <param name="ctx"></param>
	/// <param name="page"></param>
	/// <returns>An integer equivalent to <see cref="ExitCodes"/> detailing whether any errors occurred.</returns>
	DLL_PUBLIC int DisposeStructuredTextPage(fz_context* ctx, fz_stext_page* page);

	/// <summary>
	/// Finalise a document writer, closing the file and freeing all resources.
	/// </summary>
	/// <param name="ctx">The context that was used to create the document writer.</param>
	/// <param name="writ">The document writer to finalise.</param>
	/// <returns>An integer detailing whether any errors occurred.</returns>
	DLL_PUBLIC int FinalizeDocumentWriter(fz_context* ctx, fz_document_writer* writ);

	/// <summary>
	/// Render (part of) a display list as a page in the specified document writer.
	/// </summary>
	/// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
	/// <param name="list">The display list to render.</param>
	/// <param name="x0">The left coordinate in page units of the region of the display list that should be rendererd.</param>
	/// <param name="y0">The top coordinate in page units of the region of the display list that should be rendererd.</param>
	/// <param name="x1">The right coordinate in page units of the region of the display list that should be rendererd.</param>
	/// <param name="y1">The bottom coordinate in page units of the region of the display list that should be rendererd.</param>
	/// <param name="zoom">How much the specified region should be scaled when rendering. This will determine the final size of the page.</param>
	/// <param name="writ">The document writer on which the page should be written.</param>
	/// <returns>An integer detailing whether any errors occurred.</returns>
	DLL_PUBLIC int WriteSubDisplayListAsPage(fz_context* ctx, fz_display_list* list, float x0, float y0, float x1, float y1, float zoom, fz_document_writer* writ);

	/// <summary>
	/// Create a new document writer object.
	/// </summary>
	/// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
	/// <param name="file_name">The name of file that will hold the writer's output.</param>
	/// <param name="format">An integer specifying the output format.</param>
	/// <param name="out_document_writer">A pointer to the new document writer object.</param>
	/// <returns>An integer detailing whether any errors occurred.</returns>
	DLL_PUBLIC int CreateDocumentWriter(fz_context* ctx, const char* file_name, int format, const fz_document_writer** out_document_writer);

	/// <summary>
	/// Write (part of) a display list to an image buffer in the specified format.
	/// </summary>
	/// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
	/// <param name="list">The display list to render.</param>
	/// <param name="x0">The left coordinate in page units of the region of the display list that should be rendererd.</param>
	/// <param name="y0">The top coordinate in page units of the region of the display list that should be rendererd.</param>
	/// <param name="x1">The right coordinate in page units of the region of the display list that should be rendererd.</param>
	/// <param name="y1">The bottom coordinate in page units of the region of the display list that should be rendererd.</param>
	/// <param name="zoom">How much the specified region should be scaled when rendering. This determines the size in pixels of the rendered image.</param>
	/// <param name="colorFormat">The pixel data format.</param>
	/// <param name="output_format">An integer specifying the output format.</param>
	/// <param name="out_buffer">The address of the buffer on which the data has been written (only useful for disposing the buffer later).</param>
	/// <param name="out_data">The address of the byte array where the data has been actually written.</param>
	/// <param name="out_length">The length in bytes of the image data.</param>
	/// <returns>An integer detailing whether any errors occurred.</returns>
	DLL_PUBLIC int WriteImage(fz_context* ctx, fz_display_list* list, float x0, float y0, float x1, float y1, float zoom, int colorFormat, int output_format, const fz_buffer** out_buffer, const unsigned char** out_data, uint64_t* out_length);

	/// <summary>
	/// Free a native buffer and its associated resources.
	/// </summary>
	/// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
	/// <param name="buf">The buffer to free.</param>
	/// <returns>An integer detailing whether any errors occurred.</returns>
	DLL_PUBLIC int DisposeBuffer(fz_context* ctx, fz_buffer* buf);

	/// <summary>
	/// Save (part of) a display list to an image file in the specified format.
	/// </summary>
	/// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
	/// <param name="list">The display list to render.</param>
	/// <param name="x0">The left coordinate in page units of the region of the display list that should be rendererd.</param>
	/// <param name="y0">The top coordinate in page units of the region of the display list that should be rendererd.</param>
	/// <param name="x1">The right coordinate in page units of the region of the display list that should be rendererd.</param>
	/// <param name="y1">The bottom coordinate in page units of the region of the display list that should be rendererd.</param>
	/// <param name="zoom">How much the specified region should be scaled when rendering. This determines the size in pixels of the rendered image.</param>
	/// <param name="colorFormat">The pixel data format.</param>
	/// <param name="file_name">The path to the output file.</param>
	/// <param name="output_format">An integer specifying the output format.</param>
	/// <returns>An integer detailing whether any errors occurred.</returns>
	DLL_PUBLIC int SaveImage(fz_context* ctx, fz_display_list* list, float x0, float y0, float x1, float y1, float zoom, int colorFormat, const char* file_name, int output_format);

	/// <summary>
	/// Create cloned contexts that can be used in multithreaded rendering.
	/// </summary>
	/// <param name="ctx">The original context to clone</param>
	/// <param name="count">The number of cloned contexts to create.</param>
	/// <param name="out_contexts">An array of pointers to the cloned contexts.</param>
	/// <returns>An integer detailing whether any errors occurred.</returns>
	DLL_PUBLIC int CloneContext(fz_context* ctx, int count, fz_context** out_contexts);

	/// <summary>
	/// Render (part of) a display list to an array of bytes starting at the specified pointer.
	/// </summary>
	/// <param name="ctx">A context to hold the exception stack and the cached resources.</param>
	/// <param name="list">The display list to render.</param>
	/// <param name="x0">The left coordinate in page units of the region of the display list that should be rendererd.</param>
	/// <param name="y0">The top coordinate in page units of the region of the display list that should be rendererd.</param>
	/// <param name="x1">The right coordinate in page units of the region of the display list that should be rendererd.</param>
	/// <param name="y1">The bottom coordinate in page units of the region of the display list that should be rendererd.</param>
	/// <param name="zoom">How much the specified region should be scaled when rendering. This determines the size in pixels of the rendered image.</param>
	/// <param name="colorFormat">The pixel data format.</param>
	/// <param name="pixel_storage">A pointer indicating where the pixel bytes will be written. There must be enough space available!</param>
	/// <param name="cookie">A pointer to a cookie object that can be used to track progress and/or abort rendering. Can be null.</param>
	/// <returns>An integer detailing whether any errors occurred.</returns>
	DLL_PUBLIC int RenderSubDisplayList(fz_context* ctx, fz_display_list* list, float x0, float y0, float x1, float y1, float zoom, int colorFormat, unsigned char* pixel_storage, fz_cookie* cookie);

	/// <summary>
	/// Create a display list from a page.
	/// </summary>
	/// <param name="ctx">A pointer to the context used to create the document.</param>
	/// <param name="page">A pointer to the page that should be used to create the display list.</param>
	/// <param name="annotations">An integer indicating whether annotations should be included in the display list (1) or not (any other value).</param>
	/// <param name="out_display_list">A pointer to the newly-created display list.</param>
	/// <param name="out_x0">The left coordinate of the display list's bounds.</param>
	/// <param name="out_y0">The top coordinate of the display list's bounds.</param>
	/// <param name="out_x1">The right coordinate of the display list's bounds.</param>
	/// <param name="out_y1">The bottom coordinate of the display list's bounds.</param>
	/// <returns>An integer detailing whether any errors occurred.</returns>
	DLL_PUBLIC int GetDisplayList(fz_context* ctx, fz_page* page, int annotations, fz_display_list** out_display_list, float* out_x0, float* out_y0, float* out_x1, float* out_y1);

	/// <summary>
	/// Free a display list.
	/// </summary>
	/// <param name="ctx">The context that was used to create the display list.</param>
	/// <param name="list">The display list to dispose.</param>
	/// <returns>An integer detailing whether any errors occurred.</returns>
	DLL_PUBLIC int DisposeDisplayList(fz_context* ctx, fz_display_list* list);

	/// <summary>
	/// Load a page from a document.
	/// </summary>
	/// <param name="ctx">The context to which the document belongs.</param>
	/// <param name="doc">The document from which the page should be extracted.</param>
	/// <param name="page_number">The page number.</param>
	/// <param name="out_page">The newly extracted page.</param>
	/// <param name="out_x">The left coordinate of the page's bounds.</param>
	/// <param name="out_y">The top coordinate of the page's bounds.</param>
	/// <param name="out_w">The width of the page.</param>
	/// <param name="out_h">The height of the page.</param>
	/// <returns>An integer detailing whether any errors occurred.</returns>
	DLL_PUBLIC int LoadPage(fz_context* ctx, fz_document* doc, int page_number, const fz_page** out_page, float* out_x, float* out_y, float* out_w, float* out_h);

	/// <summary>
	/// Free a page and its associated resources.
	/// </summary>
	/// <param name="ctx">The context to which the document containing the page belongs.</param>
	/// <param name="page">The page to free.</param>
	/// <returns>An integer detailing whether any errors occurred.</returns>
	DLL_PUBLIC int DisposePage(fz_context* ctx, fz_page* page);

	/// <summary>
	/// Create a new document from a file name.
	/// </summary>
	/// <param name="ctx">The context to which the document will belong.</param>
	/// <param name="file_name">The path of the file to open.</param>
	/// <param name="file_name">The path of the file to open.</param>
	/// <param name="get_image_resolution">If this is not 0, try opening the file as an image and return the actual resolution (in DPI) of the image. Otherwise (or if trying to open the file as an image fails), the returned resolution will be -1.</param>
	/// <param name="out_page_count">The number of pages in the document.</param>
	/// <param name="out_image_xres">If the document is an image file, the horizontal resolution of the image.</param>
	/// <param name="out_image_yres">If the document is an image file, the vertical resolution of the image.</param>
	/// <returns>An integer detailing whether any errors occurred.</returns>
	DLL_PUBLIC int CreateDocumentFromFile(fz_context* ctx, const char* file_name, int get_image_resolution, const fz_document** out_doc, int* out_page_count, float* out_image_xres, float* out_image_yres);

	/// <summary>
	/// Create a new document from a stream.
	/// </summary>
	/// <param name="ctx">The context to which the document will belong.</param>
	/// <param name="data">A pointer to a byte array containing the data that makes up the document.</param>
	/// <param name="data_length">The length in bytes of the data that makes up the document.</param>
	/// <param name="file_type">The type (extension) of the document.</param>
	/// <param name="get_image_resolution">If this is not 0, try opening the stream as an image and return the actual resolution (in DPI) of the image. Otherwise (or if trying to open the stream as an image fails), the returned resolution will be -1.</param>
	/// <param name="out_doc">The newly created document.</param>
	/// <param name="out_str">The newly created stream (so that it can be disposed later).</param>
	/// <param name="out_page_count">The number of pages in the document.</param>
	/// <param name="out_image_xres">If the document is an image file, the horizontal resolution of the image.</param>
	/// <param name="out_image_yres">If the document is an image file, the vertical resolution of the image.</param>
	/// <returns>An integer detailing whether any errors occurred.</returns>
	DLL_PUBLIC int CreateDocumentFromStream(fz_context* ctx, const unsigned char* data, const uint64_t data_length, const char* file_type, int get_image_resolution, const fz_document** out_doc, const fz_stream** out_str, int* out_page_count, float* out_image_xres, float* out_image_yres);

	/// <summary>
	/// Free a stream and its associated resources.
	/// </summary>
	/// <param name="ctx">The context that was used while creating the stream.</param>
	/// <param name="str">The stream to free.</param>
	/// <returns>An integer detailing whether any errors occurred.</returns>
	DLL_PUBLIC int DisposeStream(fz_context* ctx, fz_stream* str);

	/// <summary>
	/// Free a document and its associated resources.
	/// </summary>
	/// <param name="ctx">The context that was used in creating the document.</param>
	/// <param name="doc">The document to free.</param>
	/// <returns>An integer detailing whether any errors occurred.</returns>
	DLL_PUBLIC int DisposeDocument(fz_context* ctx, fz_document* doc);

	/// <summary>
	/// Get the current size of the store.
	/// </summary>
	/// <param name="ctx">The context whose store's size should be determined.</param>
	/// <returns>The current size in bytes of the store.</returns>
	DLL_PUBLIC uint64_t GetCurrentStoreSize(const fz_context* ctx);

	/// <summary>
	/// Get the maximum size of the store.
	/// </summary>
	/// <param name="ctx">The context whose store's maximum size should be determined.</param>
	/// <returns>The maximum size in bytes of the store.</returns>
	DLL_PUBLIC uint64_t GetMaxStoreSize(const fz_context* ctx);

	/// <summary>
	/// Evict items from the store until the total size of the objects in the store is reduced to a given percentage of its current size.
	/// </summary>
	/// <param name="ctx">The context whose store should be shrunk.</param>
	/// <param name="perc">Fraction of current size to reduce the store to.</param>
	/// <returns>An integer detailing whether any errors occurred.</returns>
	DLL_PUBLIC int ShrinkStore(fz_context* ctx, unsigned int perc);

	/// <summary>
	/// Evict every item from the store.
	/// </summary>
	/// <param name="ctx">The context whose store should be emptied.</param>
	/// <returns>An integer detailing whether any errors occurred.</returns>
	DLL_PUBLIC void EmptyStore(fz_context* ctx);

	/// <summary>
	/// Create a MuPDF context object with the specified store size.
	/// </summary>
	/// <param name="store_size">Maximum size in bytes of the resource store.</param>
	/// <param name="out_ctx">A pointer to the native context object.</param>
	/// <returns>An integer detailing whether any errors occurred.</returns>
	DLL_PUBLIC int CreateContext(uint64_t store_size, const fz_context** out_ctx);

	/// <summary>
	/// Free a context and its global store.
	/// </summary>
	/// <param name="ctx">A pointer to the native context to free.</param>
	/// <returns>An integer detailing whether any errors occurred.</returns>
	DLL_PUBLIC int DisposeContext(fz_context* ctx);
}