using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tanks3DFPP.Camera;
using Tanks3DFPP.Camera.Interfaces;
using Tanks3DFPP.Entities;
using Tanks3DFPP.Tanks;
using Tanks3DFPP.Terrain;
using Tanks3DFPP.Terrain.Optimization.QuadTree;
using Tanks3DFPP.Utilities;

namespace Tanks3DFPP
{
    /// <summary>
    ///     This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        public static IHeightMap heightMap;
        public static int Scale = 15;
        private ICamera camera;
        private SpriteFont font;
        private GraphicsDeviceManager graphics;
        private Texture2D bgTexture;
        private const float FarClippingPlane = 20000f;

        private int mapSize = 10,
                    roughness = 500,
                    maxHeight = 300;
        public int MaxHeight
        {
            get
            {
                return this.maxHeight;
            }

            set
            {
                if (value >= 100 && value <= 1000)
                {
                    this.maxHeight = value;
                }
            }
        }

        private Matrix projection;

        private RasterizerState rs = new RasterizerState {FillMode = FillMode.Solid};
        private Sky sky;

        private CollisionSphere sphere;
        private SpriteBatch spriteBatch;
        private TankController tankController;
        private QuadTree terrain;
        private bool wireFrame;
        private Matrix world;

        public static Vector3 SunPos;

        private Color bgColor = new Color(69,125,200);

        public Game1()
        {
            Content.RootDirectory = "Content";
            graphics = new GraphicsDeviceManager(this);
        }

        public static KeyboardState CurrentKeyboardState { get; private set; }
        public static MouseState CurrentMouseState { get; private set; }

