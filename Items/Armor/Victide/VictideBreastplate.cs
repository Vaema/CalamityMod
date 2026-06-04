using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Victide
{
    [AutoloadEquip(EquipType.Body)]
    public class VictideBreastplate : ModItem, IBulkyArmor, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PreHardmode";
        public string BulkTexture => "CalamityMod/Items/Armor/Victide/VictideBreastplate_Bulk";

        public static int RegenBoost = 1;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RegenBoost.ToRegenPerSecond());

        public override void Load()
        {
            if (!Main.dedServ)
            {
                //register the faulds texture. This appears either when the leggings  or the chestplate is equipped (both works)
                EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Armor/Victide/VictideFaulds_Waist", EquipType.Waist, name: "VictideFaulds");
            }
        }

        public override void SetStaticDefaults()
        {
            if (!Main.dedServ)
            {
                var equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);
                ArmorIDs.Body.Sets.HidesArms[equipSlot] = true;
                ArmorIDs.Body.Sets.HidesTopSkin[equipSlot] = true;
            }
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.defense = 4;
        }

        public override void UpdateEquip(Player player)
        {
            player.lifeRegen += RegenBoost;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<SeaRemains>(5).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
