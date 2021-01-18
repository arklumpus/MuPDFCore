#include <mupdf/fitz.h>
#include <mupdf/fitz/display-list.h>
#include <mupdf/fitz/store.h>

#include <stdio.h>
#include <stdlib.h>
#include <mutex>

#include "MuPDFWrapper.h"
#include <iostream>

fz_pixmap*
new_pixmap_with_data(fz_context* ctx, fz_colorspace* colorspace, int w, int h, fz_separations* seps, int alpha, unsigned char* pixel_storage)
{
	int stride;
	int s = fz_count_active_separations(ctx, seps);
	if (!colorspace && s == 0) alpha = 1;
	stride = (fz_colorspace_n(ctx, colorspace) + s + alpha) * w;
	return fz_new_pixmap_with_data(ctx, colorspace, w, h, seps, alpha, stride, pixel_storage);
}

fz_pixmap*
new_pixmap_with_bbox_and_data(fz_context* ctx, fz_colorspace* colorspace, fz_irect bbox, fz_separations* seps, int alpha, unsigned char* pixel_storage)
{
	fz_pixmap* pixmap;
	pixmap = new_pixmap_with_data(ctx, colorspace, bbox.x1 - bbox.x0, bbox.y1 - bbox.y0, seps, alpha, pixel_storage);
	pixmap->x = bbox.x0;
	pixmap->y = bbox.y0;
	return pixmap;
}

fz_pixmap*
new_pixmap_from_display_list_with_separations_bbox_and_data(fz_context* ctx, fz_display_list* list, fz_rect rect, fz_matrix ctm, fz_colorspace* cs, fz_separations* seps, int alpha, unsigned char* pixel_storage, fz_cookie* cookie)
{
	fz_irect bbox;
	fz_pixmap* pix;
	fz_device* dev = NULL;

	fz_var(dev);

	rect = fz_transform_rect(rect, ctm);
	bbox = fz_round_rect(rect);

	pix = new_pixmap_with_bbox_and_data(ctx, cs, bbox, seps, alpha, pixel_storage);
	if (alpha)
		fz_clear_pixmap(ctx, pix);
	else
		fz_clear_pixmap_with_value(ctx, pix, 0xFF);

	fz_try(ctx)
	{
		dev = fz_new_draw_device(ctx, ctm, pix);
		fz_run_display_list(ctx, list, dev, fz_identity, fz_infinite_rect, cookie);
		fz_close_device(ctx, dev);
	}
	fz_always(ctx)
	{
		fz_drop_device(ctx, dev);
	}
	fz_catch(ctx)
	{
		fz_drop_pixmap(ctx, pix);
		fz_rethrow(ctx);
	}

	return pix;
}

fz_pixmap*
new_pixmap_from_display_list_with_separations_bbox(fz_context* ctx, fz_display_list* list, fz_rect rect, fz_matrix ctm, fz_colorspace* cs, fz_separations* seps, int alpha)
{
	fz_irect bbox;
	fz_pixmap* pix;
	fz_device* dev = NULL;

	fz_var(dev);

	rect = fz_transform_rect(rect, ctm);
	bbox = fz_round_rect(rect);

	pix = fz_new_pixmap_with_bbox(ctx, cs, bbox, seps, alpha);
	if (alpha)
		fz_clear_pixmap(ctx, pix);
	else
		fz_clear_pixmap_with_value(ctx, pix, 0xFF);

	fz_try(ctx)
	{
		dev = fz_new_draw_device(ctx, ctm, pix);
		fz_run_display_list(ctx, list, dev, fz_identity, fz_infinite_rect, NULL);
		fz_close_device(ctx, dev);
	}
	fz_always(ctx)
	{
		fz_drop_device(ctx, dev);
	}
	fz_catch(ctx)
	{
		fz_drop_pixmap(ctx, pix);
		fz_rethrow(ctx);
	}

	return pix;
}

void lock_mutex(void* user, int lock)
{
	mutex_holder* mutex = (mutex_holder*)user;

	switch (lock)
	{
	case 0:
		mutex->mutex0.lock();
		break;
	case 1:
		mutex->mutex1.lock();
		break;
	case 2:
		mutex->mutex2.lock();
		break;
	default:
		mutex->mutex3.lock();
		break;
	}
}

