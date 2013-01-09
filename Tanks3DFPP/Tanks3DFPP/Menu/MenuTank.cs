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

namespace Tanks3DFPP.Menu
{
    /// <summary>
    /// Class to hendle manu tank model.
    /// </summary>
    class MenuTank
    {

        private Model tankModel;
        private Matrix world;
        private ModelBone leftBackWheelBone;
        private ModelBone rightBackWheelBone;
        private ModelBone leftFrontWheelBone;
        private ModelBone rightFrontWheelBone;
        private ModelBone leftSteerBone;
        private ModelBone rightSteerBone;
        private ModelBone turretBone;
        private ModelBone cannonBone;
        private ModelBone hatchBone;
        public int which { get; set; }

        public int Getwhich() { return which; }
        private Matrix leftBackWheelTransform;
        private Matrix rightBackWheelTransform;
        private Matrix leftFrontWheelTransform;
        private Matrix rightFrontWheelTransform;
        private Matrix leftSteerTransform;
        private Matrix rightSteerTransform;
        private Matrix turretTransform;
        private Matrix cannonTransform;
        private Matrix hatchTransform;
        private Matrix[] boneTransforms;
        public float wheelRotationValue = 0;
        public float steerRotationValue = 45;
        private float cannonRotationValue = -15;
        public float hatchRotationValue = 0;
        private float[] turretpositions = new float[3];
        private float[] steerpositions = new float[3];

        public MenuTank()
        {
            which = 0;
        }

        public void move(Matrix translation)
        {
            world = world * translation;
        }

        public void Load(ContentManager content, Matrix worldm)
        {
            turretpositions[0] = 45;
            turretpositions[1] = 0;
            turretpositions[2] = -25;
            steerpositions[0] = 45;
            steerpositions[1] = 0;
            steerpositions[2] = -45;
            tankModel = content.Load<Model>(@"tank");
            world = worldm;
            leftBackWheelBone = tankModel.Bones["l_back_wheel_geo"];
            rightBackWheelBone = tankModel.Bones["r_back_wheel_geo"];
            leftFrontWheelBone = tankModel.Bones["l_front_wheel_geo"];
            rightFrontWheelBone = tankModel.Bones["r_front_wheel_geo"];
            leftSteerBone = tankModel.Bones["l_steer_geo"];
            rightSteerBone = tankModel.Bones["r_steer_geo"];
            turretBone = tankModel.Bones["turret_geo"];
            cannonBone = tankModel.Bones["canon_geo"];
            hatchBone = tankModel.Bones["hatch_geo"];

            leftBackWheelTransform = leftBackWheelBone.Transform;
            rightBackWheelTransform = rightBackWheelBone.Transform;
            leftFrontWheelTransform = leftFrontWheelBone.Transform;
            rightFrontWheelTransform = rightFrontWheelBone.Transform;
            leftSteerTransform = leftSteerBone.Transform;
            rightSteerTransform = rightSteerBone.Transform;
            turretTransform = turretBone.Transform;
            cannonTransform = cannonBone.Transform;
            hatchTransform = hatchBone.Transform;

            boneTransforms = new Matrix[tankModel.Bones.Count];
        }


        public void Draw(Matrix view, Matrix projection)
        {

            Matrix wheelRotation = Matrix.CreateRotationX(MathHelper.ToRadians(wheelRotationValue));
            Matrix steerRotation = Matrix.CreateRotationY(MathHelper.ToRadians(steerpositions[which]));
            Matrix turretRotation = Matrix.CreateRotationY(MathHelper.ToRadians(turretpositions[which]));
            Matrix cannonRotation = Matrix.CreateRotationX(MathHelper.ToRadians(cannonRotationValue));
            Matrix hatchRotation = Matrix.CreateRotationX(MathHelper.ToRadians(hatchRotationValue));

            leftBackWheelBone.Transform = wheelRotation * leftBackWheelTransform;
            rightBackWheelBone.Transform = wheelRotation * rightBackWheelTransform;
            leftFrontWheelBone.Transform = wheelRotation * leftFrontWheelTransform;
            rightFrontWheelBone.Transform = wheelRotation * rightFrontWheelTransform;
            leftSteerBone.Transform = steerRotation * leftSteerTransform;
            rightSteerBone.Transform = steerRotation * rightSteerTransform;
            turretBone.Transform = turretRotation * turretTransform;
            cannonBone.Transform = cannonRotation * cannonTransform;
            hatchBone.Transform = hatchRotation * hatchTransform;

            tankModel.CopyAbsoluteBoneTransformsTo(boneTransforms);

            foreach (ModelMesh mesh in tankModel.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index] * world;
                    effect.View = view;
                    effect.Projection = projection;

                    effect.EnableDefaultLighting();

                    effect.PreferPerPixelLighting = true;
                }
                mesh.Draw();
            }
        }

    }
}

