using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class ChickenCannon : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<DragonsBreath>();
        }
        public override void SetDefaults()
        {
            Item.width = 126;
            Item.height = 42;
            Item.damage = 277;
            Item.DamageType = DamageClass.Ranged;
            Item.useAnimation = Item.useTime = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 1.5f;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<BurnishedAuric>();
            // No use sound is intentional
            Item.autoReuse = true;
            Item.shootSpeed = 14.5f;
            Item.shoot = ModContent.ProjectileType<ChickenCannonHeld>();
            Item.useAmmo = AmmoID.Rocket;
            Item.channel = true;
            Item.noUseGraphic = true;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-20, 0);

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, Item.shoot, 0, 0f, player.whoAmI, 1);
            return false;
        }
    }
}
