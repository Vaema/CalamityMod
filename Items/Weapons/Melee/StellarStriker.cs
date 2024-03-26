using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    public class StellarStriker : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";

        public override void SetDefaults()
        {
            Item.width = 90;
            Item.height = 100;
            Item.damage = 450;
            Item.DamageType = DamageClass.Melee;
            Item.useAnimation = 25;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 25;
            Item.useTurn = true;
            Item.knockBack = 7.75f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.Rarity10BuyPrice;
            Item.rare = ItemRarityID.Red;
            Item.shootSpeed = 12f;
        }

        // Stellar Striker is classed as a regular melee weapon, so despite being a true melee on-hit, these scale with regular melee.
        private int GetOnHitDamage(Player player) => (int)player.GetTotalDamage<MeleeDamageClass>().ApplyTo(0.5f * Item.damage);

        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (player.whoAmI == Main.myPlayer)
                SpawnFlaresNPC(player, Item.knockBack, GetOnHitDamage(player), target);
        }

        public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo)
        {
            if (player.whoAmI == Main.myPlayer)
                SpawnFlaresPvp(player, Item.knockBack, GetOnHitDamage(player), target);
        }

        private void SpawnFlaresNPC(Player player, float knockback, int damage, NPC target)
        {
            var source = player.GetSource_ItemUse(Item);
            SoundEngine.PlaySound(SoundID.Item88, player.Center);
            int i = Main.myPlayer;
            float cometSpeed = Item.shootSpeed;
            Vector2 realPlayerPos = player.RotatedRelativePoint(player.MountedCenter, true);

            for (int j = 0; j < 2; j++)
            {
                realPlayerPos = new Vector2(player.Center.X + (float)(Main.rand.Next(201) * -(float)player.direction) + ((float)Main.mouseX + Main.screenPosition.X - player.position.X), player.MountedCenter.Y - 600f);
                realPlayerPos.X = (realPlayerPos.X + player.Center.X) / 2f + (float)Main.rand.Next(-200, 201);
                realPlayerPos.Y -= (float)(100 * j);

                Vector2 flareVelocity = CalamityUtils.CalculatePredictiveAimToTargetMaxUpdates(realPlayerPos, target, cometSpeed, 6);

                int proj = Projectile.NewProjectile(source, realPlayerPos, flareVelocity, ProjectileID.LunarFlare, damage, knockback, i, 0f, (float)Main.rand.Next(3));
                if (proj.WithinBounds(Main.maxProjectiles))
                    Main.projectile[proj].DamageType = DamageClass.Melee;
            }
        }

        private void SpawnFlaresPvp(Player player, float knockback, int damage, Player target)
        {
            var source = player.GetSource_ItemUse(Item);
            SoundEngine.PlaySound(SoundID.Item88, player.Center);
            int i = Main.myPlayer;
            float cometSpeed = Item.shootSpeed;
            Vector2 realPlayerPos = player.RotatedRelativePoint(player.MountedCenter, true);

            for (int j = 0; j < 2; j++)
            {
                realPlayerPos = new Vector2(player.Center.X + (float)(Main.rand.Next(201) * -(float)player.direction) + ((float)Main.mouseX + Main.screenPosition.X - player.position.X), player.MountedCenter.Y - 600f);
                realPlayerPos.X = (realPlayerPos.X + player.Center.X) / 2f + (float)Main.rand.Next(-200, 201);
                realPlayerPos.Y -= (float)(100 * j);

                Vector2 flareVelocity = Vector2.Normalize(target.Center - realPlayerPos) * cometSpeed;

                int proj = Projectile.NewProjectile(source, realPlayerPos, flareVelocity, ProjectileID.LunarFlare, damage, knockback, i, 0f, (float)Main.rand.Next(3));
                if (proj.WithinBounds(Main.maxProjectiles))
                    Main.projectile[proj].DamageType = DamageClass.Melee;
            }
        }

        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            if (Main.rand.NextBool(3))
            {
                Dust.NewDust(new Vector2(hitbox.X, hitbox.Y), hitbox.Width, hitbox.Height, DustID.Vortex);
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<CometQuasher>().
                AddIngredient(ItemID.LunarBar, 5).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
