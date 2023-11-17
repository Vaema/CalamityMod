using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using CalamityMod.Items.Placeables;
using CalamityMod.Items.Materials;

namespace CalamityMod.Items.Armor.Vanity
{
    [AutoloadEquip(EquipType.Head)]
    public class SnailHead : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.Vanity";

        public override void SetStaticDefaults()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false;
            }
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 22;
            Item.rare = ItemRarityID.Cyan;
            Item.vanity = true;
            Item.Calamity().devItem = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AbyssGravel>(10).
                AddIngredient<Lumenyl>(18).
                AddTile(TileID.Loom).
                Register();
        }
    }
}
