﻿using System;
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

        public MenuPage(ContentManager content, GraphicsDevice graphicsDevice, string backgroundResourcePath, IEnumerable<MenuOption> options)
        {
            characters = new Characters(content);
            backGround = content.Load<Texture2D>(backgroundResourcePath);
            select = content.Load<SoundEffect>("MenuContent/menu_select");
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

        public event EventHandler<OptionChosenEventArgs> OptionChosen;

        public event EventHandler Cancelled;

        protected void FireOptionChosen(MenuPage sender)
        {
            this.OptionChosen.Invoke(sender, new OptionChosenEventArgs(this.currentOptionIndex));
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
            KeyboardHandler.KeyAction(Keys.Escape, () =>
                {
                    this.Cancelled.Invoke(this,null);
                });
        }

        protected void DrawString(string text, float scale, Vector2 position, Matrix view, Matrix projection)
        {
            characters.Draw(text, scale, new Vector3(position, 0), view, projection);
        }

        //protected void CreateOptions(string[] names)
        //{
        //    this.options = new List<MenuOption>();
        //    for (int i = 0; i < names.Length; ++i)
        //    {
        //        this.options.Add(new MenuOption(names[i], i, new Vector2(200, 100 - 330 * i)));
        //    }
        //}

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