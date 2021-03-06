#ifndef CS_BITMAP_h
#define CS_BITMAP_H
#include "Typedefs.h"
/* Represents a 2D array of pixels.
   Copyright 2014-2017 ClassicalSharp | Licensed under BSD-3
*/

typedef struct Bitmap {
	/* Pointer to first scaneline. */
	UInt8* Scan0;

	/* Number of bytes in each scanline. */
	Int32 Stride;

	/* Number of pixels horizontally. */
	Int32 Width;

	/* Number of pixels vertically. */
	Int32 Height;
} Bitmap;

/* Constructs or updates a Bitmap instance. */
void Bitmap_Create(Bitmap* bmp, Int32 width, Int32 height, Int32 stride, UInt8* scan0);

/* Returns a pointer to the start of the y'th scanline. */
UInt32* Bitmap_GetRow(Bitmap* bmp, Int32 y);

/* Copies a block of pixels from one bitmap to another. */
void Bitmap_CopyBlock(Int32 srcX, Int32 srcY, Int32 dstX, Int32 dstY, Bitmap* src, Bitmap* dst, Int32 size);

/* Copies a row of pixels from one bitmap to another. */
void Bitmap_CopyRow(Int32 srcY, Int32 dstY, Bitmap* src, Bitmap* dst, Int32 width);
#endif