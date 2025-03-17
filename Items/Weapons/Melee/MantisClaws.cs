using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using CalamityMod.Items.BaseItems;
using Terraria.ModLoader;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using CalamityMod.Balancing;
using Terraria.Audio;
using CalamityMod.Projectiles.Melee;
using System;
using CalamityMod.Particles;

namespace CalamityMod.Items.Weapons.Melee
{
    public class MantisClaws : CustomUseProjItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            base.SetDefaults();
            Item.width = 26;
            Item.height = 20;
            Item.damage = 111;
            Item.DamageType = DamageClass.Melee;
            Item.useAnimation = Item.useTime = 25;
            Item.knockBack = 0.25f;
            Item.shoot = ModContent.ProjectileType<MantisClawHoldout>();

            Item.useStyle = ItemUseStyleID.HiddenAnimation;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
        }

        public override bool AltFunctionUse(Player player) => true;
    }
}
