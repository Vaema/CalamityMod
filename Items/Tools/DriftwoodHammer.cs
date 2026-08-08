using CalamityMod.Items.Placeables.FurnitureDriftwood;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Tools
{
    public class DriftwoodHammer : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Tools";

        public static int NormalUseTime = 9; // Equals Shadewood
        public static int FasterUseTime = 7;

        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.ShadewoodHammer); // Driftwood (base) = Shadewood
            Item.width = 40;
            Item.height = 42;
            Item.useTime = NormalUseTime;
        }

        // The tool is mechanically faster when wet.
        public override float UseTimeMultiplier(Player player)
        {
            if (player.Calamity().countsAsAnyWet)
                return (float)FasterUseTime / NormalUseTime;
            return base.UseTimeMultiplier(player);
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (player.Calamity().countsAsAnyWet)
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.MagnetSphere);
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
