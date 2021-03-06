﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tanks3DFPP.Entities;

namespace Tanks3DFPP.Tanks
{
    public class Tank: CollidingEntity
    {
        const float TurretTurnSpeed = 0.015f;
        const float CannonDegMax = -90;
        const float CannonDegMin = 0;
        const float ScaleFactor = 0.1f;
        const float MaxPower = 10.0f;
        const float MinPower = 1.5f;
        readonly Matrix ScaleMatrix = Matrix.CreateScale(ScaleFactor);

        public String PlayerName { get; set; }

        #region Properties

        //public Vector3 Position
        //{
        //    get { return base.Position; }
        //}

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

        public float PreviousTurretDirectionAngle
        {
            get { return previousTurretDirectionAngle; }
        }
        private float previousTurretDirectionAngle;

        public float PreviousCannonDirectionAngle
        {
            get { return previousCannonDirectionAngle; }
        }
        private float previousCannonDirectionAngle;

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

        public Matrix CameraOrientation
        {
            get { return cameraOrientation; }
        }
        private Matrix cameraOrientation;

        public bool bPowerIncreases { get; private set; }

        public bool bPowerDecreases { get; private set; }

        public bool bTurretMoves { get; private set; }

        public bool bCannonMoves { get; private set; }

        public int Health { get; set; }

        #endregion

        #region Fields

        public Model Model { get; private set; }

        Matrix tankOrientation = Matrix.Identity;
        Matrix turretOrientation = Matrix.Identity;
        Matrix cannonOrientation = Matrix.Identity;

        Matrix[] boneTransforms;
        Matrix worldMatrix;

        ModelBone turretBone;
        ModelBone cannonBone;

        Matrix turretTransform;
        Matrix cannonTransform;

        float turretTurnAmount;
        float cannonTurnAmount;
        float previousInitialVelocityPower;

        #endregion

        /// <summary>
        /// Loads the content.
        /// </summary>
        /// <param name="content">The content.</param>
        public void LoadContent(ContentManager content)
        {
            Model = content.Load<Model>("Tank");
            boneTransforms = new Matrix[Model.Bones.Count];

            boundingSpheres = new List<BoundingSphere>();
            foreach (ModelMesh mesh in Model.Meshes)
            {
                boundingSpheres.Add(new BoundingSphere(mesh.BoundingSphere.Center, mesh.BoundingSphere.Radius * ScaleFactor));
            }

            turretBone = Model.Bones["turret_geo"];
            cannonBone = Model.Bones["canon_geo"];

            turretTransform = turretBone.Transform;
            cannonTransform = cannonBone.Transform;

        }

