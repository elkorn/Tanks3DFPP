using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tanks3DFPP.Utilities;

namespace Tanks3DFPP.Menu
{
    /// <summary>
    /// Class used to handle play page.
    /// </summary>
    internal class PlayPage : MenuPage
    {
        private List<TextBox> listOfTextBoxes = new List<TextBox>();
        private bool buttonUpOn;
        private bool buttonDownOn;
        private bool buttonLeftOn;
        private bool buttonRightOn;
        private SoundEffect menuSelect;
        private bool nextButtonFocus;
        private bool backButtonFocus;

        private GraphicsDevice gd;
        /// <summary>
        /// Class constructor loads necessary elements.
        /// </summary>
        /// <param name="content">content manager.</param>
        public PlayPage(ContentManager content, GraphicsDevice graphicsDevice, List<string> playerNames)
            : base(content, graphicsDevice, Menu.AltBackgroundResourceName, new[]
                {
                    new TextBox(content, "FIRST PLAYER NAME", 0, "PLAYER1", false, 0, 0, new Vector2(-900, 400), 0.2f),
                    new TextBox(content, "THIRD PLAYER NAME", 1, string.Empty, false, 0, 0, new Vector2(-900, 200), 0.2f),
                    new TextBox(content, "MAP SIZE", 2, Game1.MapScale.ToString(), true, 0, 100, new Vector2(-900, 0), 0.2f),
                    new TextBox(content, "MAP HEIGHT", 3, Game1.MaxHeight.ToString(), true, 0, 500, new Vector2(-900, -200), 0.2f),
                    new MenuOption("BACK", 4, new Vector2(-700, -550), 1.1f),
                    new TextBox(content, "SECOND PLAYER NAME", 5, "PLAYER2", false, 0, 0, new Vector2(100, 400), 0.2f),
                    new TextBox(content, "FOURTH PLAYER NAME", 6, string.Empty, false, 0, 0, new Vector2(100, 200), 0.2f),
                    new TextBox(content, "ROUGHNESS", 7, Game1.Roughness.ToString(), true, 0, 1000, new Vector2(100, 0), 0.2f),
                    new MenuOption("NEXT", 8, new Vector2(200, -550), 1.1f)
                })
        {
            //
            //Creating textboxes in loading page.
            //
            if (playerNames == null)
            {
                playerNames = new List<string>();
                playerNames.Add("PLAYER1");
                playerNames.Add("PLAYER2");
            }

            gd = graphicsDevice;
        }

        /// <summary>
        /// Method used to draw play page.
        /// </summary>
        /// <param name="spritebatch">Spritebatch</param>
        /// <param name="GraphicsDevice">Graphics device</param>
        /// <param name="view">View matrix.</param>
        /// <param name="projection">Projection matrix.</param>
        public override void Draw(Matrix view, Matrix projection)
        {
            base.Draw(view, projection);
            this.DrawString("IF YOU WANT TO PLAY YOU NEED TO HAVE A NAME.", 0.2f, new Vector2(-900, 550), view, projection);
            this.DrawString("THE GAME NEEDS AT LEAST TWO PLAYERS.", 0.2f, new Vector2(-900, 500), view, projection);

            //if (nextButtonFocus == false)
            //    characters.Draw("NEXT", 1.1f, new Vector3(200, -550, 0), view, projection);
            //else
            //    characters.Draw("NEXT", 1.3f, new Vector3(200, -550, 0), view, projection);

            //if (backButtonFocus == false)
            //    characters.Draw("BACK", 1.1f, new Vector3(-700, -550, 0), view, projection);
            //else
            //    characters.Draw("BACK", 1.3f, new Vector3(-700, -550, 0), view, projection);

            //for (int i = 0; i < listOfTextBoxes.Count; ++i)
            //{
            //    listOfTextBoxes[i].Draw(view, projection);
            //}
        }

