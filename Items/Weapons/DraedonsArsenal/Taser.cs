using System;
using System.Collections.Generic;
using CalamityMod.CustomRecipes;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables;
using CalamityMod.Projectiles.DraedonsArsenal;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.DraedonsArsenal
{
    public class Taser : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.DraedonsArsenal";
        public static readonly SoundStyle Fire = new("CalamityMod/Sounds/Item/TaserLaunch") { Volume = 0.6f };
        public override void SetDefaults()
        {
            CalamityGlobalItem modItem = Item.Calamity();

            Item.width = 42;
            Item.height = 24;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = 20;
            Item.knockBack = 0f;
            Item.useTime = Item.useAnimation = 28;
            Item.autoReuse = true;
            Item.reuseDelay = 15;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = Fire;
            Item.noMelee = true;

            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;

            Item.shoot = ModContent.ProjectileType<TaserHook>();
            Item.shootSpeed = 6f;

            modItem.UsesCharge = true;
            modItem.MaxCharge = 50f;
            modItem.ChargePerUse = 0.05f;
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

        public override Vector2? HoldoutOffset() => new Vector2(5f, 0f);
        public override void ModifyTooltips(List<TooltipLine> tooltips) => CalamityGlobalItem.InsertKnowledgeTooltip(tooltips, 1);
        public override void UseItemFrame(Player player)
        {
            //float rotation = (player.Center - Item.).ToRotation() * player.gravDir + MathHelper.PiOver2;

            //player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rotation);
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<MysteriousCircuitry>(7).
                AddIngredient<DubiousPlating>(5).
                AddIngredient<AerialiteBar>(4).
                AddIngredient<SeaPrism>(7).
                AddCondition(ArsenalTierGatedRecipe.ConstructRecipeCondition(1, out Func<bool> condition), condition).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
