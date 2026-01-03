using System;
using CalamityMod.Buffs.Summon;
using CalamityMod.Dusts;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    [LegacyName("SeashineSword")]
    public class SeashineHilt : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 34;
            Item.useTime = Item.useAnimation = 36;
            Item.damage = 55;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Summon;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.knockBack = 5f;
            Item.shootSpeed = 12f;
            Item.mana = 10;
            Item.rare = ItemRarityID.Green;
            Item.buffType = ModContent.BuffType<SeashineSwordBuff>();
            Item.shoot = ModContent.ProjectileType<SeashineSwordProj>();
        }
        public override bool AltFunctionUse(Player player) => true;
        public override float UseSpeedMultiplier(Player player)
        {
            // There's a long cooldown on commanding blades
            // This is so you don't just hold down right click, since now it's optimal to only command blades when all are ready
            if (player.altFunctionUse == 2)
                return 0.1f;
            else
                return 1f;
        }
        public override void HoldItem(Player player)
        {
            player.Calamity().mouseWorldListener = true;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Call all active blades to attack if they are charged
            // The check to see if a blade is charged is done in the blade itself
            if (player.altFunctionUse == 2)
            {
                int blades = 0;
                for (int x = 0; x < Main.maxProjectiles; x++)
                {
                    Projectile projectile = Main.projectile[x];
                    if (projectile.active && projectile.type == Item.shoot && projectile.ai[0] < 5 && projectile.timeLeft < 90000 - 180)
                    {
                        projectile.ai[0] = 5;
                        blades++;
                    }
                }
                if (blades > 0) // If theres no blades, then don't do anything
                {
                    for (int i = 0; i < 7; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(player.Center, ModContent.DustType<LightDust>(), (velocity.SafeNormalize(Vector2.UnitX)).RotatedByRandom(0.6f) * Main.rand.NextFloat(5f, 10f));
                        dust.noGravity = false;
                        dust.scale = Main.rand.NextFloat(0.95f, 1.7f);
                        dust.color = Main.rand.NextBool() ? Color.Cyan : Color.DodgerBlue;
                        dust.noLightEmittence = true;

                        Particle fx = new CustomSpark(player.Center, (velocity.SafeNormalize(Vector2.UnitX)).RotatedByRandom(0.6f) * Main.rand.NextFloat(10f, 20f), "CalamityMod/Particles/Sparkle", false, (int)(Main.rand.Next(16, 26 + 1)), Main.rand.NextFloat(1.1f, 1.7f), Main.rand.NextBool() ? Color.Cyan : Color.DodgerBlue, new Vector2(0.5f, 1.1f), extraRotation: 0, shrinkSpeed: 0.2f);
                        GeneralParticleHandler.SpawnParticle(fx);
                    }
                    Particle e = new CustomSpark(player.Center, (velocity.SafeNormalize(Vector2.UnitX)).RotatedByRandom(0.6f) * Main.rand.NextFloat(7f, 20f), "CalamityMod/Particles/WaterFoam", false, 14, Main.rand.NextFloat(0.15f, 0.25f) * 3, Main.rand.NextBool() ? Color.Cyan : Color.DodgerBlue, new Vector2(1f, 1f), true, false, shrinkSpeed: 0.4f);
                    GeneralParticleHandler.SpawnParticle(e);
                    SoundEngine.PlaySound(SoundID.Item101 with { Volume = 0.9f, Pitch = Main.rand.NextFloat(0.3f, 0.5f) }, player.Center);
                }
            }
            else // Toss out the summon
            {
                player.AddBuff(Item.buffType, 2);
                SoundEngine.PlaySound(SoundID.Item1, player.Center);

                var minion = Projectile.NewProjectileDirect(source, player.Center, velocity, type, damage, knockback, player.whoAmI);
                minion.originalDamage = Item.damage;

                float angleMax = MathHelper.ToRadians(360f);
                if (CalamityUtils.CountProjectiles(type) == 1)
                    angleMax = 0f;
                float index = 1f;
                if (player.ownedProjectileCounts[Item.shoot] > 30)
                {
                    angleMax += MathHelper.ToRadians((player.ownedProjectileCounts[Item.shoot] - 30) * 2.5f);
                }
                angleMax = angleMax > MathHelper.ToRadians(360f) ? MathHelper.ToRadians(360f) : angleMax; // More intuative than using a min function
                foreach (Projectile p in Main.ActiveProjectiles)
                {
                    if (p.type == type && p.owner == player.whoAmI)
                    {
                        p.ai[2] = (index / CalamityUtils.CountProjectiles(type)) * angleMax - angleMax / 2f;
                        p.ai[1] = index;
                        p.netUpdate = true;
                        index++;
                    }
                }
            }
            return false;
        }
        public override void UseItemFrame(Player player) // Player hand animation when you command the blades
        {
            if (player.altFunctionUse == 2)
            {
                player.ChangeDir(Math.Sign((player.Calamity().mouseWorld - player.Center).X));

                float animProgress = 0.5f - player.itemTime / (float)player.itemTimeMax;
                float rotation = (player.Center - player.Calamity().mouseWorld).ToRotation() * player.gravDir + MathHelper.PiOver2;
                if (animProgress < 0.4f)
                    rotation += (-0.15f) * (float)Math.Pow((0.6f - animProgress) / 0.6f, 2) * player.direction;

                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation);
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PearlShard>(3).
                AddIngredient<SeaPrism>(7).
                AddIngredient<Navystone>(10).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
