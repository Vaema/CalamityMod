using CalamityMod.Items.BaseItems;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Rarities;
using System.Collections.Generic;

namespace CalamityMod.Items.Weapons.Melee
{
    public class GrandDad : CustomUseProjItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.width = 124;
            Item.height = 124;
            Item.damage = 2777;
            Item.DamageType = TrueMeleeDamageClass.Instance;
            Item.useAnimation = 77;
            Item.useTime = 77;
            Item.useTurn = true;
            Item.knockBack = 77f;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();

            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<GrandDadHoldout>();
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Shoot;
        }
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/GrandDadGlow").Value);
        }
        public override bool MeleePrefix() => true;

        // Has a GFB tooltip stuffed to the brim with various references
        public override void ModifyTooltips(List<TooltipLine> list)
        {
            list.FindAndReplace("[GFB]", Lang.SupportGlyphs(this.GetLocalizedValue(Main.zenithWorld ? "TooltipGFB" : "TooltipNormal")));
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<MajesticGuard>().
                AddIngredient<TwistingNether>(3).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
