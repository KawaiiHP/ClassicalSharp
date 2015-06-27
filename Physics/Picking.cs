﻿using System;
using System.Collections.Generic;
using OpenTK;

namespace ClassicalSharp {

	public static class Picking {
		
		// http://www.xnawiki.com/index.php/Voxel_traversal
		const float inf = Single.PositiveInfinity;
		public static void GetPickedBlockPos( Game window, Vector3 origin, Vector3 dir, float reach, PickedPos pickedPos ) {
			// Implementation is based on:
			// "A Fast Voxel Traversal Algorithm for Ray Tracing"
			// John Amanatides, Andrew Woo
			// http://www.cse.yorku.ca/~amana/research/grid.pdf
			// http://www.devmaster.net/articles/raytracing_series/A%20faster%20voxel%20traversal%20algorithm%20for%20ray%20tracing.pdf

			// NOTES:
			// * This code assumes that the ray's position and direction are in 'cell coordinates', which means
			//   that one unit equals one cell in all directions.
			// * When the ray doesn't start within the voxel grid, calculate the first position at which the
			//   ray could enter the grid. If it never enters the grid, there is nothing more to do here.
			// * Also, it is important to test when the ray exits the voxel grid when the grid isn't infinite.
			// * The Point3D structure is a simple structure having three integer fields (X, Y and Z).

			// The cell in which the ray starts.
			Vector3I start = Vector3I.Floor( origin ); // Rounds the position's X, Y and Z down to the nearest integer values.
			int x = start.X;
			int y = start.Y;
			int z = start.Z;

			// Determine which way we go.
			int stepX = Math.Sign( dir.X );
			int stepY = Math.Sign( dir.Y );
			int stepZ = Math.Sign( dir.Z );

			// Calculate cell boundaries. When the step (i.e. direction sign) is positive,
			// the next boundary is AFTER our current position, meaning that we have to add 1.
			// Otherwise, it is BEFORE our current position, in which case we add nothing.
			Vector3I cellBoundary = new Vector3I(
				x + ( stepX > 0 ? 1 : 0 ),
				y + ( stepY > 0 ? 1 : 0 ),
				z + ( stepZ > 0 ? 1 : 0 ) );

			// Determine how far we can travel along the ray before we hit a voxel boundary.
			Vector3 tMax = new Vector3(
				dir.X == 0 ? inf : ( cellBoundary.X - origin.X ) / dir.X,    // Boundary is a plane on the YZ axis.
				dir.Y == 0 ? inf : ( cellBoundary.Y - origin.Y ) / dir.Y,    // Boundary is a plane on the XZ axis.
				dir.Z == 0 ? inf : ( cellBoundary.Z - origin.Z ) / dir.Z );  // Boundary is a plane on the XY axis.

			// Determine how far we must travel along the ray before we have crossed a gridcell.
			Vector3 tDelta = new Vector3(
				dir.X == 0 ? inf : stepX / dir.X,     // Crossing the width of a cell.
				dir.Y == 0 ? inf : stepY / dir.Y,     // Crossing the height of a cell.
				dir.Z == 0 ? inf : stepZ / dir.Z );   // Crossing the depth of a cell.
			
			Map map = window.Map;
			BlockInfo info = window.BlockInfo;
			float reachSquared = reach * reach;
			int iterations = 0;

			// For each step, determine which distance to the next voxel boundary is lowest (i.e.
			// which voxel boundary is nearest) and walk that way.
			while( iterations < 10000 ) {
				Vector3 pos = new Vector3( x, y, z );
				if( Utils.DistanceSquared( pos, origin ) >= reachSquared ) {
					pickedPos.Valid = false;
					return;
				}
				iterations++;
				
				byte block;
				if( map.IsValidPos( x, y, z ) && ( block = map.GetBlock( x, y, z ) ) != 0 ) {
					bool cantPickBlock = !window.Inventory.CanPlace[block] && !window.Inventory.CanDelete[block] && info.IsLiquid( block );
					if( !cantPickBlock ) {
						// This cell falls on the path of the ray. Now perform an additional bounding box test,
						// since some blocks do not occupy a whole cell.
						float height = info.BlockHeight( block );
						Vector3 min = new Vector3( x, y, z );
						Vector3 max = new Vector3( x + 1, y + height, z + 1 );
						float t0, t1;
						if( IntersectionUtils.RayIntersectsBox( origin, dir, min, max, out t0, out t1 ) ) {
							pickedPos.UpdateBlockPos( min, max, origin, dir, t0, t1 );
							return;
						}
					}
				}
				
				if( tMax.X < tMax.Y && tMax.X < tMax.Z ) {
					// tMax.X is the lowest, an YZ cell boundary plane is nearest.
					x += stepX;
					tMax.X += tDelta.X;
				} else if( tMax.Y < tMax.Z ) {
					// tMax.Y is the lowest, an XZ cell boundary plane is nearest.
					y += stepY;
					tMax.Y += tDelta.Y;
				} else {
					// tMax.Z is the lowest, an XY cell boundary plane is nearest.
					z += stepZ;
					tMax.Z += tDelta.Z;
				}
			}
			throw new InvalidOperationException( "did over 10000 iterations in GetPickedBlockPos(). " +
			                                    "Something has gone wrong. (dir: " + dir + ")" );
		}
	}
	
	public class PickedPos {
		
		public Vector3 Min, Max;
		public Vector3I BlockPos;
		public Vector3I TranslatedPos;
		public bool Valid = true;
		
		public void UpdateBlockPos( Vector3 p1, Vector3 p2, Vector3 origin, Vector3 dir, float t0, float t1 ) {
			Min = Vector3.Min( p1, p2 );
			Max = Vector3.Max( p1, p2 );
			BlockPos = Vector3I.Truncate( Min );
			Valid = true;
			
			Vector3I normal = Vector3I.Zero;
			Vector3 intersect = origin + dir * t0;
			float dist = float.PositiveInfinity;
			TestAxis( intersect.X - Min.X, ref dist, -Vector3I.UnitX, ref normal );
			TestAxis( intersect.X - Max.X, ref dist, Vector3I.UnitX, ref normal );
			TestAxis( intersect.Y - Min.Y, ref dist, -Vector3I.UnitY, ref normal );
			TestAxis( intersect.Y - Max.Y, ref dist, Vector3I.UnitY, ref normal );
			TestAxis( intersect.Z - Min.Z, ref dist, -Vector3I.UnitZ, ref normal );
			TestAxis( intersect.Z - Max.Z, ref dist, Vector3I.UnitZ, ref normal );
			TranslatedPos = BlockPos + normal;
		}
		
		static void TestAxis( float dAxis, ref float dist, Vector3I nAxis, ref Vector3I normal ) {
			dAxis = Math.Abs( dAxis );
			if( dAxis < dist ) {
				dist = dAxis;
				normal = nAxis;
			}
		}
	}
}