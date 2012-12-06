using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Tanks3DFPP.Utilities
{
    public static class MouseHandler
    {
        public static Vector2 GetPositionDifference(MouseState sourceState)
        {
            MouseState currentState = Mouse.GetState();
            return new Vector2(currentState.X - sourceState.X, currentState.Y - sourceState.Y);
        }
    }
}
