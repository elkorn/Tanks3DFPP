using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tanks3DFPP.Terrain
{
    public interface IHeightMap
    {
        float[,] Data { get; }
        int Width { get; }
        int Height { get; }
        int HeightOffset { get; }
        int HighestPeak { get; }
    }
}
