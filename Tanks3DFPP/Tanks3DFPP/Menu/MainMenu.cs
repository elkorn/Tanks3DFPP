using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tanks3DFPP.Utilities;

namespace Tanks3DFPP.Menu
{
    /// <summary>
    /// Class used to handle mainmenu page.
    /// </summary>
    class MainMenu : MenuPage
    {
        private MenuTank tank = new MenuTank();

        /// <summary>
        /// Class constructor loads necessary elements.
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="GD"></param>
        public MainMenu(ContentManager content, GraphicsDevice graphicsDevice)
            : base(content, graphicsDevice, "MenuContent/xnuke.jpg.pagespeed.ic.XD9-0bi6PQ", new[] {"PLAY", "HELP", "QUIT"})
        {
            tank.Load(content, Matrix.Identity * Matrix.CreateScale(0.8f) * Matrix.CreateFromYawPitchRoll(MathHelper.ToRadians(90.0f), MathHelper.ToRadians(0.0f), MathHelper.ToRadians(45.0f)) * Matrix.CreateTranslation(-300, -200, 0));
            this.OptionChanged += (sender, e) =>
                {
                    tank.which = this.CurrentOption.Index;
                };
        }

        /// <summary>
        /// Method used to update mainmenu page.
        /// </summary>
        /// <returns></returns>
        public int updateMainMenu()
        {
            int result = -1;
            KeyboardHandler.KeyAction(Keys.Enter, () =>
                {
                    
                });

            KeyboardHandler.KeyAction(Keys.Up, this.SelectPreviousOption);
            KeyboardHandler.KeyAction(Keys.Down, this.SelectNextOption);

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
            base.Draw(view, projection);
            tank.Draw(view, projection);
        }

    }
}
