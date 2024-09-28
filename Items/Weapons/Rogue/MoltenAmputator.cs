using System;
using CalamityMod.Dusts;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class MoltenAmputator : RogueWeapon
    {
        public float speed = 16;
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }
        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 60;
            Item.damage = 200;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.useAnimation = 19;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 25;
            Item.knockBack = 25f;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.shoot = ModContent.ProjectileType<MoltenAmputatorProj>();
            Item.shootSpeed = speed;
            Item.DamageType = RogueDamageClass.Instance;
        }
        public override bool CanUseItem(Player player) => player.altFunctionUse == 2 || true; //|| player.ownedProjectileCounts[Item.shoot] <= 0;
        public override bool AltFunctionUse(Player player) => true;
        public override float StealthDamageMultiplier => 1.07f;
        public override float UseSpeedMultiplier(Player player)
        {
            if (player.altFunctionUse == 2)
                return 1.5f;
            else
                return (player.Calamity().amputatorBuff > 0 ? 3f : 1);

        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                if (player.Calamity().StealthStrikeAvailable())
                {
                    player.Calamity().amputatorBuff = 30;
                    player.Calamity().ConsumeStealthByAttacking();
                    SoundStyle buff = new("CalamityMod/Sounds/Custom/ProfanedGuardians/GuardianRay");
                    SoundEngine.PlaySound(buff with { Volume = 1f, Pitch = Main.rand.NextFloat(0.2f, 0.3f) }, position);

                    for (int i = 0; i < 20; i++)
                    {
                        Dust c = Dust.NewDustPerfect(position, ModContent.DustType<LightDust>());
                        c.velocity = (MathHelper.TwoPi * i / 20f).ToRotationVector2() * 15.5f * (i % 2 == 0 ? 0.88f : 1f);
                        c.scale = Main.rand.NextFloat(1.3f, 1.6f) * 0.8f * (i % 2 == 0 ? 2.2f : 1.8f);
                        c.noGravity = true;
                        c.color = Color.Goldenrod;
                        c.noLightEmittence = true;
                    }
                }
                for (int x = 0; x < Main.maxProjectiles; x++)
                {
                    Projectile projectile = Main.projectile[x];
                    if (projectile.active && projectile.type == type && projectile.ai[2] < 5)
                    {
                        projectile.ai[2] = 5;
                        SoundStyle pullback = new("CalamityMod/Sounds/Item/SwingMid");
                        SoundEngine.PlaySound(pullback with { Volume = 0.4f, Pitch = Main.rand.NextFloat(0.4f, 0.5f) }, position);
                    }
                }
            }
            else
            {
                SoundStyle fire = new("CalamityMod/Sounds/Item/SpearofDestiny");
                SoundEngine.PlaySound(fire with { Volume = 0.5f, Pitch = (player.Calamity().amputatorBuff > 0 ? -0.4f + (player.Calamity().amputatorBuff * 0.02f) : Main.rand.NextFloat(-0.4f, -0.65f)) }, position);
                // Since the positioning of the scythe is important, its velocity is based on your mouse position
                Vector2 staticSpeed = Utils.DirectionTo(position, position + velocity) * Utils.Distance(position, player.Calamity().mouseWorld) * 0.022f;
                Projectile scythe = Projectile.NewProjectileDirect(source, position, staticSpeed.RotatedByRandom((player.Calamity().amputatorBuff > 0 ? 0.8f : 0)), type, damage, knockback, player.whoAmI, 0, 0, 0);
                if (player.Calamity().amputatorBuff > 0)
                {
                    scythe.extraUpdates = 6;
                    scythe.Calamity().stealthStrike = true;
                    player.Calamity().amputatorBuff--;
                }
                player.Calamity().ConsumeStealthByAttacking();
            }
            return false;
        }
        public override void UseItemFrame(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                player.ChangeDir(Math.Sign((player.Calamity().mouseWorld - player.Center).X));

                float animProgress = 0.5f - player.itemTime / (float)player.itemTimeMax;
                float rotation = (player.Center - player.Calamity().mouseWorld).ToRotation() * player.gravDir + MathHelper.PiOver2;
                if (animProgress < 0.4f)
                    rotation += 0.39f * (float)Math.Pow((0.6f - animProgress) / 0.6f, 2) * player.direction;

                player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation);
            }
        }
    }
}
