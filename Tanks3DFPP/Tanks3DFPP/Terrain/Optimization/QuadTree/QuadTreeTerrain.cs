﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tanks3DFPP.Camera.Interfaces;
using Tanks3DFPP.Terrain.Optimization.QuadTree;

namespace Tanks3DFPP.Terrain
{
    public class QuadTreeTerrain : GameComponent
    {
        private QuadNode root;


        private TreeVertexCollection vertices;

        private Effect effect;
        private Texture sand, grass, rock, snow;

        private BufferManager buffers;
        private Vector3 position;
        private int topNodeSize, indexCount;

        private Vector3 cameraPosition,
            lastCameraPosition;

        /// <summary>
        /// The indices to be currently used.
        /// </summary>
        public int[] Indices;

        public Matrix View,
            Projection;

        public GraphicsDevice GraphicsDevice
        {
            get
            {
                return this.Game.GraphicsDevice;
            }
        }

        public int TopNodeSize
        {
            get
            {
                return this.topNodeSize;
            }
        }

        public QuadNode Root
        {
            get
            {
                return root;
            }
        }

        public TreeVertexCollection Vertices
        {
            get
            {
                return vertices;
            }
        }

        public Vector3 CameraPosition
        {
            get
            {
                return cameraPosition;
            }

            set
            {
                cameraPosition = value;
            }
        }

        internal BoundingFrustum ViewFrustum { get; set; }

        public QuadTreeTerrain(Game game, Vector3 position, IHeightMap heightMap, Matrix view, Matrix projection, int scale)
            :base(game)
        {
            this.position = position;
            this.topNodeSize = heightMap.Width - 1;
            
            this.vertices = new TreeVertexCollection(this.Game.Content, this.position, heightMap, scale);
            this.buffers = new BufferManager(this.vertices.Vertices, this.GraphicsDevice);

            this.effect = this.Game.Content.Load<Effect>("Multitexture");
            this.sand = this.Game.Content.Load<Texture>("sand");
            this.grass = this.Game.Content.Load<Texture>("grass");
            this.rock = this.Game.Content.Load<Texture>("rock");
            this.snow = this.Game.Content.Load<Texture>("snow");
            this.InitializeEffect();

            this.root = new QuadNode(NodeType.FullNode, this.topNodeSize, 1, null, this, 0);
            
            this.View = view;
            this.Projection = projection;
            
            this.ViewFrustum = new BoundingFrustum(this.View * this.Projection);

            this.Indices = new int[(heightMap.Width + 1) * (heightMap.Height + 1) * 3];
        }

        public void Update(ICamera camera, Matrix projection)
        {
            if (camera.Position != this.lastCameraPosition)
            {
                this.effect.Parameters["xView"].SetValue(camera.View);
                this.effect.Parameters["xProjection"].SetValue(projection);

                this.lastCameraPosition = camera.Position;
                this.indexCount = 0;

                this.root.ActivateVertices();
                this.buffers.UpdateIndexBuffer(this.Indices, this.indexCount);
                this.buffers.SwapBuffer();
            }

        }

        public void Draw(GameTime gameTime)
        {
            this.GraphicsDevice.SetVertexBuffer(this.buffers.VertexBuffer);
            this.GraphicsDevice.Indices = this.buffers.IndexBuffer;

            foreach (EffectPass pass in this.effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                this.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, this.vertices.Vertices.Length, 0, this.indexCount / 3);
            }
        }

        internal void UpdateBuffer(int vertexIndex)
        {
            this.Indices[this.indexCount++] = vertexIndex;
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
            effect.Parameters["xWorld"].SetValue(Matrix.Identity);
        }

        protected override void Dispose(bool disposing)
        {
            this.buffers.Dispose();
            base.Dispose(disposing);
        }

    }
}