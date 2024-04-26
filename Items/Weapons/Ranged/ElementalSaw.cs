using CalamityMod.Cooldowns;
using CalamityMod.Items.Materials;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    [LegacyName("ElementalBlaster")]
    public class ElementalSaw : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";

        public const float ShootSpeed = 24f;

        public const int DashCooldown = 360;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DashCooldown / 60);

        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 104;
            Item.height = 42;
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
            Item.shoot = ModContent.ProjectileType<ElementalSawHoldout>();
            Item.shootSpeed = ShootSpeed;
            Item.Calamity().canFirePointBlankShots = true;
        }

        // Terraria seems to really dislike high crit values in SetDefaults
        public override void ModifyWeaponCrit(Player player, ref float crit) => crit += 21;
        public override bool AltFunctionUse(Player player) => true;
        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2 && player.HasCooldown(ElementalSawBoost.ID))
                return false;
            else
                return player.ownedProjectileCounts[Item.shoot] <= 0;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Right click dash
            if (player.altFunctionUse == 2)
            {
                if (!player.HasCooldown(ElementalSawBoost.ID))
                {
                    player.AddCooldown(ElementalSawBoost.ID, DashCooldown);
                    player.Calamity().sBlasterDashActivated = true;
                    SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/MeatySlash"), player.Center);

                    // Throws a lingering saw at the cursor
                    Vector2 mouseDist = Main.MouseWorld - player.Center;
                    float mouseLength = mouseDist.Length() / 368f;
                    if (mouseLength > 1.7f)
                        mouseLength = 1.7f;
                    Vector2 magnitude = new Vector2(mouseDist.X < 0 ? -1f : 1f, mouseDist.Y < 0 ? -1f : 1f); // Why do I need to do this why does magnitude get eaten shnsfqnowjqiuoquehmguo

                    mouseDist.Normalize();
                    mouseDist *= mouseLength;
                    mouseDist *= magnitude;

                    Projectile.NewProjectile(source, player.Center, velocity.SafeNormalize(Vector2.UnitY) * ShootSpeed * mouseDist, ModContent.ProjectileType<ElementalSawLingering>(), damage, knockback, Main.myPlayer);

                    // If moving, make particle effects when the dash activates
                    if (player.velocity != Vector2.Zero)
                    {
                        int particleAmt = 7;
                        for (int c = 0; c < particleAmt; c++)
                        {
                            Color sparkColor = Color.Lerp(new Color(122, 240, 58), new Color(32, 186, 171), c / (particleAmt - 1));
                            Particle spark = new CritSpark(player.Center, player.velocity.RotatedByRandom(MathHelper.ToRadians(13f)) * Main.rand.NextFloat(-2.1f, -4.5f), Color.White, sparkColor, 2f, 45, 2.25f, 2f);
                            GeneralParticleHandler.SpawnParticle(spark);
                        }
                        for (int e = 0; e < particleAmt; e++)
                        {
                            Color sparkColor2 = Color.Lerp(new Color(122, 240, 58), new Color(32, 186, 171), e / (particleAmt - 1));
                            Particle spark2 = new NanoParticle(player.Center, player.velocity.RotatedByRandom(MathHelper.ToRadians(-MathHelper.PiOver4)) * Main.rand.NextFloat(0.25f, 0.5f), sparkColor2, 1.5f, 45, Main.rand.NextBool(3));
                            GeneralParticleHandler.SpawnParticle(spark2);
                        }
                    }
                }
            }
            else
            {
                // The holdout deals 2x base damage.
                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<ElementalSawHoldout>(), (int)(damage * 2), knockback, player.whoAmI, 0f, 0f);
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
