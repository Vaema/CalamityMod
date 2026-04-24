using CalamityMod.Items.PermanentBoosters;
using CalamityMod.Items.Potions.Food;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using Terraria.DataStructures;

namespace CalamityMod.Items.Tools
{
    public class DisgustingMeat : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Tools";

        public override void SetDefaults()
        {
            Item.DefaultToFood(26, 36, 0, 0);
            Item.value = 0;
            Item.UseSound = SoundID.NPCHit20;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool? UseItem(Player player)
        {
            if (player.ItemTimeIsZero)
            {
                DisgustingMeatAnimationPlayer modPlayer = player.GetModPlayer<DisgustingMeatAnimationPlayer>();
                if (player.altFunctionUse == 2)
                    modPlayer.EjectMiscUpgrades = true;
                modPlayer.DoingVomitAnimation = true;
                return true;
            }

            return null;
        }
    }

    public class DisgustingMeatAnimationPlayer : ModPlayer
    {
        public static int VomitEjectTime => 75;

        public static int VomitMaxTime => 130;

        public bool EjectMiscUpgrades = false;

        public bool DoingVomitAnimation = false;

        public int VomitTime = 0;

        public override void UpdateDead()
        {
            DoingVomitAnimation = false;
            EjectMiscUpgrades = false;
            VomitTime = 0;
        }

        public override void PostUpdateMiscEffects()
        {
            if (Player.whoAmI != Main.myPlayer)
                return;

            if (DoingVomitAnimation)
            {
                // Eject all necessary items at once.
                if (VomitTime == VomitEjectTime)
                {
                    EjectPermanentUpgrades();
                    SoundEngine.PlaySound(SoundID.NPCDeath13, Player.Center);
                }

                // Vomit particles bleeehhhhggghghghgh
                if (VomitTime >= VomitEjectTime && VomitTime <= VomitMaxTime)
                {
                    if (Main.rand.NextBool(2))
                    {
                        int dustAmt = Main.rand.Next(2, 5);
                        for (int i = 0; i < dustAmt; i++)
                        {
                            int dustType = Utils.SelectRandom(Main.rand, DustID.ToxicBubble, DustID.GreenBlood, DustID.Blood);
                            Vector2 spawnPosition = Player.Center + new Vector2(9f * Player.direction, -8f);
                            Vector2 velocity = Vector2.UnitX.RotatedByRandom(MathHelper.ToRadians(20f) + Player.headRotation) * Main.rand.NextFloat(6f, 8f) * Player.direction;
                            Dust.NewDust(spawnPosition, 1, 1, dustType, velocity.X, velocity.Y, Scale: Main.rand.NextFloat(0.8f, 1.2f));
                        }
                    }

                    if (Main.rand.NextBool(3))
                    {
                        Vector2 spawnPosition = Player.Center - new Vector2(9f * Player.direction, 8f);
                        Vector2 velocity = Vector2.UnitX.RotatedByRandom(MathHelper.ToRadians(20f) + Player.headRotation) * Main.rand.NextFloat(6f, 8f) * Player.direction;
                        Color color = Color.Lerp(Color.DarkOliveGreen, Color.Green, Main.rand.NextFloat());
                        float rotationSpeed = Main.rand.NextFloat(0.01f, 0.03f) * Main.rand.NextBool().ToDirectionInt();

                        TimedSmokeParticle vomit = new(spawnPosition, velocity, color, color, Main.rand.NextFloat(0.3f, 0.5f), Main.rand.NextFloat(0.8f, 1f), Main.rand.Next(30, 45), rotationSpeed);
                        GeneralParticleHandler.SpawnParticle(vomit, true);
                    }
                }

                if (VomitTime >= VomitMaxTime)
                {
                    VomitTime = 0;
                    DoingVomitAnimation = false;
                    EjectMiscUpgrades = false;
                }

                VomitTime++;
            }
        }

        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (VomitTime >= 0 && DoingVomitAnimation)
            {
                // Rapidly shift between random angles while vomiting.
                float interpolant = Utils.GetLerpValue(0f, VomitEjectTime, VomitTime, true) * Utils.GetLerpValue(VomitMaxTime, VomitMaxTime - 15, VomitTime, true);
                float idealRotationDegrees = MathHelper.Lerp(0f, 15f, interpolant);
                drawInfo.drawPlayer.headRotation = MathHelper.ToRadians(Main.rand.NextFloat(-idealRotationDegrees, idealRotationDegrees));

                // Close the eyes as well.
                drawInfo.drawPlayer.eyeHelper.CurrentEyeFrame = Terraria.GameContent.PlayerEyeHelper.EyeFrame.EyeHalfClosed;
                if (VomitTime >= VomitEjectTime)
                    drawInfo.drawPlayer.eyeHelper.CurrentEyeFrame = Terraria.GameContent.PlayerEyeHelper.EyeFrame.EyeClosed;
            }
        }

