using System;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    public class Greentide : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";

        internal const float ShootSpeed = 32f;

        internal const float TeethSpread = 960f;

        internal const float HalvedTeethSpread = TeethSpread * 0.5f;

        internal const int TotalRows = 2;

        internal const int TotalTeeth = 4;
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Leviatitan>();
        }
        public override void SetDefaults()
        {
            Item.width = 62;
            Item.height = 62;
            Item.damage = 97;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 27;
            Item.useAnimation = 27;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7f;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<GreenWater>();
            Item.autoReuse = true;
            Item.shootSpeed = 10;
        }
        public override void HoldItem(Player player)
        {
            player.Calamity().mouseWorldListener = true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 2; ++i)
            {
                Projectile tooth = Projectile.NewProjectileDirect(source, position, velocity.RotatedBy(-0.1f * (i + 1)).RotatedByRandom(0.06f) * (1 - i * 0.3f), type, (int)(damage * 0.5f), knockback / 3, player.whoAmI, 1f, 0f);
            }
            for (int i = 0; i < 2; ++i)
            {
                Projectile tooth = Projectile.NewProjectileDirect(source, position, velocity.RotatedBy(0.1f * (i + 1)).RotatedByRandom(0.06f) * (1 - i * 0.3f), type, (int)(damage * 0.5f), knockback / 3, player.whoAmI, 1f, 0f);
            }
            Projectile strongTooth = Projectile.NewProjectileDirect(source, position, velocity * 1.2f, type, (int)(damage), knockback, player.whoAmI, 2f, 0f);
            strongTooth.penetrate = -1;
            strongTooth.timeLeft = 150;
            return false;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            // 14NOV2024: Ozzatron: clamped mouse position unnecessary, only used for aiming a projectile
            NPC target2 = player.Calamity().mouseWorld.ClosestNPCAt(300);
            if (target2 == null)
                target2 = target;

            Vector2 destination = target.Center;

            Vector2 position = new Vector2(target2.Center.X, target2.Center.Y - 350);
            Vector2 cachedPosition = position;
            Vector2 secondPosition = new Vector2(target2.Center.X, target2.Center.Y + 350);
            Vector2 secondCachedPosition = secondPosition;

            Vector2 velocity = (destination - position).SafeNormalize(Vector2.UnitY) * ShootSpeed;
            Vector2 cachedVelocity = velocity;
            Vector2 secondVelocity = (destination - secondPosition).SafeNormalize(Vector2.UnitY) * ShootSpeed;
            Vector2 secondCachedVelocity = secondVelocity;

            int teethDamage = player.CalcIntDamage<MeleeDamageClass>((int)(Item.damage * 0.1f));
            float teethKnockback = Item.knockBack * 0.2f;
            bool evenNumberOfProjectiles = TotalTeeth % 2 == 0;
            float amountToAdd = evenNumberOfProjectiles ? 0.5f : 0f;
            int centralProjectile = TotalTeeth / 2;
            int otherCentralProjectile = centralProjectile - 1;
            float teethXVelocityReduction = 0.9f;
            float minVelocityAdjustment = 0.8f;
            float maxVelocityAdjustment = 1f;
            float velocityAdjustment = minVelocityAdjustment;
            for (int i = 0; i < TotalRows; i++)
            {
                bool topTeeth = i == 0;
                for (int j = 0; j < TotalTeeth; j++)
                {
                    velocityAdjustment = ((j == centralProjectile || j == otherCentralProjectile) && evenNumberOfProjectiles) ? minVelocityAdjustment : MathHelper.Lerp(minVelocityAdjustment, maxVelocityAdjustment, Math.Abs((j + amountToAdd) - centralProjectile) / (float)centralProjectile);
                    if (topTeeth)
                    {
                        position.X += MathHelper.Lerp(-HalvedTeethSpread * 0.3f, HalvedTeethSpread * 0.3f, j / (float)(TotalTeeth - 1));
                        velocity = CalamityUtils.CalculatePredictiveAimToTargetMaxUpdates(position, target2, ShootSpeed, 1) * velocityAdjustment;
                        velocity.X *= teethXVelocityReduction;
                        Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, velocity * 0.25f, ModContent.ProjectileType<GreenWater>(), teethDamage, teethKnockback, player.whoAmI, 0f, i, target2.Center.Y);
                        position = cachedPosition;
                        velocity = cachedVelocity;
                    }
                    else
                    {
                        secondPosition.X += MathHelper.Lerp(-HalvedTeethSpread * 0.3f, HalvedTeethSpread * 0.3f, j / (float)(TotalTeeth - 1));
                        secondVelocity = CalamityUtils.CalculatePredictiveAimToTargetMaxUpdates(secondPosition, target2, ShootSpeed, 1) * velocityAdjustment;
                        secondVelocity.X *= teethXVelocityReduction;
                        Projectile.NewProjectile(player.GetSource_ItemUse(Item), secondPosition, secondVelocity * 0.25f, ModContent.ProjectileType<GreenWater>(), teethDamage, teethKnockback, player.whoAmI, 0f, i, target2.Center.Y);
                        secondPosition = secondCachedPosition;
                        secondVelocity = secondCachedVelocity;
                    }
                }
            }
        }

        public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo)
        {
            Vector2 destination = target.Center;

            Vector2 position = destination - (Vector2.UnitY * (destination.Y - Main.screenPosition.Y + 80f));
            Vector2 cachedPosition = position;
            Vector2 secondPosition = cachedPosition + (Vector2.UnitY * (Main.screenHeight + 160f));
            Vector2 secondCachedPosition = secondPosition;

            Vector2 velocity = (destination - position).SafeNormalize(Vector2.UnitY) * ShootSpeed;
            Vector2 cachedVelocity = velocity;
            Vector2 secondVelocity = (destination - secondPosition).SafeNormalize(Vector2.UnitY) * ShootSpeed;
            Vector2 secondCachedVelocity = secondVelocity;

            int teethDamage = player.CalcIntDamage<MeleeDamageClass>((int)(Item.damage * 0.5));
            float teethKnockback = Item.knockBack * 0.2f;
            bool evenNumberOfProjectiles = TotalTeeth % 2 == 0;
            float amountToAdd = evenNumberOfProjectiles ? 0.5f : 0f;
            int centralProjectile = TotalTeeth / 2;
            int otherCentralProjectile = centralProjectile - 1;
            float teethXVelocityReduction = 0.9f;
            float minVelocityAdjustment = 0.8f;
            float maxVelocityAdjustment = 1f;
            float velocityAdjustment = minVelocityAdjustment;
            for (int i = 0; i < TotalRows; i++)
            {
                bool topTeeth = i == 0;
                for (int j = 0; j < TotalTeeth; j++)
                {
                    velocityAdjustment = ((j == centralProjectile || j == otherCentralProjectile) && evenNumberOfProjectiles) ? minVelocityAdjustment : MathHelper.Lerp(minVelocityAdjustment, maxVelocityAdjustment, Math.Abs((j + amountToAdd) - centralProjectile) / (float)centralProjectile);
                    if (topTeeth)
                    {
                        position.X += MathHelper.Lerp(-HalvedTeethSpread, HalvedTeethSpread, j / (float)(TotalTeeth - 1));
                        velocity = CalamityUtils.CalculatePredictiveAimToTargetMaxUpdates(position, target, ShootSpeed, 1) * velocityAdjustment;
                        velocity.X *= teethXVelocityReduction;
                        Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, velocity, ModContent.ProjectileType<GreenWater>(), teethDamage, teethKnockback, player.whoAmI, 0f, i, target.Center.Y);
                        position = cachedPosition;
                        velocity = cachedVelocity;
                    }
                    else
                    {
                        secondPosition.X += MathHelper.Lerp(-HalvedTeethSpread, HalvedTeethSpread, j / (float)(TotalTeeth - 1));
                        secondVelocity = CalamityUtils.CalculatePredictiveAimToTargetMaxUpdates(secondPosition, target, ShootSpeed, 1) * velocityAdjustment;
                        secondVelocity.X *= teethXVelocityReduction;
                        Projectile.NewProjectile(player.GetSource_ItemUse(Item), secondPosition, secondVelocity, ModContent.ProjectileType<GreenWater>(), teethDamage, teethKnockback, player.whoAmI, 0f, i, target.Center.Y);
                        secondPosition = secondCachedPosition;
                        secondVelocity = secondCachedVelocity;
                    }
                }
            }
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            Vector2 dustVel = Vector2.One.RotatedByRandom(100) * Main.rand.NextFloat(0.9f, 1.5f);
            if (Main.rand.NextBool(3))
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, 102, dustVel.X, dustVel.Y);
        }
    }
}
