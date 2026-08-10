using System;
using System.Collections.Generic;
using CalamityMod.CustomRecipes;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Plates;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.PlaceableTurrets;

public class HostileWaterTurret : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override string Texture => "CalamityMod/Items/Placeables/PlaceableTurrets/WaterTurret";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.DraedonStructures.HostileWaterTurret>());

        Item.value = Item.sellPrice(silver: 50);
        Item.rare = ItemRarityID.Orange;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) => CalamityGlobalItem.InsertKnowledgeTooltip(tooltips, 1);
    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<MysteriousCircuitry>(14).
            AddIngredient<DubiousPlating>(20).
            AddIngredient<Navyplate>(10).
            AddCondition(ArsenalTierGatedRecipe.ConstructRecipeCondition(1, out Func<bool> condition), condition).
            AddCondition(Condition.InGraveyard).
            AddTile(TileID.Anvils).
            Register();
    }
}
