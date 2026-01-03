using CalamityMod.Items.Placeables.FurnitureDriftwood;
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
            if (IsPlayerInContactWithWater(player))
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.MagnetSphere);
        }

        public override float UseSpeedMultiplier(Player player) => IsPlayerInContactWithWater(player) ? 1.66f : 1f;

        public override void ModifyWeaponKnockback(Player player, ref StatModifier knockback) => knockback.Base += IsPlayerInContactWithWater(player) ? 1.5f : 0f;

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Driftwood>(7).
                AddTile(TileID.WorkBenches).
                Register();
        }

        private static bool IsPlayerInContactWithWater(Player player)
        {
            bool surface = player.Center.Y < Main.worldSurface * 16.0;
            return (Main.raining && surface) || player.dripping || (player.wet && !player.lavaWet && !player.honeyWet);
        }
    }
}
