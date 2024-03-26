using CalamityMod.Projectiles.Ranged;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Ammo
{
    [LegacyName("IcyBullet")]
    public class HailstormBullet : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Ammo";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 99;
        }
        public override void SetDefaults()
        {
            Item.width = 14;
            Item.height = 20;
            Item.damage = 12;
            Item.DamageType = DamageClass.Ranged;
            Item.consumable = true;
            Item.knockBack = 2f;
            Item.value = Item.buyPrice(0, 0, 0, 80);
            Item.rare = ItemRarityID.Yellow;
            Item.shoot = ModContent.ProjectileType<HailstormBulletProj>();
            Item.shootSpeed = 0.3f;
            Item.ammo = AmmoID.Bullet;
            Item.maxStack = 9999;
        }
    }
}
