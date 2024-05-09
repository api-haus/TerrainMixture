using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace TerrainMixture.Runtime.Grid
{
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct TileCoordinate : IEquatable<TileCoordinate>, IComparable<TileCoordinate>
	{
		public int x;
		public int y;

		public TileCoordinate(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public static TileCoordinate operator +(TileCoordinate left, TileCoordinate right)
		{
			return new TileCoordinate(right.x + left.x, right.y + left.y);
		}

		public static TileCoordinate ToTile(float3 observer, float tileSize)
		{
			var coord = (int3)math.floor(observer / tileSize);

			return new(coord.x, coord.z);
		}

		public bool Equals(TileCoordinate other)
		{
			return x == other.x && y == other.y;
		}

		public override bool Equals(object obj)
		{
			return obj is TileCoordinate other && Equals(other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(x, y);
		}

		public List<TileCoordinate> Grow(int vastness)
		{
			List<TileCoordinate> growth = new();

			for (int i = 0; i < math.pow(8, vastness); i++)
			{
				growth.Add(ModifyCellVastness(this, i, vastness));
			}

			return growth;
		}

		TileCoordinate ModifyCellVastness(TileCoordinate tileCoordinate, int i, int vastness)
		{
			// TODO:
			if (vastness == 0)
			{
				return tileCoordinate;
			}

			if (vastness == 1)
				switch (i)
				{
					case 0:
						return tileCoordinate + new TileCoordinate(+0, +1);
					case 1:
						return tileCoordinate + new TileCoordinate(+1, +1);
					case 2:
						return tileCoordinate + new TileCoordinate(+1, 0);
					case 3:
						return tileCoordinate + new TileCoordinate(+1, -1);
					case 4:
						return tileCoordinate + new TileCoordinate(+0, -1);
					case 5:
						return tileCoordinate + new TileCoordinate(-1, -1);
					case 6:
						return tileCoordinate + new TileCoordinate(-1, +0);
					case 7:
						return tileCoordinate + new TileCoordinate(-1, +1);
				}

			throw new ArgumentOutOfRangeException("not implemented yet");
		}

		public readonly Vector3 ToWorldPosition(float tileSize)
		{
			return new Vector3(x * tileSize, 0, y * tileSize);
		}

		public readonly Vector3 ToWorldOrigin()
		{
			return new Vector3(x, 0, y);
		}

		public int CompareTo(TileCoordinate other)
		{
			var xComparison = x.CompareTo(other.x);
			if (xComparison != 0)
			{
				return xComparison;
			}

			return y.CompareTo(other.y);
		}
	}
}
