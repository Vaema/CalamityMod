using CalamityMod.Items.Placeables;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Tools
{
    public class DriftwoodPickaxe : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Tools";
        public override void SetDefaults()
        {
            Item.damage = 7;
            Item.knockBack = 1.5f;
            Item.useTime = 9;
            Item.useAnimation = 18;
            Item.pick = 55;

            Item.DamageType = DamageClass.Melee;
            Item.width = 38;
            Item.height = 38;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = CalamityGlobalItem.RarityWhiteBuyPrice;
            Item.rare = ItemRarityID.White;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            bool surface = player.Center.Y < Main.worldSurface * 16.0;
            bool GetEffects = ((Main.raining && surface) || player.dripping || (player.wet && !player.lavaWet && !player.honeyWet));
            if (GetEffects)
            {
                int dust = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 160);
                Item.useTime = 7;
                Item.useAnimation = 14;
            }
            else
            {
                Item.useTime = 9;
                Item.useAnimation = 18;
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Driftwood>(15).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