void unlock_mutex(void* user, int lock)
{
	mutex_holder* mutex = (mutex_holder*)user;

	switch (lock)
	{
	case 0:
		mutex->mutex0.unlock();
		break;
	case 1:
		mutex->mutex1.unlock();
		break;
	case 2:
		mutex->mutex2.unlock();
		break;
	default:
		mutex->mutex3.unlock();
		break;
	}
}

extern "C"
{
	DLL_PUBLIC int GetStructuredTextChar(fz_stext_char* character, int* out_c, int* out_color, float* out_origin_x, float* out_origin_y, float* out_size, float* out_ll_x, float* out_ll_y, float* out_ul_x, float* out_ul_y, float* out_ur_x, float* out_ur_y, float* out_lr_x, float* out_lr_y)
	{
		*out_c = character->c;

		*out_color = character->color;

		*out_origin_x = character->origin.x;
		*out_origin_y = character->origin.y;

		*out_size = character->size;

		*out_ll_x = character->quad.ll.x;
		*out_ll_y = character->quad.ll.y;

		*out_ul_x = character->quad.ul.x;
		*out_ul_y = character->quad.ul.y;

		*out_ur_x = character->quad.ur.x;
		*out_ur_y = character->quad.ur.y;

		*out_lr_x = character->quad.lr.x;
		*out_lr_y = character->quad.lr.y;

		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int GetStructuredTextChars(fz_stext_line* line, fz_stext_char** out_chars)
	{
		int count = 0;

		fz_stext_char* curr_char = line->first_char;

		while (curr_char != nullptr)
		{
			out_chars[count] = curr_char;
			count++;
			curr_char = curr_char->next;
		}

		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int GetStructuredTextLine(fz_stext_line* line, int* out_wmode, float* out_x0, float* out_y0, float* out_x1, float* out_y1, float* out_x, float* out_y, int* out_char_count)
	{
		*out_wmode = line->wmode;

		*out_x0 = line->bbox.x0;
		*out_y0 = line->bbox.y0;
		*out_x1 = line->bbox.x1;
		*out_y1 = line->bbox.y1;

		*out_x = line->dir.x;
		*out_y = line->dir.y;

		int count = 0;

		fz_stext_char* curr_char = line->first_char;

		while (curr_char != nullptr)
		{
			count++;
			curr_char = curr_char->next;
		}

		*out_char_count = count;

		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int GetStructuredTextLines(fz_stext_block* block, fz_stext_line** out_lines)
	{
		int count = 0;

		fz_stext_line* curr_line = block->u.t.first_line;

		while (curr_line != nullptr)
		{
			out_lines[count] = curr_line;
			count++;
			curr_line = curr_line->next;
		}

		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int GetStructuredTextBlock(fz_stext_block* block, int* out_type, float* out_x0, float* out_y0, float* out_x1, float* out_y1, int* out_line_count)
	{
		*out_type = block->type;

		*out_x0 = block->bbox.x0;
		*out_y0 = block->bbox.y0;
		*out_x1 = block->bbox.x1;
		*out_y1 = block->bbox.y1;

		if (block->type == FZ_STEXT_BLOCK_IMAGE)
		{
			*out_line_count = 0;
		}
		else if (block->type == FZ_STEXT_BLOCK_TEXT)
		{
			int count = 0;

			fz_stext_line* curr_line = block->u.t.first_line;

			while (curr_line != nullptr)
			{
				count++;
				curr_line = curr_line->next;
			}

			*out_line_count = count;
		}

		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int GetStructuredTextBlocks(fz_stext_page* page, fz_stext_block** out_blocks)
	{
		fz_stext_block* curr_block = page->first_block;

		int count = 0;

		while (curr_block != nullptr)
		{
			out_blocks[count] = curr_block;
			count++;
			curr_block = curr_block->next;
		}

		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int GetStructuredTextPage(fz_context* ctx, fz_display_list* list, fz_stext_page** out_page, int* out_stext_block_count)
	{
		fz_stext_page* page;
		fz_stext_options options;
		fz_device* device;

		fz_try(ctx)
		{
			page = fz_new_stext_page(ctx, fz_infinite_rect);
		}
		fz_catch(ctx)
		{
			return ERR_CANNOT_CREATE_PAGE;
		}

		fz_try(ctx)
		{
			device = fz_new_stext_device(ctx, page, &options);
			fz_run_display_list(ctx, list, device, fz_identity, fz_infinite_rect, NULL);
			fz_close_device(ctx, device);
		}
		fz_always(ctx)
		{
			fz_drop_device(ctx, device);
		}
		fz_catch(ctx)
		{
			return ERR_CANNOT_POPULATE_PAGE;
		}

		*out_page = page;

		int count = 0;

		fz_stext_block* curr_block = page->first_block;

		while (curr_block != nullptr)
		{
			count++;
			curr_block = curr_block->next;
		}

		*out_stext_block_count = count;

		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int DisposeStructuredTextPage(fz_context* ctx, fz_stext_page* page)
	{
		fz_drop_stext_page(ctx, page);
		return EXIT_SUCCESS;
	}


	DLL_PUBLIC int FinalizeDocumentWriter(fz_context* ctx, fz_document_writer* writ)
	{
		fz_try(ctx)
		{
			fz_close_document_writer(ctx, writ);
		}
		fz_catch(ctx)
		{
			return ERR_CANNOT_CLOSE_DOCUMENT;
		}

		fz_drop_document_writer(ctx, writ);

		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int WriteSubDisplayListAsPage(fz_context* ctx, fz_display_list* list, float x0, float y0, float x1, float y1, float zoom, fz_document_writer* writ)
	{
		fz_device* dev;
		fz_rect rect;
		fz_matrix ctm;

		ctm = fz_concat(fz_translate(-x0, -y0), fz_scale(zoom, zoom));

		rect.x0 = x0;
		rect.y0 = y0;
		rect.x1 = x1;
		rect.y1 = y1;

		rect = fz_transform_rect(rect, ctm);

		fz_var(dev);

		fz_try(ctx)
		{
			dev = fz_begin_page(ctx, writ, rect);
			fz_run_display_list(ctx, list, dev, ctm, fz_infinite_rect, NULL);
			fz_end_page(ctx, writ);
		}
		fz_catch(ctx)
		{
			return ERR_CANNOT_RENDER;
		}

		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int CreateDocumentWriter(fz_context* ctx, const char* file_name, int format, const fz_document_writer** out_document_writer)
	{
		fz_document_writer* writ;

		fz_try(ctx)
		{
			switch (format)
			{
			case OUT_DOC_PDF:
				writ = fz_new_document_writer(ctx, file_name, "pdf", NULL);
				break;
			case OUT_DOC_SVG:
				writ = fz_new_document_writer(ctx, file_name, "svg", NULL);
				break;
			case OUT_DOC_CBZ:
				writ = fz_new_document_writer(ctx, file_name, "cbz", NULL);
				break;
			}
		}
		fz_catch(ctx)
		{
			return ERR_CANNOT_CREATE_WRITER;
		}

		*out_document_writer = writ;

		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int WriteImage(fz_context* ctx, fz_display_list* list, float x0, float y0, float x1, float y1, float zoom, int colorFormat, int output_format, const fz_buffer** out_buffer, const unsigned char** out_data, size_t* out_length)
	{
		fz_matrix ctm;
		fz_pixmap* pix;
		fz_output* out;
		fz_buffer* buf;
		fz_rect rect;
		int alpha;
		fz_colorspace* cs;

		ctm = fz_scale(zoom, zoom);

		rect.x0 = x0;
		rect.y0 = y0;
		rect.x1 = x1;
		rect.y1 = y1;

		fz_var(out);
		fz_var(buf);

		fz_try(ctx)
		{
			buf = fz_new_buffer(ctx, 1024);
			out = fz_new_output_with_buffer(ctx, buf);
		}
		fz_catch(ctx)
		{
			fz_drop_buffer(ctx, buf);
			return ERR_CANNOT_CREATE_BUFFER;
		}

		switch (colorFormat)
		{
		case COLOR_RGB:
			cs = fz_device_rgb(ctx);
			alpha = 0;
			break;
		case COLOR_RGBA:
			cs = fz_device_rgb(ctx);
			alpha = 1;
			break;
		case COLOR_BGR:
			cs = fz_device_bgr(ctx);
			alpha = 0;
			break;
		case COLOR_BGRA:
			cs = fz_device_bgr(ctx);
			alpha = 1;
			break;
		}


		//Render page to an RGB/RGBA pixmap.
		fz_try(ctx)
		{
			pix = new_pixmap_from_display_list_with_separations_bbox(ctx, list, rect, ctm, cs, NULL, alpha);
		}
		fz_catch(ctx)
		{
			fz_drop_output(ctx, out);
			fz_drop_buffer(ctx, buf);
			return ERR_CANNOT_RENDER;
		}

		//Write the rendered pixmap to the output buffer in the specified format.
		fz_try(ctx)
		{
			switch (output_format)
			{
			case OUT_PNM:
				fz_write_pixmap_as_pnm(ctx, out, pix);
				break;
			case OUT_PAM:
				fz_write_pixmap_as_pam(ctx, out, pix);
				break;
			case OUT_PNG:
				fz_write_pixmap_as_png(ctx, out, pix);
				break;
			case OUT_PSD:
				fz_write_pixmap_as_psd(ctx, out, pix);
				break;
			}
		}
		fz_catch(ctx)
		{
			fz_drop_output(ctx, out);
			fz_drop_buffer(ctx, buf);
			fz_drop_pixmap(ctx, pix);
			return ERR_CANNOT_SAVE;
		}

		fz_close_output(ctx, out);
		fz_drop_output(ctx, out);
		fz_drop_pixmap(ctx, pix);

		*out_buffer = buf;
		*out_data = buf->data;
		*out_length = buf->len;

		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int DisposeBuffer(fz_context* ctx, fz_buffer* buf)
	{
		fz_drop_buffer(ctx, buf);
		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int SaveImage(fz_context* ctx, fz_display_list* list, float x0, float y0, float x1, float y1, float zoom, int colorFormat, const char* file_name, int output_format)
	{
		fz_matrix ctm;
		fz_pixmap* pix;
		fz_rect rect;
		int alpha;
		fz_colorspace* cs;

		switch (colorFormat)
		{
		case COLOR_RGB:
			cs = fz_device_rgb(ctx);
			alpha = 0;
			break;
		case COLOR_RGBA:
			cs = fz_device_rgb(ctx);
			alpha = 1;
			break;
		case COLOR_BGR:
			cs = fz_device_bgr(ctx);
			alpha = 0;
			break;
		case COLOR_BGRA:
			cs = fz_device_bgr(ctx);
			alpha = 1;
			break;
		}


		ctm = fz_scale(zoom, zoom);

		rect.x0 = x0;
		rect.y0 = y0;
		rect.x1 = x1;
		rect.y1 = y1;

		//Render page to an RGB/RGBA pixmap.
		fz_try(ctx)
		{
			pix = new_pixmap_from_display_list_with_separations_bbox(ctx, list, rect, ctm, cs, NULL, alpha);
		}
		fz_catch(ctx)
		{
			return ERR_CANNOT_RENDER;
		}

		//Save the rendered pixmap to the output file in the specified format.
		fz_try(ctx)
		{
			switch (output_format)
			{
			case OUT_PNM:
				fz_save_pixmap_as_pnm(ctx, pix, file_name);
				break;
			case OUT_PAM:
				fz_save_pixmap_as_pam(ctx, pix, file_name);
				break;
			case OUT_PNG:
				fz_save_pixmap_as_png(ctx, pix, file_name);
				break;
			case OUT_PSD:
				fz_save_pixmap_as_psd(ctx, pix, file_name);
				break;
			}
		}
		fz_catch(ctx)
		{
			fz_drop_pixmap(ctx, pix);
			return ERR_CANNOT_SAVE;
		}

		fz_drop_pixmap(ctx, pix);

		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int CloneContext(fz_context* ctx, int count, fz_context** out_contexts)
	{
		for (int i = 0; i < count; i++)
		{
			fz_try(ctx)
			{
				fz_context* curr_ctx = fz_clone_context(ctx);
				fz_var(curr_ctx);

				out_contexts[i] = curr_ctx;

				if (!curr_ctx)
				{
					for (int j = 0; j < i; j++)
					{
						fz_drop_context(out_contexts[j]);
					}
					return ERR_CANNOT_CLONE_CONTEXT;
				}
			}
			fz_catch(ctx)
			{
				for (int j = 0; j < i; j++)
				{
					fz_drop_context(out_contexts[j]);
				}
				return ERR_CANNOT_CLONE_CONTEXT;
			}
		}

		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int RenderSubDisplayList(fz_context* ctx, fz_display_list* list, float x0, float y0, float x1, float y1, float zoom, int colorFormat, unsigned char* pixel_storage, fz_cookie* cookie)
	{
		if (cookie != NULL && cookie->abort)
		{
			return EXIT_SUCCESS;
		}

		fz_matrix ctm;
		fz_pixmap* pix;
		fz_rect rect;
		int alpha;
		fz_colorspace* cs;
		switch (colorFormat)
		{
		case COLOR_RGB:
			cs = fz_device_rgb(ctx);
			alpha = 0;
			break;
		case COLOR_RGBA:
			cs = fz_device_rgb(ctx);
			alpha = 1;
			break;
		case COLOR_BGR:
			cs = fz_device_bgr(ctx);
			alpha = 0;
			break;
		case COLOR_BGRA:
			cs = fz_device_bgr(ctx);
			alpha = 1;
			break;
		}

		ctm = fz_scale(zoom, zoom);

		rect.x0 = x0;
		rect.y0 = y0;
		rect.x1 = x1;
		rect.y1 = y1;

		//Render page to an RGB/RGBA pixmap.
		fz_try(ctx)
		{
			pix = new_pixmap_from_display_list_with_separations_bbox_and_data(ctx, list, rect, ctm, cs, NULL, alpha, pixel_storage, cookie);
		}
		fz_catch(ctx)
		{

			return ERR_CANNOT_RENDER;
		}

		fz_drop_pixmap(ctx, pix);

		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int GetDisplayList(fz_context* ctx, fz_page* page, int annotations, fz_display_list** out_display_list, float* out_x0, float* out_y0, float* out_x1, float* out_y1)
	{
		fz_display_list* list;
		fz_rect bounds;
		fz_device* bbox;

		fz_try(ctx)
		{
			if (annotations == 1)
			{
				list = fz_new_display_list_from_page(ctx, page);
			}
			else
			{
				list = fz_new_display_list_from_page_contents(ctx, page);
			}
		}
		fz_catch(ctx)
		{
			return ERR_CANNOT_RENDER;
		}

		fz_var(bbox);

		fz_try(ctx)
		{
			bbox = fz_new_bbox_device(ctx, &bounds);
			fz_run_display_list(ctx, list, bbox, fz_identity, fz_infinite_rect, NULL);
			fz_close_device(ctx, bbox);
		}
		fz_always(ctx)
		{
			fz_drop_device(ctx, bbox);
		}
		fz_catch(ctx)
		{
			return ERR_CANNOT_COMPUTE_BOUNDS;
		}

		*out_display_list = list;

		*out_x0 = bounds.x0;
		*out_y0 = bounds.y0;
		*out_x1 = bounds.x1;
		*out_y1 = bounds.y1;

		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int DisposeDisplayList(fz_context* ctx, fz_display_list* list)
	{
		fz_drop_display_list(ctx, list);
		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int LoadPage(fz_context* ctx, fz_document* doc, int page_number, const fz_page** out_page, float* out_x, float* out_y, float* out_w, float* out_h)
	{
		fz_page* page;
		fz_rect bounds;

		fz_try(ctx)
		{
			page = fz_load_page(ctx, doc, page_number);
		}
		fz_catch(ctx)
		{
			return ERR_CANNOT_LOAD_PAGE;
		}

		fz_try(ctx)
		{
			bounds = fz_bound_page(ctx, page);
		}
		fz_catch(ctx)
		{
			fz_drop_page(ctx, page);
			return ERR_CANNOT_COMPUTE_BOUNDS;
		}

		*out_x = bounds.x0;
		*out_y = bounds.y0;
		*out_w = bounds.x1 - bounds.x0;
		*out_h = bounds.y1 - bounds.y0;

		*out_page = page;

		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int DisposePage(fz_context* ctx, fz_page* page)
	{
		fz_drop_page(ctx, page);
		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int CreateDocumentFromFile(fz_context* ctx, const char* file_name, int get_image_resolution, const fz_document** out_doc, int* out_page_count, float* out_image_xres, float* out_image_yres)
	{
		if (get_image_resolution != 0)
		{
			fz_image* img;

			fz_try(ctx)
			{
				img = fz_new_image_from_file(ctx, file_name);

				if (img != nullptr)
				{
					*out_image_xres = img->xres;
					*out_image_yres = img->yres;
				}
				else
				{
					*out_image_xres = -1;
					*out_image_yres = -1;
				}

				fz_drop_image(ctx, img);
			}
			fz_catch(ctx)
			{
				*out_image_xres = -1;
				*out_image_yres = -1;
			}
		}
		else
		{
			*out_image_xres = -1;
			*out_image_yres = -1;
		}

		fz_document* doc;

		//Open the document.
		fz_try(ctx)
		{
			doc = fz_open_document(ctx, file_name);
		}
		fz_catch(ctx)
		{
			return ERR_CANNOT_OPEN_FILE;
		}

		//Count the number of pages.
		fz_try(ctx)
		{
			*out_page_count = fz_count_pages(ctx, doc);
		}
		fz_catch(ctx)
		{
			fz_drop_document(ctx, doc);
			return ERR_CANNOT_COUNT_PAGES;
		}

		*out_doc = doc;

		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int CreateDocumentFromStream(fz_context* ctx, const unsigned char* data, const size_t data_length, const char* file_type, int get_image_resolution, const fz_document** out_doc, const fz_stream** out_str, int* out_page_count, float* out_image_xres, float* out_image_yres)
	{
		
		fz_stream* str;
		fz_document* doc;

		fz_try(ctx)
		{
			str = fz_open_memory(ctx, data, data_length);
		}
		fz_catch(ctx)
		{
			return ERR_CANNOT_OPEN_STREAM;
		}

		if (get_image_resolution != 0)
		{
			fz_image* img;
			fz_buffer* img_buf;

			int bufferCreated = 0;

			fz_try(ctx)
			{
				img_buf = fz_new_buffer_from_shared_data(ctx, data, data_length);
				bufferCreated = 1;
			}
			fz_catch(ctx)
			{
				bufferCreated = 0;
			}

			if (bufferCreated == 1)
			{
				fz_try(ctx)
				{
					img = fz_new_image_from_buffer(ctx, img_buf);

					if (img != nullptr)
					{
						*out_image_xres = img->xres;
						*out_image_yres = img->yres;
					}
					else
					{
						*out_image_xres = -1;
						*out_image_yres = -1;
					}

					fz_drop_image(ctx, img);
				}
				fz_catch(ctx)
				{
					*out_image_xres = -1;
					*out_image_yres = -1;
				}

				fz_drop_buffer(ctx, img_buf);
			}
			else
			{
				*out_image_xres = -1;
				*out_image_yres = -1;
			}
		}
		else
		{
			*out_image_xres = -1;
			*out_image_yres = -1;
		}

		//Open the document.
		fz_try(ctx)
		{
			doc = fz_open_document_with_stream(ctx, file_type, str);
		}
		fz_catch(ctx)
		{

			fz_drop_stream(ctx, str);
			return ERR_CANNOT_OPEN_FILE;
		}

		//Count the number of pages.
		fz_try(ctx)
		{
			*out_page_count = fz_count_pages(ctx, doc);
		}
		fz_catch(ctx)
		{

			fz_drop_document(ctx, doc);
			fz_drop_stream(ctx, str);
			return ERR_CANNOT_COUNT_PAGES;
		}

		*out_str = str;
		*out_doc = doc;

		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int DisposeStream(fz_context* ctx, fz_stream* str)
	{
		fz_drop_stream(ctx, str);
		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int DisposeDocument(fz_context* ctx, fz_document* doc)
	{
		fz_drop_document(ctx, doc);
		return EXIT_SUCCESS;
	}

	DLL_PUBLIC size_t GetCurrentStoreSize(const fz_context* ctx)
	{
		return ctx->store->size;
	}

	DLL_PUBLIC size_t GetMaxStoreSize(const fz_context* ctx)
	{
		return ctx->store->max;
	}

	DLL_PUBLIC int ShrinkStore(fz_context* ctx, unsigned int perc)
	{
		return fz_shrink_store(ctx, perc);
	}

	DLL_PUBLIC void EmptyStore(fz_context* ctx)
	{
		fz_empty_store(ctx);
	}

	DLL_PUBLIC int CreateContext(long store_size, const fz_context** out_ctx)
	{
		fz_context* ctx;
		fz_locks_context locks;

		//Create lock objects necessary for multithreaded context operations.
		locks.user = &global_mutex;
		locks.lock = lock_mutex;
		locks.unlock = unlock_mutex;

		lock_mutex(locks.user, 0);
		unlock_mutex(locks.user, 0);

		//Create a context to hold the exception stack and various caches.
		ctx = fz_new_context(NULL, &locks, store_size);
		if (!ctx)
		{
			return ERR_CANNOT_CREATE_CONTEXT;
		}

		fz_var(ctx);

		//Register the default file types to handle.
		fz_try(ctx)
		{
			fz_register_document_handlers(ctx);
		}
		fz_catch(ctx)
		{
			fz_drop_context(ctx);

			return ERR_CANNOT_REGISTER_HANDLERS;
		}

		*out_ctx = ctx;

		return EXIT_SUCCESS;
	}

	DLL_PUBLIC int DisposeContext(fz_context* ctx)
	{
		fz_drop_context(ctx);
		return EXIT_SUCCESS;
	}
}