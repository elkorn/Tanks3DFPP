using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tanks3DFPP.Utilities
{
    public struct VertexPositionColorNormal
    {
        public Vector3 Position;
        public Color Color;
        public Vector3 Normal;

        public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
        (
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(3* sizeof(float), VertexElementFormat.Color, VertexElementUsage.Color, 0),
            new VertexElement(3* sizeof(float) + 4, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
        );
    }
}
