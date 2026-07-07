using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    // Bleak, directionless slaughter.
    // A celebration of the vices that brought Her to this point.
    [LegacyName("NanoblackReaperMelee", "NanoblackReaperRogue")]
    public class NanoblackReaper : RogueWeapon, IHoldShiftTooltipItem
    {
        // Right triangles, and like triangles.
        internal const float PiOver3 = MathHelper.Pi / 3f;
        internal const float TwoPiOver3 = MathHelper.TwoPi / 3f;

        internal static readonly Color NanoblackSlashColor1     = new Color(47, 248, 211); // #2FF8D4
        internal static readonly Color NanoblackSlashColor2     = new Color(15, 15, 15); // #0F0F0F
        internal static readonly Color NanoblackDustColor1      = new Color(52, 239, 184); // #34EFB8
        internal static readonly Color TesselationParticleColor = new Color(79, 240, 168); // 4FF0A8
        internal static readonly Color ZeroPointLineColor       = new Color(24, 191, 160); // #1FBFA0
        internal static readonly Color ZeroPointImpactColor     = new Color(31, 223, 128, 96); // #1FDF80
        internal static readonly Color PiercingStrikeColor      = new Color(36, 252, 212); // #24FCD4
        internal static readonly Color LightspeedCarveColor1    = new Color(68, 242, 242); // #44F2F2
        internal static readonly Color LightspeedCarveColor2    = new Color(66, 219, 173); // #42DBAD

        public bool ShowExtensionIndicator => false;
        public bool HasFlavorTooltip => true;
        public Color? TooltipExtensionColor => new Color(31, 223, 128); // #1FDF80
        public Color? FlavorTooltipColor => TooltipExtensionColor;

        public static float Knockback = 9f;
        public static float Speed = 16f;

        public static int FocusFlurryAttacks = 12;
        public static int PerfectLightspeedCarveFrames = 4;
        public static int ImperfectLightspeedCarveFrames = 8; // This frame window is immediately after the perfect window.
        public static float LightspeedCarveKnockback = 7f;

        public static int ArmorPenetration = 30;
        // Armor pen declared on projectiles will be added to that of the parent projectile or, failing that, item that spawned it.
        public static int ZeroPointArmorPenetration = 120; // Total: 150.
        public static int LightspeedCarveArmorPenetration = 120; // Total: 150.
        public static float TesselationDamageRatio = 0.25f;
        public static float TesselationKnockback = 1.5f;

        public override void SetDefaults()
        {
            Item.width = 78;
            Item.height = 64;
            Item.damage = 315;
            Item.knockBack = Knockback;
            Item.ArmorPenetration = ArmorPenetration;
            Item.useTime = Item.useAnimation = 19;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item18;

            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;

            Item.DamageType = RogueDamageClass.Instance;
            Item.shoot = ModContent.ProjectileType<NanoblackMain>();
            Item.shootSpeed = Speed;
        }

        public override void HoldItem(Player player)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.mouseWorldListener = true;

            // Safeguard because focus flurries are currently cross-weapon.
            if (modPlayer.focusFlurryAttackCount > FocusFlurryAttacks)
                modPlayer.focusFlurryAttackCount = FocusFlurryAttacks;

            // Remainder of behavior should only be called on owning client
            if (player.whoAmI != Main.myPlayer)
                return;

            // Right click behavior: activate Focus Flurry as applicable
            if (modPlayer.mouseRight && modPlayer.StealthStrikeAvailable())
            {
                // "Stealth strikes" with Nanoblack Reaper are Focus Flurries: the next 30 attacks come out very, very quickly.
                modPlayer.ConsumeStealthByAttacking();
                modPlayer.focusFlurryAttackCount = FocusFlurryAttacks;

                SoundStyle flurryActivationSound1 = new("CalamityMod/Sounds/Item/StygianDash");
                SoundStyle flurryActivationSound2 = new("CalamityMod/Sounds/Item/HeliumFlashCoreImpact");
                float sound2Pitch = Main.rand.NextFloat(0.08f, 0.2f);
                SoundEngine.PlaySound(flurryActivationSound1 with { Volume = 1f }, player.Center);
                SoundEngine.PlaySound(flurryActivationSound2 with { Volume = 0.3f, Pitch = sound2Pitch }, player.Center);

                // Spawn a dramatic void slash particle over the player when this is activated
                {
                    Color color = NanoblackSlashColor1;
                    float scale = 0.33f;
                    Vector2 slashDir = (Main.rand.NextBool() ? -1f : 1f) * Vector2.UnitX;
                    Vector2 vel = 0.01f * slashDir.RotatedByRandom(MathHelper.Pi / 8f);

                    // scale of void sparks is arbitrarily multiplied by 0.357f. thanks!
                    float voidScale = scale / 0.357f;
                    Particle blackSpark = new VoidSparkParticle(player.Center, vel, false, 12, voidScale, color, 1f);
                    GeneralParticleHandler.SpawnParticle(blackSpark);

                    float glowScale = scale * 0.333f;
                    Vector2 squashStretch = new(1.3333f, 0.8f);
                    Particle innerSpark = new GlowSparkParticle(player.Center, vel, false, 11, glowScale, color, squashStretch, true, true, 1f);
                    GeneralParticleHandler.SpawnParticle(innerSpark);
                }
            }

            // Negative-edge left click behavior: attempt light-speed carves
            // This will evaluate to true for exactly one frame when the player lets go of the left mouse button.
            bool leftMouseJustReleased = Main.mouseLeft && Main.mouseLeftRelease;
            if (leftMouseJustReleased)
            {
                int scytheID = ModContent.ProjectileType<NanoblackMain>();
                for (int i = 0; i < Main.maxProjectiles; ++i)
                {
                    Projectile p = Main.projectile[i];
                    if (!p.active || p.type != scytheID || p.owner != player.whoAmI)
                        continue;

                    NanoblackMain nr = p.ModProjectile as NanoblackMain;
                    nr.AttemptLightspeedCarve();
                }
            }
        }

        // Nanoblack Reaper's attack speed triples (similar to its classic speed) during a Focus Flurry.
        public override float UseSpeedMultiplier(Player player) => player.Calamity().focusFlurryAttackCount > 0 ? 3f : 1f;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Focus flurries produce a different sound and set the stealth strike flag to change behavior.
            CalamityPlayer modPlayer = player.Calamity();
            bool focusFlurry = modPlayer.focusFlurryAttackCount > 0;
            if (focusFlurry)
            {
                SoundStyle flurryThrowSound = new("CalamityMod/Sounds/Item/DemonSwordSwing2");
                float pitch = Main.rand.NextFloat(-0.24f, -0.12f) + modPlayer.focusFlurryAttackCount * 0.01f;
                SoundEngine.PlaySound(flurryThrowSound with { Volume = 0.2f, Pitch = pitch, MaxInstances = 12 }, player.Center);

                Projectile focusReaper = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
                focusReaper.Calamity().stealthStrike = true;
                --modPlayer.focusFlurryAttackCount;
                return false;
            }

            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<MoltenAmputator>().
                AddIngredient<GhoulishGouger>().
                AddIngredient<ShadowspecBar>(5).
                AddIngredient<EndothermicEnergy>(40).
                AddIngredient<PlagueCellCanister>(20).
                AddIngredient(ItemID.Nanites, 400).
                AddTile<DraedonsForge>().
                Register();
        }

        public static Color RarityColor() => new Color(0.34f, 0.34f + 0.66f * Main.DiscoG / 255f, 0.34f + 0.5f * Main.DiscoG / 255f);
    }
}
