﻿using CalamityHunt.Common.Systems.Particles;
using CalamityHunt.Content.Bosses.Goozma;
using CalamityHunt.Content.Items.Rarities;
using CalamityHunt.Content.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityHunt.Content.Items.Materials
{
    public class ChromaticMass : ModItem
    {
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(6, 5));
            ItemID.Sets.AnimatesAsSoul[Type] = true;

            ItemID.Sets.ItemNoGravity[Type] = true;
            Item.ResearchUnlockCount = 50;
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 28;
            Item.value = Item.sellPrice(0, 30);
            Item.rare = ModContent.RarityType<VioletRarity>();
            Item.maxStack = Item.CommonMaxStack;
            if (ModLoader.HasMod("CalamityMod"))
            {
                ModRarity r;
                Mod calamity = ModLoader.GetMod("CalamityMod");
                calamity.TryFind<ModRarity>("Violet", out r);
                Item.rare = r.Type;
            }
        }

        public override void PostUpdate()
        {
            if (Main.rand.NextBool(5))
            {
                Dust dark = Dust.NewDustPerfect(Item.Center + Main.rand.NextVector2Circular(18, 18), DustID.TintableDust, -Vector2.UnitY.RotatedByRandom(0.1f) * Main.rand.NextFloat(3f), 150, Color.Black, 1f + Main.rand.NextFloat());
                dark.noGravity = true;
            }            
            
            if (Main.rand.NextBool(20))
            {
                Particle spark = Particle.NewParticle(Particle.ParticleType<HueLightDust>(), Item.Center + Main.rand.NextVector2Circular(15, 15), -Vector2.UnitY * Main.rand.NextFloat(2f), Color.White, 0.7f + Main.rand.NextFloat());
                spark.data = Main.GlobalTimeWrappedHourly * 40f;
            }
        }

        public static Texture2D glowTexture;
        public static Texture2D auraTexture;

        public override void Load()
        {
            glowTexture = new TextureAsset(Texture + "Glow");
            auraTexture = new TextureAsset(Texture + "Aura");
        }

        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            float backScale = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 8f % MathHelper.TwoPi) * 0.1f;
            spriteBatch.Draw(auraTexture, position, auraTexture.Frame(), Color.Black * 0.3f, 1f, auraTexture.Size() * 0.5f, scale * backScale * 1.2f, 0, 0);

            return true;
        }

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
        {
            float backScale = 1f + (float)Math.Sin(Main.GlobalTimeWrappedHourly * 8f % MathHelper.TwoPi) * 0.1f;
            spriteBatch.Draw(auraTexture, Item.Center - Main.screenPosition, auraTexture.Frame(), Color.Black * 0.4f, 1f, auraTexture.Size() * 0.5f, scale * backScale * 1.2f, 0, 0);
            return true;
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Color color = new GradientColor(SlimeUtils.GoozOilColors, 0.2f, 0.2f).ValueAt(Main.GlobalTimeWrappedHourly * 40f);
            spriteBatch.Draw(glowTexture, position, frame, Color.White, 0, origin, scale, 0, 0);
            spriteBatch.Draw(glowTexture, position, frame, new Color(50, 50, 50, 0), 0, origin, scale, 0, 0);
            spriteBatch.Draw(auraTexture, position, auraTexture.Frame(), new Color(color.R, color.G, color.B, 0) * 0.1f, 0, auraTexture.Size() * 0.5f, scale, 0, 0);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Rectangle frame = Main.itemAnimations[Type].GetFrame(glowTexture, Main.itemFrameCounter[whoAmI]);
            Color color = new GradientColor(SlimeUtils.GoozOilColors, 0.2f, 0.2f).ValueAt(Main.GlobalTimeWrappedHourly * 40f);
            spriteBatch.Draw(glowTexture, Item.Center - Main.screenPosition, frame, Color.White, rotation, frame.Size() * 0.5f, scale, 0, 0);
            spriteBatch.Draw(glowTexture, Item.Center - Main.screenPosition, frame, new Color(50, 50, 50, 0), rotation, frame.Size() * 0.5f, scale, 0, 0);
            spriteBatch.Draw(auraTexture, Item.Center - Main.screenPosition, auraTexture.Frame(), new Color(color.R, color.G, color.B, 0) * 0.1f, rotation, auraTexture.Size() * 0.5f, scale, 0, 0);
        }
    }
}