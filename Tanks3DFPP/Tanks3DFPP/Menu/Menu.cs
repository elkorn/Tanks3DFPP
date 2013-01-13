using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Tanks3DFPP.Utilities;

namespace Tanks3DFPP.Menu
{
    /// <summary>
    ///Class used to handle menu.
    /// </summary>
    public class Menu
    {
        public const string DefaultBackgroundResourceName = "MenuContent/xnuke.jpg.pagespeed.ic.XD9-0bi6PQ",
            AltBackgroundResourceName = "MenuContent/18627-33959";
        private bool enabled = true;

        private Song music;

        /// <summary>
        /// method to get enabled.
        /// </summary>
        /// <returns>if true then enabled is on.</returns>      
        public bool Enabled
        {
            get { return enabled; }
        }

        private MainMenu mainMenu;
        private HelpPage help;
        private PlayPage play;
        private LoadingPage loading;

        private MenuPage currentPage;

        private GraphicsDevice graphicsDevice;
        private Matrix view;
        private Matrix projection;

        private SoundEffect menuChange;

        public event EventHandler<GameParametersReadyEventArgs> GameParametersReady;
        public event EventHandler GameComponentsReady;

        private ContentManager content;

        /// <summary>
        /// Initializes a new instance of the <see cref="Menu" /> class.
        /// </summary>
        /// <param name="content">content manager reference</param>
        /// <param name="graph">graphiscdevice reference</param>
        public Menu(Game game)
        {
            this.content = new ContentManager(game.Content.ServiceProvider);
            content.RootDirectory = "Content";
            graphicsDevice = game.GraphicsDevice;
            view = Matrix.CreateLookAt(Vector3.UnitZ * 1500, Vector3.Zero, Vector3.Up);
            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45),
                graphicsDevice.Viewport.AspectRatio, 0.1f, 1000000.0f);
        }

        private void SwitchPageTo(MenuPage page)
        {
            currentPage = page;
            menuChange.Play();
        }

        /// <summary>
        /// Method used to load models and create necessary objects.
        /// </summary>
        /// <param name="content"></param>
        public void LoadContent()
        {
            menuChange = content.Load<SoundEffect>("MenuContent/150219__killkhan__reload-4");
            mainMenu = new MainMenu(content, graphicsDevice);
            help = new HelpPage(content, this.graphicsDevice);
            play = new PlayPage(content, this.graphicsDevice, Game1.GameParameters);
            loading = new LoadingPage(content,this.graphicsDevice);

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

            this.play.OptionChosen += (sender, e) =>
                {
                    MenuNavigationOption option = (MenuNavigationOption) e.Code;
                    switch (option)
                    {
                        case MenuNavigationOption.Back:
                            this.SwitchPageTo(mainMenu);
                            break;
                        case MenuNavigationOption.Next:
                            this.GameParametersReady.Invoke(this, new GameParametersReadyEventArgs(play.GameParametersModel));
                            this.SwitchPageTo(loading);
                            break;
                    }
                };

            this.mainMenu.Cancelled += (sender, e) =>
                {
                    Game1.Quit();
                };
            this.play.Cancelled += (sender, e) =>
                {
                    this.SwitchPageTo(mainMenu);
                };

            this.loading.Ready += LoadingOnReady;

            this.currentPage = mainMenu;
            music = content.Load<Song>("MenuContent/Summon the Rawk");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Volume = .6f;
            MediaPlayer.Play(music);
        }

        private void LoadingOnReady(object sender, EventArgs eventArgs)
        {
            MediaPlayer.Stop();
            this.content.Unload();    // may wanna go back to the menu
            this.GameComponentsReady.Invoke(this, null);
            this.enabled = false;
        }

        public void AddProgress(int howMuch)
        {
            if (this.currentPage == this.loading)
            {
                this.loading.AddProgress(howMuch);
            }
            else
            {
                throw new InvalidOperationException("Cannot add progress in this state.");
            }
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
        public void Update()
        {
            currentPage.Update();
        }

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="listWithResults"></param>
        //public void setVariables(List<object> listWithResults)
        //{
        //    if ((int)listWithResults[1] == 1)
        //    {
        //        //next button in play page pressed
        //        //set variables
        //        Game1.GameParameters.MapScale = int.Parse((string)listWithResults[2]);
        //        Game1.MaxHeight = int.Parse((string)listWithResults[3]);
        //        Game1.Roughness = int.Parse((string)listWithResults[4]);

        //        playerNames = new List<string>();
        //        for (int i = 0; i < listWithResults.Count - 5; ++i)
        //        {
        //            playerNames.Add((string)listWithResults[i + 5]);
        //        }
        //    }
        //}
    }
}

