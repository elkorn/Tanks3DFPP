using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;

namespace Tanks3DFPP.Menu
{
    /// <summary>
    ///Class used to handle menu.
    /// </summary>
    public class Menu
    {
        public const string DefaultBackgroundResourceName = "MenuContent/xnuke.jpg.pagespeed.ic.XD9-0bi6PQ",
            AltBackgroundResourceName = "MenuContent/nuke_symbol_Wallpaper_lnsps";
        private SpriteBatch spritebatch;
        private bool menuON = true;

        private Song music;

        /// <summary>
        /// list of player names , size of it is player count(menu adjusted from 2 to 4 players)
        /// </summary>
        List<string> playerNames;

        /// <summary>
        /// method to get menuON.
        /// </summary>
        /// <returns>if true then menuON is on.</returns>      
        public bool Enabled
        {
            get { return menuON; }
        }

        private bool mainPageON = true;
        private bool playPageON = false;
        private bool loadingPageON = false;
        private bool helpPageON = false;

        private MainMenu mainMenu;
        private HelpPage help;
        private PlayPage play;
        private LoadingPage loading;

        private MenuPage currentPage;

        private GraphicsDeviceManager graphics;
        private Matrix view;
        private Matrix projection;

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
            view = Matrix.CreateLookAt(Vector3.UnitZ * 1500, Vector3.Zero, Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45),
                graphics.GraphicsDevice.Viewport.AspectRatio, 0.1f, 1000000.0f);
        }

        private void SwitchPageTo(MenuPage page)
        {
            currentPage = page;
            menuChange.Play();
        }

        /// <summary>
        /// Method used to load models and create necessary objects.
        /// </summary>
        /// <param name="Content"></param>
        public void LoadMenu(ContentManager Content)
        {
            menuChange = Content.Load<SoundEffect>("MenuContent/menu_change");
            mainMenu = new MainMenu(Content, graphics.GraphicsDevice);
            help = new HelpPage(Content, this.graphics.GraphicsDevice);
            play = new PlayPage(Content, this.graphics.GraphicsDevice, playerNames);
            loading = new LoadingPage(Content);

            this.mainMenu.OptionChosen += (sender, e) =>
                {
                    MainMenuPage targetPage = (MainMenuPage) e.Code;
                    switch (targetPage)
                {
                    case MainMenuPage.Play:
                        this.SwitchPageTo(play);
                        break;
                    case MainMenuPage.Help:
                        this.SwitchPageTo(help);
                        break;
                    case MainMenuPage.Quit:
                        Game1.Quit();
                        break;
                }
            };

            this.help.OptionChosen += (sender, e) =>
            {
                this.SwitchPageTo(mainMenu);
            };

            this.mainMenu.Cancelled += (sender, e) =>
                {
                    Game1.Quit();
                };
            this.play.Cancelled += (sender, e) =>
                {
                    this.SwitchPageTo(mainMenu);
                };

            this.SwitchPageTo(mainMenu);
            music = Content.Load<Song>("Summon the Rawk");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(music);
        }

        /// <summary>
        /// Method used to draw appropriate page of the menu.
        /// </summary>
        public void Draw()
        {
            currentPage.Draw(view, projection);
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
        public List<object> Update(GameTime gameTime, int percent)
        {
            List<object> result = new List<object>();
            result.Add(0);
            currentPage.Update();
            //if (mainPageON)
            //{
            //    mainMenu.Update();
            //    switch (mainMenuResult)
            //    {
            //        case 0:
            //            playPageON = true;
            //            mainPageON = false;
            //            menuChange.Play();
            //            break;
            //        case 1:
            //            helpPageON = true;
            //            mainPageON = false;
            //            menuChange.Play();
            //            break;
            //        case 2:
            //            menuChange.Play();
            //            Game1.Quit();
            //            break;
            //    }
            //}

            //if (helpPageON)
            //    {
            //        result[0] = 0;
            //        help.Update();
            //    }

            //    if (loadingPageON)
            //    {
            //        result[0] = 2;
            //        if (loading.updateLoadingPage(graphics.GraphicsDevice, percent))
            //        {
            //            result.Add(loading.whichSideOfTheCube());
            //            MediaPlayer.Stop();
            //            menuON = false;
            //        }
            //    }

            //    if (playPageON)
            //    {
            //        result[0] = 1;
            //        result.AddRange(play.updateplayPage());
            //        if ((int)result[1] == 1)
            //        {
            //            //next button pressed
            //            playPageON = false;
            //            loadingPageON = true;
            //            loading.playernumber = result.Count - 5;
            //            menuChange.Play();
            //            setVariables(result);
            //        }
            //        if ((int)result[1] == -1)
            //        {
            //            //back button pressed
            //            playPageON = false;
            //            mainPageON = true;
            //            menuChange.Play();
            //        }
            //    }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listWithResults"></param>
        public void setVariables(List<object> listWithResults)
        {
            if ((int)listWithResults[1] == 1)
            {
                //next button in play page pressed
                //set variables
                Game1.MapScale = int.Parse((string)listWithResults[2]);
                Game1.MaxHeight = int.Parse((string)listWithResults[3]);
                Game1.Roughness = int.Parse((string)listWithResults[4]);

                playerNames = new List<string>();
                for (int i = 0; i < listWithResults.Count - 5; ++i)
                {
                    playerNames.Add((string)listWithResults[i + 5]);
                }
            }
        }
    }
}
