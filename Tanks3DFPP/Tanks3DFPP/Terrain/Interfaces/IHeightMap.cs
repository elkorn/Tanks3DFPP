using Microsoft.Xna.Framework;
using Tanks3DFPP.Terrain.Interfaces;

namespace Tanks3DFPP.Terrain
{
    public interface IHeightMap
    {
        float[,] Data { get; }
        int Width { get; }
        int Height { get; }
        int HeightOffset { get; }
        int HighestPeak { get; }

        Vector3? Intersection(Ray ray, int scale = 1);
    }
}
