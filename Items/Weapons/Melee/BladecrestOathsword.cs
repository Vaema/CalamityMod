using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee;

public class BladecrestOathsword : ModItem, ILocalizedModType, IHoldShiftTooltipItem
{
    public new string LocalizationCategory => "Items.Weapons.Melee";
    public int throwCount = 0;
    public override void SetDefaults()
    {
        Item.width = 56;
        Item.height = 56;

        Item.damage = 35;
        Item.knockBack = 3f;
        Item.useAnimation = Item.useTime = 53;
        Item.shoot = ModContent.ProjectileType<BladecrestOathswordThrownBlade>();
        Item.shootSpeed = 6f;

        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.DamageType = DamageClass.Melee;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.UseSound = null;
        Item.autoReuse = true;

        Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
        Item.rare = ItemRarityID.Orange;
    }
    public override void HoldItem(Player player) => player.Calamity().mouseWorldListener = true;
    public override bool MeleePrefix() => true;
    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        throwCount++;
        Projectile.NewProjectileDirect(source, player.MountedCenter, velocity, type, damage, knockback, player.whoAmI, 0, throwCount);
        return false;
    }
}
