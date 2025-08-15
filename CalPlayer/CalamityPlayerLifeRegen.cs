using System;
using System.Linq;
using CalamityMod.Buffs.Alcohol;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.Placeables;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.CalPlayer.Dashes;
using CalamityMod.Cooldowns;
using CalamityMod.Enums;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Accessories.Wings;
using CalamityMod.Items.Armor.Reaver;
using CalamityMod.Items.Fishing.BrimstoneCragCatches;
using CalamityMod.Items.Placeables.Furniture;
using CalamityMod.Items.Potions;
using CalamityMod.Items.Potions.Alcohol;
using CalamityMod.NPCs;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Systems;
using CalamityMod.Systems.Collections;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.CalPlayer
{
    public partial class CalamityPlayer : ModPlayer
    {
        #region Update Bad Life Regen
        public override void UpdateBadLifeRegen()
        {
            // Universal +25% increase to DoT debuff damage in Death Mode
            float deathNegativeRegenBonus = 0.25f;
            float calamityDebuffMultiplier = 1f + (CalamityWorld.death ? deathNegativeRegenBonus : 0f);

            // Cumulative amount of DoT debuff negative life regen from Calamity debuffs (or changes to vanilla debuffs)
            float totalNegativeLifeRegen = 0;

            #region Damage over Time Debuffs (Negative Life Regen)

            // Vanilla debuffs (+25% damage over time in Death Mode is applied here)
            if (CalamityWorld.death)
            {
                int totalVanillaDoT = 0;

                if (Player.poisoned && !purity)
                    totalVanillaDoT += 4;

                if (Player.onFire && !purity)
                    totalVanillaDoT += 8;

                if (Player.tongued)
                    totalVanillaDoT += 100;

                if (Player.venom && !purity)
                    totalVanillaDoT += 12;

                if (Player.onFrostBurn && !purity)
                    totalVanillaDoT += 12;

                if (Player.onFire2 && !purity)
                    totalVanillaDoT += 12;

                if (Player.burned)
                    totalVanillaDoT += 60;

                if (Player.suffocating)
                    totalVanillaDoT += 40;

                if (Player.electrified && !purity)
                {
                    totalVanillaDoT += eleResist ? 4 : 8;
                    if (Player.controlLeft || Player.controlRight)
                        totalVanillaDoT += eleResist ? 16 : 32;
                }

                // Tally up total current vanilla DoT so it can be added as extra DoT from Death Mode
                totalNegativeLifeRegen += totalVanillaDoT * deathNegativeRegenBonus;
            }

            //
            // Calamity debuffs (Vanilla Shadowflame and Daybroken are added here)
            //
            void ApplyDoTDebuff(bool hasDebuff, int negativeLifeRegenToApply, bool immuneCondition = false)
            {
                if (!hasDebuff || immuneCondition)
                    return;

                if (Player.lifeRegen > 0)
                    Player.lifeRegen = 0;

                Player.lifeRegenTime = 0;
                totalNegativeLifeRegen += negativeLifeRegenToApply * calamityDebuffMultiplier;
            }

            // Whispering Death sets positive regen to zero but doesn't actually deal any damage
            ApplyDoTDebuff(whisperingDeath, 0, laudanum);

            ApplyDoTDebuff(irradiated, 4, purity);
            int sulphurDoT = 6 - (sulphurSet ? 2 : 0) - (sulphurskin ? 2 : 0) - (corrosiveSpine ? 2 : 0);
            ApplyDoTDebuff(sulphurPoison, sulphurDoT, purity);
            ApplyDoTDebuff(riptide, 6, purity);
            ApplyDoTDebuff(weakBrimstoneFlames, 7);
            ApplyDoTDebuff(burningBlood, 8, purity);
            ApplyDoTDebuff(brainRot, 8, purity);
            ApplyDoTDebuff(vaporfied, 8, purity);
            int staticDoT = ((Player.controlLeft || Player.controlRight) ? 12 : 3) / (eleResist ? 2 : 1);
            ApplyDoTDebuff(staticDischarge, staticDoT, purity);
            ApplyDoTDebuff(heavybleeding, 16, purity);
            ApplyDoTDebuff(crushDepth, 18, purity);
            ApplyDoTDebuff(astralInfection, 24, infectedJewel || hideOfDeus || purity);
            ApplyDoTDebuff(shadowflame, 30, purity);
            ApplyDoTDebuff(brimstoneFlames, abaddon ? 15 : 30, purity);
            ApplyDoTDebuff(plague, alchFlask ? 15 : 30, purity);
            ApplyDoTDebuff(vHex, 30); // Has other effects
            ApplyDoTDebuff(searingLava, 30);
            ApplyDoTDebuff(demonicFlames, 33, purity); // Never inflicted on the player
            ApplyDoTDebuff(laceration, 36, purity);
            ApplyDoTDebuff(daybroken, reducedDaybrokenDamage ? 20 : 40, purity);
            ApplyDoTDebuff(nightwither, reducedNightwitherDamage ? 20 : 40, purity);
            ApplyDoTDebuff(holyFlames, 40, purity);
            ApplyDoTDebuff(voidfrost, 40, purity);
            ApplyDoTDebuff(hadopelagicPressure, 40, purity);

            // Profaned Soul Crystal turns you into Providence, a God, and you take more damage from God Slayer Inferno
            ApplyDoTDebuff(godSlayerInferno, profanedCrystalBuffs ? 50 : 40);
            int fluxDoT = ((Player.controlLeft || Player.controlRight) ? 50 : 10) / (eleResist ? 2 : 1);
            ApplyDoTDebuff(vermillionFlux, fluxDoT);
            ApplyDoTDebuff(elementalMix, 50, purity); // Never inflicted on the player
            ApplyDoTDebuff(trueVHex, 50);
            int dragonfireDoT = ((Player.name == "JFL" || Player.name == "MrJFL") ? 200 : 50) / (dynamoStemCells ? 2 : 1);
            ApplyDoTDebuff(dragonFire, dragonfireDoT);
            ApplyDoTDebuff(miracleBlight, 60);
            ApplyDoTDebuff(banishingFire, 60); // Never inflicted on the player
            int rebukeDoT = ((Player.controlLeft || Player.controlRight) ? 80 : 16) / (eleResist ? 2 : 1);
            ApplyDoTDebuff(auricRebuke, rebukeDoT);

            // Slowly increase the sulphuric water poisoning effect. Once it's high enough, the player takes damage and the meter resets.
            bool nearSafeZone = false;
            if (SulphuricWaterSafeZoneSystem.NearbySafeTiles.Count >= 1)
            {
                Point closestSafeZone = SulphuricWaterSafeZoneSystem.NearbySafeTiles.Keys.OrderBy(t => t.ToVector2().DistanceSQ(Player.Center / 16f)).First();
                if (Vector2.Distance(Player.Center.ToTileCoordinates().ToVector2(), closestSafeZone.ToVector2()) < SulphuricWaterSafeZoneSystem.NearbySafeTiles[closestSafeZone] * 17f)
                    nearSafeZone = true;
            }

            float ASPoisonLevel = 0f;
            if (CalamityGlobalNPC.aquaticScourge >= 0 && Main.zenithWorld)
            {
                NPC AS = Main.npc[CalamityGlobalNPC.aquaticScourge];
                float scoogDistance = Vector2.Distance(Player.Center, AS.Center);
                // GFB Aquatic Scourge poisons you if:
                // 1. You are over 50 blocks away from the head
                // 2. You are under 250 blocks away from the head (so that people halfway across the world aren't getting killed for no reason)
                // 3. Aquatic Scourge has been damaged
                if (AS.life < AS.lifeMax && scoogDistance < 4000f)
                    ASPoisonLevel = Utils.GetLerpValue(800f, 1600f, scoogDistance, true);
            }

            bool ASPoisoning = ASPoisonLevel > 0f;
            if (ASPoisoning || ((ZoneSulphur || ZoneAbyssLayer1) && !Player.creativeGodMode && Player.IsUnderwater() && !decayEffigy && !abyssalDivingSuit && !Player.lavaWet && !Player.honeyWet && !nearSafeZone))
            {
                float increment = 1f / SulphSeaWaterSafetyTime;
                //No way to mitigate AS Poisoning
                if (ASPoisoning)
                    increment *= 3f + (6f * ASPoisonLevel);
                if (sulphurskin && !ASPoisoning)
                    increment *= 0.5f;
                if (sulphurSet && !ASPoisoning)
                    increment *= 0.5f;
                if (corrosiveSpine && !ASPoisoning)
                    increment *= 0.5f;
                if (ZoneAbyssLayer1 && !ASPoisoning)
                    increment *= 0.33f;

                SulphWaterPoisoningLevel = MathHelper.Clamp(SulphWaterPoisoningLevel + increment, 0f, 1f);
                if (SulphWaterPoisoningLevel >= 1f)
                {
                    SulphWaterPoisoningLevel = 0f;
                    Player.Hurt(PlayerDeathReason.ByCustomReason(CalamityUtils.GetText("Status.Death.SulphurMeter").ToNetworkText(Player.name)), Math.Min(Player.statLifeMax2 / 4, 150), 0);
                }
            }
            else
                SulphWaterPoisoningLevel = MathHelper.Clamp(SulphWaterPoisoningLevel - 1f / SulphSeaWaterRecoveryTime, 0f, 1f);
            #endregion

            #region Alcohol
            for (int l = 0; l < Player.MaxBuffs; l++)
            {
                int buff = Player.buffType[l];
                if (CalamityBuffSets.AlcoholStrength.TryGetValue(buff, out int level))
                    alcoholPoisonLevel += level;
            }
            if (vodka)
                totalNegativeLifeRegen += Vodka.RegenLoss;
            if (redWine)
                totalNegativeLifeRegen += baguette ? Baguette.RedWineBuffedRegenLoss : RedWine.RegenLoss;
            if (moonshine)
                totalNegativeLifeRegen += Moonshine.RegenLoss;
            if (fireball)
                totalNegativeLifeRegen += Fireball.RegenLoss;
            if (everclear)
                totalNegativeLifeRegen += Everclear.RegenLoss;
            if (bloodyMary)
                totalNegativeLifeRegen += BloodyMary.RegenLoss;
            if (tequila)
                totalNegativeLifeRegen += Tequila.RegenLoss;
            if (tequilaSunrise)
                totalNegativeLifeRegen += TequilaSunrise.RegenLoss;
            if (screwdriver)
                totalNegativeLifeRegen += Screwdriver.RegenLoss;
            if (margarita)
                totalNegativeLifeRegen += Margarita.RegenLoss;
            if (starBeamRye)
                totalNegativeLifeRegen += StarBeamRye.RegenLoss;
            if (moscowMule)
                totalNegativeLifeRegen += MoscowMule.RegenLoss;
            if (whiteWine)
                totalNegativeLifeRegen += WhiteWine.RegenLoss;
            if (evergreenGin)
                totalNegativeLifeRegen += EvergreenGin.RegenLoss;

            // Blanket effect for all alcohols
            if (alcoholPoisonLevel > 0)
            {
                // This applies the tipsy eyes effect
                Player.tipsy = true;

                // This one is checked through a buff so we have to counter that
                if (!Player.HasBuff(BuffID.Tipsy))
                    Player.fishingSkill += 5;

            }
            if (alcoholPoisonLevel > 3)
            {
                // Independently of Calamity's nerfs to Nebula life regen, it is disabled entirely by alcohol poisoning.
                Player.nebulaLevelLife = 0;

                // This has to last over 60 frames for the nurse to count the debuff, so...
                if (Player.whoAmI == Main.myPlayer)
                    Player.AddBuff(ModContent.BuffType<AlcoholPoisoning>(), 61, false);

                if (Player.lifeRegen > 0)
                    Player.lifeRegen = 0;

                Player.lifeRegenTime = 0;
                totalNegativeLifeRegen += 3 * alcoholPoisonLevel;
            }
            #endregion

            if (brimflameFrenzy)
            {
                Player.manaRegen = 0;
                Player.manaRegenBonus = 0;
                Player.manaRegenDelay = (int)Player.maxRegenDelay;
                if (Player.lifeRegen > 0)
                    Player.lifeRegen = 0;
                totalNegativeLifeRegen += 42; //the meaning of death
            }

            if (witheredDebuff)
            {
                witheredWeaponHoldTime += witheringWeaponEnchant.ToDirectionInt();
                if (witheredWeaponHoldTime < 0)
                {
                    witheredWeaponHoldTime = 0;
                }
                else
                {
                    totalNegativeLifeRegen += (int)(5D * Math.Pow(1.5D, witheredWeaponHoldTime / 87D));
                    if (Player.lifeRegen > 0)
                        Player.lifeRegen = 0;
                }
            }
            else
                witheredWeaponHoldTime = 0;

            if (Player.statMana < 0)
            {
                totalNegativeLifeRegen -= Player.statMana/10f;
            }

            //
            // ACTUALLY APPLY NEGATIVE LIFE REGEN
            //

            // At the last second, Reaver defense helm reduces DoT debuffs by 20%
            if (reaverDefense)
                totalNegativeLifeRegen -= (int)(totalNegativeLifeRegen * ReaverHeadTank.SetBonusDebuffDamageReduction);

            Player.lifeRegen -= (int)totalNegativeLifeRegen;

            #region Life Regen That Works Even During DoT Debuffs

            // Honey Dew (and upgrades)
            if (alwaysHoneyRegen)
            {
                // Exact copy of vanilla Honey behavior, but does not stack with actually standing in Honey
                if (!Player.honey)
                {
                    alwaysHoneyRegenAmount += 1;
                    Player.lifeRegen += 2;
                    Player.lifeRegenTime += 1;

                    // Grants +2 life regen if negative life regen would otherwise occur.
                    // However, this can't bring regen into the positives.
                    if (Player.lifeRegen < 0)
                    {
                        alwaysHoneyRegenAmount += Math.Min(1f, 0 - Player.lifeRegen/2f);
                        Player.lifeRegen += 2;
                        if (Player.lifeRegen > 0)
                            Player.lifeRegen = 0;
                    }
                }
            }

            if (honeyDewHalveDebuffs)
            {
                // Tick down all sickness debuffs; this makes them expire 2x faster
                // Upgrades increase the sets of debuffs which expire faster
                for (int l = 0; l < Player.MaxBuffs; ++l)
                {
                    int buffID = Player.buffType[l];
                    if (Player.buffTime[l] <= 2)
                        continue;
                    bool shouldHalveDuration = CalamityBuffSets.IsSicknessDebuff[buffID];
                    if (livingDewHalveDebuffs)
                        shouldHalveDuration |= CalamityBuffSets.IsFireDebuff[buffID];
                    if (purity)
                        shouldHalveDuration |= CalamityBuffSets.IsDebuff[buffID];

                    if (shouldHalveDuration)
                        --Player.buffTime[l];
                }
            }

            if (divineBless)
            {
                if (Player.whoAmI == Main.myPlayer && Player.miscCounter % 15 == 0) // Flat 4 health per second
                {
                    if (!noLifeRegen)
                        Player.HealPlayer(1, HealTextType.None);
                }
            }

            if (bloodfinBoost)
            {
                if (Player.lifeRegen < 0)
                {
                    if (Player.lifeRegenTime < Bloodfin.DebuffedRegenTimeFloor)
                        Player.lifeRegenTime = Bloodfin.DebuffedRegenTimeFloor;

                    Player.lifeRegen += Bloodfin.DebuffedRegenBoost;
                }
                else
                {
                    Player.lifeRegen += Bloodfin.RegenBoost;
                    Player.lifeRegenTime += Bloodfin.RegenTimeBoost;
                }

                if (bloodfinTimer > 0)
                    bloodfinTimer--;

                if (Player.whoAmI == Main.myPlayer && bloodfinTimer <= 0)
                {
                    bloodfinTimer = Bloodfin.FramesForExtraRegen;

                    if (Player.statLife < (int)(Player.statLifeMax2 * Bloodfin.ExtraRegenHealthThreshold) && !noLifeRegen)
                        Player.HealPlayer(1, HealTextType.None);
                }
            }

            // Permafrost's Concoction increases life regen while afflicted with a fire debuff
            if (permafrostsConcoction && Player.buffType.Any(l => CalamityBuffSets.IsFireDebuff[l]))
            {
                if (Player.lifeRegenTime < 900)
                    Player.lifeRegenTime = 900;

                Player.lifeRegen += 6;
            }

            // Grant life regen based on missing health for Radiant Ooze, Ambrosial Ampule, and purity
            if (rOoze || aAmpoule || purity)
            {
                float missingLifeRatio = (Player.statLifeMax2 - Player.statLife) / (float)Player.statLifeMax2;
                //Ambrosial Ampule and ooze give between 2 and 6 hp/s
                int lifeRegenToGive = (int)Math.Round(MathHelper.Lerp((purity || aAmpoule? 2f : 4f), (purity || aAmpoule ? 10f : 12f), missingLifeRatio));//Rounding is needed for it to ever actually give +6 hp/s, as the integer conversion would otherwise floor it.
                Player.lifeRegen += lifeRegenToGive; 
                radiantOozeRegen += lifeRegenToGive / 2f;
                ambrosialAmpouleRegen += lifeRegenToGive / 2f;
                purityRegen += lifeRegenToGive / 2f;
            }

            if (purity)
            {
                int intendedPurityDefense = 0;
                int currentDebuffs = Player.buffType.Count(i => CalamityBuffSets.IsDebuff[i]);
                if (currentDebuffs > 0)
                {
                    // Healing rate is normally 5 HP/s (+1 every 12 frames)
                    // However, that 12 frames can and will slowly increase if you try to abuse this accessory
                    int healFrameCadence = 12;

                    // Healing slows down after 5 seconds (300 frames) debuffed. For every 15 frames thereafter the cadence slows
                    // The upper limit to how slow it can get is after 15 seconds (900 frames)
                    int punishmentFrames = PurityHealSlowdownFrames - 300;
                    //lowest punishment is a little under a second between the one health heal
                    if (healFrameCadence < 52)
                        healFrameCadence += (punishmentFrames < 0) ? 0 : punishmentFrames / 15;

                    if (Player.miscCounter % healFrameCadence == healFrameCadence - 1)
                        Player.Heal(1);

                    if (Player.lifeRegenTime < 900)
                        Player.lifeRegenTime = 900;

                    intendedPurityDefense = 15 + (currentDebuffs - 1) * 5;
                    if (jewelBonusDefense < intendedPurityDefense)
                        jewelBonusDefense = intendedPurityDefense;

                    // Count up total frames spent healing for slowdown.
                    if (PurityHealSlowdownFrames < 900)
                        ++PurityHealSlowdownFrames;
                    purityRegen += (60 / (float)healFrameCadence);
                }

                // If the defense should be ticking down to some lower value, do that.
                // Purity loses 1 point of defense every second.
                if (Player.miscCounter % 60 == 0 && jewelBonusDefense > intendedPurityDefense)
                    --jewelBonusDefense;

                // If the player is clear of all debuffs then gradually reduce the slowdown frames
                if (currentDebuffs <= 0)
                {
                    --PurityHealSlowdownFrames;
                    if (PurityHealSlowdownFrames < 0)
                        PurityHealSlowdownFrames = 0;
                }

                // Actually apply defense bonus
                Player.statDefense += jewelBonusDefense;
            }

            // Infected Jewel does not stack with Purity
            else if (infectedJewel)
            {
                Player.lifeRegen += 2;

                // If the player has any debuffs, give the extra life regen and defense
                // More defense is given for each additional debuff
                int intendedJewelDefense = 0;
                int currentDebuffs = Player.buffType.Count(i => CalamityBuffSets.IsDebuff[i]);
                if (currentDebuffs > 0)
                {
                    Player.lifeRegen += 4;
                    if (Player.lifeRegenTime < 900)
                        Player.lifeRegenTime = 900;

                    intendedJewelDefense = 12 + (currentDebuffs - 1) * 4;
                    if (jewelBonusDefense < intendedJewelDefense)
                        jewelBonusDefense = intendedJewelDefense;
                }

                // If the defense should be ticking down to some lower value, do that.
                // Infected Jewel loses 1 point of defense every 20 frames.
                if (Player.miscCounter % 60 == 0 && jewelBonusDefense > intendedJewelDefense)
                    --jewelBonusDefense;

                // Actually apply defense bonus
                Player.statDefense += jewelBonusDefense;
            }

            // Crown Jewel does not stack with Purity or Infected Jewel
            else if (crownJewel)
            {
                Player.lifeRegen += 2;

                // If any debuff is detected, provide even more life regen and massively accelerate it
                if (Player.buffType.Any(i => CalamityBuffSets.IsDebuff[i]))
                {
                    Player.lifeRegen += 3;
                    if (Player.lifeRegenTime < 900)
                        Player.lifeRegenTime = 900;
                }
            }
            #endregion

            // During Silva revive or God Slayer dash, all negative life regen is canceled
            if ((silvaCountdown > 0 && hasSilvaEffect && silvaSet) || (LastUsedDashID == GodslayerArmorDash.ID && Player.dashDelay < 0))
            {
                if (Player.lifeRegen < 0)
                    Player.lifeRegen = 0;
            }

            #region Things That Disable Even That Life Regen
            //
            // Yes, really, there's a list of conditions under which life regen doesn't work
            // even if it's life regen that normally works during a damage over time debuff.
            //
            // 1. No life regen bool (Omega Blue armor)
            // 2. Being too far from Providence cocoon ("Holy Inferno")
            // 3. Air drowning in the Abyss
            //

            // All forms of overtly disabling life regeneration disable Nebula Life boosters as well.

            if (noLifeRegen)
            {
                Player.nebulaLevelLife = 0;

                if (Player.lifeRegen > 0)
                    Player.lifeRegen = 0;

                Player.lifeRegenTime = 0;

                if (Player.lifeRegenCount > 0)
                    Player.lifeRegenCount = 0;
            }

            if (holyInferno)
            {
                Player.nebulaLevelLife = 0;

                hInfernoBoost++;

                if (Player.lifeRegen > 0)
                    Player.lifeRegen = 0;

                Player.lifeRegenTime = 0;
                Player.lifeRegen -= (int)(hInfernoBoost * calamityDebuffMultiplier);

                if (Player.lifeRegen < -200)
                    Player.lifeRegen = -200;
            }
            else
                hInfernoBoost = 0;

            if (ZoneAbyss)
            {
                if (!Player.IsUnderwater())
                {
                    if (Player.statLife > 100)
                    {
                        Player.nebulaLevelLife = 0;

                        if (Player.lifeRegen > 0)
                            Player.lifeRegen = 0;

                        Player.lifeRegenTime = 0;
                        Player.lifeRegen -= (int)(160D * calamityDebuffMultiplier);
                    }
                }
            }
            #endregion

            // Chalice of the Blood God bleedout
            // The bleedout is applied by directly reducing the player's health. It is not canceled by anything.
            ChaliceOfTheBloodGod.HandleBleedout(Player);
        }
        #endregion

        #region Update Life Regen
        public override void UpdateLifeRegen()
        {
            if (rum)
                Player.lifeRegen += Rum.RegenBoost;

            if (caribbeanRum)
                Player.lifeRegen += CaribbeanRum.RegenBoost;

            if (mushy)
                Player.lifeRegen += Mushy.RegenBoost;

            if (permafrostsConcoction)
            {
                if (Player.statLife < actualMaxLife / 2)
                    Player.lifeRegen++;
                if (Player.statLife < actualMaxLife / 4)
                    Player.lifeRegen++;
                if (Player.statLife < actualMaxLife / 10)
                    Player.lifeRegen += 2;
            }

            if (tRegen)
                Player.lifeRegen += 3;

            if (sRegen)
                Player.lifeRegen += SpiritGlyph.RegenBoost;

            if (PinkJellyRegen)
                Player.lifeRegen += LifeJelly.AuraRegenBoost;

            if (GreenJellyRegen)
                Player.lifeRegen += Items.Accessories.GrandGelatin.AuraRegenBoost;

            if (AbsorberRegen)
                Player.lifeRegen += TheAbsorber.AuraRegenBoost;

            if (hallowedRegen)
                Player.lifeRegen += HallowedRune.RegenBoost;

            if (affliction || afflicted)
                Player.lifeRegen += 1;

            if (trinketOfChi || chiRegen)
                Player.lifeRegen += 2;

            if (evolutionLifeRegenCounter > 0)
            {
                if (Player.lifeRegenTime < 3600f)
                    Player.lifeRegenTime = 3600f;
            }

            if (darkSunRing)
            {
                if (Main.eclipse || Main.dayTime)
                    Player.lifeRegen += Main.eclipse ? 2 : 4;
            }

            if (silvaSet)
                Player.lifeRegen += 6;

            if (phantomicHeartRegen <= 720 && phantomicHeartRegen >= 600)
            {
                Player.lifeRegen += PhantomicArtifact.RegenBoost;
                if (Main.rand.NextBool())
                {
                    Dust regen = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.Blood, 0f, 0f, 200, new Color(99, 54, 84), 2f);
                    regen.noGravity = true;
                    regen.fadeIn = 1.3f;
                    Vector2 velocity = CalamityUtils.RandomVelocity(100f, 50f, 100f, 0.04f);
                    regen.velocity = velocity;
                    velocity.Normalize();
                    velocity *= 34f;
                    regen.position = Player.Center - velocity;
                }
            }

            if (community)
            {
                int regenBoost = 1 + (int)(TheCommunity.CalculatePower() * TheCommunity.RegenMultiplier);
                bool lesserEffect = false;
                for (int l = 0; l < Player.MaxBuffs; l++)
                {
                    int hasBuff = Player.buffType[l];
                    lesserEffect = CalamityBuffSets.AlcoholStrength.TryGetValue(hasBuff, out var a);
                }
                if (Player.lifeRegen < 0)
                    Player.lifeRegen += lesserEffect ? 1 : regenBoost;
            }

            if (handWarmer && eskimoSet)
            {
                Player.lifeRegen += 2;
            }
            if (avertorBonus)
            {
                Player.lifeRegen += 4;
            }

            if (bloodflareSummon)
            {
                if (Player.statLife <= (int)(actualMaxLife * 0.5))
                    Player.lifeRegen += 2;
            }

            if (fearmongerSet && fearmongerRegenFrames > 0)
            {
                Player.lifeRegen += 7;

                if (Player.lifeRegenTime < 900)
                    Player.lifeRegenTime = 900;

                Player.lifeRegenTime += 4;
            }

            if (pinkCandle && !noLifeRegen)
            {
                // Every frame, add up 1/60th of the healing value (0.4% max HP per second)
                pinkCandleHealFraction += Player.statLifeMax2 * VigorousCandle.PercentHealthPerSecond / 60;

                if (pinkCandleHealFraction >= 1D)
                {
                    pinkCandleHealFraction = 0D;
                    Player.HealPlayer(1, HealTextType.None);
                }
            }
            else
                pinkCandleHealFraction = 0D;

            if (manaOverloader)
            {
                float manaRatio = Player.statMana / (float)Player.statManaMax2;
                Player.lifeRegen += (int)(MathF.Round(MathHelper.Lerp(4f, -4f, manaRatio)) * (Player.HasBuff(BuffID.ManaSickness) ? 0.5f : 1f));
            }

            #region Standing Still Life Regen
            // Standing still healing bonuses (all are exclusive with vanilla Shiny Stone, but all function similarly)
            if (!Player.shinyStone && Player.StandingStill() && Player.velocity.Y == 0 && Player.itemAnimation == 0)
            {
                bool honeyDewWorking = honeyTurboRegen && Player.honeyWet;
                bool anyStandingStillLifeRegen = shadeRegen || cFreeze || honeyDewWorking || aAmpoule || purity;

                // Divides all negative life regen by two before applying any other effects.
                if (anyStandingStillLifeRegen && Player.lifeRegen < 0)
                    Player.lifeRegen /= 2;

                // Spawn dust of some flavor while actually regenerating, aAmpule and purity have a slightly different looking style
                if (Player.lifeRegen > 0 && Player.statLife < actualMaxLife)
                {
                    int dustType = shadeRegen ? 173 : cFreeze ? 67 : honeyDewWorking ? DustID.Honey2 : aAmpoule ? 228 : purity ? 187 : -1;
                    bool dustSpawnRolled = Main.rand.Next(30000) < Player.lifeRegenTime || purity ? Main.rand.NextBool() : aAmpoule ? Main.rand.NextBool(4) : Main.rand.NextBool(30);
                    if (dustType != -1 && dustSpawnRolled)
                    {
                        Dust regen = Dust.NewDustDirect(Player.position, Player.width, Player.height, dustType, 0f, 0f, purity || aAmpoule ? 80 : 200, default, purity || aAmpoule ? 0.5f : 1f);
                        regen.noGravity = true;
                        regen.fadeIn = 1.3f;
                        Vector2 velocity = CalamityUtils.RandomVelocity(100f, 50f, 100f, 0.04f);
                        regen.velocity = velocity;
                        velocity.Normalize();
                        velocity *= purity || aAmpoule ? 55f : 34f;
                        regen.position = Player.Center - velocity;
                    }
                }

                // Actually apply "standing still" regeneration (the stats are granted even at full health)
                float regenTimeNeededForTurboRegen = shadeRegen ? 40f : cFreeze ? 60f : honeyDewWorking ? 90f : aAmpoule ? 90f : purity ? 60f : -1f;

                // 4 = vanilla Shiny Stone
                int turboRegenPower = shadeRegen || cFreeze || purity ? 4 : honeyDewWorking || aAmpoule ? 3 : -1;

                if (turboRegenPower > 0)
                {
                    // After a brief delay determined by your form of standing still regen, min-cap life regen time at 900 / 3600.
                    if (Player.lifeRegenTime > regenTimeNeededForTurboRegen && Player.lifeRegenTime < 900f)
                        Player.lifeRegenTime = 900f;

                    Player.lifeRegen += turboRegenPower;
                    Player.lifeRegenTime += turboRegenPower;
                    purityRegen += turboRegenPower / 2f;
                    if (!shadeRegen || cFreeze || purity) ambrosialAmpouleRegen += turboRegenPower / 2f;
                }

            }
            #endregion

            if (regenerator) // Gives special regen of it's own, but disables all regular life regen
            {
                if (Player.miscCounter % 7 == 0 && Player.statLife < (int)(Player.statLifeMax2 * 0.5f))
                    Player.HealPlayer(1, HealTextType.None);

                // Boost life regen time quite a bit.
                // This is so that in events and such where small hits are common, your damage boost isn't completley negated
                if (Player.lifeRegenTime < 3600)
                    Player.lifeRegenTime += 10;
            }
            else
                regeneratorDamage = 0;

            if (toxicHeart) // Since it needs to know your life regen, it must be placed here
            {
                float minLifeRegen = -20; // Fastest rate
                float maxLifeRegen = 15; // Slowest rate
                int auraDamage = (int)Player.GetBestClassDamage().ApplyTo(200);
                var source = Player.GetSource_Accessory(FindAccessory(ModContent.ItemType<ToxicHeart>()));
                float lifeRegenRate = Utils.Remap(Player.lifeRegen, minLifeRegen, maxLifeRegen, 20, 1, true);

                if (pulseRate < lifeRegenRate) // Jump to fastest pulse rate and slowly slow down if life regen increases
                    pulseRate = lifeRegenRate;
                else
                    pulseRate = MathHelper.Lerp(pulseRate, lifeRegenRate, 0.002f);

                if (pulseCounter >= 420)
                {
                    Projectile.NewProjectile(source, Player.Center, Vector2.Zero, ModContent.ProjectileType<PlaguePulse>(), auraDamage, 0f, Player.whoAmI, 0, 0, 0);
                    pulseCounter = 0;
                    if (toxicHeartVisuals)
                    {
                        float soundVolume = Utils.Remap(Player.lifeRegen, minLifeRegen, maxLifeRegen, 1f, 0.3f, true);
                        SoundStyle heartbeat = new("CalamityMod/Sounds/Item/Heartbeat");
                        SoundEngine.PlaySound(heartbeat with { Volume = soundVolume, PitchVariance = 0.2f }, Player.Center);
                    }
                }
                else
                {
                    pulseCounter += MathHelper.Clamp(pulseRate, 1, 20);
                }
            }
        }
        #endregion

        public override void NaturalLifeRegen(ref float regen)
        {
            // The Camper counteracts the regen loss while moving horizontally
            if (camper && (Player.velocity.X != 0 && Player.grappling[0] <= 0))
            {
                // Normally 1.25 while resting and 0.5 while not so we apply this cancelling multiplier
                regen *= 2.5f;

                if (Main.rand.Next(30000) < Player.lifeRegenTime || Main.rand.NextBool())
                {
                    Dust heart = Dust.NewDustDirect(Player.position, Player.width, Player.height, DustID.HeartCrystal, 0f, 0f, 200, Color.OrangeRed, 1f);
                    heart.noGravity = true;
                    heart.fadeIn = 1.3f;
                    Vector2 velocity = CalamityUtils.RandomVelocity(100f, 50f, 100f, 0.04f);
                    heart.velocity = velocity;
                    velocity.Normalize();
                    velocity *= 34f;
                    heart.position = Player.Center - velocity;
                }
            }

            // Regenerator trades all positive regen for damage, and caps your health gain at 50%
            if (regenerator)
            {
                int finalRegen = Player.lifeRegen + (int)Math.Round(regen * (Player.statLifeMax2 / 400f * 0.85f + 0.15f));
                finalRegen = (int)Math.Max(finalRegen, 0);

                // Rapid Healing increments RegenCount directly so it needs to be manually added
                // It also works while debuffs are active so the same logic applies here
                if (Player.palladiumRegen)
                    finalRegen += 4;

                regeneratorDamage = (finalRegen * 1.75f) * 0.01f;
                Player.GetDamage<GenericDamageClass>() += regeneratorDamage;

                if (Player.lifeRegen > 0)
                    Player.lifeRegen = 0;
                if (regen > 0f)
                    regen = 0f;
                if (Player.lifeRegenCount > 0)
                    Player.lifeRegenCount = 0;

                //Hard-lock the player's health to 50%.
                //No lifesteal, no regen, no healing pots
                if (Player.statLife >= (int)(Player.statLifeMax2 * 0.5f))
                {
                    Player.statLife = (int)(Player.statLifeMax2 * 0.5f);
                    Player.moonLeech = true;
                    healingPotionMultiplier = 0;
                }
            }
        }
    }
}
