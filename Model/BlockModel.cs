﻿using OpenTK;
using System;
using System.Drawing;
using ClassicalSharp.GraphicsAPI;
using ClassicalSharp.Renderers;

namespace ClassicalSharp.Model {

	public class BlockModel : IModel {
		
		byte block = (byte)Block.Air;
		public BlockModel( Game window ) : base( window ) {
			vertices = new VertexPos3fTex2fCol4b[6 * 6];
		}
		
		public override float NameYOffset {
			get { return blockHeight + 0.075f; }
		}
		
		protected override void DrawPlayerModel( Player player, PlayerRenderer renderer ) {
			graphics.Texturing = true;
			graphics.AlphaTest = true;
			block = Byte.Parse( player.ModelName );
			if( block == 0 ) {
				blockHeight = 1;
				return;
			}
			
			graphics.Bind2DTexture( window.TerrainAtlasTexId );
			blockHeight = window.BlockInfo.BlockHeight( block );
			atlas = window.TerrainAtlas;
			BlockInfo = window.BlockInfo;
			index = 0;
			if( BlockInfo.IsSprite( block ) ) {
				DrawXFace( 0f, TileSide.Right, false );
				DrawZFace( 0f, TileSide.Back, false );
				graphics.DrawVertices( DrawMode.Triangles, vertices, 6 * 2 );
			} else {
				DrawYFace( blockHeight, TileSide.Top );
				DrawXFace( -0.5f, TileSide.Right, false );
				DrawXFace( 0.5f, TileSide.Left, true );
				DrawZFace( -0.5f, TileSide.Front, true );
				DrawZFace( 0.5f, TileSide.Back, false );
				DrawYFace( 0f, TileSide.Bottom );
				graphics.DrawVertices( DrawMode.Triangles, vertices, 6 * 6 );
			}
		}
		float blockHeight;
		TextureAtlas2D atlas;
		BlockInfo BlockInfo;
		
		public override void Dispose() {
		}
		
		void DrawYFace( float y, int side ) {
			int texId = BlockInfo.GetOptimTextureLoc( block, side );
			TextureRectangle rec = atlas.GetTexRec( texId );

			vertices[index++] = new VertexPos3fTex2fCol4b( -0.5f, y, -0.5f, rec.U1, rec.V1, col );
			vertices[index++] = new VertexPos3fTex2fCol4b( 0.5f, y, -0.5f, rec.U2, rec.V1, col );
			vertices[index++] = new VertexPos3fTex2fCol4b( 0.5f, y, 0.5f, rec.U2, rec.V2, col );
			
			vertices[index++] = new VertexPos3fTex2fCol4b( 0.5f, y, 0.5f, rec.U2, rec.V2, col );
			vertices[index++] = new VertexPos3fTex2fCol4b( -0.5f, y, 0.5f, rec.U1, rec.V2, col );
			vertices[index++] = new VertexPos3fTex2fCol4b( -0.5f, y, -0.5f, rec.U1, rec.V1, col );
		}

		void DrawZFace( float z, int side, bool swapU ) {
			int texId = BlockInfo.GetOptimTextureLoc( block, side );
			TextureRectangle rec = atlas.GetTexRec( texId );
			if( blockHeight != 1 ) {
				rec.V2 = rec.V1 + blockHeight * atlas.invVerElementSize;
			}
			if( swapU ) rec.SwapU();
			
			vertices[index++] = new VertexPos3fTex2fCol4b( -0.5f, 0f, z, rec.U1, rec.V2, col );
			vertices[index++] = new VertexPos3fTex2fCol4b( -0.5f, blockHeight, z, rec.U1, rec.V1, col );
			vertices[index++] = new VertexPos3fTex2fCol4b( 0.5f, blockHeight, z, rec.U2, rec.V1, col );
			
			vertices[index++] = new VertexPos3fTex2fCol4b( 0.5f, blockHeight, z, rec.U2, rec.V1, col );
			vertices[index++] = new VertexPos3fTex2fCol4b( 0.5f, 0f, z, rec.U2, rec.V2, col );
			vertices[index++] = new VertexPos3fTex2fCol4b( -0.5f, 0f, z, rec.U1, rec.V2, col );
		}

		void DrawXFace( float x, int side, bool swapU ) {
			int texId = BlockInfo.GetOptimTextureLoc( block, side );
			TextureRectangle rec = atlas.GetTexRec( texId );
			if( blockHeight != 1 ) {
				rec.V2 = rec.V1 + blockHeight * atlas.invVerElementSize;
			}
			if( swapU ) rec.SwapU();
			
			vertices[index++] = new VertexPos3fTex2fCol4b( x, 0f, -0.5f, rec.U1, rec.V2, col );
			vertices[index++] = new VertexPos3fTex2fCol4b( x, blockHeight, -0.5f, rec.U1, rec.V1, col );
			vertices[index++] = new VertexPos3fTex2fCol4b( x, blockHeight, 0.5f, rec.U2, rec.V1, col );
			
			vertices[index++] = new VertexPos3fTex2fCol4b( x, blockHeight, 0.5f, rec.U2, rec.V1, col );
			vertices[index++] = new VertexPos3fTex2fCol4b( x, 0f, 0.5f, rec.U2, rec.V2, col );
			vertices[index++] = new VertexPos3fTex2fCol4b( x, 0f, -0.5f, rec.U1, rec.V2, col );
		}
	}
}