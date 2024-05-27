using CalamityMod.Cooldowns;
using CalamityMod.Items.Materials;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    [LegacyName("ElementalBlaster")]
    public class SuperradiantSlaughterer : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";

        public const float ShootSpeed = 24f;

        public const int DashCooldown = 360;

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (tooltips == null)
                return;

            Player player = Main.player[Main.myPlayer];
            if (player is null)
                return;

            var mainTooltip = tooltips.FirstOrDefault(x => x.Text.Contains("[MAIN]") && x.Mod == "Terraria");
            if (mainTooltip != null)
            {
                mainTooltip.Text = Lang.SupportGlyphs(this.GetLocalizedValue("MainInfo"));
                mainTooltip.OverrideColor = Color.Chartreuse;
            }
            var altTooltip = tooltips.FirstOrDefault(x => x.Text.Contains("[ALT]") && x.Mod == "Terraria");
            if (altTooltip != null)
            {
                altTooltip.Text = Lang.SupportGlyphs(this.GetLocalization("AltInfo").Format(DashCooldown / 60));
                altTooltip.OverrideColor = Color.SpringGreen;
            }
        }

        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 84;
            Item.height = 46;
            Item.damage = 67;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.knockBack = 1.75f;
            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.rare = ItemRarityID.Purple;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<SuperradiantSlaughtererHoldout>();
            Item.shootSpeed = ShootSpeed;
            Item.Calamity().canFirePointBlankShots = true;
        }

        // Terraria seems to really dislike high crit values in SetDefaults
        public override void ModifyWeaponCrit(Player player, ref float crit) => crit += 21;
        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2 && player.HasCooldown(SuperradiantSawBoost.ID))
                return false;
            else
                return player.ownedProjectileCounts[Item.shoot] <= 0;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Right click dash
            if (player.altFunctionUse == 2)
            {
                if (!player.HasCooldown(SuperradiantSawBoost.ID))
                {
                    // Throws a lingering saw at the cursor
                    float mouseDist = Vector2.Distance(player.Center, Main.MouseWorld) / 21f;
                    Projectile.NewProjectile(source, player.Center, velocity.SafeNormalize(Vector2.UnitY) * mouseDist, ModContent.ProjectileType<SuperradiantSawLingering>(), damage, knockback, Main.myPlayer);

                    player.AddCooldown(SuperradiantSawBoost.ID, DashCooldown);
                    player.Calamity().sBlasterDashActivated = true;
                    SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/MeatySlash"), player.Center);

                    // If moving, make particle effects when the dash activates
                    if (player.velocity != Vector2.Zero)
                    {
                        for (int c = 0; c < 9; c++)
                        {
                            Vector2 sparkVel = player.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(MathHelper.ToRadians(45f)) * Main.rand.NextFloat(-28f, -36f);
                            Color sparkColor = Color.Lerp(new Color(150, 255, 60), new Color(60, 255, 220), c / 8f);
                            Particle spark = new CritSpark(player.Center, sparkVel, Color.White, sparkColor, 1.5f, 45, 0.5f, 2f);
                            GeneralParticleHandler.SpawnParticle(spark);
                        }
                        for (int e = 0; e < 7; e++)
                        {
                            Vector2 sparkVel2 = player.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(MathHelper.ToRadians(36f)) * Main.rand.NextFloat(-4f, -6f);
                            Color sparkColor2 = Color.Lerp(new Color(150, 255, 60), new Color(60, 255, 220), e / 6f);
                            Particle spark2 = new NanoParticle(player.Center, sparkVel2, sparkColor2, 1.5f, 45, Main.rand.NextBool(3));
                            GeneralParticleHandler.SpawnParticle(spark2);
                        }
                    }
                }
            }
            else
            {
                // The holdout deals 2x base damage.
                Projectile.NewProjectile(source, position, velocity, Item.shoot, damage * 2, knockback, player.whoAmI);
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Buzzkill>().
                AddIngredient<SpeedBlaster>().
                AddIngredient(ItemID.LunarBar, 5).
                AddIngredient<LifeAlloy>(5).
                AddIngredient<GalacticaSingularity>(5).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
