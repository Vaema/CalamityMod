using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    [LegacyName("Starfall")]
    public class StarShower : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        internal const float ShootSpeed = 28f;

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 40;
            Item.damage = 60;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 15;
            Item.rare = ItemRarityID.Cyan;
            Item.useTime = 14;
            Item.useAnimation = 14;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3.25f;
            Item.value = CalamityGlobalItem.Rarity9BuyPrice;
            Item.UseSound = SoundID.Item105;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<AstralStarMagic>();
            Item.shootSpeed = ShootSpeed;
        }

        // Terraria seems to really dislike high crit values in SetDefaults
        public override void ModifyWeaponCrit(Player player, ref float crit) => crit += 25;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 destination = Main.MouseWorld;
            position = destination - Vector2.UnitY * (Main.MouseWorld.Y - Main.screenPosition.Y + 80f);
            Vector2 cachedPosition = position;
            float maxRandomOffset = 16f;
            int totalProjectiles = 5;
            for (int i = 0; i < totalProjectiles; i++)
            {
                position.X += MathHelper.Lerp(-160f, 160f, i / (float)(totalProjectiles - 1));
                position += Main.rand.NextVector2Circular(maxRandomOffset, maxRandomOffset);
                velocity = (Main.MouseWorld - position).SafeNormalize(Vector2.UnitY) * ShootSpeed * Main.rand.NextFloat(0.9f, 1.1f);
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                position = cachedPosition;
            }
            return false;
        }
    }
}
