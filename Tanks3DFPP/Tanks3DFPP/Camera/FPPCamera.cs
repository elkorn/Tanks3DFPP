using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Tanks3DFPP.Camera.Interfaces;
using Tanks3DFPP.Utilities;
using Tanks3DFPP.Tanks;

namespace Tanks3DFPP.Camera
{
    internal class FPPCamera : ICamera
    {
        private readonly float maxPitch;
        private readonly Matrix projection;
        private readonly float rotationSpeed;
        private Vector3 lookAt;
        private float moveSpeed;
        private MouseState originalMouseState;
        private float pitchAngle;
        private MouseState referenceMouseState;

        private Vector3 up;
        private float yawAngle;
        private bool bNeedsToResetPitchAngle;

        public bool ControlledByMouse { get; set; }

        public FPPCamera(GraphicsDevice device, Vector3 startingPosition, float rotationSpeed, float moveSpeed,
                         Matrix projection)
        {
            Mouse.SetPosition(device.Viewport.Width/2, device.Viewport.Height/2);
            originalMouseState = referenceMouseState = Mouse.GetState();
            this.rotationSpeed = rotationSpeed;
            this.moveSpeed = moveSpeed;
            Position = startingPosition;
            this.projection = projection;
            maxPitch = MathHelper.ToRadians(90);
        }

        public Vector3 Direction { get; private set; }

        private Matrix Rotation
        {
            get { return Matrix.CreateRotationX(pitchAngle)*Matrix.CreateRotationY(yawAngle); }
        }

        #region ICamera Members

        public BoundingFrustum Frustum
        {
            get { return new BoundingFrustum(View*projection); }
        }

        public Vector3 Position { get; set; }

        public Vector3 LookAt
        {
            get { return lookAt; }
            set
            {
                lookAt = value;
                Direction = Position - lookAt;
                Direction.Normalize();
            }
        }

        public Ray Ray
        {
            get
            {
                return new Ray(this.Position, this.Direction);
            }
        }

        /*
         *      YAW     - around y axis
         *      PITCH   - around X axis
         *      ROLL    - around Z axis
         */

        public Matrix View
        {
            get { return Matrix.CreateLookAt(Position, LookAt, up); }
        }

        public void Update(GameTime gameTime)
        {
            //Vector3 velocity = Vector3.Zero;
            //KeyboardHandler.TurboKeyAction(Keys.W, () =>
            //    {
            //        velocity += -Vector3.UnitZ;
            //    });
            //KeyboardHandler.TurboKeyAction(Keys.S, () =>
            //    {
            //        velocity += Vector3.UnitZ;
            //    });
            //KeyboardHandler.TurboKeyAction(Keys.A, () =>
            //    {
            //        velocity += -Vector3.UnitX;
            //    });
            //KeyboardHandler.TurboKeyAction(Keys.D, () =>
            //    {
            //        velocity += Vector3.UnitX;
            //    });

            //KeyboardHandler.TurboKeyAction(Keys.OemPlus, () => { moveSpeed++; });
            //KeyboardHandler.TurboKeyAction(Keys.OemMinus, () => { moveSpeed--; });

            //if (velocity != Vector3.Zero)
            //{
            //    Move(velocity);
            //}

            if (this.ControlledByMouse)
            {
                Rotate((float) (gameTime.ElapsedGameTime.TotalMilliseconds/1000));
            }
        }

        #endregion

        private Vector2 GetMousePositionDifference()
        {
            MouseState currentState = Mouse.GetState();
            return new Vector2(currentState.X - originalMouseState.X, currentState.Y - originalMouseState.Y);
        }

        private void Move(Vector3 velocity)
        {
            Position += moveSpeed*Vector3.Transform(velocity, Rotation);
            UpdateView();
        }

        private void Rotate(float increment)
        {
            Vector2 dRotation = GetMousePositionDifference();
            yawAngle -= rotationSpeed*dRotation.X*increment;
            pitchAngle += rotationSpeed*dRotation.Y*increment;
            pitchAngle = MathHelper.Clamp(pitchAngle, -maxPitch, maxPitch);
            Mouse.SetPosition(referenceMouseState.X, referenceMouseState.Y);
            UpdateView();
        }

        private void UpdateView()
        {
            Matrix rotation = Rotation;
            Vector3 originalTarget = -Vector3.UnitZ,
                    rotatedTarget = Vector3.Transform(originalTarget, rotation);
            LookAt = Position + rotatedTarget;
            up = Vector3.Transform(Vector3.Up, rotation);
        }

        public void AttachAndUpdate(Tank tank)
        {
            // get difference in positions
            // compute yawangle and pitchangle
            pitchAngle = -tank.PreviousCannonDirectionAngle;
            yawAngle = tank.PreviousTurretDirectionAngle;
            pitchAngle -= tank.CannonDirectionAngle - tank.PreviousCannonDirectionAngle;
            yawAngle += tank.TurretDirectionAngle - tank.PreviousTurretDirectionAngle;

            Position = tank.CannonPosition;
            UpdateView();
            bNeedsToResetPitchAngle = true;
        }

        public void AttachAndUpdate(Vector3 missilePos)
        {
            if (bNeedsToResetPitchAngle)
            {
                pitchAngle = 0;
                bNeedsToResetPitchAngle = false;
            }

            this.Position = missilePos;
            LookAt = -this.up;
        }
    }
}
