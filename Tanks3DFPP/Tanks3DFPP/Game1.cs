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
        public static IHeightMap heightMap;
        public static int Scale = 5;
        private FPPCamera camera;
        private SpriteFont font;
        private GraphicsDeviceManager graphics;
        private Texture2D bgTexture;
        private const float FarClippingPlane = 20000f;

        KeyboardHandler mainKeyboardHandler = new KeyboardHandler();

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

        private RasterizerState rs = new RasterizerState { FillMode = FillMode.Solid };
        private Sky sky;

        private CollisionSphere sphere;
        private SpriteBatch spriteBatch;
        private TankController tankController;
        private QuadTree terrain;
        private bool wireFrame;
        private Matrix world;

        public static Vector3 SunPos;

        private Color bgColor = new Color(69, 125, 200);



        /// <summary>
        /// menu variable
        /// </summary>
        Menu.Menu menu;

        /// <summary>
        /// list of player names , size of it is player count(menu adjusted from 2 to 4 players)
        /// </summary>
        List<string> playerNames;

        /// <summary>
        /// starting player number according to the playerNames index
        /// </summary>
        private int startingPlayerNumber;

        /// <summary>
        /// percent of creating terrain process(from 0 - 100)
        /// </summary>
        private int percent;

        //time for loading percent needed until backgroundworker for loading terrain is done :D
        private int timesince = 0;
        private int timeperframe = 400;
        private bool generatingHeightMap, firstGenerationDone;

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
            if (menu.GetmenuON())
            {
                menu.showMenu();
            }
            else
            {
                if (!generatingHeightMap)
                {
                    GraphicsDevice.Clear(Color.Black);
                    sky.Draw(camera);
                    GraphicsDevice.BlendState = BlendState.Opaque;
                    GraphicsDevice.DepthStencilState = DepthStencilState.Default;
                    GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
                    GraphicsDevice.RasterizerState = rs;
                    sphere.Draw(world, camera.View, projection);
                    terrain.Draw(gameTime);
                    tankController.Draw(camera.View, projection);
                }
            }
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
            spriteBatch.DrawString(font, string.Format("pos: {0}", camera.Position), Vector2.UnitY * 20, Color.Wheat);
            spriteBatch.DrawString(font, string.Format("lookat: {0}", camera.LookAt), Vector2.UnitY * 40, Color.Wheat);
            spriteBatch.DrawString(font, string.Format("vec: {0}", (camera).Direction), Vector2.UnitY * 60,
                                   Color.Wheat);
            spriteBatch.DrawString(font,
                                   string.Format("tris: {0}, culling: {1}", terrain.IndexCount / 3, terrain.CullingEnabled),
                                   Vector2.UnitY * 80, Color.Wheat);
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
                heightMap, // = new FractalMap(mapSize, Roughness, maxHeight)
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
            spriteBatch = new SpriteBatch(GraphicsDevice);
            menu = new Menu.Menu(graphics);
            AsyncHeightMap.Progressing += (sender, e) =>
                {
                    percent = e.Value;
                };

            AsyncHeightMap.Finished += (sender, e) =>
                {
                    camera = new FPPCamera(GraphicsDevice,
                        new Vector3(500, maxHeight * Scale, 500),
                        0.3f,
                        2.0f,
                        projection);
                    GenerateEverything();
                    sky = new Sky(this.GraphicsDevice, this.Content, this.projection, heightMap.Width);
                    percent = 100;
                    generatingHeightMap = false;
                    firstGenerationDone = true;
                };

    base.Initialize();
        }

        /// <summary>
        ///     LoadContent will be called once per game and is the place to load
        ///     all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            font = Content.Load<SpriteFont>("SpriteFont1");
            this.bgTexture = Content.Load<Texture2D>("TRON__grid_only_by_hardwayjackson");
            menu.LoadMenu(Content, mapSize, roughness, maxHeight);
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
            if (terrain != null)
            {
                terrain.Dispose();
            }

            this.Content.Unload();
        }

/// <summary>
/// Allows the game to run logic such as updating the world,
/// checking for collisions, gathering input, and playing audio.
/// </summary>
/// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //CurrentKeyboardState = Keyboard.GetState();
            //CurrentMouseState = Mouse.GetState();
            //
            //for menu
            //

            mainKeyboardHandler.KeyAction(Keys.Escape, () =>
                {
                    this.Exit();
                });

            if (menu.GetmenuON())
            {
                //
                //this list contains menu results depending on its state.
                //
                List<object> listWithResults = menu.updateMenu(gameTime, percent);

                //checking the state of menu, if 1 then play page is done which means we have necessary variables for creating terrain
                if ((int) listWithResults[0] == 1)
                {
                    if ((int) listWithResults[1] == 1)
                    {
                        //next button in play page pressed
                        //set variables
                        mapSize = int.Parse((string) listWithResults[2]);
                        maxHeight = int.Parse((string) listWithResults[3]);
                        roughness = int.Parse((string) listWithResults[4]);

                        playerNames = new List<string>();
                        for (int i = 0; i < listWithResults.Count - 5; ++i)
                        {
                            playerNames.Add((string) listWithResults[i + 5]);
                        }
                    }
                }

                if ((int) listWithResults[0] == 2)
                {
                    //loading page is on 
                    //start creating the terrain
                    //getting percent variable which ranges from 0 to 100 , if it reaches 100(loading terrain process is over) menu will be no more... :D and the fun will begin

                    //tu powinno byc zaczecie tworzenia terenu
                    //loading jest teraz pokazowe jak chcesz zeby polaczyc go z kreowaniem terenu to  
                    //zmienna percent(int) trzeba by zmieniac od 0 do 100 (wtedy wyjdzie z menu, czyli powinno zakonczyc tworzenie terenu

                    //timesince += gameTime.ElapsedGameTime.Milliseconds;
                    //if (timesince > timeperframe)
                    //{
                    if (!(generatingHeightMap || firstGenerationDone))
                    {
                        generatingHeightMap = true;
                        heightMap = new FractalMap(mapSize, roughness, maxHeight);
                        ((FractalMap)heightMap).Initialize();
                    }

                    if (listWithResults.Count > 1)
                        startingPlayerNumber = (int) listWithResults[1];
                    else
                    {
                        percent = (int)MathHelper.Clamp(++percent, 0, 99);
                    }
                }

                if (menu.quit)
                    this.Exit();
            }
            else
            {
                if (firstGenerationDone)
                {
                    mainKeyboardHandler.KeyAction(Keys.G, GenerateEverything);
                    mainKeyboardHandler.KeyAction(Keys.F, () =>
                        {
                            rs = new RasterizerState {FillMode = wireFrame ? FillMode.Solid : FillMode.WireFrame};
                            wireFrame = !wireFrame;
                        });

                    mainKeyboardHandler.KeyAction(Keys.L, terrain.SwitchLighting);
                    //mainKeyboardHandler.KeyAction(Keys.L, terrain.);
                    terrain.Update(camera);
                    sphere.Update(gameTime);
                    if (tankController.bShotFired)
                    {
                        this.camera.Update(gameTime);
                        camera.AttachAndUpdate(tankController.MissleInGame.Position);
                    }
                    else
                    {
                        camera.AttachAndUpdate(tankController.TankWithToken);
                    }
                    camera.Update(gameTime);
                    tankController.Update(gameTime);
                }
            }

            base.Update(gameTime);
        }
    }
}