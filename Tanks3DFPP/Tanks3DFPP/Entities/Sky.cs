using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Tanks3DFPP.Camera.Interfaces;

namespace Tanks3DFPP.Entities
{
    public class Sky
    {
        private BasicEffect effect;
        private Model dome;
        private Matrix[] modelTransforms;
        private Matrix baseTransform;
        private Texture2D texture;
        public Sky(GraphicsDevice device, ContentManager content, Matrix projection, float width, int scale)
        {
            dome = content.Load<Model>("dome");
            texture = content.Load<Texture2D>("fractal_sky_by_nplmxandi-d5fo0bi");
          
            effect = new BasicEffect(device);
            effect.TextureEnabled = true;
            effect.Texture = texture;
            effect.Projection = projection;
            dome.Meshes[0].MeshParts[0].Effect = effect;
            modelTransforms = new Matrix[dome.Bones.Count];
            this.baseTransform = Matrix.CreateTranslation(0, -.3f, 0) * Matrix.CreateScale(width * scale);
        }

        public void Draw(ICamera camera)
        {
            dome.CopyAbsoluteBoneTransformsTo(modelTransforms);
            foreach (ModelMesh mesh in dome.Meshes)
            {
                foreach (BasicEffect currentEffect in mesh.Effects)
                {
                    Matrix worldMatrix = 
                        modelTransforms[mesh.ParentBone.Index] 
                        * this.baseTransform
                        * Matrix.CreateTranslation(camera.Position);
                    currentEffect.World = worldMatrix;
                    currentEffect.View = camera.View;
                }
                mesh.Draw();
            }
        }
    }
}
