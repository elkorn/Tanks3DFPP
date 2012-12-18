using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;

namespace Tanks3DFPP.Tanks
{
    public class Tank
    {
        const float TurretTurnSpeed = 0.015f;
        const float CannonDegMax = -90;
        const float CannonDegMin = 0;
        const float ScaleFactor = 0.05f;
        const float MaxPower = 10.0f;
        const float MinPower = 0.1f;
        readonly Matrix ScaleMatrix = Matrix.CreateScale(ScaleFactor);

        public String PlayerName { get; set; }

        #region Properties

        public Vector3 Position
        {
            get { return position; }
        }
        private Vector3 position;

        public Vector3 CannonPosition
        {
            get { return cannonPosition; }
        }
        private Vector3 cannonPosition;

        public float TankDirection
        {
            get { return tankDirection; }
        }
        private float tankDirection;

        public float TurretDirectionAngle
        {
            get { return turretDirectionAngle; }
        }
        private float turretDirectionAngle;

        public float CannonDirectionAngle
        {
            get { return cannonDirectionAngle; }
        }
        private float cannonDirectionAngle;

        public float InitialVelocityPower
        {
            get { return initialVelocityPower; }
        }
        private float initialVelocityPower;

        public List<BoundingSphere> BoundingSpheres
        {
            get { return boundingSpheres; }
        }
        private List<BoundingSphere> boundingSpheres;

        public int Health { get; set; }

        #endregion

        #region Fields

        Model model;

        Matrix tankOrientation = Matrix.Identity;
        Matrix turretOrientation = Matrix.Identity;
        Matrix cannonOrientation = Matrix.Identity;

        Matrix[] boneTransforms;
        Matrix worldMatrix;

        ModelBone turretBone;
        ModelBone cannonBone;

        Matrix turretTransform;
        Matrix cannonTransform;

        #endregion

        /// <summary>
        /// Loads the content.
        /// </summary>
        /// <param name="content">The content.</param>
        public void LoadContent(ContentManager content)
        {
            model = content.Load<Model>("Tank");
            boneTransforms = new Matrix[model.Bones.Count];

            boundingSpheres = new List<BoundingSphere>();
            foreach (ModelMesh mesh in model.Meshes)
            {
                boundingSpheres.Add(new BoundingSphere(mesh.BoundingSphere.Center, mesh.BoundingSphere.Radius * ScaleFactor));
            }

            turretBone = model.Bones["turret_geo"];
            cannonBone = model.Bones["canon_geo"];

            turretTransform = turretBone.Transform;
            cannonTransform = cannonBone.Transform;
        }

        public void SpawnAt(Vector3 location)
        {
            position = location;
            Health = 100;
            initialVelocityPower = 1.0f;

            model.CopyAbsoluteBoneTransformsTo(boneTransforms);
            worldMatrix = tankOrientation * Matrix.CreateTranslation(Position);

            for (int i = 0; i < model.Meshes.Count; ++i)
            {
                ModelMesh mesh = model.Meshes[i];
                boundingSpheres[i] = new BoundingSphere(mesh.BoundingSphere.Transform(boneTransforms[mesh.ParentBone.Index] * ScaleMatrix * worldMatrix).Center,
                    mesh.BoundingSphere.Radius * ScaleFactor);
                mesh.Draw();
            }
        }

        public void BrakeDown()
        {
            boundingSpheres.Clear();
        }

        /// <summary>
        /// Handles the input.
        /// </summary>
        /// <param name="KS">The KS.</param>
        public void HandleInput(KeyboardState KS)
        {
            float turretTurnAmount = 0;
            if (KS.IsKeyDown(Keys.Left))
            {
                turretTurnAmount += 1;
            }
            if (KS.IsKeyDown(Keys.Right))
            {
                turretTurnAmount -= 1;
            }

            float cannonTurnAmount = 0;
            if (KS.IsKeyDown(Keys.Up))
            {
                cannonTurnAmount -= 1;
            }
            if (KS.IsKeyDown(Keys.Down))
            {
                cannonTurnAmount += 1;
            }

            if (KS.IsKeyDown(Keys.OemPlus))
            {
                initialVelocityPower += 0.02f;
            }
            if (KS.IsKeyDown(Keys.OemMinus))
            {
                initialVelocityPower -= 0.02f;
            }
            initialVelocityPower = MathHelper.Clamp(initialVelocityPower, MinPower, MaxPower);

            turretTurnAmount = MathHelper.Clamp(turretTurnAmount, -1, +1);
            turretDirectionAngle += turretTurnAmount * TurretTurnSpeed;
            turretDirectionAngle = turretDirectionAngle % MathHelper.ToRadians(360);

            cannonTurnAmount = MathHelper.Clamp(cannonTurnAmount, -1, +1);
            cannonDirectionAngle += cannonTurnAmount * TurretTurnSpeed;
            cannonDirectionAngle = MathHelper.Clamp(cannonDirectionAngle, MathHelper.ToRadians(CannonDegMax), MathHelper.ToRadians(CannonDegMin));

            turretOrientation = Matrix.CreateRotationY(turretDirectionAngle);
            cannonOrientation = Matrix.CreateRotationX(cannonDirectionAngle);
        }

        public bool CollisionCheckWith(Tank tank)
        {
            foreach (BoundingSphere thisSphere in this.boundingSpheres)
            {
                foreach (BoundingSphere thatSphere in tank.BoundingSpheres)
                {
                    if (thisSphere.Intersects(thatSphere))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public void Draw(Matrix viewMatrix, Matrix projectionMatrix)
        {
            turretBone.Transform = turretOrientation * turretTransform;
            cannonBone.Transform = cannonOrientation * cannonTransform;

            model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            worldMatrix = tankOrientation * Matrix.CreateTranslation(Position);

            cannonPosition = (cannonBone.Transform * ScaleMatrix * worldMatrix).Translation;
            cannonPosition.Y += 2 * cannonPosition.Y;

            for (int i = 0; i < model.Meshes.Count; ++i)
            {
                ModelMesh mesh = model.Meshes[i];
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index] * ScaleMatrix * worldMatrix;
                    effect.View = viewMatrix;
                    effect.Projection = projectionMatrix;

                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;
                }

                if (boundingSpheres.Count > 0)
                {
                    boundingSpheres[i] = new BoundingSphere(mesh.BoundingSphere.Transform(boneTransforms[mesh.ParentBone.Index] * ScaleMatrix * worldMatrix).Center,
                        mesh.BoundingSphere.Radius * ScaleFactor);
                }
                mesh.Draw();
            }
        }

    }
}
