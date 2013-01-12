using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Tanks3DFPP.Utilities;

namespace Tanks3DFPP.Menu
{
    /// <summary>
    /// Class used to handle help page.
    /// </summary>
    internal class HelpPage: MenuPage
    {
        private Texture2D controls;

        /// <summary>
        /// Class constructor loads necessary elements.
        /// </summary>
        /// <param name="Content"></param>
        public HelpPage(ContentManager Content, GraphicsDevice graphicsDevice):
            base(Content,graphicsDevice, Menu.DefaultBackgroundResourceName, new MenuOption[] {})
        {
            controls = Content.Load<Texture2D>("MenuContent/controls");
        }

        /// <summary>
        /// Method used to draw help page.
        /// </summary>
        /// <param name="view">View matrix.</param>
        /// <param name="projection">Projection matrix.</param>
        public override void Draw(Matrix view, Matrix projection)
        {
            base.Draw(view, projection);
            this.DrawString("CONTROLS", 1.0f, new Vector2(-550, 350), view, projection);
            this.DrawString("PRESS ANY KEY TO CONTINUE...", 0.4f, new Vector2(-850, -600), view, projection);
        }

        /// <summary>
        /// Method used to update help page.
        /// </summary>
        /// <returns></returns>
        public override void Update()
        {
           KeyboardHandler.AnyKey(() =>
               {
                   this.FireOptionChosen(this);
               });
        }
    }
}
