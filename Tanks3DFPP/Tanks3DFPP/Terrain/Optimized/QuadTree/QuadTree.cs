using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Tanks3DFPP.Camera.Interfaces;
using Tanks3DFPP.Terrain.Optimization.QuadTree;

namespace Tanks3DFPP.Terrain
{
    public class QuadTree : GameComponent
    {
        private QuadNode root, active;

        private TreeVertexCollection vertices;

        private Effect effect;
        private Texture sand, grass, rock, snow;

        private BufferManager buffers;
        private Vector3 position;
        private int topNodeSize, indexCount;

        private BoundingFrustum lastCameraFrustum;

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

        internal BoundingFrustum ViewFrustum { get; set; }

        public readonly int MinimumDepth = 6;

        public QuadTree(Game game, Vector3 position, IHeightMap heightMap, Matrix view, Matrix projection, int scale)
            :base(game)
        {
            this.position = position;
            this.topNodeSize = heightMap.Width - 1;
            
            this.vertices = new TreeVertexCollection(this.position, heightMap, scale);
            this.buffers = new BufferManager(this.vertices.Vertices, this.GraphicsDevice);

            this.effect = this.Game.Content.Load<Effect>("Multitexture");
            this.sand = this.Game.Content.Load<Texture>("sand");
            this.grass = this.Game.Content.Load<Texture>("grass");
            this.rock = this.Game.Content.Load<Texture>("rock");
            this.snow = this.Game.Content.Load<Texture>("snow");


            this.View = view;
            this.Projection = projection;

            this.InitializeEffect();

            this.root = new QuadNode(NodeType.FullNode, this.topNodeSize, 1, null, this, 0);
            this.ViewFrustum = new BoundingFrustum(this.View * this.Projection);
            this.Indices = new int[(heightMap.Width + 1) * (heightMap.Height + 1) * 3];
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

        public void Update(ICamera camera)
        {
            // Checking camera position is not enough - terrain has to update also while changing the angle.
            if (camera.Frustum != this.lastCameraFrustum)
            {
                this.effect.Parameters["xView"].SetValue(camera.View);

                this.lastCameraFrustum = camera.Frustum;
                this.indexCount = 0;

                this.root.Merge();
                this.root.EnforceMinimumDepth();
                this.active = root.DeepestNodeContainingPoint(camera.LookAt);
                if (this.active != null)
                {
                    this.active.Split();
                }

                this.root.SetActiveVertices();

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
            this.Indices[this.indexCount] = vertexIndex;
            this.indexCount += 1;
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
            this.effect.Parameters["xProjection"].SetValue(this.Projection);
        }

        protected override void Dispose(bool disposing)
        {
            this.buffers.Dispose();
            base.Dispose(disposing);
        }

    }
}