        public int Roughness
        {
            get { return roughness; }
            set
            {
                if (value >= 150 && value <= 900)
                {
                    roughness = value;
                }
            }
        }

        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            //spriteBatch.Begin();
            //spriteBatch.Draw(bgTexture, GraphicsDevice.Viewport.Bounds, Color.White);
            //spriteBatch.End();
            sky.Draw(camera);
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.RasterizerState = rs;
            sphere.Draw(world, camera.View, projection);
            terrain.Draw(gameTime);
            tankController.Draw(camera.View, projection);
            DrawDebugInfo();
            base.Draw(gameTime);
        }

        private void DrawBoundingElements()
        {
            foreach (Tank tank in tankController.TanksInGame)
            {
                foreach (BoundingSphere boundingSphere in tank.BoundingSpheres)
                {
                    BoundingSphereRenderer.Render(boundingSphere, GraphicsDevice, camera.View, projection, Color.Red);
                }
            }

            BoundingSphereRenderer.Render(tankController.MissleInGame.BoundingSphere, GraphicsDevice, camera.View,
                                          projection, Color.Red);
            BoundingSphereRenderer.Render(sphere.BoundingSphere, GraphicsDevice, camera.View, projection, Color.Red);
            BoundingFrustumRenderer.Render(camera.Frustum, GraphicsDevice, camera.View, projection, Color.Red);
        }

        private void DrawDebugInfo()
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(font,
                                   string.Format("Near: {0}, Far: {1}, rs: {2}", camera.Frustum.Near.D,
                                                 camera.Frustum.Far.D,
                                                 GraphicsDevice.RasterizerState.FillMode.ToString()), Vector2.Zero,
                                   Color.Wheat);
            spriteBatch.DrawString(font, string.Format("pos: {0}", camera.Position), Vector2.UnitY*20, Color.Wheat);
            spriteBatch.DrawString(font, string.Format("lookat: {0}", camera.LookAt), Vector2.UnitY*40, Color.Wheat);
            spriteBatch.DrawString(font, string.Format("vec: {0}", ((FPPCamera) camera).Direction), Vector2.UnitY*60,
                                   Color.Wheat);
            spriteBatch.DrawString(font,
                                   string.Format("tris: {0}, culling: {1}", terrain.IndexCount/3, terrain.CullingEnabled),
                                   Vector2.UnitY*80, Color.Wheat);
            spriteBatch.End();
        }

        private void GenerateEverything()
        {
            if (terrain != null)
            {
                terrain.Dispose();
            }

            terrain = new QuadTree(
                this, 
                Vector3.Zero,
                heightMap = new FractalMap(mapSize, Roughness, maxHeight),
                camera.View,
                projection,
                Scale);
            sphere = new CollisionSphere(this, heightMap, new Vector3(50, 0, 150), Scale);
            tankController = new TankController(this, 2);
        }

        /// <summary>
        ///     Allows the game to perform any initialization it needs to before starting to run.
        ///     This is where it can query for any required services and load any non-graphic
        ///     related content.  Calling base.Initialize will enumerate through any components
        ///     and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            world = Matrix.Identity;
            QuadNode.limY = (Scale + 1)*maxHeight;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45),
                                                             GraphicsDevice.Viewport.AspectRatio, 
                                                             1, 
                                                             FarClippingPlane);
            camera = new FPPCamera(GraphicsDevice, 
                new Vector3(500, maxHeight*Scale, 500), 
                0.3f,
                2.0f,
                projection);

            spriteBatch = new SpriteBatch(GraphicsDevice);
            base.Initialize();
            GenerateEverything();
            //sphere = new CollisionSphere(this, heightMap, new Vector3(500, maxHeight * Scale, 500), Scale);
            sky = new Sky(this.GraphicsDevice, this.Content, this.projection, heightMap.Width);

        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("SpriteFont1");
            this.bgTexture = Content.Load<Texture2D>("TRON__grid_only_by_hardwayjackson");
        }

        public static void SetNodeAsRendered(QuadNode node, bool rendered)
        {
            //if (rendered)
            //{
            //    if (!nodes.Contains(node))
            //    {
            //        nodes.Add(node);
            //    }
            //}
            //else
            //{
            //    if (nodes.Contains(node))
            //    {
            //        nodes.Remove(node);
            //    }
            //}

            //nodeText = string.Join(" : ", nodes.Select(x => x.Depth.ToString()));
            //if (rendered)
            //{
            //    if (!nodes.Contains(node))
            //    {
            //        nodes.Add(node);
            //        nodesCount++;
            //    }
            //}
            //else
            //{
            //    if (nodes.Contains(node))
            //    {
            //        nodes.Remove(node);
            //        nodesCount--;
            //    }
            //}
        }

        /// <summary>
        ///     UnloadContent will be called once per game and is the place to unload
        ///     all content.
        /// </summary>
        protected override void UnloadContent()
        {
            terrain.Dispose();
            this.Content.Unload();
        }

        /// <summary>
        ///     Allows the game to run logic such as updating the world,
        ///     checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            CurrentKeyboardState = Keyboard.GetState();
            CurrentMouseState = Mouse.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || CurrentKeyboardState.IsKeyDown(Keys.Escape))
                Exit();

            KeyboardHandler.KeyAction(Keys.G, GenerateEverything);
            KeyboardHandler.KeyAction(Keys.F, () =>
                {
                    rs = new RasterizerState {FillMode = wireFrame ? FillMode.Solid : FillMode.WireFrame};
                    wireFrame = !wireFrame;
                });

            KeyboardHandler.KeyAction(Keys.L, terrain.SwitchLighting);
            terrain.Update(camera);
            sphere.Update(gameTime);
            if (tankController.bShotFired)
            {
                this.camera.Update(gameTime);
                camera.AttachAndUpdate(tankController.MissleInGame.Position);
                //camera.Position = tankController.MissleInGame.Position;
                //camera.LookAt = tankController.MissleInGame.Position + tankController.MissleInGame.Velocity;
            }
            //else
            //{
            //    camera.Position = tankController.TanksInGame[tankController.TurnToken].CannonPosition;
            //    camera.AttachAndUpdate(tankController.TanksInGame[tankController.TurnToken].CameraOrientation);
            //}
            tankController.Update(gameTime);
            base.Update(gameTime);
        }
            //BoundingFrustumRenderer.Render(this.camera.Frustum, this.GraphicsDevice, this.camera.View, this.projection, Color.Red);
    }
}