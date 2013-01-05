using System;
using Microsoft.Xna.Framework.Graphics;
using Tanks3DFPP.Utilities;

namespace Tanks3DFPP.Terrain.Optimization.QuadTree
{
    /// <summary>
    /// Responsible for managing the active vertex and index buffers.
    /// </summary>
    internal class BufferManager: IDisposable
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
        private int activeIndexBuffer;

        private const int indexBufferSize = 100000;

        private readonly IndexBuffer[] indexBuffers;

        private readonly GraphicsDevice graphicsDevice;

        private int InactiveBufferIndex
        {
            get
            {
                return activeIndexBuffer == 0 ? 1 : 0;
            }
        }

        internal BufferManager(VertexMultitextured[] vertices, GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            this.VertexBuffer = new VertexBuffer(this.graphicsDevice, VertexMultitextured.VertexDeclaration, vertices.Length, BufferUsage.WriteOnly);
            this.VertexBuffer.SetData(vertices);

            this.indexBuffers = new[]
            {
                new IndexBuffer(this.graphicsDevice, IndexElementSize.ThirtyTwoBits, indexBufferSize, BufferUsage.WriteOnly),
                new IndexBuffer(this.graphicsDevice, IndexElementSize.ThirtyTwoBits, indexBufferSize, BufferUsage.WriteOnly)
            };
        }

        internal void UpdateIndexBuffer(int[] indices, int indexCount)
        {
            if (this.indexBuffers[this.InactiveBufferIndex] != this.graphicsDevice.Indices)
            {
                this.indexBuffers[this.InactiveBufferIndex].SetData(indices, 0, indexCount);
            }
        }

        internal void SwapBuffer()
        {
            activeIndexBuffer = activeIndexBuffer == 1 ? 0 : 1;
        }

        public void Dispose()
        {
            this.VertexBuffer.Dispose();
            foreach(IndexBuffer buffer in this.indexBuffers)
            {
                buffer.Dispose();
            }
        }
    }
}
