using CalamityMod.Items.Materials;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue;

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
        Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
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
    }

    public override void HoldItem(Player player)
    {
        if (player.controlUseTile && !player.mouseInterface && !player.ItemAnimationActive)
        {
            if (Main.rand.NextBool(7))
            {
                Particle streak = new ManaDrainStreak(player, Main.rand.NextFloat(0.2f, 0.5f), Main.rand.NextVector2CircularEdge(1f, 1f) * Main.rand.NextFloat(170f, 670f), Main.rand.NextFloat(30f, 44f), Color.GreenYellow, Color.DeepSkyBlue, Main.rand.Next(15, 30));
                GeneralParticleHandler.SpawnParticle(streak);
            }
            foreach (var item in Main.ActiveProjectiles)
            {
                if (item.type == ModContent.ProjectileType<WulfrumKnifeProj>() && item.owner == player.whoAmI && (item.ai[0] > 0 || item.damage == 0))
                {
                    item.ai[0] = -1;
                }
            }
        }
    }
    public override bool AltFunctionUse(Player player) => false;

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
        if (player.altFunctionUse == 2)
        {
            foreach (var item in Main.ActiveProjectiles)
            {
                if (item.type == ModContent.ProjectileType<WulfrumKnifeProj>() && item.owner == player.whoAmI && item.ai[0] > 0)
                {
                    item.ai[0] = -1;
                }
            }
            return false;
        }
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
