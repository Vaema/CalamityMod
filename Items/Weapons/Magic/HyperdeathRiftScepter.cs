using System;
using System.Collections.Generic;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Items.Weapons.Summon;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    [LegacyName("DeathhailStaff")]
    public class HyperdeathRiftScepter : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.FindAndReplace("ff00ff", Utils.Hex3(DevourerofGodsHead.SpecialMoveColor));
        }
        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<VoidEaterMarionette>();
        }
        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 84;
            Item.damage = 4400;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 50;
            Item.useTime = 50;
            Item.useAnimation = 50;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 4f;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.rare = ModContent.RarityType<CosmicPurple>();
            Item.UseSound = SoundID.Item12 with {Volume = 0.75f};
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<HyperdeathRiftScepterBeam>();
            Item.shootSpeed = 18f;
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Item.DrawItemGlowmaskSingleFrame(spriteBatch, rotation, ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Magic/HyperdeathRiftScepterGlow").Value);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, player.Calamity().mouseWorld, Vector2.Zero, type, damage, knockback, player.whoAmI,1);
            return false;
        }
    }
}
