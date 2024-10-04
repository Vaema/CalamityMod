using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Materials;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    [LegacyName("GalacticaBlade")]
    public class GalactusBlade : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        private int swordDirection;
        public int time = 0;
        private float swingRotation = 0;
        public Color useColor = Color.White;

        public override void SetDefaults()
        {
            Item.width = 144;
            Item.height = 146;
            Item.damage = 184;
            Item.DamageType = DamageClass.Melee;
            Item.useAnimation = 44;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 44;
            Item.useTurn = true;
            Item.knockBack = 14f;
            Item.UseSound = SoundID.Item105;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.shoot = ModContent.ProjectileType<GalacticaComet>();
            Item.shootSpeed = 23f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            for (int i = 0; i < 15; i++)
            {
                Vector2 spawnSpot = new Vector2(player.Calamity().mouseWorld.X, player.Center.Y) + new Vector2(Main.rand.NextFloat(-850, 850), Main.rand.NextFloat(-750, -1250));
                Projectile.NewProjectileDirect(source, spawnSpot, Utils.DirectionTo(spawnSpot, player.Calamity().mouseWorld + Main.rand.NextVector2Circular(90, 90)) * Item.shootSpeed, type, damage / 3, knockback, player.whoAmI);
            }
            return false;
        }
        public override void UseAnimation(Player player)
        {
            swordDirection = (player.Center - player.Calamity().mouseWorld).X > 1 ? -1 : 1;
            time = 0;
            swingRotation = 0;
        }
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            useColor = Main.rand.Next(3) switch
            {
                0 => Color.Yellow,
                1 => Color.Pink,
                _ => Color.Cyan,
            };

            player.itemRotation = swingRotation - 1.7f * swordDirection;
            player.itemLocation = player.Center;
            player.direction = swordDirection;

            float val = MathF.Abs(time - player.itemAnimationMax * 0.75f) / player.itemAnimationMax;

            float goalRot = Utils.Remap(time, 0, player.itemAnimationMax, -0.5f, 5.2f) * swordDirection;
            float swingEasing = Utils.GetLerpValue(0, player.itemAnimationMax * 0.4f, time, true) * (0.35f - val);
            if (time < player.itemAnimationMax)
            {
                swingRotation = MathHelper.Lerp(swingRotation, goalRot, swingEasing);
            }

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, swingRotation + MathHelper.ToRadians(120f * swordDirection));

            if (Main.rand.NextBool())
            {
                Vector2 dustVel = new Vector2(5 * swordDirection, -5).RotatedByRandom(1.55f) * Main.rand.NextFloat(0.7f, 1.3f) * 2;
                Dust dust = Dust.NewDustPerfect(player.Center + dustVel * 9, 66);
                dust.scale = Main.rand.NextFloat(0.5f, 0.75f);
                dust.velocity = dustVel * 0.85f;
                dust.color = useColor;
                dust.noGravity = true;
            }
            if (Main.rand.NextBool())
            {
                Vector2 dustVel = new Vector2(5 * swordDirection, -5).RotatedBy(swingRotation - 1.7f * swordDirection);

                float partScale = Main.rand.NextFloat(0.6f, 0.9f);
                Vector2 partVel = (dustVel * Main.rand.NextFloat(0.2f, 0.3f)).RotatedBy(MathHelper.ToRadians(90f * swordDirection)).RotatedByRandom(-0.4) * -3;
                Vector2 partPos = player.Center + dustVel * 25 + Main.rand.NextVector2Circular(12, 12);

                Particle spark3 = new AltSparkParticle(partPos, partVel, false, 24, partScale, useColor);
                GeneralParticleHandler.SpawnParticle(spark3);
                Particle spark2 = new SparkParticle(partPos, partVel, false, 24, partScale * 0.6f, useColor);
                GeneralParticleHandler.SpawnParticle(spark2);
            }
            Vector2 dustVel2 = new Vector2(5 * swordDirection, -5).RotatedBy(swingRotation - 1.7f * swordDirection);

            float partScale2 = Main.rand.NextFloat(0.3f, 0.7f);
            Vector2 partVel2 = dustVel2 * Main.rand.NextFloat(0.2f, 0.3f);
            for (int i = 0; i < 3; i++)
            {
                Particle smoke = new HeavySmokeParticle(player.Center + dustVel2 * Main.rand.Next(1, 35 + 1) + Main.rand.NextVector2Circular(12, 12), partVel2.RotatedBy(MathHelper.ToRadians(90f * swordDirection)).RotatedBy(-0.3 * swordDirection) * Main.rand.NextFloat(-3, -20), useColor, 19, partScale2, 0.7f, Main.rand.NextFloat(-0.2f, 0.2f), true);
                GeneralParticleHandler.SpawnParticle(smoke);
            }
            Lighting.AddLight(player.Center + dustVel2, Color.White.ToVector3() * 1.2f);
            time++;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.StarWrath).
                AddIngredient<DivineGeode>(10).
                AddIngredient<GalacticaSingularity>(5).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
