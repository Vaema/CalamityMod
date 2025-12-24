using CalamityMod.Events;
using CalamityMod.Items.Materials;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.Rarities;
using CalamityMod.UI.DialogueDisplay;
using CalamityMod.UI.DialogueDisplay.DisplayEffects;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.SummonItems
{
    public class CosmicWorm : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.SummonItems";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.SortingPriorityBossSpawns[Type] = 19; // Celestial Sigil
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 18;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.consumable = false;
            Item.rare = ModContent.RarityType<Turquoise>();
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.BossItem;
        }

        public override bool CanUseItem(Player player)
        {
            return !NPC.AnyNPCs(ModContent.NPCType<DevourerofGodsHead>()) && !BossRushEvent.BossRushActive;
        }

        public override bool? UseItem(Player player)
        {
            // Server literally do nothing here.
            if (Main.dedServ)
                return true;

            string key = "Mods.CalamityMod.Status.Boss.DoGSpawn";
            string spawnText = Language.GetTextValue(key);
            Color messageColor = Color.Cyan;
            Main.NewText(spawnText, messageColor);
            /*
            var subtitleID = CombatText.NewText(player.Hitbox, messageColor, spawnText, true);
            if (subtitleID < Main.maxCombatText)
            {
                var localPlayer = Main.LocalPlayer.Calamity();
                localPlayer.subtitletext = Main.combatText[subtitleID];
                localPlayer.subtitleColors = [Color.Cyan, Color.Fuchsia];
            }
            */

            NPC n = CalamityUtils.SpawnBossOnPosUsingItem<DevourerofGodsHead>(player, (int)player.Center.X, (int)player.Center.Y - 1600, DevourerofGodsHead.SpawnSound);

            DialogueDisplaySystem.StartDialogue("DevourerOfGodsPhases", n, 0, 120, false, new BossText());
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<ArmoredShell>().
                AddIngredient<TwistingNether>().
                AddIngredient<DarkPlasma>().
                AddTile(TileID.MythrilAnvil).
                Register()
                .DisableDecraft();

            // sequence breaking recipe
            CreateRecipe().
                AddIngredient(ItemID.LunarBar, 40).
                AddIngredient<GalacticaSingularity>(10).
                AddIngredient<Necroplasm>(40).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
