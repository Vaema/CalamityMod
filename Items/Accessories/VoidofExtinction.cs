using System.Collections.Generic;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class VoidofExtinction : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public static float critScaling = 6; // How effective crit chance is at increasing debuff damage
        public static float critDamageBoostPerDebuff = 0.03f; // 3% increased crit damage per debuff
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
            modPlayer.abaddonEffectVisual = !hideVisual;
        }
        public override void ModifyTooltips(List<TooltipLine> list)
        {
            Player player = Main.LocalPlayer;
            if (Main.LocalPlayer != null)
                list.FindAndReplace("[DAMAGE]", ((int)(Bane.debuffData.EnemyLostRegen / 2 * player.Calamity().abaddonDebuffDamage)).ToString());
        }
    }
}
