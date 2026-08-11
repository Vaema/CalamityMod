using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged;

public class Shadethrower : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Ranged";
    public override void SetDefaults()
    {
        Item.width = 76;
        Item.height = 30;
        Item.damage = 21;
        Item.DamageType = DamageClass.Ranged;
        Item.useTime = 10;
        Item.useAnimation = 40;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 1.5f;
        Item.UseSound = SoundID.Item34;
        Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
        Item.rare = ItemRarityID.Orange;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<ShadeFire>();
        Item.shootSpeed = 8f;
        Item.useAmmo = AmmoID.Gel;
        Item.consumeAmmoOnFirstShotOnly = true;
    }

    public override Vector2? HoldoutOffset() => new Vector2(-5, 0);
}
