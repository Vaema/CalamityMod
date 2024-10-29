using CalamityMod.Items.Placeables.SunkenSea;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    public class DriftwoodSword : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.damage = 14;
            Item.width = 42;
            Item.height = 46;
            Item.DamageType = DamageClass.Melee;
            Item.useAnimation = Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 4f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.value = CalamityGlobalItem.RarityWhiteBuyPrice;
            Item.rare = ItemRarityID.White;
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            bool surface = player.Center.Y < Main.worldSurface * 16.0;
            bool GetEffects = ((Main.raining && surface) || player.dripping || (player.wet && !player.lavaWet && !player.honeyWet));
            if (GetEffects)
            {
                int dust = Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 160);
                Item.useTime = 12;
                Item.useAnimation = 12;
                Item.knockBack = 5.5f;
            }
            else
            {
                Item.useTime = 20;
                Item.useAnimation = 20;
                Item.knockBack = 4f;
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Driftwood>(7).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
