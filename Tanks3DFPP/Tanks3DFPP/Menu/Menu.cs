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
    ///Class used to handle menu.
    /// </summary>
    public class Menu
    {

        private int timesince = 0;
        private int timeper = 200;
        private SpriteBatch spritebatch;
        private bool menuON = true;

        /// <summary>
        /// method to get menuON.
        /// </summary>
        /// <returns>if true then menuON is on.</returns>      
        public bool GetmenuON() { return menuON; }

        public bool quit = false;

        /// <summary>
        /// Method to get quit.
        /// </summary>
        /// <returns>If true then quit is on.</returns>      
        public bool Getquit() { return quit; }

        private bool mainPageON = true;
        private bool playPageON = false;
        private bool loadingPageON = false;
        private bool helpPageON = false;

        private MainMenu mainMenuObj;
        private HelpPage helpPageObj;
        private PlayPage playPageObj;
        private LoadingPage loadingPageObj;

        private GraphicsDeviceManager graphics;
        private Matrix world;
        private Matrix view;
        private Matrix projection;

        private SoundEffect menuCancel;
        private SoundEffect menuChange;

        /// <summary>
        /// Initializes a new instance of the <see cref="Menu" /> class.
        /// </summary>
        /// <param name="Content">content manager reference</param>
        /// <param name="graph">graphiscdevice reference</param>
        public Menu(GraphicsDeviceManager graph)
        {
            spritebatch = new SpriteBatch(graph.GraphicsDevice);
            graphics = graph;
            world = Matrix.Identity;
            view = Matrix.CreateLookAt(new Vector3(0, 0, 1500), new Vector3(0, 0, 0), Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45),
                graphics.GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000000.0f);
        }

        /// <summary>
        /// Method used to load models and create necessary objects.
        /// </summary>
        /// <param name="Content"></param>
        public void LoadMenu(ContentManager Content)
        {
            menuCancel = Content.Load<SoundEffect>("MenuContent/menu_cancel");
            menuChange = Content.Load<SoundEffect>("MenuContent/menu_change");
            mainMenuObj = new MainMenu(Content, graphics.GraphicsDevice);
            helpPageObj = new HelpPage(Content);
            playPageObj = new PlayPage(Content);
            loadingPageObj = new LoadingPage(Content);
        }

        /// <summary>
        /// Method used to draw appropriate page of the menu.
        /// </summary>
        public void showMenu()
        {
            if (timesince > timeper)
            {
                if (mainPageON)
                {
                    mainMenuObj.showMainMenu(view, projection, graphics.GraphicsDevice);
                }
                if (helpPageON)
                {
                    helpPageObj.showhelpPage(spritebatch, graphics.GraphicsDevice, view, projection);
                }
                if (playPageON)
                {
                    playPageObj.showPlayPage(spritebatch, graphics.GraphicsDevice, view, projection);
                }
                if (loadingPageON)
                {
                    loadingPageObj.showLoadingPage(spritebatch, view, projection, graphics.GraphicsDevice);
                }
            }
        }

        /// <summary>
        /// Method used to update menu elements.
        /// </summary>
        /// <param name="gameTime">Gametime.</param>
        /// <param name="percent">loading terrain percent value</param>
        /// <returns>Method returns list including: 
        /// Menu state in int, 0-not done , 1 - play page is done , 2 - loading page is done,
        /// Depending on the state the rest is either from play page or loading page. 
        /// </returns>
        public List<object> updateMenu(GameTime gameTime, int percent)
        {
            List<object> result = new List<object>();
            result.Add(0);
            timesince += gameTime.ElapsedGameTime.Milliseconds;
            if (timesince > timeper)
            {
                if (mainPageON)
                {
                    switch (mainMenuObj.updateMainMenu())
                    {
                        case 0:
                            playPageON = true;
                            mainPageON = false;
                            menuChange.Play();
                            timesince = 0;
                            break;
                        case 1:
                            helpPageON = true;
                            mainPageON = false;
                            menuChange.Play();
                            timesince = 0;
                            break;
                        case 2:
                            quit = true;
                            menuChange.Play();
                            timesince = 0;
                            break;
                    }
                }

                if (helpPageON)
                {
                    result[0] = 0;
                    if (helpPageObj.updatehelpPage())
                    {
                        mainPageON = true;
                        helpPageON = false;
                        menuChange.Play();
                        timesince = 0;
                    }
                }

                if (loadingPageON)
                {
                    result[0] = 2;
                    if (loadingPageObj.updateLoadingPage(graphics.GraphicsDevice, percent))
                    {
                        result.Add(loadingPageObj.whichSideOfTheCube());
                        menuON = false;
                    }
                }

                if (playPageON)
                {
                    result[0] = 1;
                    result.AddRange(playPageObj.updateplayPage());
                    if ((int)result[1] == 1)
                    {
                        //next button pressed
                        playPageON = false;
                        loadingPageON = true;
                        menuChange.Play();
                        timesince = 0;
                    }
                    if ((int)result[1] == -1)
                    {
                        //back button pressed
                        playPageON = false;
                        mainPageON = true;
                        menuChange.Play();
                        timesince = 0;
                    }
                }

            }
            return result;
        }


    }
}
