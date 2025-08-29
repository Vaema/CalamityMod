using System.Collections.Generic;
using CalamityMod.CalPlayer.Dashes;
using CalamityMod.Items.Armor.Bloodflare;
using CalamityMod.Items.Armor.GodSlayer;
using CalamityMod.Items.Armor.Tarragon;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using static CalamityMod.Items.Armor.Bloodflare.BloodflareHeadMelee;
using static CalamityMod.Items.Armor.Tarragon.TarragonHeadMelee;

namespace CalamityMod.Items.Armor.Auric
{
    [AutoloadEquip(EquipType.Head)]
    [LegacyName("AuricTeslaHelm")]
    [LegacyName("AuricTeslaRoyalHelm")]
    public class AuricTeslaHeadMelee : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PostMoonLord";
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.defense = 54; //132
            Item.rare = ModContent.RarityType<BurnishedAuric>();
        }

        public override bool IsArmorSet(Item head, Item body, Item legs)
        {
            return body.type == ModContent.ItemType<AuricTeslaBodyArmor>() && legs.type == ModContent.ItemType<AuricTeslaCuisses>();
        }

        public override void ArmorSetShadows(Player player)
        {
            player.armorEffectDrawOutlines = true;
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = this.GetLocalizedValue("SetBonus");
            var modPlayer = player.Calamity();
            player.GetAttackSpeed<MeleeDamageClass>() += 0.28f;
            modPlayer.tarraSet = true;
            modPlayer.tarraMelee = true;
            modPlayer.bloodflareSet = true;
            modPlayer.bloodflareMelee = true;
            modPlayer.godSlayer = true;
            modPlayer.godSlayerDamage = true;
            modPlayer.auricSet = true;
            modPlayer.auricSetMelee = true;
            player.crimsonRegen = true; // Inherited from Bloodflare
            player.aggro += 1200;

            if (modPlayer.godSlayerDashHotKeyPressed || (player.dashDelay != 0 && modPlayer.LastUsedDashID == GodslayerArmorDash.ID))
                modPlayer.DeferredDashID = GodslayerArmorDash.ID;
        }

        public override void UpdateEquip(Player player)
        {
            var modPlayer = player.Calamity();
            modPlayer.auricBoost = true;
            player.GetDamage<MeleeDamageClass>() += 0.2f;
            player.GetCritChance<MeleeDamageClass>() += 10;
        }
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            ref var holdingShift = ref AuricTeslaBodyArmor.holdingShift;
            ref var setBonusTooltipNumber = ref AuricTeslaBodyArmor.setBonusTooltipNumber;
            if (Main.keyState.PressingShift())
            {
                if (!holdingShift)
                {
                    holdingShift = true;
                    setBonusTooltipNumber++;
                    if (setBonusTooltipNumber > 3) setBonusTooltipNumber = 1;
                }
                foreach (TooltipLine line in tooltips)
                {
                    if (line.Name == "SetBonus")
                    {
                        Color[] armorColors = { AuricTeslaBodyArmor.tooltipTarragonColor, AuricTeslaBodyArmor.tooltipBloodflareColor, AuricTeslaBodyArmor.tooltipGodslayerColor };
                        var LocalizedText = CalamityUtils.GetTextFromModItem(Type, $"SetBonus{setBonusTooltipNumber}");
                        line.Text = (setBonusTooltipNumber == 3 ? LocalizedText.Format(CalamityKeybinds.GodSlayerDashHotKey.TooltipHotkeyString(), GodslayerArmorDash.GodslayerCooldown)
                        : setBonusTooltipNumber == 2 ? LocalizedText.Format(HitsToActivateFrenzy, FrenzyDuration.FramesToSeconds(), FrenzyMeleeDamageBoost.ToPercent(), FrenzyContactDamageReduction.ToPercent(), FrenzyCooldown.FramesToSeconds())
                        : LocalizedText.Format(TarraLifeRegenBoost.ToRegenPerSecond(), CalamityUtils.GetArmorSetBonusKey(), CloakDuration.FramesToSeconds(), CloakCooldown.FramesToSeconds()));
                        line.OverrideColor = armorColors[setBonusTooltipNumber - 1];
                    }
                }
            }
            else
            {
                holdingShift = false;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<GodSlayerHeadMelee>().
                AddIngredient<BloodflareHeadMelee>().
                AddIngredient<TarragonHeadMelee>().
                AddIngredient<AuricBar>(12).
                AddTile<CosmicAnvil>().
                SortBeforeFirstRecipesOf(ModContent.ItemType<AuricTeslaHeadMagic>()).
                Register();
        }
    }
}
