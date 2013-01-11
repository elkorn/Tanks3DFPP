using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tanks3DFPP.Terrain.Optimization.QuadTree
{
    /// <summary>
    ///  Represents the vertices for each quad node
    /// </summary>
    public struct QuadNodeVertex
    {
        /// <summary>
        /// The index of this vertex in the vertex array (VB index).
        /// </summary>
        public int Index;

        /// <summary>
        /// Determines wherther this vertex should be rendered.
        /// </summary>
        public bool Activated;
    }
}
