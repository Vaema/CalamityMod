using System;
using CalamityMod.Dusts;
using CalamityMod.Items.BaseItems;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    public class CometQuasher : CustomUseProjItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.width = 74;
            Item.height = 96;
            Item.damage = 52;

            Item.DamageType = TrueMeleeDamageClass.Instance;
            Item.useAnimation = Item.useTime = 33;

            Item.knockBack = 2.75f;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.rare = ItemRarityID.Yellow;

            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<CometQuasherHoldout>();
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Shoot;
        }
        public override bool MeleePrefix() => true;
        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/CometQuasherGlow").Value);
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.MeteoriteBar, 25).
                AddIngredient(ItemID.Ectoplasm, 5).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
