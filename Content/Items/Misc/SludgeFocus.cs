using CalamityHunt.Content.Items.Materials;
using CalamityHunt.Content.Particles;
using Microsoft.Xna.Framework;
using CalamityHunt.Common.Systems.Particles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityHunt.Content.Tiles;
using CalamityHunt.Content.Bosses.Goozma;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace CalamityHunt.Content.Items.Misc
{
    public class SludgeFocus : ModItem
    {
        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 26;
            Item.knockBack = 6;
            Item.value = 10000;
            Item.rare = ItemRarityID.Blue;
            Item.channel = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient<ChromaticMass>(5)
                .AddIngredient(ItemID.Gel, 100)
                .AddIngredient<GelatinousCatalyst>()
                .AddTile<SlimeNinjaStatueTile>()
                .Register();
        }

        public static Texture2D glowTexture;

        public override void Load()
        {
            glowTexture = new TextureAsset(Texture + "Glow");
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            spriteBatch.Draw(glowTexture, position, glowTexture.Frame(), new Color(255, 255, 255, 0), 0, glowTexture.Size() * 0.5f, scale, 0, 0);
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            spriteBatch.Draw(glowTexture, Item.Center - Main.screenPosition, glowTexture.Frame(), new Color(255, 255, 255, 0), rotation, glowTexture.Size() * 0.5f, scale, 0, 0);
        }
    }
}