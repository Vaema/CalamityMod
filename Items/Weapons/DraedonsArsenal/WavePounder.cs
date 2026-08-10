using System;
using System.Collections.Generic;
using CalamityMod.CustomRecipes;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Projectiles.DraedonsArsenal;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.DraedonsArsenal;

public class WavePounder : RogueWeapon, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.DraedonsArsenal";
    public override void SetDefaults()
    {
        Item.width = 26;
        Item.height = 44;
        Item.damage = 75;
        Item.DamageType = RogueDamageClass.Instance;
        Item.useAnimation = Item.useTime = 56;
        Item.knockBack = 0.25f;
        Item.shoot = ModContent.ProjectileType<WavePounderProjectile>();
        Item.shootSpeed = 16f;

        Item.UseSound = SoundID.Item1;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.autoReuse = true;
        Item.noMelee = true;
        Item.noUseGraphic = true;

        Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
        Item.rare = ModContent.RarityType<Turquoise>();
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        int proj = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
        if (proj.WithinBounds(Main.maxProjectiles))
            Main.projectile[proj].Calamity().stealthStrike = player.Calamity().StealthStrikeAvailable();
        return false;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) => CalamityGlobalItem.InsertKnowledgeTooltip(tooltips, 4);

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<MysteriousCircuitry>(12).
            AddIngredient<DubiousPlating>(18).
            AddIngredient<UelibloomBar>(8).
            AddIngredient(ItemID.LunarBar, 4).
            AddCondition(ArsenalTierGatedRecipe.ConstructRecipeCondition(4, out Func<bool> condition), condition).
            AddTile(TileID.MythrilAnvil).
            Register();
    }
}
