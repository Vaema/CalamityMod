using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Buffs.Summon;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    public class Cosmilamp : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";
        public const int BeamShootRate = 105;

        public const float MaxTargetingDistance = 1360f;

        public const float BeamHomeSpeed = 17f;

        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 60;
            Item.damage = 95;
            Item.mana = 10;
            Item.useAnimation = Item.useTime = 24;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.knockBack = 4f;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.UseSound = SoundID.Item44;
            Item.autoReuse = true;
            Item.buffType = ModContent.BuffType<CosmilampBuff>();
            Item.shoot = ModContent.ProjectileType<CosmilampMinion>();
            Item.DamageType = DamageClass.Summon;
        }

        public override bool CanUseItem(Player player) => player.maxMinions >= 2;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);

            // Reset the timer for all lamps, to re-align the formation.
            foreach (Projectile pro in Main.ActiveProjectiles)
            {
                if (pro.type == type && pro.owner == player.whoAmI)
                {
                    pro.ModProjectile<CosmilampMinion>().Timer = 0f;
                    pro.netUpdate = true;
                }
            }

            int existingLamps = player.ownedProjectileCounts[type];
            var minion = Projectile.NewProjectileDirect(source, player.ClampedMouseWorld(), Vector2.Zero, type, damage, knockback, player.whoAmI);
            minion.originalDamage = Item.damage;
            minion.ai[0] = existingLamps;
            return false;
        }
    }
}
