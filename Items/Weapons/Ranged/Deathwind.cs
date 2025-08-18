using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class Deathwind : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<DeathhailStaff>();
        }
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 82;
            Item.damage = 4000;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 45;
            Item.useAnimation = 45;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 5f;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<CosmicPurple>();
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<DWArrow>();
            Item.shootSpeed = 20f;
            Item.useAmmo = AmmoID.Arrow;
            Item.channel = true;
            Item.Calamity().canFirePointBlankShots = true;
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Ranged/DeathwindGlow").Value);
        }
        private int storedDMG = 1;
        private float storedKB = 1;
        public override void UseItemFrame(Player player)
        {
            if (player.direction == -1)
                player.itemRotation -= MathHelper.Pi;
            if (player.itemTime < 60)
            {
                player.itemRotation = player.DirectionTo(player.Calamity().mouseWorld).ToRotation();
                float scale = (1 - (player.itemTime - 10) / 50f) * 0.3f;
                if (player.itemTime < 10)
                {
                    scale = ((player.itemTime) / 10f) * 0.3f;
                }
                GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(player.Center + Vector2.UnitX.RotatedBy(player.itemRotation) * (48 + scale*96), Vector2.Zero, Color.Cyan, Vector2.One, 0, scale, scale, 3));
                if (player.itemTime == 1)
                {
                    if (player.channel)
                    {
                        player.itemTime = 180;
                        player.itemAnimation = 180;
                    }
                    else
                    {

                        if (Main.LocalPlayer.Calamity().GeneralScreenShakePower < 2 && Main.LocalPlayer.Distance(player.Center) < 1600)
                            Main.LocalPlayer.Calamity().GeneralScreenShakePower = 2;
                        if (Main.myPlayer == player.whoAmI)
                        {
                            Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center + Vector2.UnitX.RotatedBy(player.itemRotation) * 1016, Vector2.UnitX.RotatedBy(player.itemRotation), ModContent.ProjectileType<FriendlyLaserWallBeam>(), storedDMG, storedKB, player.whoAmI, -1, 1);
                        }
                    }
                }
            }
            else
            {
                player.itemRotation += MathHelper.Clamp(MathHelper.WrapAngle(player.DirectionTo(player.Calamity().mouseWorld).ToRotation() - player.itemRotation), -0.04f, 0.04f);
                float scale = (1- (player.itemTime-70) / 110f) * 0.75f;
                if (player.itemTime < 70)
                {
                    scale = ((player.itemTime - 60) / 10f) * 0.75f;
                }
                GeneralParticleHandler.SpawnParticle(new DirectionalPulseRing(player.Center + Vector2.UnitX.RotatedBy(player.itemRotation) * (48 + scale * 96), Vector2.Zero, Color.Fuchsia, Vector2.One, 0, scale, scale, 3));
                if (player.itemTime == 60)
                {

                    if (Main.LocalPlayer.Calamity().GeneralScreenShakePower < 5 && Main.LocalPlayer.Distance(player.Center) < 1600)
                        Main.LocalPlayer.Calamity().GeneralScreenShakePower = 5;
                    if (Main.myPlayer == player.whoAmI)
                    {
                        int p = Projectile.NewProjectile(Item.GetSource_FromThis(), player.Center + Vector2.UnitX.RotatedBy(player.itemRotation) * 1016, Vector2.UnitX.RotatedBy(player.itemRotation), ModContent.ProjectileType<FriendlyLaserWallBeam>(), storedDMG*4, storedKB, player.whoAmI,-0.25f,1);
                        if (Main.projectile.IndexInRange(p))
                        {
                            Main.projectile[p].scale = 4;
                        }
                    }
                    player.itemTime = 0;
                    player.itemAnimation = 0;
                }
            }
            player.direction = player.itemRotation.ToRotationVector2().X.DirectionalSign();
            if (player.direction == -1)
                player.itemRotation += MathHelper.Pi;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            storedDMG = damage;
            storedKB = knockback;
            return false;
        }
    }
}
