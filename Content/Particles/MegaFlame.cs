﻿using CalamityHunt.Common.Systems.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ModLoader;

namespace CalamityHunt.Content.Particles
{
    public class MegaFlame : ParticleBehavior
    {
        public int time;
        public int maxTime;
        public int variant;
        public float rotationalVelocity;

        public override void OnSpawn()
        {
            variant = Main.rand.Next(8);
            scale *= 0.8f + Main.rand.NextFloat(0.9f, 1.1f);
            maxTime = (int)(25 * (scale * 0.2f + 0.8f));
        }

        public override void Update()
        {
            scale *= 0.99f;
            time++;

            rotation = velocity.ToRotation();

            if (time > maxTime * 0.3f)
            {
                scale *= 0.93f - Math.Clamp(scale * 0.0001f, 0f, 0.5f);
                velocity *= 0.95f;
                velocity.Y -= 0.1f;
            }
            else
                velocity *= 1.01f;

            if (time > maxTime * 0.8f)
                Active = false;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (data is string)
                if ((string)data == "Sludge")
                    return;

            Asset<Texture2D> texture = ModContent.Request<Texture2D>(Texture);
            Rectangle frame = texture.Frame(4, 2, variant % 4, (int)(variant / 4f));
            float grow = (float)Math.Sqrt(Utils.GetLerpValue(0, maxTime * 0.3f, time, true));
            float opacity = Utils.GetLerpValue(maxTime * 0.8f, 0, time, true) * Math.Clamp(scale, 0, 1);
            spriteBatch.Draw(texture.Value, position - Main.screenPosition, frame, color * opacity, rotation - MathHelper.PiOver2, frame.Size() * 0.5f, scale * grow * 0.5f, 0, 0);
        }
    }
}
