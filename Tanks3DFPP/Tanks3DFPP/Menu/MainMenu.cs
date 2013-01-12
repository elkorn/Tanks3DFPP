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
    /// Class used to handle mainmenu page.
    /// </summary>
    class MainMenu
    {
        private MenuTank tank = new MenuTank();
        private Texture2D backGround;
        private Model a;
        private bool[] menuButtonOn = new bool[3];
        private bool buttonUpOn = false;
        private bool buttonDownOn = false;
        private SpriteBatch spriteBatch;
        private SoundEffect menuSelect;
        private Characters characters;

        /// <summary>
        /// Class constructor loads necessary elements.
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="GD"></param>
        public MainMenu(ContentManager Content, GraphicsDevice GD)
        {
            backGround = Content.Load<Texture2D>("MenuContent/xnuke.jpg.pagespeed.ic.XD9-0bi6PQ");
            menuSelect = Content.Load<SoundEffect>(@"MenuContent/menu_select");
            tank.Load(Content, Matrix.Identity * Matrix.CreateScale(0.8f) * Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(90.0f), MathHelper.ToRadians(0.0f), MathHelper.ToRadians(45.0f)) * Matrix.CreateTranslation(-300, -200, 0));
            menuButtonOn[0] = true;
            spriteBatch = new SpriteBatch(GD);
            characters = new Characters(Content);
        }

        /// <summary>
        /// Method used to update mainmenu page.
        /// </summary>
        /// <returns></returns>
        public int updateMainMenu()
        {
            int result = -1;
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.Enter))
            {
                int choice = -1;
                for (int i = 0; i < 3; ++i)
                {
                    if (menuButtonOn[i])
                    {
                        choice = i;
                    }
                }
                result = choice;
            }
            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                result = 2;
            }

            if (keyboardState.IsKeyDown(Keys.Up))
            {
                buttonUpOn = true;
            }
            if (keyboardState.IsKeyDown(Keys.Down))
            {
                buttonDownOn = true;
            }
            if (keyboardState.IsKeyUp(Keys.Up))
            {
                if (buttonUpOn)
                {
                    buttonUpOn = false;
                    for (int i = 0; i < 3; ++i)
                    {
                        if (menuButtonOn[i])
                        {
                            if (i != 0)
                            {
                                menuSelect.Play(0.5f, 0, 0);
                                tank.which = i - 1;
                                menuButtonOn[i] = false;
                                menuButtonOn[i - 1] = true;
                                break;
                            }
                        }
                    }
                }
                if (keyboardState.IsKeyUp(Keys.Down))
                {
                    if (buttonDownOn)
                    {
                        buttonDownOn = false;
                        for (int i = 0; i < 3; ++i)
                        {
                            if (menuButtonOn[i])
                            {
                                if (i != 2)
                                {
                                    menuSelect.Play(0.5f, 0, 0);
                                    tank.which = i + 1;
                                    menuButtonOn[i] = false;
                                    menuButtonOn[i + 1] = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Method to draw mainmenu.
        /// </summary>
        /// <param name="view"></param>
        /// <param name="projection"></param>
        /// <param name="GD"></param>
        public void showMainMenu(Matrix view, Matrix projection, GraphicsDevice GD)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(backGround, new Rectangle(0, 0, GD.Viewport.Width, GD.Viewport.Height), Color.White);
            spriteBatch.End();

            GD.BlendState = BlendState.Opaque;
            GD.DepthStencilState = DepthStencilState.Default;
            GD.SamplerStates[0] = SamplerState.LinearWrap;

            characters.Draw("TANKS 3D FPP", 1f, new Vector3(-900, 350, 0), view, projection);
            string[] menuButtonwsWords = new string[3];
            menuButtonwsWords[0] = "PLAY";
            menuButtonwsWords[1] = "HELP";
            menuButtonwsWords[2] = "QUIT";

            for (int i = 0; i < 3; ++i)
            {
                if (menuButtonOn[i])
                {
                    characters.Draw(menuButtonwsWords[i], 0.9f, new Vector3(200, 100 - 330 * i, 0), view, projection);
                }
                else
                {
                    characters.Draw(menuButtonwsWords[i], 0.7f, new Vector3(200, 100 - 330 * i, 0), view, projection);
                }
            }
            tank.Draw(view, projection);
        }

    }
}
