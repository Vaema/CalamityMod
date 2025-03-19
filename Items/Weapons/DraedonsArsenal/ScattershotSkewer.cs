using System;
using System.Collections.Generic;
using CalamityMod.CustomRecipes;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.DraedonsArsenal;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.DraedonsArsenal
{
    [LegacyName("HeavyLaserRifle")]
    public class ScattershotSkewer : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.DraedonsArsenal";

        public override void SetDefaults()
        {
            CalamityGlobalItem modItem = Item.Calamity();

            Item.width = 64;
            Item.height = 36;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 196;
            Item.knockBack = 4f;
            Item.useTime = 45;
            Item.useAnimation = 35;
            Item.autoReuse = true;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.channel = true;
            Item.noUseGraphic = true;

            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();

            Item.shoot = ModContent.ProjectileType<ScattershotSkewerHoldout>();
            Item.shootSpeed = 5f;

            modItem.UsesCharge = true;
            modItem.MaxCharge = 190f;
            modItem.ChargePerUse = 0.001f; // Charge depletion is done in the holdout, but it needs a small fee to prevent usage at 0%
        }
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
        public override void HoldItem(Player player) => player.Calamity().mouseWorldListener = true;
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/DraedonsArsenal/ScattershotSkewerGlow").Value);
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile holdout = Projectile.NewProjectileDirect(source, player.MountedCenter, Vector2.Zero, ModContent.ProjectileType<ScattershotSkewerHoldout>(), damage, knockback, player.whoAmI, 0, 1);
            holdout.velocity = (player.Calamity().mouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero);
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips) => CalamityGlobalItem.InsertKnowledgeTooltip(tooltips, 4);

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<MysteriousCircuitry>(15).
                AddIngredient<DubiousPlating>(15).
                AddIngredient<UelibloomBar>(8).
                AddIngredient(ItemID.LunarBar, 4).
                AddCondition(ArsenalTierGatedRecipe.ConstructRecipeCondition(4, out Func<bool> condition), condition).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
