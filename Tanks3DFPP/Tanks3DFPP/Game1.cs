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
using Tanks3DFPP.Terrain.Optimization.QuadTree;
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


        private static int nodesCount = 0;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Matrix world, projection;
        QuadTreeTerrain terrain;
        //Terrain.MultiTexturedTerrain terrain;
        private static IList<QuadNode> nodes = new List<QuadNode>();

        SpriteFont font;
        ICamera camera;
        bool wireFrame;
        public static KeyboardState CurrentKeyboardState { get; private set; }
        public static MouseState CurrentMouseState { get; private set; }
        IHeightToColorTranslationMethod[] coloringMethods;

        CollisionSphere sphere;
        TankController tankController;

        public static IHeightMap heightMap;
        public static int Scale = 2;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
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
            if (rendered)
            {
                if (!nodes.Contains(node))
                {
                    nodes.Add(node);
                    nodesCount++;
                }
            }
            else
            {
                if (nodes.Contains(node))
                {
                    nodes.Remove(node);
                    nodesCount--;
                }
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

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), this.GraphicsDevice.Viewport.AspectRatio, 1, 20000f);
            this.camera = new FPPCamera(this.GraphicsDevice, new Vector3(100, maxHeight * Scale, 100), 0.3f, 2.0f, this.projection);

            heightMap = new FractalMap(mapSize, roughness, maxHeight);
            coloringMethods = new IHeightToColorTranslationMethod[] 
            {
                new GreenYellowRed(maxHeight),
                new HeightToGrayscale(maxHeight)
            };

            spriteBatch = new SpriteBatch(this.GraphicsDevice);
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

            nodes.Clear();
            this.terrain = new QuadTreeTerrain(this, Vector3.Zero, heightMap = new FractalMap(mapSize, roughness, maxHeight, 1), this.camera.View, this.projection, Scale);
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
            Game1.CurrentKeyboardState = Keyboard.GetState();
            Game1.CurrentMouseState = Mouse.GetState();
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
                || CurrentKeyboardState.IsKeyDown(Keys.Escape))
                this.Exit();

            //KeyboardHandler.KeyAction(Keys.C, () =>
            //{
            //    currentColoringMethod += 1;
            //    currentColoringMethod %= this.coloringMethods.Length;
            //});

            KeyboardHandler.KeyAction(Keys.G, GenerateEverything);

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

            this.terrain.Update(this.camera);
            sphere.Update(gameTime);
            this.camera.Update(gameTime);
            tankController.Update(gameTime);
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

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            GraphicsDevice.Clear(Color.Black);
            sphere.Draw(world, this.camera.View, projection);
            //this.terrain.Draw(this.world, this.camera.View, this.projection);
            this.terrain.Draw(gameTime);
            tankController.Draw(this.camera.View, this.projection);
            spriteBatch.Begin();
            spriteBatch.DrawString(font, string.Format("Near: {0}, Far: {1}, nodes: {2}", camera.Frustum.Near.D, camera.Frustum.Far.D, nodesCount), Vector2.Zero, Color.Wheat);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}


