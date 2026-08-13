using CalamityMod.Items.Weapons.Summon;
using CalamityMod.Projectiles.Magic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic;

public class Downpour : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Magic";
    public override void SetStaticDefaults()
    {
        Item.staff[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 42;
        Item.height = 42;
        Item.damage = 50;
        Item.DamageType = DamageClass.Magic;
        Item.mana = 10;
        Item.useTime = 15;
        Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.noMelee = true;
        Item.knockBack = 3f;
        Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
        Item.rare = ItemRarityID.Pink;
        Item.UseSound = SoundID.Item13;
        Item.autoReuse = true;
        Item.shoot = ModContent.ProjectileType<Sandstream>();
        Item.shootSpeed = 14f;
    }
}
