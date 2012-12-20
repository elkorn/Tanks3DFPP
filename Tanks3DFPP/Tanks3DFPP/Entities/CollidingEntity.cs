using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Tanks3DFPP.Terrain;

namespace Tanks3DFPP.Entities
{
    public abstract class CollidingEntity
    {
        public Vector3 Position { get; protected set; }

        protected abstract bool IsInFloorBounds(IHeightMap floor);

        protected abstract bool IsInFloorBounds(IHeightMap floor, Vector3 position);

        protected abstract Vector3 OffsetToFloorHeight(IHeightMap floor);

        protected abstract Vector3 OffsetToFloorHeight(IHeightMap floor, Vector3 position);
    }
}
