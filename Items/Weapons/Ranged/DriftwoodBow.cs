using CalamityMod.Items.Placeables.FurnitureDriftwood;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged;

public class DriftwoodBow : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Ranged";

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.ShadewoodBow); // Driftwood (base) = Shadewood
        Item.width = 22;
        Item.height = 42;
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
    {
        if (player.Calamity().countsAsAnyWet)
        {
            velocity *= 1.2f;
            knockback += 1f;

            for (int i = 0; i <= 18; i++)
            {
                Dust dust = Dust.NewDustPerfect(position + velocity * 3f, DustID.MagnetSphere, velocity.RotatedByRandom(MathHelper.ToRadians(19f)) * Main.rand.NextFloat(0.8f, 3.8f), Scale: Main.rand.NextFloat(1.2f, 1.6f));
                dust.noGravity = true;
            }
        }
    }

    public override float UseSpeedMultiplier(Player player) => player.Calamity().countsAsAnyWet ? 1.2f : 1f;

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<Driftwood>(10).
            AddTile(TileID.WorkBenches).
            Register();
    }
}
