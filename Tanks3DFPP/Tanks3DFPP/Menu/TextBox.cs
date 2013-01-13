using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Tanks3DFPP.Menu
{
    /// <summary>
    /// Classused to handle textbox.
    /// </summary>
    internal class TextBox: MenuOption
    {
        private bool numericOnly;
        private float min, max;
        private string value;
        private bool keyDone;
        private SoundEffect typeWriter;
        private Vector3 valuePosition;

        private float actualScale
        {
            get
            {
                return this.HasFocus
                           ? this.baseScale * 1.5f
                           : this.baseScale * 1.3f;
            }
        }
        public bool HasFocus
        {
            get { return this.IsSelected; }
            set { this.IsSelected = value; }
        }

        /// <summary>
        /// Method used to get value.
        /// </summary>
        /// <returns>value.</returns>
        public string GetValue()
        {
            return value;
        }

        /// <summary>
        /// Class constructor loads necessary elements.
        /// </summary>
        /// <param name="Content">Content manager.</param>
        /// <param name="name">Name of textbox.</param>
        /// <param name="value">Value of textbox.</param>
        /// <param name="numericOnly">If true textbox object accepst only numeric keys.</param>
        /// <param name="minimum">Minimum numeric value if numericOnly = true.</param>
        /// <param name="maximum">Maximum numeric value if numericOnly = true.</param>
        /// <param name="position">Textbox position.</param>
        /// <param name="scale">Characters scale.</param>
        public TextBox(ContentManager Content, string name, int index, string value, bool numericOnly, float minimum, float maximum, Vector2 position, float scale)
            :base(name, index, position, scale)
        {
            this.value = value;
            this.numericOnly = numericOnly;
            min = minimum;
            max = maximum;
            this.valuePosition = new Vector3(position.X, position.Y - this.baseScale * 400f, 0);
            typeWriter = Content.Load<SoundEffect>(@"MenuContent/typewriter");
        }

        /// <summary>
        /// Method used to update textbox.
        /// </summary>
        public override void Update()
        {
            KeyboardState keyboardState = Game1.CurrentKeyboardState;
            if (keyboardState.GetPressedKeys().Length > 0)
            {
                if (!keyDone)
                {
                    int number = 0;
                    if (keyboardState.IsKeyDown(Keys.LeftShift))
                        number = 1;

                    if (numericOnly)
                    {
                        #region this makes me sick but is legit...
                        switch (keyboardState.GetPressedKeys()[number])
                        {
                            case Keys.Back:
                                if (value.Length > 0) value = value.Substring(0, value.Length - 1);
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.D0:
                                value += "0";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.D1:
                                value += "1";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.D2:
                                value += "2";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.D3:
                                value += "3";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.D4:
                                value += "4";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.D5:
                                value += "5";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.D6:
                                value += "6";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.D7:
                                value += "7";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.D8:
                                value += "8";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.D9:
                                value += "9";
                                typeWriter.Play(.5f,0,0);
                                break;
                        }
                        #endregion

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
                        #region this makes me sick but is legit...
                        switch (keyboardState.GetPressedKeys()[number])
                        {
                            case Keys.Back:
                                if (value.Length > 0) value = value.Substring(0, value.Length - 1);
                                break;
                            case Keys.D0:
                                value += "0";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.D1:
                                value += "1";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.D2:
                                value += "2";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.D3:
                                value += "3";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.D4:
                                value += "4";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.D5:
                                value += "5";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.D6:
                                value += "6";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.D7:
                                value += "7";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.D8:
                                value += "8";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.D9:
                                value += "9";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.OemPeriod:
                                value += ".";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.Q:
                                value += "Q";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.W:
                                value += "W";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.E:
                                value += "E";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.R:
                                value += "R";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.T:
                                value += "T";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.Y:
                                value += "Y";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.U:
                                value += "U";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.I:
                                value += "I";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.O:
                                value += "O";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.P:
                                value += "P";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.A:
                                value += "A";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.S:
                                value += "S";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.D:
                                value += "D";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.F:
                                value += "F";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.G:
                                value += "G";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.H:
                                value += "H";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.J:
                                value += "J";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.K:
                                value += "K";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.L:
                                value += "L";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.Z:
                                value += "Z";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.X:
                                value += "X";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.C:
                                value += "C";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.V:
                                value += "V";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.B:
                                value += "B";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.N:
                                value += "N";
                                typeWriter.Play(.5f,0,0);
                                break;
                            case Keys.M:
                                value += "M";
                                typeWriter.Play(.5f,0,0);
                                break;
                        }
                        #endregion
                    }

                    if (value.Length > 15)
                    {
                        value = value.Substring(0, 15);
                    }

                    keyDone = true;
                }
            }
            else
            {
                keyDone = false;
            }
        }

        /// <summary>
        /// Method used to draw textbox.
        /// </summary>
        /// <param name="view">View matrix.</param>
        /// <param name="projection">Projection  matrix.</param>
        public override void Draw(Characters characters, Matrix view, Matrix projection)
        {
            base.Draw(characters, view, projection);
            characters.Draw(value, actualScale, valuePosition, view, projection);
        }
    }
}

