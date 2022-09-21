﻿using Cosmos.System;

namespace PrismGL2D.UI
{
    public class Switch : Control
    {
        public bool Toggled { get; set; }

        public override void OnClickEvent(int X, int Y, MouseState State)
        {
            IsEnabled = !IsEnabled;
            base.OnClickEvent(X, Y, State);
        }

        public override void OnDrawEvent(Graphics Buffer)
        {
            base.OnDrawEvent(this);

            DrawFilledRectangle(1, 1, (int)(Width - 2), (int)(Height - 2), (int)Config.Radius, Config.GetBackground(false, false));
            DrawFilledRectangle((int)(Toggled ? 2 : Width / 2 + 2), 0, (int)(Width / 2), (int)(Width - 2), (int)Config.Radius, Config.AccentColor);

            if (HasBorder)
            {
                DrawRectangle(0, 0, (int)(Width - 1), (int)(Height - 1), (int)Config.Radius, Config.AccentColor);
            }

            Buffer.DrawImage(X, Y, this, Config.ShouldContainAlpha(this));
        }
    }
}