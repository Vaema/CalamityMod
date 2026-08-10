using System.Collections.Generic;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Placeables.Ores;
using CalamityMod.Items.Placeables.Plates;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Tools;

public class NormalityRelocator : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Tools";
    public static readonly SoundStyle TeleportSound = new("CalamityMod/Sounds/Item/NormalityRelocator", 3);
    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 7));
        ItemID.Sets.AnimatesAsSoul[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 38;
        Item.height = 38;
        Item.value = CalamityGlobalItem.RarityRedBuyPrice;
        Item.rare = ItemRarityID.Red;
        Item.Calamity().donorItem = true;
    }

    public override void ModifyTooltips(List<TooltipLine> list) => list.IntegrateHotkey(CalamityKeybinds.NormalityRelocatorHotKey);

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        itemGroup = (ContentSamples.CreativeHelper.ItemGroup)CalamityResearchSorting.ToolsOther;
    }

    public override void UpdateInventory(Player player)
    {
        CalamityPlayer modPlayer = player.Calamity();
        modPlayer.normalityRelocator = true;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.RodofDiscord).
            AddIngredient(ItemID.FragmentStardust, 30).
            AddIngredient<Cinderplate>(5).
            AddIngredient<ExodiumCluster>(10).
            AddTile(TileID.MythrilAnvil).
            Register();
    }
}
