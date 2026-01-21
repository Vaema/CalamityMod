using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Typeless
{
    public class EyeofMagnus : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Typeless";
        public static readonly SoundStyle ImpactSound = new("CalamityMod/Sounds/Item/MagnusImpact") { PitchVariance = 0.1f };

        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 50;
            Item.damage = 60;
            Item.DamageType = AverageDamageClass.Instance;
            Item.useAnimation = Item.useTime = 22;
            Item.knockBack = 5f;
            Item.shoot = ModContent.ProjectileType<MagnusBeam>();
            Item.shootSpeed = 12f;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = LunicEye.UseSound;
            Item.autoReuse = true;
            Item.noMelee = true;

            Item.value = CalamityGlobalItem.RarityRedBuyPrice;
            Item.rare = ItemRarityID.Red;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = (ContentSamples.CreativeHelper.ItemGroup)CalamityResearchSorting.ClasslessWeapon;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-15, 0);

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) => position += velocity.SafeNormalize(Vector2.Zero) * 44f;

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<LunicEye>().
                AddIngredient(ItemID.FragmentNebula, 12).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
