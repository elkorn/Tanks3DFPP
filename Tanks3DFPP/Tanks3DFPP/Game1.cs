using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tanks3DFPP.Camera;
using Tanks3DFPP.Entities;
using Tanks3DFPP.Tanks;
using Tanks3DFPP.Terrain;
using Tanks3DFPP.Terrain.Interfaces;
using Tanks3DFPP.Terrain.Optimization.QuadTree;
using Tanks3DFPP.Utilities;

namespace Tanks3DFPP
{
    /// <summary>
    ///     This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        private const float FarClippingPlane = 20000f;
        public static FractalMap heightMap;
        public static GameParameters GameParameters = new GameParameters();

        private static int mapSize = 10,
                           roughness = 500,
                           maxHeight = 300,
                           scale = 1,
            //TODO: Wire this up.
                           lightChangeSpeed = 1;

        private readonly GraphicsDeviceManager graphics;
        private FPPCamera camera;
        private SpriteFont font;


        private bool debug;
        /// <summary>
        ///     menu variable
        /// </summary>
        private Menu.Menu menu;

        /// <summary>
        ///     percent of creating terrain process(from 0 - 100)
        /// </summary>
        private int percent;

        /// <summary>
        ///     list of player names , size of it is player count(menu adjusted from 2 to 4 players)
        /// </summary>
        private List<string> playerNames;

        private Matrix projection;

        private RasterizerState rs = new RasterizerState { FillMode = FillMode.Solid };
        private Sky sky;

        private SpriteBatch spriteBatch;

        private TankController tankController;
        private QuadTree terrain;

        private bool wireFrame;

        public Game1()
        {
            Content.RootDirectory = "Content";
            graphics = new GraphicsDeviceManager(this);
            Quitting += (sender, e) => { Exit(); };
        }

        //public static int MapScale
        //{
        //    get { return scale; }

        //    set
        //    {
        //        if (value > 0 && value < 16)
        //        {
        //            scale = value;
        //        }
        //    }
        //}

        //public static int MaxHeight
        //{
        //    get { return maxHeight; }

        //    set
        //    {
        //        if (value >= 100 && value <= 1000)
        //        {
        //            maxHeight = value;
        //        }
        //    }
        //}

        //public static int Roughness
        //{
        //    get { return roughness; }
        //    set
        //    {
        //        if (value >= 150 && value <= 900)
        //        {
        //            roughness = value;
        //        }
        //    }
        //}

        //public static int LightChangeSpeed
        //{
        //    get { return lightChangeSpeed; }
        //    set { lightChangeSpeed = value; }
        //}

        public static KeyboardState CurrentKeyboardState { get; private set; }
        public static MouseState CurrentMouseState { get; private set; }

        /// <summary>
        /// Resets the game to the first state.
        /// </summary>
        public void Reset()
        {
            GameParameters = new GameParameters();
            this.menu.Reset();
            terrain.Dispose();
        }

        public void StartNew()
        {
            terrain.Dispose();
            this.menu.ShowLoadingPage();
        }

        /// <summary>
        ///     This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (menu.Enabled)
            {
                menu.Draw();
            }
            else
            {
                GraphicsDevice.Clear(Color.Black);
                sky.Draw(camera);
                GraphicsDevice.BlendState = BlendState.Opaque;
                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                GraphicsDevice.RasterizerState = rs;
                terrain.Draw(gameTime);
                tankController.Draw(camera.View, projection);
                if (debug)
                {
                    this.DrawDebugInfo();
                }
            }

