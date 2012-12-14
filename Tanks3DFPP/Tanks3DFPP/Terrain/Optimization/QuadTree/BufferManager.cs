using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Tanks3DFPP.Utilities;

namespace Tanks3DFPP.Terrain.Optimization.QuadTree
{
    /// <summary>
    /// Responsible for managing the active vertex and index buffers.
    /// </summary>
    internal class BufferManager
    {
        internal VertexBuffer VertexBuffer;

        internal IndexBuffer IndexBuffer
        {
            get
            {
                return this.indexBuffers[this.activeIndexBuffer];
            }
        }

        /// <summary>
        /// The active index buffer.
        /// </summary>
        private int activeIndexBuffer = 0;

        private const int indexBufferSize = 100000;

        private IndexBuffer[] indexBuffers;

        private GraphicsDevice graphicsDevice;


        internal BufferManager(VertexMultitextured[] vertices, GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            this.VertexBuffer = new VertexBuffer(this.graphicsDevice, VertexMultitextured.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);

            this.indexBuffers = new[]
            {
                new IndexBuffer(this.graphicsDevice, IndexElementSize.ThirtyTwoBits, indexBufferSize, BufferUsage.WriteOnly),
                new IndexBuffer(this.graphicsDevice, IndexElementSize.ThirtyTwoBits, indexBufferSize, BufferUsage.WriteOnly)
            };
        }

        internal void UpdateIndexBuffer(int[] indices, int indexCount)  //getlength maybe?
        {

        }

        internal void SwapBuffer()
        {
            ++activeIndexBuffer;
            activeIndexBuffer %= 2;
        }
    }
}
