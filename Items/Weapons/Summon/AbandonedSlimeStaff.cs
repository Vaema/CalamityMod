using System;
using CalamityMod.Buffs.Summon;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    public class AbandonedSlimeStaff : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        int slimeSlots;
        public override void SetDefaults()
        {
            Item.width = 62;
            Item.height = 62;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.UseSound = SoundID.Item44;

            Item.DamageType = DamageClass.Summon;
            Item.mana = 40;
            Item.damage = 56;
            Item.knockBack = 3f;
            Item.useAnimation = Item.useTime = 36;
            Item.buffType = ModContent.BuffType<AbandonedSlimeBuff>();
            Item.shoot = ModContent.ProjectileType<AstrageldonSummon>();

            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.Calamity().donorItem = true;
        }

        public override void HoldItem(Player player)
        {
            player.jumpSpeedBoost += 0.5f;

            double minionCount = 0;
            foreach (Projectile projectile in Main.ActiveProjectiles)
            {
                if (projectile.owner == player.whoAmI && projectile.minion && projectile.type != ModContent.ProjectileType<AstrageldonSummon>())
                {
                    minionCount += projectile.minionSlots;
                }
            }
            slimeSlots = (int)(player.maxMinions - minionCount);
        }

        public override bool CanUseItem(Player player) => slimeSlots >= 1;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            CalamityUtils.KillShootProjectiles(true, type, player);
            float damageMult = 0.8f + slimeSlots * 0.2f;
            var minion = Projectile.NewProjectileDirect(source, player.ClampedMouseWorld(), Vector2.Zero, type, (int)(damage * damageMult), knockback, player.whoAmI);
            minion.originalDamage = (int)(Item.damage * damageMult);
            minion.minionSlots = slimeSlots;
            return false;
        }
    }
}
