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

        public static int NormalUseTime = 8;
        public static int FasterUseTime = 6;

        public override void SetDefaults()
        {
            Item.damage = 7;
            Item.knockBack = 1.5f;
            Item.useTime = NormalUseTime;
            Item.useAnimation = 18;
            Item.pick = 45;

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

        // The tool is mechanically faster when wet.
        public override float UseTimeMultiplier(Player player)
        {
            bool surface = player.Center.Y < Main.worldSurface * 16.0;
            bool GetEffects = ((Main.raining && surface) || player.dripping || (player.wet && !player.lavaWet && !player.honeyWet));
            if (GetEffects)
                return (float)FasterUseTime / NormalUseTime;
            return base.UseTimeMultiplier(player);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            bool surface = player.Center.Y < Main.worldSurface * 16.0;
            bool GetEffects = ((Main.raining && surface) || player.dripping || (player.wet && !player.lavaWet && !player.honeyWet));
            if (GetEffects)
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 160);
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
