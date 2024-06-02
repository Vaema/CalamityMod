using CalamityMod.Items.Placeables;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Tools
{
    public class DriftwoodAxe : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Tools";
        public override void SetDefaults()
        {
            Item.damage = 9;
            Item.knockBack = 3.5f;
            Item.useTime = 13;
            Item.useAnimation = 30;
            Item.axe = 60 / 5;

            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 34;
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
                Item.useAnimation = 20;
            }
            else
            {
                Item.useTime = 13;
                Item.useAnimation = 30;
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Driftwood>(10).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
