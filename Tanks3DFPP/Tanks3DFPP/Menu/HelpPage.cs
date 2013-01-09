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
    /// Class used to handle help page.
    /// </summary>
    public class HelpPage
    {

        private bool waitOn = false;
        private Texture2D backGround;
        private Texture2D controls;
        private Characters characters;

        /// <summary>
        /// Class constructor loads necessary elements.
        /// </summary>
        /// <param name="Content"></param>
        public HelpPage(ContentManager Content)
        {
            characters = new Characters(Content);
            backGround = Content.Load<Texture2D>("MenuContent/xnuke.jpg.pagespeed.ic.XD9-0bi6PQ");
            controls = Content.Load<Texture2D>("MenuContent/controls");
        }

        /// <summary>
        /// Method used to draw help page.
        /// </summary>
        /// <param name="spritebatch">Spritebatch.</param>
        /// <param name="GraphicsDevice">Graphics device.</param>
        /// <param name="view">View matrix.</param>
        /// <param name="projection">Projection matrix.</param>
        public void showhelpPage(SpriteBatch spritebatch, GraphicsDevice GraphicsDevice, Matrix view, Matrix projection)
        {
            spritebatch.Begin();
            spritebatch.Draw(backGround, new Rectangle(0, 0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height), Color.White);
            spritebatch.Draw(controls, new Rectangle(0, GraphicsDevice.Viewport.Height / 4, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height - GraphicsDevice.Viewport.Height / 3), Color.White);
            spritebatch.End();

            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            characters.Draw("CONTROLS", 1.0f, new Vector3(-550, 350, 0), view, projection);
            characters.Draw("PRESS ANY KEY TO CONTINUE...", 0.4f, new Vector3(-850, -600, 0), view, projection);

        }

        /// <summary>
        /// Method used to update help page.
        /// </summary>
        /// <returns></returns>
        public bool updatehelpPage()
        {
            bool result = false;
            if (Keyboard.GetState().GetPressedKeys().Length == 0)
            {
                waitOn = true;
            }
            if (waitOn && Keyboard.GetState().GetPressedKeys().Length > 0)
            {
                waitOn = false;
                result = true;
            }
            return result;
        }
    }
}
