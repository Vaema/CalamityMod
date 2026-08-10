using System;
using System.Collections.Generic;
using CalamityMod.CustomRecipes;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.DraedonsArsenal;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.DraedonsArsenal;

public class Phaseslayer : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.DraedonsArsenal";
    // When below this percentage of charge, the sword is small instead of big.
    public const float SizeChargeThreshold = 0.25f;
    // The small sword barely affects damage on its own because damage is already dropping significantly at low charge.
    public const float SmallDamageMultiplier = 0.9f;

    public override void SetDefaults()
    {
        CalamityGlobalItem modItem = Item.Calamity();

        Item.width = 26;
        Item.height = 26;
        Item.damage = 1350;
        Item.DamageType = DamageClass.MeleeNoSpeed;
        Item.useTime = 24;
        Item.useAnimation = 24;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.useTurn = false;
        Item.knockBack = 7f;

        Item.noMelee = true;
        Item.noUseGraphic = true;

        Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
        Item.rare = ModContent.RarityType<CosmicPurple>();

        Item.UseSound = SoundID.Item1;
        Item.autoReuse = true;

        Item.shoot = ModContent.ProjectileType<PhaseslayerProjectile>();
        Item.channel = true;
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        Projectile blade = Projectile.NewProjectileDirect(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);
        // 14NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
        blade.rotation = blade.AngleTo(Main.MouseWorld);
        blade.netUpdate = true;
        return false;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) => CalamityGlobalItem.InsertKnowledgeTooltip(tooltips, 5);

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<MysteriousCircuitry>(15).
            AddIngredient<DubiousPlating>(25).
            AddIngredient<CosmiliteBar>(8).
            AddIngredient<AscendantSpiritEssence>(2).
            AddCondition(ArsenalTierGatedRecipe.ConstructRecipeCondition(5, out Func<bool> condition), condition).
            AddTile<CosmicAnvil>().
            Register();
    }
}
