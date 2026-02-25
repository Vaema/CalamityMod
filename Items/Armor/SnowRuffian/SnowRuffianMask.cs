using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.SnowRuffian
{
    [AutoloadEquip(EquipType.Head)]
    public class SnowRuffianMask : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PreHardmode";

        public static float RangedDamageBoost = 0.05f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RangedDamageBoost.ToPercent());

        // Set Bonus
        public static float GlideFallSpeedMult = 0.9f;
        public static int SetBonusFrostburnDuration = CalamityUtils.SecondsToFrames(2);

        public override void Load()
        {
            if (!Main.dedServ)
            {
                EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Armor/SnowRuffian/SnowRuffianWings", EquipType.Wings, this, equipTexture: new SnowRuffianWings());
            }
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.rare = ItemRarityID.Blue;
            Item.defense = 2; // 9
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<SnowRuffianChestplate>() && legs.type == ModContent.ItemType<SnowRuffianGreaves>();

        public override void UpdateArmorSet(Player player)
        {
            var modPlayer = player.Calamity();
            modPlayer.snowRuffianSet = true;
            player.setBonus = this.GetLocalization("SetBonus").Format(SetBonusFrostburnDuration.FramesToSeconds());
        }

        public override void UpdateEquip(Player player) => player.GetDamage<RangedDamageClass>() += RangedDamageBoost;

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.BorealWood, 10).
                AddIngredient(ItemID.Silk, 4).
                AddIngredient(ItemID.FlinxFur).
                AddTile(TileID.Anvils).
                SortBeforeFirstRecipesOf(ModContent.ItemType<SnowRuffianChestplate>()).
                Register();
        }
    }

    public class SnowRuffianWings : EquipTexture
    {
    }
}