        public void SpawnAt(Vector3 location)
        {
            base.Position = this.OffsetToFloorHeight(Game1.heightMap, location);
            Health = 100;
            initialVelocityPower = 1.5f;
            previousInitialVelocityPower = initialVelocityPower;
            previousCannonDirectionAngle = cannonDirectionAngle;
            previousTurretDirectionAngle = turretDirectionAngle;

            //Model.CopyAbsoluteBoneTransformsTo(boneTransforms);
            //worldMatrix = tankOrientation * Matrix.CreateTranslation(Position);
            this.InitializeTransforms();

            for (int i = 0; i < Model.Meshes.Count; ++i)
            {
                ModelMesh mesh = Model.Meshes[i];
                boundingSpheres[i] = new BoundingSphere(mesh.BoundingSphere.Transform(boneTransforms[mesh.ParentBone.Index] * ScaleMatrix * worldMatrix).Center,
                    mesh.BoundingSphere.Radius * ScaleFactor);
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
            turretTurnAmount = 0;
            if (KS.IsKeyDown(Keys.Left))
            {
                turretTurnAmount += 1;
            }
            if (KS.IsKeyDown(Keys.Right))
            {
                turretTurnAmount -= 1;
            }

            cannonTurnAmount = 0;
            if (KS.IsKeyDown(Keys.Up))
            {
                cannonTurnAmount -= 1;
            }
            if (KS.IsKeyDown(Keys.Down))
            {
                cannonTurnAmount += 1;
            }

            previousInitialVelocityPower = initialVelocityPower;
            if(KS.IsKeyDown(Keys.OemPlus))
            {
                initialVelocityPower += 0.01f;
            }
            if (KS.IsKeyDown(Keys.OemMinus))
            {
                initialVelocityPower -= 0.01f;
            }

            previousTurretDirectionAngle = turretDirectionAngle;
            turretTurnAmount = MathHelper.Clamp(turretTurnAmount, -1, +1);
            turretDirectionAngle += turretTurnAmount * TurretTurnSpeed;
            turretDirectionAngle = turretDirectionAngle % MathHelper.ToRadians(360);

            previousCannonDirectionAngle = cannonDirectionAngle;
            cannonTurnAmount = MathHelper.Clamp(cannonTurnAmount, -1, +1);
            cannonDirectionAngle += cannonTurnAmount * TurretTurnSpeed;
            cannonDirectionAngle = MathHelper.Clamp(cannonDirectionAngle, MathHelper.ToRadians(CannonDegMax), MathHelper.ToRadians(CannonDegMin));

            initialVelocityPower = MathHelper.Clamp(initialVelocityPower, MinPower, MaxPower);

            turretOrientation = Matrix.CreateRotationY(turretDirectionAngle);
            cannonOrientation = Matrix.CreateRotationX(cannonDirectionAngle);

            //sound
            if (cannonDirectionAngle != previousCannonDirectionAngle)
                bCannonMoves = true;
            else
                bCannonMoves = false;

            if (turretTurnAmount != 0)
                bTurretMoves = true;
            else
                bTurretMoves = false;

            if (previousInitialVelocityPower > initialVelocityPower)
                bPowerIncreases = true;
            else if (previousInitialVelocityPower < initialVelocityPower)
                bPowerDecreases = true;
            else
            {
                bPowerIncreases = false;
                bPowerDecreases = false;
            }
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

        private void InitializeTransforms()
        {
            turretBone.Transform = turretOrientation * turretTransform;
            cannonBone.Transform = cannonOrientation * cannonTransform;

            Model.CopyAbsoluteBoneTransformsTo(boneTransforms);

            worldMatrix = tankOrientation * Matrix.CreateTranslation(Position);

            cameraOrientation = turretBone.Transform * cannonBone.Transform * ScaleMatrix * worldMatrix;
            cannonPosition = (turretBone.Transform * cannonBone.Transform * ScaleMatrix * worldMatrix).Translation;
            //cannonPosition.Y += 2 * (cannonPosition.Y - Position.Y);
        }

        public void Draw(Matrix viewMatrix, Matrix projectionMatrix)
        {
            this.InitializeTransforms();
            for (int i = 0; i < Model.Meshes.Count; ++i)
            {
                ModelMesh mesh = Model.Meshes[i];
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

        public bool IsOnMap
        {
            get { return IsInFloorBounds(Game1.heightMap); }
        }

        protected override bool IsInFloorBounds(Terrain.IHeightMap floor)
        {
            return this.IsInFloorBounds(floor, this.Position);
        }

        protected override bool IsInFloorBounds(Terrain.IHeightMap floor, Vector3 position)
        {
            //foreach (ModelMesh mesh in this.model.Meshes)
            //{
            //    BoundingSphere sphere = mesh.BoundingSphere.Transform(
            //        mesh.ParentBone.Transform 
            //        * this.tankOrientation 
            //        * Matrix.CreateTranslation(position));   // probably needs fixing.
            //    if (sphere.Center.X + sphere.Radius > floor.Width * Game1.GameParameters.MapScale
            //        || sphere.Center.X - sphere.Radius < 0
            //        || sphere.Center.Z + sphere.Radius > 0
            //        || sphere.Center.Z - sphere.Radius < -floor.Height * Game1.GameParameters.MapScale)
            //    {
            //        return false;
            //    }
            //}

            foreach (BoundingSphere sphere in boundingSpheres)
            {
                if (sphere.Center.X + sphere.Radius > floor.Width * Game1.GameParameters.MapScale
                    || sphere.Center.X - sphere.Radius < 0
                    || sphere.Center.Z - sphere.Radius < 0
                    || sphere.Center.Z + sphere.Radius > floor.Height * Game1.GameParameters.MapScale)
                {
                    return false;
                }
            }

            return true;
        }

        protected override Vector3 OffsetToFloorHeight(Terrain.IHeightMap floor)
        {
            return this.OffsetToFloorHeight(floor, this.Position);
        }

        protected override Vector3 OffsetToFloorHeight(Terrain.IHeightMap floor, Vector3 position)
        {
            BoundingSphere mergedSphere = new BoundingSphere();
            foreach (ModelMesh mesh in this.Model.Meshes)
            {
                mergedSphere = BoundingSphere.CreateMerged(mergedSphere, mesh.BoundingSphere.Transform(mesh.ParentBone.Transform));
            }

            return new Vector3(
                    position.X,
                    floor.Data[(int)(position.Z / Game1.GameParameters.MapScale), (int)(position.X / Game1.GameParameters.MapScale)]
                    * Game1.GameParameters.MapScale - floor.HeightOffset,
                    position.Z);
        }

    }
}
