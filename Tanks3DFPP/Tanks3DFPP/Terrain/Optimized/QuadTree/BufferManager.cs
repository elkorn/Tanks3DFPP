﻿using System;
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

        private IndexBuffer[] indexBuffers;

        private GraphicsDevice graphicsDevice;

        private int InactiveBufferIndex
        {
            get
            {
                return (activeIndexBuffer + 1) % 2;
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
            this.indexBuffers[this.InactiveBufferIndex].SetData(indices, 0, indexCount);
        }

        internal void SwapBuffer()
        {
            ++activeIndexBuffer;
            activeIndexBuffer %= 2;
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
