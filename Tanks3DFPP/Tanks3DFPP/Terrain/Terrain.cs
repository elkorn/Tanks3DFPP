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
using Tanks3DFPP.Utilities;


namespace Tanks3DFPP.Terrain
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Terrain
    {
        private IHeightMap heightMap;

        private IHeightToColorTranslationMethod coloringMethod;
        private VertexPositionColorNormal[] vertices;
        private int[] indices;

        /// <summary>
        /// Gets the width (X) of the terrain.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public int Width
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the height (Z) of the terrain.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public int Height
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Terrain" /> class.
        /// </summary>
        /// <param name="heightMap">The height map.</param>
        /// <param name="coloringMethod">The coloring method.</param>
        public Terrain(IHeightMap heightMap, IHeightToColorTranslationMethod coloringMethod)
        {
            this.Width = heightMap.Width;
            this.Height = heightMap.Height;
            this.heightMap = heightMap;
            this.coloringMethod = coloringMethod;
            this.SetUpVertices();
            this.SetUpIndices();
            this.CalculateNormals();
        }

        /// <summary>
        /// Calculates the value by which the terrain's height has to be offset.
        /// </summary>
        /// <returns>The minimal height present on the map.</returns>
        private float CalculateHeightOffset()
        {
            float result = this.heightMap.Data[0, 0];
            for (int x = 0; x < this.Width; ++x)
            {
                for (int y = 0; y < this.Height; ++y)
                {
                    if (this.heightMap.Data[x, y] < result)
                    {
                        result = this.heightMap.Data[0, 0];
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Sets up vertices.
        /// </summary>
        private void SetUpVertices()
        {
            float heightOffset = CalculateHeightOffset();
            this.vertices = new VertexPositionColorNormal[this.Width * this.Height];
            for (int x = 0; x < this.Width; ++x)
            {
                for (int y = 0; y < this.Height; ++y)
                {
                    this.vertices[x + y * this.Width].Position = new Vector3(x, this.heightMap.Data[x, y] - heightOffset, this.Height / 2 - y);
                    this.vertices[x + y * this.Width].Color = this.coloringMethod.Calculate((int)this.heightMap.Data[x, y]);
                    this.vertices[x + y * this.Width].Normal = new Vector3(0,0,0);
                }
            }
        }

        /// <summary>
        /// Calculates the normals for all vertices.
        /// </summary>
        private void CalculateNormals()
        {
            for (int i = 0; i < this.indices.Length / 3; ++i)
            {
                // the three vertices of a triangle
                int index1 = this.indices[3 * i],
                    index2 = this.indices[3 * i + 1],
                    index3 = this.indices[3 * i + 2];
                // The sides of the triangle that the light will shine on
                Vector3 side1 = this.vertices[index1].Position - this.vertices[index3].Position,
                        side2 = this.vertices[index1].Position - this.vertices[index2].Position;

                Vector3 normal = Vector3.Cross(side1, side2);
                normal.Normalize();
                this.vertices[index1].Normal += normal;
                this.vertices[index2].Normal += normal;
                this.vertices[index3].Normal += normal;
            }

            //for (int i = 0; i < this.vertices.Length; ++i)
            //{
            //    this.vertices[i].Normal.Normalize();
            //}
        }

        /// <summary>
        /// Sets up vertex indices.
        /// </summary>
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

        /// <summary>
        /// Renders the terrain to specified device.
        /// </summary>
        /// <param name="device">The device.</param>
        public void Render(GraphicsDevice device)
        {
            device.DrawUserIndexedPrimitives(
                PrimitiveType.TriangleList,
                this.vertices, 0,
                this.vertices.Length,
                this.indices,
                0,
                this.indices.Length / 3,
                VertexPositionColorNormal.VertexDeclaration);
        }

        //TODO: setUpIndices
    }
}

