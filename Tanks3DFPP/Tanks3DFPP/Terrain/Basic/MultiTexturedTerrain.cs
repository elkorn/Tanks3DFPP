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
    public class MultiTexturedTerrain : IDisposable
    {
        private IHeightMap heightMap;

        private Texture sand, grass, rock, snow;
        private VertexMultitextured[] vertices;
        private int[] indices;
        private Effect effect;

        private int scale;

        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;

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
        /// Initializes a new instance of the <see cref="ColoredTerrain" /> class.
        /// </summary>
        /// <param name="heightMap">The height map.</param>
        /// <param name="coloringMethod">The coloring method.</param>
        public MultiTexturedTerrain(GraphicsDevice device, ContentManager content, IHeightMap heightMap, int scale = 1)
        {
            this.effect = content.Load<Effect>("Multitexture");
            this.sand = content.Load<Texture>("sand");
            this.grass = content.Load<Texture>("grass");
            this.rock = content.Load<Texture>("rock");
            this.snow = content.Load<Texture>("snow");
            this.InitializeEffect();


            this.Width = heightMap.Width;
            this.Height = heightMap.Height;
            this.heightMap = heightMap;

            this.scale = scale;

            this.SetUpVertices();

            this.SetUpIndices();
            this.CalculateNormals();
            this.BufferVertexData(device);
            this.BufferIndexData(device);
        }

        private void InitializeEffect()
        {
            Vector3 lightDir = new Vector3(1, -1, -1);
            lightDir.Normalize();
            effect.CurrentTechnique = effect.Techniques["MultiTextured"];
            effect.Parameters["xTexture0"].SetValue(sand);
            effect.Parameters["xTexture1"].SetValue(grass);
            effect.Parameters["xTexture2"].SetValue(rock);
            effect.Parameters["xTexture3"].SetValue(snow);
            effect.Parameters["xEnableLighting"].SetValue(true);
            effect.Parameters["xAmbient"].SetValue(0.1f);
            effect.Parameters["xLightDirection"].SetValue(lightDir);
        }

        /// <summary>
        /// Switches the lighting.
        /// </summary>
        public void SwitchLighting()
        {
            effect.Parameters["xEnableLighting"].SetValue(!effect.Parameters["xEnableLighting"].GetValueBoolean());
        }

        public void SwitchBlending(bool? value)
        {
            if (!value.HasValue)
            {
                value = !effect.Parameters["xEnableBlending"].GetValueBoolean();
            }

            effect.Parameters["xEnableBlending"].SetValue(value.Value);
        }

        /// <summary>
        /// Sets up vertices.
        /// </summary>
        private void SetUpVertices()
        {
            this.vertices = new VertexMultitextured[this.Width * this.Height];
            float minSandHeight = 0,
                  minGrassHeight = 0.4f * this.heightMap.HighestPeak,
                  minRockHeight = (2 / 3f) * this.heightMap.HighestPeak,
                  minSnowHeight = this.heightMap.HighestPeak,
                  sandBracket = 0.25f * this.heightMap.HighestPeak,
                  grassBracket = 0.2f * this.heightMap.HighestPeak,
                  rockBracket = 0.2f * this.heightMap.HighestPeak,
                  snowBracket = 0.2f * this.heightMap.HighestPeak;
            for (int z = 0; z < this.Width; ++z)
            {
                for (int x = 0; x < this.Height; ++x)
                {
                    int ndx = x + z * this.Width;
                    this.vertices[ndx].Position = new Vector3(x * this.scale, (this.heightMap.Data[x, z]) * this.scale - this.heightMap.HeightOffset * this.scale, z * this.scale);
                    this.vertices[ndx].Normal = new Vector3(0, 0, 0);
                    this.vertices[ndx].TextureCoordinate.X = (float)x / 50f;
                    this.vertices[ndx].TextureCoordinate.Y = (float)z / 50f;

                    #region Texture weight calculation
                    this.vertices[ndx].TextureWeights.X = MathHelper.Clamp(1f - Math.Abs(this.heightMap.Data[x, z] - minSandHeight) / sandBracket, 0, 1);     // Sand, 0.2666666666666667
                    this.vertices[ndx].TextureWeights.Y = MathHelper.Clamp(1f - Math.Abs(this.heightMap.Data[x, z] - minGrassHeight /* 0.4 */) / grassBracket, 0, 1);   // Grass, 0.2
                    this.vertices[ndx].TextureWeights.Z = MathHelper.Clamp(1f - Math.Abs(this.heightMap.Data[x, z] - minRockHeight /* 0.6666666666666667 n*/) / rockBracket, 0, 1);  // Rock
                    this.vertices[ndx].TextureWeights.W = MathHelper.Clamp(1f - Math.Abs(this.heightMap.Data[x, z] - minSnowHeight /* 1 */) / snowBracket, 0, 1);  // Snow

                    float totalWeight = this.vertices[ndx].TextureWeights.X
                        + this.vertices[ndx].TextureWeights.Y
                        + this.vertices[ndx].TextureWeights.Z
                        + this.vertices[ndx].TextureWeights.W;
                    this.vertices[ndx].TextureWeights.X /= totalWeight;
                    this.vertices[ndx].TextureWeights.Y /= totalWeight;
                    this.vertices[ndx].TextureWeights.Z /= totalWeight;
                    this.vertices[ndx].TextureWeights.W /= totalWeight;

                    #endregion
                }
            }
        }

        /// <summary>
        /// Buffers the vertex data.
        /// </summary>
        /// <param name="device">The device.</param>
        private void BufferVertexData(GraphicsDevice device)
        {
            this.vertexBuffer = new VertexBuffer(device, VertexMultitextured.VertexDeclaration, this.vertices.Count(), BufferUsage.WriteOnly);
            this.vertexBuffer.SetData(this.vertices);
        }

        /// <summary>
        /// Buffers the index data.
        /// </summary>
        /// <param name="device">The device.</param>
        private void BufferIndexData(GraphicsDevice device)
        {
            this.indexBuffer = new IndexBuffer(device, IndexElementSize.ThirtyTwoBits, this.indices.Length, BufferUsage.WriteOnly);
            this.indexBuffer.SetData(this.indices);
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
                this.vertices[index1].Normal += normal;
                this.vertices[index2].Normal += normal;
                this.vertices[index3].Normal += normal;
            }

            for (int i = 0; i < this.vertices.Length; ++i)
            {
                this.vertices[i].Normal.Normalize();
            }
        }

        /// <summary>
        /// Sets up vertex indices.
        /// </summary>
        private void SetUpIndices()
        {
            int height = (this.Height - 1),         // number of triangles in a row
                width = (this.Width - 1),           // number of rows of triangles within a height map
                index = 0;
            ///* 
            // * to draw only half of a rectangle in one place:
            // * triangles/row * rows * 3 vertices for a spot, making only a triangular half filled.
            // * this.indices = new int[height * width * 3];
            // * 
            // * To draw a full rectangle in one place:
            // * triangles/row * rows * 6 vertices for a spot, placing two triangles in one spto, thus filling it completely
            // * this.indices = new int[height * width * 6];
            //*/
            this.indices = new int[height * width * 6];
            for (int y = 0; y < height; ++y)
            {
                for (int x = 0; x < width; ++x)
                {
                    #region Indices for -z
                    //int bottomLeft = x + y * this.Width;
                    //int bottomRight = (x + 1) + y * this.Width;
                    //int topLeft = x + (y + 1) * this.Width;
                    //int topRight = (x + 1) + (y + 1) * this.Width; 
                    #endregion
                    #region Indices for +z
                    int topLeft = x + y * this.Width;
                    int topRight = (x + 1) + y * this.Width;
                    int bottomLeft = x + (y + 1) * this.Width;
                    int bottomRight = (x + 1) + (y + 1) * this.Width;
                    #endregion

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
        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            effect.Parameters["xWorld"].SetValue(world);
            effect.Parameters["xView"].SetValue(view);
            effect.Parameters["xProjection"].SetValue(projection);
            int drawLimit = 100000;
            int primitives = this.indices.Length / 3;
            int segments = primitives / drawLimit;
            int rest = primitives % drawLimit;


            foreach (EffectPass pass in this.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                effect.GraphicsDevice.Indices = this.indexBuffer;
                effect.GraphicsDevice.SetVertexBuffer(this.vertexBuffer);
                for (int offset = 0; offset < segments; ++offset)
                {
                    /* 
                     * All the commentary refers to:
                     * http://blogs.msdn.com/b/jsteed/archive/2004/07/16/drawindexedprimitive-demystified.aspx
                     * The MinIndex and NumVertices values are really just hints to help Direct3D optimize memory access
                     * during software vertex processing,
                     * and could simply be set to include the entire vertex buffer at the price of performance.
                     * NOTE: There is probably still room for optimization here.
                     */
                    effect.GraphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        0,
                        /* minVertexIndex:
                         * The index of the firstmost vertex contained in the VertexBuffer (VB Index)
                         * that can be used in the process of drawing the current segment.
                         * Put differently: the lowest VB Index used to reference the VertexBuffer during this drawing call.
                         */
                        0,
                        /*
                         * numVertices:
                         * The number of vertices (starting from baseVertex's VB Index) among which the ones required to perform the draw call are contained.
                         */
                        this.vertices.Length,
                        /*
                         * startIndex:
                         * The starting IB Index.
                         */
                        offset * drawLimit * 3,
                        drawLimit);
                }
                if (rest > 0)
                {
                    effect.GraphicsDevice.DrawIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        0,
                        0,
                        this.vertices.Length,
                        segments * drawLimit * 3,
                        rest);
                }
            }
        }

        public void Dispose()
        {
            this.indexBuffer.Dispose();
            this.vertexBuffer.Dispose();
        }
    }
}

