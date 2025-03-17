using System;
using System.Collections.Generic;
using CalamityMod.CustomRecipes;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.DraedonsArsenal;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.DraedonsArsenal
{
    public class PulseRifle : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.DraedonsArsenal";
        public static readonly SoundStyle FireSound = new("CalamityMod/Sounds/Item/PulseRifleFire") { Volume = 0.8f };

        private int BaseDamage = 990;
        public int FiringTimer = 30;

        public override void SetDefaults()
        {
            CalamityGlobalItem modItem = Item.Calamity();

            Item.width = 100;
            Item.height = 32;
            Item.DamageType = DamageClass.Ranged;
            Item.damage = BaseDamage;
            Item.useAnimation = Item.useTime = 55;
            Item.knockBack = 0.25f;
            Item.shoot = ModContent.ProjectileType<PulseRifleShot>();
            Item.shootSpeed = 5f;

            Item.UseSound = FireSound;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.noMelee = true;

            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<DarkBlue>();

            modItem.UsesCharge = true;
            modItem.MaxCharge = 250f;
            modItem.ChargePerUse = 0.24f;
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/DraedonsArsenal/PulseRifleGlow").Value);
        }

        public override Vector2? HoldoutOffset() => new Vector2(-15, 0);

        public override void ModifyTooltips(List<TooltipLine> tooltips) => CalamityGlobalItem.InsertKnowledgeTooltip(tooltips, 5);

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<MysteriousCircuitry>(20).
                AddIngredient<DubiousPlating>(20).
                AddIngredient<CosmiliteBar>(8).
                AddIngredient<AscendantSpiritEssence>(2).
                AddCondition(ArsenalTierGatedRecipe.ConstructRecipeCondition(5, out Func<bool> condition), condition).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
