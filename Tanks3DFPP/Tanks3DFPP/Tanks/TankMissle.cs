using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input;
using Tanks3DFPP.Entities;

namespace Tanks3DFPP.Tanks
{
    public class TankMissle : CollidingEntity
    {
        const float ScaleFactor = 10.0f;

        #region Properties

        public Vector3 Position
        {
            get { return position; }
        }
        private Vector3 position;

        public Vector3 Velocity
        {
            get { return velocity; }
        }
        private Vector3 velocity;

        public Matrix Orientation
        {
            get { return orientation; }
        }
        private Matrix orientation;

        public BoundingSphere BoundingSphere
        {
            get { return boundingSphere; }
        }
        private BoundingSphere boundingSphere;

        // to mod by player
        public float InitialVelocityPower
        {
            get { return initialVelocityPower; }
        }
        float initialVelocityPower;

        #endregion

        #region Fields

        // to mod by changing missle type
        float initialGravityFactor = 1.0f;
        float gravityFactor = 1.02f;
        float aerodynamicFactor = 0.99f;

        float yawAngle, pitchAngle;
        Vector3 gravityForce;
        Vector3 previousPosition;

        Model model;
        Matrix[] transforms;
        Matrix initialOrientation;

        Vector3 FacingDirectionNorm;
        Vector3 NextFacingDirectionNorm;
        float angleDiff, radius;

        bool bInAir;

        #endregion

        public void LoadContent(ContentManager Content)
        {
            model = Content.Load<Model>("tank shell");
            transforms = new Matrix[model.Bones.Count];
            boundingSphere = new BoundingSphere();
            radius = model.Meshes[0].BoundingSphere.Radius;
            bInAir = false;
        }

        // PreShotInitialization
        public void SetPreShotValues(float turretDirectionAngle, float cannonDirectionAngle, Vector3 tankPosition, float initialVelocityPower)
        {
            yawAngle = turretDirectionAngle;
            pitchAngle = cannonDirectionAngle;
            initialOrientation = orientation = Matrix.CreateRotationY(MathHelper.ToRadians(180)) * Matrix.CreateTranslation(-Vector3.UnitZ) *
                        Matrix.CreateFromYawPitchRoll(yawAngle, pitchAngle, 0) * Matrix.CreateScale(ScaleFactor);
            position = tankPosition;

            gravityForce = -Vector3.UnitY * initialGravityFactor;
            velocity = Vector3.Transform(Vector3.UnitZ * -initialVelocityPower, orientation);
        }

        // CollisionPoint to change
        public bool UpdatePositionAfterShot(List<Tank> tanks, int except, out BoundingSphere SphereHit, out int HitIndex) //float CollisionPoint, 
        {
            SphereHit = new BoundingSphere(Vector3.Zero, 0f);
            HitIndex = -1;

            if (IsInFloorBounds(Game1.heightMap, position))
            {
                if (position.Y > this.OffsetToFloorHeight(Game1.heightMap, position).Y) //position.Y > CollisionPoint && 
                {
                    previousPosition = position;
                    position += velocity + gravityForce;
                    gravityForce *= gravityFactor;

                    if ( (position.Y < previousPosition.Y) && (-gravityForce.Y > gravityFactor * 5))
                    {
                        except = -1;
                    }

                    FacingDirectionNorm = position - previousPosition;
                    FacingDirectionNorm.Normalize();

                    NextFacingDirectionNorm = velocity;
                    NextFacingDirectionNorm.Normalize();

                    angleDiff = (float)Math.Acos(Vector3.Dot(FacingDirectionNorm, NextFacingDirectionNorm));

                    orientation = Matrix.CreateRotationY(MathHelper.ToRadians(180)) *
                        Matrix.CreateFromYawPitchRoll(yawAngle, pitchAngle + angleDiff, 0) *
                        Matrix.CreateScale(ScaleFactor);

                    // check for collision with tanks
                    for (int i = 0; i < tanks.Count; ++i)
                    {
                        if (i != except)
                        {
                            foreach (BoundingSphere sphere in tanks[i].BoundingSpheres)
                            {
                                if (this.boundingSphere.Intersects(sphere))
                                {
                                    bInAir = false;
                                    SphereHit = sphere;
                                    HitIndex = i;
                                    return true;
                                }
                            }
                        }
                    }

                    bInAir = true;
                    return false;
                }
            }

            bInAir = false;
            return true;
        }

        public void Draw(Matrix viewMatrix, Matrix projectionMatrix)
        {
            model.CopyAbsoluteBoneTransformsTo(transforms);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = transforms[mesh.ParentBone.Index] * orientation * Matrix.CreateTranslation(position);
                    effect.View = viewMatrix;
                    effect.Projection = projectionMatrix;
                    effect.TextureEnabled = false;
                }
                if (bInAir)
                {
                    mesh.Draw();
                }
            }
            boundingSphere = new BoundingSphere(this.position, radius);
        }

        protected override bool IsInFloorBounds(Terrain.IHeightMap floor)
        {
            return this.IsInFloorBounds(floor, this.Position);
        }

        protected override bool IsInFloorBounds(Terrain.IHeightMap floor, Vector3 position)
        {
            foreach (ModelMesh mesh in this.model.Meshes)
            {
                BoundingSphere sphere = mesh.BoundingSphere.Transform(
                    mesh.ParentBone.Transform
                    * this.orientation
                    * Matrix.CreateTranslation(position));   // probably needs fixing.
                if (sphere.Center.X + sphere.Radius > floor.Width * Game1.Scale
                    || sphere.Center.X - sphere.Radius < 0
                    || sphere.Center.Z + sphere.Radius > 0
                    || sphere.Center.Z - sphere.Radius < -floor.Height * Game1.Scale)
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
            //BoundingSphere mergedSphere = new BoundingSphere();
            //foreach (ModelMesh mesh in this.model.Meshes)
            //{
            //    mergedSphere = BoundingSphere.CreateMerged(mergedSphere, mesh.BoundingSphere.Transform(mesh.ParentBone.Transform));
            //}
            return new Vector3(
                    position.X,
                    floor.Data[(int)(position.X / Game1.Scale), (int)(-position.Z / Game1.Scale)]
                    * Game1.Scale - floor.HeightOffset,
                    position.Z);
        }
    }
}
