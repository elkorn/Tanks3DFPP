using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tanks3DFPP.Camera.Interfaces;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Tanks3DFPP.Utilities;

namespace Tanks3DFPP.Camera
{
    public class OrbitingCamera : ICamera
    {

        private float maxVerticalAngle = MathHelper.ToRadians(85);
        private float minVerticalAngle = MathHelper.ToRadians(-85);
        private float cameraSpeed = 140.0f;
        private bool leftMousePreviouslyDown;
        private Point mouseStartPoint;
        private KeyboardHandler keyboardHandler = new KeyboardHandler();

        public Microsoft.Xna.Framework.Vector3 Position { get; set; }

        public Microsoft.Xna.Framework.Vector3 LookAt { get; set; }

        public BoundingFrustum Frustum
        {
            get
            {
                return new BoundingFrustum(this.View);
            }
        }

        public Matrix View
        {
            get 
            {
                return Matrix.CreateLookAt(this.Position, this.LookAt, Vector3.Up);
            }
        }

        public void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            Vector3 distance = this.Position - this.LookAt;
            Vector3 viewDirection = distance;
            viewDirection.Normalize();

            keyboardHandler.TurboKeyAction(Keys.W, () =>
            {
                Vector3 shift = (float)gameTime.ElapsedGameTime.TotalSeconds * viewDirection * cameraSpeed;
                this.Position -= shift;
                this.LookAt -= shift;
            });

            keyboardHandler.TurboKeyAction(Keys.S, () =>
            {
                Vector3 shift = (float)gameTime.ElapsedGameTime.TotalSeconds * viewDirection * cameraSpeed;
                this.Position += shift;
                this.LookAt += shift;
            });

            MouseState mouseState = Game1.CurrentMouseState;
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if (!leftMousePreviouslyDown)
                {
                    this.mouseStartPoint = new Point(mouseState.X, mouseState.Y);
                }

                // orbiting
                float horizontalAngle = (float)Math.Atan2(distance.Z, distance.X);

                distance = Vector3.Transform(distance, Matrix.CreateRotationY(horizontalAngle));

                float verticalAngle = (float)Math.Atan2(distance.Y, distance.X);

                distance = Vector3.Transform(distance, Matrix.CreateRotationZ(-verticalAngle));

                distance = Vector3.Transform(distance, Matrix.CreateRotationZ(MathHelper.Clamp(verticalAngle + MathHelper.ToRadians(mouseState.Y - this.mouseStartPoint.Y), this.minVerticalAngle, this.maxVerticalAngle)));

                distance = Vector3.Transform(distance, Matrix.CreateRotationY(-horizontalAngle));

                this.Position = this.LookAt + Vector3.Transform(distance, Matrix.CreateRotationY(-MathHelper.ToRadians(mouseState.X - this.mouseStartPoint.X)));

                this.mouseStartPoint = new Point(mouseState.X, mouseState.Y);

                //this.effect.View = Matrix.CreateLookAt(this.newPosition, this.LookAt, Vector3.Up);

                leftMousePreviouslyDown = true;
            }
            else
            {
                leftMousePreviouslyDown = false;
            }
        }

    }
}
