using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    public class Ataraxia : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public bool hitsound = true;
        public override void SetDefaults()
        {
            Item.width = 94;
            Item.height = 92;
            Item.DamageType = DamageClass.Melee;
            Item.damage = 675;
            Item.knockBack = 2.5f;
            Item.useAnimation = 10;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.useTurn = true;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item1;

            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<Violet>();
            Item.Calamity().donorItem = true;

            Item.shoot = ModContent.ProjectileType<AtaraxiaMain>();
            Item.shootSpeed = 10f;
        }

        // Fires one large and two small projectiles which stay together in formation.
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Play the Terra Blade sound upon firing
            SoundEngine.PlaySound(SoundID.Item60, position);

            // Center projectile
            int centerID = ModContent.ProjectileType<AtaraxiaMain>();
            int centerDamage = (damage / 2);
            Projectile.NewProjectile(source, position, velocity, centerID, centerDamage, knockback, player.whoAmI, 0f, 0f);

            // Side projectiles (these deal 75% damage)
            int sideID = ModContent.ProjectileType<AtaraxiaSide>();
            int sideDamage = ((int)(0.75f * centerDamage) / 2);
            Vector2 originalVelocity = velocity;
            velocity.Normalize();
            velocity *= 22f;
            Vector2 rrp = player.RotatedRelativePoint(player.MountedCenter, true);
            Vector2 leftOffset = velocity.RotatedBy(MathHelper.PiOver4, default);
            Vector2 rightOffset = velocity.RotatedBy(-MathHelper.PiOver4, default);
            leftOffset -= 1.4f * velocity;
            rightOffset -= 1.4f * velocity;
            Projectile.NewProjectile(source, new Vector2(rrp.X + leftOffset.X, rrp.Y + leftOffset.Y), originalVelocity, sideID, sideDamage, knockback, player.whoAmI, 0f, 1f);
            Projectile.NewProjectile(source, new Vector2(rrp.X + rightOffset.X, rrp.Y + rightOffset.Y), originalVelocity, sideID, sideDamage, knockback, player.whoAmI, 0f, 2f);
            hitsound = true;
            return false;
        }

        // On-hit, explode for extra damage.
        public override void OnHitNPC(Player player, NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 480);
            OnHitEffects(player, target.Center);
        }
        public override void OnHitPvp(Player player, Player target, Player.HurtInfo hurtInfo)
        {
            target.AddBuff(ModContent.BuffType<Shadowflame>(), 480);
            OnHitEffects(player, target.Center);
        }

        private void OnHitEffects(Player player, Vector2 targetPos)
        {
            if (hitsound)
            {
                SoundStyle fire = new("CalamityMod/Sounds/Item/CursedDaggerThrow");
                SoundEngine.PlaySound(fire with { Volume = 0.5f, Pitch = 0.9f, PitchVariance = 0.2f, MaxInstances = -1 }, player.Center);
                hitsound = false;
            }

            int trueMeleeID = ModContent.ProjectileType<AtaraxiaBoom>();
            int trueMeleeDamage = (int)player.GetTotalDamage<MeleeDamageClass>().ApplyTo(0.7f * Item.damage);
            var source = player.GetSource_ItemUse(Item);
            Projectile.NewProjectile(source, targetPos, Vector2.Zero, trueMeleeID, trueMeleeDamage, Item.knockBack, player.whoAmI, 0.0f, 0.0f);
            
        }

        // Spawn some fancy dust while swinging
        public override void MeleeEffects(Player player, Rectangle hitbox)
        {
            int dustCount = Main.rand.Next(3, 6);
            Vector2 corner = new Vector2(hitbox.X + hitbox.Width / 4, hitbox.Y + hitbox.Height / 4);
            for (int i = 0; i < dustCount; ++i)
            {
                // Pick a random dust to spawn
                int dustID;
                switch (Main.rand.Next(5))
                {
                    case 0:
                    case 1:
                        dustID = 70;
                        break;
                    case 2:
                        dustID = 71;
                        break;
                    default:
                        dustID = 86;
                        break;
                }
                int idx = Dust.NewDust(corner, hitbox.Width / 2, hitbox.Height / 2, dustID);
                Main.dust[idx].noGravity = true;
            }
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.BrokenHeroSword).
                AddIngredient<AuricBar>(5).
                AddIngredient<CosmiliteBar>(8).
                AddIngredient<AscendantSpiritEssence>(2).
                AddIngredient<NightmareFuel>(20).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
