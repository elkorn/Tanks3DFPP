using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Tanks3DFPP.Terrain;


namespace Tanks3DFPP.Terrain
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Terrain
    {
        private IHeightMap heightMap;
        
        private IHeightToColorTranslationMethod coloringMethod;
        private VertexPositionColor[] vertices;
        private VertexBuffer vertexBuffer;
        private int[] indices;

        public int Width
        {
            get;
            private set;
        }

        public int Height
        {
            get;
            private set;
        }

        public Terrain(IHeightMap heightMap, IHeightToColorTranslationMethod coloringMethod)
        {
            this.Width = heightMap.Width;
            this.Height = heightMap.Height;
            this.heightMap = heightMap;
            this.coloringMethod = coloringMethod;
            this.setUpVertices();
            this.SetUpIndices();
        }

        private void setUpVertices()
        {
            float min = this.heightMap.Data[0,0];
            for (int x = 0; x < this.Width; ++x)
            {
                for (int y = 0; y < this.Height; ++y)
                {
                    if (this.heightMap.Data[x, y] < min)
                    {
                        min = this.heightMap.Data[0, 0];
                    }
                }
            }

            this.vertices = new VertexPositionColor[this.Width * this.Height];
            for (int x = 0; x < this.Width; ++x)
            {
                for (int y = 0; y < this.Height; ++y)
                {
                    this.vertices[x + y * this.Width].Position = new Vector3(x, this.heightMap.Data[x,y] - min, this.Height / 2 -y);
                    this.vertices[x + y * this.Width].Color = this.coloringMethod.Calculate((int)this.heightMap.Data[x, y]);
                }
            }
        }

        private void SetUpIndices()
        {
            int height = (this.Height - 1),         // number of triangles in a row
                width = (this.Width - 1),           // number of rows of triangles within a height map
                index = 0;
            /* 
             * to draw only half of a rectangle in one place:
             * triangles/row * rows * 3 vertices for a spot, making only a triangular half filled.
             * this.indices = new int[height * width * 3];
             * 
             * To draw a full rectangle in one place:
             * triangles/row * rows * 6 vertices for a spot, placing two triangles in one spto, thus filling it completely
             * this.indices = new int[height * width * 6];
            */
            this.indices = new int[height * width * 6];
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    int bottomLeft = x + y * this.Width;
                    int bottomRight = (x + 1) + y * this.Width;
                    int topLeft = x + (y + 1) * this.Width;
                    int topRight = (x + 1) + (y + 1) * this.Width;

                    this.indices[index++] = topLeft;
                    this.indices[index++] = bottomRight;
                    this.indices[index++] = bottomLeft;

                    // The second half of the indexed spot:
                    this.indices[index++] = topLeft;
                    this.indices[index++] = topRight;
                    this.indices[index++] = bottomRight;
                }

            } 
        }

        public void Render(GraphicsDevice device)
        {
            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, this.vertices, 0, this.vertices.Length, this.indices, 0, this.indices.Length / 3, VertexPositionColor.VertexDeclaration);
        }

        //TODO: setUpIndices
    }
}
