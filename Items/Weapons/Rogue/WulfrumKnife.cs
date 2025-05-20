using CalamityMod.Items.Materials;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class WulfrumKnife : RogueWeapon
    {
        public static readonly SoundStyle Throw3Sound = new("CalamityMod/Sounds/Item/WulfrumKnifeThrowFull") { Volume = 0.7f, PitchVariance = 0.4f };
        public static readonly SoundStyle Throw2Sound = new("CalamityMod/Sounds/Item/WulfrumKnifeThrowTwo") { Volume = 0.7f, PitchVariance = 0.4f };
        public static readonly SoundStyle Throw1Sound = new("CalamityMod/Sounds/Item/WulfrumKnifeThrowSingle") { Volume = 0.7f, PitchVariance = 0.4f };
        public static readonly SoundStyle TileHitSound = new("CalamityMod/Sounds/Item/WulfrumKnifeTileHit", 2) { Volume = 0.7f, PitchVariance = 0.4f, MaxInstances = 3 };

        public int shootCount = 0;
        public bool stealthStrikeStarted = false;

        public override void SetDefaults()
        {
            Item.width = 22;
            Item.height = 38;
            Item.damage = 11;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Swing;
            //Clockwork burst
            Item.useTime = 4;
            Item.useAnimation = 10;
            Item.reuseDelay = 24;
            Item.useLimitPerAnimation = 3;

            Item.knockBack = 1f;
            Item.UseSound = Throw3Sound;
            Item.autoReuse = true;
            Item.value = Item.sellPrice(0, 0, 0, 5);
            Item.rare = ItemRarityID.Blue;
            Item.shoot = ModContent.ProjectileType<WulfrumKnifeProj>();
            Item.shootSpeed = 4f;
            Item.DamageType = RogueDamageClass.Instance;
        }
        //Random spread
        public override void UseAnimation(Player player)
        {
            shootCount = 0;
            stealthStrikeStarted = false;

            Item.UseSound = Throw3Sound;

            if (Item.stack == 2)
                Item.UseSound = Throw2Sound;
            if (Item.stack == 1)
                Item.UseSound = Throw1Sound;
        }

        public override float StealthDamageMultiplier => 1.5f;
        public override bool AdditionalStealthCheck() => stealthStrikeStarted;

        public override void ModifyStatsExtra(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            bool stealthStrike = player.Calamity().StealthStrikeAvailable() || stealthStrikeStarted;
            float spread = stealthStrike ? MathHelper.PiOver4 * 0.04f : MathHelper.PiOver4 * 0.1f;
            float speedBoost = stealthStrike ? 1.25f : 1f;

            velocity = velocity.RotatedByRandom(shootCount / 2f * spread) * speedBoost;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.Calamity().StealthStrikeAvailable() || stealthStrikeStarted)
            {
                stealthStrikeStarted = true;

                int p = Projectile.NewProjectile(source, position, velocity * 1.3f, ModContent.ProjectileType<WulfrumKnifeProj>(), damage, knockback, player.whoAmI);
                Projectile proj = Main.projectile[p];
                if (p.WithinBounds(Main.maxProjectiles))
                {
                    proj.Calamity().stealthStrike = true;
                    proj.penetrate = 2;
                }
                return false;
            }
            return true;
        }


        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<WulfrumMetalScrap>(10).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
