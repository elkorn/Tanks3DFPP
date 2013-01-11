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
    /// Class used to handle play page.
    /// </summary>
    public class PlayPage
    {

        private Characters characters;
        private Texture2D backGround;
        private List<TextBox> listOfTextBoxes = new List<TextBox>();
        private bool buttonUpOn = false;
        private bool buttonDownOn = false;
        private bool buttonLeftOn = false;
        private bool buttonRightOn = false;
        private SoundEffect menuSelect;
        private bool nextButtonFocus = false;
        private bool backButtonFocus = false;

        /// <summary>
        /// Class constructor loads necessary elements.
        /// </summary>
        /// <param name="Content">Content manager.</param>
        public PlayPage(ContentManager Content,List<string> playerNames,int mapSize,int roughness,int maxHeight)
        {
            backGround = Content.Load<Texture2D>("MenuContent/TextureX steel texture shiney piece metal silver camo pattern Texture (1)");
            menuSelect = Content.Load<SoundEffect>("MenuContent/menu_select");
            characters = new Characters(Content);
            //
            //Creating textboxes in loading page.
            //
            if (playerNames ==null)
            {
                playerNames = new List<string>();
                playerNames.Add("PLAYER1");
                playerNames.Add("PLAYER2");
            }
            //
            //first column in menu
            //
            listOfTextBoxes.Add(new TextBox(Content, "FIRST PLAYER NAME", playerNames[0], false, 0, 0, new Vector3(-900, 400, 0), 0.2f));
            listOfTextBoxes.Add(new TextBox(Content, "THIRD PLAYER NAME", playerNames.Count>2 ? playerNames[2]:"", false, 0, 0, new Vector3(-900, 200, 0), 0.2f));
            listOfTextBoxes.Add(new TextBox(Content, "MAP SIZE", mapSize.ToString(), true, 0, 100, new Vector3(-900, 0, 0), 0.2f));
            listOfTextBoxes.Add(new TextBox(Content, "MAP HEIGHT", maxHeight.ToString(), true, 0, 500, new Vector3(-900, -200, 0), 0.2f));

            //
            //second column in menu
            //
            listOfTextBoxes.Add(new TextBox(Content, "SECOND PLAYER NAME", playerNames[1], false, 0, 0, new Vector3(100, 400, 0), 0.2f));
            listOfTextBoxes.Add(new TextBox(Content, "FOURTH PLAYER NAME", playerNames.Count > 3 ? playerNames[3] : "", false, 0, 0, new Vector3(100, 200, 0), 0.2f));
            listOfTextBoxes.Add(new TextBox(Content, "ROUGHNESS", roughness.ToString(), true, 0, 1000, new Vector3(100, 0, 0), 0.2f));

            listOfTextBoxes[0].SetFocus(true);
        }

        /// <summary>
        /// Method used to draw play page.
        /// </summary>
        /// <param name="spritebatch">Spritebatch</param>
        /// <param name="GraphicsDevice">Graphics device</param>
        /// <param name="view">View matrix.</param>
        /// <param name="projection">Projection matrix.</param>
        public void showPlayPage(SpriteBatch spritebatch, GraphicsDevice GraphicsDevice, Matrix view, Matrix projection)
        {
            spritebatch.Begin();
            spritebatch.Draw(backGround, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            spritebatch.End();

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            characters.Draw("IF YOU WANT TO PLAY YOU NEED TO HAVE A NAME.", 0.2f, new Vector3(-900, 550, 0), view, projection);
            characters.Draw("THE GAME NEEDS AT LEAST TWO PLAYERS.", 0.2f, new Vector3(-900, 500, 0), view, projection);

            if (nextButtonFocus == false)
                characters.Draw("NEXT", 1.1f, new Vector3(200, -550, 0), view, projection);
            else
                characters.Draw("NEXT", 1.3f, new Vector3(200, -550, 0), view, projection);

            if (backButtonFocus == false)
                characters.Draw("BACK", 1.1f, new Vector3(-700, -550, 0), view, projection);
            else
                characters.Draw("BACK", 1.3f, new Vector3(-700, -550, 0), view, projection);

            for (int i = 0; i < listOfTextBoxes.Count; ++i)
            {
                listOfTextBoxes[i].drawTextBox(spritebatch, GraphicsDevice, view, projection);
            }
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
                if (listOfTextBoxes[i].GetFocus())
                {
                    listOfTextBoxes[i].updateTextBox();
                    break;
                }
            }

            KeyboardState keyboardState = Keyboard.GetState();


            if (keyboardState.IsKeyDown(Keys.Up))
            {
                buttonUpOn = true;
            }
            if (keyboardState.IsKeyDown(Keys.Down))
            {
                buttonDownOn = true;
            }

            if (keyboardState.IsKeyDown(Keys.Left))
            {
                buttonLeftOn = true;
            }
            if (keyboardState.IsKeyDown(Keys.Right))
            {
                buttonRightOn = true;
            }

            #region right button

            if (keyboardState.IsKeyUp(Keys.Right))
            {
                if (buttonRightOn)
                {
                    buttonRightOn = false;

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
                        if (listOfTextBoxes[i].GetFocus())
                        {
                            if (i <= num)
                                if (!backButtonFocus)
                                {
                                    if (i + num + 1 <= listOfTextBoxes.Count - 1)
                                    {
                                        menuSelect.Play(0.5f, 0, 0);
                                        listOfTextBoxes[i].SetFocus(false);
                                        listOfTextBoxes[i + num + 1].SetFocus(true);
                                    }
                                    break;
                                }
                                else
                                {
                                    menuSelect.Play(0.5f, 0, 0);
                                    listOfTextBoxes[i].SetFocus(false);
                                    nextButtonFocus = true;
                                }
                        }
                }
            }

            #endregion

            #region down button

            if (keyboardState.IsKeyUp(Keys.Down))
            {
                if (buttonDownOn)
                {
                    buttonDownOn = false;

                    int num = 0;
                    if (listOfTextBoxes.Count % 2 == 0)
                        num = listOfTextBoxes.Count / 2;
                    else
                        num = (listOfTextBoxes.Count - 1) / 2;

                    for (int i = 0; i < listOfTextBoxes.Count; ++i)
                        if (listOfTextBoxes[i].GetFocus())
                        {
                            if (i == num || i == listOfTextBoxes.Count - 1)
                            {
                                menuSelect.Play(0.5f, 0, 0);
                                listOfTextBoxes[i].SetFocus(false);
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
                                listOfTextBoxes[i].SetFocus(false);
                                listOfTextBoxes[i + 1].SetFocus(true);
                                break;
                            }
                        }
                }
            }

            #endregion

            #region up button

            if (keyboardState.IsKeyUp(Keys.Up))
            {
                if (buttonUpOn)
                {
                    buttonUpOn = false;
                    int num = 0;
                    if (listOfTextBoxes.Count % 2 == 0)
                        num = listOfTextBoxes.Count / 2;
                    else
                        num = (listOfTextBoxes.Count - 1) / 2;

                    for (int i = 0; i < listOfTextBoxes.Count; ++i)
                        if (i > 0 && i != num + 1)
                            if (listOfTextBoxes[i].GetFocus())
                            {
                                menuSelect.Play(0.5f, 0, 0);
                                listOfTextBoxes[i].SetFocus(false);
                                listOfTextBoxes[i - 1].SetFocus(true);
                                break;
                            }

                    if (backButtonFocus)
                    {
                        menuSelect.Play(0.5f, 0, 0);
                        backButtonFocus = false;
                        listOfTextBoxes[num].SetFocus(true);
                    }
                    if (nextButtonFocus)
                    {
                        menuSelect.Play(0.5f, 0, 0);
                        nextButtonFocus = false;
                        listOfTextBoxes[listOfTextBoxes.Count - 1].SetFocus(true);
                    }
                }
            }

            #endregion

            #region left button

            if (keyboardState.IsKeyUp(Keys.Left))
            {
                if (buttonLeftOn)
                {
                    buttonLeftOn = false;

                    int num = 0;
                    if (listOfTextBoxes.Count % 2 == 0)
                        num = listOfTextBoxes.Count / 2;
                    else
                        num = (listOfTextBoxes.Count - 1) / 2;

                    if (nextButtonFocus)
                    {
                        menuSelect.Play(0.5f, 0, 0);
                        nextButtonFocus = false;
                        backButtonFocus = true;
                    }
                    else
                        for (int i = 0; i < listOfTextBoxes.Count; ++i)
                            if (i > num)
                                if (listOfTextBoxes[i].GetFocus())
                                {
                                    menuSelect.Play(0.5f, 0, 0);
                                    listOfTextBoxes[i].SetFocus(false);
                                    listOfTextBoxes[i - num - 1].SetFocus(true);
                                    break;
                                }


                }
            }

            #endregion

            #region escape button

            if (keyboardState.IsKeyDown(Keys.Escape))
            {
                result[0] = -1;
                backButtonFocus = false;
                nextButtonFocus = false;
                listOfTextBoxes[0].SetFocus(true);
            }

            #endregion

            #region enter button

            if (keyboardState.IsKeyDown(Keys.Enter))
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

                        listOfTextBoxes[0].SetFocus(true);
                    }
                }
                if (backButtonFocus)
                {
                    result[0] = -1;
                    backButtonFocus = false;
                    listOfTextBoxes[0].SetFocus(true);
                }
            }

            #endregion

            return result;
        }

    }
}
