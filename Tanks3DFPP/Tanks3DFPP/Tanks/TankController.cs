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


namespace Tanks3DFPP.Tanks
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class TankController : Microsoft.Xna.Framework.GameComponent
    {

        public List<Tank> TanksInGame
        {
            get { return tanksInGame; }
        }
        private List<Tank> tanksInGame;
        public TankMissle MissleInGame
        {
            get { return missleInGame; }
        }
        private TankMissle missleInGame;
        public List<String> PlayersOrderedByScore
        {
            get { return playersOrderedByScore; }
        }
        public List<String> playersOrderedByScore;


        BoundingSphere SphereHit;
        int HitIndex;

        public bool bShotFired, bDisplayScores;
        private bool bKeyPressed;

        public int TurnToken;

        GraphicsDevice GD;
        SpriteBatch spriteBatch;
        SpriteFont InfoFont;
        String PlayerInfoString;
        String ShotInfoString;
        Random rand;


        SoundEffect explosionSound;
        SoundEffect shotSound;
        SoundEffectInstance turretMoveSound;
        SoundEffectInstance cannonMoveSound;
        SoundEffectInstance morePowerSound;
        SoundEffectInstance lessPowerSound;
        SoundEffectInstance danseMacabre;
        SoundEffectInstance ambience;

        //ExplosionSystem Explosion;

        public TankController(Game game, int numOfPlayers)
            : base(game)
        {
            GD = game.GraphicsDevice;
            spriteBatch = new SpriteBatch(GD);
            rand = new Random();
            bool bNotSpawnedCorrectly;

            //Create instances and load their content
            tanksInGame = new List<Tank>();
            for (int i = 0; i < numOfPlayers; ++i)
            {
                TanksInGame.Add(new Tank());
                TanksInGame[i].LoadContent(game.Content);
                tanksInGame[i].PlayerName = "Player " + i.ToString();
                do
                {
                    bNotSpawnedCorrectly = false;
                    do
                    {
                        TanksInGame[i].SpawnAt(new Vector3(rand.Next(0, (Game1.heightMap.Width - 1) * Game1.Scale), 0, rand.Next((-Game1.heightMap.Height + 1) * Game1.Scale, 0))); // Y should be calculated
                    } while (!TanksInGame[i].IsOnMap);
                    for (int j = 0; j < TanksInGame.Count; ++j)
                    {
                        // make sure the tanks don't intersect with themselves at the beginning
                        if (i != j && TanksInGame[i].CollisionCheckWith(TanksInGame[j]))
                        {
                            bNotSpawnedCorrectly = true;
                        }
                    }
                } while (bNotSpawnedCorrectly);
            }
            missleInGame = new TankMissle();
            MissleInGame.LoadContent(game.Content);

            InfoFont = game.Content.Load<SpriteFont>("SpriteFont1");
            explosionSound = game.Content.Load<SoundEffect>("cinema boom impact");
            shotSound = game.Content.Load<SoundEffect>("shoot");
            turretMoveSound = game.Content.Load<SoundEffect>("turret").CreateInstance();
            cannonMoveSound = game.Content.Load<SoundEffect>("turret").CreateInstance();
            morePowerSound = game.Content.Load<SoundEffect>("morepower").CreateInstance();
            lessPowerSound = game.Content.Load<SoundEffect>("lesspower").CreateInstance();
            danseMacabre = game.Content.Load<SoundEffect>("Danse Macabre - Big Hit 2").CreateInstance();
            //ambience = game.Content.Load<SoundEffect>("nosferatu ambience").CreateInstance();

            //Explosion = new ExplosionSystem();
            //Explosion.LoadContent(game.Content, GD);

            this.Initialize();
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            TurnToken = 0;
            PlayerInfoString = "Player: [{0}], Health {1}%";
            ShotInfoString = "Strength: {0} \nTurret angle: {1} \nCannon angle: {2} ";
            SphereHit = new BoundingSphere(Vector3.Zero, 0);
            playersOrderedByScore = new List<string>();
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            KeyboardState KS = Keyboard.GetState();

            PlayerInfoString = String.Format("{0}, Health {1}%",
                Game1.GameParameters.,
                TanksInGame[TurnToken].Health);
            ShotInfoString = String.Format("Strength: {0} \nTurret angle: {1} \nCannon angle: {2} ",
                TanksInGame[TurnToken].InitialVelocityPower,
                MathHelper.ToDegrees(TanksInGame[TurnToken].TurretDirectionAngle),
                MathHelper.ToDegrees(TanksInGame[TurnToken].CannonDirectionAngle));

            // freeze handle input while missle in air
            // update missle position after bshotfired (make is false when it hit sth)

            if (!bShotFired)
            {
                TanksInGame[TurnToken].HandleInput(KS);
            }

            if (KeyPressed(Keys.Space) && !bShotFired) //(KS.IsKeyDown(Keys.Space))
            {
                MissleInGame.SetPreShotValues(TanksInGame[TurnToken].TurretDirectionAngle,
                    TanksInGame[TurnToken].CannonDirectionAngle,
                    TanksInGame[TurnToken].CannonPosition, //new Vector3(TanksInGame[TurnToken].CannonPosition.X, TanksInGame[TurnToken].CannonPosition.Y, TanksInGame[TurnToken].CannonPosition.Z), 
                    TanksInGame[TurnToken].InitialVelocityPower);
                bShotFired = true;
                shotSound.Play();
            }

            HandleMovementSound();

            if (bShotFired)
            {
                if (MissleInGame.UpdatePositionAfterShot(TanksInGame, TurnToken, out SphereHit, out HitIndex)) //0, 
                {
                    //Explosion.AddExplosion(new Vector2(GD.Viewport.X / 2, GD.Viewport.Y / 2), 4, 30.0f, 1000.0f, gameTime);
                    if (HitIndex != -1)
                    {
                        // fire the particle system explosion anim at missle pos and hide the missle
                        // reduce health of TanksInGame[HitIndex];
                        TanksInGame[HitIndex].Health -= rand.Next(33, 51);
                        if (TanksInGame[HitIndex].Health < 0)
                        {
                            // animation of tank shattering in loop and camera zoomout to see the animation
                            // fire the particle system huge explosion anim
                            //tanksInGame[HitIndex].BrakeDown();
                            playersOrderedByScore.Add(TanksInGame[HitIndex].PlayerName);
                            tanksInGame.RemoveAt(HitIndex);

                            // display scores
                            if (TanksInGame.Count == 1)
                            {
                                playersOrderedByScore.Add(TanksInGame[0].PlayerName);
                                bDisplayScores = true;
                                if (ambience != null)
                                    ambience.Stop();
                                danseMacabre.Play();
                            }
                        }
                    }
                    if (explosionSound != null)
                        explosionSound.Play();
                    // next turn
                    ++TurnToken;
                    TurnToken %= TanksInGame.Count;
                    bShotFired = false;
                    MissleInGame.SetPreShotValues(TanksInGame[TurnToken].TurretDirectionAngle,
                    TanksInGame[TurnToken].CannonDirectionAngle,
                    TanksInGame[TurnToken].Position,
                    TanksInGame[TurnToken].InitialVelocityPower);
                }
                //allow free camera to look around
            }

            if (TanksInGame.Count == 1)
            {
                // go back to menu?

                if (KS.IsKeyDown(Keys.Enter) && danseMacabre.State != SoundState.Playing)
                {
                    bDisplayScores = false;
                    this.Game.Exit();
                }
            }
            else
            {
                if (ambience != null && ambience.State != SoundState.Playing)
                {
                    ambience.Play();
                }
            }

            base.Update(gameTime);
        }

        private void HandleMovementSound()
        {
            if (tanksInGame[TurnToken].bCannonMoves)
                cannonMoveSound.Play();
            else
                cannonMoveSound.Stop();

            if (tanksInGame[TurnToken].bTurretMoves)
                turretMoveSound.Play();
            else
                turretMoveSound.Stop();

            if (tanksInGame[TurnToken].bPowerIncreases)
                morePowerSound.Play();
            else
                morePowerSound.Stop();

            if (tanksInGame[TurnToken].bPowerDecreases)
                lessPowerSound.Play();
            else
                lessPowerSound.Stop();

        }

        private bool KeyPressed(Keys key)
        {
            KeyboardState KS = Keyboard.GetState();
            if (KS.IsKeyUp(key))
            {
                bKeyPressed = false;
            }
            if (KS.IsKeyDown(key) && !bKeyPressed)
            {
                bKeyPressed = true;
                return true;
            }
            else
            {
                return false;
            }
        }

        public void Draw(Matrix viewMatrix, Matrix projectionMatrix)
        {
            foreach (Tank tank in TanksInGame)
            {
                tank.Draw(viewMatrix, projectionMatrix);
            }
            if (!bShotFired)
            {
                MissleInGame.Draw(viewMatrix, projectionMatrix);
            }

            spriteBatch.Begin();
            if (bDisplayScores)
            {
                for (int i = playersOrderedByScore.Count - 1; i >= 0; --i)
                {
                    spriteBatch.DrawString(InfoFont, String.Format("{0}. {1}", playersOrderedByScore.Count - i, playersOrderedByScore[i]), (Vector2.UnitX * GD.Viewport.Width * 0.5f) + (Vector2.UnitY * ((playersOrderedByScore.Count - i) + 1) * 25), Color.Orange);
                }
                spriteBatch.DrawString(InfoFont, String.Format("Press G to start a new battle!\nPress Enter to chicken out."), (Vector2.UnitX * GD.Viewport.Width * 0.3f) + (Vector2.UnitY * GD.Viewport.Height * 0.7f), Color.Orange);
            }
            else
            {
                spriteBatch.DrawString(InfoFont, PlayerInfoString, Vector2.UnitX * GD.Viewport.Width * 0.3f, Color.DarkKhaki);
                spriteBatch.DrawString(InfoFont, ShotInfoString, Vector2.Zero, Color.DarkGreen);
                for (int i = 0; i < tanksInGame.Count; ++i)
                {
                    spriteBatch.DrawString(InfoFont, String.Format("{0} Health: {1}%", tanksInGame[i].PlayerName, tanksInGame[i].Health), (Vector2.UnitX * GD.Viewport.Width * 0.7f) + (Vector2.UnitY * (i + 1) * 25), Color.DarkMagenta);
                    //    spriteBatch.DrawString(InfoFont, string.Format("Tank {0} bs center: {1}", i, tanksInGame[i].BoundingSpheres[5].Center), (Vector2.UnitX * GD.Viewport.Width * 0.35f) + (Vector2.UnitY * (i+1) * 25), Color.White);
                    //    spriteBatch.DrawString(InfoFont, string.Format("Tank {0} position: {1}", i, tanksInGame[i].Position), (Vector2.UnitX * GD.Viewport.Width * 0.35f) + (Vector2.UnitY * (i+2)*2 * 25), Color.White);
                }
                //spriteBatch.DrawString(InfoFont, string.Format("Missle position: {0}", missleInGame.boundingSphere.Center), (Vector2.UnitX * GD.Viewport.Width * 0.35f) + (Vector2.UnitY * 5 * 25), Color.White);
            }
            //Explosion.DrawExplosion();
            spriteBatch.End();

            GD.BlendState = BlendState.Opaque;
            GD.DepthStencilState = DepthStencilState.Default;
            GD.SamplerStates[0] = SamplerState.LinearWrap;
        }
    }
}