        /// <summary>
        /// Method used to update play page.
        /// </summary>
        /// <returns>Method returns list including: 
        /// int (1 if update page is done 0 not done -1 back button pressed),
        /// map creation parameters in string,
        /// player names in  string.</returns>
        public List<object> updateplayPage()
        {
            List<object> result = new List<object>();
            result.Add(0);
            for (int i = 0; i < listOfTextBoxes.Count; ++i)
            {
                if (listOfTextBoxes[i].HasFocus)
                {
                    listOfTextBoxes[i].Update();
                    break;
                }
            }

            #region right button

            KeyboardHandler.KeyAction(Keys.Right, () =>
                {
                    int num = 0;
                    if (listOfTextBoxes.Count % 2 == 0)
                        num = listOfTextBoxes.Count / 2;
                    else
                        num = (listOfTextBoxes.Count - 1) / 2;

                    if (backButtonFocus)
                    {
                        menuSelect.Play(0.5f, 0, 0);
                        nextButtonFocus = true;
                        backButtonFocus = false;
                    }

                    for (int i = 0; i < listOfTextBoxes.Count; ++i)
                        if (listOfTextBoxes[i].HasFocus)
                        {
                            if (i <= num)
                                if (!backButtonFocus)
                                {
                                    if (i + num + 1 <= listOfTextBoxes.Count - 1)
                                    {
                                        menuSelect.Play(0.5f, 0, 0);
                                        listOfTextBoxes[i].HasFocus = false;
                                        listOfTextBoxes[i + num + 1].HasFocus = true;
                                    }
                                    break;
                                }
                                else
                                {
                                    menuSelect.Play(0.5f, 0, 0);
                                    listOfTextBoxes[i].HasFocus = false;
                                    nextButtonFocus = true;
                                }
                        }
                });

            #endregion

            #region down button

            KeyboardHandler.KeyAction(Keys.Down, () =>
                {
                    int num = 0;
                    if (listOfTextBoxes.Count % 2 == 0)
                        num = listOfTextBoxes.Count / 2;
                    else
                        num = (listOfTextBoxes.Count - 1) / 2;

                    for (int i = 0; i < listOfTextBoxes.Count; ++i)
                        if (listOfTextBoxes[i].HasFocus)
                        {
                            if (i == num || i == listOfTextBoxes.Count - 1)
                            {
                                menuSelect.Play(0.5f, 0, 0);
                                listOfTextBoxes[i].HasFocus = false;
                                if (i == listOfTextBoxes.Count - 1)
                                {
                                    nextButtonFocus = true;
                                }
                                if (i == num)
                                {
                                    backButtonFocus = true;
                                }
                            }
                            else
                            {
                                menuSelect.Play(0.5f, 0, 0);
                                listOfTextBoxes[i].HasFocus = false;
                                listOfTextBoxes[i + 1].HasFocus = true;
                                break;
                            }
                        }
                });

            #endregion

            #region up button

            KeyboardHandler.KeyAction(Keys.Up, () =>
                {
                        int num = 0;
                        if (listOfTextBoxes.Count%2 == 0)
                            num = listOfTextBoxes.Count/2;
                        else
                            num = (listOfTextBoxes.Count - 1)/2;

                        for (int i = 0; i < listOfTextBoxes.Count; ++i)
                            if (i > 0 && i != num + 1)
                                if (listOfTextBoxes[i].HasFocus)
                                {
                                    menuSelect.Play(0.5f, 0, 0);
                                    listOfTextBoxes[i].HasFocus = false;
                                    listOfTextBoxes[i - 1].HasFocus = true;
                                    break;
                                }

                        if (backButtonFocus)
                        {
                            menuSelect.Play(0.5f, 0, 0);
                            backButtonFocus = false;
                            listOfTextBoxes[num].HasFocus = true;
                        }
                        if (nextButtonFocus)
                        {
                            menuSelect.Play(0.5f, 0, 0);
                            nextButtonFocus = false;
                            listOfTextBoxes[listOfTextBoxes.Count - 1].HasFocus = true;
                        }
                });

            #endregion

            #region left button

            KeyboardHandler.KeyAction(Keys.Left, () =>
                {
                        int num = 0;
                        if (listOfTextBoxes.Count%2 == 0)
                            num = listOfTextBoxes.Count/2;
                        else
                            num = (listOfTextBoxes.Count - 1)/2;

                        if (nextButtonFocus)
                        {
                            menuSelect.Play(0.5f, 0, 0);
                            nextButtonFocus = false;
                            backButtonFocus = true;
                        }
                        else
                            for (int i = 0; i < listOfTextBoxes.Count; ++i)
                                if (i > num)
                                    if (listOfTextBoxes[i].HasFocus)
                                    {
                                        menuSelect.Play(0.5f, 0, 0);
                                        listOfTextBoxes[i].HasFocus = false;
                                        listOfTextBoxes[i - num - 1].HasFocus = true;
                                        break;
                                    }
                });

            #endregion

            #region escape button

            KeyboardHandler.KeyAction(Keys.Escape, () =>
                {
                    result[0] = -1;
                    backButtonFocus = false;
                    nextButtonFocus = false;
                    listOfTextBoxes[0].HasFocus = true;
                });

            #endregion

            #region enter button

            KeyboardHandler.KeyAction(Keys.Enter, () =>
                {
                    if (nextButtonFocus)
                    {
                        int numberOfPlayers = 0;
                        if (listOfTextBoxes[0].GetValue() != "")
                            numberOfPlayers++;
                        if (listOfTextBoxes[4].GetValue() != "")
                            numberOfPlayers++;
                        if (listOfTextBoxes[1].GetValue() != "")
                            numberOfPlayers++;
                        if (listOfTextBoxes[5].GetValue() != "")
                            numberOfPlayers++;

                        // there must be minimum 2 players
                        if (numberOfPlayers >= 2)
                        {
                            result[0] = 1;
                            nextButtonFocus = false;

                            //adding map creation variables
                            //map size
                            result.Add(listOfTextBoxes[2].GetValue());
                            //map height
                            result.Add(listOfTextBoxes[3].GetValue());
                            //map roughness
                            result.Add(listOfTextBoxes[6].GetValue());

                            //
                            //adding player names in result
                            //
                            //first player name
                            if (listOfTextBoxes[0].GetValue() != "")
                                result.Add(listOfTextBoxes[0].GetValue());
                            //second player name
                            if (listOfTextBoxes[4].GetValue() != "")
                                result.Add(listOfTextBoxes[4].GetValue());
                            //third player name
                            if (listOfTextBoxes[1].GetValue() != "")
                                result.Add(listOfTextBoxes[0].GetValue());
                            //fourth player name
                            if (listOfTextBoxes[5].GetValue() != "")
                                result.Add(listOfTextBoxes[5].GetValue());

                            listOfTextBoxes[0].HasFocus = true;
                        }
                    }
                    if (backButtonFocus)
                    {
                        result[0] = -1;
                        backButtonFocus = false;
                        listOfTextBoxes[0].HasFocus = true;
                    }
                });

            #endregion

            return result;
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public override void Update()
        {
            base.Update();

        }

    }
}
