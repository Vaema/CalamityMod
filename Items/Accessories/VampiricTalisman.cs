using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class VampiricTalisman : ModItem, ILocalizedModType
    {
        internal const int ArmorCrunchDebuffTime = 150;
        internal const int HeavyBleedingDebuffTime = 300;
        internal const float StealthStrikeDamageMultiplier = 0.08f;

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
            player.Calamity().bonusStealthDamage += StealthStrikeDamageMultiplier;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<RaidersTalisman>().
                AddIngredient<RottenDogtooth>().
                AddIngredient<SolarVeil>(10).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
