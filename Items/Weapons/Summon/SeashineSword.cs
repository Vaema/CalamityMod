using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    public class SeashineSword : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Summon";

        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 40;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.damage = 55;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Summon;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.knockBack = 1f;
            Item.shootSpeed = 12f;
            Item.mana = 10;
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<SeashineSwordProj>();
        }
        public override void HoldItem(Player player)
        {
            player.Calamity().mouseWorldListener = true;
            if (player.Calamity().mouseRight)
            {
                for (int x = 0; x < Main.maxProjectiles; x++)
                {
                    Projectile projectile = Main.projectile[x];
                    if (projectile.active && projectile.type == Item.shoot && projectile.ai[0] < 5 && projectile.timeLeft < 90000 - 180)
                    {
                        projectile.ai[0] = 5;
                    }
                }
            }
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {

            int pr = Projectile.NewProjectile(source, player.Center, velocity, type, damage, knockback, player.whoAmI);
            if (Main.projectile.IndexInRange(pr))
                Main.projectile[pr].originalDamage = Item.damage;

            float angleMax = MathHelper.ToRadians(360f);
            if (CalamityUtils.CountProjectiles(type) == 1)
                angleMax = 0f;
            float index = 1f;
            if (player.ownedProjectileCounts[Item.shoot] > 30)
            {
                angleMax += MathHelper.ToRadians((player.ownedProjectileCounts[Item.shoot] - 30) * 2.5f);
            }
            angleMax = angleMax > MathHelper.ToRadians(360f) ? MathHelper.ToRadians(360f) : angleMax; // More intuative than using a min function
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.type == type && p.owner == player.whoAmI)
                {
                    p.ai[2] = (index / CalamityUtils.CountProjectiles(type)) * angleMax - angleMax / 2f;
                    p.ai[1] = index;
                    p.netUpdate = true;
                    index++;
                }
            }
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PearlShard>(3).
                AddIngredient<SeaPrism>(7).
                AddIngredient<Navystone>(10).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
