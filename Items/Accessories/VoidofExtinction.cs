using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class VoidofExtinction : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public static int CritBoost = 13;
        public static int VoidExploDamage = 40;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(CritBoost, Abaddon.BrimstoneFlamesReduction.ToPercent());

        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.rare = ItemRarityID.Yellow;
            Item.accessory = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Abaddon>().
                AddIngredient<ScoriaBar>(3).
                AddIngredient<CoreofCalamity>().
                AddTile(TileID.MythrilAnvil).
                Register();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.voidOfExtinction = true;
            modPlayer.abaddon = true;
            player.GetCritChance<GenericDamageClass>() += CritBoost;

        }
    }
}
