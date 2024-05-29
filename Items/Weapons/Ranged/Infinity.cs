using System;
using CalamityMod.Items.Materials;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class Infinity : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        internal int rotation = 0;
        internal bool limit = true;
        public int SineCounter = 0;
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 56;
            Item.height = 24;
            Item.damage = 90;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 1;
            Item.useAnimation = 4;
            Item.useLimitPerAnimation = 9;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 1f;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<DarkBlue>();
            Item.UseSound = SoundID.Item31;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.shootSpeed = 6f;
            Item.useAmmo = AmmoID.Bullet;
            Item.Calamity().canFirePointBlankShots = true;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-5, 0);
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            position = position + (player.Calamity().mouseWorld - player.MountedCenter).SafeNormalize(Vector2.UnitX) * 65;
            float sine = (float)Math.Sin(SineCounter * 0.175f / MathHelper.Pi) * 3.5f;
            SineCounter++;
            if (SineCounter % 4 == 0)
            {
                if (player.altFunctionUse == 2)
                {
                    //If you right click, shoots a helix of normal bullets
                    Vector2 helixVel1 = (velocity * Main.rand.NextFloat(0.9f, 1.1f)).RotatedBy(MathHelper.ToRadians(sine));
                    Vector2 helixVel2 = (velocity * Main.rand.NextFloat(0.9f, 1.1f)).RotatedBy(MathHelper.ToRadians(-sine));
                    int shot1 = Projectile.NewProjectile(source, position.X, position.Y, helixVel1.X, helixVel1.Y, type, damage, knockback, player.whoAmI, 0f, 0f);
                    int shot2 = Projectile.NewProjectile(source, position.X, position.Y, helixVel2.X, helixVel2.Y, type, damage, knockback, player.whoAmI, 0f, 0f);
                }
                else
                {
                    //If left click, do the same as above but spawn Charged Blasts instead
                    Vector2 helixVel1 = (velocity * Main.rand.NextFloat(0.9f, 1.1f)).RotatedBy(MathHelper.ToRadians(sine));
                    Vector2 helixVel2 = (velocity * Main.rand.NextFloat(0.9f, 1.1f)).RotatedBy(MathHelper.ToRadians(-sine));
                    int shot1 = Projectile.NewProjectile(source, position.X, position.Y, helixVel1.X, helixVel1.Y, ModContent.ProjectileType<ChargedBlast>(), damage, knockback, player.whoAmI, 0f, 1, 1f);
                    int shot2 = Projectile.NewProjectile(source, position.X, position.Y, helixVel2.X, helixVel2.Y, ModContent.ProjectileType<ChargedBlast>(), damage, knockback, player.whoAmI, 0f, 0, 1f);
                }
            }
            else if (player.altFunctionUse != 2)
            {
                Particle spark2 = new LineParticle(position + Main.rand.NextVector2Circular(6, 6), (velocity * 4).RotatedByRandom(0.2f) * Main.rand.NextFloat(0.8f, 1.2f), false, Main.rand.Next(15, 25 + 1), Main.rand.NextFloat(1.5f, 2f), new Color(229, 49, 39));
                GeneralParticleHandler.SpawnParticle(spark2);
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Shredder>().
                AddIngredient<CosmiliteBar>(10).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
