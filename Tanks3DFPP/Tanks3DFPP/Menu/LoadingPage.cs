using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
namespace Tanks3DFPP.Menu
{
    /// <summary>
    /// Class used to handle loading page.
    /// </summary>
    internal class LoadingPage : MenuPage
    {
        private MenuTank tank = new MenuTank();
        private Model cube;
        private float cubeRotationValue = 0;
        private Random rand = new Random();

        private Vector3 cubePosition;

        private int targetPercent, _currentPercent = 0;

        private int currentPercent
        {
            get { return _currentPercent; }
            set
            {
                _currentPercent = value;
                if (_currentPercent >= 100)
                {
                    this.Ready.Invoke(this, null);
                }
            }
        }

        private readonly int dCubeMovement, dTankMovement;

        public event EventHandler Ready;

        /// <summary>
        /// Class constructor used to load necessary elements.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="num"></param>
        public LoadingPage(ContentManager content, GraphicsDevice graphicsDevice)
            : base(content, graphicsDevice, Menu.AltBackgroundResourceName, new MenuOption[] { })
        {
            dCubeMovement = (int)Math.Ceiling((12 / (float)graphicsDevice.Viewport.Width) * 100);
            dTankMovement = (int)Math.Ceiling((27 / (float)graphicsDevice.Viewport.Width) * 100);
            cube = content.Load<Model>(string.Format("MenuContent/cube{0}", Game1.GameParameters.NumberOfPlayers));
            cubeRotationValue = rand.Next(70, 140);
            cubePosition = new Vector3(-700, 400, 0);
            tank.Load(content, Matrix.Identity * Matrix.CreateRotationY(MathHelper.ToRadians(90.0f)) * Matrix.CreateTranslation(-1500, -500, -1200));
            tank.which = 1;
        }

        /// <summary>
        /// Method used to draw loading page.
        /// </summary>
        /// <param name="spritebatch">Spritebatch.</param>
        /// <param name="view">View matrix.</param>
        /// <param name="projection">Projection matrix.</param>
        /// <param name="GraphicsDevice">Graphics device.</param>
        public override void Draw(Matrix view, Matrix projection)
        {
            base.Draw(view, projection);
            tank.Draw(view, projection);
            this.DrawString("LOADING : ", 1.0f, new Vector2(-600, 0), view, projection);
            //if (loadingPercent >= 100)
            //    this.DrawString("PRESS ANY KEY TO CONTINUE...", 0.4f, new Vector2(-850, -500), view, projection);
            foreach (ModelMesh mesh in cube.Meshes)
            {
                Matrix[] transforms = new Matrix[cube.Bones.Count];
                cube.CopyAbsoluteBoneTransformsTo(transforms);
                foreach (BasicEffect effect in mesh.Effects)
                {
                    Matrix transform = Matrix.Identity * Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(cubeRotationValue), MathHelper.ToRadians(cubeRotationValue / 2), MathHelper.ToRadians(cubeRotationValue / 3)) * Matrix.CreateScale(100) * Matrix.CreateTranslation(cubePosition);
                    effect.World = effect.World = transforms[mesh.ParentBone.Index] * transform;
                    effect.View = view;
                    effect.Projection = projection;
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }
        }

        public void AddProgress(int howMuch)
        {
            this.targetPercent = (int)MathHelper.Clamp(this.targetPercent + howMuch, 0, 100);
        }

        public override void Update()
        {
            if (this.currentPercent < this.targetPercent && this.currentPercent < 100)
            {
                ++this.currentPercent;
                cubeRotationValue += 5;
                tank.wheelRotationValue += 5;
                tank.move(Matrix.CreateTranslation(Vector3.UnitX * 27));
                cubePosition += Vector3.UnitX * 12;
            }

            base.Update();
        }

        ///// <summary>
        ///// Method used to update loading page.
        ///// </summary>
        ///// <param name="GraphicsDevice">Graphics device.</param>
        ///// <param name="targetPercent">Loading terrain targetPercent value.</param>
        ///// <returns>True if loading is done.</returns>
        //public bool updateLoadingPage(GraphicsDevice GraphicsDevice, int targetPercent)
        //{
        //    bool result = false;
        //    if (targetPercent >= 100)
        //    {
        //        targetPercent = 100;
        //        result = true;
        //    }
        //    else
        //    {
        //        cubeRotationValue += 5f;
        //        tank.wheelRotationValue += 5;
        //    }

        //    if (!result && loadingPercent != targetPercent)
        //    {

        //        loadingPercent = targetPercent;
        //    }
        //    return result;
        //}

