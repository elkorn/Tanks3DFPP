using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Tanks3DFPP.Terrain
{
    class GreenYellowRed: IHeightToColorTranslationMethod
    {
        /// <summary>
        /// The max height
        /// </summary>
        private readonly int maxHeight;

        /// <summary>
        /// Initializes a new instance of the <see cref="GreenYellowRed" /> class.
        /// </summary>
        /// <param name="maxHeight">Height of the max.</param>
        public GreenYellowRed(int maxHeight)
        {
            this.maxHeight = maxHeight;
        }

        /// <summary>
        /// Calculates the color representing specified height.
        /// </summary>
        /// <param name="height">The height.</param>
        /// <returns>A color from the range of 0f0 to f00.</returns>
        public Color Calculate(int height)
        {
            int red = (int)((height / (float)this.maxHeight) * 255);
            return new Color(red, 255 - red, 0);
        }
    }
}
