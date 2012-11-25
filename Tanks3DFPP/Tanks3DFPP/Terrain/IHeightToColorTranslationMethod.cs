using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Tanks3DFPP.Terrain
{
    public interface IHeightToColorTranslationMethod
    {
        Color Calculate(int height);
    }
}
