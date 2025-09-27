using CalamityMod.Buffs.Summon;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    public class FrostbatStaff : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";

        public override void SetDefaults()
        {
            Item.damage = 22;
            Item.DamageType = DamageClass.Summon;
            Item.buffType = ModContent.BuffType<FrostbatBuff>();
            Item.shoot = ModContent.ProjectileType<FrostbatMinion>();
            Item.knockBack = 1f;

            Item.useAnimation = Item.useTime = 15;
            (Item.width, Item.height) = (50, 50);
            Item.mana = 10;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item28;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            var minion = Projectile.NewProjectileDirect(source, player.ClampedMouseWorld(), Vector2.Zero, type, damage, knockback, player.whoAmI);
            minion.originalDamage = Item.damage;
            return false;
        }
    }
}
