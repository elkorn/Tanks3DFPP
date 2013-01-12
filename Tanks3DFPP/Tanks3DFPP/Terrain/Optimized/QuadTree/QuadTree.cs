using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tanks3DFPP.Camera.Interfaces;
using Tanks3DFPP.Terrain.Optimization.QuadTree;
using Tanks3DFPP.Utilities;

namespace Tanks3DFPP.Terrain
{
    public class QuadTree : GameComponent
    {
        public readonly int MinimumDepth = 7;
        private readonly BufferManager buffers;
        private readonly Effect effect;
        private readonly Texture grass;
        private readonly Vector3 position;
        private Vector3 lightDir;
        private readonly Texture rock;
        private readonly QuadNode root;
        private readonly Texture sand;
        private readonly Texture snow;

        private readonly int topNodeSize;
        private readonly TreeVertexCollection vertices;

        /// <summary>
        ///     The indices to be currently used.
        /// </summary>
        public int[] Indices;

        public Matrix Projection;
        public Matrix View;
        private QuadNode active;

        public int IndexCount { get; private set; }

        private BoundingFrustum lastCameraFrustum;

        public QuadTree(Game game, Vector3 position, IHeightMap heightMap, Matrix view, Matrix projection, int scale)
            : base(game)
        {
            this.position = position;
            topNodeSize = heightMap.Width - 1;

            vertices = new TreeVertexCollection(this.position, heightMap, scale);
            buffers = new BufferManager(vertices.Vertices, GraphicsDevice);

            effect = Game.Content.Load<Effect>("Multitexture");
            sand = Game.Content.Load<Texture>("Dirt cracked 00 seamless");
            grass = Game.Content.Load<Texture>("grass0026_1_l2");
            rock = Game.Content.Load<Texture>("Tileable stone texture (2)");
            snow = Game.Content.Load<Texture>("snow");


            View = view;
            Projection = projection;

            InitializeEffect();

            root = new QuadNode(NodeType.FullNode, topNodeSize, 1, null, this, 0);
            Indices = new int[(heightMap.Width + 1) * (heightMap.Height + 1) * 3];
        }

        internal BoundingFrustum ViewFrustum
        {
            get { return lastCameraFrustum; }
        }

        public bool CullingEnabled { get; set; }

        public GraphicsDevice GraphicsDevice
        {
            get { return Game.GraphicsDevice; }
        }

        public int TopNodeSize
        {
            get { return topNodeSize; }
        }

        public QuadNode Root
        {
            get { return root; }
        }

        public TreeVertexCollection Vertices
        {
            get { return vertices; }
        }

        public Effect Effect
        {
            get { return effect; }
        }

        protected override void Dispose(bool disposing)
        {
            buffers.Dispose();
            base.Dispose(disposing);
        }

        public void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetVertexBuffer(buffers.VertexBuffer);
            GraphicsDevice.Indices = buffers.IndexBuffer;

            foreach (EffectPass pass in Effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertices.Vertices.Length, 0,
                                                     IndexCount / 3);
            }
        }

        private void InitializeFog(float start, float end, Color color)
        {
            this.effect.Parameters["xFogStart"].SetValue(start);
            this.effect.Parameters["xFogEnd"].SetValue(end);
            this.effect.Parameters["xFogColor"].SetValue(new Vector3(color.R,color.G,color.B));
            this.effect.Parameters["xFogEnabled"].SetValue(true);
        }

        private void InitializeEffect()
        {
            lightDir = new Vector3(0, -1, -1);
            lightDir.Normalize();
            Effect.CurrentTechnique = Effect.Techniques["MultiTextured"];
            Effect.Parameters["xTexture0"].SetValue(sand);
            Effect.Parameters["xTexture1"].SetValue(grass);
            Effect.Parameters["xTexture2"].SetValue(rock);
            Effect.Parameters["xTexture3"].SetValue(snow);
            Effect.Parameters["xEnableLighting"].SetValue(true);
            this.SetAmbient(.2f);
            Effect.Parameters["xLightDirection"].SetValue(lightDir);
            Effect.Parameters["xWorld"].SetValue(Matrix.Identity);
            Effect.Parameters["xProjection"].SetValue(Projection);
            Effect.Parameters["xEnableBlending"].SetValue(true);
            //this.InitializeFog(2,18,Color.Red);
        }

        private void SetAmbient(float value)
        {
            Effect.Parameters["xAmbient"].SetValue(value);
        }

        private void MoreAmbient()
        {
            this.SetAmbient(Effect.Parameters["xAmbient"].GetValueSingle() + .1f);
        }

        private void LessAmbient()
        {
            this.SetAmbient(Effect.Parameters["xAmbient"].GetValueSingle() - .1f);
        }

        public void SwitchBlending(bool? value)
        {
            if (!value.HasValue)
            {
                value = !Effect.Parameters["xEnableBlending"].GetValueBoolean();
            }

            Effect.Parameters["xEnableBlending"].SetValue(value.Value);
        }

        /// <summary>
        ///     Switches the lighting.
        /// </summary>
        public void SwitchLighting()
        {
            Effect.Parameters["xEnableLighting"].SetValue(!Effect.Parameters["xEnableLighting"].GetValueBoolean());
        }

        private void RotateLight(float degrees = 1)
        {
            lightDir = Vector3.TransformNormal(lightDir, Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(degrees), 0, 0));
            lightDir.Normalize();
            Effect.Parameters["xLightDirection"].SetValue(lightDir);
        }

        public void Update(ICamera camera)
        {
            // Checking camera position is not enough - terrain has to update also while changing the angle.
            if (camera.Frustum != lastCameraFrustum)
            {

                Effect.Parameters["xView"].SetValue(camera.View);

                lastCameraFrustum = camera.Frustum;
                IndexCount = 0;

                root.Merge();
                root.EnforceMinimumDepth();
                active = root.DeepestNodeContainingPoint(camera.LookAt);
                if (active != null)
                {
                    active.Split();
                }

                root.SetActiveVertices();

                buffers.UpdateIndexBuffer(Indices, IndexCount);
                buffers.SwapBuffer();
            }

            this.RotateLight(.2f);
        }

        internal void UpdateBuffer(int vertexIndex)
        {
            Indices[IndexCount++] = vertexIndex;
        }
    }
}