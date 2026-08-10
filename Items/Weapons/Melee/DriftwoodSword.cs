using CalamityMod.Items.Placeables.FurnitureDriftwood;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee;

public class DriftwoodSword : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Melee";

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.ShadewoodSword); // Driftwood (base) = Shadewood
        Item.width = 42;
        Item.height = 46;
    }

    public override void MeleeEffects(Player player, Rectangle hitbox)
    {
        if (player.Calamity().countsAsAnyWet)
            Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.MagnetSphere);
    }

    public override float UseSpeedMultiplier(Player player) => player.Calamity().countsAsAnyWet ? 1.25f : 1f;

    public override void ModifyWeaponKnockback(Player player, ref StatModifier knockback) => knockback.Base += player.Calamity().countsAsAnyWet ? 1.25f : 0f;

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<Driftwood>(7).
            AddTile(TileID.WorkBenches).
            Register();
    }
}
