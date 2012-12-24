using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Text;

namespace Tanks3DFPP.Menu
{
    /// <summary>
    /// Class used to handle chracters writing.
    /// </summary>
    public class Characters
    {
        /// <summary>
        /// List with font models.
        /// </summary>
        private List<Model> charactersmodels;

        /// <summary>
        /// Class constructor that loads all of the models.
        /// </summary>
        /// <param name="content"></param>
        public Characters(ContentManager content)
        {
            charactersmodels = new List<Model>();
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/A"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/B"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/C"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/D"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/E"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/F"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/G"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/H"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/I"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/J"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/K"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/L"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/M"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/N"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/O"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/P"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/Q"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/R"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/S"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/T"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/U"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/V"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/W"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/X"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/Y"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/Z"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/0"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/1"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/2"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/3"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/4"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/5"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/6"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/7"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/8"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/9"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/Dot"));
            charactersmodels.Add(content.Load<Model>(@"MenuContent/Font/dk"));
        }

        /// <summary>
        /// Public method used to draw word.
        /// </summary>
        /// <param name="word">Word to draw.</param>
        /// <param name="scale">Scale of the characters models.</param>
        /// <param name="position">Start position for the word.</param>
        /// <param name="view">View matrix.</param>
        /// <param name="projection">Projection matrix.</param>
        public void Draw(string word, float scale, Vector3 position, Matrix view, Matrix projection)
        {
            scale *= 18f;
            for (int j = 0; j < word.Length; ++j)
            {
                byte[] asciiBytes = Encoding.ASCII.GetBytes(word);
                int modelNumber = -1;
                if (asciiBytes[j] == 46)
                    modelNumber = 36;
                if (asciiBytes[j] >= 65 && asciiBytes[j] <= 90)
                    modelNumber = asciiBytes[j] - 65;
                if (asciiBytes[j] >= 48 && asciiBytes[j] <= 57)
                    modelNumber = asciiBytes[j] - 48 + 26;
                float space = 0;
                if (asciiBytes[j] == 32)
                    space += scale * 9;
                if (asciiBytes[j] == 58)
                    modelNumber = 37;

                if (modelNumber >= 0)
                    foreach (ModelMesh mesh in charactersmodels[modelNumber].Meshes)
                    {
                        foreach (BasicEffect effect in mesh.Effects)
                        {
                            Vector3 temp = new Vector3(position.X + j * scale * 9 + space, position.Y, position.Z);
                            effect.World = Matrix.Identity * Matrix.CreateScale(scale) * Matrix.CreateTranslation(temp);
                            effect.View = view;
                            effect.Projection = projection;
                            effect.EnableDefaultLighting();
                        }
                        mesh.Draw();
                    }
            }
        }

    }
}
