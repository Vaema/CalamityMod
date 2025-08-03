using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Ores;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class HalleysInferno : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";

        public static readonly SoundStyle ShootSound = new("CalamityMod/Sounds/Item/HalleysInfernoShoot") { Volume = 0.68f };
        public static readonly SoundStyle Hit = new("CalamityMod/Sounds/Item/HalleysInfernoHit") { Volume = 0.75f };
        public override void SetDefaults()
        {
            Item.width = 84;
            Item.height = 34;
            Item.damage = 444;
            Item.knockBack = 5.5f;
            Item.DamageType = DamageClass.Ranged;

            // Burst of 5, one every 5 frames for 25 total. Cooldown of 39 frames.
            Item.useTime = 5;
            Item.useAnimation = 25;
            Item.reuseDelay = 39;
            Item.useLimitPerAnimation = 5;
            Item.autoReuse = true;

            Item.useAmmo = AmmoID.Gel;
            Item.shootSpeed = 12f;
            Item.shoot = ModContent.ProjectileType<HalleysComet>();

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.UseSound = ShootSound;
            Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
            Item.rare = ModContent.RarityType<PureGreen>();
        }

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        // Terraria seems to really dislike high crit values in SetDefaults
        public override void ModifyWeaponCrit(Player player, ref float crit) => crit += 20;

        public override Vector2? HoldoutOffset() => new Vector2(-15, 0);
        public override void HoldItem(Player player)
        {
            player.Calamity().HasStratusItemCooldown = (int)MathHelper.Max(player.Calamity().HasStratusItemCooldown, 600);
        }

        public override bool AltFunctionUse(Player player)
        {
            if (player.Calamity().AvaliableStarburst > 0)
            return true;
            return false;
        }

        public override bool? UseItem(Player player)
        {
            return base.UseItem(player);
        }

        public override void UseItemFrame(Player player)
        {
            if (player.altFunctionUse == 2 && (player.itemAnimation > 4 || player.itemTime > 4))
            {
                player.itemAnimation = 2;
                player.itemTime = 2;
            }
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                if (player.Calamity().StratusStarburst > 0)
                {
                    Projectile.NewProjectile(source,position + velocity*5,velocity*0.75f, ModContent.ProjectileType<HalleysStarburst>(), damage,knockback,player.whoAmI);
                    player.Calamity().StratusStarburst--;
                }
                return false;
            }
            return base.Shoot(player, source, position, velocity, type, damage, knockback);
        }

        public override bool CanConsumeAmmo(Item ammo, Player player) => Main.rand.Next(100) >= 50;

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.ElfMelter).
                AddIngredient<Lumenyl>(6).
                AddIngredient<RuinousSoul>(4).
                AddIngredient<ExodiumCluster>(12).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