        /// <summary>
        /// Method used to get player number from cube.
        /// </summary>
        public int whichSideOfTheCube()
        {
            int result = 0;
            switch (Game1.GameParameters.NumberOfPlayers)
            {
                case 2:
                    if (((cubeRotationValue % 360) > 135 && (cubeRotationValue % 360) <= 160) || ((cubeRotationValue % 360) > 195 && (cubeRotationValue % 360) <= 240) || ((cubeRotationValue % 360) > 250 && (cubeRotationValue % 360) <= 285) || ((cubeRotationValue % 360) > 40 && (cubeRotationValue % 360) <= 110))
                        result = 0;
                    else
                        result = 1;
                    break;
                case 3:
                    if (((cubeRotationValue % 360) > 0 && (cubeRotationValue % 360) <= 20) || ((cubeRotationValue % 360) > 45 && (cubeRotationValue % 360) <= 90) || ((cubeRotationValue % 360) > 100 && (cubeRotationValue % 360) <= 110) || ((cubeRotationValue % 360) > 140 && (cubeRotationValue % 360) <= 145) || ((cubeRotationValue % 360) > 170 && (cubeRotationValue % 360) <= 180) || ((cubeRotationValue % 360) > 200 && (cubeRotationValue % 360) <= 210) || ((cubeRotationValue % 360) > 225 && (cubeRotationValue % 360) <= 240) || ((cubeRotationValue % 360) > 290 && (cubeRotationValue % 360) <= 300) || ((cubeRotationValue % 360) > 330 && (cubeRotationValue % 360) <= 340))
                        result = 0;
                    else if (((cubeRotationValue % 360) > 25 && (cubeRotationValue % 360) <= 35) || ((cubeRotationValue % 360) > 90 && (cubeRotationValue % 360) <= 100) || ((cubeRotationValue % 360) > 110 && (cubeRotationValue % 360) <= 120) || ((cubeRotationValue % 360) > 155 && (cubeRotationValue % 360) <= 170) || ((cubeRotationValue % 360) > 180 && (cubeRotationValue % 360) <= 200) || ((cubeRotationValue % 360) > 220 && (cubeRotationValue % 360) <= 225) || ((cubeRotationValue % 360) > 270 && (cubeRotationValue % 360) <= 290))
                        result = 1;
                    else
                        result = 2;
                    break;
                case 4:
                    if (((cubeRotationValue % 360) > 60 && (cubeRotationValue % 360) <= 65) || ((cubeRotationValue % 360) > 75 && (cubeRotationValue % 360) <= 80) || ((cubeRotationValue % 360) > 85 && (cubeRotationValue % 360) <= 100) || ((cubeRotationValue % 360) > 135 && (cubeRotationValue % 360) <= 145) || ((cubeRotationValue % 360) > 160 && (cubeRotationValue % 360) <= 175) || ((cubeRotationValue % 360) > 180 && (cubeRotationValue % 360) <= 200) || ((cubeRotationValue % 360) > 215 && (cubeRotationValue % 360) <= 225) || ((cubeRotationValue % 360) > 245 && (cubeRotationValue % 360) <= 265) || ((cubeRotationValue % 360) > 275 && (cubeRotationValue % 360) <= 285) || ((cubeRotationValue % 360) > 300 && (cubeRotationValue % 360) <= 310))
                        result = 0;
                    else if (((cubeRotationValue % 360) > 0 && (cubeRotationValue % 360) <= 20) || ((cubeRotationValue % 360) > 40 && (cubeRotationValue % 360) <= 50) || ((cubeRotationValue % 360) > 70 && (cubeRotationValue % 360) <= 75) || ((cubeRotationValue % 360) > 80 && (cubeRotationValue % 360) <= 85) || ((cubeRotationValue % 360) > 105 && (cubeRotationValue % 360) <= 130) || ((cubeRotationValue % 360) > 145 && (cubeRotationValue % 360) <= 160) || ((cubeRotationValue % 360) > 175 && (cubeRotationValue % 360) <= 180) || ((cubeRotationValue % 360) > 210 && (cubeRotationValue % 360) <= 215) || ((cubeRotationValue % 360) > 285 && (cubeRotationValue % 360) <= 300) || ((cubeRotationValue % 360) > 325 && (cubeRotationValue % 360) <= 340) || ((cubeRotationValue % 360) > 340 && (cubeRotationValue % 360) <= 355))
                        result = 1;
                    else if (((cubeRotationValue % 360) > 35 && (cubeRotationValue % 360) <= 40) || ((cubeRotationValue % 360) > 65 && (cubeRotationValue % 360) <= 70) || ((cubeRotationValue % 360) > 50 && (cubeRotationValue % 360) <= 60) || ((cubeRotationValue % 360) > 100 && (cubeRotationValue % 360) <= 105) || ((cubeRotationValue % 360) > 130 && (cubeRotationValue % 360) <= 135) || ((cubeRotationValue % 360) > 200 && (cubeRotationValue % 360) <= 210) || ((cubeRotationValue % 360) > 215 && (cubeRotationValue % 360) <= 235) || ((cubeRotationValue % 360) > 265 && (cubeRotationValue % 360) <= 275))
                        result = 2;
                    else
                        result = 3;
                    break;
            }

            return result;
        }

    }
}
