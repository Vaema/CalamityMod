using System.Collections.Generic;
using CalamityMod.Items.Armor.Bloodflare;
using CalamityMod.Items.Armor.Silva;
using CalamityMod.Items.Armor.Tarragon;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using static CalamityMod.Items.Armor.Bloodflare.BloodflareHeadSummon;

namespace CalamityMod.Items.Armor.Auric
{
    [AutoloadEquip(EquipType.Head)]
    [LegacyName("AuricTeslaSpaceHelmet")]
    public class AuricTeslaHeadSummon : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PostMoonLord";
        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.defense = 12; //132
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
            modPlayer.tarraSet = true;
            modPlayer.tarraSummon = true;
            modPlayer.bloodflareSet = true;
            modPlayer.bloodflareSummon = true;
            modPlayer.silvaSet = true;
            modPlayer.silvaSummon = true;
            modPlayer.auricSet = true;
            modPlayer.WearingPostMLSummonerSet = true;
            player.crimsonRegen = true; // Inherited from Bloodflare
            player.GetDamage<SummonDamageClass>() += 0.75f;
            player.maxMinions += 6;
        }

        public override void UpdateEquip(Player player)
        {
            var modPlayer = player.Calamity();
            modPlayer.auricBoost = true;
            player.GetDamage<SummonDamageClass>() += 0.15f;
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
                        Color[] armorColors = { AuricTeslaBodyArmor.tooltipTarragonColor, AuricTeslaBodyArmor.tooltipBloodflareColor, AuricTeslaBodyArmor.tooltipSilvaColor };
                        var LocalizedText = this.GetLocalization($"SetBonus{setBonusTooltipNumber}");
                        line.Text = (setBonusTooltipNumber == 3 ? LocalizedText.Format()
                        : setBonusTooltipNumber == 2 ? LocalizedText.Format(DefenseBoostBelowHealthThreshold, DefenseBoostHealthThreshold.ToPercent())
                        : LocalizedText.Format());
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
                AddIngredient<SilvaHeadSummon>().
                AddIngredient<BloodflareHeadSummon>().
                AddIngredient<TarragonHeadSummon>().
                AddIngredient<AuricBar>(12).
                AddTile<CosmicAnvil>().
                SortBeforeFirstRecipesOf(ModContent.ItemType<AuricTeslaHeadRogue>()).
                Register();
        }
    }
}
