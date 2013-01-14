using System.Collections.Generic;
using System.Linq;
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
        private SoundEffect menuSelect;
        private bool nextButtonFocus;
        private bool backButtonFocus;

        public GameParameters GameParametersModel
        {
            get
            {
                IDictionary<string, string> state = this.CurrentOptionsDataState;
                string[] playerNames = state.Where(kvp => kvp.Key.Contains("PLAYER NAME")).Select(kvp => kvp.Value).ToArray();
                return new GameParameters(int.Parse(state["MAP SIZE"]), int.Parse(state["ROUGHNESS"]), int.Parse(state["MAX HEIGHT"]), int.Parse(state["LIGHT CHANGE SPEED"]), playerNames);
            }
        }

        /// <summary>
        /// Class constructor loads necessary elements.
        /// </summary>
        /// <param name="content">content manager.</param>
        public PlayPage(ContentManager content, GraphicsDevice graphicsDevice, GameParameters defaultGameParams)
            : base(content, graphicsDevice, Menu.AltBackgroundResourceName, new[]
                {
                    new TextBox(content, "FIRST PLAYER NAME", 0, "PLAYER1", false, 0, 0, new Vector2(-900, 400), 0.2f),
                    new TextBox(content, "SECOND PLAYER NAME", 1, "PLAYER2", false, 0, 0, new Vector2(-900, 200), 0.2f),
                    new TextBox(content, "MAP SIZE", 2, defaultGameParams.MapScale.ToString(), true, 0, 100, new Vector2(-900, 0), 0.2f),
                    new TextBox(content, "MAX HEIGHT", 3, defaultGameParams.MaxMapHeight.ToString(), true, 0, 500, new Vector2(-900, -200), 0.2f),
                    new MenuOption("BACK", 4, new Vector2(-700, -550), 1.1f),
                    new TextBox(content, "THIRD PLAYER NAME", 5, string.Empty, false, 0, 0, new Vector2(100, 400), 0.2f),
                    new TextBox(content, "FOURTH PLAYER NAME", 6, string.Empty, false, 0, 0, new Vector2(100, 200), 0.2f),
                    new TextBox(content, "ROUGHNESS", 7, defaultGameParams.Roughness.ToString(), true, 0, 1000, new Vector2(100, 0), 0.2f),
                    new TextBox(content, "LIGHT CHANGE SPEED", 8, defaultGameParams.LightChangeSpeed.ToString(), true, 0, 1000, new Vector2(100, -200), 0.2f),
                    new MenuOption("NEXT", 9, new Vector2(200, -550), 1.1f)
                })
        {
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
        }

        /// <summary>
        /// Updates this instance.
        /// </summary>
        public override void Update()
        {
            #region A fix for Issue #13- just enough for it to work.
            KeyboardHandler.KeyAction(Keys.Left, this.SwitchColumnLeft);
            KeyboardHandler.KeyAction(Keys.Right, this.SwitchColumnRight);
            #endregion

            KeyboardHandler.KeyAction(Keys.Up, this.SelectPreviousOption);
            KeyboardHandler.KeyAction(Keys.Down, this.SelectNextOption);

            KeyboardHandler.KeyAction(Keys.Enter, () =>
                {
                    if (!(CurrentOption is TextBox))
                    {
                        this.FireOptionChosen(this, this.CurrentOption.Index/5);
                    }
                });

            this.CurrentOption.Update();
            base.Update();
        }
    }
}
