using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Daedalus
{
    [AutoloadEquip(EquipType.Head)]
    [LegacyName("DaedalusHelmet")]
    public class DaedalusHeadRanged : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.Hardmode";

        public static float RangedDamageBoost = 0.13f;
        public static int RangedCritBoost = 7;
        // NOTE: Ammo conservation is a bool so the number is manually added in the tooltip and equip
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RangedDamageBoost.ToPercent(), RangedCritBoost);

        // Set Bonus
        public static int ShardDamage = 60;

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;
            Item.defense = 9; // 43
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<DaedalusBreastplate>() && legs.type == ModContent.ItemType<DaedalusLeggings>();

        public override void ArmorSetShadows(Player player)
        {
            player.armorEffectDrawShadowSubtle = true;
            player.armorEffectDrawOutlines = true;
        }

        public override void UpdateArmorSet(Player player)
        {
            player.setBonus = this.GetLocalizedValue("SetBonus");
            var modPlayer = player.Calamity();
            modPlayer.daedalusShard = true;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<RangedDamageClass>() += RangedDamageBoost;
            player.GetCritChance<RangedDamageClass>() += RangedCritBoost;
            player.ammoCost80 = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<CryonicBar>(7).
                AddIngredient<EssenceofEleum>().
                AddTile(TileID.MythrilAnvil).
                SortBeforeFirstRecipesOf(ModContent.ItemType<DaedalusHeadMagic>()).
                Register();
        }
    }
}