        private void EjectPermanentUpgrades()
        {
            var calPlayer = Player.Calamity();
            if (EjectMiscUpgrades)
            {
                // Rage and Adrenaline upgrades.
                TryDropBoosterItem(ref calPlayer.rageBoostOne, ModContent.ItemType<MushroomPlasmaRoot>());
                TryDropBoosterItem(ref calPlayer.rageBoostTwo, ModContent.ItemType<InfernalBlood>());
                TryDropBoosterItem(ref calPlayer.rageBoostThree, ModContent.ItemType<RedLightningContainer>());
                TryDropBoosterItem(ref calPlayer.adrenalineBoostOne, ModContent.ItemType<ElectrolyteGelPack>());
                TryDropBoosterItem(ref calPlayer.adrenalineBoostTwo, ModContent.ItemType<StarlightFuelCell>());
                TryDropBoosterItem(ref calPlayer.adrenalineBoostThree, ModContent.ItemType<Ectoheart>());

                // Celestal Onion.
                TryDropBoosterItem(ref calPlayer.extraAccessoryML, ModContent.ItemType<CelestialOnion>());

                // Demon Heart.
                TryDropBoosterItem(ref Player.extraAccessory, ItemID.DemonHeart);

                // Shimmer upgrades.
                if (Player.usedAegisCrystal || Player.usedAegisFruit || Player.usedArcaneCrystal || Player.usedAmbrosia || Player.usedGalaxyPearl || Player.usedGummyWorm || NPC.peddlersSatchelWasUsed || NPC.combatBookWasUsed || NPC.combatBookVolumeTwoWasUsed)
                {
                    TryDropBoosterItem(ref Player.usedAegisCrystal, ItemID.AegisCrystal);
                    TryDropBoosterItem(ref Player.usedAegisFruit, ItemID.AegisFruit);
                    TryDropBoosterItem(ref Player.usedAmbrosia, ItemID.Ambrosia);
                    TryDropBoosterItem(ref Player.usedGalaxyPearl, ItemID.GalaxyPearl);
                    TryDropBoosterItem(ref Player.usedGummyWorm, ItemID.GummyWorm);

                    // Sync the world data immediately for these three specifically since they aren't fully player side.
                    if (NPC.peddlersSatchelWasUsed || NPC.combatBookWasUsed || NPC.combatBookVolumeTwoWasUsed)
                    {
                        TryDropBoosterItem(ref NPC.peddlersSatchelWasUsed, ItemID.PeddlersSatchel);
                        TryDropBoosterItem(ref NPC.combatBookWasUsed, ItemID.CombatBook);
                        TryDropBoosterItem(ref NPC.combatBookVolumeTwoWasUsed, ItemID.CombatBookVolumeTwo);
                        NetMessage.SendData(MessageID.WorldData);
                    }
                }

                // Artisan Loaf.
                TryDropBoosterItem(ref Player.ateArtisanBread, ItemID.ArtisanLoaf);

                // Torch God's Favor.
                TryDropBoosterItem(ref Player.unlockedBiomeTorches, ItemID.TorchGodsFavor);

                // Minecart Upgrade Kit.
                TryDropBoosterItem(ref Player.unlockedSuperCart, ItemID.MinecartPowerup);
            }
            else
            {
                // Calamity health booster fruits.
                if (calPlayer.sTangerine || calPlayer.mFruit || calPlayer.tCloudberry || calPlayer.sStrawberry)
                {
                    TryDropBoosterItem(ref calPlayer.sTangerine, ModContent.ItemType<SanguineTangerine>());
                    TryDropBoosterItem(ref calPlayer.mFruit, ModContent.ItemType<MiracleFruit>());
                    TryDropBoosterItem(ref calPlayer.tCloudberry, ModContent.ItemType<TaintedCloudberry>());
                    TryDropBoosterItem(ref calPlayer.sStrawberry, ModContent.ItemType<SacredStrawberry>());
                }

                // Life fruit.
                if (Player.ConsumedLifeFruit > 0)
                {
                    for (int i = 0; i < Player.ConsumedLifeFruit; i++)
                    {
                        int drop = Player.QuickSpawnItem(Player.GetSource_DropAsItem(), ItemID.LifeFruit);
                        Main.item[drop].noGrabDelay = 100;
                        Main.item[drop].velocity = new Vector2(Main.rand.NextFloat(3f, 9f) * Player.direction, Main.rand.NextFloat(-6f, -4f));
                    }
                    Player.ConsumedLifeFruit = 0;
                }

                // Heart crystals.
                if (Player.ConsumedLifeCrystals > 0)
                {
                    for (int j = 0; j < Player.ConsumedLifeCrystals; j++)
                    {
                        int drop = Player.QuickSpawnItem(Player.GetSource_DropAsItem(), ItemID.LifeCrystal);
                        Main.item[drop].noGrabDelay = 100;
                        Main.item[drop].velocity = new Vector2(Main.rand.NextFloat(3f, 9f) * Player.direction, Main.rand.NextFloat(-6f, -4f));
                    }
                    Player.ConsumedLifeCrystals = 0;
                }

                // Calamity mana upgrades.
                if (calPlayer.cShard || calPlayer.eCore || calPlayer.pHeart)
                {
                    TryDropBoosterItem(ref calPlayer.cShard, ModContent.ItemType<CometShard>());
                    TryDropBoosterItem(ref calPlayer.eCore, ModContent.ItemType<EtherealCore>());
                    TryDropBoosterItem(ref calPlayer.pHeart, ModContent.ItemType<PhantomHeart>());
                }

                // Mana Crystals.
                if (Player.ConsumedManaCrystals > 0)
                {
                    for (int k = 0; k < Player.ConsumedManaCrystals; k++)
                    {
                        int drop = Player.QuickSpawnItem(Player.GetSource_DropAsItem(), ItemID.ManaCrystal);
                        Main.item[drop].noGrabDelay = 100;
                        Main.item[drop].velocity = new Vector2(Main.rand.NextFloat(3f, 9f) * Player.direction, Main.rand.NextFloat(-6f, -4f));
                    }
                    Player.ConsumedManaCrystals = 0;
                }
            }

            NetMessage.SendData(MessageID.SyncPlayer, -1, -1, null, Player.whoAmI);
        }

        private void TryDropBoosterItem(ref bool condition, int itemType)
        {
            if (condition)
            {
                condition = false;
                int drop = Player.QuickSpawnItem(Player.GetSource_DropAsItem(), itemType);
                Main.item[drop].noGrabDelay = 100;
                Main.item[drop].velocity = new Vector2(Main.rand.NextFloat(3f, 9f) * Player.direction, Main.rand.NextFloat(-6f, -4f));
            }
        }
    }
}
