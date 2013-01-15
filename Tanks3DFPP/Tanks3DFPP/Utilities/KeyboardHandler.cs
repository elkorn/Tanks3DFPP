using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;

namespace Tanks3DFPP.Utilities
{
    public static class KeyboardHandler
    {
        private static Dictionary<Action, bool> actionSafeGuards = new Dictionary<Action, bool>();

        private static bool anyKeySafeGuard;

        public static void KeyAction(Keys key, Action action)
        {
            var ks = Game1.CurrentKeyboardState;
            if (ks.IsKeyDown(key))
            {
                if (!actionSafeGuards.ContainsKey(action))
                {
                    actionSafeGuards.Add(action, false);
                }
                else
                {
                    if (!actionSafeGuards[action])
                    {
                        actionSafeGuards[action] = true;
                    }
                }
            }

            if (ks.IsKeyUp(key) && actionSafeGuards.ContainsKey(action) && actionSafeGuards[action])
            {
                action.Invoke();
                actionSafeGuards[action] = false;
            }
        }

        public static void TurboKeyAction(Keys key, Action action)
        {
            if (Game1.CurrentKeyboardState.IsKeyDown(key))
            {
                action.Invoke();
            }
        }

        public static void AnyKey(Action action)
        {
            if (Game1.CurrentKeyboardState.GetPressedKeys().Length > 0)
            {
                anyKeySafeGuard = true;
            }
            else
            {
                if (anyKeySafeGuard)
                {
                    action.Invoke();
                    anyKeySafeGuard = false;
                }
            }
        }
    }
}
