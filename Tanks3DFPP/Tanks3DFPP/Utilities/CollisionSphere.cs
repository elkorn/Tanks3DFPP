﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Tanks3DFPP.Camera.Interfaces;
using Tanks3DFPP.Terrain;
using Tanks3DFPP.Entities;


namespace Tanks3DFPP.Utilities
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class CollisionSphere : CollidingEntity
    {
        float velocity = 2f;
        float turnSpeed = .025f;
        float radius = 12.0f;

        public float FacingDirection { get; private set; }

        Model model;
        Matrix rollingMatrix = Matrix.Identity;

        Matrix[] boneTransforms;

        Vector3 movementQuant = Vector3.Zero;
        public CollisionSphere(Game game, IHeightMap floor, Vector3 startingPosition)
            : base(game, floor)
        {
            this.Position = startingPosition;
            this.model = this.Game.Content.Load<Model>("sphere");
            this.boneTransforms = new Matrix[this.model.Bones.Count];
            this.Position = this.OffsetToFloorHeight(startingPosition);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            float turnAmount = 0;
            KeyboardHandler.TurboKeyAction(Keys.Left, () =>
            {
                turnAmount += 1;
            });

            KeyboardHandler.TurboKeyAction(Keys.Right, () =>
            {
                turnAmount -= 1;
            });

            turnAmount = MathHelper.Clamp(turnAmount, -1, 1);
            this.FacingDirection += turnAmount *= this.turnSpeed;

            KeyboardHandler.TurboKeyAction(Keys.Up, () =>
            {
                movementQuant.Z -= .2f;
            });

            KeyboardHandler.TurboKeyAction(Keys.Down, () =>
            {
                movementQuant.Z += .2f;
            });

            // A matrix representing the sphere facing direction.
            Matrix facingMatrix = Matrix.CreateRotationY(this.FacingDirection);

            // The actual amount of movement that is going to be performed in each direction.
            Vector3 velocity = Vector3.Transform(movementQuant, facingMatrix * this.velocity);
            // The distance that the ball is to move.
            float distanceMoved = Vector3.Distance(this.Position, this.Position + velocity);

            if (this.IsInFloorBounds(this.Position + velocity))
            {
                this.Position = this.OffsetToFloorHeight(this.Position + velocity);
                #region Ball rolling around its own axis
                // The angular movement of the ball itself.
                float rotation = distanceMoved / this.radius;
                // Whether the ball has moved forward or backward.
                int rollDirection = movementQuant.Z > 0 ? 1 : -1;
                // The matrix allowing the ball to be drawn as rolled around its axis.
                this.rollingMatrix *= Matrix.CreateFromAxisAngle(facingMatrix.Right, rotation * rollDirection);
                #endregion
            }

            this.movementQuant *= .9f;
            base.Update(gameTime);
        }

        public void Draw(Matrix world, Matrix view, Matrix projection)
        {
            world = this.rollingMatrix * Matrix.CreateTranslation(this.Position);
            this.model.CopyAbsoluteBoneTransformsTo(this.boneTransforms);
            foreach (ModelMesh mesh in this.model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index] * world;
                    effect.View = view;
                    effect.Projection = projection;
                    effect.EnableDefaultLighting();
                    effect.PreferPerPixelLighting = true;


                    // fog here?! 
                    effect.FogEnabled = true;
                    effect.FogColor = Vector3.Zero;
                    effect.FogStart = 1000;
                    effect.FogEnd = 3200;
                }

                mesh.Draw();
            }
        }

        protected override bool IsInFloorBounds(Vector3 position)
        {
            return
                position.X + this.radius < this.floor.Width
                && position.X - this.radius > 0
                && position.Z + this.radius < this.floor.Height / 2
                && position.Z - this.radius > -this.floor.Height / 2;
        }

        protected override Vector3 OffsetToFloorHeight(Vector3 position)
        {
            // Calculating the height of the ball's position,
            // taking into account the ball's radius, the floor's height offset
            // and how the height map is laid out onto terrain.
            return new Vector3(
                    position.X,
                    this.floor.Data[(int)(this.Position.X), (int)(this.floor.Height / 2 - position.Z)] - this.floor.HeightOffset + this.radius,
                    position.Z);
        }
    }
}