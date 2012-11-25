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

namespace Tanks3DFPP
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {

        int mapSize = 9,
            roughness = 1000,
            maxHeight = 100;
        
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        private Vector3 cameraPosition, cameraLookAt;
        Matrix world, view, projection;
        Terrain.Terrain terrain;
        private float maxVerticalAngle = MathHelper.ToRadians(85);
        private float minVerticalAngle = MathHelper.ToRadians(-85);
        private float cameraSpeed = 140.0f;
        private bool leftMousePreviouslyDown;
        private Point mouseStartPoint;
        int currentColoringMethod = 0;
        bool wireFrame, fillModeSwitch;
        IHeightToColorTranslationMethod[] coloringMethods;
        //TODO: fix terrain generation
        //TODO: vertex indexing
        //TODO: FPP camera
        /*
         * TODO: Is one effect enough to draw everything? 
         * Is using separate effects for objects beneficial for any aspect? 
         * If so, how should they be divided?
         * 
         * Or maybe effects should be assigned by
         * how should different elements be rendered?
         */
        BasicEffect effect; 
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
            cameraPosition = new Vector3(100, 100, 100);
            cameraLookAt = Vector3.Zero;
            view = Matrix.CreateLookAt(cameraPosition, cameraLookAt, Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90), this.GraphicsDevice.Viewport.AspectRatio, 1, 2000f); // MathHelper.PiOver4

            base.Initialize();
            
            effect = new BasicEffect(this.GraphicsDevice);
            this.effect.World = world;
            this.effect.View = view;
            this.effect.Projection = projection;
            this.effect.VertexColorEnabled = true;
            
            //GraphicsDevice.RasterizerState = new RasterizerState { CullMode = Microsoft.Xna.Framework.Graphics.CullMode.None/*, FillMode = Microsoft.Xna.Framework.Graphics.FillMode.WireFrame*/ };
            coloringMethods = new IHeightToColorTranslationMethod[] 
            {
                new GreenYellowRed(maxHeight),
                new HeightToGrayscale(maxHeight)
            };

            this.terrain = new Terrain.Terrain(new FractalMap(mapSize, roughness, maxHeight, true), coloringMethods[currentColoringMethod]);
        }
        
        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            var ks = Keyboard.GetState();
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();


            Vector3 distance = this.cameraPosition - this.cameraLookAt;

            Vector3 viewDirection = distance;
            viewDirection.Normalize();

            if (ks.IsKeyDown(Keys.W))
            {
                Vector3 shift = (float)gameTime.ElapsedGameTime.TotalSeconds * viewDirection * cameraSpeed;
                this.cameraPosition -= shift;
                this.cameraLookAt -= shift;
            }

            if (ks.IsKeyDown(Keys.S))
            {
                Vector3 shift = (float)gameTime.ElapsedGameTime.TotalSeconds * viewDirection * cameraSpeed;
                this.cameraPosition += shift;
                this.cameraLookAt += shift;
            }

            // TODO: Add your update logic here
            if (ks.IsKeyDown(Keys.G))
            {
                this.terrain = new Terrain.Terrain(new FractalMap(mapSize, roughness, maxHeight, false), new GreenYellowRed(maxHeight));
            }

            if (ks.IsKeyDown(Keys.F))
            {
                if (!fillModeSwitch)
                {
                    fillModeSwitch = true;
                }
            }

            if (ks.IsKeyUp(Keys.F) && fillModeSwitch)
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
                fillModeSwitch = false;
            }



            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (!leftMousePreviouslyDown)
                {
                    this.mouseStartPoint = new Point(mouseState.X, mouseState.Y);
                }

                // orbiting
                float horizontalAngle = (float)Math.Atan2(distance.Z, distance.X);

                distance = Vector3.Transform(distance, Matrix.CreateRotationY(horizontalAngle));

                float verticalAngle = (float)Math.Atan2(distance.Y, distance.X);

                distance = Vector3.Transform(distance, Matrix.CreateRotationZ(-verticalAngle));

                distance = Vector3.Transform(distance, Matrix.CreateRotationZ(MathHelper.Clamp(verticalAngle + MathHelper.ToRadians(mouseState.Y - this.mouseStartPoint.Y), this.minVerticalAngle, this.maxVerticalAngle)));

                distance = Vector3.Transform(distance, Matrix.CreateRotationY(-horizontalAngle));

                this.cameraPosition = this.cameraLookAt + Vector3.Transform(distance, Matrix.CreateRotationY(-MathHelper.ToRadians(mouseState.X - this.mouseStartPoint.X)));

                this.mouseStartPoint = new Point(mouseState.X, mouseState.Y);

                //this.effect.View = Matrix.CreateLookAt(this.newCameraPosition, this.cameraLookAt, Vector3.Up);

                leftMousePreviouslyDown = true;
            }
            else
            {
                leftMousePreviouslyDown = false;
            }

            this.view = Matrix.CreateLookAt(this.cameraPosition, this.cameraLookAt, Vector3.Up);

            this.effect.View = this.view;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            this.effect.CurrentTechnique.Passes[0].Apply();
            
            // TODO: Add your drawing code here
            this.terrain.Render(this.GraphicsDevice);
            base.Draw(gameTime);
        }
    }
}
