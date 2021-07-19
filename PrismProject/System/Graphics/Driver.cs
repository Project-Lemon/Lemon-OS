﻿using Cosmos.System.Graphics;
using System;
using System.Drawing;
using System.IO;

namespace PrismProject
{
    internal class Driver
    {
        public static int screenY = 720;
        public static int screenX = 1280;
        public static SVGAIICanvas canvas = new SVGAIICanvas(new Mode(screenX, screenY, ColorDepth.ColorDepth32));
        public static string font = "YuGothicUI";
        private static readonly Random rnd = new Random();
        public static int randomcolor = rnd.Next(256) + rnd.Next(256) + rnd.Next(256);

        public static void Init()
        {
            string CustomCharset = "🡬abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()~`\"\':;?/>.<,{[}]\\|+=_-";
            MemoryStream YuGothicUICustomCharset16 = new MemoryStream(Convert.FromBase64String("AAAAAB/8GAwUFBIkEUQQhBCEEUQSJBQUGBwf/AAAAAAAAAAAH/wYDBQUEiQRRBCEEIQRRBIkFBQYHB/8AAAAAAAAAAAAAAAAAAAOABMAAQAPABEAEwAdAAAAAAAAAAAAAAAAABAAEAAQABcAGYAQgBCAEIAZgBcAAAAAAAAAAAAAAAAAAAAAAAAADwAZABAAEAAQABAADgAAAAAAAAAAAAAAAAAAgACAAIAOgBGAEIAQgBGAEYAOgAAAAAAAAAAAAAAAAAAAAAAAAA4AEQARAB8AEAAQAA8AAAAAAAAAAAAAAAAADAAIABAAPAAQABAAEAAQABAAEAAAAAAAAAAAAAAAAAAAAAAAAAAOgBGAEIAQgBGAEYAOgAEAAQAeAAAAAAAAABAAEAAQABcAGQARABEAEQARABEAAAAAAAAAAAAAAAAAEAAAAAAAEAAQABAAEAAQABAAEAAAAAAAAAAAAAAAAAAQAAAAAAAQABAAEAAQABAAEAAQABAAEABgAAAAAAAAABAAEAAQABMAEgAUABgAFAASABEAAAAAAAAAAAAAAAAAEAAQABAAEAAQABAAEAAQABAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAABZwGZAREBEQERAREBEQAAAAAAAAAAAAAAAAAAAAABcAGQARABEAEQARABEAAAAAAAAAAAAAAAAAAAAAAAAADwARgBCAEIAQgBGADwAAAAAAAAAAAAAAAAAAAAAAAAAXABmAEIAQgBCAGYAXABAAEAAQAAAAAAAAAAAAAAAAAA6AEYAQgBCAEYARgA6AAIAAgACAAAAAAAAAAAAAAAAAFAAYABAAEAAQABAAEAAAAAAAAAAAAAAAAAAAAAAAAAAeABAAEAAMAAIAAgAcAAAAAAAAAAAAAAAAAAAAEAAQADwAEAAQABAAEAAYAAwAAAAAAAAAAAAAAAAAAAAAAAAAEQARABEAEQARABEADwAAAAAAAAAAAAAAAAAAAAAAAAAhABEAEgASAAoADAAMAAAAAAAAAAAAAAAAAAAAAAAAACIgEyATIBVAFUAMwAiAAAAAAAAAAAAAAAAAAAAAAAAAEgASAAwADAAMABIAMgAAAAAAAAAAAAAAAAAAAAAAAAAhABEAEgASAAoADAAMAAgACAAwAAAAAAAAAAAAAAAAAB4AAgAEAAgACAAQAD8AAAAAAAAAAAAAAAAAAAAGAAYABQAJAAkAGIAfgBDAMEAAAAAAAAAAAAAAAAAAAB8AEQARABEAHgARgBCAEYAfAAAAAAAAAAAAAAAAAAAAB4AIABAAEAAQABAAEAAIAAeAAAAAAAAAAAAAAAAAAAAfABDAEEAQQBBgEEAQQBCAHwAAAAAAAAAAAAAAAAAAAB8AEAAQABAAHwAQABAAEAAfAAAAAAAAAAAAAAAAAAAAHwAQABAAEAAfABAAEAAQABAAAAAAAAAAAAAAAAAAAAAHwAhAEAAQABHAEEAQQAhAB4AAAAAAAAAAAAAAAAAAABBAEEAQQBBAH8AQQBBAEEAQQAAAAAAAAAAAAAAAAAAAEAAQABAAEAAQABAAEAAQABAAAAAAAAAAAAAAAAAAAAAMAAwADAAMAAwADAAIAAgAMAAAAAAAAAAAAAAAAAAAABGAEQASABQAHAAUABIAEQAQgAAAAAAAAAAAAAAAAAAAEAAQABAAEAAQABAAEAAQAB8AAAAAAAAAAAAAAAAAAAAAABgQGDAUMBQwElASUBKQEZAREAAAAAAAAAAAAAAAABhgHGAUYBJgEmARYBDgEKAQYAAAAAAAAAAAAAAAAAAAB4AIQBAgECAQIBAgECAIQAeAAAAAAAAAAAAAAAAAAAAfABGAEIAQgBEAHgAQABAAEAAAAAAAAAAAAAAAAAAAAAeACEAQIBAgECAQIBAgCEAHwABgAAAAAAAAAAAAAAAAHwARgBCAEQAeABMAEQAQgBCAAAAAAAAAAAAAAAAAAAAPABEAEAAYAA4AAwABAAEAHgAAAAAAAAAAAAAAAAAAAD+ABAAEAAQABAAEAAQABAAEAAAAAAAAAAAAAAAAAAAAEEAQQBBAEEAQQBBAEEAYgA8AAAAAAAAAAAAAAAAAAAAgQBCAEIAYgAkACQAHAAYABgAAAAAAAAAAAAAAAAAAAAAAIIQRjBGIEUgaSApQClAMMAQwAAAAAAAAAAAAAAAAEIAZAAkABgAGAAYACQARgBCAAAAAAAAAAAAAAAAAAAAwgBEAEQAKAAoABAAEAAQABAAAAAAAAAAAAAAAAAAAAB+AAQADAAIABAAMAAgAEAA/gAAAAAAAAAAAAAAAAAAABgAaAAIAAgACAAIAAgACAAIAAAAAAAAAAAAAAAAAAAAOABEAAQABAAIABAAYABAAHwAAAAAAAAAAAAAAAAAAAB4AEwABAAIADAADAAEAAQAeAAAAAAAAAAAAAAAAAAAAAgAGAAYACgASABIAP4ACAAIAAAAAAAAAAAAAAAAAAAAfABAAEAAeAAMAAQABABMAHgAAAAAAAAAAAAAAAAAAAAcACAAQAB4AEQARABEAEQAOAAAAAAAAAAAAAAAAAAAAHwABAAIAAgAEAAQABAAIAAgAAAAAAAAAAAAAAAAAAAAOABEAEQARAA4AEQARABEADgAAAAAAAAAAAAAAAAAAAA4AEQARABEAEQAPAAEAAwAeAAAAAAAAAAAAAAAAAAAADgARABEAEQARABEAEQARAA4AAAAAAAAAAAAAAAAAAAAQABAAEAAQABAAEAAAABAAEAAAAAAAAAAAAAAAAAAAAAAAA+AEEAmIEkgSSBJIEkgLsAwAA+AAAAAAAAAAAAAABQAFAB+ACQAKAD+ACgASAAAAAAAAAAAAAAAAAAAABAAPABUAFAAcAA4ABwAFABUAHgAEAAAAAAAAAAAAAAAAABxAEkASgBMAHWACkASQBJAIYAAAAAAAAAAAAAAAAAAAAgAGAAUACIAIgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAHAAkACSAOIAsgEKAQwBDADzAAAAAAAAAAAAAAAAAAAAgAHgAIABQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACAAIABAAEAAQABAAEAAQABAACAAIAAAAAAAAAAAAAAAgABAACAAIAAgACAAIAAgACAAQABAAAAAAAAAAAAAAAAAAAAAAAAAADkATgAAAAAAAAAAAAAAAAAAAAAAAABAACAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAUABQAFAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAAEAAQAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABAAEAAAAAAAAAAQABAAAAAAAAAAAAAAAAAAAAAAAAAAEAAQAAAAAAAAAAAAEAAQACAAAAAAAAAAAAAAAB4AAgACAAQACAAIAAAACAAIAAAAAAAAAAAAAAAAAAAAAgACAAQABAAIAAgACAAQABAAIAAgAAAAAAAAAAAAAAAAAAAACAAGAAEAAYAGAAgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEAAQAAAAAAAAAAAAAAAAAAAAAAAAAACAAwAEAAwAAwAAgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEAAQACAAAAAAAAAAAAAAAAwACAAIAAgAEAAQABAACAAIAAgADAAAAAAAAAAAAAAAHAAQABAAEAAQABAAEAAQABAAEAAcAAAAAAAAAAAAAAAwABAAEAAQABgACAAYABAAEAAQADAAAAAAAAAAAAAAADgACAAIAAgACAAIAAgACAAIAAgAOAAAAAAAAAAAAAAAEQARAAoACgAfAAQAHwAEAAQAAAAAAAAAAAAAAAAAEAAQABAAEAAQABAAEAAQABAAEAAQABAAEAAAAAAAAAAAAAAAAgACAAIAH4ACAAIAAgAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAH4AAAAAAH4AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD4AAAAAAAAAAAAAAAAAAAAAAAAAHAAAAAAAAAAAAAAAAAAAAAAA"));
            BitFont.RegisterBitFont("YuGothicUI", new BitFontDescriptor(CustomCharset, YuGothicUICustomCharset16, 16));
        }

        public static void Clear(Color clr)
        {
            canvas.Clear(clr);
        }
    }
}