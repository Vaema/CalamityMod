using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using CalamityMod.Utilities.Daybreak;
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
                float maxDist = 57600; //240^2
                NPC target = null;
                foreach (var item in Main.ActiveNPCs)
                {
                    if (item.CanBeChasedBy(item) && item.DistanceSQ(goalPos) < maxDist)
                    {
                        maxDist = item.DistanceSQ(goalPos);
                        target = item;
                    }
                }
                if (target != null)
                {
                    goalPos = target.Center;
                    Projectile.ai[0] = goalPos.X;
                    Projectile.ai[1] = goalPos.Y;
                    Projectile.Calamity().HomingTarget = target.whoAmI;
                } else
                {
                    Projectile.Calamity().HomingTarget = -1;
                }

                    Projectile.velocity += Projectile.DirectionTo(goalPos);
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
                Projectile.Calamity().HomingTarget = -1;
                Projectile.velocity *= 0.98f;
                if (Projectile.velocity.Length() < 1 && Projectile.ai[2] == 1)
                    Projectile.Kill();
                if (Projectile.ai[2] == 2)
                {
                    Projectile.rotation += 0.05f;
                    Projectile.scale = MathHelper.Min(Projectile.scale + 0.02f, 1);
                    Player closestPlayer = null;
                    float closestDis = 320;
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
                if (Main.rand.NextFloat() <= 0.5f) //50% chance to become pick-up-able stars
                {
                    Projectile.ai[2] = 2;
                    Projectile.timeLeft = 600 * Projectile.MaxUpdates;
                }
                else
                    Projectile.ai[2] = 1;
                Projectile.velocity *= Main.rand.NextFloat(0.9f, 1.1f);
                Projectile.netUpdate = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            using (Main.spriteBatch.Scope())
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                CalamityUtils.DrawAfterimagesCentered(Projectile, 1, Color.White, shrink: true);
                Main.spriteBatch.End();
            }
            return false;
        }
    }
}
