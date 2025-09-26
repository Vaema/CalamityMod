using System;
using System.Collections.Generic;
using System.Reflection;
using CalamityMod.Events;
using CalamityMod.Items.Fishing;
using CalamityMod.Items.Materials;
using CalamityMod.Items.TreasureBags.MiscGrabBags;
using CalamityMod.NPCs.AcidRain;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.TownNPCs;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Walls;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.GameInput;

namespace CalamityMod.ILEditing
{
    public partial class ILChanges
    {
        public static WindGrid Windgrid
        {
            get;
            internal set;
        }

        #region Decrease Sandstorm Wind Speed Requirement
        private static void DecreaseSandstormWindSpeedRequirement(ILContext il)
        {
            // Sandstorms don't rapidly diminish unless the wind speed is less than 0.2f instead of 0.6f.
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcR4(0.6f))) // The 0.6f wind speed check.
            {
                LogFailure("Decrease Sandstorm Wind Speed Requirement", "Could not locate the wind speed variable.");
                return;
            }
            cursor.Remove();
            cursor.Emit(OpCodes.Ldc_R4, 0.2f); // Change to 0.2f.
        }
        #endregion Decrease Sandstorm Wind Speed Requirement

        #region Reforge Requirement Relaxation
        private static void RelaxPrefixRequirements(ILContext il)
        {
            var cursor = new ILCursor(il);

            // Search for the first instance of Math.Round, which is used to round damage.
            // This one isn't edited, but hitting the Round function is the easiest way to get to the relevant part of the method.
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchCall("System.Math", "Round")))
            {
                LogFailure("Prefix Requirements", "Could not locate the damage Math.Round call.");
                return;
            }

            // Search for the branch-if-not-equal which checks whether the damage change rounds to nothing.
            ILLabel passesDamageCheck = null;
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchBneUn(out passesDamageCheck)))
            {
                LogFailure("Prefix Requirements", "Could not locate damage prefix failure branch.");
                return;
            }

            // Emit an unconditional branch which skips the damage check failure.
            cursor.Emit(OpCodes.Br_S, passesDamageCheck);

            // Search for the branch-if-not-equal which checks whether the use time change rounds to nothing.
            // If the change rounds to nothing, then it's equal, so the branch is NOT taken.
            // The branch skips over the "fail this prefix" code.
            ILLabel passesUseTimeCheck = null;
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchBneUn(out passesUseTimeCheck)))
            {
                LogFailure("Prefix Requirements", "Could not locate use time rounding equality branch.");
                return;
            }

            // To allow use-time affecting prefixes even on super fast weapons where they would round to nothing,
            // add another branch which skips over the "fail this prefix" code, given a custom condition.

            // Load the item itself onto the stack so that it becomes an argument for the following delegate.
            cursor.Emit(OpCodes.Ldarg_0);

            // Emit a delegate which returns whether the item's use time is 2, 3, 4 or 5.
            cursor.EmitDelegate<Func<Item, bool>>((Item i) => i.useAnimation >= 2 && i.useAnimation <= 5);

            cursor.Emit(OpCodes.Brtrue_S, passesUseTimeCheck);

            // Search for the branch-if-not-equal which checks whether the mana change rounds to nothing.
            ILLabel passesManaCheck = null;
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchBneUn(out passesManaCheck)))
            {
                LogFailure("Prefix Requirements", "Could not locate mana prefix failure branch.");
                return;
            }

            // Emit an unconditional branch which skips the mana check failure.
            cursor.Emit(OpCodes.Br_S, passesManaCheck);

            // Search for the instance field load which retrieves the item's knockback.
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdfld<Item>("knockBack")))
            {
                LogFailure("Prefix Requirements", "Could not locate knockback load instruction.");
                return;
            }

            // Search for the immediately-following constant load which pulls in 0.0.
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcR4(0f)))
            {
                LogFailure("Prefix Requirements", "Could not locate zero knockback comparison constant.");
                return;
            }

            // Completely nullify the knockback computation by replacing the check against 0 with a check against negative one million.
            // If you absolutely need to block knockback reforges for some reason, you can set your knockback to this value.
            cursor.Remove();
            cursor.Emit(OpCodes.Ldc_R4, -1000000f);
        }
        #endregion Reforge Requirement Relaxation

        #region Remove Forced Inaccuracy from Chain Gun and Gatligator
        private static void RemoveForcedInaccuracyFromChainGunAndGatligator(ILContext il)
        {
            var cursor = new ILCursor(il);

            // Go to the load of the Chain Gun's item ID (1929).
            if (!cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdcI4(1929)))
            {
                LogFailure("Remove Chain Gun and Gatligator Inaccuracy", "Could not locate the ID of the Chain Gun.");
                return;
            }

            // Change this item ID check to check for -1048576. This will never occur.
            cursor.Next.Operand = -1048576;

            // Go to the load of the Gatligator's item ID (2270).
            if (!cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdcI4(2270)))
            {
                LogFailure("Remove Chain Gun and Gatligator Inaccuracy", "Could not locate the ID of the Gatligator.");
                return;
            }

            // Change this item ID check to check for -1048576. This will never occur.
            cursor.Next.Operand = -1048576;
        }
        #endregion

        #region Prevention of Slime Rain Spawns When Near Bosses
        private static void PreventBossSlimeRainSpawns(On_NPC.orig_SlimeRainSpawns orig, int plr)
        {
            if (!Main.player[plr].Calamity().isNearbyBoss && CalamityServerConfig.Instance.BossZen)
                orig(plr);
        }
        #endregion Prevention of Slime Rain Spawns When Near Bosses

        #region Remove Expert Brain of Cthulhu Random Debuffs
        private static void RemoveExpertBrainRandomDebuffs(ILContext il)
        {
            // Remove Expert+ Brain of Cthulhu and Creeper random debuffs on hit.
            var cursor = new ILCursor(il);

            // Go to the check for Expert Mode.
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchCall<Main>("get_expertMode")))
            {
                LogFailure("Remove Expert Brain Random Debuffs", "Could not locate the Expert Mode check.");
                return;
            }

            // Remove the Expert Mode check, and in its place put a check for the Zenith seed (Get fixed boi).
            cursor.Emit(OpCodes.Pop);
            cursor.Emit(OpCodes.Ldsfld, typeof(Main).GetField("zenithWorld"));
        }
        #endregion

        #region Prevent Lava Slime Dropping Lava
        private static void PreventLavaSlimeLavaDrop(ILContext il)
        {
            // Disable Lava Slimes dropping lava if its respective config is enabled.
            var cursor = new ILCursor(il);

            // Go to the check for Remix world.
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdsfld<Main>("remixWorld")))
            {
                LogFailure("Prevent Lava Slime Dropping Lava", "Could not find the check for Remix World.");
                return;
            }

            // Replace the Remix world check with a check for the config.
            cursor.Remove();
            cursor.EmitDelegate<Func<bool>>(() => CalamityServerConfig.Instance.RemoveLavaDropsFromLavaSlimes);
        }
        #endregion

        #region Disable Detonating Bubble StrikeNPC Hardcoded Override
        private static void LetDetonatingBubblesTakeDamage(ILContext il)
        {
            // In vanilla's StrikeNPC function, Detonating Bubbles have a hardcoded type check which sets the damage of the strike to 0.
            // This IL edit disables that type check in Death Mode.
            var cursor = new ILCursor(il);

            // Go to the point after the check for the Detonating Bubble NPC ID.
            if (!cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdcR8(0.0)))
            {
                LogFailure("Let Detonating Bubbles Take Damage in Death", "Could not move after the NPC type check.");
                return;
            }

            // Define the label.
            var label = il.DefineLabel();

            // Add a branch if it is Death Mode.
            cursor.Emit(OpCodes.Ldsfld, typeof(CalamityWorld).GetField("death"));
            cursor.Emit(OpCodes.Brtrue, label);

            // Move to the point after Detonating Bubble changes are implemented to place the branch label.
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchStfld<NPC>("dontTakeDamage")))
            {
                LogFailure("Let Detonating Bubbles Take Damage in Death", "Could not move to after the Detonating Bubble logic.");
                return;
            }
            if (!cursor.TryGotoNext(MoveType.AfterLabel, i => i.MatchLdarg0()))
            {
                LogFailure("Let Detonating Bubbles Take Damage in Death", "Could not move to after the Detonating Bubble logic.");
                return;
            }
            cursor.MarkLabel(label);
        }
        #endregion

        #region Make Meteorite Explodable
        private static void MakeMeteoriteExplodable(ILContext il)
        {
            // Find the Tile ID of Meteorite and change it to something that doesn't matter.
            var cursor = new ILCursor(il);

            // There are two checks for the Meteorite Tile ID. The first one is required for the switch cases to function properly, so we need to move past it.
            ILLabel label = null; // pointless label for MatchBeq
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchBeq(out label)))
            {
                LogFailure("Make Meteorite Explodable", "Could not locate the branching instruction.");
                return;
            }

            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(TileID.Meteorite))) // The Meteorite Tile ID check.
            {
                LogFailure("Make Meteorite Explodable", "Could not locate the Meteorite Tile ID variable.");
                return;
            }
            cursor.Remove();
            cursor.Emit(OpCodes.Ldc_I4, TileID.HellstoneBrick); // This won't actually do anything since the ID is above Meteorite's and thus unreachable
        }
        #endregion

        #region Change Blood Moon Max HP Requirements
        private static void BloodMoonsRequire200MaxLife(ILContext il)
        {
            // Blood Moons only happen when the player has over 200 max life.
            var cursor = new ILCursor(il);
            // Find the moon phase check which will forward the cursor around the Blood Moon portion
            if (!cursor.TryGotoNext(MoveType.After, c => c.MatchLdsfld<Main>("moonPhase")))
            {
                LogFailure("Make Blood Moons Require 200 Max Life", "Could not locate the moon phase check.");
                return;
            }
            // Find the player check itself
            if (!cursor.TryGotoNext(MoveType.After, c => c.MatchCallOrCallvirt<Player>("get_ConsumedLifeCrystals")))
            {
                LogFailure("Make Blood Moons Require 200 Max Life", "Could not locate the Life Crystal check.");
                return;
            }
            // Find the >1 Life Crystal requirement
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(1)))
            {
                LogFailure("Make Blood Moons Require 200 Max Life", "Could not locate the Life Crystal requirement.");
                return;
            }
            cursor.Remove();
            // Change it to >4 Life Crystals, which effectively allows a Blood Moon at 200 natural health.
            cursor.Emit(OpCodes.Ldc_I4, 4);
        }
        #endregion Change Blood Moon Max HP Requirements

        #region Prevent Fossil Shattering
        private static void PreventFossilShattering(ILContext il)
        {
            // Find the Tile ID of Desert Fossil and change it to something that doesn't matter.
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchLdcI4(TileID.DesertFossil))) // The Desert Fossil Tile ID check.
            {
                LogFailure("Prevent Fossil Shattering", "Could not locate the Desert Fossil Tile ID variable.");
                return;
            }

            // Remove this value and replace it with a large number that will never be a valid tile ID.
            cursor.Remove();
            cursor.Emit(OpCodes.Ldc_I4, 40000);
        }
        #endregion

        #region Remove Hellforge Pickaxe Requirement
        private static int RemoveHellforgePickaxeRequirement(On_Player.orig_GetPickaxeDamage orig, Player self, int x, int y, int pickPower, int hitBufferIndex, Tile tileTarget)
        {
            if (tileTarget.TileType == TileID.Hellforge)
                pickPower = 65;

            return orig(self, x, y, pickPower, hitBufferIndex, tileTarget);
        }
        #endregion

        #region Remove Flail Throw Velocity Being Affected By Player Velocity
        private static void FlailsNoLongerAffectedByPlayerVelocity(On_Projectile.orig_AI_015_Flails orig, Projectile self)
        {
            orig(self);
            if (self.ai[0] == 1f && self.ai[1] == 0f)
                self.velocity -= Main.player[self.owner].velocity;
        }
        #endregion

        #region Allow Victide Bobber to Exist
        private static void WhitelistVictideBobber(ILContext il)
        {
            var cursor = new ILCursor(il);

            // Find the label which skips the "flag = true" that kills the projectile
            ILLabel flagStorage = null;
            if (!cursor.TryGotoNext(MoveType.After, x => x.MatchBeq(out flagStorage)))
            {
                LogFailure("Allow Victide Bobber to Exist", "Failed to properly navigate label to direct to");
                return;
            }

            // Properly perform the skip if the projectile type is the Victide Bobber
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.Emit(OpCodes.Ldfld, typeof(Projectile).GetField("type"));
            cursor.Emit(OpCodes.Ldc_I4, ModContent.ProjectileType<VictideBobber>());
            cursor.Emit(OpCodes.Beq_S, flagStorage);
        }
        #endregion

        #region Prevent Victide Bobber from Jammming
        private static bool PreventVictideBobberFromJamming(On_Player.orig_ItemCheck_CheckFishingBobbers orig, Player self, bool canUse)
        {
            // Run through the original stuff
            canUse = orig(self, canUse);

            int bobberCount = 0;
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (proj.active && proj.owner == self.whoAmI && proj.bobber)
                {
                    bobberCount++;
                    if (proj.type == ModContent.ProjectileType<VictideBobber>())
                    {
                        // Go back to casting if there's nothing loaded
                        if (proj.ai[1] == 0f)
                            proj.ai[0] = 0f;

                        // Allow you to still use the fishing rod
                        canUse = true;
                    }
                }
            }

            // Unless.. you have a bobber already that's NOT Victide, then back to disabling
            if (canUse && bobberCount > 1)
                canUse = false;

            return canUse;
        }
        #endregion

        #region Prevent UFO Mount from Dismounting in Water
        private static void PreventUFODismountInWater(ILContext il)
        {
            // Prevent the Cosmic Car Key's UFO mount from dismounting when the player is in water.
            var cursor = new ILCursor(il);

            // Unfortunately, the code responsible for this is 4000 lines into Player.Update, meaning that reaching it is far from simple.
            // The following method was the easiest way I could find to reach it:
            // Move to the third call of Mount.Dismount.
            for (int i = 0; i < 3; i++)
            {
                if (!cursor.TryGotoNext(MoveType.Before, i => i.MatchCallvirt<Mount>("Dismount")))
                {
                    LogFailure("Prevent UFO Dismounting in Water", "Could not reach the Dismount instruction.");
                    return;
                }
            }
            // Move the cursor backwards to place it right after the instruction which loads Main.myPlayer onto the stack.
            if (!cursor.TryGotoPrev(MoveType.After, i => i.MatchLdsfld<Main>("myPlayer")))
            {
                LogFailure("Prevent UFO Dismounting in Water", "Could not locate the myPlayer check.");
                return;
            }

            // Remove the instruction and replace it with the integer limit. The next instruction checks if this value is equal to Player.whoAmI.
            // Player.whoAmI will never be the integer limit, so the check will always fail and the UFO will not dismount.
            cursor.EmitPop();
            cursor.Emit(OpCodes.Ldc_I4, int.MaxValue);
        }
        #endregion Prevent UFO Mount from Dismounting in Water

        #region Color Blighted Gel
        private static void ColorBlightedGel(On_CommonCode.orig_ModifyItemDropFromNPC orig, NPC npc, int itemIndex)
        {
            orig(npc, itemIndex);

            Item item = Main.item[itemIndex];
            int itemID = item.type;
            bool colorWasChanged = false;

            if (itemID == ModContent.ItemType<BlightedGel>() && npc.type == ModContent.NPCType<CrimulanBlightSlime>())
            {
                item.color = new Color(1f, 0f, 0.16f, 0.6f);
                colorWasChanged = true;
            }
            if (itemID == ItemID.SharkFin && npc.type == ModContent.NPCType<Mauler>())
            {
                item.color = new Color(151, 115, 57, 255);
                colorWasChanged = true;
            }

            // Sync the color changes.
            if (colorWasChanged)
                NetMessage.SendData(MessageID.ItemTweaker, -1, -1, null, itemIndex, 1f);
        }
        #endregion Color Blighted Gel

        #region Improve Angler Quest Rewards
        private static void ImproveAnglerRewards(On_Player.orig_GetAnglerReward orig, Player self, NPC angler, int questItemType)
        {
            orig(self, angler, questItemType);

            EntitySource_Gift source = new EntitySource_Gift(angler);
            int questsDone = self.anglerQuestsFinished;
            float rarityReduction = 1f;
            rarityReduction = (questsDone <= 50) ? (rarityReduction - questsDone * 0.01f) : ((questsDone <= 100) ? (0.5f - (questsDone - 50) * 0.005f) : ((questsDone > 150) ? 0.15f : (0.25f - (questsDone - 100) * 0.002f)));
            rarityReduction *= 0.9f;
            rarityReduction *= (float)(self.currentShoppingSettings.PriceAdjustment + 1.0) / 2f;

            if (rarityReduction < 0.1f)
                rarityReduction = 0.1f;

            List<Item> rewardItems = new List<Item>();

            GetItemSettings anglerRewardSettings = GetItemSettings.NPCEntityToPlayerInventorySettings;

            Item item = new Item();

            // GUARANTEED REWARDS

            // BAIT
            switch (questsDone)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                    item = new Item();
                    item.SetDefaults(ItemID.Stinkbug);
                    item.stack = Main.rand.Next(2, 6);
                    break;

                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    item = new Item();
                    item.SetDefaults(ItemID.ApprenticeBait);
                    item.stack = Main.rand.Next(2, 6);
                    break;

                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                case 15:
                    item = new Item();
                    item.SetDefaults(Main.rand.NextBool() ? ItemID.Worm : ItemID.Maggot);
                    item.stack = Main.rand.Next(2, 6);
                    break;

                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                    item = new Item();
                    item.SetDefaults(ItemID.JourneymanBait);
                    item.stack = Main.rand.Next(2, 6);
                    break;

                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                    item = new Item();
                    item.SetDefaults(Main.rand.NextBool() ? ItemID.EnchantedNightcrawler : ItemID.Buggy);
                    item.stack = Main.rand.Next(2, 6);
                    break;

                case 27:
                case 28:
                case 29:
                case 30:
                    item = new Item();
                    item.SetDefaults(ItemID.MasterBait);
                    item.stack = Main.rand.Next(2, 6);
                    break;

                default:
                    item = new Item();
                    item.SetDefaults(ModContent.ItemType<GrandMarquisBait>());
                    item.stack = Main.rand.Next(2, 6);
                    break;
            }

            item.position = self.Center;
            Item item2 = self.GetItem(self.whoAmI, item, anglerRewardSettings);
            rewardItems.Add(item2);

            // COINS
            switch (questsDone)
            {
                case 0:
                case 1:
                case 2:
                case 3:
                case 4:
                    item = new Item();
                    item.SetDefaults(ItemID.GoldCoin);
                    break;

                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                    item = new Item();
                    item.SetDefaults(ItemID.GoldCoin);
                    item.stack = 2;
                    item = new Item();
                    item.SetDefaults(ItemID.SilverCoin);
                    item.stack = 50;
                    break;

                case 10:
                case 11:
                case 12:
                case 13:
                case 14:
                    item = new Item();
                    item.SetDefaults(ItemID.GoldCoin);
                    item.stack = 4;
                    break;

                case 15:
                case 16:
                case 17:
                case 18:
                case 19:
                    item = new Item();
                    item.SetDefaults(ItemID.GoldCoin);
                    item.stack = 6;
                    break;

                case 20:
                case 21:
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                case 27:
                case 28:
                case 29:
                    item = new Item();
                    item.SetDefaults(ItemID.GoldCoin);
                    item.stack = 8;
                    break;

                default:
                    item = new Item();
                    item.SetDefaults(ItemID.GoldCoin);
                    item.stack = 10;
                    break;
            }

            item.position = self.Center;
            item2 = self.GetItem(self.whoAmI, item, anglerRewardSettings);
            rewardItems.Add(item2);

            // PRIMARY ITEMS
            switch (questsDone)
            {
                case 0:
                case 1:
                    item = new Item();
                    item.SetDefaults(ModContent.ItemType<Spadefish>());
                    rewardItems.Add(item);
                    break;

                case 2:
                    item = new Item();
                    item.SetDefaults(ModContent.ItemType<StuffedFish>());
                    item.stack = Main.rand.Next(4, 10);
                    rewardItems.Add(item);
                    break;

                case 3:
                    item = new Item();
                    item.SetDefaults(ItemID.HighTestFishingLine);
                    rewardItems.Add(item);
                    break;

                case 4:
                    item = new Item();
                    item.SetDefaults(ItemID.FishHook);
                    rewardItems.Add(item);
                    break;

                case 5:
                    item = new Item();
                    item.SetDefaults(ItemID.FuzzyCarrot);
                    rewardItems.Add(item);
                    break;

                case 6:
                    item = new Item();
                    item.SetDefaults(ItemID.FishermansGuide);
                    rewardItems.Add(item);
                    break;

                case 7:
                    item = new Item();
                    item.SetDefaults(ItemID.FishCostumeMask);
                    rewardItems.Add(item);
                    item = new Item();
                    item.SetDefaults(ItemID.FishCostumeShirt);
                    rewardItems.Add(item);
                    item = new Item();
                    item.SetDefaults(ItemID.FishCostumeFinskirt);
                    rewardItems.Add(item);
                    item = new Item();
                    item.SetDefaults(ModContent.ItemType<SandyAnglingKit>());
                    rewardItems.Add(item);
                    break;

                case 8:
                    item = new Item();
                    item.SetDefaults(ItemID.FishMinecart);
                    rewardItems.Add(item);
                    break;

                case 9:
                    item = new Item();
                    item.SetDefaults(ItemID.SailfishBoots);
                    rewardItems.Add(item);
                    break;

                case 10:
                    item = new Item();
                    item.SetDefaults(ItemID.AnglerHat);
                    rewardItems.Add(item);
                    item = new Item();
                    item.SetDefaults(ItemID.AnglerVest);
                    rewardItems.Add(item);
                    item = new Item();
                    item.SetDefaults(ItemID.AnglerPants);
                    rewardItems.Add(item);
                    break;

                case 11:
                    item = new Item();
                    item.SetDefaults(ItemID.WeatherRadio);
                    rewardItems.Add(item);
                    break;

                case 12:
                    item = new Item();
                    item.SetDefaults(ItemID.FishingBobber);
                    rewardItems.Add(item);
                    break;

                case 13:
                    item = new Item();
                    item.SetDefaults(ItemID.SeashellHairpin);
                    rewardItems.Add(item);
                    item = new Item();
                    item.SetDefaults(ItemID.MermaidAdornment);
                    rewardItems.Add(item);
                    item = new Item();
                    item.SetDefaults(ItemID.MermaidTail);
                    rewardItems.Add(item);
                    item = new Item();
                    item.SetDefaults(ModContent.ItemType<SandyAnglingKit>());
                    rewardItems.Add(item);
                    break;

                case 14:
                    item = new Item();
                    item.SetDefaults(ItemID.Sextant);
                    rewardItems.Add(item);
                    break;

                case 15:
                    item = new Item();
                    item.SetDefaults(ItemID.TackleBox);
                    rewardItems.Add(item);
                    break;

                case 16:
                    item = new Item();
                    item.SetDefaults(ItemID.SuperAbsorbantSponge);
                    rewardItems.Add(item);
                    break;

                case 17:
                    item = new Item();
                    item.SetDefaults(ItemID.LavaFishingHook);
                    rewardItems.Add(item);
                    break;

                case 18:
                    item = new Item();
                    item.SetDefaults(ItemID.MagicConch);
                    rewardItems.Add(item);
                    break;

                case 19:
                    item = new Item();
                    item.SetDefaults(ItemID.DemonConch);
                    rewardItems.Add(item);
                    break;

                case 20:
                    item = new Item();
                    item.SetDefaults(ItemID.AnglerEarring);
                    rewardItems.Add(item);
                    break;

                case 21:
                    item = new Item();
                    item.SetDefaults(ItemID.HoneyAbsorbantSponge);
                    rewardItems.Add(item);
                    break;

                case 22:
                    item = new Item();
                    item.SetDefaults(ItemID.HotlineFishingHook);
                    rewardItems.Add(item);
                    break;

                case 23:
                    item = new Item();
                    item.SetDefaults(ItemID.FrogLeg);
                    rewardItems.Add(item);
                    break;

                case 24:
                    item = new Item();
                    item.SetDefaults(ItemID.SuperheatedBlood);
                    rewardItems.Add(item);
                    break;

                case 25:
                    item = new Item();
                    item.SetDefaults(ItemID.BottomlessBucket);
                    rewardItems.Add(item);
                    break;

                case 26:
                    item = new Item();
                    item.SetDefaults(ItemID.Sundial);
                    rewardItems.Add(item);
                    break;

                case 27:
                    item = new Item();
                    item.SetDefaults(ItemID.BottomlessHoneyBucket);
                    rewardItems.Add(item);
                    break;

                case 28:
                    item = new Item();
                    item.SetDefaults(ItemID.GoldenBugNet);
                    rewardItems.Add(item);
                    break;

                case 29:
                    item = new Item();
                    item.SetDefaults(ItemID.BottomlessLavaBucket);
                    rewardItems.Add(item);
                    break;

                case 30:
                    item = new Item();
                    item.SetDefaults(ItemID.GoldenFishingRod);
                    rewardItems.Add(item);
                    break;
            }

            // RANDOM DROPS

            // Angling Kits
            if (Main.rand.NextBool((int)(12f * rarityReduction)) && questsDone > 30)
            {
                item = new Item();
                item.SetDefaults(Main.hardMode ? ModContent.ItemType<BleachedAnglingKit>() : ModContent.ItemType<SandyAnglingKit>());
                rewardItems.Add(item);
            }

            // Golden Fishing Rod
            if (Main.rand.NextBool((int)(500f * rarityReduction)) && questsDone > 30)
            {
                item = new Item();
                item.SetDefaults(ItemID.GoldenFishingRod);
                rewardItems.Add(item);
            }

            // Hotline Fishing Hook
            if (Main.rand.NextBool((int)(200f * rarityReduction)) && questsDone > 22)
            {
                item = new Item();
                item.SetDefaults(ItemID.HotlineFishingHook);
                rewardItems.Add(item);
            }

            // Angler Set
            if (Main.rand.NextBool((int)(150f * rarityReduction)) && questsDone > 10)
            {
                item = new Item();
                item.SetDefaults(ItemID.AnglerHat);
                rewardItems.Add(item);
                item = new Item();
                item.SetDefaults(ItemID.AnglerVest);
                rewardItems.Add(item);
                item = new Item();
                item.SetDefaults(ItemID.AnglerPants);
                rewardItems.Add(item);
            }

            // Mermaid Set
            if (Main.rand.NextBool((int)(150f * rarityReduction)) && questsDone > 13)
            {
                item = new Item();
                item.SetDefaults(ItemID.SeashellHairpin);
                rewardItems.Add(item);
                item = new Item();
                item.SetDefaults(ItemID.MermaidAdornment);
                rewardItems.Add(item);
                item = new Item();
                item.SetDefaults(ItemID.MermaidTail);
                rewardItems.Add(item);
            }

            // Fish Set
            if (Main.rand.NextBool((int)(150f * rarityReduction)) && questsDone > 7)
            {
                item = new Item();
                item.SetDefaults(ItemID.FishCostumeMask);
                rewardItems.Add(item);
                item = new Item();
                item.SetDefaults(ItemID.FishCostumeShirt);
                rewardItems.Add(item);
                item = new Item();
                item.SetDefaults(ItemID.FishCostumeFinskirt);
                rewardItems.Add(item);
            }

            // Fin Wings
            if (Main.rand.NextBool((int)(140f * rarityReduction)) && Main.hardMode && questsDone > 10)
            {
                item = new Item();
                item.SetDefaults(ItemID.FinWings);
                rewardItems.Add(item);
            }

            // Bottomless Water Bucket
            if (Main.rand.NextBool((int)(140f * rarityReduction)) && questsDone > 25)
            {
                item = new Item();
                item.SetDefaults(ItemID.BottomlessBucket);
                rewardItems.Add(item);
            }

            // Bottomless Honey Bucket
            if (Main.rand.NextBool((int)(140f * rarityReduction)) && questsDone > 27)
            {
                item = new Item();
                item.SetDefaults(ItemID.BottomlessHoneyBucket);
                rewardItems.Add(item);
            }

            // Bottomless Lava Bucket
            if (Main.rand.NextBool((int)(140f * rarityReduction)) && questsDone > 29)
            {
                item = new Item();
                item.SetDefaults(ItemID.BottomlessLavaBucket);
                rewardItems.Add(item);
            }

            // Magic Conch
            if (Main.rand.NextBool((int)(140f * rarityReduction)) && questsDone > 18)
            {
                item = new Item();
                item.SetDefaults(ItemID.MagicConch);
                rewardItems.Add(item);
            }

            // Demon Conch
            if (Main.rand.NextBool((int)(140f * rarityReduction)) && questsDone > 19)
            {
                item = new Item();
                item.SetDefaults(ItemID.DemonConch);
                rewardItems.Add(item);
            }

            // Super Absorbant Sponge
            if (Main.rand.NextBool((int)(140f * rarityReduction)) && questsDone > 16)
            {
                item = new Item();
                item.SetDefaults(ItemID.SuperAbsorbantSponge);
                rewardItems.Add(item);
            }

            // Honey Absorbant Sponge
            if (Main.rand.NextBool((int)(140f * rarityReduction)) && questsDone > 21)
            {
                item = new Item();
                item.SetDefaults(ItemID.SuperAbsorbantSponge);
                rewardItems.Add(item);
            }

            // Golden Bug Net
            if (Main.rand.NextBool((int)(140f * rarityReduction)) && questsDone > 28)
            {
                item = new Item();
                item.SetDefaults(ItemID.GoldenBugNet);
                rewardItems.Add(item);
            }

            // Fish Hook
            if (Main.rand.NextBool((int)(120f * rarityReduction)) && questsDone > 4)
            {
                item = new Item();
                item.SetDefaults(ItemID.FishHook);
                rewardItems.Add(item);
            }

            // Minecarp
            if (Main.rand.NextBool((int)(120f * rarityReduction)) && questsDone > 8)
            {
                item = new Item();
                item.SetDefaults(ItemID.FishMinecart);
                rewardItems.Add(item);
            }

            // Lava Shark
            if (Main.rand.NextBool((int)(120f * rarityReduction)) && questsDone > 24)
            {
                item = new Item();
                item.SetDefaults(ItemID.SuperheatedBlood);
                rewardItems.Add(item);
            }

            // High Test Fishing Line
            if (Main.rand.NextBool((int)(80f * rarityReduction)) && questsDone > 3)
            {
                item = new Item();
                item.SetDefaults(ItemID.HighTestFishingLine);
                rewardItems.Add(item);
            }

            // Angler Earring
            if (Main.rand.NextBool((int)(80f * rarityReduction)) && questsDone > 20)
            {
                item = new Item();
                item.SetDefaults(ItemID.AnglerEarring);
                rewardItems.Add(item);
            }

            // Lavaproof Fishing Hook
            if (Main.rand.NextBool((int)(80f * rarityReduction)) && questsDone > 17)
            {
                item = new Item();
                item.SetDefaults(ItemID.LavaFishingHook);
                rewardItems.Add(item);
            }

            // Tackle Box
            if (Main.rand.NextBool((int)(80f * rarityReduction)) && questsDone > 15)
            {
                item = new Item();
                item.SetDefaults(ItemID.TackleBox);
                rewardItems.Add(item);
            }

            // Fisherman's Pocket Guide
            if (Main.rand.NextBool((int)(60f * rarityReduction)) && questsDone > 6)
            {
                item = new Item();
                item.SetDefaults(ItemID.FishermansGuide);
                rewardItems.Add(item);
            }

            // Weather Radio
            if (Main.rand.NextBool((int)(60f * rarityReduction)) && questsDone > 11)
            {
                item = new Item();
                item.SetDefaults(ItemID.WeatherRadio);
                rewardItems.Add(item);
            }

            // Sextant
            if (Main.rand.NextBool((int)(60f * rarityReduction)) && questsDone > 14)
            {
                item = new Item();
                item.SetDefaults(ItemID.Sextant);
                rewardItems.Add(item);
            }

            // Fishing Bobber
            if (Main.rand.NextBool((int)(50f * rarityReduction)) && questsDone > 12)
            {
                item = new Item();
                item.SetDefaults(ItemID.FishingBobber);
                rewardItems.Add(item);
            }

            PlayerLoader.AnglerQuestReward(self, rarityReduction, rewardItems);

            foreach (Item rewardItem in rewardItems)
            {
                rewardItem.position = self.Center;

                Item getItem = self.GetItem(self.whoAmI, rewardItem, GetItemSettings.NPCEntityToPlayerInventorySettings);

                if (getItem.stack > 0)
                {
                    int number = Item.NewItem(source, (int)self.position.X, (int)self.position.Y, self.width, self.height, getItem.type, getItem.stack, noBroadcast: false, 0, noGrabDelay: true);

                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, number, 1f);
                }
            }
        }
        #endregion

        #region Render Special Map Colors
        private static void UseVisibleThroughWaterMapTile(ILContext il)
        {
            var c = new ILCursor(il);

            if (!c.TryGotoNext(x => x.MatchCall<Tilemap>("get_Item")))
            {
                LogFailure("Use VisibleThroughWater Map Tile", "Could not locate call to Terraria.Map.TileMap::get_Item.");
                return;
            }
            
            int tileIndex = -1;
            if (!c.TryGotoNext(x => x.MatchStloc(out tileIndex)) || tileIndex == -1)
            {
                LogFailure("Use VisibleThroughWater Map Tile", "Could not determine the local variable index tile is pushed to.");
                return;
            }

            if (!c.TryGotoNext(x => x.MatchCall<Tile>("liquidType")))
            {
                LogFailure("Use VisibleThroughWater Map Tile", "Could not locate call to Terraria.Tile::liquidType.");
                return;
            }

            int liquidTypeIndex = -1;
            if (!c.TryGotoNext(x => x.MatchStloc(out liquidTypeIndex)) || liquidTypeIndex == -1)
            {
                LogFailure("Use VisibleThroughWater Map Tile", "Could not determine the local variable index liquidType is pushed to.");
                return;
            }

            int relativeMapTypeIndex = -1;
            if (!c.TryGotoNext(MoveType.After, x => x.MatchStloc(out relativeMapTypeIndex)) || relativeMapTypeIndex == -1)
            {
                LogFailure("Use VisibleThroughWater Map Tile", "Could not determine the local variable index of the relative map type.");
                return;
            }

            c.Emit(OpCodes.Ldloc_0);
            c.Emit(OpCodes.Ldloc, relativeMapTypeIndex);
            c.Emit(OpCodes.Ldloc, liquidTypeIndex);
            c.EmitDelegate(
                (Tile tile, int relativeMapType, int liquidType) =>
                {
                    if (liquidType != LiquidID.Water)
                        return relativeMapType;

                    if (WallLoader.GetWall(tile.WallType) is IVisibleThroughWater visibleThroughWater)
                        return visibleThroughWater.WaterMapEntry;

                    return relativeMapType;
                }
            );
            c.Emit(OpCodes.Stloc, relativeMapTypeIndex);
        }
        #endregion

        #region Make Magma Stone & Fire Gauntlet Dust Toggleable
        private static void MakeMagmaStoneFireGauntletDustToggleable(ILContext il)
        {
            // Allows Magma Stone and Fire Gauntlet's obnoxious dust on melee swings to be toggled off with visbility
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdfld<Player>("magmaStone"))) // Flag for if Magma Stone is equipped. Fire Gauntlet also uses this.
            {
                LogFailure("Make Magma Stone & Fire Gauntlet Dust Toggleable", "Could not locate the Magma Stone variable.");
                return;
            }
            // Load the player itself onto the stack so that it becomes an argument for the following delegate.
            cursor.Emit(OpCodes.Ldarg_0);

            // Emit a delegate which places whether the player has their Magma Stone visuals enabled onto the stack.
            cursor.EmitDelegate<Func<Player, bool>>(MagmaStoneVisualsEnabled);
            cursor.Emit(OpCodes.And);
        }

        private static readonly Func<Player, bool> MagmaStoneVisualsEnabled = (Player p) => p.Calamity().magmaStoneVisuals;

        private static void MakeMagmaStoneFireGauntletProjectileDustToggleable(ILContext il)
        {
            // Allows Magma Stone and Fire Gauntlet's obnoxious dust on projectiles to be toggled off with visbility
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdfld<Player>("magmaStone"))) // Flag for if Magma Stone is equipped. Fire Gauntlet also uses this.
            {
                LogFailure("Make Magma Stone & Fire Gauntlet Projectile Dust Toggleable", "Could not locate the magma stone variable.");
                return;
            }
            // Load the player itself onto the stack so that it becomes an argument for the following delegate.
            cursor.Emit(OpCodes.Ldloc_0);

            // Emit a delegate which places whether the player has their Magma Stone visuals enabled onto the stack.
            cursor.EmitDelegate<Func<Player, bool>>(MagmaStoneVisualsEnabled);
            cursor.Emit(OpCodes.And);
        }

        #endregion Make Magma Stone & Fire Gauntlet Dust Toggleable

        #region Vanilla Non-Linearity Fixes
        private static void RemovePowerCellPlanteraLock(ILContext il)
        {
            // Remove the check requiring Plantera to be defeated to use Lihzahrd Power Cells at the Altar.
            var cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(MoveType.After, i => i.MatchLdsfld<NPC>("downedPlantBoss")))
            {
                LogFailure("Remove Power Cell Plantera Lock", "Could not locate the downed Plantera bool.");
                return;
            }

            // Remove the instruction and replace with 1 (true). This effectively removes the requirement for defeating Plantera.
            // The only requirements for summoning Golem with Power Cells are now: 1) Golem is not alive, and 2) The world is in Hardmode.
            cursor.EmitPop();
            cursor.Emit(OpCodes.Ldc_I4_1);
        }

        private static bool RemoveUseLocks(On_Player.orig_ItemCheck_CheckCanUse orig, Player self, Item sItem)
        {
            if (sItem.type == ItemID.CelestialSigil)
                return !NPC.AnyNPCs(NPCID.MoonLordCore) && !BossRushEvent.BossRushActive;
            if (sItem.type == ItemID.SolarTablet)
                return Main.dayTime && !Main.eclipse && (Main.hardMode || NPC.downedMechBossAny || NPC.downedPlantBoss);

            return orig(self, sItem);
        }

        private static void ApplyCelestialSigilChanges(On_Player.orig_ItemCheck_UseEventItems orig, Player self, Item sItem)
        {
            if (self.ItemTimeIsZero && self.itemAnimation > 0 && sItem.type == ItemID.CelestialSigil)
            {
                if (NPC.AnyNPCs(NPCID.MoonLordCore) || BossRushEvent.BossRushActive)
                    return;

                SoundEngine.PlaySound(SoundID.Roar, self.Center);
                self.ApplyItemTime(sItem);

                if (Main.netMode != NetmodeID.MultiplayerClient)
                    NPC.SpawnOnPlayer(self.whoAmI, NPCID.MoonLordCore);
                else
                    NetMessage.SendData(MessageID.SpawnBossUseLicenseStartEvent, -1, -1, null, self.whoAmI, NPCID.MoonLordCore);
            }
            else
                orig(self, sItem);
        }
        #endregion

        #region Remove NPC.damage Condition from Radar
        private static void RemoveDamageConditionFromRadar(ILContext il)
        {
            var cursor = new ILCursor(il);

            Func<Instruction, bool>[] searchFor =
            [
                (x => x.MatchLdfld<NPC>(nameof(NPC.damage))),
                (x => x.MatchLdcI4(out var comp) && comp == 0),
                (x => x.MatchBle(out _)) //ble.s
            ];

            if (!cursor.TryGotoNext(MoveType.After, searchFor))
            {
                LogFailure("Radar Condition", "Unable to locate condition for NPC.damage > 0");
                return;
            }

            // Branch is used for exit condition. So setting ble.s opcode to nop will remove the condition
            cursor.Prev.OpCode = OpCodes.Nop;

            // After that we pop NPC.damage and 0 from stack
            cursor.EmitPop();
            cursor.EmitPop();
        }
        #endregion

        #region Multiple NPC Happiness support 
        // Currently unused as the one NPC who used it was removed. However it is very likely it'll be used again in the future, so this code is being kept.
        /*private static void AllowMultipleLikedNPCs(On_ShopHelper.orig_ApplyNpcRelationshipEffect orig, ShopHelper self, int npcType, AffectionLevel affectionLevel)
        {
            FieldInfo npcTalkField = typeof(ShopHelper).GetField("_currentNPCBeingTalkedTo", BindingFlags.Instance | BindingFlags.NonPublic);
            NPC talkedNPC = (NPC)npcTalkField.GetValue(self);

            int npcTypee = 0;

            // Allow the given NPC to have things to say about multiple NPCs with the same happiness level
            if (talkedNPC.type == npcTypee)
            {
                MethodInfo addReportField = typeof(ShopHelper).GetMethod("AddHappinessReportText", BindingFlags.Instance | BindingFlags.NonPublic);

                FieldInfo happinessField = typeof(ShopHelper).GetField("_currentPriceAdjustment", BindingFlags.Instance | BindingFlags.NonPublic);
                float currentPriceAdjustment = (float)happinessField.GetValue(self);

                if (affectionLevel != 0 && Enum.IsDefined(affectionLevel))
                {
                    // Add a suffix to the localization key which specifies the NPC's name
                    addReportField.Invoke(self, [ $"{affectionLevel}NPC_" + NPCID.Search.GetName(npcType),  new
                    {
                        NPCName = NPC.GetFullnameByID(npcType)
                    }, 0]);
                    currentPriceAdjustment *= NPCHappiness.AffectionLevelToPriceMultiplier[affectionLevel];
                    happinessField.SetValue(self, currentPriceAdjustment);
                }
            }
            else
            {
                orig(self, npcType, affectionLevel);
            }
        }*/
        #endregion

        #region Allow Disabling Gravity Swap Visual and Allow Gravity Keybind
        private static void DelayGravity(On_Player.orig_UpdateControlHolds orig, Player Player)
        {
            var cplay = Player.Calamity();
            if (CalamityKeybinds.SwitchGravityHotkey.GetAssignedKeys().Count != 0 && (Player.gravControl || Player.gravControl2) && !Player.mount.Active)
            {
                if (Player.controlUp && Player.releaseUp) {
                    Player.gravDir *= -1;
                }
                if (CalamityKeybinds.SwitchGravityHotkey.JustPressed) 
                {
                    Player.gravDir *= -1;
                    Player.fallStart = (int)(Player.position.Y / 16f);
                    Player.jump = 0;
                    SoundEngine.PlaySound(SoundID.Item8, Player.position);
                }

                if (Player.forcedGravity > 0) {
				    Player.gravDir = -1f;
			}   
            }
            
            if (cplay.justChangedGravity) {
                Player.gravDir = cplay.oldGravDir;
            }
            cplay.justChangedGravity = cplay.oldGravDir != Player.gravDir;
            
            cplay.oldGravDir = Player.gravDir;
            if (Main.netMode != NetmodeID.Server && !Main.gameMenu && CalamityClientConfig.Instance.DisableGravityScreenSwap)
            {
            if (Player.gravDir == -1) {
                if (!Filters.Scene["CalamityMod:FlipScreen"].IsActive()) {
                    Filters.Scene.Activate("CalamityMod:FlipScreen");
                    Filters.Scene["CalamityMod:FlipScreen"].Opacity = 1f;

                }
            } else {
                if (Filters.Scene["CalamityMod:FlipScreen"].IsActive()) {
                    Filters.Scene["CalamityMod:FlipScreen"].Opacity = 0f;
                    Filters.Scene.Deactivate("CalamityMod:FlipScreen");

                }
            }
            }
            if (cplay.justChangedGravity)
            {
                Player.gravDir *= -1;
            }
            orig(Player);
        }

        private static void GravityMouse(On_PlayerInput.orig_SetZoom_MouseInWorld orig) {
            orig();
            if (!Main.gameMenu && Filters.Scene["CalamityMod:FlipScreen"].IsActive())//((Main.LocalPlayer.gravDir == -1 && !Main.LocalPlayer.Calamity().justChangedGravity) || (Main.LocalPlayer.Calamity().oldGravDir == -1 && Main.LocalPlayer.Calamity().justChangedGravity))
            {
                var center = Main.screenHeight / 2;
                Main.mouseY = center - (Main.mouseY - center);
            };
        }
        private static void UI_Unflip_Start(On_Main.orig_DrawPlayerChatBubbles orig, Main self)
        {
            if (!Main.gameMenu && (Filters.Scene["CalamityMod:FlipScreen"].IsActive() || Main.LocalPlayer.Calamity().justChangedGravity))
            {
                Main.LocalPlayer.Calamity().tempGravDir = Main.LocalPlayer.gravDir;
                Main.LocalPlayer.gravDir = 1;
            }
            orig(self);
        }
        
        private static void UI_Unflip_End(On_Main.orig_DrawInterface orig, Main self, GameTime gameTime)
        {
            orig(self, gameTime);
            if (!Main.gameMenu && Filters.Scene["CalamityMod:FlipScreen"].IsActive())
            {
                Main.LocalPlayer.gravDir = Main.LocalPlayer.Calamity().tempGravDir;
            }
        }
        #endregion

        // 02JUN2024: Ozzatron: The below code is being kept in its initial state for historic value.
        #region Store The Stupid Fucking Private Wind Map In Public Property
        [/*TotallyNot*/Obsolete("This function serves no purpose and is included in the Calamity source code for historic value.", error: true)]
        private static void StoreWindGrid(On_TileDrawing.orig_Update orig, TileDrawing self)
        {
            orig(self);

            // FUCK YOU FUCK YOU FUCK YOU FUCK YOU FUCK YOU FUCK YOU FUCK YOU FUCK YOU FUCK YOU FUCK
            if (Windgrid is null)
                Windgrid = typeof(TileDrawing).GetField("_windGrid", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(self) as WindGrid;
        }
        #endregion Store The Stupid Fucking Private Wind Map In Public Property
    }
}
