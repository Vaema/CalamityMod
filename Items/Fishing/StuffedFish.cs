using CalamityMod.Items.Placeables.Astral;
using CalamityMod.Items.Placeables.Crags;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing;

public class StuffedFish : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Fishing";
    public static List<int> HerbDisplay = new()
    {
        ItemID.Daybloom,
        ItemID.Blinkroot,
        ItemID.Waterleaf,
        ItemID.Shiverthorn,
        ItemID.Moonglow,
        ItemID.Deathweed,
        ItemID.Fireblossom
    };
    public static List<int> SeedDisplay = new()
    {
        ItemID.DaybloomSeeds,
        ItemID.BlinkrootSeeds,
        ItemID.WaterleafSeeds,
        ItemID.ShiverthornSeeds,
        ItemID.MoonglowSeeds,
        ItemID.DeathweedSeeds,
        ItemID.FireblossomSeeds,
        ItemID.GrassSeeds,
        ItemID.JungleGrassSeeds,
        ItemID.MushroomGrassSeeds,
        ItemID.AshGrassSeeds,
        ModContent.ItemType<CinderBlossomSeeds>(),
        ItemID.PumpkinSeed
    };

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 10;
        ItemID.Sets.CanBePlacedOnWeaponRacks[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 34;
        Item.height = 30;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.value = Item.sellPrice(silver: 10);
        Item.rare = ItemRarityID.Green;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.GoodieBags;
    }

    public override bool CanRightClick() => true;
    public override void ModifyItemLoot(ItemLoot itemLoot)
    {
        int herbMin = 1;
        int herbMax = 3;
        int seedMin = 2;
        int seedMax = 5;

        // Herbs
        itemLoot.Add(ItemID.Daybloom, 4, herbMin, herbMax);
        itemLoot.Add(ItemID.Blinkroot, 4, herbMin, herbMax);
        itemLoot.Add(ItemID.Waterleaf, 4, herbMin, herbMax);
        itemLoot.Add(ItemID.Shiverthorn, 4, herbMin, herbMax);
        itemLoot.Add(ItemID.Moonglow, 4, herbMin, herbMax);
        itemLoot.Add(ItemID.Deathweed, 4, herbMin, herbMax);
        itemLoot.Add(ItemID.Fireblossom, 4, herbMin, herbMax);

        // Herb seeds
        itemLoot.Add(ItemID.DaybloomSeeds, 5, seedMin, seedMax);
        itemLoot.Add(ItemID.BlinkrootSeeds, 5, seedMin, seedMax);
        itemLoot.Add(ItemID.WaterleafSeeds, 5, seedMin, seedMax);
        itemLoot.Add(ItemID.ShiverthornSeeds, 5, seedMin, seedMax);
        itemLoot.Add(ItemID.MoonglowSeeds, 5, seedMin, seedMax);
        itemLoot.Add(ItemID.DeathweedSeeds, 5, herbMin, herbMax);
        itemLoot.Add(ItemID.FireblossomSeeds, 5, seedMin, seedMax);

        // Miscellaneous seeds
        itemLoot.Add(ItemID.GrassSeeds, 10, seedMin, seedMax);
        itemLoot.Add(ItemID.JungleGrassSeeds, 10, seedMin, seedMax);
        itemLoot.Add(ItemID.MushroomGrassSeeds, 10, seedMin, seedMax);
        itemLoot.Add(ItemID.AshGrassSeeds, 20, seedMin, seedMax);
        itemLoot.Add(ModContent.ItemType<CinderBlossomSeeds>(), 20, seedMin, seedMax);
        itemLoot.Add(ItemID.PumpkinSeed, 20, seedMin, seedMax);

        // Biome grass seeds
        itemLoot.AddIf(() => !WorldGen.crimson, ItemID.CorruptSeeds, 20, seedMin, seedMax);
        itemLoot.AddIf(() => WorldGen.crimson, ItemID.CrimsonSeeds, 20, seedMin, seedMax);
        itemLoot.AddIf(() => Main.hardMode, ItemID.HallowedSeeds, 20, seedMin, seedMax);
        itemLoot.AddIf(() => Main.hardMode, ModContent.ItemType<AstralGrassSeeds>(), 20, seedMin, seedMax);

        // Add Thorium Marine Kelp if Thorium is loaded.
        Mod thorium = ExternalMods.thorium;
        if (thorium is null)
            return;

        var marineKelp = thorium.Find<ModItem>("MarineKelp");
        var marineKelpSeeds = thorium.Find<ModItem>("MarineKelpSeeds");
        if (marineKelp is not null && marineKelpSeeds is not null)
        {
            itemLoot.Add(marineKelp.Type, 4, herbMin, herbMax);
            itemLoot.Add(marineKelpSeeds.Type, 10, seedMin, seedMax);
        }
        else
            CalamityMod.Log.Warn("Could not find either Marine Kelp or Marine Kelp Seeds from Thorium. These items will not be added to Stuffed Fish.");
    }

    public override void ModifyTooltips(List<TooltipLine> list)
    {
        int currentHerb = (int)(Main.GlobalTimeWrappedHourly * 1.5f) % HerbDisplay.Count;
        list.FindAndReplace("[HERBS]", $"[i:{HerbDisplay[currentHerb]}]");

        int currentSeed = (int)(Main.GlobalTimeWrappedHourly * 1.5f) % SeedDisplay.Count;
        list.FindAndReplace("[SEEDS]", $"[i:{SeedDisplay[currentSeed]}]");
    }
}
