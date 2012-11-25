using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Tanks3DFPP.Terrain
{
    internal class HeightToGrayscale : IHeightToColorTranslationMethod
    {
        int maxHeight;
        public HeightToGrayscale(int maxHeight)
        {
            this.maxHeight = maxHeight;
        }

        public Microsoft.Xna.Framework.Color Calculate(int height)
        {
            int color = (int)(height * 255 / maxHeight);
            return new Color(color, color, color);
        }
    }
}
