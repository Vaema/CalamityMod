using CalamityMod.Items.BaseItems;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Ores;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    public class GrandDad : CustomUseProjItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.width = 124;
            Item.height = 124;
            Item.damage = 7777;
            Item.DamageType = DamageClass.Melee;
            Item.useAnimation = 77;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 77;
            Item.useTurn = true;
            Item.knockBack = 7f;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<DarkBlue>();

            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<GrandDadHoldout>();
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Shoot;
        }

        public override bool MeleePrefix() => true;
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<GrandGuardian>().
                AddIngredient<CosmiliteBar>(8).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
