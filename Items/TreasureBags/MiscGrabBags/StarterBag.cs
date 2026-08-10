using CalamityMod.Items.Accessories.Vanity;
using CalamityMod.Items.LoreItems;
using CalamityMod.Items.Pets;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Weapons.Summon;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.TreasureBags.MiscGrabBags;

public class StarterBag : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.TreasureBags";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 0;
    }

    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 24;
        Item.consumable = true;
        Item.rare = ItemRarityID.Blue;
    }

    public override bool CanRightClick() => true;

    public override void ModifyItemLoot(ItemLoot itemLoot)
    {
        // Weapons
        // Tin and copper content is explicitly separated
        LeadingConditionRule tin = itemLoot.DefineConditionalDropSet(() => WorldGen.SavedOreTiers.Copper == TileID.Tin);
        tin.Add(ItemID.TinBroadsword);
        tin.Add(ItemID.TinBow);
        tin.Add(ItemID.TopazStaff);
        tin.OnFailedConditions(new CommonDrop(ItemID.CopperBroadsword, 1));
        tin.OnFailedConditions(new CommonDrop(ItemID.CopperBow, 1));
        tin.OnFailedConditions(new CommonDrop(ItemID.AmethystStaff, 1));
        itemLoot.Add(ItemID.WoodenArrow, 1, 100, 100); // You must specify 100 to 100.
        itemLoot.Add(ModContent.ItemType<SquirrelSquireStaff>());
        itemLoot.Add(ModContent.ItemType<ThrowingBrick>(), 1, 150, 150);

        // 1 Mana Crystal
        itemLoot.Add(ItemID.ManaCrystal);

        // Tools and Utility Items
        tin.Add(ItemID.TinHammer);
        tin.OnFailedConditions(new CommonDrop(ItemID.CopperHammer, 1));
        itemLoot.Add(ItemID.Rope, 1, 50, 50);

        // Potions
        LeadingConditionRule multiplayer = itemLoot.DefineConditionalDropSet(() => Main.netMode == NetmodeID.MultiplayerClient);
        itemLoot.Add(ItemID.RecallPotion, 1, 3, 3);
        multiplayer.Add(ItemID.WormholePotion, 1, 3, 3);

        // Tiles
        itemLoot.Add(ItemID.Torch, 1, 25, 25);

        // Calamity title theme music box (if music mod is enabled)
        Mod musicMod = ExternalMods.musicMod;
        if (musicMod is not null)
            itemLoot.Add(musicMod.Find<ModItem>("CalamityMusicbox").Type);

        // Awakening lore item
        itemLoot.Add(ModContent.ItemType<LoreAwakening>());

        // Aleksh donator item
        // Name specific: "Aleksh" or "Shark Lad"
        static bool getsLadPet(DropAttemptInfo info)
        {
            string playerName = info.player.name;
            return playerName == "Aleksh" || playerName == "Shark Lad";
        };
        itemLoot.AddIf(getsLadPet, ModContent.ItemType<JoyfulHeart>());

        // HPU dev item
        // Name specific: "Heart Plus Up"
        static bool getsHapuFruit(DropAttemptInfo info)
        {
            string playerName = info.player.name;
            return playerName == "Heart Plus Up";
        };
        itemLoot.AddIf(getsHapuFruit, ModContent.ItemType<HapuFruit>());

        // CIT vanity item
        // Name specific: "CongratsIsTrash" or "CIT"
        static bool getsSharkyPlush(DropAttemptInfo info)
        {
            string playerName = info.player.name;
            return playerName == "CongratsIsTrash" || playerName == "CIT";
        }
        itemLoot.AddIf(getsSharkyPlush, ModContent.ItemType<SharkyPlush>());

        // Dandy dev item
        // Name specific: "Dandy"
        static bool getsGhostBracelet(DropAttemptInfo info)
        {
            string playerName = info.player.name;
            return playerName == "Dandy";
        }

        itemLoot.AddIf(getsGhostBracelet, ModContent.ItemType<GhostBracelet>());

        // Xyk dev item
        // Name specific: "Xyk"
        static bool getsXyksBlessing(DropAttemptInfo info)
        {
            string playerName = info.player.name;
            return playerName.Contains("Xyk"); // Any name containing "Xyk" will work
        }

        itemLoot.AddIf(getsXyksBlessing, ModContent.ItemType<XyksBlessingBlue>());

        // Mishiro dev vanity
        // Name specific: "Amber" or "Mishiro"
        static bool getsOracleHeadphones(DropAttemptInfo info)
        {
            string playerName = info.player.name;
            return playerName is "Amber" or "Mishiro";
        }

        itemLoot.AddIf(getsOracleHeadphones, ModContent.ItemType<OracleHeadphones>());

        // Big E dev vanity
        // Name specific: "Big E" in any form
        static bool getsLittleE(DropAttemptInfo info)
        {
            string playerName = info.player.name.ToLower();
            return playerName is "big e" or "bige";
        }

        itemLoot.AddIf(getsLittleE, ModContent.ItemType<LittleE>());

        // Sagittariod dev vanity
        // Name specific: "Sagi" or "Sagittariod" (case insensitive
        static bool getsShimmeringRibbon(DropAttemptInfo info)
        {
            string playerName = info.player.name.ToLower();
            return playerName is "sagi" or "sagittariod";
        }

        itemLoot.AddIf(getsShimmeringRibbon, ModContent.ItemType<GlimmeringRibbon>());
    }
}
