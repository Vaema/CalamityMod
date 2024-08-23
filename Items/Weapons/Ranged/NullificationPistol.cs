using System;
using CalamityMod.Dusts;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    [LegacyName("NullificationRifle")]
    public class NullificationPistol : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public bool ShotType = true;
        public float mult = 0;

        public override void SetStaticDefaults()
        {
            ItemID.Sets.IsRangedSpecialistWeapon[Item.type] = true;
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 52;
            Item.height = 28;
            Item.damage = 150;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 0.1f;
            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.rare = ItemRarityID.Yellow;
            Item.autoReuse = true;
            Item.shootSpeed = 8f;
            Item.shoot = ModContent.ProjectileType<NullShot>();
        }
        public override bool AltFunctionUse(Player player) => true;
        public override float UseSpeedMultiplier(Player player)
        {
            if (player.altFunctionUse == 2)
                return 1.3f;

            if (!ShotType)
                return 1.5f - mult;

            return 1f - mult;
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                SoundStyle fire = new("CalamityMod/Sounds/Item/DudFire");
                SoundEngine.PlaySound(fire with { Volume = 0.7f, Pitch = -0.5f + (ShotType ? 0.5f : 0) }, position);
                for (int i = 0; i < 18; i++)
                {
                    Dust dust = Dust.NewDustPerfect(position, ShotType ? ModContent.DustType<VoidDust>() : ModContent.DustType<LightDust>(), (velocity).RotatedByRandom(100) * Main.rand.NextFloat(0.2f, 1f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.9f, 1.25f);
                    dust.color = ShotType ? Color.White : (Main.rand.NextBool() ? Color.Orchid : Color.Turquoise);
                }
                ShotType = !ShotType;
                mult = 0;
            }
            else if (ShotType)
            {
                for (int i = 0; i < 3; i++)
                    Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<NullShot>(), damage / 3, 0, player.whoAmI, 0, 0, i);
                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<NullFlash>(), damage / 2, 0, player.whoAmI, 0, 0);
                if (mult < 0.35f)
                    mult += 0.013f;
            }
            else
            {
                Projectile aBeam = Projectile.NewProjectileDirect(source, position, velocity, ModContent.ProjectileType<NullShot>(), damage / 2, 0, player.whoAmI, 0, 5);
                aBeam.penetrate = 1;
                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<NullFlash>(), damage / 2, 0, player.whoAmI, 0, 5);
                if (mult < 0.5f)
                    mult += 0.013f;
            }
            if (player.altFunctionUse != 2)
            {
                SoundStyle fire = new("CalamityMod/Sounds/Item/NullShot");
                SoundEngine.PlaySound(fire with { Volume = 0.7f, Pitch = Main.rand.NextFloat(0f, 0.2f) + (!ShotType ? 0.3f : 0) }, position);
            }
            return false;
        }
        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.ChangeDir(Math.Sign((player.Calamity().mouseWorld - player.Center).X));
            float itemRotation = player.compositeFrontArm.rotation + MathHelper.PiOver2 * player.gravDir;

            float pullback = 7f;

            float animProgress = 0.5f - player.itemTime / (float)player.itemTimeMax;
            float rotation = (player.Center - player.Calamity().mouseWorld).ToRotation() * player.gravDir + MathHelper.PiOver2;
            if (animProgress < 0.4f)
                pullback -= (2.75f) * (float)Math.Pow((0.6f - animProgress) / 0.6f, 2);

            Vector2 itemPosition = player.MountedCenter + itemRotation.ToRotationVector2() * pullback;
            Vector2 itemSize = new Vector2(52, 28);
            Vector2 itemOrigin = new Vector2(-24, 4);

            CalamityUtils.CleanHoldStyle(player, itemRotation, itemPosition, itemSize, itemOrigin);

            base.UseStyle(player, heldItemFrame);
        }

        public override void UseItemFrame(Player player)
        {
            player.ChangeDir(Math.Sign((player.Calamity().mouseWorld - player.Center).X));

            float animProgress = 0.5f - player.itemTime / (float)player.itemTimeMax;
            float rotation = (player.Center - player.Calamity().mouseWorld).ToRotation() * player.gravDir + MathHelper.PiOver2;
            if (animProgress < 0.4f)
                rotation += (player.altFunctionUse == 2 ? -0.15f : 0) * (float)Math.Pow((0.6f - animProgress) / 0.6f, 2) * player.direction;

            player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation);
        }
    }
}
