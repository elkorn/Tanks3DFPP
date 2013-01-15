using System;

namespace Tanks3DFPP.Utilities
{
    public class GameParametersReadyEventArgs: EventArgs
    {
        public GameParametersReadyEventArgs(GameParameters gameState)
        {
            if (gameState == null)
            {
                throw new ArgumentNullException("gameState");
            }

            this.Parameters = gameState;
        }

        public GameParameters Parameters { get; private set; }
    }
}
