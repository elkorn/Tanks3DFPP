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
    /// Classused to handle textbox.
    /// </summary>
    public class TextBox
    {

        private bool numericOnly = false;
        private float min, max;
        private string name;
        private string value;
        private bool focus = false;
        private bool keyDone = false;
        private Vector3 position;
        private Characters characters;
        private float scale;
        private SoundEffect typeWriter;

        /// <summary>
        /// Method used to get value.
        /// </summary>
        /// <returns>value.</returns>
        public string GetValue()
        {
            return value;
        }

        /// <summary>
        /// Method used to set focus variable.
        /// </summary>
        /// <param name="f">Bool variable to set.</param>
        public void SetFocus(bool f)
        {
            focus = f;
        }

        /// <summary>
        /// Method used to get Focus.
        /// </summary>
        /// <returns>Focus.</returns>
        public bool GetFocus()
        {
            return focus;
        }

        /// <summary>
        /// Class constructor loads necessary elements.
        /// </summary>
        /// <param name="Content">Content manager.</param>
        /// <param name="nam">Name of textbox.</param>
        /// <param name="val">Value of textbox.</param>
        /// <param name="no">If true textbox object accepst only numeric keys.</param>
        /// <param name="mi">Minimum numeric value if no = true.</param>
        /// <param name="ma">Maximum numeric value if no = true.</param>
        /// <param name="pos">Textbox position.</param>
        /// <param name="sca">Characters scale.</param>
        public TextBox(ContentManager Content, string nam, string val, bool no, float mi, float ma, Vector3 pos, float sca)
        {
            name = nam;
            value = val;
            numericOnly = no;
            min = mi;
            max = ma;
            characters = new Characters(Content);
            position = pos;
            scale = sca;
            typeWriter = Content.Load<SoundEffect>(@"MenuContent/typewriter");
        }

        /// <summary>
        /// Method used to update textbox.
        /// </summary>
        public void updateTextBox()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            if (keyboardState.GetPressedKeys().Length > 0)
            {
                if (keyDone == false)
                {
                    int number = 0;
                    if (keyboardState.IsKeyDown(Keys.LeftShift))
                        number = 1;

                    if (numericOnly)
                    {
                        switch (keyboardState.GetPressedKeys()[number])
                        {
                            case Keys.Back: if (value.Length > 0) value = value.Substring(0, value.Length - 1); typeWriter.Play(); break;
                            case Keys.D0: value += "0"; typeWriter.Play(); break;
                            case Keys.D1: value += "1"; typeWriter.Play(); break;
                            case Keys.D2: value += "2"; typeWriter.Play(); break;
                            case Keys.D3: value += "3"; typeWriter.Play(); break;
                            case Keys.D4: value += "4"; typeWriter.Play(); break;
                            case Keys.D5: value += "5"; typeWriter.Play(); break;
                            case Keys.D6: value += "6"; typeWriter.Play(); break;
                            case Keys.D7: value += "7"; typeWriter.Play(); break;
                            case Keys.D8: value += "8"; typeWriter.Play(); break;
                            case Keys.D9: value += "9"; typeWriter.Play(); break;
                            //case Keys.OemPeriod: value += "."; break;
                        }
                        if (value.Length > 15) value = value.Substring(0, 15);
                        if (value.Length > 0)
                        {
                            float num = 0;
                            if (float.TryParse(value, out num))
                            {
                                if (!(num >= min && num <= max))
                                {
                                    value = value.Substring(0, value.Length - 1);
                                }
                            }
                            else
                                value = value.Substring(0, value.Length - 1);
                        }
                    }
                    else
                    {
                        switch (keyboardState.GetPressedKeys()[number])
                        {
                            case Keys.Back: if (value.Length > 0) value = value.Substring(0, value.Length - 1); break;
                            case Keys.D0: value += "0"; typeWriter.Play(); break;
                            case Keys.D1: value += "1"; typeWriter.Play(); break;
                            case Keys.D2: value += "2"; typeWriter.Play(); break;
                            case Keys.D3: value += "3"; typeWriter.Play(); break;
                            case Keys.D4: value += "4"; typeWriter.Play(); break;
                            case Keys.D5: value += "5"; typeWriter.Play(); break;
                            case Keys.D6: value += "6"; typeWriter.Play(); break;
                            case Keys.D7: value += "7"; typeWriter.Play(); break;
                            case Keys.D8: value += "8"; typeWriter.Play(); break;
                            case Keys.D9: value += "9"; typeWriter.Play(); break;
                            case Keys.OemPeriod: value += "."; typeWriter.Play(); break;
                            case Keys.Q: value += "Q"; typeWriter.Play(); break;
                            case Keys.W: value += "W"; typeWriter.Play(); break;
                            case Keys.E: value += "E"; typeWriter.Play(); break;
                            case Keys.R: value += "R"; typeWriter.Play(); break;
                            case Keys.T: value += "T"; typeWriter.Play(); break;
                            case Keys.Y: value += "Y"; typeWriter.Play(); break;
                            case Keys.U: value += "U"; typeWriter.Play(); break;
                            case Keys.I: value += "I"; typeWriter.Play(); break;
                            case Keys.O: value += "O"; typeWriter.Play(); break;
                            case Keys.P: value += "P"; typeWriter.Play(); break;
                            case Keys.A: value += "A"; typeWriter.Play(); break;
                            case Keys.S: value += "S"; typeWriter.Play(); break;
                            case Keys.D: value += "D"; typeWriter.Play(); break;
                            case Keys.F: value += "F"; typeWriter.Play(); break;
                            case Keys.G: value += "G"; typeWriter.Play(); break;
                            case Keys.H: value += "H"; typeWriter.Play(); break;
                            case Keys.J: value += "J"; typeWriter.Play(); break;
                            case Keys.K: value += "K"; typeWriter.Play(); break;
                            case Keys.L: value += "L"; typeWriter.Play(); break;
                            case Keys.Z: value += "Z"; typeWriter.Play(); break;
                            case Keys.X: value += "X"; typeWriter.Play(); break;
                            case Keys.C: value += "C"; typeWriter.Play(); break;
                            case Keys.V: value += "V"; typeWriter.Play(); break;
                            case Keys.B: value += "B"; typeWriter.Play(); break;
                            case Keys.N: value += "N"; typeWriter.Play(); break;
                            case Keys.M: value += "M"; typeWriter.Play(); break;
                        }
                        if (value.Length > 15) value = value.Substring(0, 15);
                    }

                    keyDone = true;
                }
            }
            else
                keyDone = false;
        }

        /// <summary>
        /// Method used to draw textbox.
        /// </summary>
        /// <param name="spritebatch">Spritebatch.</param>
        /// <param name="GraphicsDevice">Graphics device.</param>
        /// <param name="view">View matrix.</param>
        /// <param name="projection">Projection  matrix.</param>
        public void drawTextBox(SpriteBatch spritebatch, GraphicsDevice GraphicsDevice, Matrix view, Matrix projection)
        {
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            if (focus == false)
            {
                characters.Draw(name, scale, position, view, projection);
                Vector3 temp = new Vector3(position.X, position.Y - scale * 400f, position.Z);
                characters.Draw(value, scale * 1.3f, temp, view, projection);
            }
            else
            {
                characters.Draw(name, scale * 1.2f, position, view, projection);
                Vector3 temp = new Vector3(position.X, position.Y - scale * 400f, position.Z);
                characters.Draw(value, scale * 1.5f, temp, view, projection);
            }
        }

    }
}

