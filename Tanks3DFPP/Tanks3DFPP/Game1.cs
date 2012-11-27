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

namespace Tanks3DFPP
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        int mapSize = 9,
            roughness = 500,
            maxHeight = 255;

        /*
         * TODO: Is one effect enough to draw everything? 
         * Is using separate effects for objects beneficial for any aspect? 
         * If so, how should they be divided?
         * 
         * Or maybe effects should be assigned by
         * how should different elements be rendered?
         */
        Vector3 lightDirection;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Matrix world, projection;
        Terrain.Terrain terrain;
        ICamera camera;
        int currentColoringMethod = 1;
        bool wireFrame;
        public static KeyboardState CurrentKeyboardState { get; private set; }
        public static MouseState CurrentMouseState { get; private set; }
        IHeightToColorTranslationMethod[] coloringMethods;


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
            this.camera = new OrbitingCamera() { Position = new Vector3(100, 100, 100), LookAt = Vector3.Zero };
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), this.GraphicsDevice.Viewport.AspectRatio, 1, 2000f);
            base.Initialize();

            this.IsMouseVisible = true;
            coloringMethods = new IHeightToColorTranslationMethod[] 
            {
                new GreenYellowRed(maxHeight),
                new HeightToGrayscale(maxHeight)
            };

            this.terrain = new Terrain.Terrain(this.GraphicsDevice, new FractalMap(mapSize, roughness, maxHeight, true), coloringMethods[currentColoringMethod]);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
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
                    this.terrain = new Terrain.Terrain(this.GraphicsDevice, new FractalMap(mapSize, roughness, maxHeight, false), coloringMethods[currentColoringMethod]);
                });

            KeyboardHandler.KeyAction(Keys.F, () =>
                {
                    if (wireFrame)
                    {
                        this.GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.None, FillMode = FillMode.Solid };
                    }
                    else
                    {
                        this.GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.None, FillMode = FillMode.WireFrame };
                    }

                    wireFrame = !wireFrame;
                });

            KeyboardHandler.KeyAction(Keys.L, this.terrain.SwitchLighting);

            this.camera.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            this.terrain.Render(world, this.camera.View, this.projection);
            base.Draw(gameTime);
        }
    }
}

