using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CalamityMod.Balancing;
using CalamityMod.CustomRecipes;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.Demonshade;
using CalamityMod.Items.PermanentBoosters;
using CalamityMod.Items.Tools;
using CalamityMod.Items.VanillaArmorChanges;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.Prefixes;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace CalamityMod.Items
{
    public partial class CalamityGlobalItem : GlobalItem
    {
        #region Backup Tooltip Insertion Positions
        /// <summary>
        /// This array contains (almost) every single vanilla tooltip in reverse order starting at "Tooltip0".<br />
        /// Because "Tooltip0" is the first typical tooltip line, this is where Calamity tends to insert its tooltips.<br />
        /// When this line is not present, Calamity needs to insert tooltips in an <i>equivalent</i> position.<br />
        /// The best way to do this is to iterate backwards through all possible vanilla tooltip lines and pick the first one that is present.
        /// </summary>
        private static string[] MainTooltipBackupInsertionPositions =
        {
            "Material",
            "Consumable",
            "Ammo",
            "Placeable",
            "UseMana",
            "HealMana",
            "HealLife",
            "TileBoost",
            "HammerPower",
            "AxePower",
            "PickPower",
            "Defense",
            "Vanity",
            "Quest",
            "WandConsumes",
            "Equipable",
            "BaitPower",
            "NeedsBait",
            "FishingPower",
            "Knockback",
            "NoTransfer",
            "FavoriteDesc",
            "ItemName",
        };

        /// <summary>
        /// This array contains (almost) every single vanilla tooltip in reverse order starting at "Expert" and ending at "Tooltip0".<br />
        /// Because "Tooltip0" is the first typical tooltip line, this is the earliest conceivable place where a "Revengeance" marker can be inserted.<br />
        /// Since none of these tooltip lines are guaranteed to exist, Calamity needs to iterate through all of them to find a suitable insertion point.<br />
        /// The best way to do this is to iterate backwards through all possible vanilla tooltip lines and pick the first one that is present.
        /// </summary>
        private static string[] RevTooltipInsertionPositions =
        {
            "Expert",
            "SetBonus",
            RogueAccessoryPrefix.StealthTooltipID,
            "PrefixAccMeleeSpeed",
            "PrefixAccMoveSpeed",
            "PrefixAccDamage",
            "PrefixAccCritChance",
            "PrefixAccMaxMana",
            "PrefixAccDefense",
            RogueWeaponPrefix.StealthTooltipID,
            "PrefixKnockback",
            "PrefixShootSpeed",
            "PrefixSize",
            "PrefixUseMana",
            "PrefixCritChance",
            "PrefixSpeed",
            "PrefixDamage",
            "OneDropLogo",
            "BuffTime",
            "WellFedExpert",
            "EtherianManaWarning",
        };
        #endregion

        #region Main ModifyTooltips Function
        public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
        {
            // Get the first index, last index and total count of standard vanilla tooltip lines.
            // The first index and count are used to delete all vanilla tooltips when holding SHIFT, if requested.
            // The last index is used to insert various extra tooltip lines in the right position.
            //
            // This code used to be in the HoldShiftTooltip utility, but is needed to correctly place other tooltips.
            int firstTooltipIndex = -1;
            int lastTooltipIndex = -1;
            int standardTooltipCount = 0;
            for (int i = 0; i < tooltips.Count; i++)
            {
                if (tooltips[i].Name.StartsWith("Tooltip"))
                {
                    if (firstTooltipIndex == -1)
                        firstTooltipIndex = i;
                    lastTooltipIndex = i;
                    standardTooltipCount++;
                }
            }

            // If there are no standard vanilla tooltip lines (e.g. Flintlock Pistol, which has no tooltip)
            // then a different position needs to be selected for typical insertion.
            bool noStandardTooltips = false;
            if (firstTooltipIndex == -1)
            {
                noStandardTooltips = true;
                foreach (string lineName in MainTooltipBackupInsertionPositions)
                {
                    int idx = tooltips.FindIndex((line) => line.Name == lineName);
                    if (idx != -1)
                    {
                        firstTooltipIndex = lastTooltipIndex = idx;
                        break;
                    }
                }
            }

            // Apply custom rarity coloration to the item's name if applicable.
            TooltipLine nameLine = tooltips.FirstOrDefault(x => x.Name == "ItemName" && x.Mod == "Terraria");
            if (nameLine != null)
                ApplyRarityColor(item, nameLine);

            // Modify all vanilla tooltips before appending mod mechanics (if any).
            ModifyVanillaTooltips(item, tooltips);

            // Adds "Does extra damage to enemies shot at point-blank range" to weapons capable of it.
            if (canFirePointBlankShots)
            {
                LocalizedText lineText = CalamityUtils.GetText("Misc.PointBlank");
                TooltipLine line = new TooltipLine(Mod, "CalamityMod:PointBlankTooltip", lineText.Value);
                tooltips.Insert(++lastTooltipIndex, line);
            }

            // If an item has an enchantment, show its prefix in the first tooltip line and append its description to the tooltip list.
            EnchantmentTooltips(item, tooltips);

            // In GFB, replace all instances of "rogue" with "rouge".
            if (Main.zenithWorld)
            {
                tooltips.FindAndReplace("Rogue", "Rouge");
                tooltips.FindAndReplace("rogue", "rouge");
            }

            // Everything below this line can only apply to modded items. If the item is vanilla, stop here for efficiency.
            if (item.type < ItemID.Count)
                return;

            // Adds a Current Charge tooltip to all items which use charge.
            CalamityGlobalItem modItem = item.Calamity();
            if (modItem?.UsesCharge ?? false)
            {
                // Convert current charge ratio into a percentage.
                float displayedPercent = ChargeRatio * 100f;
                TooltipLine line = new TooltipLine(Mod, "CalamityCharge", $"Current Charge: {displayedPercent:N1}%");
                tooltips.Insert(++lastTooltipIndex, line);
            }

            // Generic mechanical implementation of any and all Hold SHIFT tooltips.
            // For more information, see IHoldShiftTooltipItem.
            //
            // Original code lifted from Iban's extended armor tooltips.
            if (item.ModItem is IHoldShiftTooltipItem holdShiftItem)
            {
                bool holdingShift = Main.keyState.IsKeyDown(Keys.LeftShift);

                // If holding SHIFT, actually display the extended tooltip.
                if (holdingShift && firstTooltipIndex != -1)
                {
                    string holdShiftText = item.ModItem.GetLocalizedValue(holdShiftItem.TooltipExtensionKey);
                    TooltipLine holdShiftLine = new TooltipLine(Mod, IHoldShiftTooltipItem.ExtensionTooltipID, holdShiftText);
                    if (holdShiftItem.TooltipExtensionColor is not null)
                        holdShiftLine.OverrideColor = holdShiftItem.TooltipExtensionColor;

                    // If asked to, remove all standard tooltip lines. This moves the last tooltip index.
                    // This only occurs if the standard tooltip lines are ACTUALLY standard tooltips. Otherwise, don't remove anything!
                    if (holdShiftItem.HidesNormalTooltip && !noStandardTooltips)
                    {
                        tooltips.RemoveRange(firstTooltipIndex, standardTooltipCount);
                        lastTooltipIndex -= standardTooltipCount;
                    }

                    // Append the "Hold SHIFT" tooltip at the end of standard tooltips.
                    tooltips.Insert(++lastTooltipIndex, holdShiftLine);
                }

                // If not holding SHIFT, display the extension indicator if appropriate.
                if (!holdingShift && holdShiftItem.ShowExtensionIndicator)
                {
                    LocalizedText indicatorText = CalamityUtils.GetText(holdShiftItem.ExtensionIndicatorKey);
                    TooltipLine indicator = new TooltipLine(Mod, IHoldShiftTooltipItem.ExtensionIndicatorTooltipID, indicatorText.Value);
                    if (holdShiftItem.ExtensionIndicatorColor is not null)
                        indicator.OverrideColor = holdShiftItem.ExtensionIndicatorColor;

                    // Append the extension indicator tooltip at the end of standard tooltips.
                    tooltips.Insert(++lastTooltipIndex, indicator);
                }

                // Generic support for flavor tooltips.
                // This is only necessary on items with Hold SHIFT tooltips.
                // The extended tooltip and tooltip extension indicator are placed above flavor tooltips for vanilla consistency.
                //
                // Flavor tooltips display unconditionally if defined. They are visible both when holding SHIFT and when not.
                if (holdShiftItem.HasFlavorTooltip && holdShiftItem.FlavorTooltipKey is not null)
                {
                    string flavorText = item.ModItem.GetLocalizedValue(holdShiftItem.FlavorTooltipKey);
                    TooltipLine flavorLine = new TooltipLine(Mod, IHoldShiftTooltipItem.FlavorTooltipID, flavorText);
                    if (holdShiftItem.FlavorTooltipColor is not null)
                        flavorLine.OverrideColor = holdShiftItem.FlavorTooltipColor;

                    // Append the flavor tooltip at the end of standard tooltips, after all Hold SHIFT tooltips and reminders.
                    tooltips.Insert(++lastTooltipIndex, flavorLine);
                }
            }

            //
            // "Late" tooltips are all inserted after vanilla's "Expert" and "Master" markers.
            //

            // The best possible position is identified using a separate backwards search.
            int difficultyTooltipIndex = -1;
            foreach (string lineName in RevTooltipInsertionPositions)
            {
                int idx = tooltips.FindIndex((line) => line.Name == lineName);
                if (idx != -1)
                {
                    difficultyTooltipIndex = idx;
                    break;
                }
            }

            // If the backwards search fails, it defaults to the last known tooltip index from the previous search.
            if (difficultyTooltipIndex == -1)
                difficultyTooltipIndex = lastTooltipIndex;

            // Adds "Revengeance" to all items which are Revengeance exclusive, like how vanilla does it for Expert and Master items.
            if (revengeanceItem)
            {
                LocalizedText revText = CalamityUtils.GetText("UI.Revengeance");
                TooltipLine revLine = new TooltipLine(Mod, "CalamityMod:RevengeanceItem", revText.Value);
                tooltips.Insert(++difficultyTooltipIndex, revLine);
            }

            // Adds "Donor Item" and "Developer Item" to donor items and developer items respectively.
            // This is intentionally at the bottom, below everything else.
            if (devItem)
            {
                LocalizedText devText = CalamityUtils.GetText("UI.DevItemTooltip");
                string coloredText = CalamityUtils.ColorMessage(devText.Value, CalamityUtils.DevItemColor);
                TooltipLine devLine = new TooltipLine(Mod, "CalamityMod:DevItem", coloredText);
                tooltips.Insert(++difficultyTooltipIndex, devLine);
            }
            else if (donorItem)
            {
                LocalizedText donorText = CalamityUtils.GetText("UI.DonorItemTooltip");
                string coloredText = CalamityUtils.ColorMessage(donorText.Value, CalamityUtils.DonatorItemColor);
                TooltipLine donorLine = new TooltipLine(Mod, "CalamityMod:DonorItem", coloredText);
                tooltips.Insert(++difficultyTooltipIndex, donorLine);
            }
        }
        #endregion

        #region Rarity Coloration
        private static void ApplyRarityColor(Item item, TooltipLine nameLine)
        {
            if (item.type == ModContent.ItemType<LiliesOfFinality>())
                nameLine.OverrideColor = Color.Lerp(Color.Red, Color.White, (float)Math.Sin(Main.GlobalTimeWrappedHourly) / 2f + 0.5f);
            if (item.type == ModContent.ItemType<HeartoftheElements>() || item.type == ModContent.ItemType<TheCommunity>() || item.type == ModContent.ItemType<IridescentExcalibur>())
                nameLine.OverrideColor = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB);

            // Developer items
            if (item.type == ModContent.ItemType<Fabstaff>())
                nameLine.OverrideColor = new Color(Main.DiscoR, 100, 255);
            if (item.type == ModContent.ItemType<StaffofBlushie>())
                nameLine.OverrideColor = new Color(0, 0, 255);
            if (item.type == ModContent.ItemType<TheDanceofLight>())
                nameLine.OverrideColor = TheDanceofLight.GetSyncedLightColor();
            if (item.type == ModContent.ItemType<NanoblackReaper>())
                nameLine.OverrideColor = new Color(0.34f, 0.34f + 0.66f * Main.DiscoG / 255f, 0.34f + 0.5f * Main.DiscoG / 255f);
            if (item.type == ModContent.ItemType<ShatteredCommunity>())
                nameLine.OverrideColor = ShatteredCommunity.GetRarityColor();
            if (item.type == ModContent.ItemType<NimbleBounder>())
                nameLine.OverrideColor = CalamityUtils.ColorSwap(new Color(132, 37, 147), new Color(0, 255, 0), 5f); //alternates purple and neon green
            if (item.type == ModContent.ItemType<ProfanedSoulCrystal>())
                nameLine.OverrideColor = CalamityUtils.ColorSwap(new Color(255, 166, 0), new Color(25, 250, 25), 6f); //alternates between emerald green and amber (BanditHueh)
            if (item.type == ModContent.ItemType<TemporalUmbrella>())
                nameLine.OverrideColor = CalamityUtils.ColorSwap(new Color(210, 0, 255), new Color(255, 248, 24), 4f);
            if (item.type == ModContent.ItemType<Endogenesis>())
                nameLine.OverrideColor = CalamityUtils.ColorSwap(new Color(131, 239, 255), new Color(36, 55, 230), 4f);
            if (item.type == ModContent.ItemType<DraconicDestruction>())
                nameLine.OverrideColor = CalamityUtils.ColorSwap(new Color(255, 69, 0), new Color(139, 0, 0), 4f);
            if (item.type == ModContent.ItemType<ScarletDevil>())
                nameLine.OverrideColor = CalamityUtils.ColorSwap(new Color(191, 45, 71), new Color(185, 187, 253), 4f);
            if (item.type == ModContent.ItemType<RedSun>())
                nameLine.OverrideColor = CalamityUtils.ColorSwap(new Color(204, 86, 80), new Color(237, 69, 141), 4f);
            if (item.type == ModContent.ItemType<CrystylCrusher>())
                nameLine.OverrideColor = new Color(129, 29, 149);
            if (item.type == ModContent.ItemType<SomaPrime>())
                nameLine.OverrideColor = CalamityUtils.ColorSwap(new Color(255, 255, 255), new Color(0xD1, 0xCC, 0x6F), 4f);
            if (item.type == ModContent.ItemType<Svantechnical>())
                nameLine.OverrideColor = new Color(220, 20, 60);
            if (item.type == ModContent.ItemType<Contagion>())
                nameLine.OverrideColor = new Color(207, 17, 117);
            if (item.type == ModContent.ItemType<TriactisTruePaladinianMageHammerofMight>())
                nameLine.OverrideColor = new Color(227, 226, 180);
            if (item.type == ModContent.ItemType<IllustriousKnives>())
                nameLine.OverrideColor = CalamityUtils.ColorSwap(new Color(154, 255, 151), new Color(228, 151, 255), 4f);
            if (item.type == ModContent.ItemType<DemonshadeHelm>() || item.type == ModContent.ItemType<DemonshadeBreastplate>() || item.type == ModContent.ItemType<DemonshadeGreaves>())
                nameLine.OverrideColor = CalamityUtils.ColorSwap(new Color(255, 132, 22), new Color(221, 85, 7), 4f);
            if (item.type == ModContent.ItemType<AngelicAlliance>())
            {
                nameLine.OverrideColor = CalamityUtils.MulticolorLerp(Main.GlobalTimeWrappedHourly / 2f % 1f, new Color[]
                {
                    new Color(255, 196, 55),
                    new Color(255, 231, 107),
                    new Color(255, 254, 243)
                });
            }

            // TODO -- for cleanliness, ALL color math should either be a one-line color swap or inside the item's own file
            // The items that currently violate this are all below:
            // Eternity, Flamsteed Ring, Earth
            if (item.type == ModContent.ItemType<Eternity>())
            {
                List<Color> colorSet = new List<Color>()
                    {
                        new Color(188, 192, 193), // white
                        new Color(157, 100, 183), // purple
                        new Color(249, 166, 77), // honey-ish orange
                        new Color(255, 105, 234), // pink
                        new Color(67, 204, 219), // sky blue
                        new Color(249, 245, 99), // bright yellow
                        new Color(236, 168, 247), // purplish pink
                    };
                if (nameLine != null)
                {
                    int colorIndex = (int)(Main.GlobalTimeWrappedHourly / 2 % colorSet.Count);
                    Color currentColor = colorSet[colorIndex];
                    Color nextColor = colorSet[(colorIndex + 1) % colorSet.Count];
                    nameLine.OverrideColor = Color.Lerp(currentColor, nextColor, Main.GlobalTimeWrappedHourly % 2f > 1f ? 1f : Main.GlobalTimeWrappedHourly % 1f);
                }
            }
            if (item.type == ModContent.ItemType<FlamsteedRing>())
            {
                if (Main.GlobalTimeWrappedHourly % 1f < 0.6f)
                {
                    nameLine.OverrideColor = new Color(89, 229, 255);
                }
                else if (Main.GlobalTimeWrappedHourly % 1f < 0.8f)
                {
                    nameLine.OverrideColor = Color.Lerp(new Color(89, 229, 255), Color.White, (Main.GlobalTimeWrappedHourly % 1f - 0.6f) / 0.2f);
                }
                else
                {
                    nameLine.OverrideColor = Color.Lerp(Color.White, new Color(89, 229, 255), (Main.GlobalTimeWrappedHourly % 1f - 0.8f) / 0.2f);
                }
            }
            if (item.type == ModContent.ItemType<Earth>())
            {
                List<Color> earthColors = new List<Color>()
                {
                    Color.OrangeRed,
                    Color.MediumTurquoise,
                    Color.LimeGreen
                };
                if (nameLine != null)
                {
                    int colorIndex = (int)(Main.GlobalTimeWrappedHourly / 2 % earthColors.Count);
                    Color currentColor = earthColors[colorIndex];
                    Color nextColor = earthColors[(colorIndex + 1) % earthColors.Count];
                    nameLine.OverrideColor = Color.Lerp(currentColor, nextColor, Main.GlobalTimeWrappedHourly % 2f > 1f ? 1f : Main.GlobalTimeWrappedHourly % 1f);
                }
            }
        }
        #endregion

        #region Enchantment Tooltips
        private void EnchantmentTooltips(Item item, IList<TooltipLine> tooltips)
        {
            if (!item.IsAir && AppliedEnchantment.HasValue)
            {
                foreach (string line in AppliedEnchantment.Value.Description.ToString().Split('\n'))
                {
                    TooltipLine descriptionLine = new TooltipLine(Mod, "Enchantment", CalamityUtils.ColorMessage(line, CalamityUtils.DonatorItemColor));
                    tooltips.Add(descriptionLine);
                }
            }
        }
        #endregion

        #region Vanilla Item Tooltip Modification

        // Turns a number into a string of increased mining speed.
        public static string MiningSpeedString(int percent) => $"\n{percent}% increased mining speed";

        private static void ModifyVanillaTooltips(Item item, IList<TooltipLine> tooltips)
        {
            #region Modular Tooltip Editing Code
            // This is a modular tooltip editor which loops over all tooltip lines of an item,
            // selects all those which match an arbitrary function you provide,
            // then edits them using another arbitrary function you provide.
            void ApplyTooltipEdits(IList<TooltipLine> lines, Func<Item, TooltipLine, bool> predicate, Action<TooltipLine> action)
            {
                foreach (TooltipLine line in lines)
                    if (predicate.Invoke(item, line))
                        action.Invoke(line);
            }

            // This function produces simple predicates to match a specific line of a tooltip, by number/index.
            Func<Item, TooltipLine, bool> LineNum(int n) => (Item i, TooltipLine l) => l.Mod == "Terraria" && l.Name == $"Tooltip{n}";
            // This function produces simple predicates to match a specific line of a tooltip, by name.
            Func<Item, TooltipLine, bool> LineName(string s) => (Item i, TooltipLine l) => l.Mod == "Terraria" && l.Name == s;

            // These functions are shorthand to invoke ApplyTooltipEdits using the above predicates.
            void EditTooltipByNum(int lineNum, Action<TooltipLine> action) => ApplyTooltipEdits(tooltips, LineNum(lineNum), action);
            void EditTooltipByName(string lineName, Action<TooltipLine> action) => ApplyTooltipEdits(tooltips, LineName(lineName), action);
            string EditedTooltip(string key) => CalamityUtils.GetTextValue($"Vanilla.EditedTooltip.{key}");
            LocalizedText GetEditedTooltip(string key) => CalamityUtils.GetText($"Vanilla.EditedTooltip.{key}");

            // For items such as a Copper Helmet which literally have no tooltips at all, add a custom "Tooltip0" which mimics the vanilla Tooltip0.
            void AddTooltip(string key)
            {
                // Don't add the tooltip if the item is in a social slot
                if (item.social)
                    return;

                int defenseIndex = -1;
                for (int i = 0; i < tooltips.Count; ++i)
                    if (tooltips[i].Name == "Defense")
                    {
                        defenseIndex = i;
                        break;
                    }
                tooltips.Insert(defenseIndex + 1, new TooltipLine(CalamityMod.Instance, "Tooltip0", CalamityUtils.GetTextValue($"Vanilla.AddedTooltip.{key}")));
            }
            string AddedTooltip(string key) => "\n" + CalamityUtils.GetTextValue($"Vanilla.AddedTooltip.{key}");
            LocalizedText GetAddedTooltip(string key) => CalamityUtils.GetText($"Vanilla.AddedTooltip.{key}");
            #endregion

            // Exact life regen descriptions
            #region Life Regen Clarity Tooltips
            bool isCampfire = item.type == ItemID.Campfire || item.type == ItemID.CursedCampfire || item.type == ItemID.DemonCampfire || item.type == ItemID.FrozenCampfire || item.type == ItemID.IchorCampfire || item.type == ItemID.RainbowCampfire || item.type == ItemID.UltraBrightCampfire || item.type == ItemID.BoneCampfire || item.type == ItemID.DesertCampfire || item.type == ItemID.CoralCampfire || item.type == ItemID.CorruptCampfire || item.type == ItemID.CrimsonCampfire || item.type == ItemID.HallowedCampfire || item.type == ItemID.JungleCampfire || item.type == ItemID.MushroomCampfire || item.type == ItemID.ShimmerCampfire;
            if (isCampfire)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("Campfires"));

            if (item.type == ItemID.HeartLantern)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("HeartLantern"));

            if (item.type == ItemID.BottledHoney)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("BottledHoney"));

            if (item.type == ItemID.ShinyStone)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("ShinyStone"));

            if (item.type == ItemID.BandofRegeneration)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("BandofRegeneration"));

            if (item.type == ItemID.CharmofMyths)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("CharmofMyths"));

            if (item.type == ItemID.RegenerationPotion)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("RegenerationPotion"));

            if (item.type == ItemID.SoulDrain)
                EditTooltipByNum(0, (line) => line.Text += AddedTooltip("SoulDrain"));

            if (item.type == ItemID.HamBat)
                EditTooltipByNum(1, (line) => line.Text = EditedTooltip("HamBat"));

            if (item.type == ItemID.AegisCrystal)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("AegisCrystal"));
            #endregion

            // Numerous random tooltip edits which don't fit into another category
            #region Various Tooltip Edits

            // Lilies of Finality 512 edit
            if (item.type == ModContent.ItemType<LiliesOfFinality>())
                EditTooltipByName("Damage", (line) => line.Text = LiliesOfFinality.TheNumber + Language.GetTextValue("LegacyTooltip.53"));

            // Apparently 612 is a homestuck reference
            if (item.type == ModContent.ItemType<Respiteblock>())
                EditTooltipByName("AxePower", (line) => line.Text = line.Text.Replace("610%", "612%"));

            // Master Mode items also drop in Revengeance
            // Only affects vanilla and Calamity items
            if (item.master && (item.type < ItemID.Count || item.ModItem?.Mod is CalamityMod))
                EditTooltipByName("Master", (line) => line.Text = EditedTooltip("MasterExclusive"));

            // Add a tooltip about Slimed's effects
            if (item.type == ItemID.SlimeGun)
                EditTooltipByNum(0, (line) => line.Text += AddedTooltip("SlimeGun"));
            // Replace the meme tooltip with a useful one.
            if (item.type == ItemID.GelBalloon)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("GelBalloon"));

            // Flesh Knuckles giving extra max life.
            if (item.type == ItemID.FleshKnuckles || item.type == ItemID.HeroShield || item.type == ItemID.BerserkerGlove)
                EditTooltipByNum(0, (line) => line.Text += AddedTooltip("FleshKnucklesLine"));

            // Rod of Discord cannot be used multiple times to hurt yourself
            if (item.type == ItemID.RodofDiscord)
                EditTooltipByNum(1, (line) => line.Text += AddedTooltip("RodofDiscord"));

            // Indicate that the Ankh Shield provides sandstorm wind push immunity
            if (item.type == ItemID.AnkhShield)
            {
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("AnkhShield1"));
                EditTooltipByNum(1, (line) => line.Text = EditedTooltip("AnkhShield2"));
            }

            // If Early Hardmode Rework is enabled: Remind users that ores will NOT spawn when an altar is smashed.
            if (CalamityServerConfig.Instance.EarlyHardmodeProgressionRework && (item.type == ItemID.Pwnhammer || item.type == ItemID.Hammush))
                EditTooltipByNum(0, (line) => line.Text += AddedTooltip("Pwnhammer"));

            // Warmth Potion reduces debuff durations
            if (item.type == ItemID.WarmthPotion)
                EditTooltipByNum(0, (line) => line.Text += AddedTooltip("WarmthPotion"));

            // Nerfed Archery Potion tooltip
            if (item.type == ItemID.ArcheryPotion)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("ArcheryPotion"));

            // Buffed Ironskin Potion tooltip
            if (item.type == ItemID.IronskinPotion)
                EditTooltipByNum(0, (line) => line.Text = GetEditedTooltip("IronskinPotion").Format(CalamityUtils.GetScalingDefense(-1)));

            // Nerfed Swiftness Potion tooltip
            if (item.type == ItemID.SwiftnessPotion)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace("25%", "15%"));

            // Hand Warmer has a side bonus with Snow armor
            if (item.type == ItemID.HandWarmer)
                EditTooltipByNum(0, (line) => line.Text += AddedTooltip("HandWarmer"));

            // Golden Fishing Rod inherently contains High Test Fishing Line
            if (item.type == ItemID.GoldenFishingRod)
                EditTooltipByName("NeedsBait", (line) => line.Text += AddedTooltip("GoldenFishingRod"));

            // Information about graveyards
            // There are no item sets for tombstones wtf
            if (item.type == ItemID.Tombstone || item.type == ItemID.GraveMarker || item.type == ItemID.CrossGraveMarker || item.type == ItemID.Headstone || item.type == ItemID.Gravestone || item.type == ItemID.Obelisk
                || item.type == ItemID.RichGravestone1 || item.type == ItemID.RichGravestone2 || item.type == ItemID.RichGravestone3 || item.type == ItemID.RichGravestone4 || item.type == ItemID.RichGravestone5)
                EditTooltipByName("Material", (line) => line.Text += AddedTooltip("Tombstones"));

            // Eternity Crystal notifies the player that they can accelerate the invasion
            if (item.type == ItemID.DD2ElderCrystal)
                EditTooltipByNum(0, (line) => line.Text += Lang.SupportGlyphs(AddedTooltip("DD2ElderCrystal")));

            // Modify item speed tooltips to use a new scale designed to more accurately reflect practical distributions of item speeds.
            // Due to the higher complexity of the action, the actual logic is delegated to its own method.
            // I think this fits the miscellaneous category? Not seeing anything like this elsewhere. - Tomat
            EditTooltipByName("Speed", (line) => RedistributeSpeedTooltips(item, line));

            if (item.healLife > 0 && Main.LocalPlayer.Calamity().healingPotionMultiplier != 1f)
                EditTooltipByName("HealLife", (line) => line.Text = Language.GetOrRegister("CommonItemTooltip.RestoresLife").Format((int)(item.healLife * Main.LocalPlayer.Calamity().healingPotionMultiplier)));
            #endregion

            // For boss summon item clarity
            #region Boss Summon Clarity Tooltips
            if (item.type == ItemID.Abeemination)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("Abeemination"));

            if (item.type == ItemID.BloodySpine)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("BloodySpine"));

            if (item.type == ItemID.ClothierVoodooDoll)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("ClothierVoodooDoll"));

            if (item.type == ItemID.DeerThing)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("DeerThing"));

            if (item.type == ItemID.GuideVoodooDoll)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("GuideVoodooDoll"));

            if (item.type == ItemID.LihzahrdPowerCell)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("LihzahrdPowerCell"));

            if (item.type == ItemID.MechanicalEye)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("MechanicalEye"));

            if (item.type == ItemID.MechanicalSkull)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("MechanicalSkull"));

            if (item.type == ItemID.MechanicalWorm)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("MechanicalWorm"));

            if (item.type == ItemID.QueenSlimeCrystal)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("QueenSlimeCrystal"));

            if (item.type == ItemID.SuspiciousLookingEye)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("SuspiciousLookingEye"));

            if (item.type == ItemID.TruffleWorm)
                EditTooltipByName("Consumable", (line) => line.Text += AddedTooltip("TruffleWorm"));

            if (item.type == ItemID.WormFood)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("WormFood"));
            #endregion

            // Brain of Confusion, Black Belt and Master Ninja Gear have guaranteed dodges with a fixed cooldown.
            #region Guaranteed Dodge Tooltips
            if (item.type == ItemID.BlackBelt)
                EditTooltipByNum(0, (line) => line.Text = CalamityUtils.GetText("Vanilla.DodgeInfo").Format(BalancingConstants.BeltDodgeCooldownMin / 60, BalancingConstants.BeltDodgeCooldownMax / 60));
            if (item.type == ItemID.MasterNinjaGear)
                EditTooltipByNum(1, (line) => line.Text = CalamityUtils.GetText("Vanilla.DodgeInfo").Format(BalancingConstants.BeltDodgeCooldownMin / 60, BalancingConstants.BeltDodgeCooldownMax / 60));

            if (item.type == ItemID.BrainOfConfusion)
                EditTooltipByNum(0, (line) => line.Text = CalamityUtils.GetText("Vanilla.DodgeInfo").Format(BalancingConstants.BrainDodgeCooldownMin / 60, BalancingConstants.BrainDodgeCooldownMax / 60));
            #endregion

            // Weapon changes
            #region Weapon changes
            // Aerial Bane is no longer the real bane of aerial enemies (50% dmg bonus removed)
            if (item.type == ItemID.DD2BetsyBow)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("DD2BetsyBow"));

            // Death Sickle inflict Whispering Death
            if (item.type == ItemID.DeathSickle)
                EditTooltipByNum(0, (line) => line.Text += AddedTooltip("DeathSickle"));
            #endregion

            // Light pets, accessories, and other items which boost the player's Abyss light stat
            #region Abyss Light Tooltips
            // +1 to Abyss light level
            if (item.type == ItemID.CrimsonHeart || item.type == ItemID.ShadowOrb || item.type == ItemID.MagicLantern || item.type == ItemID.JellyfishNecklace ||
                item.type == ItemID.MiningHelmet || item.type == ItemID.UltrabrightHelmet)
                EditTooltipByNum(0, (line) => line.Text += "\n" + CalamityUtils.GetTextValue("Common.AbyssLightLevel1"));
            if (item.type == ItemID.JellyfishDivingGear || item.type == ItemID.Magiluminescence)
                EditTooltipByNum(1, (line) => line.Text += "\n" + CalamityUtils.GetTextValue("Common.AbyssLightLevel1"));

            // +2 to Abyss light level
            if (item.type == ItemID.ShinePotion)
                EditTooltipByName("BuffTime", (line) => line.Text += "\n" + CalamityUtils.GetTextValue("Common.AbyssLightLevel2"));
            if (item.type == ItemID.FairyBell || item.type == ItemID.DD2PetGhost)
                EditTooltipByNum(0, (line) => line.Text += "\n" + CalamityUtils.GetTextValue("Common.AbyssLightLevel2"));

            // +3 to Abyss light level
            if (item.type == ItemID.WispinaBottle || item.type == ItemID.PumpkingPetItem || item.type == ItemID.GolemPetItem || item.type == ItemID.FairyQueenPetItem)
                EditTooltipByNum(0, (line) => line.Text += "\n" + CalamityUtils.GetTextValue("Common.AbyssLightLevel3"));
            if (item.type == ItemID.SuspiciousLookingTentacle)
                EditTooltipByNum(1, (line) => line.Text += "\n" + CalamityUtils.GetTextValue("Common.AbyssLightLevel3"));
            #endregion

            // Accessories and other items which boost the player's ability to breathe in the Abyss
            #region Abyss Breath Tooltips

            // Moderate breath boost
            if (item.type == ItemID.DivingHelmet)
                EditTooltipByNum(0, (line) => line.Text += "\n" + CalamityUtils.GetTextValue("Common.AbyssBreathLevel2"));
            if (item.type == ItemID.ArcticDivingGear)
                EditTooltipByNum(1, (line) => line.Text += "\n" + CalamityUtils.GetTextValue("Common.AbyssLightLevel1") + "\n" + CalamityUtils.GetTextValue("Common.AbyssBreathLevel2"));

            // Great breath boost
            if (item.type == ItemID.GillsPotion)
                EditTooltipByName("BuffTime", (line) => line.Text += "\n" + CalamityUtils.GetTextValue("Common.AbyssBreathLevel3"));

            if (item.type == ItemID.NeptunesShell || item.type == ItemID.MoonShell)
                EditTooltipByNum(1, (line) => line.Text += "\n" + CalamityUtils.GetTextValue("Common.AbyssBreathLevel3"));
            if (item.type == ItemID.CelestialShell)
                EditTooltipByNum(4, (line) => line.Text += "\n" + CalamityUtils.GetTextValue("Common.AbyssBreathLevel3"));
            #endregion

            // Flasks apply to Rogue weapons
            #region Rogue Flask Tooltips
            if (item.type == ItemID.FlaskofCursedFlames)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace(" and Whip", ", Whip, and Rogue"));
            if (item.type == ItemID.FlaskofFire)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace(" and Whip", ", Whip, and Rogue"));
            if (item.type == ItemID.FlaskofGold)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace(" and Whip", ", Whip, and Rogue"));
            if (item.type == ItemID.FlaskofIchor)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace(" and Whip", ", Whip, and Rogue"));
            if (item.type == ItemID.FlaskofNanites)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace(" and Whip", ", Whip, and Rogue"));
            // party flask is unique because it affects ALL projectiles in Calamity, not just "also rogue ones"
            if (item.type == ItemID.FlaskofParty)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace("Melee and Whip", "All"));
            if (item.type == ItemID.FlaskofPoison)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace(" and Whip", ", Whip, and Rogue"));
            if (item.type == ItemID.FlaskofVenom)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace(" and Whip", ", Whip, and Rogue"));
            #endregion

            // Rebalances to vanilla item stats
            #region Vanilla Item Rebalance Tooltips

            // Various mining speed nerfs
            if (item.type == ItemID.MiningPotion)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace("25%", "15%"));

            if (item.type == ItemID.AncientChisel)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace("25%", "15%"));

            if (item.type == ItemID.HandOfCreation)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace("25%", "15%"));

            // Frozen Turtle Shell rebalance.
            if (item.type == ItemID.FrozenTurtleShell)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace("25%", "15%"));

            if (item.type == ItemID.FrozenShield)
                EditTooltipByNum(1, (line) => line.Text = line.Text.Replace("25%", "15%"));

            // Ale and Sake rebalance and Alcohol Poisoning.
            if (item.type == ItemID.Ale || item.type == ItemID.Sake)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("AleSake"));

            //Flame Waker Boots buff.
            if (item.type == ItemID.FlameWakerBoots)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("FlameWakerBoots"));

            // Hellfire Treads buff.
            if (item.type == ItemID.HellfireTreads)
                EditTooltipByNum(2, (line) => line.Text += AddedTooltip("HellfireTreads"));

            // Fairy Boots buff.
            if (item.type == ItemID.FairyBoots)
                EditTooltipByNum(2, (line) => line.Text += AddedTooltip("FairyBoots"));

            // Reduced Nightwither and Daybroken damage, and melee speed removal.
            if (item.type == ItemID.MoonStone)
            {
                EditTooltipByNum(2, (line) => line.Text += AddedTooltip("MoonStone"));
                EditTooltipByNum(1, (line) => line.Text = EditedTooltip("SunMoonStones"));
            }
            if (item.type == ItemID.SunStone)
            {
                EditTooltipByNum(2, (line) => line.Text += AddedTooltip("SunStone"));
                EditTooltipByNum(1, (line) => line.Text = EditedTooltip("SunMoonStones"));
            }
            if (item.type == ItemID.CelestialStone)
            {
                EditTooltipByNum(2, (line) => line.Text += AddedTooltip("CelestialStone"));
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("SunMoonStones"));
            }
            if (item.type == ItemID.CelestialShell)
            {
                EditTooltipByNum(4, (line) => line.Text += AddedTooltip("CelestialStone"));
                EditTooltipByNum(2, (line) => line.Text = EditedTooltip("SunMoonStones"));
            }

            // Mana Flower tinker buffs.
            if (item.type == ItemID.MagnetFlower)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("MagnetFlower"));
            if (item.type == ItemID.ArcaneFlower || item.type == ItemID.ManaCloak)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("ArcaneFlowerManaCloak"));
            if (item.type == ItemID.ArcaneFlower)
                EditTooltipByNum(2, (line) => line.Text += AddedTooltip("ArcaneFlower"));

            // Magiluminescence nerf and clear explanation of what it actually does.
            if (item.type == ItemID.Magiluminescence)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("Magiluminescence"));

            // Frog Leg and all upgrades get clear explanations of what they actually do.
            if (item.type == ItemID.FrogLeg)
                EditTooltipByNum(0, (line) => line.Text = GetEditedTooltip("FrogLeg").Format((int)(BalancingConstants.VanillaFrogLegJumpSpeedBoost * 20f)));

            if (item.type == ItemID.FrogFlipper || item.type == ItemID.FrogWebbing)
                EditTooltipByNum(1, (line) => line.Text = GetEditedTooltip("FrogLeg").Format((int)(BalancingConstants.VanillaFrogLegJumpSpeedBoost * 20f)));

            if (item.type == ItemID.FrogGear)
                EditTooltipByNum(2, (line) => line.Text = GetEditedTooltip("FrogLeg").Format((int)(BalancingConstants.VanillaFrogLegJumpSpeedBoost * 20f)));

            if (item.type == ItemID.AmphibianBoots)
                EditTooltipByNum(1, (line) => line.Text = GetEditedTooltip("FrogLeg").Format((int)(BalancingConstants.AmphibianBootsJumpSpeedBoost * 20f)));

            // Soaring Insignia nerf and clear explanation of what it actually does.
            if (item.type == ItemID.EmpressFlightBooster)
            {
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("EmpressFlightBooster1"));
                EditTooltipByNum(1, (line) => line.Text = EditedTooltip("EmpressFlightBooster2"));
            }

            // Rifle Scope visibility change
            if (item.type == ItemID.RifleScope)
            {
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("RifleScope1"));
                EditTooltipByNum(1, (line) => line.Text = EditedTooltip("RifleScope2"));
            }

            // Sniper Scope rebalance and visibility change
            if (item.type == ItemID.SniperScope)
            {
                EditTooltipByNum(1, (line) => line.Text = EditedTooltip("SniperScope"));
                EditTooltipByNum(0, (line) => line.Text += AddedTooltip("RifleScope"));
            }

            // Recon Scope visibility change
            if (item.type == ItemID.ReconScope)
                EditTooltipByNum(0, (line) => line.Text += AddedTooltip("RifleScope"));


            // Magic Quiver
            if (item.type == ItemID.MagicQuiver)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("MagicQuiver"));

            // Molten Quiver
            if (item.type == ItemID.MoltenQuiver)
            {
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("MoltenQuiver1"));
                EditTooltipByNum(2, (line) => line.Text = EditedTooltip("MoltenQuiver2"));
            }

            // Magic Power Potion nerf
            if (item.type == ItemID.MagicPowerPotion)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("MagicPowerPotion"));

            // Featherfall Potion being stupid broken with Aero Stone
            if (item.type == ItemID.FeatherfallPotion)
                EditTooltipByNum(0, (line) => line.Text += AddedTooltip("FeatherfallPotion"));

            // Magic Hat nerf
            if (item.type == ItemID.MagicHat)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("MagicHat"));

            // Gem Robe nerfs
            if (item.type == ItemID.AmethystRobe)
                EditTooltipByNum(1, (line) => line.Text = line.Text.Replace("5%", "4%"));
            if (item.type == ItemID.TopazRobe)
            {
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace("40", "20"));
                EditTooltipByNum(1, (line) => line.Text = line.Text.Replace("7%", "5%"));
            }
            if (item.type == ItemID.SapphireRobe)
                EditTooltipByNum(1, (line) => line.Text = line.Text.Replace("9%", "6%"));
            if (item.type == ItemID.EmeraldRobe)
            {
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace("60", "40"));
                EditTooltipByNum(1, (line) => line.Text = line.Text.Replace("11%", "7%"));
            }
            if (item.type == ItemID.RubyRobe || item.type == ItemID.AmberRobe)
                EditTooltipByNum(1, (line) => line.Text = line.Text.Replace("13%", "8%"));
            if (item.type == ItemID.DiamondRobe)
            {
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace("80", "60"));
                EditTooltipByNum(1, (line) => line.Text = line.Text.Replace("15%", "9%"));
            }

            // Worm Scarf only gives 14% DR instead of 17%
            if (item.type == ItemID.WormScarf)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace("17%", "14%"));

            // Feral Claws line melee speed and true melee damage changes
            if (item.type == ItemID.FeralClaws)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace("12%", "10%"));

            if (item.type == ItemID.TitanGlove)
                EditTooltipByNum(0, (line) => line.Text += AddedTooltip("TitanGloveLine"));

            if (item.type == ItemID.PowerGlove)
            {
                EditTooltipByNum(1, (line) => line.Text = EditedTooltip("PowerGlove"));
                EditTooltipByNum(0, (line) => line.Text += AddedTooltip("TitanGloveLine"));
            }

            if (item.type == ItemID.BerserkerGlove)
                EditTooltipByNum(1, (line) => line.Text = EditedTooltip("BerserkerGlove"));

            if (item.type == ItemID.MechanicalGlove)
                EditTooltipByNum(1, (line) => line.Text = EditedTooltip("MechanicalGlove") + AddedTooltip("TitanGloveLine"));

            if (item.type == ItemID.FireGauntlet)
            {
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("FireGauntlet1"));
                EditTooltipByNum(1, (line) => line.Text = EditedTooltip("FireGauntlet2") + AddedTooltip("TitanGloveLine"));
            }

            // On Fire! debuff immunities
            if (item.type == ItemID.ObsidianSkull || item.type == ItemID.ObsidianSkullRose || item.type == ItemID.MoltenCharm)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("ObsidianSkullLine"));

            if (item.type == ItemID.ObsidianHorseshoe || item.type == ItemID.ObsidianShield || item.type == ItemID.ObsidianWaterWalkingBoots || item.type == ItemID.LavaSkull || item.type == ItemID.MoltenSkullRose)
                EditTooltipByNum(1, (line) => line.Text = EditedTooltip("ObsidianSkullLine"));

            if (item.type == ItemID.LavaWaders)
                EditTooltipByNum(1, (line) => line.Text = EditedTooltip("LavaWaders"));

            if (item.type == ItemID.TerrasparkBoots)
                EditTooltipByNum(3, (line) => line.Text = EditedTooltip("LavaWaders"));

            // Ozzatron 23NOV2023: Removed tooltip edits for Magma Skull and Molten Skull Rose, as they were invalid after vanilla tooltip changes.

            // Yoyo Glove/Bag apply a 0.5x damage multiplier on the second yoyo
            if (item.type == ItemID.YoyoBag || item.type == ItemID.YoYoGlove)
                EditTooltipByNum(0, (line) => line.Text += AddedTooltip("YoyoGlove"));

            //Gi 10% melee speed into 10% jump speed replacement
            if (item.type == ItemID.Gi)
                EditTooltipByNum(1, (line) => line.Text = EditedTooltip("Gi"));
            #endregion

            // Pre-Hardmode ore armor tooltip edits
            #region Pre-Hardmode Ore Armor
            // Copper
            if (item.type == ItemID.CopperHelmet)
                AddTooltip("CopperHelmet");
            if (item.type == ItemID.CopperChainmail)
                AddTooltip("CopperChainmail");
            if (item.type == ItemID.CopperGreaves)
                AddTooltip("CopperGreaves");

            // Tin
            if (item.type == ItemID.TinHelmet)
                AddTooltip("TinHelmet");
            if (item.type == ItemID.TinChainmail)
                AddTooltip("TinChainmail");
            if (item.type == ItemID.TinGreaves)
                AddTooltip("TinGreaves");

            // Iron
            if (item.type == ItemID.IronHelmet || item.type == ItemID.AncientIronHelmet || item.type == ItemID.IronChainmail || item.type == ItemID.IronGreaves)
                AddTooltip("IronPieces");

            // Lead
            if (item.type == ItemID.LeadHelmet || item.type == ItemID.LeadChainmail || item.type == ItemID.LeadGreaves)
                AddTooltip("LeadPieces");

            // Silver
            if (item.type == ItemID.SilverHelmet)
                AddTooltip("SilverHelmet");
            if (item.type == ItemID.SilverChainmail)
                AddTooltip("SilverChainmail");
            if (item.type == ItemID.SilverGreaves)
                AddTooltip("SilverGreaves");

            // Tungsten
            if (item.type == ItemID.TungstenHelmet)
                AddTooltip("TungstenHelmet");
            if (item.type == ItemID.TungstenChainmail)
                AddTooltip("TungstenChainmail");
            if (item.type == ItemID.TungstenGreaves)
                AddTooltip("TungstenGreaves");

            // Gold
            if (item.type == ItemID.GoldHelmet || item.type == ItemID.AncientGoldHelmet)
                AddTooltip("GoldHelmet");
            if (item.type == ItemID.GoldChainmail)
                AddTooltip("GoldChainmail");
            if (item.type == ItemID.GoldGreaves)
                AddTooltip("GoldGreaves");

            // Platinum
            if (item.type == ItemID.PlatinumHelmet)
                AddTooltip("PlatinumHelmet");
            if (item.type == ItemID.PlatinumChainmail)
                AddTooltip("PlatinumChainmail");
            if (item.type == ItemID.PlatinumGreaves)
                AddTooltip("PlatinumGreaves");

            // Jungle
            if (item.type == ItemID.JungleHat || item.type == ItemID.AncientCobaltHelmet)
            {
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace("40", "20"));
                EditTooltipByNum(1, (line) => line.Text = line.Text.Replace("6%", "3%"));
            }
            if (item.type == ItemID.JunglePants || item.type == ItemID.AncientCobaltLeggings)
                EditTooltipByNum(1, (line) => line.Text = line.Text.Replace("6%", "3%"));

            // Crimson
            if (item.type == ItemID.CrimsonHelmet || item.type == ItemID.CrimsonScalemail || item.type == ItemID.CrimsonGreaves)
            {
                EditTooltipByNum(0, (line) =>
                {
                    string newTooltip = line.Text.Replace("3%", "6%");
                    // Chest piece has 2 regen instead of 1
                    newTooltip += item.type == ItemID.CrimsonScalemail ? AddedTooltip("CrimsonBreastplate") : AddedTooltip("CrimsonOtherPieces");
                    line.Text = newTooltip;
                });
            }

            // Meteor
            if (item.type == ItemID.MeteorHelmet || item.type == ItemID.MeteorSuit || item.type == ItemID.MeteorLeggings)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace("9%", "8%"));
            #endregion

            // Hardmode armor tooltip edits
            #region Hardmode Ore Armor
            // Cobalt
            if (item.type == ItemID.CobaltHat)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace("40", $"{CobaltArmorSetChange.MaxManaBoost + 40}"));

            // Palladium
            if (item.type == ItemID.PalladiumBreastplate)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace("3%", $"{PalladiumArmorSetChange.ChestplateDamagePercentageBoost + 3}%"));
            if (item.type == ItemID.PalladiumLeggings)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace("2%", $"{PalladiumArmorSetChange.LeggingsDamagePercentageBoost + 2}%"));

            // Mythril
            if (item.type == ItemID.MythrilHood)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace("60", $"{MythrilArmorSetChange.MaxManaBoost + 60}"));

            // Orichalcum
            if (item.type == ItemID.OrichalcumBreastplate)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace("6%", $"{OrichalcumArmorSetChange.ChestplateCritChanceBoost + 6}%"));

            // Adamantite
            if (item.type == ItemID.AdamantiteHeadgear)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace("80", $"{AdamantiteArmorSetChange.MaxManaBoost + 80}"));

            // Titanium
            if (item.type == ItemID.TitaniumMask)
                EditTooltipByNum(1, (line) => line.Text = line.Text.Replace("9%", "14%"));

            // Solar Flare
            if (item.type == ItemID.SolarFlareHelmet)
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace("26%", "20%"));
            if (item.type == ItemID.SolarFlareHelmet || item.type == ItemID.SolarFlareBreastplate || item.type == ItemID.SolarFlareLeggings)
                EditTooltipByNum(1, (line) => line.Text = EditedTooltip("SolarFlarePieces"));

            // Vortex
            if (item.type == ItemID.VortexHelmet)
            {
                EditTooltipByNum(0, (line) => line.Text = line.Text.Replace("16%", "10%"));
                EditTooltipByNum(1, (line) => line.Text = line.Text.Replace("7%", "5%"));
            }
            #endregion

            // DD2 armor tooltip edits
            #region DD2 Armor
            // Reduce DD2 armor piece bonuses because they're overpowered, and clarify life regen boosts
            // Squire armor
            if (item.type == ItemID.SquireGreatHelm)
                EditTooltipByNum(0, (line) => line.Text = "Increases your max number of sentries by 1 and grants +2 HP/s life regen");
            if (item.type == ItemID.SquirePlating)
                EditTooltipByNum(0, (line) => line.Text = "10% increased minion damage and 15% increased melee damage");
            if (item.type == ItemID.SquireGreaves)
                EditTooltipByNum(0, (line) => line.Text = "5% increased minion damage and melee critical strike chance\n" +
                "15% increased movement speed");

            // Monk armor
            if (item.type == ItemID.MonkBrows)
                EditTooltipByNum(0, (line) => line.Text = "Increases your max number of sentries by 1 and increases melee attack speed by 10%");
            if (item.type == ItemID.MonkShirt)
                EditTooltipByNum(0, (line) => line.Text = "10% increased minion and melee damage");
            if (item.type == ItemID.MonkPants)
            {
                EditTooltipByNum(0, (line) => line.Text = "5% increased minion damage and melee critical strike chance");
                EditTooltipByNum(1, (line) => line.Text = "20% increased movement speed");
            }

            // Huntress armor
            if (item.type == ItemID.HuntressJerkin)
                EditTooltipByNum(0, (line) => line.Text = "10% increased minion and ranged damage\n" +
                "10% chance to not consume ammo");

            // Apprentice armor
            if (item.type == ItemID.ApprenticeTrousers)
                EditTooltipByNum(0, (line) => line.Text = "5% increased minion damage and magic critical strike chance\n" +
                "20% increased movement speed");

            // Valhalla Knight armor
            if (item.type == ItemID.SquireAltShirt)
                EditTooltipByNum(0, (line) => line.Text = "30% increased minion damage and grants +4 HP/s life regen");
            if (item.type == ItemID.SquireAltPants)
                EditTooltipByNum(0, (line) => line.Text = "10% increased minion damage and melee critical strike chance");

            // Shinobi Infiltrator armor
            if (item.type == ItemID.MonkAltHead)
                EditTooltipByNum(0, (line) => line.Text = "Increases your max number of sentries by 2\n" +
                "10% increased melee and minion damage");
            if (item.type == ItemID.MonkAltShirt)
                EditTooltipByNum(0, (line) => line.Text = "10% increased minion damage and melee speed");
            if (item.type == ItemID.MonkAltPants)
                EditTooltipByNum(0, (line) => line.Text = "10% increased minion damage and melee critical strike chance");

            // Red Riding armor
            if (item.type == ItemID.HuntressAltShirt)
                EditTooltipByNum(0, (line) => line.Text = "15% increased minion and ranged damage and 20% chance to not consume ammo");

            // Dark Artist armor
            if (item.type == ItemID.ApprenticeAltPants)
                EditTooltipByNum(0, (line) => line.Text = "10% increased minion damage and magic critical strike chance");
            #endregion

            // Non-consumable boss summon items
            #region Vanilla Boss Summon Non-consumable Tooltips
            if (item.type == ItemID.SlimeCrown || item.type == ItemID.SuspiciousLookingEye || item.type == ItemID.BloodMoonStarter || item.type == ItemID.GoblinBattleStandard ||
                item.type == ItemID.WormFood || item.type == ItemID.BloodySpine || item.type == ItemID.Abeemination || item.type == ItemID.DeerThing || item.type == ItemID.QueenSlimeCrystal ||
                item.type == ItemID.PirateMap || item.type == ItemID.SnowGlobe || item.type == ItemID.MechanicalEye || item.type == ItemID.MechanicalWorm || item.type == ItemID.MechanicalSkull ||
                item.type == ItemID.NaughtyPresent || item.type == ItemID.PumpkinMoonMedallion || item.type == ItemID.SolarTablet || item.type == ItemID.SolarTablet || item.type == ItemID.CelestialSigil)

                EditTooltipByNum(0, (line) => line.Text += "\n" + CalamityUtils.GetTextValue("Common.NotConsumable"));
            #endregion

            // Add mentions of what Calamity ores vanilla pickaxes can mine
            #region Pickaxe New Ore Tooltips
            if (item.type == ItemID.GoldPickaxe || item.type == ItemID.PlatinumPickaxe)
                EditTooltipByNum(0, (line) => line.Text = EditedTooltip("GoldPickaxe"));

            if (item.type == ItemID.Picksaw)
                EditTooltipByNum(0, (line) => line.Text += AddedTooltip("Picksaw"));

            if (item.type == ItemID.SolarFlarePickaxe || item.type == ItemID.VortexPickaxe || item.type == ItemID.NebulaPickaxe || item.type == ItemID.StardustPickaxe)
                EditTooltipByName("Material", (line) => line.Text += "\n" + CalamityUtils.GetTextValue("Common.CanMineUelibloom"));

            if (item.type == ItemID.SolarFlareDrill || item.type == ItemID.VortexDrill || item.type == ItemID.NebulaDrill || item.type == ItemID.StardustDrill)
                EditTooltipByName("TileBoost", (line) => line.Text += "\n" + CalamityUtils.GetTextValue("Common.CanMineUelibloom"));
            #endregion

            // Rebalances and information about vanilla set bonuses
            #region Vanilla Set Bonus Tooltips

            EditTooltipByName("SetBonus", (line) => VanillaArmorChangeManager.ApplySetBonusTooltipChanges(item, ref line.Text));

            // Gladiator
            if (item.type == ItemID.GladiatorHelmet)
                EditTooltipByName("Defense", (line) => line.Text += "\n" +CalamityUtils.GetText("Common.RogueDamage").Format(GladiatorArmorSetChange.HelmetRogueDamageBoostPercent));
            if (item.type == ItemID.GladiatorBreastplate)
                EditTooltipByName("Defense", (line) => line.Text += "\n" +CalamityUtils.GetText("Common.RogueCrit").Format(GladiatorArmorSetChange.ChestplateRogueCritBoostPercent));
            if (item.type == ItemID.GladiatorLeggings)
                EditTooltipByName("Defense", (line) => line.Text += "\n" +CalamityUtils.GetText("Common.RogueVelocity").Format(GladiatorArmorSetChange.LeggingRogueVelocityBoostPercent));

            // Forbidden (UNLESS you are wearing the Circlet, which is Summon/Rogue and does not get this line)
            if ((item.type == ItemID.AncientBattleArmorHat || item.type == ItemID.AncientBattleArmorShirt || item.type == ItemID.AncientBattleArmorPants)
                && !Main.LocalPlayer.Calamity().forbiddenCirclet)
                EditTooltipByName("SetBonus", (line) => line.Text = CalamityUtils.GetText($"Vanilla.Armor.SetBonus.Forbidden").Format(Language.GetTextValue(Main.ReversedUpDownArmorSetBonuses ? "Key.UP" : "Key.DOWN")));
            #endregion

            // Provide the full stats of every vanilla set of wings
            #region Wing Stat Tooltips

            // This function produces a "stat sheet" for a pair of wings from the raw stats.
            // For "vertical speed", 0 = Bad, 1 = Average, 2 = Good, 3 = Great, 4 = Excellent.
            string[] vertSpeedStrings = new string[] { "Bad vertical speed", "Average vertical speed", "Good vertical speed", "Great vertical speed", "Excellent vertical speed" };
            string WingStatsTooltip(float hSpeed, float accelMult, int vertSpeed, int flightTime, string extraTooltip = null)
            {
                StringBuilder sb = new StringBuilder(512);
                sb.Append('\n');
                sb.Append($"Horizontal speed: {hSpeed:N2}\n");
                sb.Append($"Acceleration multiplier: {accelMult:N1}\n");
                sb.Append(vertSpeedStrings[vertSpeed]);
                sb.Append('\n');
                sb.Append($"Flight time: {flightTime}");
                if (extraTooltip != null)
                {
                    sb.Append('\n');
                    sb.Append(extraTooltip);
                }
                return sb.ToString();
            }

            // This function is shorthand for appending a stat sheet to a pair of wings.
            void AddWingStats(float h, float a, int v, int f, string s = null) => EditTooltipByNum(0, (line) => line.Text += WingStatsTooltip(h, a, v, f, s));
            void AddWingStats2(float h, float a, int v, int f, string s = null, string lineName = null) => EditTooltipByName(lineName, (line) => line.Text += WingStatsTooltip(h, a, v, f, s));

            if (item.type == ItemID.CreativeWings)
                AddWingStats(3f, 1f, 0, 25);

            if (item.type == ItemID.AngelWings)
                AddWingStats(6.25f, 1f, 1, 100);

            if (item.type == ItemID.DemonWings)
                AddWingStats(6.25f, 1f, 1, 100);

            if (item.type == ItemID.Jetpack)
                AddWingStats(6.5f, 1f, 1, 150);

            if (item.type == ItemID.ButterflyWings)
                AddWingStats(7.5f, 1f, 1, 160, "Increases mana regeneration rate");

            if (item.type == ItemID.FairyWings)
                AddWingStats(6.75f, 1f, 1, 130);

            if (item.type == ItemID.BeeWings)
                AddWingStats(7.5f, 1f, 1, 160, "Permanently gives the Honey buff");

            if (item.type == ItemID.HarpyWings)
                AddWingStats(6.75f, 1f, 1, 130, "10% increased movement speed\n" +
                    "With Harpy Ring or Angel Treads equipped, most attacks sometimes launch feathers");

            if (item.type == ItemID.BoneWings)
                AddWingStats(7.5f, 1f, 1, 240, "Halves flight time when taking a hit");

            if (item.type == ItemID.FlameWings)
                AddWingStats(7.5f, 1f, 1, 160, "Multiplies all heat debuff damage by 1.25x");

            if (item.type == ItemID.FrozenWings)
                AddWingStats(6.75f, 1f, 1, 130, "Multiplies all cold debuff damage by 1.25x");

            if (item.type == ItemID.GhostWings)
                AddWingStats(7.5f, 1f, 1, 170);

            if (item.type == ItemID.BeetleWings)
                AddWingStats(7.5f, 1f, 1, 170);

            if (item.type == ItemID.FinWings)
                AddWingStats(6.75f, 1f, 1, 130, "Gills effect and you can move freely through liquids\n" +
                    "You fall faster while submerged in liquid");

            if (item.type == ItemID.FishronWings)
                AddWingStats(8f, 2f, 2, 180);

            if (item.type == ItemID.SteampunkWings)
                AddWingStats(7.5f, 1f, 1, 180);

            if (item.type == ItemID.LeafWings)
                AddWingStats(7.5f, 1f, 1, 160, "+10 defense and permanent Dryad's Blessing");

            if (item.type == ItemID.BatWings)
                AddWingStats(7.5f, 1f, 1, 160, "Improves vision");

            // All developer wings have identical stats and no special effects
            if (item.type == ItemID.Yoraiz0rWings || item.type == ItemID.JimsWings || item.type == ItemID.SkiphsWings ||
                item.type == ItemID.LokisWings || item.type == ItemID.ArkhalisWings || item.type == ItemID.LeinforsWings ||
                item.type == ItemID.BejeweledValkyrieWing || item.type == ItemID.RedsWings || item.type == ItemID.DTownsWings ||
                item.type == ItemID.WillsWings || item.type == ItemID.CrownosWings || item.type == ItemID.CenxsWings ||
                item.type == ItemID.FoodBarbarianWings || item.type == ItemID.GroxTheGreatWings || item.type == ItemID.GhostarsWings ||
                item.type == ItemID.SafemanWings)
            {
                AddWingStats(7f, 1f, 1, 150);
            }

            if (item.type == ItemID.TatteredFairyWings)
                AddWingStats(7.5f, 1f, 1, 180, "You leave a trail of fairy dust as you fly");

            if (item.type == ItemID.SpookyWings)
                AddWingStats(7.5f, 1f, 1, 180);

            if (item.type == ItemID.Hoverboard)
                AddWingStats(6.5f, 1f, 1, 170);

            if (item.type == ItemID.FestiveWings)
                AddWingStats(7.5f, 1f, 1, 180, "Homing ornaments rain down as you fly");

            if (item.type == ItemID.MothronWings)
                AddWingStats(7.5f, 1f, 1, 200);

            if (item.type == ItemID.WingsSolar)
                AddWingStats(9f, 2.5f, 3, 180, "Increases Solar Flare armor's dash velocity by 30%");

            if (item.type == ItemID.WingsStardust)
                AddWingStats(9f, 2.5f, 3, 180, "Greatly increases Stardust armor's stardust guardian damage and attack range");

            if (item.type == ItemID.WingsVortex)
                AddWingStats(6.5f, 1.5f, 2, 180, "Prevents dashes from disabling Vortex armor's stealth ability");

            if (item.type == ItemID.WingsNebula)
                AddWingStats(6.5f, 1.5f, 2, 180, "Increases the pickup range of Nebula armor's nebula boosters");

            // Betsy's Wings (and dev wings) are the only wings without "Allows flight and free fall"
            if (item.type == ItemID.BetsyWings)
                AddWingStats2(6f, 2.5f, 2, 150, null, "Equipable");

            if (item.type == ItemID.RainbowWings)
                AddWingStats(7f, 2.5f, 2, 100);

            if (item.type == ItemID.LongRainbowTrailWings)
                AddWingStats(8f, 2.75f, 4, 180);
            #endregion

            // Provide the full stats of every vanilla grappling hook
            #region Grappling Hook Stat Tooltips

            // This function is shorthand for appending a stat sheet to a grappling hook.
            void AddGrappleStats(float r, float l, float e, float p) => EditTooltipByName("Equipable", (line) => line.Text += "\n" + CalamityUtils.GetText("Common.GrappleStats").Format(r.ToString(), l.ToString(), e.ToString(), p.ToString()));

            if (item.type == ItemID.GrapplingHook)
                AddGrappleStats(18.75f, 11.5f, 11f, 11f);
            if (item.type == ItemID.AmethystHook)
                AddGrappleStats(18.75f, 10f, 11f, 11f);
            if (item.type == ItemID.SquirrelHook)
                AddGrappleStats(19f, 11.5f, 11f, 11f);
            if (item.type == ItemID.TopazHook)
                AddGrappleStats(20.625f, 10.5f, 11.75f, 11f);
            if (item.type == ItemID.SapphireHook)
                AddGrappleStats(22.5f, 11f, 12.5f, 11f);
            if (item.type == ItemID.EmeraldHook)
                AddGrappleStats(24.375f, 11.5f, 13.25f, 11f);
            if (item.type == ItemID.RubyHook)
                AddGrappleStats(26.25f, 12f, 14f, 11f);
            if (item.type == ItemID.AmberHook)
                AddGrappleStats(27.5f, 12.5f, 15f, 11f);
            if (item.type == ItemID.DiamondHook)
                AddGrappleStats(29.125f, 12.5f, 14.75f, 11f);
            if (item.type == ItemID.WebSlinger)
                AddGrappleStats(22.625f, 10f, 11f, 11f);
            if (item.type == ItemID.SkeletronHand)
                AddGrappleStats(21.875f, 15f, 11f, 11f);
            if (item.type == ItemID.SlimeHook)
                AddGrappleStats(18.75f, 13f, 11f, 11f);
            if (item.type == ItemID.FishHook)
                AddGrappleStats(25f, 13f, 11f, 11f);
            if (item.type == ItemID.IvyWhip)
                AddGrappleStats(25f, 13f, 15f, 11f);
            if (item.type == ItemID.BatHook)
                AddGrappleStats(31.25f, 13.5f, 20f, 13f);
            if (item.type == ItemID.CandyCaneHook)
                AddGrappleStats(25f, 11.5f, 11f, 11f);
            if (item.type == ItemID.DualHook)
                AddGrappleStats(27.5f, 14f, 17f, 11f);
            if (item.type == ItemID.QueenSlimeHook)
                AddGrappleStats(30f, 16f, 18f, 11f);
            // these three grapple hooks are all functionally identical
            if (item.type == ItemID.WormHook || item.type == ItemID.TendonHook || item.type == ItemID.IlluminantHook)
                AddGrappleStats(30f, 15f, 18f, 11f);
            if (item.type == ItemID.ThornHook)
                AddGrappleStats(30f, 16f, 18f, 12f);
            if (item.type == ItemID.AntiGravityHook)
                AddGrappleStats(31.25f, 14f, 20f, 11f);
            if (item.type == ItemID.SpookyHook)
                AddGrappleStats(34.375f, 15.5f, 22f, 11f);
            if (item.type == ItemID.ChristmasHook)
                AddGrappleStats(34.375f, 15.5f, 17f, 11f);
            if (item.type == ItemID.LunarHook)
                AddGrappleStats(34.375f, 18f, 24f, 16f);
            if (item.type == ItemID.StaticHook)
                AddGrappleStats(37.5f, 16f, 24f, 0f);
            #endregion

            #region Herbs and Seeds Tooltips

            void AddHerbTooltips(string text)
            {
                int materialIndex = 0;
                for (int i = 0; i < tooltips.Count; ++i)
                    if (tooltips[i].Name == "Material")
                    {
                        materialIndex = i;
                        break;
                    }
                tooltips.Insert(materialIndex + 1, new TooltipLine(CalamityMod.Instance, "Tooltip0", text));
            }

            if (item.type == ItemID.Daybloom)
                AddHerbTooltips(CalamityUtils.GetTextValue("Vanilla.HerbTooltips.Daybloom"));
            if (item.type == ItemID.Moonglow)
                AddHerbTooltips(CalamityUtils.GetTextValue("Vanilla.HerbTooltips.Moonglow"));
            if (item.type == ItemID.Waterleaf)
                AddHerbTooltips(CalamityUtils.GetTextValue("Vanilla.HerbTooltips.Waterleaf"));
            if (item.type == ItemID.Blinkroot)
                AddHerbTooltips(CalamityUtils.GetTextValue("Vanilla.HerbTooltips.Blinkroot"));
            if (item.type == ItemID.Shiverthorn)
                AddHerbTooltips(CalamityUtils.GetTextValue("Vanilla.HerbTooltips.Shiverthorn"));
            if (item.type == ItemID.Deathweed)
                AddHerbTooltips(CalamityUtils.GetTextValue("Vanilla.HerbTooltips.Deathweed"));
            if (item.type == ItemID.Fireblossom)
                AddHerbTooltips(CalamityUtils.GetTextValue("Vanilla.HerbTooltips.Fireblossom"));

            void AddSeedTooltips(string text)
            {
                int materialIndex = 0;
                for (int i = 0; i < tooltips.Count; ++i)
                    if (tooltips[i].Name == "Placeable")
                    {
                        materialIndex = i;
                        break;
                    }
                tooltips.Insert(materialIndex + 1, new TooltipLine(CalamityMod.Instance, "Tooltip0", text));
            }

            if (item.type == ItemID.DaybloomSeeds)
                AddSeedTooltips(CalamityUtils.GetTextValue("Vanilla.SeedTooltips.Daybloom"));
            if (item.type == ItemID.MoonglowSeeds)
                AddSeedTooltips(CalamityUtils.GetTextValue("Vanilla.SeedTooltips.Moonglow"));
            if (item.type == ItemID.WaterleafSeeds)
                AddSeedTooltips(CalamityUtils.GetTextValue("Vanilla.SeedTooltips.Waterleaf"));
            if (item.type == ItemID.BlinkrootSeeds)
                AddSeedTooltips(CalamityUtils.GetTextValue("Vanilla.SeedTooltips.Blinkroot"));
            if (item.type == ItemID.ShiverthornSeeds)
                AddSeedTooltips(CalamityUtils.GetTextValue("Vanilla.SeedTooltips.Shiverthorn"));
            if (item.type == ItemID.DeathweedSeeds)
                AddSeedTooltips(CalamityUtils.GetTextValue("Vanilla.SeedTooltips.Deathweed"));
            if (item.type == ItemID.FireblossomSeeds)
                AddSeedTooltips(CalamityUtils.GetTextValue("Vanilla.SeedTooltips.Fireblossom"));

            #endregion

            // Beyond this point all code only applies to accessories. Skip it all if the item is not an accessory.
            if (!item.accessory)
                return;

            // Display the stat changes to vanilla prefixes
            #region Accessory Prefix Rebalance Tooltips

            // Turns a number into a string of increased DR.
            string DRString(float percent) => "\n" + GetAddedTooltip("DefensePrefix").Format(percent.ToString());

            switch (item.prefix)
            {
                case PrefixID.Hard:
                    EditTooltipByName("PrefixAccDefense",
                        (line) => line.Text = line.Text.Replace("1", CalamityUtils.GetScalingDefense(item.prefix).ToString()) + DRString(0.25f));
                    return;
                case PrefixID.Guarding:
                    EditTooltipByName("PrefixAccDefense",
                        (line) => line.Text = line.Text.Replace("2", CalamityUtils.GetScalingDefense(item.prefix).ToString()) + DRString(0.5f));
                    return;
                case PrefixID.Armored:
                    EditTooltipByName("PrefixAccDefense",
                        (line) => line.Text = line.Text.Replace("3", CalamityUtils.GetScalingDefense(item.prefix).ToString()) + DRString(0.75f));
                    return;
                case PrefixID.Warding:
                    EditTooltipByName("PrefixAccDefense",
                        (line) => line.Text = line.Text.Replace("4", CalamityUtils.GetScalingDefense(item.prefix).ToString()) + DRString(1f));
                    return;
                case PrefixID.Lucky:
                    EditTooltipByName("PrefixAccCritChance", (line) => line.Text += AddedTooltip("LuckyPrefix"));
                    return;
            }
            #endregion
        }
        #endregion

        #region Speed Tooltips

        // TODO: Investigate using a SortedDictionary instead? May be slower, but removes the need for carefully adding KVPs.
        /// <summary>
        /// This dictionary handles easily retrieving tooltip text based on a numerical threshold. <br />
        /// As items are added to the dictionary, the keys should only increase as they go down. <br />
        /// For example: <code>{ 2, x }, { 4, y }, { 7, z }, ...</code>. <br />
        /// When iterating with the threshold in mind, this essentially equates to: <br />
        /// <code>
        /// if (foo &lt;= 2) bar = x;
        /// else if (foo &lt;= 4) bar = y;
        /// else if (foo &lt;= 7) bar = z;
        /// </code>
        /// </summary>
        /// <remarks>
        /// Currently, the dictionary functions as follows: <br />
        /// 1-5   insanely fast <br />
        /// 6-9   very fast <br />
        /// 10-14 fast <br />
        /// 15-22 average <br />
        /// 23-29 slow <br />
        /// 30-37 very slow <br />
        /// 38-45 extremely slow <br />
        /// 46+   snail
        /// </remarks>
        private static readonly Dictionary<int, LocalizedText> SpeedTooltips = new Dictionary<int, LocalizedText>()
        {
            { 5, Language.GetText("LegacyTooltip.6") },
            { 9, Language.GetText("LegacyTooltip.7") },
            { 14, Language.GetText("LegacyTooltip.8") },
            { 22, Language.GetText("LegacyTooltip.9") },
            { 29, Language.GetText("LegacyTooltip.10") },
            { 37, Language.GetText("LegacyTooltip.11") },
            { 45, Language.GetText("LegacyTooltip.12") },
            // TODO: Using int.MaxValue here may be considered kind of strange - only alternatives I can think of require hardcoding.
            { int.MaxValue, Language.GetText("LegacyTooltip.13") }
        };

        private static void RedistributeSpeedTooltips(Item item, TooltipLine line)
        {
            // Iterate through each KeyValuePair in this dictionary.
            // See the summary of SpeedTooltips to understand the purpose and logic of this loop.
            foreach ((int threshold, LocalizedText tooltip) in SpeedTooltips)
                if (item.useAnimation <= threshold)
                {
                    line.Text = tooltip.Value;
                    break;
                }
        }
        #endregion

        #region Enchanted Rarity Text Drawing
        public override bool PreDrawTooltipLine(Item item, DrawableTooltipLine line, ref int yOffset)
        {
            // Special enchantment line color.
            if (line.Name == "ItemName" && line.Mod == "Terraria" && item.IsEnchanted())
            {
                Color rarityColor = line.OverrideColor ?? line.Color;
                Vector2 basePosition = new Vector2(line.X, line.Y);

                float backInterpolant = (float)Math.Pow(Main.GlobalTimeWrappedHourly * 0.81f % 1f, 1.5f);
                Vector2 backScale = line.BaseScale * MathHelper.Lerp(1f, 1.2f, backInterpolant);
                Color backColor = Color.Lerp(rarityColor, Color.DarkRed, backInterpolant) * (float)Math.Pow(1f - backInterpolant, 0.46f);
                Vector2 backPosition = basePosition - new Vector2(1f, 0.1f) * backInterpolant * 10f;

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, null, null, null, null, Main.UIScaleMatrix);

                // Draw the back text as an ominous pulse.
                for (int i = 0; i < 2; i++)
                    ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, line.Font, line.Text, backPosition, backColor, line.Rotation, line.Origin, backScale, line.MaxWidth, line.Spread);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, null, null, null, null, Main.UIScaleMatrix);

                // Draw the front text as usual.
                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, line.Font, line.Text, basePosition, rarityColor, line.Rotation, line.Origin, line.BaseScale, line.MaxWidth, line.Spread);

                return false;
            }
            return true;
        }
        #endregion

        #region Schematic Knowledge Tooltip Utility
        public static void InsertKnowledgeTooltip(List<TooltipLine> tooltips, int tier, bool allowOldWorlds = false)
        {
            TooltipLine line = new TooltipLine(CalamityMod.Instance, "SchematicKnowledge1", "You don't have sufficient knowledge to create this yet");
            TooltipLine line2 = new TooltipLine(CalamityMod.Instance, "SchematicKnowledge2", "A specific schematic must be deciphered first");
            switch (tier)
            {
                case 1:
                    line2 = new TooltipLine(CalamityMod.Instance, "SchematicKnowledge2", "The Sunken Sea schematic must be deciphered first");
                    break;
                case 2:
                    line2 = new TooltipLine(CalamityMod.Instance, "SchematicKnowledge2", "The Planetoid schematic must be deciphered first");
                    break;
                case 3:
                    line2 = new TooltipLine(CalamityMod.Instance, "SchematicKnowledge2", "The Jungle schematic must be deciphered first");
                    break;
                case 4:
                    line2 = new TooltipLine(CalamityMod.Instance, "SchematicKnowledge2", "The Underworld schematic must be deciphered first");
                    break;
                case 5:
                    line2 = new TooltipLine(CalamityMod.Instance, "SchematicKnowledge2", "The Ice biome schematic must be deciphered first");
                    break;
            }
            line.OverrideColor = line2.OverrideColor = Color.Cyan;

            bool allowedDueToOldWorld = allowOldWorlds && CalamityWorld.IsWorldAfterDraedonUpdate;
            tooltips.AddWithCondition(line, !ArsenalTierGatedRecipe.HasTierBeenLearned(tier) && !allowedDueToOldWorld);
            tooltips.AddWithCondition(line2, !ArsenalTierGatedRecipe.HasTierBeenLearned(tier) && !allowedDueToOldWorld);
        }
        #endregion
    }
}
