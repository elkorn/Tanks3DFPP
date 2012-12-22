using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tanks3DFPP.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Tanks3DFPP.Camera.Interfaces;
namespace Tanks3DFPP.Terrain.Optimization.QuadTree
{
    public class TreeVertexCollection
    {
        public VertexMultitextured[] Vertices;

        private Vector3 position;

        private int topSize,
                    halfSize,
                    vertexCount,
                    scale;

        public VertexMultitextured this[int index]
        {
            get
            {
                return this.Vertices[index];
            }

            set
            {
                this.Vertices[index] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeVertexCollection" /> class.
        /// </summary>
        /// <param name="position">The position of the top-left terrain corner in the 3D space.</param>
        /// <param name="heightMap">The height map.</param>
        /// <param name="scale">The scale.</param>
        public TreeVertexCollection(
            ContentManager content,
            Vector3 position,
            IHeightMap heightMap,
            int scale)
        {

            this.position = position;
            this.scale = scale;
            this.topSize = heightMap.Width - 1;
            this.halfSize = this.topSize / 2;
            this.vertexCount = heightMap.Width * heightMap.Height;

            this.BuildVertices(heightMap);
            this.CalculateAllNormals();
        }

        private void BuildVertices(IHeightMap heightMap)
        {
            this.Vertices = new VertexMultitextured[this.vertexCount];
            float[] heightData = Utilities.Utilities.Flatten(heightMap.Data);
            float x = this.position.X,
                  y = this.position.Y,
                  z = this.position.Z,
                  limX = x + this.topSize;

            float minSandHeight = 0,
                  minGrassHeight = 0.4f * heightMap.HighestPeak,
                  minRockHeight = (2 / 3f) * heightMap.HighestPeak,
                  minSnowHeight = heightMap.HighestPeak,
                  sandBracket = (2 / 3f) * heightMap.HighestPeak,
                  grassBracket = 0.2f * heightMap.HighestPeak,
                  rockBracket = 0.2f * heightMap.HighestPeak,
                  snowBracket = 0.2f * heightMap.HighestPeak;

            for (int ndx = 0; ndx < this.vertexCount; ++ndx)
            {
                float height = heightData[ndx];
                if (x > limX)
                {
                    x = this.position.X;
                    ++z;
                }

                this.Vertices[ndx].Position = new Vector3(x * this.scale, (height) * this.scale - heightMap.HeightOffset, z * this.scale);
                this.Vertices[ndx].Normal = new Vector3(0, 0, 0);
                this.Vertices[ndx].TextureCoordinate.X = (this.Vertices[ndx].Position.X - this.position.X) / this.topSize;
                this.Vertices[ndx].TextureCoordinate.Y = (this.Vertices[ndx].Position.Z - this.position.Z) / this.topSize;

                #region Texture weight calculation
                this.Vertices[ndx].TextureWeights.X = MathHelper.Clamp(1f - Math.Abs(height - minSandHeight) / sandBracket, 0, 1);
                this.Vertices[ndx].TextureWeights.Y = MathHelper.Clamp(1f - Math.Abs(height - minGrassHeight) / grassBracket, 0, 1);
                this.Vertices[ndx].TextureWeights.Z = MathHelper.Clamp(1f - Math.Abs(height - minRockHeight) / rockBracket, 0, 1);
                this.Vertices[ndx].TextureWeights.W = MathHelper.Clamp(1f - Math.Abs(height - minSnowHeight) / snowBracket, 0, 1);

                float totalWeight = this.Vertices[ndx].TextureWeights.X
                    + this.Vertices[ndx].TextureWeights.Y
                    + this.Vertices[ndx].TextureWeights.Z
                    + this.Vertices[ndx].TextureWeights.W;
                this.Vertices[ndx].TextureWeights.X /= totalWeight;
                this.Vertices[ndx].TextureWeights.Y /= totalWeight;
                this.Vertices[ndx].TextureWeights.Z /= totalWeight;
                this.Vertices[ndx].TextureWeights.W /= totalWeight;
                #endregion

                ++x;
            }
        }

        private void CalculateAllNormals()
        {
            #region It goes like this
            /*
             * +++--
             * +*+--
             * +++--
             * -----
             * -----
             * 
             * to
             * +++++
             * +++*+
             * +++++
             * -----
             * -----
             * 
             * to
             * +++++
             * +++++
             * +++++
             * +*+--
             * +++--
             * 
             * to
             * +++++
             * +++++
             * +++++
             * +++*+
             * +++++
             */
            #endregion

            if (this.vertexCount < 9)   // not enough verts
                return;

            int i = this.topSize + 2, j = 0, k = i + this.topSize;

            for (int n = 0; i <= (this.vertexCount - this.topSize) - 2; i += 2, n++, j += 2, k += 2)
            {

                if (n == this.halfSize)
                {
                    n = 0;
                    i += this.topSize + 2;
                    j += this.topSize + 2;
                    k += this.topSize + 2;
                }

                //Calculate normals for each of the 8 triangles
                SetNormals(i, j, j + 1);
                SetNormals(i, j + 1, j + 2);
                SetNormals(i, j + 2, i + 1);
                SetNormals(i, i + 1, k + 2);
                SetNormals(i, k + 2, k + 1);
                SetNormals(i, k + 1, k);
                SetNormals(i, k, i - 1);
                SetNormals(i, i - 1, j);
            }
        }

        private void SetNormals(int idx1, int idx2, int idx3)
        {
            if (idx3 >= this.Vertices.Length)
            {
                idx3 = this.Vertices.Length - 1;
            }

            var normal = Vector3.Cross(Vertices[idx2].Position - Vertices[idx1].Position, Vertices[idx1].Position - Vertices[idx3].Position);
            normal.Normalize();
            Vertices[idx1].Normal += normal;
            Vertices[idx2].Normal += normal;
            Vertices[idx3].Normal += normal;
        }
    }
}
