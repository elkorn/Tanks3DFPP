using System;

namespace Tanks3DFPP.Utilities
{
    public class GameStateReadyEventArgs: EventArgs
    {
        public GameStateReadyEventArgs(GameState gameState)
        {
            if (gameState == null)
            {
                throw new ArgumentNullException("gameState");
            }

            this.GameState = gameState;
        }

        public GameState GameState { get; private set; }
    }
}
