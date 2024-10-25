using CalamityMod.Dusts;
using CalamityMod.Items.Placeables.Astral;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Tools
{
    public class MonolithPickaxe : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Tools";
        public override void SetDefaults()
        {
            Item.damage = 19;
            Item.knockBack = 0.8f;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.pick = 90;
            Item.tileBoost = 1;

            Item.DamageType = DamageClass.Melee;
            Item.width = 38;
            Item.height = 40;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(3))
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, Main.rand.NextBool() ? ModContent.DustType<AstralOrange>() : ModContent.DustType<AstralBlue>());
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AstralMonolith>(15).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
