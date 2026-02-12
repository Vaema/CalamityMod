using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class NanoblackLightspeedCarve : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override LocalizedText DisplayName => this.GetLocalization(IsPerfect ? "Perfect" : "Standard");

        internal const float TargetingRange = 600f;

        internal static float HitboxRadius = 60.0f;
        internal static float PlacementRandomness = 12f;

        private static int Lifetime = 24;
        private static int HitboxDuration = 9; // Hits on 0-4-8

        internal bool IsPerfect => Projectile.ai[0] == 1f;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = (int)(2f * HitboxRadius);
            Projectile.friendly = true;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.penetrate = 1; // Actually three hits, but intentionally obfuscates and violates pierce resistance
            Projectile.extraUpdates = 0;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.timeLeft = Lifetime;

            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 4;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == Lifetime)
                FrameOneEffects();
        }

        private void FrameOneEffects()
        {
            // todo: multiplayer test
            if (Main.netMode != NetmodeID.Server)
            {
                int slashVisualID = ModContent.ProjectileType<NanoblackLightspeedCarveSlashVisual>();
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, slashVisualID, 0, 0f, Projectile.owner, IsPerfect ? 1f : 0f);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.timeLeft > Lifetime - HitboxDuration)
                Projectile.penetrate++;

            Main.NewText("bazinga");
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, HitboxRadius, targetHitbox);
    }
}
