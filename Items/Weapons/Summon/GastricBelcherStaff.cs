using System;
using CalamityMod.Buffs.Summon;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    public class GastricBelcherStaff : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Whitewater>();
        }
        public override void SetDefaults()
        {
            Item.width = 66;
            Item.height = 70;
            Item.damage = 50;
            Item.mana = 10;
            Item.useAnimation = Item.useTime = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 1f;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.UseSound = SoundID.Item76;
            Item.buffType = ModContent.BuffType<GastricAberrationBuff>();
            Item.shoot = ModContent.ProjectileType<GastricBelcher>();
            Item.DamageType = DamageClass.Summon;
            Item.autoReuse = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            var minion = Projectile.NewProjectileDirect(source, player.ClampedMouseWorld(), Main.rand.NextVector2Circular(5f, 5f), type, damage, knockback, player.whoAmI, 0f, 1f);
            minion.originalDamage = Item.damage;
            return false;
        }
    }
}
