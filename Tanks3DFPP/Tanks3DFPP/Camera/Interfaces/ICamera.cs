using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Tanks3DFPP.Camera.Interfaces
{
    public interface ICamera
    {
        Vector3 Position { get; set; }
        Vector3 LookAt { get; set; }
        Matrix View { get; }
        BoundingFrustum Frustum { get; }
        void Update(GameTime gameTime);
    }
}
