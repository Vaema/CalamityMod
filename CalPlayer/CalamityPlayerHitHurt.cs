using System;
using System.Linq;
using CalamityMod.Balancing;
using CalamityMod.Buffs.Cooldowns;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.Placeables;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.CalPlayer.Dashes;
using CalamityMod.Cooldowns;
using CalamityMod.Dusts;
using CalamityMod.Enums;
using CalamityMod.Events;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Accessories.Vanity;
using CalamityMod.Items.Armor.Aerospec;
using CalamityMod.Items.Armor.Daedalus;
using CalamityMod.Items.Armor.Demonshade;
using CalamityMod.Items.Armor.Empyrean;
using CalamityMod.Items.Armor.GodSlayer;
using CalamityMod.Items.Armor.Hydrothermic;
using CalamityMod.Items.Armor.LunicCorps;
using CalamityMod.Items.Armor.Reaver;
using CalamityMod.Items.Armor.Silva;
using CalamityMod.Items.Armor.Sulphurous;
using CalamityMod.Items.Armor.Victide;
using CalamityMod.Items.Armor.Wulfrum;
using CalamityMod.Items.Mounts;
using CalamityMod.Items.Placeables.Furniture;
using CalamityMod.Items.Potions;
using CalamityMod.Items.Potions.Alcohol;
using CalamityMod.Items.VanillaArmorChanges;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.NPCs;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.NPCs.Other;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Systems.Collections;
using CalamityMod.UI;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.CalPlayer
{
    public partial class CalamityPlayer : ModPlayer
    {
        #region Dodges
        private void SpectralVeilDodge()
        {
            // 17APR2024: Ozzatron: Spectral Veil counts as a dodge. It uses vanilla dodge iframes and benefits from Cross Necklace.
            int spectralVeilIFrames = spectralVeilImmunity + (Player.longInvince ? BalancingConstants.CrossNecklaceIFrameBoost : 0);
            Player.GiveUniversalIFrames(spectralVeilIFrames, true);
            rogueStealth = rogueStealthMax;
            spectralVeilImmunity = 0;

            Vector2 sVeilDustDir = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
            sVeilDustDir.Normalize();
            sVeilDustDir *= 0.5f;

            for (int j = 0; j < 20; j++)
            {
                Dust sVeilDust1 = Dust.NewDustDirect(Player.Center, 1, 1, DustID.VilePowder, sVeilDustDir.X * j, sVeilDustDir.Y * j);
                Dust sVeilDust2 = Dust.NewDustDirect(Player.Center, 1, 1, DustID.VilePowder, -sVeilDustDir.X * j, -sVeilDustDir.Y * j);
                sVeilDust1.noGravity = false;
                sVeilDust1.noLight = false;
                sVeilDust2.noGravity = false;
                sVeilDust2.noLight = false;
            }

            SoundEngine.PlaySound(SilvaHeadSummon.DispelSound, Player.Center);

            NetMessage.SendData(MessageID.Dodge, -1, -1, null, Player.whoAmI, 1f, 0f, 0f, 0, 0, 0);
        }

        private void GodSlayerDodge()
        {
            // 17APR2024: Ozzatron: God Slayer Dodge is a dodge. It uses vanilla dodge iframes and benefits from Cross Necklace.
            int godSlayerDodgeIFrames = Player.ComputeDodgeIFrames();
            Player.GiveUniversalIFrames(godSlayerDodgeIFrames, true);

            SoundEngine.PlaySound(SoundID.Item67, Player.Center);

            for (int j = 0; j < 30; j++)
            {
                Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, (int)CalamityDusts.PurpleCosmilite, 0f, 0f, 100, default, 2f);
                dust.position.X += Main.rand.Next(-20, 21);
                dust.position.Y += Main.rand.Next(-20, 21);
                dust.velocity *= 0.4f;
                dust.scale *= 1f + Main.rand.Next(40) * 0.01f;
                dust.shader = GameShaders.Armor.GetSecondaryShader(Player.ArmorSetDye(), Player);
                if (Main.rand.NextBool())
                {
                    dust.scale *= 1f + Main.rand.Next(40) * 0.01f;
                    dust.noGravity = true;
                }
            }

            NetMessage.SendData(MessageID.Dodge, -1, -1, null, Player.whoAmI, 1f, 0f, 0f, 0, 0, 0);
        }

        private void CounterScarfDodge()
        {
            int duration = CalamityUtils.SecondsToFrames(30);
            Player.AddCooldown(ScarfCooldown.ID, duration, true, evasionScarf ? "evasionscarf" : "counterscarf");

            // 17APR2024: Ozzatron: Counter Scarf is a dodge. It uses vanilla dodge iframes and benefits from Cross Necklace.
            int counterScarfIFrames = Player.ComputeDodgeIFrames();
            Player.GiveUniversalIFrames(counterScarfIFrames, true);

            for (int j = 0; j < 100; j++)
            {
                Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.LifeDrain, 0f, 0f, 100, default, 2f);
                dust.position.X += Main.rand.Next(-20, 21);
                dust.position.Y += Main.rand.Next(-20, 21);
                dust.velocity *= 0.4f;
                dust.scale *= 1f + Main.rand.Next(40) * 0.01f;
                dust.shader = GameShaders.Armor.GetSecondaryShader(Player.cNeck, Player);
                if (Main.rand.NextBool())
                {
                    dust.scale *= 1f + Main.rand.Next(40) * 0.01f;
                    dust.noGravity = true;
                }
            }

            NetMessage.SendData(MessageID.Dodge, -1, -1, null, Player.whoAmI, 1f, 0f, 0f, 0, 0, 0);
        }

        public void AbyssMirrorDodge(double dodgeDamageGateValuePercent, int dodgeDamageGateValue, int hitDamage)
        {
            double maxCooldownDurationDamagePercent = 0.5;
            int maxCooldownDurationDamageValue = (int)Math.Round(Player.statLifeMax2 * (maxCooldownDurationDamagePercent - dodgeDamageGateValuePercent));

            // Just in case...
            if (maxCooldownDurationDamageValue <= 0)
                maxCooldownDurationDamageValue = 1;

            float cooldownDurationScalar = MathHelper.Clamp((hitDamage - dodgeDamageGateValue) / (float)maxCooldownDurationDamageValue, 0f, 1f);

            if (Player.whoAmI == Main.myPlayer && abyssalMirror && !eclipseMirror)
            {
                int cooldownDuration = (int)MathHelper.Lerp(BalancingConstants.MirrorDodgeCooldownMin, BalancingConstants.MirrorDodgeCooldownMax, cooldownDurationScalar);
                Player.AddCooldown(GlobalDodge.ID, cooldownDuration, true, "abyssmirror");

                // 17APR2024: Ozzatron: Abyssal Mirror is a dodge. It uses vanilla dodge iframes and benefits from Cross Necklace.
                int abyssalMirrorDodgeIFrames = Player.ComputeDodgeIFrames();
                Player.GiveUniversalIFrames(abyssalMirrorDodgeIFrames, true);

                rogueStealth += 0.5f;
                SoundEngine.PlaySound(SilvaHeadSummon.ActivationSound, Player.Center);

                var source = Player.GetSource_Accessory(FindAccessory(ModContent.ItemType<AbyssalMirror>()));
                for (int i = 0; i < 10; i++)
                {
                    int damage = (int)Player.GetTotalDamage<RogueDamageClass>().ApplyTo(55);

                    int lumenyl = Projectile.NewProjectile(source, Player.Center.X, Player.Center.Y, Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f), ModContent.ProjectileType<AbyssalMirrorProjectile>(), damage, 0, Player.whoAmI);
                    Main.projectile[lumenyl].rotation = Main.rand.NextFloat(0, 360);
                    Main.projectile[lumenyl].frame = Main.rand.Next(0, 4);
                    if (lumenyl.WithinBounds(Main.maxProjectiles))
                        Main.projectile[lumenyl].DamageType = DamageClass.Generic;
                }

                // TODO -- Calamity dodges should probably not send a vanilla dodge packet considering that causes Tabi dust
                if (Player.whoAmI == Main.myPlayer)
                {
                    NetMessage.SendData(MessageID.Dodge, -1, -1, null, Player.whoAmI, 1f, 0f, 0f, 0, 0, 0);
                }
            }
        }

        public void EclipseMirrorDodge(double dodgeDamageGateValuePercent, int dodgeDamageGateValue, int hitDamage)
        {
            double maxCooldownDurationDamagePercent = 0.5;
            int maxCooldownDurationDamageValue = (int)Math.Round(Player.statLifeMax2 * (maxCooldownDurationDamagePercent - dodgeDamageGateValuePercent));

            // Just in case...
            if (maxCooldownDurationDamageValue <= 0)
                maxCooldownDurationDamageValue = 1;

            float cooldownDurationScalar = MathHelper.Clamp((hitDamage - dodgeDamageGateValue) / (float)maxCooldownDurationDamageValue, 0f, 1f);

            if (Player.whoAmI == Main.myPlayer && eclipseMirror)
            {
                int cooldownDuration = (int)MathHelper.Lerp(BalancingConstants.MirrorDodgeCooldownMin, BalancingConstants.MirrorDodgeCooldownMax, cooldownDurationScalar);
                Player.AddCooldown(GlobalDodge.ID, cooldownDuration, true, "eclipsemirror");

                // 17APR2024: Ozzatron: Eclipse Mirror is a dodge. It uses vanilla dodge iframes and benefits from Cross Necklace.
                int eclipseMirrorDodgeIFrames = Player.ComputeDodgeIFrames();
                Player.GiveUniversalIFrames(eclipseMirrorDodgeIFrames, true);

                rogueStealth += 0.5f;
                SoundEngine.PlaySound(SoundID.Item68, Player.Center);

                var source = Player.GetSource_Accessory(FindAccessory(ModContent.ItemType<EclipseMirror>()));
                int damage = (int)Player.GetTotalDamage<RogueDamageClass>().ApplyTo(2000);

                int eclipse = Projectile.NewProjectile(source, Player.Center, Vector2.Zero, ModContent.ProjectileType<EclipseMirrorBurst>(), damage, 0, Player.whoAmI);
                if (eclipse.WithinBounds(Main.maxProjectiles))
                    Main.projectile[eclipse].DamageType = DamageClass.Generic;

                // TODO -- Calamity dodges should probably not send a vanilla dodge packet considering that causes Tabi dust
                if (Player.whoAmI == Main.myPlayer)
                {
                    NetMessage.SendData(MessageID.Dodge, -1, -1, null, Player.whoAmI, 1f, 0f, 0f, 0, 0, 0);
                }
            }
        }
        #endregion

        #region Pre Kill
        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            PopupGUIManager.SuspendAll();

            if (andromedaState == AndromedaPlayerState.LargeRobot)
            {
                if (!Main.dedServ)
                {
                    for (int i = 0; i < 40; i++)
                    {
                        Dust dust = Dust.NewDustPerfect(Player.Center + Utils.NextVector2Circular(Main.rand, 60f, 90f), 133);
                        dust.velocity = Utils.NextVector2Circular(Main.rand, 4f, 4f);
                        dust.noGravity = true;
                        dust.scale = Main.rand.NextFloat(1.2f, 1.35f);
                    }

                    for (int i = 0; i < 3; i++)
                        Utils.PoofOfSmoke(Player.Center + Utils.NextVector2Circular(Main.rand, 20f, 30f));
                }
            }

            // Xyk vanity death animation
            if (XykVisualsBlue || XykVisualsOrange)
            {
                Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.Zero, ModContent.ProjectileType<XykDeathAnim>(), Main.zenithWorld ? Main.rand.Next(5000, 50000 + 1) : 0, 0, Player.whoAmI);
            }

            if (holyInferno)
            {
                foreach (NPC n in Main.ActiveNPCs)
                {
                    if (n.type == ModContent.NPCType<Providence>())
                        n.active = false;
                }
            }

            if (nebulousCore && !Player.HasCooldown(Cooldowns.NebulousCore.ID))
            {
                SoundEngine.PlaySound(SoundID.Item67, Player.Center);

                for (int j = 0; j < 50; j++)
                {
                    Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.ShadowbeamStaff, 0f, 0f, 100, default, 2f);
                    dust.position.X += Main.rand.Next(-20, 21);
                    dust.position.Y += Main.rand.Next(-20, 21);
                    dust.velocity *= 0.9f;
                    dust.scale *= 1f + Main.rand.Next(40) * 0.01f;
                    // Change this accordingly if we have a proper equipped sprite.
                    dust.shader = GameShaders.Armor.GetSecondaryShader(Player.cBody, Player);
                    if (Main.rand.NextBool())
                        dust.scale *= 1f + Main.rand.Next(40) * 0.01f;
                }

                // Nebulous Core clears Chalice of the Blood God's bleedout buffer
                if (chaliceOfTheBloodGod)
                {
                    chaliceBleedoutBuffer = 0D;
                    chaliceDamagePointPartialProgress = 0D;
                }
                Player.HealPlayer(100);

                Player.AddCooldown(Cooldowns.NebulousCore.ID, CalamityUtils.SecondsToFrames(90));
                return false;
            }

            if (DashID == GodslayerArmorDash.ID && Player.dashDelay < 0)
            {
                if (Player.statLife < 1)
                    Player.statLife = 1;

                return false;
            }

            if (silvaSet && silvaCountdown > 0)
            {
                if (silvaCountdown == SilvaReviveDuration && !hasSilvaEffect)
                {
                    SoundEngine.PlaySound(SilvaHeadSummon.ActivationSound, Player.Center);
                    Player.AddBuff(ModContent.BuffType<SilvaRevival>(), SilvaReviveDuration);
                }

                hasSilvaEffect = true;

                if (Player.statLife < 1)
                    Player.statLife = 1;

                // Silva revive clears Chalice of the Blood God's bleedout buffer every frame while active
                // Can we please remove this from the game
                if (chaliceOfTheBloodGod)
                {
                    chaliceBleedoutBuffer = 0D;
                    chaliceDamagePointPartialProgress = 0D;
                }

                return false;
            }

            if (necroSet && necroReviveCounter == -1)
            {
                SoundEngine.PlaySound(SoundID.DD2_SkeletonDeath, Player.Center);

                necroReviveCounter = 0; // Start ticking the timer of death
                Player.statLife = Player.statLifeMax2;

                if (Player.statLife < 1)
                    Player.statLife = 1;
                return false;
            }

            if (permafrostsConcoction && !Player.HasCooldown(PermafrostConcoction.ID))
            {
                Player.AddCooldown(PermafrostConcoction.ID, CalamityUtils.SecondsToFrames(180));
                Player.AddBuff(ModContent.BuffType<Encased>(), CalamityUtils.SecondsToFrames(3f));

                Player.statLife = Player.statLifeMax2 * 3 / 10;
                if (Player.statMana < 0)
                    Player.statMana = 0;

                SoundEngine.PlaySound(SoundID.Item92, Player.Center);

                for (int i = 0; i < 60; i++)
                {
                    Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.GemSapphire, 0f, 0f, 0, default, 2.5f);
                    dust.noGravity = true;
                    dust.velocity *= 5f;
                }

                return false;
            }

            // Custom Death Messages

            if (damage == 10.0 && hitDirection == 0 && damageSource.SourceOtherIndex == 8)
            {
                if (alcoholPoisoning)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.AlcoholBig" + Main.rand.Next(1, 2 + 1)).ToNetworkText(Player.name));
                }
                if (vHex)
                {
                    // Unique messages appear half the time during each individual stage of SCal's fight
                    string vHexKeyToUse = "Status.Death.VulnerabilityHex" + Main.rand.Next(1, 3 + 1);
                    if (Main.rand.NextBool() && CalamityGlobalNPC.SCal != -1)
                    {
                        if (CalamityGlobalNPC.SCalGrief != -1)
                            vHexKeyToUse = "Status.Death.VulnerabilityHexGrief";
                        else if (CalamityGlobalNPC.SCalLament != -1)
                            vHexKeyToUse = "Status.Death.VulnerabilityHexLament";
                        else if (CalamityGlobalNPC.SCalEpiphany != -1)
                            vHexKeyToUse = "Status.Death.VulnerabilityHexEpiphany";
                        // good luck dying to SCal in Acceptance to see this
                        else if (CalamityGlobalNPC.SCalAcceptance != -1)
                            vHexKeyToUse = "Status.Death.VulnerabilityHexAcceptance";

                        // if none of SCal's phases are detected somehow then it just uses the normal messages all the time
                    }
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText(vHexKeyToUse).ToNetworkText(Player.name));
                }
                if (ZoneCalamity && Player.lavaWet)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.SearingLava" + Main.rand.Next(1, 2 + 1)).ToNetworkText(Player.name));
                }
                if (godSlayerInferno)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.GodSlayerInferno" + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
                }
                if (sulphurPoison)
                {
                    if (!Main.rand.NextBool(4)) // 75% custom
                        damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.SulphuricPoisoning" + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
                    else
                        damageSource = PlayerDeathReason.ByOther(9); // 25% generic Poisoned death text
                }
                if (dragonFire)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.Dragonfire" + Main.rand.Next(1, 4 + 1)).ToNetworkText(Player.name));
                }
                if (vermillionFlux)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.VermillionFlux" + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
                }
                if (auricRebuke)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.AuricRebuke" + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
                }
                if (staticDischarge)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.StaticDischarge" + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
                }
                if (miracleBlight)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.MiracleBlight" + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
                }
                if (holyInferno)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.HolyInferno").ToNetworkText(Player.name));
                }
                if (holyFlames || banishingFire)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.HolyFlames" + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
                }
                if (shadowflame)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.Shadowflame").ToNetworkText(Player.name));
                }
                if (daybroken)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.Daybroken").ToNetworkText(Player.name));
                }
                if (burningBlood)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.BurningBlood" + Main.rand.Next(1, 2 + 1)).ToNetworkText(Player.name));
                }
                if (brainRot)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.BrainRot" + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
                }
                if (heavybleeding)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.HeavyBleeding" + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
                }
                if (laceration)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.Laceration" + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
                }
                if (elementalMix)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.ElementalMix" + Main.rand.Next(1, 2 + 1)).ToNetworkText(Player.name));
                }
                if (crushDepth)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.CrushDepth" + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
                }
                if (riptide)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.Riptide" + Main.rand.Next(1, 2 + 1)).ToNetworkText(Player.name));
                }
                if (hadopelagicPressure)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.HadopelagicPressure" + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
                }
                if (brimstoneFlames || weakBrimstoneFlames || demonicFlames)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.BrimstoneFlames" + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
                }
                if (plague)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.Plague" + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
                }
                if (astralInfection)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.AstralInfection" + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
                }
                if (nightwither)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.Nightwither").ToNetworkText(Player.name));
                }
                if (vaporfied)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.Vaporfied").ToNetworkText(Player.name));
                }
                if (manaOverloader || ManaBurn)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.ManaBurn").ToNetworkText(Player.name));
                }
                if (witheredDebuff)
                {
                    damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.Withered").ToNetworkText(Player.name));
                }
            }
            if (profanedCrystalBuffs && Player.Transformation().Type == ModContent.ItemType<ProfanedSoulCrystal>())
            {
                damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.ProfanedSoulCrystal").ToNetworkText(Player.name));
            }

            // Leon Death Noise RE4
            if (Main.zenithWorld)
                SoundEngine.PlaySound(LeonDeathNoiseRE4_ForGFB, Player.Center);

            if (NorfleetCounter > 3 && NorfleetCounter < 1000)
                damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.Norfleet").ToNetworkText(Player.name));
            NorfleetCounter = 0;

            if (NPC.AnyNPCs(ModContent.NPCType<SupremeCalamitas>()))
            {
                if (sCalDeathCount < 51)
                    sCalDeathCount++;
            }

            return true;
        }
        #endregion

        #region Modify Hit NPC
        public override void ModifyHitNPCWithItem(Item item, NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.CritDamage += critDamage;
            // All Calamity multipliers are added together to prevent insane exponential stacking
            float totalDamageMult = 1f;

            // Rippers are always checked for application, because there are ways to get rippers outside of Rev now
            CalamityUtils.ApplyRippersToDamage(this, item.IsTrueMelee(), ref totalDamageMult);

            // Demonshade enrage
            if (enraged)
                totalDamageMult += DemonshadeHelm.MultDamageBoost;
            // Withering enchantment when it's draining your HP
            if (witheredDebuff && witheringWeaponEnchant)
                totalDamageMult += 0.6f;

            // Apply all Calamity multipliers as a sum total to TML New Damage in a single step
            modifiers.SourceDamage *= totalDamageMult;

            // 01JUN2024: Ozzatron: apply Yellow Candle "chip damage" as a dirty modifier
            // The registration of the dirty modifier is conditional to ensure it doesn't apply to "near invincible" targets
            //
            // FinalDamage cannot be used for the intended effect because there is no way to access the actual damage of the hit
            CalamityGlobalNPC cgn = target.Calamity();
            if (yellowCandle && cgn.DR < 0.99f && target.takenDamageMultiplier > 0.05f)
                modifiers.ModifyHitInfo += YellowCandleBuff.ModifyHitInfo_Spite;

            if (Player.Calamity().scionsCurio && item.CountsAsClass<RangedDamageClass>())
                target.Calamity().scionsCurioEffected = true;

            // Frost Armor's rework gives +X% melee damage and +Y% ranged damage based on distance, where X+Y = 15.
            if (frostSet)
            {
                // 0f = point blank, 1f = max range or further
                float DistanceInterpolant = Utils.GetLerpValue(FrostArmorSetChange.MinDistance, FrostArmorSetChange.MaxDistance, target.Distance(Main.LocalPlayer.Center), true);

                if (item.CountsAsClass<MeleeDamageClass>())
                {
                    float meleeBoost = MathHelper.Lerp(0f, FrostArmorSetChange.ProximityBoost, 1 - DistanceInterpolant);
                    modifiers.SourceDamage += meleeBoost;

                    if (meleeBoost >= FrostArmorSetChange.ProximityBoost * 0.5f)
                    {
                        float intensity = meleeBoost / FrostArmorSetChange.ProximityBoost;
                        SoundEngine.PlaySound(Cryogen.HitSound with { Volume = intensity - 0.2f }, Player.Center);
                    }
                    if (meleeBoost > 0f)
                    {
                        int count = (int)(30f * meleeBoost);
                        for (int i = 0; i < count; i++)
                        {
                            Vector2 velocity = Main.rand.NextVector2Unit() * (5f + 100f * meleeBoost);
                            float scale = Main.rand.NextFloat(0.5f, 1f) + 0.5f * meleeBoost / FrostArmorSetChange.ProximityBoost;
                            Particle sparkle = new CritSpark(target.Center, velocity, Color.White, Color.DodgerBlue, scale, 15, 0.1f, scale * 2f);
                            GeneralParticleHandler.SpawnParticle(sparkle);
                        }
                    }
                }
                else if (item.CountsAsClass<RangedDamageClass>())
                {
                    float rangedBoost = MathHelper.Lerp(0f, FrostArmorSetChange.ProximityBoost, DistanceInterpolant);
                    modifiers.SourceDamage += rangedBoost;

                    if (rangedBoost >= FrostArmorSetChange.ProximityBoost * 0.5f)
                    {
                        float intensity = rangedBoost / FrostArmorSetChange.ProximityBoost;
                        SoundEngine.PlaySound(Cryogen.HitSound with { Volume = intensity - 0.2f }, Player.Center);
                    }
                    if (rangedBoost > 0f)
                    {
                        int count = (int)(30f * rangedBoost);
                        for (int i = 0; i < count; i++)
                        {
                            Vector2 velocity = Main.rand.NextVector2Unit() * (5f + 100f * rangedBoost);
                            float scale = Main.rand.NextFloat(0.5f, 1f) + 0.5f * rangedBoost / FrostArmorSetChange.ProximityBoost;
                            Particle sparkle = new CritSpark(target.Center, velocity, Color.White, Color.DodgerBlue, scale, 15, 0.1f, scale * 2f);
                            GeneralParticleHandler.SpawnParticle(sparkle);
                        }
                    }
                }
            }
        }

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref NPC.HitModifiers modifiers)/* tModPorter If you don't need the Projectile, consider using ModifyHitNPC instead */
        {
            if (proj.npcProj || proj.trap)
                return;
                
            modifiers.CritDamage += critDamage;

            // All Calamity multipliers are added together to prevent insane exponential stacking
            float totalDamageMult = 1f;

            // Rippers are always checked for application, because there are ways to get rippers outside of Rev now
            CalamityUtils.ApplyRippersToDamage(this, proj.IsTrueMelee(), ref totalDamageMult);

            // Demonshade enrage
            if (enraged)
                totalDamageMult += DemonshadeHelm.MultDamageBoost;
            // Withering enchantment when it's draining your HP
            if (witheredDebuff && witheringWeaponEnchant)
                totalDamageMult += 0.6f;

            // Apply all Calamity multipliers as a sum total to TML New Damage in a single step
            modifiers.SourceDamage *= totalDamageMult;

            // 01JUN2024: Ozzatron: apply Yellow Candle "chip damage" as a dirty modifier
            // The registration of the dirty modifier is conditional to ensure it doesn't apply to "near invincible" targets
            //
            // FinalDamage cannot be used for the intended effect because there is no way to access the actual damage of the hit
            CalamityGlobalNPC cgn = target.Calamity();
            if (yellowCandle && cgn.DR < 0.99f && target.takenDamageMultiplier > 0.05f)
                modifiers.ModifyHitInfo += YellowCandleBuff.ModifyHitInfo_Spite;

            // Stealth strike damage multipliers are applied here.
            // TODO -- stealth should be its own damage class and this should be applied as player StealthDamage *= XYZ
            if (proj.Calamity().stealthStrike && proj.CountsAsClass<RogueDamageClass>())
                modifiers.SourceDamage *= (float)bonusStealthDamage + 1; // Default bonusStealthDamage is 0, a 1 has to be added to take the damage of the weapon.

            // Screwdriver adds 5% bonus damage to all piercing projectiles.
            if (screwdriver)
            {
                if (proj.penetrate > 1 || proj.penetrate == -1)
                    modifiers.ScalingBonusDamage += Screwdriver.PiercingDamageBuff;
            }

            if (Player.Calamity().scionsCurio && proj.CountsAsClass<RangedDamageClass>())
                target.Calamity().scionsCurioEffected = true;

            // Frost Armor's rework gives +X% melee damage and +Y% ranged damage based on distance, where X+Y = 15.
            if (frostSet)
            {
                // 0f = point blank, 1f = max range or further
                float DistanceInterpolant = Utils.GetLerpValue(FrostArmorSetChange.MinDistance, FrostArmorSetChange.MaxDistance, target.Distance(Main.LocalPlayer.Center), true);

                if (proj.CountsAsClass<MeleeDamageClass>())
                {
                    float meleeBoost = MathHelper.Lerp(0f, FrostArmorSetChange.ProximityBoost, 1 - DistanceInterpolant);
                    modifiers.SourceDamage += meleeBoost;

                    if (meleeBoost >= FrostArmorSetChange.ProximityBoost * 0.5f)
                    {
                        float intensity = meleeBoost / FrostArmorSetChange.ProximityBoost;
                        SoundEngine.PlaySound(Cryogen.HitSound with { Volume = intensity - 0.2f }, Player.Center);
                    }
                    if (meleeBoost > 0f)
                    {
                        int count = (int)(30f * meleeBoost);
                        for (int i = 0; i < count; i++)
                        {
                            Vector2 velocity = Main.rand.NextVector2Unit() * (5f + 100f * meleeBoost);
                            float scale = Main.rand.NextFloat(0.5f, 1f) + 0.5f * meleeBoost / FrostArmorSetChange.ProximityBoost;
                            Particle sparkle = new CritSpark(target.Center, velocity, Color.White, Color.DodgerBlue, scale, 15, 0.1f, scale * 2f);
                            GeneralParticleHandler.SpawnParticle(sparkle);
                        }
                    }
                }
                else if (proj.CountsAsClass<RangedDamageClass>())
                {
                    float rangedBoost = MathHelper.Lerp(0f, FrostArmorSetChange.ProximityBoost, DistanceInterpolant);
                    modifiers.SourceDamage += rangedBoost;

                    if (rangedBoost >= FrostArmorSetChange.ProximityBoost * 0.5f)
                    {
                        float intensity = rangedBoost / FrostArmorSetChange.ProximityBoost;
                        SoundEngine.PlaySound(Cryogen.HitSound with { Volume = intensity - 0.2f }, Player.Center);
                    }
                    if (rangedBoost > 0f)
                    {
                        int count = (int)(30f * rangedBoost);
                        for (int i = 0; i < count; i++)
                        {
                            Vector2 velocity = Main.rand.NextVector2Unit() * (5f + 100f * rangedBoost);
                            float scale = Main.rand.NextFloat(0.5f, 1f) + 0.5f * rangedBoost / FrostArmorSetChange.ProximityBoost;
                            Particle sparkle = new CritSpark(target.Center, velocity, Color.White, Color.DodgerBlue, scale, 15, 0.1f, scale * 2f);
                            GeneralParticleHandler.SpawnParticle(sparkle);
                        }
                    }
                }
            }

            // SUMMONER CROSS CLASS NERF IS APPLIED HERE
            //
            // There are several ways to negate the summoner cross class nerf:
            // - Wearing Forbidden armor and using a magic weapon
            // - Wearing Fearmonger armor
            // - Wearing Gem Tech armor and having the Blue Gem active
            // - Using Profaned Soul Crystal
            // - During the Old One's Army event it's disabled by default
            bool isSummon = proj.CountsAsClass<SummonDamageClass>();
            if (isSummon)
            {
                Item heldItem = Player.ActiveItem();

                if (CalamityUtils.ShouldTriggerSummonPenalty(Player, heldItem) && !CalamityProjectileSets.MinionWhichIgnoresSummonerNerf[proj.type])
                    modifiers.FinalDamage *= BalancingConstants.SummonerCrossClassNerf;
            }
        }
        #endregion

        #region Modify Hit By NPC
        public override void ModifyHitByNPC(NPC npc, ref Player.HurtModifiers modifiers)
        {
            if (npc.Calamity().antlionCloudDebuffTimer > 0)
                modifiers.SourceDamage *= AntlionSkewer.CloudDamageDebuffMult;

            // Enemies deal less contact damage while sick, due to being weakened.
            if (npc.poisoned)
            {
                float damageReductionFromPoison = (float)((npc.Calamity().irradiated ? npc.Calamity().irradiatedContactBoost : 1) * 0.05f);
                if (npc.Calamity().VulnerableToSickness.HasValue)
                {
                    if (npc.Calamity().VulnerableToSickness.Value)
                        damageReductionFromPoison *= 2f;
                    else
                        damageReductionFromPoison /= 2f;
                }
                damageReductionFromPoison = 1f - damageReductionFromPoison;

                modifiers.SourceDamage *= damageReductionFromPoison;
            }

            if (npc.venom)
            {
                float damageReductionFromVenom = (float)((npc.Calamity().irradiated ? npc.Calamity().irradiatedContactBoost : 1) * 0.05f);
                if (npc.Calamity().VulnerableToSickness.HasValue)
                {
                    if (npc.Calamity().VulnerableToSickness.Value)
                        damageReductionFromVenom *= 2f;
                    else
                        damageReductionFromVenom /= 2f;
                }
                damageReductionFromVenom = 1f - damageReductionFromVenom;

                modifiers.SourceDamage *= damageReductionFromVenom;
            }

            if (npc.Calamity().astralInfection)
            {
                float damageReductionFromAstralInfection = (float)((npc.Calamity().irradiated ? npc.Calamity().irradiatedContactBoost : 1) * 0.05f);
                if (npc.Calamity().VulnerableToSickness.HasValue)
                {
                    if (npc.Calamity().VulnerableToSickness.Value)
                        damageReductionFromAstralInfection *= 2f;
                    else
                        damageReductionFromAstralInfection /= 2f;
                }
                damageReductionFromAstralInfection = 1f - damageReductionFromAstralInfection;

                modifiers.SourceDamage *= damageReductionFromAstralInfection;
            }

            if (npc.Calamity().plague)
            {
                float damageReductionFromPlague = (float)((npc.Calamity().irradiated? npc.Calamity().irradiatedContactBoost : 1) * 0.05f);
                if (npc.Calamity().VulnerableToSickness.HasValue)
                {
                    if (npc.Calamity().VulnerableToSickness.Value)
                        damageReductionFromPlague *= 2f;
                    else
                        damageReductionFromPlague /= 2f;
                }
                damageReductionFromPlague = 1f - damageReductionFromPlague;

                modifiers.SourceDamage *= damageReductionFromPlague;
            }

            if (npc.Calamity().whisperingDeath)
            {
                float damageReductionFromWhisperingDeath = (float)((npc.Calamity().irradiated ? npc.Calamity().irradiatedContactBoost : 1) * 0.1f);
                if (npc.Calamity().VulnerableToSickness.HasValue)
                {
                    if (npc.Calamity().VulnerableToSickness.Value)
                        damageReductionFromWhisperingDeath *= 2f;
                    else
                        damageReductionFromWhisperingDeath /= 2f;
                }
                damageReductionFromWhisperingDeath = 1f - damageReductionFromWhisperingDeath;

                modifiers.SourceDamage *= damageReductionFromWhisperingDeath;
            }

            if (trueVHex)
                modifiers.SourceDamage *= 1.15f;

            //
            // At this point, the player is guaranteed to be hit if there is no dodge.
            // The amount of damage that will be dealt is yet to be determined.
            //

            // Can't have any cooldowns here because dodges grrrrr....
            if (fleshTotem && !Player.HasCooldown(Cooldowns.FleshTotem.ID) && TotalEnergyShielding <= 0)
                contactDamageReduction += 0.5;

            if (tarragonCloak && tarraMelee && !Player.HasCooldown(Cooldowns.TarragonCloak.ID))
                contactDamageReduction += Buffs.StatBuffs.TarragonCloak.ContactDamageReduction;

            if (bloodflareMelee && bloodflareFrenzy && !Player.HasCooldown(BloodflareFrenzy.ID))
                contactDamageReduction += 0.5;

            if (npc.Calamity().temporalSadness)
                contactDamageReduction += 0.5;

            if (eskimoSet)
            {
                if (npc.coldDamage)
                    contactDamageReduction += 0.1;
            }

            if (trinketOfChiBuff)
                contactDamageReduction += 0.08;

            if (abyssalDivingSuitPlates)
                contactDamageReduction += AbyssalDivingSuit.PlatesAllDamageReduction - abyssalDivingSuitPlateHits * AbyssalDivingSuit.PlatesHitDecay;

            if (aquaticHeartIce)
                contactDamageReduction += AquaticHeart.IceShieldAllDamageReduction;

            if (encased)
                contactDamageReduction += PermafrostsConcoction.EncasedAllDamageReduction;

            if (Player.ownedProjectileCounts[ModContent.ProjectileType<EnergyShell>()] > 0 && Player.ActiveItem().type == ModContent.ItemType<LionHeart>())
                contactDamageReduction += 0.5;

            bool lifeAndShieldCondition = Player.statLife >= Player.statLifeMax2 && (!HasAnyEnergyShield || TotalEnergyShielding >= TotalMaxShieldDurability);
            if (theBee && theBeeCooldown <= 0 && lifeAndShieldCondition)
            {
                contactDamageReduction += 0.5;
                shouldTriggerBeeCooldown = true;
            }

            // Apply Adrenaline DR if available
            if (AdrenalineEnabled)
            {
                bool fullAdrenWithoutDH = !draedonsHeart && (adrenaline == adrenalineMax) && !adrenalineModeActive;
                bool usingNanomachinesWithDH = draedonsHeart && adrenalineModeActive;

                // 18AUG2023: Ozzatron: Adrenaline DR does not apply if you have energy shields active.
                // Otherwise, it becomes almost impossible to break energy shields due to Adrenaline DR.
                // If the shield never breaks, you never lose full Adrenaline, meaning you keep the DR forever and are functionally immortal.
                // This intentionally gives Adrenaline much-needed anti-synergy with energy shields, because they make gaining Adrenaline much safer.
                if ((fullAdrenWithoutDH || usingNanomachinesWithDH) && TotalEnergyShielding <= 0)
                    contactDamageReduction += this.GetAdrenalineDR();
            }

            if (Player.mount.Active && (Player.mount.Type == ModContent.MountType<RimehoundMount>() || Player.mount.Type == ModContent.MountType<OnyxExcavator>()) && Math.Abs(Player.velocity.X) > Player.mount.RunSpeed / 2f)
                contactDamageReduction += 0.1;

            if (vHex)
                contactDamageReduction -= 0.1;

            if (irradiated)
                contactDamageReduction -= 0.1;

            if (corrEffigy)
                contactDamageReduction -= CorruptionEffigy.DamageReductionLoss;

            // 10% is converted to 9%, 25% is converted to 20%, 50% is converted to 33%, 75% is converted to 43%, 100% is converted to 50%
            if (contactDamageReduction > 0D)
            {
                if (armorCrunch)
                    contactDamageReduction *= (double)ArmorCrunch.MultiplicativeDamageReductionPlayer;
                if (crumble)
                    contactDamageReduction *= (double)Crumbling.MultiplicativeDamageReductionPlayer;

                // Contact damage reduction is reduced by DR Damage, which itself is proportional to defense damage
                // In FTW, as defense damage is uncapped, DR damage is also uncapped.
                int currentDefense = Player.GetCurrentDefense(false);
                if (totalDefenseDamage > 0 && currentDefense > 0)
                {
                    double drDamageRatio = CurrentDefenseDamage / (double)currentDefense;
                    if (!Main.getGoodWorld && drDamageRatio > 1D)
                        drDamageRatio = 1D;

                    contactDamageReduction *= 1D - drDamageRatio;
                    if (!Main.getGoodWorld && contactDamageReduction < 0D)
                        contactDamageReduction = 0D;
                }

                // Scale with base damage reduction
                if (Player.endurance > 0)
                    contactDamageReduction *= 1f - Player.endurance;

                contactDamageReduction = 1D / (1D + contactDamageReduction);
                modifiers.SourceDamage *= (float)contactDamageReduction;
            }

            if (Main.hardMode && Main.expertMode)
            {
                bool reduceChaosBallDamage = npc.type == NPCID.ChaosBall && !NPC.AnyNPCs(NPCID.GoblinSummoner);
                if (reduceChaosBallDamage || npc.type == NPCID.ChaosBallTim || npc.type == NPCID.BurningSphere || npc.type == NPCID.WaterSphere)
                    modifiers.SourceDamage *= 0.6f;
            }
        }
        #endregion

        #region Modify Hit By Proj
        public override void ModifyHitByProjectile(Projectile proj, ref Player.HurtModifiers modifiers)
        {
            if (!CalamityProjectileSets.ShouldNotBeReflected[proj.type] && proj.active && !proj.friendly && proj.hostile && proj.damage > 0 && !modifiers.PvP)
            {
                double dodgeDamageGateValuePercent = 0.05;
                int dodgeDamageGateValue = (int)Math.Round(Player.statLifeMax2 * dodgeDamageGateValuePercent);

                // Reflects count as dodges. They share the timer and can be disabled by Armageddon right click.
                if (!disableAllDodges && !Player.HasCooldown(GlobalDodge.ID) && proj.damage >= dodgeDamageGateValue)
                {
                    double maxCooldownDurationDamagePercent = 0.5;
                    int maxCooldownDurationDamageValue = (int)Math.Round(Player.statLifeMax2 * (maxCooldownDurationDamagePercent - dodgeDamageGateValuePercent));

                    // Just in case...
                    if (maxCooldownDurationDamageValue <= 0)
                        maxCooldownDurationDamageValue = 1;

                    float cooldownDurationScalar = MathHelper.Clamp((proj.damage - dodgeDamageGateValue) / (float)maxCooldownDurationDamageValue, 0f, 1f);

                    // The Evolution
                    if (evolution)
                    {
                        proj.hostile = false;
                        proj.friendly = true;
                        proj.damage *= 10;
                        proj.velocity *= -2f;
                        proj.extraUpdates += 1;
                        proj.penetrate = 1;

                        // 17APR2024: Ozzatron: The Evolution is a reflect which also functions as a dodge. It uses vanilla dodge iframes and benefits from Cross Necklace.
                        int evolutionIFrames = Player.ComputeReflectIFrames();
                        Player.GiveUniversalIFrames(evolutionIFrames, true);

                        modifiers.Cancel();
                        evolutionLifeRegenCounter = 600;
                        projTypeJustHitBy = proj.type;

                        int cooldownDuration = (int)MathHelper.Lerp(BalancingConstants.EvolutionReflectCooldownMin, BalancingConstants.EvolutionReflectCooldownMax, cooldownDurationScalar);
                        Player.AddCooldown(GlobalDodge.ID, cooldownDuration);

                        return;
                    }
                    else if (daedalusReflect)
                    {
                        proj.hostile = false;
                        proj.friendly = true;
                        proj.velocity *= -1f;
                        proj.penetrate = 1;

                        // 17APR2024: Ozzatron: The Daedalus Reflect set bonus also functions as a dodge. It uses vanilla dodge iframes and benefits from Cross Necklace.
                        int daedalusReflectIFrames = Player.ComputeReflectIFrames();
                        Player.GiveUniversalIFrames(daedalusReflectIFrames, true);
                        modifiers.Cancel();

                        int cooldownDuration = (int)MathHelper.Lerp(DaedalusHeadMelee.ReflectCooldownMin, DaedalusHeadMelee.ReflectCooldownMax, cooldownDurationScalar);
                        Player.AddCooldown(GlobalDodge.ID, cooldownDuration);
                    }
                }
            }

            if (phantomicArtifact && Player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.Summon.PhantomicShield>()] != 0)
            {
                Projectile pro = Main.projectile.AsEnumerable().Where(projectile => projectile.friendly && projectile.owner == Player.whoAmI && projectile.type == ModContent.ProjectileType<Projectiles.Summon.PhantomicShield>()).First();
                phantomicBulwarkCooldown = 1800; // 30 second cooldown
                pro.Kill();
                projectileDamageReduction += 0.2;
            }

            if (trueVHex)
                modifiers.SourceDamage *= 1.15f;

            if (auralisAuroraCounter >= 300)
            {
                modifiers.SourceDamage.Flat -= 100;

                auralisAuroraCounter = 0;
                auralisAuroraCooldown = CalamityUtils.SecondsToFrames(30f);
            }

            // Torch God does 1 damage but inflicts a random fire debuff
            if (proj.type == ProjectileID.TorchGod)
                modifiers.SetMaxDamage(1);

            // Reduce damage from vanilla traps

            // Explosives
            // 350 damage
            if (proj.type == ProjectileID.Explosives)
                modifiers.SourceDamage *= 0.7f;

            // Rolling Cacti
            // 42 in normal, 84 in expert for cactus
            // 36 in normal, 72 in expert for spikes
            else if (proj.type == ProjectileID.RollingCactus || proj.type == ProjectileID.RollingCactusSpike)
                modifiers.SourceDamage *= 0.6f;

            // Boulders
            if (Main.expertMode && !areThereAnyDamnBosses)
            {
                // 140 in normal, 210 in expert, 315 in master for boulder
                // 104 in normal, 156 in expert, 234 in master for mini boulder
                if (proj.type == ProjectileID.Boulder || proj.type == ProjectileID.MiniBoulder || proj.type == ProjectileID.BouncyBoulder || proj.type == ProjectileID.LifeCrystalBoulder)
                    modifiers.SourceDamage *= 0.75f;
            }

            bool isFallingBlock = (proj.type == ProjectileID.SandBallFalling) || (proj.type == ProjectileID.SiltBall) || (proj.type == ProjectileID.AshBallFalling) ||
                (proj.type == ProjectileID.CrimsandBallFalling) || (proj.type == ProjectileID.EbonsandBallFalling) || (proj.type == ProjectileID.PearlSandBallFalling) ||
                (proj.type == ProjectileID.ShellPileFalling) || (proj.type == ProjectileID.SlushBall) || (proj.type == ModContent.ProjectileType<AstralSandBallFalling>());
            if (Player.Calamity().fallingBlockProtection && isFallingBlock)
                modifiers.Cancel();

            bool isIgnoredTrap = (proj.type == ProjectileID.PoisonDartTrap) || (proj.type == ProjectileID.VenomDartTrap) || (proj.type == ProjectileID.PoisonDart);
            if (Player.Calamity().trapProtection && isIgnoredTrap)
                modifiers.Cancel();

            bool isReducedTrap = (proj.trap || proj.type == ProjectileID.RollingCactusSpike || proj.type == ProjectileID.Landmine) && !isIgnoredTrap;
            if (Player.Calamity().trapProtection && isReducedTrap)
                modifiers.SourceDamage *= 0.35f;

            // Reduce damage dealt by rainbow trails
            if (proj.type == ProjectileID.HallowBossLastingRainbow)
            {
                // Find the oldPos of the projectile that is intersecting the player hitbox.
                Rectangle hitbox = proj.Hitbox;
                int trailLength = 80;
                int startOfDamageFalloff = 20;
                for (int k = 0; k < trailLength; k += 2)
                {
                    Vector2 trailHitbox = proj.oldPos[k];
                    if (!(trailHitbox == Vector2.Zero))
                    {
                        hitbox.X = (int)trailHitbox.X;
                        hitbox.Y = (int)trailHitbox.Y;

                        if (hitbox.Intersects(Player.Hitbox))
                        {
                            if (k > startOfDamageFalloff)
                                modifiers.SourceDamage *= EmpressofLightAI.EverlastingRainbowTrailDamageMult;

                            break;
                        }
                    }
                }
            }

            //
            // At this point, the player is guaranteed to be hit if there is no dodge.
            // The amount of damage that will be dealt is yet to be determined.
            //

            if (evolution)
            {
                if (proj.type == projTypeJustHitBy)
                    projectileDamageReduction += 0.25;
            }

            if (trinketOfChiBuff)
                projectileDamageReduction += 0.08;

            if (abyssalDivingSuitPlates)
                projectileDamageReduction += AbyssalDivingSuit.PlatesAllDamageReduction - abyssalDivingSuitPlateHits * AbyssalDivingSuit.PlatesHitDecay;

            if (aquaticHeartIce)
                projectileDamageReduction += AquaticHeart.IceShieldAllDamageReduction;

            if (encased)
                projectileDamageReduction += PermafrostsConcoction.EncasedAllDamageReduction;

            if (Player.ownedProjectileCounts[ModContent.ProjectileType<EnergyShell>()] > 0 && Player.ActiveItem().type == ModContent.ItemType<LionHeart>())
                projectileDamageReduction += 0.5;

            bool lifeAndShieldCondition = Player.statLife >= Player.statLifeMax2 && (!HasAnyEnergyShield || TotalEnergyShielding >= TotalMaxShieldDurability);
            if (theBee && theBeeCooldown <= 0 && lifeAndShieldCondition)
            {
                projectileDamageReduction += 0.5;
                shouldTriggerBeeCooldown = true;
            }

            // Apply Adrenaline DR if available
            if (AdrenalineEnabled)
            {
                bool fullAdrenWithoutDH = !draedonsHeart && (adrenaline == adrenalineMax) && !adrenalineModeActive;
                bool usingNanomachinesWithDH = draedonsHeart && adrenalineModeActive;

                // 18AUG2023: Ozzatron: Adrenaline DR does not apply if you have energy shields active.
                // Otherwise, it becomes almost impossible to break energy shields due to Adrenaline DR.
                // If the shield never breaks, you never lose full Adrenaline, meaning you keep the DR forever and are functionally immortal.
                // This intentionally gives Adrenaline much-needed anti-synergy with energy shields, because they make gaining Adrenaline much safer.
                if ((fullAdrenWithoutDH || usingNanomachinesWithDH) && TotalEnergyShielding <= 0)
                    projectileDamageReduction += this.GetAdrenalineDR();
            }

            if (Player.mount.Active && (Player.mount.Type == ModContent.MountType<RimehoundMount>() || Player.mount.Type == ModContent.MountType<OnyxExcavator>()) && Math.Abs(Player.velocity.X) > Player.mount.RunSpeed / 2f)
                projectileDamageReduction += 0.1;

            // Damage reduction from Shield of the High Ruler if facing the projectile that just hit.
            // If the projectile is in the exact center of the player on the X axis YOU GET NOTHING, GOOD DAY, SIR!
            if (copyrightInfringementShield)
            {
                bool projectileRight = (Player.Center.X - proj.Center.X) < 0f;
                bool projectileLeft = (Player.Center.X - proj.Center.X) > 0f;
                if (Player.direction == 1)
                {
                    if (projectileRight)
                        projectileDamageReduction += 0.15;
                }
                else
                {
                    if (projectileLeft)
                        projectileDamageReduction += 0.15;
                }
            }

            if (vHex)
                projectileDamageReduction -= 0.1;

            if (irradiated)
                projectileDamageReduction -= 0.1;

            if (corrEffigy)
                projectileDamageReduction -= CorruptionEffigy.DamageReductionLoss;

            // 10% is converted to 9%, 25% is converted to 20%, 50% is converted to 33%, 75% is converted to 43%, 100% is converted to 50%
            if (projectileDamageReduction > 0D)
            {
                if (armorCrunch)
                    projectileDamageReduction *= (double)ArmorCrunch.MultiplicativeDamageReductionPlayer;
                if (crumble)
                    projectileDamageReduction *= (double)Crumbling.MultiplicativeDamageReductionPlayer;

                // Projectile damage reduction is reduced by DR Damage, which itself is proportional to defense damage
                int currentDefense = Player.GetCurrentDefense(false);
                if (totalDefenseDamage > 0 && currentDefense > 0)
                {
                    double drDamageRatio = CurrentDefenseDamage / (double)currentDefense;
                    if (drDamageRatio > 1D)
                        drDamageRatio = 1D;

                    projectileDamageReduction *= 1D - drDamageRatio;

                    if (projectileDamageReduction < 0D)
                        projectileDamageReduction = 0D;
                }

                // Scale with base damage reduction
                if (Player.endurance > 0)
                    projectileDamageReduction *= 1f - Player.endurance;

                projectileDamageReduction = 1D / (1D + projectileDamageReduction);
                modifiers.SourceDamage *= (float)projectileDamageReduction;
            }
        }
        #endregion

        #region On Hit By NPC / Projectile
        public override void OnHitByNPC(NPC npc, Player.HurtInfo hurtInfo)
        {
            // Check if the player has iframes for the sake of avoiding defense damage.
            bool hasIFrames = false;
            for (int i = 0; i < Player.hurtCooldowns.Length; i++)
                if (Player.hurtCooldowns[i] > 0)
                    hasIFrames = true;

            // If this NPC deals defense damage with contact damage, then mark the player to take defense damage.
            // Defense damage is not applied if the player has iframes, or is in Journey god mode.
            if (!hasIFrames && !Player.creativeGodMode)
                nextHitDealsDefenseDamage |= npc.Calamity().canBreakPlayerDefense;

            // ModifyHit (Flesh Totem effect happens here) -> Hurt (includes dodges) -> OnHit
            // As such, to avoid cooldowns proccing from dodge hits, do it here
            if (fleshTotem && !Player.HasCooldown(Cooldowns.FleshTotem.ID) && hurtInfo.Damage > 0)
                Player.AddCooldown(Cooldowns.FleshTotem.ID, CalamityUtils.SecondsToFrames(20));

            if (NPC.AnyNPCs(ModContent.NPCType<THELORDE>()))
                Player.AddBuff(ModContent.BuffType<NOU>(), 15, true);

            if (crawCarapace)
            {
                npc.AddBuff(ModContent.BuffType<Crumbling>(), 900);
                Vector2 pushVel = Utils.DirectionTo(Player.Center, npc.Center) * 7;
                if (!npc.dontTakeDamage)
                {
                    int onHitDamage = (int)Player.GetBestClassDamage().ApplyTo(CrawCarapace.ThornsDamage);
                    Projectile revenge = Projectile.NewProjectileDirect(Player.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<DirectStrike>(), onHitDamage, 0f, Player.whoAmI, npc.whoAmI, pushVel.X, pushVel.Y);
                }
                SoundEngine.PlaySound(SoundID.NPCHit33 with { Volume = 0.5f }, Player.Center);

                for (int i = 0; i < 10; i++)
                {
                    float accuracy = Main.rand.NextFloat(-0.4f, 0.4f);
                    float powerMult = (1 - Math.Abs(accuracy));

                    Vector2 dustVel = (pushVel).SafeNormalize(Vector2.UnitY).RotatedBy(accuracy * 2) * Main.rand.NextFloat(4, 7) * powerMult;
                    Vector2 fxPos = Player.Center + dustVel;

                    Dust dust = Dust.NewDustPerfect(fxPos, Main.rand.NextBool(4) ? 249 : 115, dustVel, 0, default, Main.rand.NextFloat(0.75f, 1.2f));
                    dust.noGravity = true;
                    dust.noGravity = true;
                    dust.fadeIn = 1f;
                }
            }

            if (baroclaw)
            {
                Vector2 pushVel = Utils.DirectionTo(Player.Center, npc.Center) * 15;
                if (!npc.dontTakeDamage)
                {
                    int onHitDamage = (int)Player.GetBestClassDamage().ApplyTo(Baroclaw.ThornsDamage);
                    Projectile revenge = Projectile.NewProjectileDirect(Player.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<DirectStrike>(), onHitDamage, -1f, Player.whoAmI, npc.whoAmI, pushVel.X, pushVel.Y);
                    
                    npc.AddBuff(ModContent.BuffType<ArmorCrunch>(), 900);
                    npc.AddBuff(ModContent.BuffType<CrushDepth>(), 900);
                }
                SoundEngine.PlaySound(BaroclawHit, Player.Center);

                for (int i = 0; i < 17; i++)
                {
                    float accuracy = Main.rand.NextFloat(-0.55f, 0.55f);
                    float powerMult = (1 - Math.Abs(accuracy));
                    Vector2 fxVel = (pushVel).SafeNormalize(Vector2.UnitY).RotatedBy(accuracy) * Main.rand.NextFloat(5, 12) * powerMult;
                    Vector2 dustVel = (pushVel).SafeNormalize(Vector2.UnitY).RotatedBy(accuracy * 2) * Main.rand.NextFloat(10, 20) * powerMult;
                    Vector2 fxPos = Player.Center + fxVel;
                    Color fxColor = Color.Lerp(Color.RoyalBlue, Color.DarkBlue, Main.rand.NextFloat(1f));

                    Particle fx = new CustomSpark(fxPos, fxVel, "CalamityMod/Particles/PointParticle", false, (int)(Main.rand.Next(22, 40 + 1) * powerMult), Main.rand.NextFloat(1.95f, 2.2f) * powerMult, fxColor, new Vector2(0.5f, 1.1f), extraRotation: 0, shrinkSpeed: Main.rand.NextFloat(0.1f, 0.3f) + (1 - powerMult) * 0.3f);
                    GeneralParticleHandler.SpawnParticle(fx);

                    if (i % 3 == 0)
                    {
                        Dust dust = Dust.NewDustPerfect(fxPos, 278, dustVel, 0, default, Main.rand.NextFloat(0.75f, 1.1f));
                        dust.noGravity = true;
                        dust.color = Color.Gold;
                        dust.noGravity = false;
                    }
                }
            }

            if (absorber)
            {
                Vector2 pushVel = Utils.DirectionTo(Player.Center, npc.Center) * 22;
                if (!npc.dontTakeDamage)
                {
                    int onHitDamage = (int)Player.GetBestClassDamage().ApplyTo(TheAbsorber.ThornsDamage);
                    Projectile revenge = Projectile.NewProjectileDirect(Player.GetSource_FromThis(), npc.Center, Vector2.Zero, ModContent.ProjectileType<DirectStrike>(), onHitDamage, -1f, Player.whoAmI, npc.whoAmI, pushVel.X, pushVel.Y);
                    
                    npc.AddBuff(ModContent.BuffType<AbsorberAffliction>(), 900);
                }
                SoundEngine.PlaySound(AbsorberHit, Player.Center);

                for (int i = 0; i < 25; i++)
                {
                    float accuracy = Main.rand.NextFloat(-0.7f, 0.7f);
                    float powerMult = (1 - Math.Abs(accuracy));
                    Vector2 fxVel = (pushVel).SafeNormalize(Vector2.UnitY).RotatedBy(accuracy) * Main.rand.NextFloat(10, 18) * powerMult;
                    Vector2 dustVel = (pushVel).SafeNormalize(Vector2.UnitY).RotatedBy(accuracy * 2) * Main.rand.NextFloat(15, 30) * powerMult;
                    Vector2 fxPos = Player.Center + fxVel;
                    Color fxColor = Color.Lerp(Color.DarkSeaGreen, Color.MediumSeaGreen, Main.rand.NextFloat(1f));

                    Particle fx = new CustomSpark(fxPos, fxVel, "CalamityMod/Particles/Sparkle", false, (int)(Main.rand.Next(32, 50 + 1) * powerMult), Main.rand.NextFloat(2.25f, 2.5f) * powerMult, fxColor, new Vector2(0.5f, 1.1f), extraRotation: 0, shrinkSpeed: Main.rand.NextFloat(0.1f, 0.3f) + (1 - powerMult) * 0.3f);
                    GeneralParticleHandler.SpawnParticle(fx);

                    Dust dust = Dust.NewDustPerfect(fxPos, ModContent.DustType<LightDust>(), dustVel, 0, default, Main.rand.NextFloat(0.95f, 2.1f));
                    dust.noGravity = true;
                    dust.color = fxColor;
                }
            }

            OnHitByCombat(hurtInfo);
        }

        public override void OnHitByProjectile(Projectile proj, Player.HurtInfo hurtInfo)
        {
            // Check if the player has iframes for the sake of avoiding defense damage.
            bool hasIFrames = false;
            for (int i = 0; i < Player.hurtCooldowns.Length; i++)
                if (Player.hurtCooldowns[i] > 0)
                    hasIFrames = true;

            // If this projectile is capable of dealing defense damage, then mark the player to take defense damage.
            // Defense damage is not applied if the player has iframes, or is in Journey god mode.
            if (!hasIFrames && !Player.creativeGodMode)
                nextHitDealsDefenseDamage |= proj.Calamity().DealsDefenseDamage;

            // CIT 15FEB2025: This code previously tried to use Main.npc[proj.owner] to find the NPC the projectile came from.
            // This doesn't work because the server owns NPC-spawned projectiles.
            // All projectiles spawned from hostile NPCs are now fed the index of the NPC into ParentNPCIndex, so that these can work.
            if (!proj.friendly && hurtInfo.Damage > 0 && proj.Calamity().ParentNPCIndex != -1)
            {
                if (Main.npc[proj.Calamity().ParentNPCIndex].active)
                {
                    if (sulphurSet)
                        Main.npc[proj.Calamity().ParentNPCIndex].AddBuff(BuffID.Poisoned, SulphurousHelmet.SetBonusPoisonDuration);
                    if (ilSpark)
                        Main.npc[proj.Calamity().ParentNPCIndex].Calamity().shocked = 120;
                }   
            }

            if (proj.hostile && hurtInfo.Damage > 0)
            {
                if (proj.type == ProjectileID.TorchGod)
                {
                    int fireDebuffTypes = CalamityWorld.death ? 9 : CalamityWorld.revenge ? 7 : Main.expertMode ? 5 : 3;
                    int choice = Main.zenithWorld ? 9 : Main.rand.Next(fireDebuffTypes);
                    switch (choice)
                    {
                        case 0:
                            Player.AddBuff(BuffID.OnFire, 600);
                            break;

                        case 1:
                            Player.AddBuff(BuffID.Frostburn, 300);
                            break;

                        case 2:
                            Player.AddBuff(BuffID.CursedInferno, 300);
                            break;

                        case 3:
                            Player.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 180);
                            break;

                        case 4:
                            Player.AddBuff(ModContent.BuffType<Shadowflame>(), 150);
                            break;

                        case 5:
                            Player.AddBuff(ModContent.BuffType<GodSlayerInferno>(), 100);
                            break;

                        case 6:
                            Player.AddBuff(ModContent.BuffType<HolyFlames>(), 200);
                            break;

                        case 7:
                            Player.AddBuff(ModContent.BuffType<VulnerabilityHex>(), 300);
                            break;

                        case 8:
                            Player.AddBuff(ModContent.BuffType<Dragonfire>(), 150);
                            break;

                        case 9:
                            Player.AddBuff(ModContent.BuffType<MiracleBlight>(), 300);
                            break;
                    }
                }
                else if (proj.type == ProjectileID.Explosives)
                {
                    Player.AddBuff(BuffID.OnFire, 600);
                }
                else if (proj.type == ProjectileID.Boulder)
                {
                    Player.AddBuff(BuffID.BrokenArmor, 600);
                }
                else if (proj.type == ProjectileID.DesertDjinnCurse)
                {
                    Player.AddBuff(BuffID.Cursed, 180);
                }
                else if (proj.type == ProjectileID.BloodNautilusShot)
                {
                    Player.AddBuff(ModContent.BuffType<BurningBlood>(), 240);
                }
                else if (proj.type == ProjectileID.BloodShot)
                {
                    Player.AddBuff(ModContent.BuffType<BurningBlood>(), 180);
                }
                else if (proj.type == ProjectileID.RuneBlast && Main.zenithWorld)
                {
                    Player.AddBuff(ModContent.BuffType<MiracleBlight>(), 600);
                }

                if (CalamityWorld.revenge)
                {
                    if (proj.type == ProjectileID.CursedFlameHostile || proj.type == ProjectileID.EyeFire)
                    {
                        // Guaranteed Cursed Inferno for 1 second (vanilla also has a 68.75% chance of Cursed Inferno for 2 to 3 seconds)
                        Player.AddBuff(BuffID.CursedInferno, 60);
                    }
                    else if (proj.type == ProjectileID.ThornBall)
                    {
                        Player.AddBuff(BuffID.Venom, 120);
                    }
                    else if (proj.type == ProjectileID.CultistBossFireBall)
                    {
                        Player.AddBuff(ModContent.BuffType<Daybroken>(), 180);
                    }
                    else if (proj.type == ProjectileID.CultistBossIceMist && proj.ai[1] == 1f) // Main ice mists only; no shards
                    {
                        Player.AddBuff(BuffID.Chilled, 180);
                    }
                    else if (proj.type == ProjectileID.CultistBossLightningOrbArc)
                    {
                        Player.AddBuff(BuffID.Electrified, 180);
                    }
                    else if (proj.type == ProjectileID.CultistBossFireBallClone)
                    {
                        Player.AddBuff(ModContent.BuffType<Shadowflame>(), 240);
                    }
                    else if (proj.type == ProjectileID.PhantasmalBolt || proj.type == ProjectileID.PhantasmalEye)
                    {
                        Player.AddBuff(ModContent.BuffType<Nightwither>(), 120);
                    }
                    else if (proj.type == ProjectileID.PhantasmalSphere)
                    {
                        Player.AddBuff(ModContent.BuffType<Nightwither>(), 180);
                    }
                    else if (proj.type == ProjectileID.PhantasmalDeathray)
                    {
                        Player.AddBuff(ModContent.BuffType<Nightwither>(), 240);
                    }
                }
            }

            if (NPC.AnyNPCs(ModContent.NPCType<THELORDE>()))
            {
                Player.AddBuff(ModContent.BuffType<NOU>(), 15, true);
            }

            OnHitByCombat(hurtInfo);
        }

        // Shortcut for applying hit effects when hit by either an NPC or projectile
        // Reminder that external sources ie. forcefully called hurt functions, hazards (thorns, spikes, lava) are not valid
        public void OnHitByCombat(Player.HurtInfo hurtInfo)
        {
            // Apply The Bee cooldown, must be applied here so that it does not apply on dodges
            if (theBee && shouldTriggerBeeCooldown)
            {
                shouldTriggerBeeCooldown = false;
                if (hurtInfo.Damage > 0)
                    theBeeCooldown = TheBee.CooldownLength;
            }

            if (rOfResilienceCooldown == 0 && rOfResilienceEffect > 0)
            {
                int cooldownTime = (Player.Calamity().profanedSoulRelicBuff ? 300 : 600);
                rOfResilienceCooldown = cooldownTime;
                Player.AddCooldown(Cooldowns.RelicOfResilienceCooldown.ID, cooldownTime);
                SoundStyle youGotHit = new("CalamityMod/Sounds/Custom/ProfanedGuardians/GuardianRockShieldActivate");
                SoundEngine.PlaySound(youGotHit with { Volume = 0.7f, Pitch = -0.1f }, Player.Center);
            }

            if (alchFlask)
            {
                for (int i = 0; i < 9; i++)
                {
                    int seekerDamage = (int)Player.GetBestClassDamage().ApplyTo(15);

                    Projectile bee = Projectile.NewProjectileDirect(Player.GetSource_FromThis(), Player.Center, new Vector2(5, 5).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1.2f), ModContent.ProjectileType<BasicPlagueBee>(), seekerDamage, 0f, Player.whoAmI, -20, 30, 2);
                    bee.ArmorPenetration = 35;
                    bee.penetrate = 6;
                    bee.extraUpdates = 2;
                    bee.timeLeft = 600;
                }
                Player.AddBuff(BuffID.Honey, 900);
            }

            if (ursaSergeant)
            {
                ursaSergeantCooldown = (int)MathHelper.Clamp(ursaSergeantCooldown - 180, 0, 300);
                Player.AddBuff(ModContent.BuffType<AstralInfectionDebuff>(), 150);
                for (int i = 0; i < 9; i++)
                {
                    Particle spark2 = new LineParticle(Player.Center, new Vector2(8, 8).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1f), false, 20, Main.rand.NextFloat(0.5f, 1.1f), Main.rand.NextBool() ? Color.Coral : Color.DarkTurquoise);
                    GeneralParticleHandler.SpawnParticle(spark2);
                    Dust dust2 = Dust.NewDustPerfect(Player.Center, 267, new Vector2(8, 8).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1f));
                    dust2.scale = Main.rand.NextFloat(0.75f, 1.2f);
                    dust2.noGravity = true;
                    dust2.color = Main.rand.NextBool() ? Color.Coral : Color.DarkTurquoise;
                }
            }

            if (corrosiveSpine)
            {
                int cloudCount = 3;
                for (int i = 0; i < cloudCount; i++)
                {
                    float speed = 2f;
                    int damage = 40;
                    int cloud = Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.One.RotatedByRandom(MathHelper.TwoPi) * speed, ModContent.ProjectileType<ScourgeVenomCloud>(), damage, 0f, Player.whoAmI);
                    if (cloud.WithinBounds(Main.maxProjectiles))
                        Main.projectile[cloud].DamageType = DamageClass.Generic;
                }
            }
        }
        #endregion

        #region Free and Consumable Dodge Hooks
        public override bool FreeDodge(Player.HurtInfo info)
        {
            // If the incoming damage is somehow less than 1 (TML doesn't allow this, but...), the hit is completely ignored.
            if (info.Damage < 1)
                return true;

            // Silva armor revive provides complete immunity.
            if (silvaCountdown > 0 && hasSilvaEffect && silvaSet)
                return true;

            // If this hit was marked to be completely ignored due to shield absorption, then process Adrenaline changes and ignore it.
            if (freeDodgeFromShieldAbsorption)
            {
                freeDodgeFromShieldAbsorption = false;

                // 20FEB2024: Ozzatron: Hits fully absorbed by shields remove half of your current Adrenaline.
                // If using Draedon's Heart, it pauses for half the typical duration.
                LoseAdrenalineOnHurt(info, true);
                return true;
            }

            // Gravistar Sabaton fall ram gives you a free dodge as long as you're slamming through NPCs
            // This also strikes the NPCs as a side effect
            if (gSabatonFalling)
            {
                foreach (NPC n in Main.ActiveNPCs)
                {
                    // Ignore critters with the Guide to Critter Companionship
                    if (Player.dontHurtCritters && NPCID.Sets.CountsAsCritter[n.type])
                        continue;

                    if (!n.dontTakeDamage && !n.friendly && n.Calamity().dashImmunityTime[Player.whoAmI] <= 0)
                    {
                        Rectangle npcHitbox = n.getRect();
                        if ((Player.getRect()).Intersects(npcHitbox) && (n.noTileCollide || Collision.CanHit(Player.position, Player.width, Player.height, n.position, n.width, n.height)))
                        {
                            int damage = Player.CalcIntDamage<MeleeDamageClass>(InterstellarStompers.PassthroughDamage);

                            Projectile.NewProjectile(Player.GetSource_FromThis(), n.Center, Vector2.Zero, ModContent.ProjectileType<DirectStrike>(), damage, 0, Main.myPlayer);

                            // 17APR2024: Ozzatron: Gravistar Sabaton gives iframes when passing through enemies for projectile safety.
                            // This is a fixed and intentionally very low number of iframes, and is not boosted by Cross Necklace.
                            n.Calamity().dashImmunityTime[Player.whoAmI] = 4;
                            Player.GiveUniversalIFrames(InterstellarStompers.PassthroughIFrames, false);

                            return true;
                        }
                    }
                }
            }

            if (rOfDelivarenceRam)
            {
                foreach (NPC n in Main.ActiveNPCs)
                {
                    // Ignore critters with the Guide to Critter Companionship
                    if (Player.dontHurtCritters && NPCID.Sets.CountsAsCritter[n.type])
                        continue;

                    if (!n.dontTakeDamage && !n.friendly && n.Calamity().dashImmunityTime[Player.whoAmI] <= 0)
                    {
                        Rectangle npcHitbox = n.getRect();
                        if ((Player.getRect()).Intersects(npcHitbox) && (n.noTileCollide || Collision.CanHit(Player.position, Player.width, Player.height, n.position, n.width, n.height)))
                        {
                            // 17APR2024: Ozzatron: This item gives iframes when passing through enemies for projectile safety.
                            // This is a fixed and intentionally very low number of iframes, and is not boosted by Cross Necklace.
                            n.Calamity().dashImmunityTime[Player.whoAmI] = 4;
                            Player.GiveUniversalIFrames(InterstellarStompers.PassthroughIFrames, false);

                            return true;
                        }
                    }
                }
            }

            // If no other effects occurred, run vanilla code
            return base.FreeDodge(info);
        }

        public override bool ConsumableDodge(Player.HurtInfo info)
        {
            // Vanilla dodges are gated behind the global dodge cooldown
            // The dodges will only trigger if the player has taken greater than or equal to 5% of their max HP in damage
            double dodgeDamageGateValuePercent = 0.05;
            int dodgeDamageGateValue = (int)Math.Round(Player.statLifeMax2 * dodgeDamageGateValuePercent);

            // 14MAY2024: Ozzatron: Chalice of the Blood God now works with dodges
            int actualDamageTaken = chaliceOfTheBloodGod ? chaliceHitOriginalDamage : info.Damage;
            bool sufficientDamageForDodging = actualDamageTaken >= dodgeDamageGateValue;

            if (!Player.HasCooldown(GlobalDodge.ID) && sufficientDamageForDodging)
            {
                double maxCooldownDurationDamagePercent = 0.5;
                int maxCooldownDurationDamageValue = (int)Math.Round(Player.statLifeMax2 * (maxCooldownDurationDamagePercent - dodgeDamageGateValuePercent));

                // Just in case...
                if (maxCooldownDurationDamageValue <= 0)
                    maxCooldownDurationDamageValue = 1;

                float cooldownDurationScalar = MathHelper.Clamp((actualDamageTaken - dodgeDamageGateValue) / (float)maxCooldownDurationDamageValue, 0f, 1f);

                // Re-implementation of vanilla item Black Belt as a consumable dodge
                if (Player.whoAmI == Main.myPlayer && Player.blackBelt)
                {
                    Player.NinjaDodge();
                    int cooldownDuration = (int)MathHelper.Lerp(BalancingConstants.BeltDodgeCooldownMin, BalancingConstants.BeltDodgeCooldownMax, cooldownDurationScalar);
                    Player.AddCooldown(GlobalDodge.ID, cooldownDuration);
                    return true;
                }

                // Re-implementation of vanilla item Brain of Confusion as a consumable dodge
                if (Player.whoAmI == Main.myPlayer && Player.brainOfConfusionItem != null && !Player.brainOfConfusionItem.IsAir)
                {
                    Player.BrainOfConfusionDodge();
                    int cooldownTime = amalgam ?
                        (int)MathHelper.Lerp(BalancingConstants.AmalgamDodgeCooldownMin, BalancingConstants.AmalgamDodgeCooldownMax, cooldownDurationScalar) :
                        (int)MathHelper.Lerp(BalancingConstants.BrainDodgeCooldownMin, BalancingConstants.BrainDodgeCooldownMax, cooldownDurationScalar);
                    Player.AddCooldown(GlobalDodge.ID, cooldownTime);
                    return true;
                }
            }

            //
            // CALAMITY DODGES
            //

            if (Player.whoAmI != Main.myPlayer || disableAllDodges)
                return false;

            if (spectralVeil && spectralVeilImmunity > 0)
            {
                SpectralVeilDodge();
                return true;
            }

            // TODO -- drag all dodge code into a CalamityPlayer sub-file dedicated to dodging and nothing else
            if (HandleDashDodges())
                return true;

            // Mirror evades do not work if the global dodge cooldown is active. This cooldown can be triggered by either mirror.
            if (!Player.HasCooldown(GlobalDodge.ID) && actualDamageTaken >= dodgeDamageGateValue)
            {
                if (eclipseMirror)
                {
                    EclipseMirrorDodge(dodgeDamageGateValuePercent, dodgeDamageGateValue, actualDamageTaken);
                    return true;
                }
                else if (abyssalMirror)
                {
                    AbyssMirrorDodge(dodgeDamageGateValuePercent, dodgeDamageGateValue, actualDamageTaken);
                    return true;
                }
            }

            return base.ConsumableDodge(info);
        }
        #endregion

        #region Modify Hurt
        public override void ModifyHurt(ref Player.HurtModifiers modifiers)
        {
            // Calcium Potion knockback reduction
            if (calcium)
                modifiers.Knockback *= (1f - CalciumPotion.KnockbackResistance);

            // Handles energy shields and Boss Rush, in that order
            modifiers.ModifyHurtInfo += ModifyHurtInfo_Calamity;

            #region Custom Hurt Sounds
            if (hurtSoundTimer == 0)
            {
                if(Player.Transformation().Type != -1 && Player.Transformation().currentTransformation.HurtSound(Player) != null)
                {
                    var hurtSound = Player.Transformation().currentTransformation.HurtSound(Player).Value;
                    modifiers.DisableSound();
                    SoundEngine.PlaySound(hurtSound.sound, Player.Center);
                    hurtSoundTimer = hurtSound.delay;
                }
                else if (roverDrive && RoverDriveShieldDurability > 0)
                {
                    modifiers.DisableSound();
                    SoundEngine.PlaySound(RoverDrive.ShieldHurtSound, Player.Center);
                    hurtSoundTimer = 20;
                }
                else if (lunicCorpsSet && LunicCorpsShieldDurability > 0)
                {
                    modifiers.DisableSound();
                    SoundEngine.PlaySound(LunicCorpsHelmet.ShieldHurtSound, Player.Center);
                    hurtSoundTimer = 20;
                }
                else if (sponge && SpongeShieldDurability > 0)
                {
                    modifiers.DisableSound();
                    SoundEngine.PlaySound(TheSponge.ShieldHurtSound, Player.Center);
                    hurtSoundTimer = 20;
                }
                else if (titanHeartSet)
                {
                    modifiers.DisableSound();
                    SoundEngine.PlaySound(NPCs.Astral.Atlas.HurtSound, Player.Center);
                    hurtSoundTimer = 10;
                }
                else if (Player.GetModPlayer<WulfrumArmorPlayer>().wulfrumSet && (Player.name.ToLower() == "wagstaff" || Player.name.ToLower() == "john wulfrum"))
                {
                    modifiers.DisableSound();
                    SoundEngine.PlaySound(SoundID.DSTMaleHurt, Player.Center);
                    hurtSoundTimer = 10;
                }
            }
            #endregion

            #region Player Incoming Damage Multiplier (Increases)
            double damageMult = 1D;
            if (dArtifact) // Dimensional Soul Artifact increases incoming damage by 15%.
                damageMult += 0.15;
            if (enraged) // Demonshade Enrage
                damageMult += DemonshadeHelm.MultDamageTakenBoost;

            modifiers.SourceDamage *= (float)damageMult;
            #endregion

            //
            // At this point, the true, final incoming damage to the player has been calculated.
            // It has not yet been mitigated by any means.
            //

            if (blazingCoreParry > 0) //check for active parry
            {
                if (blazingCoreParry >= 12) //only the first 18 frames (0.3 seconds) counts for a valid parry
                {
                    if (!Player.HasCooldown(ParryCooldown.ID))
                    {
                        // 17APR2024: Ozzatron: Blazing Core is a parry. It uses vanilla parry iframes and benefits from Cross Necklace.
                        int blazingCoreParryIFrames = Player.ComputeParryIFrames();
                        Player.GiveUniversalIFrames(blazingCoreParryIFrames, true);

                        blazingCoreEmpoweredParry = true;

                        modifiers.Cancel();
                        modifiers.DisableSound(); //prevents hurt sound from playing, had no idea this was a thing
                    }

                    SoundEngine.PlaySound(BlazingCore.ParrySuccessSound, Player.Center);

                    float power = 2;
                    for (int i = 0; i < (int)(20 * power); i++)
                    {
                        if (Main.rand.NextBool())
                        {
                            Particle spark = new CustomSpark(Player.Center, ((new Vector2(19, 19) * power).RotatedByRandom(100)) * Main.rand.NextFloat(0.2f, 1f), "CalamityMod/Particles/ProvidenceMarkParticle", false, 47, Main.rand.NextFloat(1.15f, 1.3f) * power, Main.rand.NextBool(4) ? Color.Khaki : Color.Orange, new Vector2(1.3f, 0.5f), true, false, 0, false, false, Main.rand.NextFloat(0.1f, 0.2f));
                            GeneralParticleHandler.SpawnParticle(spark);
                        }
                        else
                        {
                            bool isSpark = Main.rand.NextBool(5);
                            Dust dust = Dust.NewDustPerfect(Player.Center, isSpark ? 278 : ModContent.DustType<LightDust>(), ((new Vector2(15, 15) * power).RotatedByRandom(100)) * Main.rand.NextFloat(0.2f, 1f));
                            dust.noGravity = true;
                            dust.scale = Main.rand.NextFloat(0.85f, 1.15f) * power * (isSpark ? 0.5f : 1);
                            dust.color = Main.rand.NextBool(5) ? Color.Khaki : Color.Goldenrod;
                            if (isSpark)
                                dust.noGravity = false;
                            else
                                dust.noLightEmittence = true;
                        }
                    }

                    Particle orb1 = new CustomPulse(Player.Center, Vector2.Zero, Color.Goldenrod, "CalamityMod/Particles/SoftRoundExplosion", new Vector2(1f, 0.8f), 0, 0, 0.14f * power, 25);
                    GeneralParticleHandler.SpawnParticle(orb1);

                    Particle orb2 = new CustomPulse(Player.Center, Vector2.Zero, Color.Khaki, "CalamityMod/Particles/BloomRing", new Vector2(1f, 0.5f), 0, 0, 2.1f * power, 25);
                    GeneralParticleHandler.SpawnParticle(orb2);

                    blazingCoreSuccessfulParry = 60;
                    Player.AddCooldown(ParryCooldown.ID, 60 * 30, false, "blazingcore"); //cooldown is frames in seconds multiplied by the desired amount of seconds
                }

                if (blazingCoreParry > 1)
                    blazingCoreParry = 1; //schedule parry to end next frame
            }
            else if (flameLickedShellParry > 0)
            {
                if (flameLickedShellParry >= 12)
                {
                    if (!Player.HasCooldown(ParryCooldown.ID))
                    {
                        // 17APR2024: Ozzatron: Flame-Licked Shell is a parry. It uses vanilla parry iframes and benefits from Cross Necklace.
                        int flameLickedShellParryIFrames = Player.ComputeParryIFrames();
                        Player.GiveUniversalIFrames(flameLickedShellParryIFrames, true);

                        flameLickedShellEmpoweredParry = true;

                        modifiers.FinalDamage *= 0.1f; //90% dr
                        modifiers.DisableSound();
                    }

                    SoundEngine.PlaySound(ProfanedGuardianDefender.ShieldDeathSound, Player.Center);
                    Player.AddCooldown(ParryCooldown.ID, 60 * 20, false, "flamelickedshell");
                    FlameLickedShell.handleParry(Player);
                }
            }
            else if (shieldOfTheOceanParry >= 12)
            {
                if (!Player.HasCooldown(ParryCooldown.ID))
                {
                    // Shield of the Ocean is a parry. It uses vanilla parry iframes and benefits from Cross Necklace.
                    int shieldParryIFrames = Player.ComputeParryIFrames();
                    Player.GiveUniversalIFrames(shieldParryIFrames, true);

                    shieldOfTheOceanEmpoweredParry = true;
                    modifiers.FinalDamage *= 0.4f; // 60% DR
                    modifiers.DisableSound();
                }

                SoundEngine.PlaySound(Main.zenithWorld ? ShieldoftheOcean.ParrySoundGFB : ShieldoftheOcean.ParrySound, Player.Center);
                Player.AddCooldown(ParryCooldown.ID, 1200, false, "shieldoftheocean");
                ShieldoftheOcean.ActivateParry(Player);
            }

            if (Player.Calamity().scionsCurio)
                scionsCurioGotHit = true;
        }

        private void ModifyHurtInfo_Calamity(ref Player.HurtInfo info)
        {
            // Boss Rush's damage floor is implemented as a dirty modifier
            // TODO -- implementing this correctly would require fully reimplementing all of DR and ADR
            if (BossRushEvent.BossRushActive)
            {
                int bossRushDamageFloor = (Main.expertMode ? 160 : 100) + (BossRushEvent.BossRushStage * 2);
                if (info.Damage < bossRushDamageFloor)
                    info.Damage += (bossRushDamageFloor - info.Damage);
            }

            // Energy shields are implemented as a dirty modifier
            // This is what SLR Barrier does; see
            // https://github.com/ProjectStarlight/StarlightRiver/blob/master/Core/Systems/BarrierSystem/BarrierPlayer.cs
            //
            // Currently implemented energy shields:
            // - Rover Drive
            // - Lunic Corps Armor set bonus
            // - Profaned Soul Artifact/Crystal
            // - The Sponge
            //
            // If the shield(s) completely absorb the hit, iframes are granted on the spot and the hit is marked to be dodged.
            // Shields are drained in order of progression, so your weaker shields will break first.
            // Damage can and will be blocked by multiple shields if it has to be.
            bool shieldsFullyAbsorbedHit = false;
            if (HasAnyEnergyShield)
            {
                bool shieldsTookHit = false;
                bool anyShieldBroke = false;
                int totalDamageBlocked = 0;

                // ROVER DRIVE
                if (roverDrive && RoverDriveShieldDurability > 0 && !shieldsFullyAbsorbedHit)
                {
                    // Check whether this shield can fully absorb the incoming hit (or what's left of it).
                    bool thisShieldCanFullyAbsorb = RoverDriveShieldDurability >= info.Damage;

                    // Tally up how much damage was blocked by this shield.
                    int roverDriveDamageBlocked = Math.Min(RoverDriveShieldDurability, info.Damage);
                    totalDamageBlocked += roverDriveDamageBlocked;

                    // Deal all incoming damage to this shield, because it is available.
                    RoverDriveShieldDurability -= info.Damage;
                    shieldsTookHit = true;

                    // Hits which break the Rover Drive shield cause a sound and slight screen shake.
                    // Multiple shields breaking simultaneously has slightly stronger screen shake.
                    if (RoverDriveShieldDurability <= 0)
                    {
                        RoverDriveShieldDurability = 0;
                        SoundEngine.PlaySound(RoverDrive.BreakSound, Player.Center);
                        Player.Calamity().GeneralScreenShakePower += anyShieldBroke ? 0.5f : 2f;
                        anyShieldBroke = true;
                    }

                    // Mark the hit as being canceled if this shield has enough durability to fully absorb it.
                    // This prevents further shields from attempting to absorb the hit.
                    if (thisShieldCanFullyAbsorb)
                        shieldsFullyAbsorbedHit = true;

                    // Actually remove damage from the incoming hit, so that later shields have less damage incoming.
                    info.Damage -= roverDriveDamageBlocked;
                }

                // LUNIC CORPS ARMOR
                if (lunicCorpsSet && LunicCorpsShieldDurability > 0 && !shieldsFullyAbsorbedHit)
                {
                    // Check whether this shield can fully absorb the incoming hit (or what's left of it).
                    bool thisShieldCanFullyAbsorb = LunicCorpsShieldDurability >= info.Damage;

                    // Tally up how much damage was blocked by this shield.
                    int masterChefDamageBlocked = Math.Min(LunicCorpsShieldDurability, info.Damage);
                    totalDamageBlocked += masterChefDamageBlocked;

                    // Deal all incoming damage to this shield, because it is available.
                    LunicCorpsShieldDurability -= info.Damage;
                    shieldsTookHit = true;

                    // Hits which break the Lunic Corps shield cause a sound and a slight screen shake.
                    // Multiple shields breaking simultaneously has slightly stronger screen shake.
                    if (LunicCorpsShieldDurability <= 0)
                    {
                        LunicCorpsShieldDurability = 0;
                        SoundEngine.PlaySound(LunicCorpsHelmet.BreakSound, Player.Center);
                        Player.Calamity().GeneralScreenShakePower += anyShieldBroke ? 0.5f : 2f;
                        anyShieldBroke = true;
                    }

                    // Mark the hit as being canceled if this shield has enough durability to fully absorb it.
                    // This prevents further shields from attempting to absorb the hit.
                    if (thisShieldCanFullyAbsorb)
                        shieldsFullyAbsorbedHit = true;

                    // Actually remove damage from the incoming hit, so that later shields have less damage incoming.
                    info.Damage -= masterChefDamageBlocked;
                }

                // PSA
                if (pSoulArtifact && pSoulShieldDurability > 0 && !shieldsFullyAbsorbedHit)
                {
                    // Check whether this shield can fully absorb the incoming hit (or what's left of it).
                    bool thisShieldCanFullyAbsorb = pSoulShieldDurability >= info.Damage;

                    // Tally up how much damage was blocked by this shield.
                    int pSoulDamageBlocked = Math.Min(pSoulShieldDurability, info.Damage);
                    totalDamageBlocked += pSoulDamageBlocked;

                    // Deal all incoming damage to this shield, because it is available.
                    pSoulShieldDurability -= info.Damage;
                    shieldsTookHit = true;

                    // Hits which break the PSA shield cause a sound and slight screen shake.
                    // Multiple shields breaking simultaneously has slightly stronger screen shake.
                    if (pSoulShieldDurability <= 0)
                    {
                        pSoulShieldDurability = 0;
                        SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath, Player.Center);
                        Player.Calamity().GeneralScreenShakePower += anyShieldBroke ? 0.5f : 2f;
                        anyShieldBroke = true;
                    }

                    // Mark the hit as being canceled if this shield has enough durability to fully absorb it.
                    // This prevents further shields from attempting to absorb the hit.
                    if (thisShieldCanFullyAbsorb)
                        shieldsFullyAbsorbedHit = true;

                    // Actually remove damage from the incoming hit, so that later shields have less damage incoming.
                    info.Damage -= pSoulDamageBlocked;
                }

                // THE SPONGE
                if (sponge && SpongeShieldDurability > 0 && !shieldsFullyAbsorbedHit)
                {
                    // Check whether this shield can fully absorb the incoming hit (or what's left of it).
                    bool thisShieldCanFullyAbsorb = SpongeShieldDurability >= info.Damage;

                    // Tally up how much damage was blocked by this shield.
                    int spongeDamageBlocked = Math.Min(SpongeShieldDurability, info.Damage);
                    totalDamageBlocked += spongeDamageBlocked;

                    // Deal all incoming damage to this shield, because it is available.
                    SpongeShieldDurability -= info.Damage;
                    shieldsTookHit = true;

                    // Hits which break The Sponge's shield cause a sound and a slight screen shake.
                    // Multiple shields breaking simultaneously has slightly stronger screen shake.
                    if (SpongeShieldDurability <= 0)
                    {
                        SpongeShieldDurability = 0;
                        SoundEngine.PlaySound(TheSponge.BreakSound, Player.Center);
                        Player.Calamity().GeneralScreenShakePower += anyShieldBroke ? 0.5f : 2f;
                        anyShieldBroke = true;
                    }

                    // Mark the hit as being canceled if this shield has enough durability to fully absorb it.
                    // This prevents further shields from attempting to absorb the hit.
                    if (thisShieldCanFullyAbsorb)
                        shieldsFullyAbsorbedHit = true;

                    // Actually remove damage from the incoming hit, so that later shields have less damage incoming.
                    info.Damage -= spongeDamageBlocked;
                }

                // If any shields took damage, there is some code that must be run.
                if (shieldsTookHit)
                {
                    // If any shields took damage, display text indicating that shield damage was taken.
                    string shieldDamageText = (-totalDamageBlocked).ToString();
                    Rectangle location = new Rectangle((int)Player.position.X, (int)Player.position.Y - 16, Player.width, Player.height);
                    CombatText.NewText(location, Color.LightBlue, Language.GetTextValue(shieldDamageText));

                    // Give the player iframes for taking a shield hit, regardless of whether or not the shields broke.
                    int shieldHitIFrames = Player.ComputeHitIFrames(info);
                    Player.GiveIFrames(info.CooldownCounter, shieldHitIFrames, true);

                    // Spawn particles when hit with the shields up, regardless of whether or not the shields broke.
                    // More particles spawn if a shield broke.
                    if (pSoulArtifact)
                    {
                        for (int i = 0; i < Main.rand.Next(4, 8); i++) //very light dust
                        {
                            Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, (int)CalamityDusts.ProfanedFire);
                            dust.velocity = Main.rand.NextVector2Circular(3.5f, 3.5f);
                            dust.velocity.Y -= Main.rand.NextFloat(1f, 3f);
                            dust.scale = Main.rand.NextFloat(1.15f, 1.45f);
                        }
                    }
                    else
                    {
                        int numParticles = Main.rand.Next(2, 6) + (anyShieldBroke ? 6 : 0);
                        for (int i = 0; i < numParticles; i++)
                        {
                            // Rover Drive has slightly higher particle velocity
                            float maxVelocity = roverDrive ? 14f : 7f;
                            Vector2 velocity = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(3f, maxVelocity);
                            velocity.X += 5f * info.HitDirection;

                            float scale = Main.rand.NextFloat(2.5f, 3f);
                            Color particleColor = Main.rand.NextBool() ? new Color(99, 255, 229) : new Color(25, 132, 247);
                            int lifetime = 25;

                            var shieldParticle = new TechyHoloysquareParticle(Player.Center, velocity, scale, particleColor, lifetime);
                            GeneralParticleHandler.SpawnParticle(shieldParticle);
                        }
                    }

                    // Update Rover Drive durability on the cooldown rack.
                    if (roverDrive && cooldowns.TryGetValue(WulfrumRoverDriveDurability.ID, out var roverDriveDurabilityCD))
                        roverDriveDurabilityCD.timeLeft = RoverDriveShieldDurability;

                    // Update Lunic Corps Armor durability on the cooldown rack.
                    if (lunicCorpsSet && cooldowns.TryGetValue(Cooldowns.LunicCorpsShieldDurability.ID, out var masterChefDurabilityCD))
                        masterChefDurabilityCD.timeLeft = LunicCorpsShieldDurability;

                    // Update PSA/PSC durability on the cooldown rack
                    if (pSoulArtifact && (!profanedCrystal || profanedCrystalBuffs) && cooldowns.TryGetValue(Cooldowns.ProfanedSoulShield.ID, out var profanedSoulDurabilityCD))
                        profanedSoulDurabilityCD.timeLeft = pSoulShieldDurability;

                    // Update Sponge durability on the cooldown rack.
                    if (sponge && cooldowns.TryGetValue(SpongeDurability.ID, out var spongeDurabilityCD))
                        spongeDurabilityCD.timeLeft = SpongeShieldDurability;
                }

                // Regardless of whether shields took damage, iterate over and stall all shield regen on ANY hit.
                // This applies even if you are hit while shields are fully down, or if you unequip any of the relevant items.
                {
                    // Set Rover Drive's recharge delay to full. Override any existing cooldown instance.
                    if (roverDrive)
                        Player.AddCooldown(WulfrumRoverDriveRecharge.ID, RoverDrive.ShieldRechargeDelay, true);

                    // Set the Lunic Corps Armor's recharge delay to full. Override any existing cooldown instance.
                    if (lunicCorpsSet)
                        Player.AddCooldown(LunicCorpsShieldRecharge.ID, LunicCorpsHelmet.ShieldRechargeDelay, true);

                    if (pSoulArtifact && (!profanedCrystal || profanedCrystalBuffs))
                        Player.AddCooldown(ProfanedSoulShieldRecharge.ID, profanedCrystalBuffs ? (60 * 5) : (60 * 10), true); // 5 seconds psc, 10 seconds psa 

                    // Set The Sponge's recharge delay to full. Override any existing cooldown instance.
                    if (sponge)
                        Player.AddCooldown(SpongeRecharge.ID, TheSponge.ShieldRechargeDelay, true);
                }

                // If the shields completely absorbed the hit, then delete the hit using reflection.
                if (shieldsFullyAbsorbedHit)
                {
                    freeDodgeFromShieldAbsorption = true;

                    // Cancel defense damage, if it was going to occur this frame.
                    nextHitDealsDefenseDamage = false;
                }
            }

            // Chalice of the Blood God is implemented as a dirty modifier.
            //
            // Chalice of the Blood God does nothing to a hit that was just fully blocked by shields.
            // Otherwise, it reduces the damage of any hit to 5, which allows for full iframes.
            // It then applies the full hit (minus that 5 damage) to its own bleedout buffer in OnHurt (see below).
            // Hits for less than 5 damage are ignored entirely and allowed to strike the player as normal.
            if (chaliceOfTheBloodGod && !shieldsFullyAbsorbedHit && info.Damage > ChaliceOfTheBloodGod.MinAllowedDamage)
            {
                chaliceBleedoutToApplyOnHurt = info.Damage - ChaliceOfTheBloodGod.MinAllowedDamage;

                chaliceHitOriginalDamage = info.Damage;
                info.Damage = ChaliceOfTheBloodGod.MinAllowedDamage;
            }
        }
        #endregion

        #region On Hurt
        public override void OnHurt(Player.HurtInfo hurtInfo)
        {
            // If Armageddon is active, instantly kill the player.
            if (CalamityWorld.armageddon && areThereAnyDamnBosses)
                KillPlayer();

            #region Actually Dealing Defense Damage
            // Check if the player has iframes for the sake of avoiding defense damage.
            bool hasIFrames = Player.HasIFrames();

            // If the player was just hit by something capable of dealing defense damage, then apply defense damage.
            // Bloodflare Core makes every hit deal defense damage (to enable its function).
            // Defense damage is not applied if the player has iframes or godmode.
            bool hitCanApplyDefenseDamage = nextHitDealsDefenseDamage || bloodflareCore;
            bool defenseDamageShouldApply = hitCanApplyDefenseDamage && !hasIFrames && !Player.creativeGodMode;

            // 15AUG2024: Ozzatron: External flag which completely disables defense damage. This overrides Bloodflare Core.
            bool externalFlagsAppropriate = !CalamityMod.ExternalFlag_DisableDefenseDamage && !externalDefenseDamageImmunity;

            if (defenseDamageShouldApply && externalFlagsAppropriate)
            {
                double halfDefense = Player.statDefense / 2.0;
                int netMitigation = hurtInfo.SourceDamage - hurtInfo.Damage;
                double standardDefenseDamage = netMitigation * defenseDamageRatio;

                // Bloodflare Core overrides standard defense damage if it would be less than half of the player's total defense.
                if (bloodflareCore && standardDefenseDamage < halfDefense)
                {
                    // In this case, forcibly deal half of the player's total defense as defense damage. This ignores ratios.
                    DealDefenseDamage((int)halfDefense, true);

                    // Set up Bloodflare Core's heal over time. Any in-progress heals are overwritten if they would have a shorter duration.
                    if (bloodflareCoreRemainingHealOverTime < halfDefense)
                        bloodflareCoreRemainingHealOverTime = (int)halfDefense;

                    // Play a sound and make dust to signify that defense has been shattered
                    SoundEngine.PlaySound(SoundID.DD2_MonkStaffGroundImpact, Player.Center);
                    for (int i = 0; i < 36; ++i)
                    {
                        float speed = Main.rand.NextFloat(1.8f, 8f);
                        Vector2 dustVel = new Vector2(speed, speed);
                        Dust d = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.GemRuby);
                        d.velocity = dustVel;
                        d.noGravity = true;
                        d.scale *= Main.rand.NextFloat(1.1f, 1.4f);
                        Dust.CloneDust(d).velocity = dustVel.RotatedBy(MathHelper.PiOver2);
                        Dust.CloneDust(d).velocity = dustVel.RotatedBy(MathHelper.Pi);
                        Dust.CloneDust(d).velocity = dustVel.RotatedBy(MathHelper.Pi * 1.5f);
                    }
                }

                // Chalice of the Blood God has to compensate for the "mitigation" provided by its bleedout buffer
                else if (chaliceOfTheBloodGod)
                    DealDefenseDamage(hurtInfo, chaliceBleedoutToApplyOnHurt);

                // Otherwise, just deal regular defense damage.
                else
                    DealDefenseDamage(hurtInfo);
            }

            nextHitDealsDefenseDamage = false;
            #endregion

            #region Chalice of the Blood God Bleed Application
            // This is handled in OnHurt so that Chalice hits can still be dodged based on their appropriate normal damage
            // Defense damage based on the "total lethality of the hit" is applied immediately prior to this
            // 
            // 1 - Actually apply bleedout to the player based on the damage they would have taken
            // 2 - Display an indicator of how much damage was dealt as bleedout instead of regular damage
            if (chaliceOfTheBloodGod)
            {
                int bleedoutToApply = chaliceBleedoutToApplyOnHurt;
                chaliceBleedoutBuffer += bleedoutToApply;

                // Display text indicating that damage was transferred to bleedout.
                string text = $"({-bleedoutToApply})";
                Rectangle location = new Rectangle((int)Player.position.X + 4, (int)Player.position.Y - 3, Player.width - 4, Player.height - 4);
                CombatText.NewText(location, ChaliceOfTheBloodGod.BleedoutBufferDamageTextColor, Language.GetTextValue(text), dot: true);
            }
            #endregion

            #region Shattered Community Rage Gain
            // Shattered Community makes the player gain rage based on the amount of damage taken.
            // Also set the Rage gain cooldown to prevent bizarre abuse cases.
            if (shatteredCommunity && rageGainCooldown == 0)
            {
                float HPRatio = (float)hurtInfo.SourceDamage / Player.statLifeMax2;
                float rageConversionRatio = 0.8f;

                // Damage to rage conversion is half as effective while Rage Mode is active.
                if (rageModeActive)
                    rageConversionRatio *= 0.5f;
                // If Rage is over 100%, damage to rage conversion scales down asymptotically based on how full Rage is.
                if (rage >= rageMax)
                    rageConversionRatio *= 3f / (3f + rage / rageMax);

                rage += rageMax * HPRatio * rageConversionRatio;
                rageGainCooldown = ShatteredCommunity.RageGainCooldown;
                // Rage capping is handled in MiscEffects
            }
            #endregion

            // Give Rage combat frames because being hurt counts as combat.
            if (RageEnabled)
                rageCombatFrames = BalancingConstants.RageCombatDelayTime;

            // Regenerator has been CANCLED on TWITTER.COM!!!! (Just keeping this here since it's a neat effect and I'll probably yoink it for something else later)
            if (regenerator && false)
            {
                // Projectile damage and count is based on source damage of the hit
                float hitPower = (hurtInfo.SourceDamage / (Player.statLifeMax2 * 0.5f));
                int projCount = (int)MathHelper.Clamp((15 * hitPower), 2, 15) * 2; // they come out in pairs of 2
                int projBonusDamage = (int)(hurtInfo.SourceDamage * 0.5f);
                int projDamage = (int)Player.GetBestClassDamage().ApplyTo(20 + projBonusDamage);
                for (int i = 0; i < projCount; i++)
                {
                    Vector2 vel = (MathHelper.TwoPi * i / projCount).ToRotationVector2() * (i % 2 == 0 ? 0.75f : 1f) * 10;
                    Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, vel.RotatedBy(-0.2f * (i % 2 == 0 ? -1 : 1)), ModContent.ProjectileType<RetaliationProjectile>(), projDamage, 0f, Player.whoAmI, 0, (i % 2 == 0 ? -1 : 1), -5);
                }
            }

            // Hide of Astrum Deus' melee boost
            if (hideOfDeus)
            {
                hideOfDeusMeleeBoostTimer += 3 * hurtInfo.Damage;
                if (hideOfDeusMeleeBoostTimer > 600)
                    hideOfDeusMeleeBoostTimer = 600;
            }

            if (Player.whoAmI == Main.myPlayer)
            {
                // Summon a portal if needed.
                if (Player.Calamity().persecutedEnchant)
                {
                    if (NPC.CountNPCS(ModContent.NPCType<DemonPortal>()) < 2)
                    {
                        int tries = 0;
                        Vector2 spawnPosition;
                        Vector2 spawnPositionOffset = Vector2.One * 24f;
                        do
                        {
                            spawnPosition = Player.Center + Main.rand.NextVector2Unit() * Main.rand.NextFloat(270f, 420f);
                            tries++;
                        }
                        while (Collision.SolidCollision(spawnPosition - spawnPositionOffset, 48, 24) && tries < 100);
                        CalamityNetcode.NewNPC_ClientSide(spawnPosition, ModContent.NPCType<DemonPortal>(), Player);
                    }
                }

                if (victideBarrierSet && !Player.HasCooldown(WardingWave.ID))
                {
                    int healAmt = (int)(hurtInfo.Damage * VictideHeadBarrier.BarrierDamageAbsorptionPercent);
                    victideBarrierHeal += healAmt;
                }

                if (daedalusAbsorb && Main.rand.NextBool(DaedalusHeadMagic.AbsorptionChanceDenominator))
                {
                    int healAmt = (int)(hurtInfo.Damage * DaedalusHeadMagic.DamageAbsorptionPercent);
                    Player.HealPlayer(healAmt);
                }

                if (absorber)
                {
                    int healAmt = (int)(hurtInfo.Damage * TheAbsorber.DamageTakenHealedPercent);
                    Player.HealPlayer(healAmt);
                }

                if (witheringDamageDone > 0)
                {
                    double healCompenstationRatio = Math.Log(witheringDamageDone) * Math.Pow(witheringDamageDone, 2D / 3D) / 177000D;
                    if (healCompenstationRatio > 1D)
                        healCompenstationRatio = 1D;
                    int healCompensation = (int)(healCompenstationRatio * hurtInfo.Damage);
                    Player.HealPlayer((int)(healCompenstationRatio * hurtInfo.Damage));
                    Player.AddBuff(ModContent.BuffType<Withered>(), 1080);
                    witheringDamageDone = 0;
                }

                // Lose adrenaline on hit, unless using Draedon's Heart.
                if (AdrenalineEnabled)
                {
                    LoseAdrenalineOnHurt(hurtInfo, false);
                }

                if (evilSmasherBoost > 0)
                    evilSmasherBoost -= 1;

                if (trinketOfChi)
                    chiBuffTimer = 0;

                if (amidiasBlessing && (chaliceOfTheBloodGod ? chaliceBleedoutToApplyOnHurt : hurtInfo.Damage) > 50)
                {
                    Player.ClearBuff(ModContent.BuffType<AmidiasBlessing>());
                    SoundEngine.PlaySound(SoundID.Item96, Player.Center);
                }

                if (gShell) //3 seconds of no dash reduction and reduced defense
                {
                    if (giantShellPostHit == 0)
                    {
                        float numberOfDusts = 35f;
                        float rotFactor = 360f / numberOfDusts;
                        for (int i = 0; i < numberOfDusts; i++)
                        {
                            float rot = MathHelper.ToRadians(i * rotFactor);
                            Vector2 offset = new Vector2(Main.rand.NextFloat(0.5f, 2.5f), 0).RotatedBy(rot * Main.rand.NextFloat(1.1f, 9.1f));
                            Vector2 velOffset = new Vector2(Main.rand.NextFloat(0.5f, 2.5f), 0).RotatedBy(rot * Main.rand.NextFloat(1.1f, 9.1f));
                            Dust dust = Dust.NewDustPerfect(Player.Center + offset, Main.rand.NextBool() ? 249 : 118, new Vector2(velOffset.X, velOffset.Y));
                            dust.noGravity = false;
                            dust.velocity = velOffset;
                            dust.scale = Main.rand.NextFloat(1.5f, 1.2f);
                        }
                    }
                    giantShellPostHit = 180;
                }

                if (tortShell) //3 seconds of no dash reduction and reduced defense
                {
                    if (tortShellPostHit == 0)
                    {
                        float numberOfDusts = 43f;
                        float rotFactor = 360f / numberOfDusts;
                        for (int i = 0; i < numberOfDusts; i++)
                        {
                            float rot = MathHelper.ToRadians(i * rotFactor);
                            Vector2 offset = new Vector2(Main.rand.NextFloat(0.5f, 3.1f), 0).RotatedBy(rot * Main.rand.NextFloat(1.1f, 9.1f));
                            Vector2 velOffset = new Vector2(Main.rand.NextFloat(0.5f, 3.1f), 0).RotatedBy(rot * Main.rand.NextFloat(1.1f, 9.1f));
                            Dust dust = Dust.NewDustPerfect(Player.Center + offset, Main.rand.NextBool() ? 215 : 22, new Vector2(velOffset.X, velOffset.Y));
                            dust.noGravity = false;
                            dust.velocity = velOffset;
                            dust.scale = Main.rand.NextFloat(1.6f, 2.2f);
                        }
                    }
                    tortShellPostHit = 180;
                }

                if (abyssalDivingSuitPlates && hurtInfo.Damage > 50)
                {
                    if (abyssalDivingSuitPlateHits < 3)
                        abyssalDivingSuitPlateHits++;

                    bool plateCDExists = cooldowns.TryGetValue(DivingPlatesBreaking.ID, out CooldownInstance plateDurability);
                    if (plateCDExists)
                        plateDurability.timeLeft = abyssalDivingSuitPlateHits;

                    if (abyssalDivingSuitPlateHits >= 3)
                    {
                        SoundEngine.PlaySound(SoundID.NPCDeath14, Player.Center);

                        if (plateCDExists)
                            cooldowns.Remove(DivingPlatesBreaking.ID);

                        Player.AddCooldown(DivingPlatesBroken.ID, 10830);

                        for (int d = 0; d < 20; d++)
                        {
                            Dust dust = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Smoke, 0f, 0f, 100, default, 2f);
                            dust.velocity *= 3f;
                            if (Main.rand.NextBool())
                            {
                                dust.scale = 0.5f;
                                dust.fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                            }
                        }

                        for (int d = 0; d < 35; d++)
                        {
                            Dust fire = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Torch, 0f, 0f, 100, default, 3f);
                            fire.noGravity = true;
                            fire.velocity *= 5f;
                            fire = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Torch, 0f, 0f, 100, default, 2f);
                            fire.velocity *= 2f;
                        }
                    }
                }

                if (aquaticHeartIce)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath7, Player.Center);
                    Player.AddCooldown(AquaticHeartIceShield.ID, CalamityUtils.SecondsToFrames(30));

                    for (int d = 0; d < 10; d++)
                    {
                        Dust ice = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.IceRod, 0f, 0f, 100, default, 2f);
                        ice.velocity *= 3f;
                        if (Main.rand.NextBool())
                        {
                            ice.scale = 0.5f;
                            ice.fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
                        }
                    }
                    for (int d = 0; d < 15; d++)
                    {
                        Dust ice = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.IceRod, 0f, 0f, 100, default, 3f);
                        ice.noGravity = true;
                        ice.velocity *= 5f;
                        ice = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.IceRod, 0f, 0f, 100, default, 2f);
                        ice.velocity *= 2f;
                    }
                }

                if (tarraMelee)
                {
                    Player.AddBuff(ModContent.BuffType<TarraLifeRegen>(), 120);
                }
                else if (xerocSet)
                {
                    Player.AddBuff(ModContent.BuffType<EmpyreanWrath>(), EmpyreanMask.WrathDuration);
                }
                else if (reaverDefense)
                {
                    Player.AddBuff(ModContent.BuffType<ReaverRage>(), ReaverHeadTank.ReaverRageDuration);
                }

                if (fBarrier || (aquaticHeart && NPC.downedBoss3))
                {
                    SoundEngine.PlaySound(SoundID.Item27, Player.Center);
                    foreach (NPC npc in Main.ActiveNPCs)
                    {
                        if (npc.friendly || npc.dontTakeDamage)
                            continue;

                        float npcDist = (npc.Center - Player.Center).Length();
                        float freezeDist = 300 + (int)hurtInfo.Damage * 2;
                        if (freezeDist > 500f)
                            freezeDist = 500f + (freezeDist - 500f) * 0.5f;

                        if (npcDist < freezeDist)
                        {
                            float duration = Main.rand.Next(10 + (int)hurtInfo.Damage / 2, 20 + (int)hurtInfo.Damage);
                            if (duration > 120)
                                duration = 120;

                            npc.AddBuff(ModContent.BuffType<GlacialState>(), (int)duration, false);
                        }
                    }
                }

                // By setting brainOfConfusionItem, these accessories have this code already,
                // but doing it again allows for increased duration + The Amalgam's other buffs,
                // and also doesn't have random chance (why does Brain of Confusion not guarantee confusion on hit)
                if (aBrain || amalgam)
                {
                    foreach (NPC npc in Main.ActiveNPCs)
                    {
                        if (npc.friendly || npc.dontTakeDamage)
                            continue;

                        float npcDist = (npc.Center - Player.Center).Length();
                        float range = Main.rand.Next(200 + (int)hurtInfo.Damage / 2, 301 + (int)hurtInfo.Damage * 2);
                        if (range > 500f)
                            range = 500f + (range - 500f) * 0.75f;
                        if (range > 700f)
                            range = 700f + (range - 700f) * 0.5f;
                        if (range > 900f)
                            range = 900f + (range - 900f) * 0.25f;

                        if (npcDist < range)
                        {
                            int duration = Main.rand.Next(300 + hurtInfo.Damage / 3, 480 + hurtInfo.Damage / 2);
                            npc.AddBuff(BuffID.Confused, duration, false);
                        }
                    }

                    // Spawn the harmless brain images that are actually projectiles
                    var source = Player.GetSource_Accessory(FindAccessory(ModContent.ItemType<TheAmalgam>()));
                    Projectile.NewProjectile(source, Player.Center.X + Main.rand.Next(-40, 40), Player.Center.Y - Main.rand.Next(20, 60), Player.velocity.X * 0.3f, Player.velocity.Y * 0.3f, ProjectileID.BrainOfConfusion, 0, 0f, Player.whoAmI);
                }
            }

            if (Player.ownedProjectileCounts[ModContent.ProjectileType<DrataliornusBow>()] != 0)
            {
                foreach (Projectile p in Main.ActiveProjectiles)
                {
                    if (p.type == ModContent.ProjectileType<DrataliornusBow>() && p.owner == Player.whoAmI)
                    {
                        p.Kill();
                        break;
                    }
                }

                if (Player.wingTime > Player.wingTimeMax / 2)
                    Player.wingTime = Player.wingTimeMax / 2;
            }

            // Bone Wings: Getting hit halves current flight time
            if (Player.wingsLogic == (int)VanillaWingID.BoneWings)
            {
                // Drop some bones for visual effects
                if (!Main.dedServ && Player.wingTime > 0)
                {
                    var source = Player.GetSource_Accessory(FindAccessory(ItemID.BoneWings));
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 boneVelocity = Vector2.UnitY.RotatedByRandom(MathHelper.ToRadians(30f)) * Main.rand.NextFloat(1.5f, 2.5f);
                        Gore bone = Gore.NewGoreDirect(source, Player.Center, boneVelocity, 57, Main.rand.NextFloat(0.6f, 0.9f));
                        bone.timeLeft = Main.rand.Next(6, 30 + 1);
                    }
                }
                Player.wingTime /= 2;
            }
        }
        #endregion

        #region Post Hurt
        public override void PostHurt(Player.HurtInfo hurtInfo)
        {
            // Silver Armor medkit timer
            if (silverMedkit && hurtInfo.Damage >= SilverArmorSetChange.SetBonusMinimumDamageToHeal)
                silverMedkitTimer = SilverArmorSetChange.SetBonusHealTime;

            // Handle hit effects from the gem tech armor set.
            Player.Calamity().GemTechState.PlayerOnHitEffects((int)hurtInfo.Damage);

            if (Player.whoAmI == Main.myPlayer)
            {
                // Add extra iframes on hit based on various Calamity effects.
                int iFramesToAdd = Player.GetExtraHitIFrames(hurtInfo);

                // Give bonus immunity frames based on the type of damage dealt
                if (hurtInfo.CooldownCounter != -1)
                    Player.hurtCooldowns[hurtInfo.CooldownCounter] += iFramesToAdd;
                else
                    Player.immuneTime += iFramesToAdd;

                // Similar handle to 1.4 Star Cloak: these hits ie. spikes or lava cannot activate hit effects
                bool canTriggerHitEffects = hurtInfo.CooldownCounter == -1 || hurtInfo.CooldownCounter == 1;
                if (!canTriggerHitEffects)
                    return;

                if (aeroSet && hurtInfo.Damage > AerospecBreastplate.SetBonusHurtDamageThreshold)
                {
                    // https://github.com/tModLoader/tModLoader/wiki/IEntitySource#detailed-list
                    var source = Player.GetSource_OnHurt(hurtInfo.DamageSource, AerospecBreastplate.FeatherEntitySourceContext);
                    int featherDamage = (int)Player.GetBestClassDamage().ApplyTo(AerospecBreastplate.SetBonusFeatherDamage);
                    for (int n = 0; n < 4; n++)
                    {
                        CalamityUtils.ProjectileRain(source, Player.Center, 400f, 100f, 500f, 800f, 20f, ModContent.ProjectileType<StickyFeatherAero>(), featherDamage, 1f, Player.whoAmI);
                    }
                }
                if (hideOfDeus)
                {
                    var source = Player.GetSource_Accessory(FindAccessory(ModContent.ItemType<HideofAstrumDeus>()));
                    SoundEngine.PlaySound(SoundID.Item74, Player.Center);

                    int blazeDamage = (int)Player.GetBestClassDamage().ApplyTo(HideofAstrumDeus.BlazeDamage);
                    Projectile.NewProjectile(source, Player.Center.X, Player.Center.Y, 0f, 0f, ModContent.ProjectileType<HideOfAstrumDeusExplosion>(), blazeDamage, 5f, Player.whoAmI, 0f, 1f);
                }
                // TODO -- Make Deific Amulet and Rampart of Deities' retaliation effects way cooler
                // In the meantime, gave them homing astral bombers instead of the lame falling stars
                if (dAmulet)
                {
                    var source = Player.GetSource_Accessory(FindAccessory(ModContent.ItemType<DeificAmulet>()));
                    int projAmount = (rampartOfDeities ? 12 : 6);
                    for (int n = 0; n < projAmount; n++)
                    {
                        int deificProjDamage = (int)Player.GetBestClassDamage().ApplyTo(DeificAmulet.StarDamage);

                        Projectile onHitProj = Main.projectile[Projectile.NewProjectile(source, Player.Center, new Vector2(0,-15).RotatedBy(MathHelper.TwoPi/projAmount*n), ModContent.ProjectileType<AstralStar>(), deificProjDamage, 4f, Player.whoAmI)];
                        if (onHitProj.whoAmI.WithinBounds(Main.maxProjectiles))
                        {
                            onHitProj.DamageType = DamageClass.Generic;
                            onHitProj.usesLocalNPCImmunity = true;
                            onHitProj.localNPCHitCooldown = 5;
                            onHitProj.tileCollide = false;
                            onHitProj.extraUpdates = 1;
                            onHitProj.Calamity().conditionalHomingRange = 500f;
                        }
                    }
                }
                if (ilSpark)
                {
                    SoundEngine.PlaySound(SoundID.Item93, Player.Center);

                    // Only visual effects are done here
                    // The actual spark spawning is now handled with the shocked variable in CalamityGlobalNPC

                    SeaFoamParticle boom = new(Player.Center, Vector2.Zero, new Color(89, 239, 247), new Color(56, 158, 209), 1f, 200f, Main.rand.NextBool() ? -1f : 1f);
                    GeneralParticleHandler.SpawnParticle(boom);

                    for (int e = 0; e < 6; e++)
                    {
                        Vector2 dustVel = Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(4.5f, 5.25f);
                        Dust electric = Dust.NewDustPerfect(Player.Center, DustID.Electric, dustVel, Scale: 0.75f);
                        electric.noGravity = true;
                    }
                }
                if (rBrain)
                {
                    if (!(CalamityUtils.AnyProjectiles(ModContent.ProjectileType<ShadeNimbus>()) || CalamityUtils.AnyProjectiles(ModContent.ProjectileType<ShadeNimbusSpawner>())))
                    {
                        var source = Player.GetSource_Accessory(FindAccessory(ModContent.ItemType<RottenBrain>()));
                        int effectStrength = amalgam ? 3 : aBrain ? 2 : 1;
                        int effectDamage = amalgam ? TheAmalgam.NimbusDamage : aBrain ? AmalgamatedBrain.NimbusDamage : RottenBrain.NimbusDamage;
                        effectDamage = (int)Player.GetBestClassDamage().ApplyTo(effectDamage);

                        Vector2 spawnerVelocity = -Vector2.UnitY.RotatedByRandom(MathHelper.Pi / 40f) * 12.5f;
                        Projectile.NewProjectile(source, Player.Center, spawnerVelocity, ModContent.ProjectileType<ShadeNimbusSpawner>(), effectDamage, 0f, Player.whoAmI, 0f, 0f, effectStrength);
                    }
                }
                if (inkBomb && !abyssalMirror && !eclipseMirror)
                {
                    if (Player.whoAmI == Main.myPlayer)
                    {
                        if (!Player.HasCooldown(Cooldowns.InkBomb.ID))
                        {
                            Player.AddCooldown(Cooldowns.InkBomb.ID, CalamityUtils.SecondsToFrames(20));
                            rogueStealth += 0.5f;
                            SoundEngine.PlaySound(SoundID.NPCDeath28 with { Volume = 2f }, Player.Center);
                        }

                        var source = Player.GetSource_Accessory(FindAccessory(ModContent.ItemType<Items.Accessories.InkBomb>()));
                        SoundEngine.PlaySound(SoundID.Item1, Player.Center);
                        for (int i = 0; i < 3; i++)
                        {
                            int ink = Projectile.NewProjectile(Player.GetSource_FromThis(), Player.Center, Vector2.One.RotatedByRandom(MathHelper.TwoPi) * 2f, ModContent.ProjectileType<InkBombProjectile>(), 0, 0, Player.whoAmI);
                            if (ink.WithinBounds(Main.maxProjectiles))
                                Main.projectile[ink].DamageType = DamageClass.Generic;
                        }
                    }
                }
                if (ataxiaBlaze)
                {
                    var fuckYouBitch = Player.GetSource_Misc("21");
                    if (hurtInfo.Damage > 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item74, Player.Center);
                        int eDamage = (int)Player.GetBestClassDamage().ApplyTo(HydrothermicArmor.BlazeDamage);

                        if (Player.whoAmI == Main.myPlayer)
                            Projectile.NewProjectile(fuckYouBitch, Player.Center, Vector2.Zero, ModContent.ProjectileType<DeepseaBlaze>(), eDamage, 1f, Player.whoAmI, 0f, 0f);
                    }
                }
                else if (daedalusShard) // Daedalus Ranged helm
                {
                    if (hurtInfo.Damage > 0)
                    {
                        SoundEngine.PlaySound(SoundID.Item27, Player.Center);

                        if (Player.whoAmI == Main.myPlayer)
                        {
                            var source = Player.GetSource_Misc("22");
                            float offset = Main.rand.NextFloat(MathHelper.TwoPi);
                            int sDamage = (int)Player.GetTotalDamage<RangedDamageClass>().ApplyTo(DaedalusHeadRanged.ShardDamage);
                            for (int i = 0; i < 10; i++)
                            {
                                Vector2 circleVel = ((MathHelper.TwoPi * i / 10f) + offset).ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                                int shard = Projectile.NewProjectile(source, Player.Center, circleVel, ProjectileID.CrystalShard, sDamage, 1f, Player.whoAmI);
                                if (shard.WithinBounds(Main.maxProjectiles))
                                    Main.projectile[shard].DamageType = DamageClass.Generic;
                            }
                        }
                    }
                }
                else if (godSlayerDamage) //god slayer melee helm
                {
                    var source = Player.GetSource_Misc("24");
                    if (hurtInfo.Damage > 80)
                    {
                        SoundEngine.PlaySound(SoundID.Item73, Player.Center);
                        float spread = 45f * 0.0174f;
                        double startAngle = Math.Atan2(Player.velocity.X, Player.velocity.Y) - spread / 2;
                        double deltaAngle = spread / 8f;
                        double offsetAngle;
                        int shrapnelDamage = Player.CalcIntDamage<MeleeDamageClass>(GodSlayerHeadMelee.DartDamage);
                        if (Player.whoAmI == Main.myPlayer)
                        {
                            for (int i = 0; i < 4; i++)
                            {
                                offsetAngle = startAngle + deltaAngle * (i + i * i) / 2f + 32f * i;
                                Projectile.NewProjectile(source, Player.Center.X, Player.Center.Y, (float)(Math.Sin(offsetAngle) * 5f), (float)(Math.Cos(offsetAngle) * 5f), ModContent.ProjectileType<GodKiller>(), shrapnelDamage, 5f, Player.whoAmI, 0f, 0f);
                                Projectile.NewProjectile(source, Player.Center.X, Player.Center.Y, (float)(-Math.Sin(offsetAngle) * 5f), (float)(-Math.Cos(offsetAngle) * 5f), ModContent.ProjectileType<GodKiller>(), shrapnelDamage, 5f, Player.whoAmI, 0f, 0f);
                            }
                        }
                    }
                }
                else if (dsSetBonus)
                {
                    if (Player.whoAmI == Main.myPlayer)
                    {
                        // https://github.com/tModLoader/tModLoader/wiki/IEntitySource#detailed-list
                        var source = Player.GetSource_OnHurt(hurtInfo.DamageSource, DemonshadeHelm.ShadowScytheEntitySourceContext);
                        for (int l = 0; l < 2; l++)
                        {
                            int shadowbeamDamage = (int)Player.GetBestClassDamage().ApplyTo(DemonshadeHelm.BeamDamage);

                            Projectile beam = CalamityUtils.ProjectileRain(source, Player.Center, 400f, 100f, 500f, 800f, 22f, ProjectileID.ShadowBeamFriendly, shadowbeamDamage, 7f, Player.whoAmI);
                            if (beam.whoAmI.WithinBounds(Main.maxProjectiles))
                            {
                                beam.DamageType = DamageClass.Generic;
                                beam.usesLocalNPCImmunity = true;
                                beam.localNPCHitCooldown = 10;
                            }
                        }
                        for (int l = 0; l < 5; l++)
                        {
                            int scytheDamage = (int)Player.GetBestClassDamage().ApplyTo(DemonshadeHelm.ScytheDamage);

                            Projectile scythe = CalamityUtils.ProjectileRain(source, Player.Center, 400f, 100f, 500f, 800f, 22f, ProjectileID.DemonScythe, scytheDamage, 7f, Player.whoAmI);
                            if (scythe.whoAmI.WithinBounds(Main.maxProjectiles))
                            {
                                scythe.DamageType = DamageClass.Generic;
                                scythe.usesLocalNPCImmunity = true;
                                scythe.localNPCHitCooldown = 10;
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Kill Player
        public void KillPlayer()
        {
            var source = Player.GetSource_Death();
            Player.lastDeathPostion = Player.Center;
            Player.lastDeathTime = DateTime.Now;
            Player.showLastDeath = true;
            int coinsOwned = (int)Utils.CoinsCount(out bool flag, Player.inventory, new int[0]);
            if (Main.myPlayer == Player.whoAmI)
            {
                Player.lostCoins = coinsOwned;
                Player.lostCoinString = Main.ValueToCoins(Player.lostCoins);
            }
            if (Main.myPlayer == Player.whoAmI)
            {
                Main.mapFullscreen = false;
            }
            if (Main.myPlayer == Player.whoAmI)
            {
                Player.trashItem.SetDefaults(0, false);
                if (Player.difficulty == PlayerDifficultyID.SoftCore || Player.difficulty == PlayerDifficultyID.Creative)
                {
                    for (int i = 0; i < 59; i++)
                    {
                        if (Player.inventory[i].stack > 0 && ((Player.inventory[i].type >= ItemID.LargeAmethyst && Player.inventory[i].type <= ItemID.LargeDiamond) || Player.inventory[i].type == ItemID.LargeAmber))
                        {
                            int droppedLargeGem = Item.NewItem(source, (int)Player.position.X, (int)Player.position.Y, Player.width, Player.height, Player.inventory[i].type, 1, false, 0, false, false);
                            Main.item[droppedLargeGem].netDefaults(Player.inventory[i].netID);
                            Main.item[droppedLargeGem].Prefix((int)Player.inventory[i].prefix);
                            Main.item[droppedLargeGem].stack = Player.inventory[i].stack;
                            Main.item[droppedLargeGem].velocity.Y = (float)Main.rand.Next(-20, 1) * 0.2f;
                            Main.item[droppedLargeGem].velocity.X = (float)Main.rand.Next(-20, 21) * 0.2f;
                            Main.item[droppedLargeGem].noGrabDelay = 100;
                            Main.item[droppedLargeGem].favorited = false;
                            Main.item[droppedLargeGem].newAndShiny = false;
                            if (Main.netMode == NetmodeID.MultiplayerClient)
                            {
                                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, droppedLargeGem, 0f, 0f, 0f, 0, 0, 0);
                            }
                            Player.inventory[i].SetDefaults(0, false);
                        }
                    }
                }
                else if (Player.difficulty == PlayerDifficultyID.MediumCore)
                {
                    Player.DropItems();
                }
                else if (Player.difficulty == PlayerDifficultyID.Hardcore)
                {
                    Player.DropItems();
                    Player.KillMeForGood();
                }
            }
            SoundEngine.PlaySound(SoundID.PlayerKilled, Player.Center);
            Player.headVelocity.Y = (float)Main.rand.Next(-40, -10) * 0.1f;
            Player.bodyVelocity.Y = (float)Main.rand.Next(-40, -10) * 0.1f;
            Player.legVelocity.Y = (float)Main.rand.Next(-40, -10) * 0.1f;
            Player.headVelocity.X = (float)Main.rand.Next(-20, 21) * 0.1f + (float)(2 * 0);
            Player.bodyVelocity.X = (float)Main.rand.Next(-20, 21) * 0.1f + (float)(2 * 0);
            Player.legVelocity.X = (float)Main.rand.Next(-20, 21) * 0.1f + (float)(2 * 0);
            if (Player.stoned)
            {
                Player.headPosition = Vector2.Zero;
                Player.bodyPosition = Vector2.Zero;
                Player.legPosition = Vector2.Zero;
            }
            for (int j = 0; j < 100; j++)
            {
                Dust.NewDust(Player.position, Player.width, Player.height, DustID.LifeDrain, (float)(2 * 0), -2f, 0, default, 1f);
            }
            Player.mount.Dismount(Player);
            Player.dead = true;
            Player.respawnTimer = 600;
            if (Main.expertMode)
            {
                Player.respawnTimer = (int)(Player.respawnTimer * 1.5);
            }
            Player.immuneAlpha = 0;
            Player.palladiumRegen = false;
            Player.iceBarrier = false;
            Player.crystalLeaf = false;

            PlayerDeathReason damageSource = PlayerDeathReason.ByOther(Player.Male ? 14 : 15);
            if (abyssDeath)
            {
                SoundEngine.PlaySound(DrownSound, Player.Center);
                damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.AbyssDrown" + Main.rand.Next(1, 3 + 1)).ToNetworkText(Player.name));
            }
            else if (CalamityWorld.armageddon && areThereAnyDamnBosses)
            {
                damageSource = PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.Armageddon").ToNetworkText(Player.name));
            }

            NetworkText deathText = damageSource.GetDeathText(Player.name);
            if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI == Main.myPlayer)
            {
                NetMessage.SendPlayerDeath(Player.whoAmI, damageSource, (int)1000.0, 0, false, -1, -1);
            }
            if (Main.dedServ)
            {
                ChatHelper.BroadcastChatMessage(deathText, new Color(225, 25, 25));
            }
            else if (Main.netMode == NetmodeID.SinglePlayer)
            {
                Main.NewText(deathText.ToString(), 225, 25, 25);
            }

            if (Player.whoAmI == Main.myPlayer && (Player.difficulty == PlayerDifficultyID.SoftCore || Player.difficulty == PlayerDifficultyID.Creative))
            {
                Player.DropCoins();
            }
            Player.DropTombstone(coinsOwned, deathText, 0);

            if (Player.whoAmI == Main.myPlayer)
            {
                try
                {
                    WorldGen.saveToonWhilePlaying();
                }
                catch
                {
                }
            }
        }
        #endregion

        #region Defense Damage Functions
        /// <summary>
        /// Deals Calamity defense damage to a player the "normal way", using an incoming hit.<br />
        /// This is the convenience function which follows all standard Calamity balancing rules for taking a regular hit.
        /// </summary>
        /// <param name="hurtInfo">HurtInfo of the incoming strike to the player.</param>
        public void DealDefenseDamage(Player.HurtInfo hurtInfo)
        {
            // Legacy safeguard: Skip defense damage if the player is somehow "hit for zero" (this should never happen).
            if (hurtInfo.Damage <= 0 || hurtInfo.SourceDamage <= 0)
                return;

            // Under typical circumstances, defense damage scales with "net mitigation", aka how much damage the player DIDN'T take.
            // Thematically, this means it scales with how much damage the player's defense took instead of them.
            int netMitigation = hurtInfo.SourceDamage - hurtInfo.Damage;
            int incomingDamageToUse = netMitigation <= 0 ? 0 : netMitigation;

            // Leave it to the direct function to determine how much defense damage is taken. Use standard ratios.
            DealDefenseDamage(incomingDamageToUse, false);
        }

        /// <summary>
        /// Deals Calamity defense damage to a player, specifically built to handle Chalice of the Blood God's bleedout.
        /// </summary>
        /// <param name="hurtInfo">HurtInfo of the incoming strike to the player.</param>
        /// <param name="bleedoutApplied">The bleedout applied on this specific hit. Used for reducing the defense damage inflicted.</param>
        public void DealDefenseDamage(Player.HurtInfo hurtInfo, int bleedoutApplied)
        {
            // Legacy safeguard: Skip defense damage if the player is somehow "hit for zero" (this should never happen).
            if (hurtInfo.Damage <= 0 || hurtInfo.SourceDamage <= 0)
                return;

            // Under typical circumstances, defense damage scales with "net mitigation", aka how much damage the player DIDN'T take.
            // Thematically, this means it scales with how much damage the player's defense took instead of them.
            // Chalice of the Blood God makes you take much less direct damage than you should, which would catastrophically inflate defense damage.
            //
            // Subtract the bleedout applied on this hit from the net mitigation.
            // This prevents Chalice from making the player take much more defense damage than intended.
            int netMitigation = hurtInfo.SourceDamage - (hurtInfo.Damage + bleedoutApplied);
            int incomingDamageToUse = netMitigation <= 0 ? 0 : netMitigation;

            // Leave it to the direct function to determine how much defense damage is taken. Use standard ratios.
            DealDefenseDamage(incomingDamageToUse, false);
        }

        /// <summary>
        /// Deals Calamity defense damage to a player. This is the direct function, for unusual sources of defense damage.
        /// </summary>
        /// <param name="incomingDamage">The amount of defense damage to deal.</param>
        /// <param name="absolute">If true, deals exactly the specified defense damage, ignoring the standard ratios and Draedon's Heart.<br />
        /// Setting this to false is equivalent to considering the first parameter as standard incoming damage to the player.<br />
        /// Setting this to true bypasses the defense damage floor, and can thus inflict less defense damage than is typically allowed.</param>
        public void DealDefenseDamage(int incomingDamage, bool absolute = false)
        {
            // If absolute is specified, then ignore the ratio and always inflict EXACTLY THAT MUCH defense damage.
            // This means it bypasses Draedon's Heart!
            double ratioToUse = absolute ? 1D : defenseDamageRatio;

            // Intended amount of defense damage to take. Can round up, but can also be overwritten by the floor.
            int defenseDamageTaken = (int)Math.Round(incomingDamage * ratioToUse);

            // There is a floor on defense damage based on difficulty; i.e. there is a minimum amount of defense damage from any hit that can deal defense damage.
            // This floor is only applied if bosses are alive, but is bypassed by the absolute flag.
            // Details on the floor can be seen in the BalancingConstants file.
            bool useDefenseDamageFloor = areThereAnyDamnBosses && !absolute;
            if (useDefenseDamageFloor)
            {
                int defenseDamageFloor = CalamityUtils.GetDefenseDamageFloor();

                // Apply floor
                if (defenseDamageTaken < defenseDamageFloor)
                    defenseDamageTaken = defenseDamageFloor;
            }

            // The amount of defense damage taken is now final. Apply it.
            ApplyDefenseDamageInternal(defenseDamageTaken);
        }

        // Actually applies defense damage. Really should not be called externally.
        private void ApplyDefenseDamageInternal(int defenseDamage, bool showVisuals = true)
        {
            // If zero defense damage is being dealt, don't waste your time or display a grey 0.
            if (defenseDamage <= 0)
                return;

            // There are two flags which disable the application of defense damage. If either is true, don't do anything.
            bool externalFlagsAppropriate = !CalamityMod.ExternalFlag_DisableDefenseDamage && !externalDefenseDamageImmunity;
            if (!externalFlagsAppropriate)
                return;

            // Can be dynamically reduced by Adamantite set bonus and maybe other future effects.
            int defenseDamageTaken = defenseDamage;

            // Apply incoming defense damage to the Adamantite armor set bonus.
            if (AdamantiteSetDefenseBoost > 0)
            {
                int defenseDamageToAdamantite = Math.Min(AdamantiteSetDefenseBoost, defenseDamageTaken);
                AdamantiteSetDefenseBoost -= defenseDamageToAdamantite;

                // Reduce remaining defense damage by whatever was applied to Adamantite armor.
                defenseDamageTaken -= defenseDamageToAdamantite;

                // If Adamantite Armor's set bonus entirely absorbed the defense damage, then display the number and play the sound,
                // but don't actually reduce defense or trigger the defense damage recovery cooldown.
                if (defenseDamageTaken <= 0)
                {
                    ShowDefenseDamageEffects(defenseDamageToAdamantite);
                    return;
                }
            }

            // Apply incoming defense damage on top of whatever defense damage the player currently has.
            int previousDefenseDamage = CurrentDefenseDamage;
            totalDefenseDamage = previousDefenseDamage + defenseDamageTaken;

            // Safety check to prevent illegal recovery time
            if (defenseDamageRecoveryFrames < 0)
                defenseDamageRecoveryFrames = 0;

            // Directly add the base defense damage recovery time to whatever recovery time the player already has.
            totalDefenseDamageRecoveryFrames = defenseDamageRecoveryFrames + DefenseDamageBaseRecoveryTime;
            if (totalDefenseDamageRecoveryFrames > DefenseDamageMaxRecoveryTime)
                totalDefenseDamageRecoveryFrames = DefenseDamageMaxRecoveryTime;

            // Reset any recovery progress they may have already made.
            // They start the new recovery timer from the beginning.
            defenseDamageRecoveryFrames = totalDefenseDamageRecoveryFrames;

            // Reset the delay between iframes ending and defense damage recovery starting.
            defenseDamageDelayFrames = DefenseDamageRecoveryDelay;

            if (showVisuals)
                ShowDefenseDamageEffects(defenseDamage);
        }

        // Displays visuals for taking defense damage.
        private void ShowDefenseDamageEffects(int defenseDamage)
        {
            // Play a sound from taking defense damage.
            if (hurtSoundTimer == 0 && Main.myPlayer == Player.whoAmI)
            {
                double maxVolumeDefenseDamageScalar = Main.masterMode ? 0.7 : CalamityWorld.death ? 0.6 : CalamityWorld.revenge ? 0.55 : Main.expertMode ? 0.5 : 0.4;
                float maxVolumeDefenseDamage = (float)Math.Round(Player.statDefense * maxVolumeDefenseDamageScalar);
                float minVolume = 0.5f;
                float maxVolume = 1f;
                float lerpAmount = MathHelper.Clamp(defenseDamage / maxVolumeDefenseDamage, 0f, 1f);
                float defenseDamageSoundVolumeMultiplier = MathHelper.Lerp(minVolume, maxVolume, lerpAmount);
                SoundEngine.PlaySound(DefenseDamageSound with { Volume = DefenseDamageSound.Volume * defenseDamageSoundVolumeMultiplier }, Player.Center);
                hurtSoundTimer = 30;
            }

            // Display text indicating that defense damage was taken.
            string text = (-defenseDamage).ToString();
            Color messageColor = Color.LightGray;
            Rectangle location = new Rectangle((int)Player.position.X, (int)Player.position.Y - 16, Player.width, Player.height);
            CombatText.NewText(location, messageColor, Language.GetTextValue(text));
        }
        #endregion

        #region Adrenaline Loss Function
        /// <summary>
        /// Causes the player to lose Adrenaline based on an incoming hit. The behavior differs based on energy shields or Draedon's Heart.
        /// </summary>
        /// <param name="hurtInfo">The incoming damage event to the player.</param>
        /// <param name="fullyAbsorbedByShield">Whether or not the hit was fully absorbed by one or more energy shields. Tends to halve Adrenaline loss.</param>
        private void LoseAdrenalineOnHurt(Player.HurtInfo hurtInfo, bool fullyAbsorbedByShield = false)
        {
            // Being hit for zero from Paladin's Shield damage share has no effects on Adrenaline, regardless of all other circumstances.
            // Likewise, being struck while Adrenaline is actively burning has no effects on Adrenaline.
            if (hurtInfo.Damage <= 0 || adrenalineModeActive)
                return;

            // Draedon's Heart pauses for half the usual duration on a shield hit.
            // Otherwise, nothing happens here because no Adrenaline is actually lost.
            if (draedonsHeart)
            {
                int pauseTime = fullyAbsorbedByShield ? DraedonsHeart.NanomachinePauseAfterShieldDamage : DraedonsHeart.NanomachinePauseAfterDamage;
                adrenalinePauseTimer += pauseTime;
            }

            // Standard Adrenaline behavior
            else
            {
                // Regular Adrenaline pauses on any hit, even if you lose all Adrenaline.
                adrenalinePauseTimer += BalancingConstants.AdrenalinePauseAfterDamage;

                // Play a sound if Adrenaline was lost from full (this means Adrenaline DR helped mitigate the hit).
                // If this occurs, since Adrenaline DR helped mitigate the hit's damage, we can't allow the amount of Adrenaline lost to actually be reduced.
                bool hitAtFullAdrenaline = adrenaline >= adrenalineMax;
                if (hitAtFullAdrenaline)
                {
                    SoundEngine.PlaySound(Main.zenithWorld ? AdrenalineHurtGFB : AdrenalineHurtSound, Player.Center);
                    adrenaline = 0f;
                    return;
                }

                // 19MAR2024: Ozzatron: Chalice makes you lose adrenaline based on the damage you would have suffered in total.
                int damageToUse = hurtInfo.Damage;
                if (chaliceOfTheBloodGod && chaliceHitOriginalDamage > 0)
                {
                    damageToUse = chaliceHitOriginalDamage;
                    // Maybe at some point in the future, tracking this value will be useful elsewhere. Until then, it's only used here, so it is reset here.
                    chaliceHitOriginalDamage = 0;
                }

                // Calculate the amount of Adrenaline to lose. This is done in 3 steps:
                // 1. Find out how much %HP the player lost (or was absorbed by a shield).
                // 2. Use an inverse lerp to calculate the Adrenaline loss scaling down for very small hits (5% HP or less).
                // 3. Re-scale the lerp result into a % of Adrenaline loss from 10% (min loss) to 100%.
                float damageMaxHPRatio = (float)damageToUse / Player.statLifeMax2;
                float smallHitAdrenalineLossRatio = (float)Utils.GetLerpValue(0f, BalancingConstants.AdrenalineFalloffTinyHitHealthRatio, damageMaxHPRatio, true);
                float adrenalineLossFraction = MathHelper.Lerp(BalancingConstants.MinimumAdrenalineLoss, 1f, smallHitAdrenalineLossRatio);
                float adrenalineToLose = adrenaline * adrenalineLossFraction;

                // If the hit was fully absorbed by a shield, then lose half that much instead.
                if (fullyAbsorbedByShield)
                    adrenalineToLose /= 2f;

                // Actually subtract Adrenaline.
                adrenaline -= adrenalineToLose;
                if (adrenaline < 0f)
                    adrenaline = 0f;
            }
        }
        #endregion
    }
}
