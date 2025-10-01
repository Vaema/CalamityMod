using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    [AutoloadEquip(EquipType.Face)]
    public class MoonstoneCrown : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";

        public override void SetStaticDefaults()
        {
            if (!Main.dedServ)
            {
                int equipSlot = EquipLoader.GetEquipSlot(Mod, Name, EquipType.Face);
                ArmorIDs.Face.Sets.OverrideHelmet[equipSlot] = true;
            }
        }

        public override void SetDefaults()
        {
            Item.width = 46;
            Item.height = 40;
            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.rare = ItemRarityID.Purple;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.moonCrown = true;
            player.statManaMax2 += 70;
            player.GetDamage<MagicDamageClass>() += (0.02f * modPlayer.mageCrownCount); //2% per moon sigil, up to 20%
            player.manaCost -= (0.01f * modPlayer.mageCrownCount); //1% per moon sigil, up to 10%
            if (modPlayer.mageCrownCount >= 4) //I know this looks weird but the count starts at 0
            {
                player.manaRegenBonus += 10;
            }
            if (modPlayer.mageCrownCount == 9)
            {
                player.GetCritChance<MagicDamageClass>() += 10;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<FeatherCrown>().
                AddIngredient(ItemID.LunarBar, 5).
                AddIngredient<GalacticaSingularity>(5).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
