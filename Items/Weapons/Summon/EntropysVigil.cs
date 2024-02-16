using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    [LegacyName("BlightedEyeStaff")]
    public class EntropysVigil : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";

        public override void SetStaticDefaults() => Item.staff[Type] = true;

        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 52;
            Item.damage = 47;
            Item.DamageType = DamageClass.Summon;
            Item.shoot = ModContent.ProjectileType<Calamitamini>();
            Item.knockBack = 2f;

            Item.useTime = Item.useAnimation = 25;
            Item.mana = 10;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.Rarity7BuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item82;

            // This doesn't do anything, it's just so the item is held like a staff.
            Item.shootSpeed = 1f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float randomAngleOffset = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi / 3f * i + randomAngleOffset;
                Vector2 spawnVelocity = angle.ToRotationVector2() * 5f;

                switch (i)
                {
                    case 0:
                        Projectile.NewProjectileDirect(source, Main.MouseWorld, spawnVelocity, ModContent.ProjectileType<Calamitamini>(), damage, knockback, player.whoAmI);
                        break;
                    case 1:
                        Projectile.NewProjectileDirect(source, Main.MouseWorld, spawnVelocity, ModContent.ProjectileType<Catastromini>(), damage, knockback, player.whoAmI);
                        break;
                    case 2:
                        Projectile.NewProjectileDirect(source, Main.MouseWorld, spawnVelocity, ModContent.ProjectileType<Cataclymini>(), damage, knockback, player.whoAmI);
                        break;
                }
            }

            return false;
        }
    }
}
