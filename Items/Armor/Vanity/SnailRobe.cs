using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using CalamityMod.Items.Placeables;
using CalamityMod.Items.Materials;

namespace CalamityMod.Items.Armor.Vanity
{
    [AutoloadEquip(EquipType.Body)]
    public class SnailRobe : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.Vanity";

        public override void Load()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                EquipLoader.AddEquipTexture(Mod, "CalamityMod/Items/Armor/Vanity/SnailRobe_Legs", EquipType.Legs, this);
            }
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 30;
            Item.vanity = true;
            Item.Calamity().devItem = true;
            Item.rare = ItemRarityID.Cyan;
        }

        public override void SetMatch(bool male, ref int equipSlot, ref bool robes)
        {
            robes = true;
            equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Legs);
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AbyssGravel>(22).
                AddIngredient<Lumenyl>(25).
                AddIngredient(ItemID.Silk, 25).
                AddTile(TileID.Loom).
                Register();
        }
    }
}
