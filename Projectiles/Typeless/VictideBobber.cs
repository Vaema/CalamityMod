using Microsoft.Xna.Framework;
using CalamityMod.Items;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class VictideBobber : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/Summon/CnidarianJellyfishOnTheString";

        public Player Owner => Main.player[Projectile.owner];
        public ref float ParentProjectile => ref Projectile.ai[2];
        public Projectile Parent => Main.projectile[(int)ParentProjectile];

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.aiStyle = ProjAIStyleID.Bobber;
            Projectile.bobber = true;
        }

        public override bool PreAI()
        {
            // Snap if the snail is shelled, or just nowhere to be found
            if (!Parent.active || Parent == null || Parent.frame < 6)
            {
                Projectile.Kill();
                return false;
            }
            return true;
        }

        public override void AI()
        {
            // Automatic reeling
            if (Projectile.ai[1] < 0f)
            {
                Projectile.ai[0] = 1f;
                Projectile.ai[1] = Projectile.localAI[1];
                Projectile.localAI[0] = 1f;
                Projectile.localAI[1] = 0f;
            }
        }

        // Anchor the bobber to the parent projectile, since the default is bound to the player
        // Yes. This method is obsolete. No, there is no other way and I won't make another IL edit to remedy this fact. - Iris
        public override void ModifyFishingLine(ref Vector2 lineOriginOffset, ref Color lineColor)
        {
            Vector2 originalPos = Owner.MountedCenter + Vector2.UnitY * (Owner.gfxOffY - (Owner.gravDir == -1 ? 12f : 0f));

            if (Parent.active)
                lineOriginOffset = Parent.Center - originalPos + Vector2.UnitY * 12f;

            // How this is a thing is beyond me
            if (Owner.direction == -1)
                lineOriginOffset += Vector2.UnitX * (Parent.Center.X < Owner.Center.X ? 48f : -54f);

            lineColor = Color.Cyan;
        }

        public override bool PreDrawExtras()
        {
            Lighting.AddLight(Projectile.Center, 0f, 0.2f, 0.2f);
            return true;
        }
    }
}
