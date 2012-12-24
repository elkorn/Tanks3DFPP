using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Tanks3DFPP.Terrain;
using System.IO;
using Tanks3DFPP.Utilities;
using Tanks3DFPP.Camera.Interfaces;
using Tanks3DFPP.Camera;
using Tanks3DFPP.Tanks;

namespace Tanks3DFPP
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        int mapSize = 10,
            roughness = 500,
            maxHeight = 300;


        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Matrix world, projection;
        Terrain.QuadTreeTerrain terrain;
        //Terrain.MultiTexturedTerrain terrain;
        SpriteFont font;
        ICamera camera;
        int currentColoringMethod = 1;
        bool wireFrame;
        public static KeyboardState CurrentKeyboardState { get; private set; }
        public static MouseState CurrentMouseState { get; private set; }
        IHeightToColorTranslationMethod[] coloringMethods;

        CollisionSphere sphere;
        TankController tankController;

        public static IHeightMap heightMap;
        public static int Scale = 2;

        ////////////////////////////////////////////////////////////////////////////////////////

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
        /// percent of creating terrain process
        /// </summary>
        private int percent;

        //time for loading
        private int timesince = 0;
        private int timeperframe = 200;

        ////////////////////////////////////////////////////////////////////////////////////////

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            world = Matrix.Identity;

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), this.GraphicsDevice.Viewport.AspectRatio, 1, 20000f);
            this.camera = new FPPCamera(this.GraphicsDevice, new Vector3(100, maxHeight * Scale, 100), 0.3f, 2.0f, this.projection);

            heightMap = new FractalMap(mapSize, roughness, maxHeight);
            coloringMethods = new IHeightToColorTranslationMethod[] 
            {
                new GreenYellowRed(maxHeight),
                new HeightToGrayscale(maxHeight)
            };

            spriteBatch = new SpriteBatch(this.GraphicsDevice);

            //
            //for menu
            //
            menu = new Menu.Menu(graphics);

            base.Initialize();
            this.GenerateEverything();           

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            font = this.Content.Load<SpriteFont>("SpriteFont1");
            menu.LoadMenu(Content);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        private void GenerateEverything()
        {
            if (this.terrain != null)
            {
                this.terrain.Dispose();
            }

            this.terrain = new Terrain.QuadTreeTerrain(this, Vector3.Zero, heightMap = new FractalMap(mapSize, roughness, maxHeight, 1), this.camera.View, this.projection, Scale);
            //this.terrain = new Terrain.MultiTexturedTerrain(this.GraphicsDevice, this.Content, heightMap = new FractalMap(mapSize, roughness, maxHeight, 1), Scale);
            sphere = new CollisionSphere(this, heightMap, new Vector3(50, 0, 150), Scale);
            tankController = new TankController(this, 2);
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

             //
            //for menu
            //
            if (menu.GetmenuON())
            {
                //
                //this list contains menu results depending on its state.
                //
                List<object> listWithResults = menu.updateMenu(gameTime, percent);

                //checking the state of menu, if 1 then play page is done which means we have necessary variables for creating terrain
                if ((int)listWithResults[0] == 1)
                {
                    if ((int)listWithResults[1] == 1)
                    {
                        //next button in play page pressed
                        //set variables
                        mapSize = int.Parse((string)listWithResults[2]);
                        maxHeight = int.Parse((string)listWithResults[3]);
                        roughness = int.Parse((string)listWithResults[4]);

                        playerNames = new List<string>();
                        for (int i = 0; i < listWithResults.Count - 5; ++i)
                        {
                            playerNames.Add((string)listWithResults[i + 5]);
                        }

                        percent = 0;
                    }
                }
                if ((int)listWithResults[0] == 2)
                {
                    //loading page is on 
                    //start creating the terrain
                    //getting percent variable which ranges from 0 to 100 , if it reaches 100(loading terrain process is over) menu will be no more... :D and the fun will begin
                    
                    //tu powinno byc tworzenie terenu
                    //loading jest teraz pokazowe jak chcesz zeby polaczyc go z kreowaniem terenu to  
                    //zmienna percent(int) która trzeba by zmieniac od 0 do 100 (wtedy wyjdzie z menu, czyli powinno zakonczyc tworzenie terenu)

                    timesince += gameTime.ElapsedGameTime.Milliseconds;
                    if (timesince > timeperframe)
                    {
                        if (percent >= 100)
                        {
                            startingPlayerNumber = (int)listWithResults[1];                       
                        }
                        percent++;
                    }
                }

            }
            //
            //
            //

            if (!menu.GetmenuON())
            {


                Game1.CurrentKeyboardState = Keyboard.GetState();
                Game1.CurrentMouseState = Mouse.GetState();
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                    || CurrentKeyboardState.IsKeyDown(Keys.Escape))
                    this.Exit();

                KeyboardHandler.KeyAction(Keys.C, () =>
                {
                    currentColoringMethod += 1;
                    currentColoringMethod %= this.coloringMethods.Length;
                });

                KeyboardHandler.KeyAction(Keys.G, () =>
                {
                    this.GenerateEverything();

                });

                KeyboardHandler.KeyAction(Keys.F, () =>
                {
                    if (wireFrame)
                    {
                        this.GraphicsDevice.RasterizerState = new RasterizerState { FillMode = FillMode.Solid };
                    }
                    else
                    {
                        this.GraphicsDevice.RasterizerState = new RasterizerState { FillMode = FillMode.WireFrame };
                    }

                    wireFrame = !wireFrame;
                });

                KeyboardHandler.KeyAction(Keys.L, this.terrain.SwitchLighting);
                KeyboardHandler.KeyAction(Keys.B, () =>
                {
                    this.terrain.SwitchBlending(true);
                });
                KeyboardHandler.KeyAction(Keys.N, () =>
                {
                    this.terrain.SwitchBlending(false);
                });

                KeyboardHandler.KeyAction(Keys.C, () =>
                {
                    if (this.GraphicsDevice.RasterizerState.CullMode == CullMode.None)
                    {
                        this.GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.CullCounterClockwiseFace };
                    }
                    else
                    {
                        this.GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.None };
                    }
                });

                this.terrain.Update(this.camera, this.projection);
                sphere.Update(gameTime);
                this.camera.Update(gameTime);
                tankController.Update(gameTime);
            }

            base.Update(gameTime);
        }

        private void DrawBoundingElements()
        {
            foreach (Tank tank in tankController.TanksInGame)
            {
                foreach (BoundingSphere boundingSphere in tank.BoundingSpheres)
                {
                    BoundingSphereRenderer.Render(boundingSphere, this.GraphicsDevice, this.camera.View, this.projection, Color.Red);
                }
            }

            BoundingSphereRenderer.Render(tankController.MissleInGame.BoundingSphere, this.GraphicsDevice, this.camera.View, this.projection, Color.Red);
            BoundingSphereRenderer.Render(sphere.BoundingSphere, this.GraphicsDevice, this.camera.View, this.projection, Color.Red);
            BoundingFrustumRenderer.Render(this.camera.Frustum, this.GraphicsDevice, this.camera.View, this.projection, Color.Red);
        }


        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            //GraphicsDevice.BlendState = BlendState.Opaque;
            //GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.Clear(Color.Black);

            //
            //for menu
            //
            if (menu.quit)
                this.Exit();
            if (menu.GetmenuON())
            {
                menu.showMenu();
            }
            else
            {

                sphere.Draw(world, this.camera.View, projection);
                //this.terrain.Draw(this.world, this.camera.View, this.projection);
                this.terrain.Draw(gameTime);
                tankController.Draw(this.camera.View, this.projection);
                spriteBatch.Begin();
                spriteBatch.DrawString(font, string.Format("Near: {0}, Far: {1}", camera.Frustum.Near.D, camera.Frustum.Far.D), Vector2.Zero, Color.Wheat);
                spriteBatch.End();
            }
            base.Draw(gameTime);
        }
    }
}

