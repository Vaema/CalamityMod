using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CalamityMod.Items.Accessories
{
    public class VampiricTalisman : ModItem, ILocalizedModType, IHoldShiftTooltipItem
    {
        internal const int ArmorCrunchDebuffTime = 150;
        internal const int HeavyBleedingDebuffTime = 300;
        public const float RaiderBonus = 15f;

        public bool ShowExtensionIndicator => false;

        // Easter egg has a special tooltip key and color.
        public string TooltipExtensionKey => "YearningForBlood";
        public Color? TooltipExtensionColor => Color.Red;

        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 86;
            Item.height = 48;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.accessory = true;
            Item.rare = ItemRarityID.Lime;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.vampiricTalisman = true;
            modPlayer.raiderTalisman = true;
            modPlayer.rottenDogTooth = true;

            //get fixed boi funny
            if (Main.zenithWorld)
                player.lifeRegen -= 10; //Never ending thirst
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<RaidersTalisman>().
                AddIngredient<RottenDogtooth>().
                AddIngredient<SolarVeil>(10).
                AddIngredient(ItemID.BrokenBatWing).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
