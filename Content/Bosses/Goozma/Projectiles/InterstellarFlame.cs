﻿using CalamityHunt.Common.Graphics.RenderTargets;
using CalamityHunt.Common.Systems.Particles;
using CalamityHunt.Content.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityHunt.Content.Bosses.Goozma.Projectiles
{
    public class InterstellarFlame : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 130;
            Projectile.height = 130;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
            Projectile.manualDirectionChange = true;
        }

        public ref float Time => ref Projectile.ai[0];

        public override void AI()
        {
            Projectile.velocity *= 0.9f;
            Projectile.scale += 0.1f;
            Color flameColor = Color.Lerp(Color.Lerp(new Color(150, 70, 35, 0), new Color(30, 15, 10, 0), Utils.GetLerpValue(1, 3, Time, true)), Color.DarkBlue * 0.1f, Utils.GetLerpValue(3, 8, Time, true)) * Utils.GetLerpValue(15, 5, Time, true);
            flameColor.A = 10;
            //CalamityHunt.particles.Add(Particle.Create<CosmicFlame>(particle => {
            //    particle.position = NPC.Center + Main.rand.NextVector2Circular(90, 60) * NPC.scale + NPC.velocity * (i / 6f) * 0.3f;
            //    particle.velocity = Main.rand.NextVector2Circular(4, 4) + NPC.velocity * (i / 6f) * 0.5f;
            //    particle.scale = Main.rand.NextFloat(1.5f, 2.5f) * NPC.scale;
            //    particle.color = Color.White;
            //}));
            //var interstellarFlame = Particle.NewParticle(ModContent.GetInstance<CosmicFlame>(), Projectile.Center + Main.rand.NextVector2Circular(30, 30), Main.rand.NextVector2Circular(5, 5) + Projectile.velocity * Utils.GetLerpValue(0, 8, Time, true), flameColor, (1f + Main.rand.NextFloat()) * Projectile.scale);
            //interstellarFlame.Add(new ParticleData<string> { Value = "Interstellar" });

            Time++;

            if (Time > 20) {
                Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}
