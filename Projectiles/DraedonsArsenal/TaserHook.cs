using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using ReLogic.Utilities;
using CalamityMod.Particles;

namespace CalamityMod.Projectiles.DraedonsArsenal
{
    public class TaserHook : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public static readonly SoundStyle Explode = new("CalamityMod/Sounds/Item/ElectricBurst") { Volume = 0.8f };
        public enum TaserAIState
        {
            Firing,
            Electrocuting,
            ReelingBack
        }

        public TaserAIState AIState
        {
            get => (TaserAIState)(int)Projectile.ai[0];
            set => Projectile.ai[0] = (int)value;
        }

        public float Time
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }

        public int ElectrocutionTarget
        {
            get => (int)Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }

        public const float ReelbackSpeed = 40f;
        public Color hookColor = Color.SlateGray;
        public SlotId Hum { get; set; }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = 2;
            Projectile.extraUpdates = 3;
            Projectile.tileCollide = true;
            Projectile.ownerHitCheck = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            if (AIState != TaserAIState.Firing)
                Time++;
            Player player = Main.player[Projectile.owner];
            hookColor = Color.Lerp(hookColor, Color.SlateGray, 0.25f);

            float distanceFromPlayer = Projectile.Distance(player.Center);
            switch (AIState)
            {
                case TaserAIState.Firing:
                    if (distanceFromPlayer > 800f || Time >= 90f)
                        GoToAIState(TaserAIState.ReelingBack);
                    break;
                case TaserAIState.Electrocuting:
                    if (distanceFromPlayer > 800f)
                        GoToAIState(TaserAIState.ReelingBack);

                    if (SoundEngine.TryGetActiveSound(Hum, out var hum) && hum.IsPlaying)
                    {
                        hum.Position = player.Center;
                        hum.Pitch = MathHelper.Lerp(0f, 1f, Utils.GetLerpValue(0f, 150, Time, true));
                    }
                    // electric explosion;
                    if (Time == 150 || !Main.npc[ElectrocutionTarget].active)
                    {
                        SoundEngine.PlaySound(Explode, Projectile.Center);
                        Projectile.localNPCHitCooldown = 15;

                        for (int i = 0; i < 30; i++)
                        {
                            Dust dust = Dust.NewDustPerfect(Projectile.Center, 226, new Vector2(15, 15).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1f));
                            dust.scale = Main.rand.NextFloat(0.6f, 0.9f);
                            Dust dust2 = Dust.NewDustPerfect(Projectile.Center, 66, new Vector2(13, 13).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1f));
                            dust2.scale = Main.rand.NextFloat(0.9f, 1.4f);
                            dust2.noGravity = true;
                            dust2.color = Color.Cyan;
                        }
                        DirectionalPulseRing pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.Cyan * 0.5f, new Vector2(1, 1), 0, 0, 3.5f, 13);
                        GeneralParticleHandler.SpawnParticle(pulse);

                        Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<TaserExplosion>(), Projectile.damage * 2, Projectile.knockBack * 5, Projectile.owner, 0);
                        Time = 0;
                        GoToAIState(TaserAIState.ReelingBack);
                        return;
                    }
                    if (Main.npc[ElectrocutionTarget].active)
                        Projectile.Center = Main.npc[ElectrocutionTarget].Center;
                    break;
                case TaserAIState.ReelingBack:
                    // Kill the gun and the hook if the hook has returned to the gun.
                    if (SoundEngine.TryGetActiveSound(Hum, out var hum2) && hum2.IsPlaying)
                    {
                        hum2?.Stop();
                    }
                    if (Projectile.Hitbox.Intersects(player.Hitbox))
                    {
                        SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/DudFire") with { Volume = 0.5f, Pitch = 0.3f }, Projectile.Center);
                        Projectile.Kill();
                        return;
                    }
                    Projectile.tileCollide = false;
                    Projectile.extraUpdates = 5;
                    Projectile.velocity = Projectile.SafeDirectionTo(player.Center) * (ReelbackSpeed / Projectile.extraUpdates);
                    break;
            }

            Projectile.rotation = Projectile.AngleFrom(player.Center);

            ManipulatePlayerItemValues(player);
        }


        public void ManipulatePlayerItemValues(Player player)
        {
            player.ChangeDir((player.Center.X - Projectile.Center.X < 0).ToDirectionInt());
            player.itemRotation = CalamityUtils.WrapAngle90Degrees(Projectile.rotation);
            player.itemTime = 4;
            player.itemAnimation = 4;
        }

        public void GoToAIState(TaserAIState newAIState)
        {
            // Don't waste the resources changing the AI state if the projectile is already in said state.
            if (AIState == newAIState)
                return;

            Projectile.penetrate = -1;
            AIState = newAIState;
            Projectile.netUpdate = true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Utils.DrawLine(Main.spriteBatch, player.MountedCenter, Projectile.Center, hookColor, hookColor, 3);
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, texture.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.tileCollide = false;
            if (Projectile.localNPCHitCooldown > 7 && target == Main.npc[ElectrocutionTarget])
            Projectile.localNPCHitCooldown -= 1;
            hookColor = Color.Cyan;
            target.AddBuff(BuffID.Electrified, 120);

            if (AIState == TaserAIState.Firing)
            {
                Projectile.Center = target.Center;
                Projectile.extraUpdates = 0;
                if (!Main.dedServ)
                {
                    for (int i = 0; i < 50; i++)
                    {
                        float angle = MathHelper.TwoPi / 50f * i + Utils.GetLerpValue(90f, 150f, Time, true) * MathHelper.ToRadians(1080f);
                        Dust dust = Dust.NewDustPerfect(target.Center + angle.ToRotationVector2() * 10f, 226);
                        dust.velocity = Vector2.Zero;
                        if (Main.rand.NextBool(6))
                            dust.velocity = target.SafeDirectionTo(dust.position) * 4.5f;

                        dust.noGravity = true;
                    }
                }
                ElectrocutionTarget = target.whoAmI;
                Time = 0f;

                SoundStyle charge = new("CalamityMod/Sounds/Item/LowHum");
                Hum = SoundEngine.PlaySound(charge with { Volume = 1.6f, IsLooped = true }, Projectile.Center);

                GoToAIState(TaserAIState.Electrocuting);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            GoToAIState(TaserAIState.ReelingBack);
            return false;
        }
    }
}
