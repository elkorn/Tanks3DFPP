using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tanks3DFPP.Utilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Tanks3DFPP.Terrain.Optimization.QuadTree
{
    public class TreeVertexCollection : GameComponent
    {

        public VertexMultitextured[] Vertices;

        private Vector3 position;

        private Effect effect;
        private Texture sand, grass, rock, snow;
        private int topSize,
                    halfSize,
                    vertexCount,
                    scale;

        public VertexMultitextured this[int index]
        {
            get
            {
                return this.Vertices[index];
            }

            set
            {
                this.Vertices[index] = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeVertexCollection" /> class.
        /// </summary>
        /// <param name="position">The position of the top-left terrain corner in the 3D space.</param>
        /// <param name="heightMap">The height map.</param>
        /// <param name="scale">The scale.</param>
        public TreeVertexCollection(
            Game game,
            Vector3 position,
            IHeightMap heightMap,
            int scale)
            :base(game)
        {
            this.effect = game.Content.Load<Effect>("Multitexture");
            this.sand = game.Content.Load<Texture>("sand");
            this.grass = game.Content.Load<Texture>("grass");
            this.rock = game.Content.Load<Texture>("rock");
            this.snow = game.Content.Load<Texture>("snow");

            this.InitializeEffect();

            position = Vector3.Zero;
            this.scale = scale;
            this.topSize = heightMap.Width - 1;
            this.halfSize = this.topSize / 2;
            this.vertexCount = heightMap.Width * heightMap.Height;
            this.Vertices = new VertexMultitextured[this.vertexCount];
        }

        private void InitializeEffect()
        {
            Vector3 lightDir = new Vector3(1, -1, -1);
            lightDir.Normalize();
            effect.CurrentTechnique = effect.Techniques["MultiTextured"];
            effect.Parameters["xTexture0"].SetValue(sand);
            effect.Parameters["xTexture1"].SetValue(grass);
            effect.Parameters["xTexture2"].SetValue(rock);
            effect.Parameters["xTexture3"].SetValue(snow);
            effect.Parameters["xEnableLighting"].SetValue(true);
            effect.Parameters["xAmbient"].SetValue(0.1f);
            effect.Parameters["xLightDirection"].SetValue(lightDir);
        }


    }
}
