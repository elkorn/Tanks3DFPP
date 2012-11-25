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
            roughness = 200,
            maxHeight = 255;

        /*
         * TODO: Is one effect enough to draw everything? 
         * Is using separate effects for objects beneficial for any aspect? 
         * If so, how should they be divided?
         * 
         * Or maybe effects should be assigned by
         * how should different elements be rendered?
         */
        BasicEffect effect;
        Vector3 lightDirection;
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
        int currentColoringMethod = 1;
        bool wireFrame;
        IHeightToColorTranslationMethod[] coloringMethods;

        private static Dictionary<Action, bool> actionSafeGuards = new Dictionary<Action, bool>();

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        void KeyAction(Keys key, Action action)
        {
            var ks = Keyboard.GetState();
            if (ks.IsKeyDown(key))
            {
                if (!actionSafeGuards.ContainsKey(action))
                {
                    actionSafeGuards.Add(action, false);
                }
                else
                {
                    if (!actionSafeGuards[action])
                    {
                        actionSafeGuards[action] = true;
                    }
                }
            }

            if (ks.IsKeyUp(key) && actionSafeGuards.ContainsKey(action) && actionSafeGuards[action])
            {
                action.Invoke();
                actionSafeGuards[action] = false;
            }
        }

        void TurboKeyAction(Keys key, Action action)
        {
            if (Keyboard.GetState().IsKeyDown(key))
            {
                action.Invoke();
            }
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

            lightDirection = new Vector3(1, -1, -1);
            lightDirection.Normalize();
            this.effect.LightingEnabled = true;
            this.effect.PreferPerPixelLighting = true;
            this.effect.DirectionalLight0.Direction = lightDirection;

            this.IsMouseVisible = true;
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

            TurboKeyAction(Keys.W, () =>
                {
                    Vector3 shift = (float)gameTime.ElapsedGameTime.TotalSeconds * viewDirection * cameraSpeed;
                    this.cameraPosition -= shift;
                    this.cameraLookAt -= shift;
                });

            TurboKeyAction(Keys.S, () =>
            {
                Vector3 shift = (float)gameTime.ElapsedGameTime.TotalSeconds * viewDirection * cameraSpeed;
                this.cameraPosition += shift;
                this.cameraLookAt += shift;
            });

            KeyAction(Keys.G, () =>
                {
                    this.terrain = new Terrain.Terrain(new FractalMap(mapSize, roughness, maxHeight, false), coloringMethods[currentColoringMethod]);
                });

            KeyAction(Keys.F, () =>
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

            KeyAction(Keys.L, () =>
                {
                    this.effect.LightingEnabled = !this.effect.LightingEnabled;
                });

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
            GraphicsDevice.Clear(Color.Black);
            this.effect.CurrentTechnique.Passes[0].Apply();

            // TODO: Add your drawing code here
            this.terrain.Render(this.GraphicsDevice);
            base.Draw(gameTime);
        }
    }
}
