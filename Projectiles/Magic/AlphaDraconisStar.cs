using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class AlphaDraconisStar : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 16;
            ProjectileID.Sets.TrailingMode[Type] = 1;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.extraUpdates = 4;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10 * Projectile.MaxUpdates;
            Projectile.scale = 0.5f;
            Projectile.tileCollide = false;
            Projectile.timeLeft = Projectile.MaxUpdates * 300;
        }

        public override void AI()
        {
            Vector2 goalPos = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            float velLength = Projectile.velocity.Length();
            if (Projectile.ai[2] == 0)
            {
                Projectile.velocity += Projectile.DirectionTo(goalPos) * 0.15f;
                Projectile.velocity.Normalize();
                if (Projectile.velocity.HasNaNs())
                    Projectile.velocity = Vector2.UnitY;
                Projectile.velocity *= velLength;

                if (Projectile.Center.Y > goalPos.Y + 100)
                    Projectile.ai[2] = 1;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            }
            else
            {
                Projectile.velocity *= 0.98f;
                if (Projectile.velocity.Length() < 1 && Projectile.ai[2] == 1)
                    Projectile.Kill();
                if (Projectile.ai[2] == 2)
                {
                    Projectile.rotation += 0.05f;
                    Projectile.scale = MathHelper.Min(Projectile.scale + 0.02f, 1);
                    Player closestPlayer = null;
                    float closestDis = 200;
                    foreach (var player in Main.ActivePlayers)
                    {
                        var dis = Projectile.Distance(player.Center);
                        if (dis < closestDis)
                        {
                            closestDis = dis;
                            closestPlayer = player;
                        }
                    }
                    if (closestPlayer != null)
                    {
                        Projectile.velocity += Projectile.DirectionTo(closestPlayer.Center);
                        if (Projectile.Distance(closestPlayer.Center) < 32)
                        {
                            closestPlayer.Calamity().StratusStarburst++;
                            Projectile.Kill();
                        }
                    }
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[2] == 0)
            {
                if (Main.rand.NextFloat() <= 0.4f) //40% chance to become pick-up-able stars
                    Projectile.ai[2] = 2;
                else
                    Projectile.ai[2] = 1;
                Projectile.velocity *= Main.rand.NextFloat(0.9f, 1.1f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.SafeBegin(SpriteSortMode.Immediate, BatchSetting.Additive, null, Main.GameViewMatrix.TransformationMatrix, () =>
            {
                CalamityUtils.DrawAfterimagesCentered(Projectile, 1, Color.White, shrink: true);
            });
            return false;
        }
    }
}
