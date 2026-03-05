using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class LemonNadeHoldout : BaseSwordHoldoutProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/Rogue/LemonNadeProjectile";
        public override bool useMeleeSpeed => false;
        public override bool useMeleeSize => false;
        public override int swingWidth => 180;
        public override Item BaseItem => ModContent.GetModItem(ModContent.ItemType<LemonNade>()).Item;
        public override int AfterImageLength => 0;

        public override int StartupTime { get; set; }
        public override int CooldownTime { get; set; }

        bool isChannelable = false;

        int explodeTimer = 0;

        public override void Defaults()
        {
            Projectile.width = 22;  Projectile.height = 28;
            Projectile.MaxUpdates = 4;
            Projectile.noEnchantmentVisuals = true;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.ContinuouslyUpdateDamageStats = true;
        }
        public override void Spawn()
        {
            //This sets variables for the spear in general, as well as the secondary attack
            //The secondary attack is the "default" because it was coded first
            var player = Main.player[Projectile.owner];
            var modplayer = player.GetModPlayer<BaseSwordHoldoutPlayer>();
            StartupTime = 10;
            CooldownTime = 10;
            swingTime -= StartupTime + CooldownTime;
            OffsetDistance = 24;
            modplayer.swingNum = 0;
        }

        public override void AdditionalAI()
        {
            var player = Main.player[Projectile.owner];
            var cplayer = player.Calamity();
            if (Projectile.Opacity > 0)
            {
                cplayer.temporaryStealthMax = 10;
                cplayer.temporaryStealthTimer = 2;
            }

            //MOVE SECONDS TO LemonNade.cs ITEM
            var avgStealth = 0.8f * cplayer.stealthGenMoving + 0.2f * cplayer.stealthGenStandstill;
            var explodeTimeGoal = CalamityUtils.SecondsToFrames(2) / avgStealth;
            var stealthTime = explodeTimeGoal * 0.9f;

            if (Projectile.FinalExtraUpdate() && inStartup)
                if (explodeTimer < explodeTimeGoal)
                    explodeTimer++;
                else
                {
                    Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, angle * -10, ModContent.ProjectileType<LemonNadeProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner, explodeTimer, explodeTimeGoal).hostile = true;
                    Projectile.Opacity = 0;
                    return;
                }

                    cplayer.rogueStealth = Projectile.Opacity <= 0 ? 0 : Math.Max(cplayer.temporaryStealthMax, cplayer.rogueStealthMax) * MathHelper.Clamp(explodeTimer / stealthTime, 0f, 1f);

            if (timer == 1)
            {


                if (!Main.dedServ)
                {
                    SoundEngine.PlaySound(SoundID.DD2_JavelinThrowersAttack with { pitch = 1f });
                    Gore.NewGore(Projectile.GetSource_FromAI(), Projectile.position, Vector2.UnitY * -8 + angle, Mod.Find<ModGore>("LemonNadePin").Type, 1);
                }
            }

            //When channeling, the internal timer will not progress
            if (player.channel && inStartup)
            {
                if (timer >= StartupTime - 1)
                {
                    timer--;
                    Projectile.timeLeft++;
                }
            }
            if (explodeTimer >= explodeTimeGoal && inCooldown && !Projectile.FinalExtraUpdate())
            {
                timer--;
                Projectile.timeLeft++;
            }

            if (inSwing)
            {
                if (swingTimer == (int)(swingTime * 0.75f))
                {
                    if (Projectile.owner == Main.myPlayer && Projectile.Opacity > 0)
                    {
                        var p = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, angle * -10, ModContent.ProjectileType<LemonNadeProjectile>(), Projectile.damage, Projectile.knockBack, Projectile.owner, explodeTimer-30, explodeTimeGoal);
                        if (player.Calamity().StealthStrikeAvailable())
                        {
                            p.Calamity().stealthStrike = true;
                        }
                    }
                    Projectile.Opacity = 0;
                }
            }  
        }

        public override float SwingFunction()
        {
            if (inStartup)
                return MathHelper.ToRadians(MathHelper.SmoothStep(swingWidth * -0.33f, swingWidth * -0.7f, MathF.Pow(StartupCompletion, 4f)));
            if (inCooldown)
                return MathHelper.ToRadians(MathHelper.Lerp(swingWidth * 0.2f, swingWidth * 0.33f, 1 - MathF.Pow(1 - CooldownCompletion, 3f)));
            return MathHelper.ToRadians(MathHelper.SmoothStep(swingWidth * -0.7f, swingWidth * 0.2f, SwingCompletion));
        }

        public override bool? CanHitNPC(NPC target)
        {
            return false;
        }

        public override bool CanHitPlayer(Player target)
        {
            return false;
        }
    }
}
