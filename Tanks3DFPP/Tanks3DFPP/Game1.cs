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

        int mapSize = 10,
            roughness = 500,
            maxHeight = 300;

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Matrix world, projection;
        Terrain.TexturedTerrain terrain;
        SpriteFont font;
        ICamera camera;
        int currentColoringMethod = 1;
        bool wireFrame;
        public static KeyboardState CurrentKeyboardState { get; private set; }
        public static MouseState CurrentMouseState { get; private set; }
        IHeightToColorTranslationMethod[] coloringMethods;
        IHeightMap heightMap;
        CollisionSphere sphere;

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

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), this.GraphicsDevice.Viewport.AspectRatio, 1, 2000f);
            this.camera = new FPPCamera(this.GraphicsDevice, new Vector3(100, 255, 100), 0.3f, 2.0f, this.projection);

            heightMap = new FractalMap(mapSize, roughness, maxHeight);
            coloringMethods = new IHeightToColorTranslationMethod[] 
            {
                new GreenYellowRed(maxHeight),
                new HeightToGrayscale(maxHeight)
            };

            spriteBatch = new SpriteBatch(this.GraphicsDevice);

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            this.terrain = new Terrain.TexturedTerrain(this.GraphicsDevice, this.Content, heightMap);
            font = this.Content.Load<SpriteFont>("SpriteFont1");
            sphere = new CollisionSphere(this, heightMap, new Vector3((heightMap.Width - 50) / 2, 100, (heightMap.Height - 50) / 2));
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
                this.terrain.Dispose();
                this.terrain = new Terrain.TexturedTerrain(this.GraphicsDevice, this.Content, new FractalMap(mapSize, roughness, maxHeight));
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
            sphere.Update(gameTime);
            this.camera.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.Clear(Color.Black);
            sphere.Draw(world, this.camera.View, projection);
            this.terrain.Render(world, this.camera.View, this.projection);
            BoundingFrustumRenderer.Render(this.camera.Frustum, this.GraphicsDevice, this.camera.View, this.projection, Color.Red);
            spriteBatch.Begin();
            spriteBatch.DrawString(font, string.Format("Near: {0}, Far: {1}", camera.Frustum.Near.D, camera.Frustum.Far.D), Vector2.Zero, Color.Wheat);
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}

