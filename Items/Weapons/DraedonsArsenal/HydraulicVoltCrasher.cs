using System;
using System.Collections.Generic;
using CalamityMod.CustomRecipes;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.DraedonsArsenal;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.DraedonsArsenal
{
    public class HydraulicVoltCrasher : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.DraedonsArsenal";
        // This is the amount of charge consumed every frame the holdout projectile is summoned, i.e. the weapon is in use.
        public const float HoldoutChargeUse = 0.002f;
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        public override void SetDefaults()
        {
            CalamityGlobalItem modItem = Item.Calamity();

            Item.width = 56;
            Item.height = 24;
            Item.damage = 75;
            Item.knockBack = 12f;
            Item.useTime = 4;
            Item.useAnimation = 16;
            Item.hammer = 100;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.shootSpeed = 46f;
            Item.UseSound = SoundID.Item23;

            Item.shoot = ModContent.ProjectileType<HydraulicVoltCrasherProjectile>();
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;

            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.DamageType = TrueMeleeNoSpeedDamageClass.Instance;
            Item.channel = true;

            modItem.UsesCharge = true;
            modItem.MaxCharge = 85f;
            modItem.ChargePerUse = 0f; // This weapon is a holdout. Charge is consumed by the holdout projectile.
        }
        public override bool AltFunctionUse(Player player) => true;

        public override void ModifyTooltips(List<TooltipLine> tooltips) => CalamityGlobalItem.InsertKnowledgeTooltip(tooltips, 2);

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.hammer = 0;
            }
            else
            {
                Item.hammer = 100;
            }
            return player.ownedProjectileCounts[Item.shoot] <= 0 && Item.Calamity().Charge > 0;
        }
        public override void HoldItem(Player player)
        {
            if (Main.myPlayer == player.whoAmI)
                player.Calamity().rightClickListener = true;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<MysteriousCircuitry>(8).
                AddIngredient<DubiousPlating>(12).
                AddRecipeGroup("AnyMythrilBar", 10).
                AddIngredient(ItemID.SoulofSight, 20).
                AddCondition(ArsenalTierGatedRecipe.ConstructRecipeCondition(2, out Func<bool> condition), condition).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
