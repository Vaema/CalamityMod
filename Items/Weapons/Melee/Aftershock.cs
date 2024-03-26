using System;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    public class Aftershock : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";

        internal const float ShootSpeed = 12f;

        public override void SetDefaults()
        {
            Item.width = 54;
            Item.height = 58;
            Item.damage = 65;
            Item.DamageType = DamageClass.Melee;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 9f;
            Item.value = CalamityGlobalItem.Rarity5BuyPrice;
            Item.rare = ItemRarityID.Pink;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.shootSpeed = ShootSpeed;
        }

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ArmorCrunch>(), 300);
            Vector2 destination = target.Center;
            Vector2 position = destination - (Vector2.UnitY * (destination.Y - Main.screenPosition.Y + 80f)) + (Vector2.UnitX * Main.rand.Next(-160, 161));
            Vector2 velocity = (destination - position).SafeNormalize(Vector2.UnitY) * ShootSpeed * Main.rand.NextFloat(0.9f, 1.1f);
            int rockDamage = player.CalcIntDamage<MeleeDamageClass>(Item.damage);
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, velocity, ModContent.ProjectileType<AftershockRock>(), rockDamage, hit.Knockback, player.whoAmI, 0f, (float)Main.rand.Next(10), target.Center.Y);
        }

        public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(ModContent.BuffType<ArmorCrunch>(), 300);
            Vector2 destination = target.Center;
            Vector2 position = destination - (Vector2.UnitY * (destination.Y - Main.screenPosition.Y + 80f)) + (Vector2.UnitX * Main.rand.Next(-160, 161));
            Vector2 velocity = (destination - position).SafeNormalize(Vector2.UnitY) * ShootSpeed * Main.rand.NextFloat(0.9f, 1.1f);
            int rockDamage = player.CalcIntDamage<MeleeDamageClass>(Item.damage);
            Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, velocity, ModContent.ProjectileType<AftershockRock>(), rockDamage, Item.knockBack, player.whoAmI, 0f, (float)Main.rand.Next(10), target.Center.Y);
        }
    }
}
