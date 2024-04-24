using System;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using Terraria;
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

        internal const int TotalTeeth = 5;

        public override void SetDefaults()
        {
            Item.width = 62;
            Item.height = 62;
            Item.damage = 87; // IS THAT THE BITE OF '87
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 31;
            Item.useAnimation = 31;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 7f;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shootSpeed = ShootSpeed;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
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
            if (Main.rand.NextBool(4))
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, Main.rand.NextBool() ? 33 : 89);
        }
    }
}
