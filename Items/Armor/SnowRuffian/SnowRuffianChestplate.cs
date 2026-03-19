using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.SnowRuffian
{
    [AutoloadEquip(EquipType.Body)]
    public class SnowRuffianChestplate : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PreHardmode";

        public static int RangedCritBoost = 4;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RangedCritBoost);

        public override void Load()
        {
            if (Main.dedServ)
                return;

            EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Armor/SnowRuffian/SnowRuffianChestplate_Back", EquipType.Back, this);
            EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Armor/SnowRuffian/SnowRuffianChestplate_Neck", EquipType.Neck, this);
        }

        public override void SetStaticDefaults()
        {
            if (Main.dedServ)
                return;

            int equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);

            ArmorIDs.Body.Sets.HidesTopSkin[equipSlot] = true;
            ArmorIDs.Body.Sets.HidesArms[equipSlot] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.rare = ItemRarityID.Blue;
            Item.defense = 4; //9
        }

        public override void UpdateEquip(Player player) => player.GetCritChance<RangedDamageClass>() += RangedCritBoost;

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.BorealWood, 20).
                AddIngredient(ItemID.Silk, 6).
                AddIngredient(ItemID.FlinxFur, 2).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
