using CalamityMod.CalPlayer.Dashes;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class StatisNinjaBelt : ModItem, ILocalizedModType, IHoldShiftTooltipItem
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 6));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 26;
            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.rare = ItemRarityID.Purple;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.autoJump = true;
            player.jumpSpeedBoost += 1.6f;
            player.noFallDmg = true;
            player.blackBelt = true;
            player.dashType = 0;
            player.Calamity().DashID = StatisNinjaBeltDash.ID;
            player.spikedBoots = 2;
            player.accFlipper = true;
            player.hasMagiluminescence = true;

            player.MountedCenter.ToTileCoordinates();
            DelegateMethods.v3_1 = new Vector3(0.8f, 0.5f, 1f);
            Utils.PlotTileLine(player.Center, player.Center + player.velocity * 6f, 20f, DelegateMethods.CastLightOpen);
            Utils.PlotTileLine(player.Left, player.Right, 20f, DelegateMethods.CastLightOpen);
        }

        public override void AddRecipes()
        {
            // 20FEB2024: Ozzatron: used to have one recipe which was MNG + Frog Gear. This requires 2 Tiger Climbing Gear.
            // There are now two recipes depending on whether you made Frog Gear or Master Ninja Gear.
            CreateRecipe().
                AddIngredient(ItemID.MasterNinjaGear).
                AddIngredient(ItemID.FrogFlipper).
                AddIngredient(ItemID.Magiluminescence).
                AddIngredient<PurifiedGel>(50).
                AddIngredient<Necroplasm>(5).
                AddTile(TileID.LunarCraftingStation).
                Register();

            CreateRecipe().
                AddIngredient(ItemID.Tabi).
                AddIngredient(ItemID.BlackBelt).
                AddIngredient(ItemID.FrogGear).
                AddIngredient(ItemID.Magiluminescence).
                AddIngredient<PurifiedGel>(50).
                AddIngredient<Necroplasm>(5).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
