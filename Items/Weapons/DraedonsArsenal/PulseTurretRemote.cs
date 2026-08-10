using System;
using System.Collections.Generic;
using CalamityMod.CustomRecipes;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.DraedonsArsenal;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.DraedonsArsenal;

public class PulseTurretRemote : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.DraedonsArsenal";
    public override void SetDefaults()
    {
        CalamityGlobalItem modItem = Item.Calamity();

        Item.width = 28;
        Item.height = 26;
        Item.damage = 150;
        Item.DamageType = DamageClass.Summon;
        Item.sentry = true;
        Item.mana = 10;
        Item.useAnimation = Item.useTime = 30;
        Item.knockBack = 0.25f;
        Item.shoot = ModContent.ProjectileType<PulseTurret>();
        Item.shootSpeed = 1f;

        Item.UseSound = SoundID.Item15;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.autoReuse = true;
        Item.noMelee = true;

        Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
        Item.rare = ItemRarityID.Yellow;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        CalamityUtils.OnlyOneSentry(player, type);
        player.FindSentryRestingSpot(type, out int XPosition, out int YPosition, out int YOffset);
        YOffset -= 15;
        position = new Vector2((float)XPosition, (float)(YPosition - YOffset));
        int turret = Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);
        if (Main.projectile.IndexInRange(turret))
            Main.projectile[turret].originalDamage = Item.damage;

        player.UpdateMaxTurrets();
        return false;
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) => CalamityGlobalItem.InsertKnowledgeTooltip(tooltips, 3);

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<MysteriousCircuitry>(12).
            AddIngredient<DubiousPlating>(18).
            AddIngredient<LifeAlloy>(5).
            AddIngredient<InfectedArmorPlating>(10).
            AddCondition(ArsenalTierGatedRecipe.ConstructRecipeCondition(3, out Func<bool> condition), condition).
            AddTile(TileID.MythrilAnvil).
            Register();
    }
}
