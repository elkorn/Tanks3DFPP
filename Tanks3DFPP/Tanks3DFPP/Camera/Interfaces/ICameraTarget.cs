using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Tanks3DFPP.Camera.Interfaces
{
    /// <summary>
    ///     This may disappear in some time. :P
    /// </summary>
    public interface ICameraTarget
    {
        float FacingDirection { get; }

        Vector3 Position { get; }
    }
}
