using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.FathomSwarmer
{
    [AutoloadEquip(EquipType.Body)]
    public class FathomSwarmerBreastplate : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.Hardmode";
        public override void SetStaticDefaults()
        {

            if (Main.netMode == NetmodeID.Server)
                return;

            int equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Body);

            ArmorIDs.Body.Sets.HidesTopSkin[equipSlot] = true;
            ArmorIDs.Body.Sets.HidesArms[equipSlot] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.defense = 20;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<SummonDamageClass>() += 0.1f;
            player.endurance += 0.06f;
            player.maxMinions += 1;
            player.Calamity().fathomSwarmerBreastplate = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<SeaRemains>(12).
                AddIngredient<PlantyMush>(10).
                AddIngredient<DepthCells>(5).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
