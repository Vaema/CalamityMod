using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Typeless;

public class LunicEye : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Typeless";

    public static readonly SoundStyle UseSound = new("CalamityMod/Sounds/Item/LunicShot", 2) { Volume = 0.8f, PitchVariance = 0.1f };
    public static readonly SoundStyle ImpactSound = new("CalamityMod/Sounds/Item/LunicImpact") { PitchVariance = 0.1f };

    public override void SetDefaults()
    {
        Item.width = 60;
        Item.height = 36;
        Item.damage = 32;
        Item.DamageType = AverageDamageClass.Instance;
        Item.useAnimation = Item.useTime = 22;
        Item.knockBack = 4.5f;
        Item.shoot = ModContent.ProjectileType<LunicBeam>();
        Item.shootSpeed = 12f;

        Item.useStyle = ItemUseStyleID.Shoot;
        Item.UseSound = UseSound;
        Item.autoReuse = true;
        Item.noMelee = true;

        Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
        Item.rare = ItemRarityID.LightRed;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        itemGroup = (ContentSamples.CreativeHelper.ItemGroup)CalamityResearchSorting.ClasslessWeapon;
    }

    public override Vector2? HoldoutOffset() => new Vector2(-5, 0);

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) => position += velocity.SafeNormalize(Vector2.Zero) * 48f;

    public override void AddRecipes()
    {
        CreateRecipe().
            AddRecipeGroup("AnyCobaltBar", 10).
            AddIngredient<StarblightSoot>(20).
            AddTile(TileID.Anvils).
            Register();
    }
}
