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
namespace Tanks3DFPP.Menu
{
    /// <summary>
    /// Class used to handle loading page.
    /// </summary>
    public class LoadingPage
    {
        private MenuTank tank = new MenuTank();
        private Texture2D backGround;
        private Characters characters;
        private double loadingPercent = 0;
        private Model cube2;
        private Model cube3;
        private Model cube4;
        private float cubeRotationValue = 0;
        private Random rand = new Random();
        private Vector3 cubePosition;
        public int playernumber;
        public int lastPercent = 0;

        /// <summary>
        /// Class constructor used to load necessary elements.
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="num"></param>
        public LoadingPage(ContentManager Content)
        {
            backGround = Content.Load<Texture2D>("MenuContent/backGround");
            characters = new Characters(Content);
            cube2 = Content.Load<Model>("MenuContent/cube2");
            cube3 = Content.Load<Model>("MenuContent/cube3");
            cube4 = Content.Load<Model>("MenuContent/cube4");
            cubeRotationValue = rand.Next(70, 140);
            cubePosition = new Vector3(-700, 300, 0);
            tank.Load(Content, Matrix.Identity * Matrix.CreateRotationY(MathHelper.ToRadians(90.0f)) * Matrix.CreateTranslation(-1500, -500, -1200));
            tank.which = 1;
        }

        /// <summary>
        /// Method used to draw loading page.
        /// </summary>
        /// <param name="spritebatch">Spritebatch.</param>
        /// <param name="view">View matrix.</param>
        /// <param name="projection">Projection matrix.</param>
        /// <param name="GraphicsDevice">Graphics device.</param>
        public void showLoadingPage(SpriteBatch spritebatch, Matrix view, Matrix projection, GraphicsDevice GraphicsDevice)
        {
            spritebatch.Begin();
            spritebatch.Draw(backGround, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
       
            spritebatch.End();

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;
            tank.Draw(view, projection);
            characters.Draw("LOADING : ", 1.0f, new Vector3(-600, 0, 0), view, projection);
            if (loadingPercent >= 100)
                characters.Draw("PRESS ANY KEY TO CONTINUE...", 0.4f, new Vector3(-850, -500, 0), view, projection);
            Model cube = cube2;
            if (playernumber == 3)
                cube = cube3;
            if (playernumber == 4)
                cube = cube4;
            foreach (ModelMesh mesh in cube.Meshes)
            {
                Matrix[] transforms = new Matrix[cube.Bones.Count];
                cube.CopyAbsoluteBoneTransformsTo(transforms);
                foreach (BasicEffect effect in mesh.Effects)
                {
                    Matrix transform = Matrix.Identity * Matrix.CreateRotationX(MathHelper.ToRadians(cubeRotationValue)) * Matrix.CreateRotationY(MathHelper.ToRadians(cubeRotationValue)) * Matrix.CreateRotationZ(MathHelper.ToRadians(cubeRotationValue)) * Matrix.CreateScale(100) * Matrix.CreateTranslation(cubePosition);
                    effect.World = effect.World = transforms[mesh.ParentBone.Index] * transform;
                    effect.View = view;
                    effect.Projection = projection;
                    effect.EnableDefaultLighting();
                }
                mesh.Draw();
            }
        }

        /// <summary>
        /// Method used to update loading page.
        /// </summary>
        /// <param name="GraphicsDevice">Graphics device.</param>
        /// <param name="percent">Loading terrain percent value.</param>
        /// <returns>True if loading is done.</returns>
        public bool updateLoadingPage(GraphicsDevice GraphicsDevice, int percent)
        {
            bool result = false;
            cubeRotationValue += 5f;
            tank.wheelRotationValue += 5;
            if (percent >= 100)
            {
                percent = 100;
                whichSideOfTheCube();
            }
            if (Keyboard.GetState().GetPressedKeys().Length > 0 && percent == 100)
            {
                result = true;
            }
            if (!result && loadingPercent != percent)
            {
                tank.move(Matrix.CreateTranslation(new Vector3(27, 0, 0)));
                cubePosition += new Vector3(12, 0, 0);
                loadingPercent = percent;
            }
            return result;
        }

        /// <summary>
        /// Method used to get player number from cube.
        /// </summary>
        public int whichSideOfTheCube()
        {
            int result = 0;
            switch (playernumber)
            {
                case 2:
                    if (((cubeRotationValue % 360) > 20 && (cubeRotationValue % 360) < 150) || ((cubeRotationValue % 360) > 230 && (cubeRotationValue % 360) < 280))
                        result = 0;
                    else
                        result = 1;
                    break;
                case 3:
                    if (((cubeRotationValue % 360) > 20 && (cubeRotationValue % 360) < 150) || ((cubeRotationValue % 360) > 230 && (cubeRotationValue % 360) < 280))
                        result = 0;
                    else
                        result = 1;
                    break;
                case 4:
                    if (((cubeRotationValue % 360) > 20 && (cubeRotationValue % 360) < 150) || ((cubeRotationValue % 360) > 230 && (cubeRotationValue % 360) < 280))
                        result = 0;
                    else
                        result = 1;
                    break;
            }

            return result;
        }

    }
}
