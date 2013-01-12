using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Tanks3DFPP.Menu
{
    internal class MenuPage
    {
        private Texture2D backGround;
        private SpriteBatch spriteBatch;
        private SoundEffect select;
        private static Characters characters;
        private int currentOptionIndex;
        private List<MenuOption> options;
        private GraphicsDevice graphicsDevice;

        public MenuPage(ContentManager content, GraphicsDevice graphicsDevice, string backgroundResourcePath, string[] options)
        {
            characters = new Characters(content);
            backGround = content.Load<Texture2D>(backgroundResourcePath);
            select = content.Load<SoundEffect>("MenuContent/menu_select");
            this.graphicsDevice = graphicsDevice;
            spriteBatch = new SpriteBatch(this.graphicsDevice);
            this.CreateOptions(options);
            this.OptionChanged += this.PlaySelectSound;
        }

        protected MenuOption CurrentOption
        {
            get
            {
                return this.options[currentOptionIndex];
            }
        }

        protected void SelectNextOption()
        {
            int previousOptionIndex = this.currentOptionIndex;
            this.CurrentOption.Deselect();
            this.currentOptionIndex = (int)MathHelper.Clamp(this.currentOptionIndex + 1, 0, this.options.Count - 1);
            this.CurrentOption.Select();
            if (currentOptionIndex != previousOptionIndex)
            {
                this.OptionChanged.Invoke(this, null);
            }
        }

        protected void SelectPreviousOption()
        {
            int previousOptionIndex = this.currentOptionIndex;
            this.CurrentOption.Deselect();
            this.currentOptionIndex = (int)MathHelper.Clamp(this.currentOptionIndex - 1, 0, this.options.Count - 1);
            this.CurrentOption.Select();
            if (currentOptionIndex != previousOptionIndex)
            {
                this.OptionChanged.Invoke(this, null);
            }
        }

        private void PlaySelectSound(object sender, EventArgs e)
        {
            this.select.Play(.3f, 0, 0);
        }

        public event EventHandler OptionChanged;

        public event EventHandler OptionChosen;

        protected void FireOptionChosen(MenuPage sender)
        {
            this.OptionChosen.Invoke(sender, null);
        }

        protected void Draw(Matrix view, Matrix projection)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(backGround, new Rectangle(0, 0, this.graphicsDevice.Viewport.Width, this.graphicsDevice.Viewport.Height), Color.White);
            spriteBatch.End();

            this.graphicsDevice.BlendState = BlendState.Opaque;
            this.graphicsDevice.DepthStencilState = DepthStencilState.Default;
            this.graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            characters.Draw("TANKS 3D FPP", 1f, new Vector3(-900, 350, 0), view, projection);
            foreach (MenuOption option in this.options)
            {
                option.Draw(characters, view, projection);
            }
        }

        private void CreateOptions(string[] names)
        {
            this.options = new List<MenuOption>();
            for (int i = 0; i < names.Length; ++i)
            {
                this.options.Add(new MenuOption(names[i],i));
            }
        }
    }
}