            base.Draw(gameTime);
        }

        #region Debug info drawing methods
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
            BoundingFrustumRenderer.Render(camera.Frustum, GraphicsDevice, camera.View, projection, Color.Red);
        }

        private void DrawDebugInfo()
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(font,
                                   string.Format("tris: {0}, culling: {1}", terrain.IndexCount / 3, terrain.CullingEnabled),
                                  (Vector2.UnitX * GraphicsDevice.Viewport.Width * 0.6f), Color.Wheat);
            spriteBatch.End();
        }
        #endregion

        /// <summary>
        ///     Allows the game to perform any initialization it needs to before starting to run.
        ///     This is where it can query for any required services and load any non-graphic
        ///     related content.  Calling base.Initialize will enumerate through any components
        ///     and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            QuadNode.limY = (GameParameters.MapScale + 1) * maxHeight;
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45),
                                                             GraphicsDevice.Viewport.AspectRatio,
                                                             1,
                                                             FarClippingPlane);
            spriteBatch = new SpriteBatch(GraphicsDevice);
            menu = new Menu.Menu(this);

            menu.GameComponentsReady += MenuOnGameComponentsReady;
            this.StartLoading();
            base.Initialize();
        }

        private void MenuOnGameComponentsReady(object sender, EventArgs eventArgs)
        {
            terrain.Update(this.camera);
        }

        private void AddMenuProgress(object sender= null, EventArgs e = null)
        {
            this.menu.AddProgress(34);
        }

        private void StartLoading()
        {
            if (terrain != null)
            {
                terrain.Dispose();
            }

            menu.GameParametersReady += (sender, e) =>
            {
                GameParameters = e.Parameters;
                heightMap = new FractalMap(
                    mapSize,
                    GameParameters.Roughness,
                    GameParameters.MaxMapHeight);
                tankController = new TankController(this);
                heightMap.Ready += (s, ea) =>
                {
                    camera = new FPPCamera(GraphicsDevice,
                                           new Vector3(500, maxHeight * GameParameters.MapScale, 500),
                                           0.3f,
                                           2.0f,
                                           projection);
                    this.AddMenuProgress();
                    tankController.Initialize();

                    #region Terrain initialization
                    terrain = new QuadTree(
                                    this,
                                    Vector3.Zero,
                                    camera.View,
                                    projection);
                    terrain.Ready += this.AddMenuProgress;
                    terrain.Initialize(heightMap);
                    #endregion

                    sky = new Sky(GraphicsDevice, Content, projection, heightMap.Width, GameParameters.MapScale);
                };

                tankController.Ready += TankControllerOnReady;
                tankController.ShotFired += TankControllerOnShotFired;
                tankController.MissileExploded += TankControllerOnMissileExploded;
                heightMap.Initialize();
            };
        }

        private void TankControllerOnReady(object sender, EventArgs eventArgs)
        {
            camera.AttachAndUpdate(tankController.TankWithToken);
            this.AddMenuProgress();
        }

        private void TankControllerOnMissileExploded(object sender, EventArgs eventArgs)
        {
            this.camera.ControlledByMouse = false;
        }

        private void TankControllerOnShotFired(object sender, EventArgs eventArgs)
        {
            this.camera.ControlledByMouse = true;
        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("SpriteFont1");
            menu.LoadContent();
        }

        public static void Quit()
        {
            Quitting.Invoke(null, null);
        }

        /// <summary>
        ///     UnloadContent will be called once per game and is the place to unload
        ///     all content.
        /// </summary>
        protected override void UnloadContent()
        {
            if (terrain != null)
            {
                terrain.Dispose();
            }

            Content.Unload();
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

            if (menu.Enabled)
            {
                menu.Update();
            }
            else
            {
                KeyboardHandler.KeyAction(Keys.F, () =>
                    {
                        rs = new RasterizerState { FillMode = wireFrame ? FillMode.Solid : FillMode.WireFrame };
                        wireFrame = !wireFrame;
                    });

                KeyboardHandler.KeyAction(Keys.C, () =>
                    {
                        terrain.CullingEnabled = !terrain.CullingEnabled;
                    });

                KeyboardHandler.KeyAction(Keys.D, () =>
                    {
                        debug = !debug;
                    });

                terrain.Update(camera);
                if (tankController.bShotFired)
                {
                    camera.Update(gameTime);
                    camera.AttachAndUpdate(tankController.MissleInGame.Position);
                }
                else
                {
                    camera.AttachAndUpdate(tankController.TankWithToken);
                }

                camera.Update(gameTime);
                tankController.Update(gameTime);
            }

            base.Update(gameTime);
        }

        private static event EventHandler Quitting;
    }
}