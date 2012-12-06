﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tanks3DFPP.Camera.Interfaces;
using Microsoft.Xna.Framework;
using Tanks3DFPP.Utilities;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Tanks3DFPP.Camera
{
    class FPPCamera : ICamera
    {

        private readonly float rotationSpeed, moveSpeed;

        private float yawAngle, pitchAngle;

        private Vector3 up;

        public Microsoft.Xna.Framework.Vector3 Position
        {
            get;
            set;
        }

        public Microsoft.Xna.Framework.Vector3 LookAt
        {
            get;
            set;
        }

        private Matrix Rotation
        {
            get
            {
                return Matrix.CreateRotationX(this.pitchAngle) * Matrix.CreateRotationY(this.yawAngle);
            }
        }

        MouseState referenceMouseState, originalMouseState;

        /*
         *      YAW     - around y axis
         *      PITCH   - around X axis
         *      ROLL    - around Z axis
         */


        public Microsoft.Xna.Framework.Matrix View
        {
            get
            {
                return Matrix.CreateLookAt(this.Position, this.LookAt, this.up);
            }
        }

        public FPPCamera(GraphicsDevice device, Vector3 startingPosition, float rotationSpeed, float moveSpeed)
        {
            Mouse.SetPosition(device.Viewport.Width / 2, device.Viewport.Height / 2);
            this.originalMouseState = this.referenceMouseState = Mouse.GetState();
            this.rotationSpeed = rotationSpeed;
            this.moveSpeed = moveSpeed;
            this.Position = startingPosition;
        }

        public void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            Vector3 velocity = Vector3.Zero;
            KeyboardHandler.TurboKeyAction(Keys.W, () =>
            {
                velocity += -Vector3.UnitZ;
            });
            KeyboardHandler.TurboKeyAction(Keys.S, () =>
            {
                velocity += Vector3.UnitZ;
            });
            KeyboardHandler.TurboKeyAction(Keys.A, () =>
            {
                velocity += -Vector3.UnitX;
            });
            KeyboardHandler.TurboKeyAction(Keys.D, () =>
            {
                velocity += Vector3.UnitX;
            });
            this.Move(velocity);
            this.Rotate((float)(gameTime.ElapsedGameTime.TotalMilliseconds / 1000));
        }

        private Vector2 GetMousePositionDifference()
        {
            MouseState currentState = Mouse.GetState();
            return new Vector2(currentState.X - this.originalMouseState.X, currentState.Y - this.originalMouseState.Y);
        }

        private void Rotate(float increment)
        {
            Vector2 dRotation = this.GetMousePositionDifference();
            this.yawAngle -= this.rotationSpeed * dRotation.X * increment;
            this.pitchAngle -= this.rotationSpeed * dRotation.Y * increment;
            Mouse.SetPosition(this.referenceMouseState.X, this.referenceMouseState.Y);
            UpdateView();
        }

        private void Move(Vector3 velocity)
        {
            this.Position += this.moveSpeed * Vector3.Transform(velocity, this.Rotation);
            this.UpdateView();
        }

        private void UpdateView()
        {
            Matrix rotation = this.Rotation;
            Vector3 originalTarget = -Vector3.UnitZ,
                    rotatedTarget = Vector3.Transform(originalTarget, rotation);
            this.LookAt = this.Position + rotatedTarget;
            this.up = Vector3.Transform(Vector3.Up, rotation);
        }
    }
}