using System.Collections.Generic;
using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing.SunkenSeaCatches
{
    public class SparklingEmpress : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Fishing";
        public override void SetDefaults()
        {
            Item.width = 44;
            Item.height = 44;
            Item.damage = 11;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 3;
            Item.useAnimation = Item.useTime = 8;
            Item.knockBack = 0.25f;
            Item.shoot = ModContent.ProjectileType<SparklingEmpressHoldout>();
            Item.shootSpeed = 5f;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.channel = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;

            Item.rare = ItemRarityID.Green;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
        }
        public override void ModifyTooltips(List<TooltipLine> list) => list.FindAndReplace("[GFB]", this.GetLocalizedValue(Main.zenithWorld ? "TooltipGFB" : "TooltipNormal"));
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] == 0;
        public override void HoldItem(Player player) => player.Calamity().mouseRotationListener = true;
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile holdout = Projectile.NewProjectileDirect(source, player.MountedCenter, Vector2.Zero, Item.shoot, damage, knockback, player.whoAmI);
            holdout.velocity = player.Calamity().mouseWorld - player.RotatedRelativePoint(player.MountedCenter);
            return false;
        }
    }
}
