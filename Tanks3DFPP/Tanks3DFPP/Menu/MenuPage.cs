using System;
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
    internal class MenuPage
    {
        private Texture2D backGround;
        private static SpriteBatch spriteBatch;

        internal static SpriteBatch SpriteBatch
        {
            get { return spriteBatch; }
        }

        private SoundEffect select;
        private static Characters characters;
        private int currentOptionIndex;
        private List<MenuOption> options;
        private GraphicsDevice graphicsDevice;

        protected IDictionary<string, string> CurrentOptionsDataState
        {
            get
            {
                Dictionary<string, string> value = new Dictionary<string, string>();
                foreach (TextBox tb in this.options.Where(x => x is TextBox))
                {
                    string data = tb.GetValue();
                    if (!string.IsNullOrEmpty(data))
                    {
                        value[tb.Text] = data;
                    }
                }

                return value;
            }
        }

        public MenuPage(ContentManager content, GraphicsDevice graphicsDevice, string backgroundResourcePath, IEnumerable<MenuOption> options)
        {
            characters = new Characters(content);
            backGround = content.Load<Texture2D>(backgroundResourcePath);
            select = content.Load<SoundEffect>("MenuContent/150220__killkhan__reload-1");
            if (spriteBatch == null)
            {
                spriteBatch = new SpriteBatch(graphicsDevice);
            }

            this.graphicsDevice = graphicsDevice;
            if (options.Count() > 0)
            {
                this.CreateOptions(options);
                this.SelectPreviousOption();
            }
            else
            {
                this.options = new List<MenuOption>();
            }

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
            this.SelectOption(this.currentOptionIndex + 1);
        }

        protected void SelectPreviousOption()
        {
            this.SelectOption(this.currentOptionIndex - 1);
        }

        protected void SwitchColumnRight()
        {
            int columnSize = this.options.Count / 2;
            if (this.currentOptionIndex < columnSize)
            {
                this.SelectOption(this.currentOptionIndex + columnSize);
            }
            else
            {
                this.SwitchColumnLeft();
            }

        }

        #region The proper way of doing this. (Buggy due to KeyboardHandler botch - Issue #13)
        //protected void SwitchColumn()
        //{
        //    int columnSize = this.options.Count / 2;
        //    this.SelectOption(this.currentOptionIndex > columnSize - 1
        //    ? this.currentOptionIndex - columnSize
        //    : this.currentOptionIndex + columnSize);
        //}
        #endregion

        protected void SwitchColumnLeft()
        {
            int columnSize = this.options.Count / 2;
            if (this.currentOptionIndex >= columnSize)
            {
                this.SelectOption(this.currentOptionIndex - columnSize);
            }
            else
            {
                this.SwitchColumnRight();
            }
        }

        private void SelectOption(int index)
        {
            int previousOptionIndex = this.currentOptionIndex;
            this.CurrentOption.Deselect();
            this.currentOptionIndex = (int)MathHelper.Clamp(index, 0, this.options.Count - 1);
            this.CurrentOption.Select();
            if (currentOptionIndex != previousOptionIndex)
            {
                this.OptionChanged.Invoke(this, null);
            }
        }

        private void PlaySelectSound(object sender, EventArgs e)
        {
            this.select.Play();
        }

        public event EventHandler OptionChanged;

        public event EventHandler<OptionChosenEventArgs> OptionChosen;

        public event EventHandler Cancelled;

        protected void FireOptionChosen(MenuPage sender, int? optionIndexOverride = null)
        {
            this.OptionChosen.Invoke(sender, new OptionChosenEventArgs(optionIndexOverride ?? this.currentOptionIndex));
        }

        public virtual void Draw(Matrix view, Matrix projection)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(backGround, new Rectangle(0, 0, this.graphicsDevice.Viewport.Width, this.graphicsDevice.Viewport.Height), Color.White);
            spriteBatch.End();

            this.graphicsDevice.BlendState = BlendState.Opaque;
            this.graphicsDevice.DepthStencilState = DepthStencilState.Default;
            this.graphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

            foreach (MenuOption option in this.options)
            {
                option.Draw(characters, view, projection);
            }
        }

        public virtual void Update()
        {
            if (this.Cancelled != null)
            {
                KeyboardHandler.KeyAction(Keys.Escape, () =>
                    {
                        this.Cancelled.Invoke(this, null);
                    });
            }
        }

        protected void DrawString(string text, float scale, Vector2 position, Matrix view, Matrix projection)
        {
            characters.Draw(text, scale, new Vector3(position, 0), view, projection);
        }

        protected void DrawTexture(Texture2D texture, Rectangle destination)
        {
            spriteBatch.Begin();
            spriteBatch.Draw(texture, destination, Color.White);
            spriteBatch.End();
        }

        protected void CreateOptions(IEnumerable<MenuOption> options)
        {
            this.options = new List<MenuOption>();
            foreach (MenuOption option in options)
            {
                this.options.Add(option);
            }
        }
    }
}
