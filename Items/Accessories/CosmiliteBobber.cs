using System.Collections.Generic;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Fishing.AstralCatches;
using CalamityMod.Items.Fishing.BrimstoneCragCatches;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Abyss;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    internal class CosmiliteBobber : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Fishing";
        public override string Texture => "CalamityMod/Projectiles/Typeless/DevourerofCodsBobber";
        public static List<int> FishToEat = new List<int>()
                    {
                        ItemID.Bass,
                        ItemID.AtlanticCod,
                        ItemID.Flounder,
                        ItemID.NeonTetra,
                        ItemID.RedSnapper,
                        ItemID.RockLobster,
                        ItemID.Salmon,
                        ItemID.Shrimp,
                        ItemID.Trout,
                        ItemID.Tuna,
                        ModContent.ItemType<CharredLasher>(),
                        ModContent.ItemType<CragBullhead>(),
                        ModContent.ItemType<ProcyonidPrawn>(),
                        ModContent.ItemType<TwinklingPollox>(),
                        ModContent.ItemType<PlantyMush>()
    };
        public override void SetDefaults()
        {
            Item.width = 9;
            Item.height = 9;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<CosmicPurple>();
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.fishingSkill += 10;
            player.accLavaFishing = true;
            player.Calamity().SelectedFishingMinigame = CalamityPlayer.FishingMinigames.ScoriaBobber;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.FishingBobber).
                AddIngredient<CosmiliteBar>(2).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
