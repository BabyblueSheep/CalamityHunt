﻿using Microsoft.Xna.Framework;

namespace CalamityHunt.Content.Bosses.Goozma
{
    public static class GoozmaColorUtils
    {
        public static Vector3[] Oil = new Vector3[]
        {
            new Color(0, 0, 0).ToVector3(),
            new Color(51, 46, 78).ToVector3(),
            new Color(174, 23, 189).ToVector3(),
            new Color(255, 61, 200).ToVector3(),
            new Color(237, 128, 60).ToVector3(),
            new Color(247, 255, 101).ToVector3(),
            new Color(176, 234, 85).ToVector3(),
            new Color(102, 219, 249).ToVector3(),
            new Color(0, 0, 0).ToVector3()
        };

        public static Vector3[] Nuclear = new Vector3[]
        {
            new Color(0, 0, 0).ToVector3(),
            new Color(22, 81, 12).ToVector3(),
            new Color(87, 87, 87).ToVector3(),
            new Color(0, 236, 74).ToVector3(),
            new Color(66, 255, 176).ToVector3(),
            new Color(255, 255, 20).ToVector3(),
            new Color(186, 255, 0).ToVector3(),
            new Color(55, 255, 28).ToVector3(),
            new Color(0, 0, 0).ToVector3()
        };        
        
        public static Vector3[] Gold = new Vector3[]
        {
            new Color(0, 0, 0).ToVector3(),
            new Color(23, 16, 9).ToVector3(),
            new Color(40, 28, 15).ToVector3(),
            new Color(213, 150, 78).ToVector3(),
            new Color(242, 192, 100).ToVector3(),
            new Color(255, 255, 147).ToVector3(),
            new Color(143, 99, 52).ToVector3(),
            new Color(111, 80, 41).ToVector3(),
            new Color(0, 0, 0).ToVector3()
        };

        public static Vector3[] Grayscale = new Vector3[]
        {
            new Color(0, 0, 0).ToVector3(),
            new Color(51, 51, 51).ToVector3(),
            new Color(87, 87, 87).ToVector3(),
            new Color(120, 120, 120).ToVector3(),
            new Color(153, 153, 153).ToVector3(),
            new Color(235, 235, 235).ToVector3(),
            new Color(200, 200, 200).ToVector3(),
            new Color(187, 187, 187).ToVector3(),
            new Color(0, 0, 0).ToVector3()
        };

        public static Vector3[] Test 
        {
            get
            {
                return Oil;
                return new Vector3[]
                {
                    new Color(0, 0, 0).ToVector3(),
                    new Color(23, 16, 9).ToVector3(),
                    new Color(40, 28, 15).ToVector3(),
                    new Color(213, 150, 78).ToVector3(),
                    new Color(242, 192, 100).ToVector3(),
                    new Color(255, 255, 147).ToVector3(),
                    new Color(143, 99, 52).ToVector3(),
                    new Color(111, 80, 41).ToVector3(),
                    new Color(0, 0, 0).ToVector3()
                };
            }
        }
    }
}
