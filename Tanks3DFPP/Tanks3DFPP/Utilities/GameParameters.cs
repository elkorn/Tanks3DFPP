namespace Tanks3DFPP.Utilities
{
    public class GameParameters
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="GameParameters" /> class.
        /// </summary>
        /// <param name="mapScale">The map scale.</param>
        /// <param name="roughness">The roughness.</param>
        /// <param name="maxMapHeight">Height of the max map.</param>
        /// <param name="playersNames">The players names.</param>
        public GameParameters(int mapScale, int roughness, int maxMapHeight, string[] playersNames)
        {
            this.MapScale = mapScale;
            this.Roughness = roughness;
            this.MaxMapHeight = maxMapHeight;
            this.PlayerNames = playersNames;
        }

        public int MapScale { get; private set; }
        public int Roughness { get; private set; }
        public int MaxMapHeight { get; private set; }

        public int NumberOfPlayers
        {
            get
            {
                return PlayerNames.Length;
            }
        }

        public string[] PlayerNames { get; private set; }
    }
}
