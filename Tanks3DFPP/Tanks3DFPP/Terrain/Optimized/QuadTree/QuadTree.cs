using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tanks3DFPP.Camera.Interfaces;
using Tanks3DFPP.Terrain.Interfaces;
using Tanks3DFPP.Terrain.Optimization.QuadTree;
using Tanks3DFPP.Utilities;

namespace Tanks3DFPP.Terrain
{
    public class QuadTree : AsyncLoadingElement
    {
        public readonly int MinimumDepth = 7;
        private BufferManager buffers;
        private readonly Effect effect;
        private readonly Texture grass;
        private readonly Vector3 position;
        private Vector3 lightDir;
        private readonly Texture rock;
        private QuadNode root;
        private readonly Texture sand;
        private readonly Texture snow;

        private int topNodeSize;
        private TreeVertexCollection vertices;

        /// <summary>
        ///     The indices to be currently used.
        /// </summary>
        public int[] Indices;

        public Matrix Projection;
        public Matrix View;
        private QuadNode active;

        public int IndexCount { get; private set; }

        private BoundingFrustum lastCameraFrustum;

        private GraphicsDevice graphicsDevice;

        public QuadTree(Game game, Vector3 position, Matrix view, Matrix projection)
        {
            this.graphicsDevice = game.GraphicsDevice;
            this.position = position;
            effect = game.Content.Load<Effect>("Multitexture");
            sand = game.Content.Load<Texture>("Dirt cracked 00 seamless");
            grass = game.Content.Load<Texture>("grass0026_1_l2");
            rock = game.Content.Load<Texture>("Tileable stone texture (2)");
            snow = game.Content.Load<Texture>("snow");

            View = view;
            Projection = projection;

            InitializeEffect();
        }

        public void Initialize(IHeightMap heightMap)
        {
            Thread t = new Thread(() =>
                {
                    topNodeSize = heightMap.Width - 1;
                    vertices = new TreeVertexCollection(this.position, heightMap, Game1.GameParameters.MapScale);
                    buffers = new BufferManager(vertices.Vertices, GraphicsDevice);
                    root = new QuadNode(NodeType.FullNode, topNodeSize, 1, null, this, 0);
                    Indices = new int[(heightMap.Width + 1) * (heightMap.Height + 1) * 3];
                    this.FireReady(this);
                });
            t.Start();
        }

        internal BoundingFrustum ViewFrustum
        {
            get { return lastCameraFrustum; }
        }

        public bool CullingEnabled { get; set; }

        internal GraphicsDevice GraphicsDevice
        {
            get { return this.graphicsDevice; }
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

        public void Dispose()
        {
            buffers.Dispose();
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

        //private void MoreAmbient()
        //{
        //    this.SetAmbient(Effect.Parameters["xAmbient"].GetValueSingle() + .1f);
        //}

        //private void LessAmbient()
        //{
        //    this.SetAmbient(Effect.Parameters["xAmbient"].GetValueSingle() - .1f);
        //}

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