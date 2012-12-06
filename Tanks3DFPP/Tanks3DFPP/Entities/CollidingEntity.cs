using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Tanks3DFPP.Terrain;

namespace Tanks3DFPP.Entities
{
    public abstract class CollidingEntity : Microsoft.Xna.Framework.GameComponent
    {
        public Vector3 Position { get; protected set; }
        protected readonly IHeightMap floor;

        protected CollidingEntity(Game game, IHeightMap floor)
            : base(game)
        {
            this.floor = floor;
        }

        protected abstract bool IsInFloorBounds(Vector3 position);

        protected abstract Vector3 OffsetToFloorHeight(Vector3 position);
    }
}
