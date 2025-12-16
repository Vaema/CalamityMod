using CalamityMod.Items.Ammo;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class AngelicShotgun : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";

        public override void SetDefaults()
        {
            Item.width = 86;
            Item.height = 38;
            Item.damage = 92;
            Item.knockBack = 3f;
            Item.DamageType = DamageClass.Ranged;
            Item.noMelee = true;
            Item.autoReuse = true;

            Item.useTime = 26;
            Item.useAnimation = 26;
            Item.UseSound = SoundID.Item38;
            Item.useStyle = ItemUseStyleID.Shoot;

            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.Calamity().donorItem = true;

            Item.shootSpeed = 12f;
            Item.shoot = ModContent.ProjectileType<AngelicBeam>();
            Item.useAmmo = AmmoID.Bullet;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-17, -3);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // If Musket Balls are used, damage is set to match Hallow-Point Rounds for both bullets and lasers
            if (type == ProjectileID.Bullet)
            {
                type = ModContent.ProjectileType<HallowPointRoundProj>();
                damage += HallowPointRound.BaseDamage - 7;
            }

            // Fire a shotgun spread of bullets.
            int NumBullets = Main.rand.Next(5, 6 + 1);
            Vector2 baseVelocity = velocity.SafeNormalize(Vector2.Zero) * velocity.Length();
            for (int i = 0; i < NumBullets; ++i)
            {
                Vector2 randomVelocity = baseVelocity.RotatedByRandom(MathHelper.ToRadians(12.5f)) * Main.rand.NextFloat(0.88f, 1.12f);
                Projectile.NewProjectile(source, position + velocity.SafeNormalize(Vector2.Zero) * 16f, randomVelocity, type, damage, knockback, player.whoAmI);
            }

            // Spawn a beam from the sky ala Hyperdeath Rift Scepter or Lunar Flare
            // This is more powerful if Hallow-Point Rounds (either naturally used or converted) are fired
            bool empowered = type == ModContent.ProjectileType<HallowPointRoundProj>();
            float laserSpeed = 8f;
            int laserDamage = (int)(damage * (empowered ? 3.5f : 2f));
            float laserKB = knockback * 1.6f;

            Vector2 newPos = new Vector2(player.ClampedMouseWorld().X + Main.rand.NextFloat(-160f, 160f), player.MountedCenter.Y - 1200f);
            Vector2 newVel = (player.ClampedMouseWorld() + Main.rand.NextVector2CircularEdge(8f, 8f) - newPos).SafeNormalize(Vector2.Zero) * laserSpeed;
            Projectile laser = Projectile.NewProjectileDirect(source, newPos, newVel, Item.shoot, laserDamage, laserKB, player.whoAmI);
            laser.scale = empowered ? 1.75f : 1f;

            // Play the sound of the laser beam
            SoundEngine.PlaySound(SoundID.Item72, player.Center);

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.SunplateBlock, 75).
                AddIngredient<DivineGeode>(15).
                AddIngredient<EssenceofSunlight>(7).
                AddTile(TileID.MythrilAnvil).
                Register();
        }

    }
}
