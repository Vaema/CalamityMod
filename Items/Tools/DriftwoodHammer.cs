using CalamityMod.Items.Placeables;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Tools
{
    public class DriftwoodHammer : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Tools";
        public override void SetDefaults()
        {
            Item.damage = 13;
            Item.knockBack = 4f;
            Item.useTime = 12;
            Item.useAnimation = 31;
            Item.hammer = 45;

            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 42;
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
                Item.useAnimation = 21;
            }
            else
            {
                Item.useTime = 12;
                Item.useAnimation = 31;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Driftwood>(8).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
