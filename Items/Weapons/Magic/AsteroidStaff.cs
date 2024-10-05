using System;
using System.Collections.Generic;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    public class AsteroidStaff : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        #region GFB Projectile Array
        public static int[] CorekeeperChaos =
        {
            ProjectileID.BallofFire,
            ProjectileID.BallofFrost,
            ProjectileID.MagicMissile,
            ProjectileID.WaterStream,
            ProjectileID.Flamelash,
            ProjectileID.RainbowRodBullet,
            ProjectileID.UnholyTridentFriendly,
            ProjectileID.AmethystBolt,
            ProjectileID.AmberBolt,
            ProjectileID.TopazBolt,
            ProjectileID.DiamondBolt,
            ProjectileID.SapphireBolt,
            ProjectileID.RubyBolt,
            ProjectileID.EmeraldBolt,
            ProjectileID.RainCloudMoving,
            ProjectileID.BloodCloudMoving,
            ProjectileID.ShadowBeamFriendly,
            ProjectileID.InfernoFriendlyBlast,
            ProjectileID.LostSoulFriendly,
            ProjectileID.Bat,

            ModContent.ProjectileType<AquamarineBolt>(),
            ModContent.ProjectileType<HellwingBat>(),
            ModContent.ProjectileType<ManaBolt>(),
            ModContent.ProjectileType<NightsRayBeam>(),
            ModContent.ProjectileType<WaterLeechProj>(),
            ModContent.ProjectileType<Sandstream>(),
            ModContent.ProjectileType<AuroraAustralis>(),
            ModContent.ProjectileType<BeamingBolt>(),
            ModContent.ProjectileType<AstralCrystal>(),
            ModContent.ProjectileType<AstralachneaFang>(),
            ModContent.ProjectileType<PlagueFang>(),
            ModContent.ProjectileType<VitriolicViperFang>(),
            ModContent.ProjectileType<VitriolicViperSpit>(),
            ModContent.ProjectileType<AtlantisSpear>(),
            ModContent.ProjectileType<BrimroseBeam>(),
            ModContent.ProjectileType<GleamingBolt>(),
            ModContent.ProjectileType<InfernalBlade>(),
            ModContent.ProjectileType<IcicleStaffProj>(),
            ModContent.ProjectileType<KeelhaulBubble>(),
            ModContent.ProjectileType<MiasmaGas>(),
            ModContent.ProjectileType<PhotosyntheticSolarBeam>(),
            ModContent.ProjectileType<ShiftingSandsProj>(),
            ModContent.ProjectileType<SnowflakeIceStar>(),
            ModContent.ProjectileType<UndinesRetributionSpear>(),
            ModContent.ProjectileType<ValkyrieRayBeam>(),
            ModContent.ProjectileType<AsteroidMolten>(),
            ModContent.ProjectileType<WyvernProjectile>(),
            ModContent.ProjectileType<ClamorNoctusWyvern>(),
            ModContent.ProjectileType<WyvernFeatherPink>(),
            ModContent.ProjectileType<Asteroid>(),
            ModContent.ProjectileType<DeathhailBeam>(),
            ModContent.ProjectileType<DivineRetributionSpear>(),
            ModContent.ProjectileType<StardustElementalBeam>(),
            ModContent.ProjectileType<SolarElementalBeam>(),
            ModContent.ProjectileType<VortexElementalBeam>(),
            ModContent.ProjectileType<NebulaElementalBeam>(),
            ModContent.ProjectileType<FabBolt>(),
            ModContent.ProjectileType<VolatileStarcore>(),
            ModContent.ProjectileType<FatesRevealFlame>(),
            ModContent.ProjectileType<MagneticOrb>(),
            ModContent.ProjectileType<PhantasmalFuryProj>(),
            ModContent.ProjectileType<VenusianBolt>(),
            ModContent.ProjectileType<Shadowbolt>(),
            ModContent.ProjectileType<NebulaCloudCore>(),
            ModContent.ProjectileType<PrinceFlameLarge>(),
            ModContent.ProjectileType<SanguineFlareProj>(),
            ModContent.ProjectileType<VisceraBeam>(),
            ModContent.ProjectileType<SoulPiercerBeam>(),
            ModContent.ProjectileType<LiquidBlade>(),
            ModContent.ProjectileType<VividClarityBeam>(),
            ModContent.ProjectileType<BloodfireArrowProj>(),
            ModContent.ProjectileType<VoidVortexProj>(),
            ModContent.ProjectileType<ClimaxProj>(),
            ModContent.ProjectileType<VehemenceBolt>(),
        };
        #endregion
        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 50;
            Item.damage = 146;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 18;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 6.75f;
            Item.UseSound = SoundID.Item88;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<Asteroid>();
            Item.shootSpeed = 20f;

            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.rare = ItemRarityID.Purple;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (Main.zenithWorld)
            {
                for (int i = 0; i < 4; i++)
                {
                    int projType;
                    projType = CorekeeperChaos[Main.rand.Next(CorekeeperChaos.Length)];
                    float SpeedX = velocity.X + (float)Main.rand.Next(-60, 61) * 0.05f;
                    float SpeedY = velocity.Y + (float)Main.rand.Next(-60, 61) * 0.05f;
                    int index = Projectile.NewProjectile(source, position.X, position.Y, SpeedX, SpeedY, projType, damage, knockback, player.whoAmI);

                }
            }
            else
            {
                float meteorSpeed = Item.shootSpeed;
                Vector2 realPlayerPos = player.RotatedRelativePoint(player.MountedCenter, true);
                float meteorSpawnXPos = (float)Main.mouseX + Main.screenPosition.X - realPlayerPos.X;
                float meteorSpawnYPos = (float)Main.mouseY + Main.screenPosition.Y - realPlayerPos.Y;
                if (player.gravDir == -1f)
                {
                    meteorSpawnYPos = Main.screenPosition.Y + (float)Main.screenHeight - (float)Main.mouseY - realPlayerPos.Y;
                }
                float meteorSpawnDist = (float)Math.Sqrt((double)(meteorSpawnXPos * meteorSpawnXPos + meteorSpawnYPos * meteorSpawnYPos));
                if ((float.IsNaN(meteorSpawnXPos) && float.IsNaN(meteorSpawnYPos)) || (meteorSpawnXPos == 0f && meteorSpawnYPos == 0f))
                    for (int i = 0; i < 4; i++)
                    {
                        meteorSpawnXPos = (float)player.direction;
                        meteorSpawnYPos = 0f;
                        meteorSpawnDist = meteorSpeed;
                    }
                else
                {
                    meteorSpawnDist = meteorSpeed / meteorSpawnDist;
                }

                int asteroidAmt = 3;
                for (int i = 0; i < asteroidAmt; i++)
                {
                    realPlayerPos = new Vector2(player.Center.X + (float)(Main.rand.Next(201) * -(float)player.direction) + ((float)Main.mouseX + Main.screenPosition.X - player.position.X), player.MountedCenter.Y - 600f);
                    realPlayerPos.X = (realPlayerPos.X + player.Center.X) / 2f + (float)Main.rand.Next(-200, 201);
                    realPlayerPos.Y -= (float)(100 * i);
                    meteorSpawnXPos = (float)Main.mouseX + Main.screenPosition.X - realPlayerPos.X + (float)Main.rand.Next(-40, 41) * 0.03f;
                    meteorSpawnYPos = (float)Main.mouseY + Main.screenPosition.Y - realPlayerPos.Y;
                    if (meteorSpawnYPos < 0f)
                    {
                        meteorSpawnYPos *= -1f;
                    }
                    if (meteorSpawnYPos < 20f)
                    {
                        meteorSpawnYPos = 20f;
                    }
                    meteorSpawnDist = (float)Math.Sqrt((double)(meteorSpawnXPos * meteorSpawnXPos + meteorSpawnYPos * meteorSpawnYPos));
                    meteorSpawnDist = meteorSpeed / meteorSpawnDist;
                    meteorSpawnXPos *= meteorSpawnDist;
                    meteorSpawnYPos *= meteorSpawnDist;
                    float meteorSpawnXOffset = meteorSpawnXPos;
                    float meteorSpawnYOffset = meteorSpawnYPos + (float)Main.rand.Next(-40, 41) * 0.02f;
                    Projectile.NewProjectile(source, realPlayerPos.X, realPlayerPos.Y, meteorSpawnXOffset * 0.75f, meteorSpawnYOffset * 0.75f, type, damage, knockback, player.whoAmI, 0f, 0.5f + (float)Main.rand.NextDouble() * 0.3f);
                }
            }
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> list) => list.FindAndReplace("[GFB]", this.GetLocalizedValue(Main.zenithWorld ? "TooltipGFB" : "TooltipNormal"));

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.MeteorStaff).
                AddIngredient(ItemID.LunarBar, 5).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
