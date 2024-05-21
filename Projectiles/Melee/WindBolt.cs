using System;
using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
<<<<<<< Updated upstream
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
=======
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities.Terraria.Utilities;
using XPT.Core.Audio.MP3Sharp.Decoding.Decoders.LayerIII;
>>>>>>> Stashed changes

namespace CalamityMod.Projectiles.Melee
{
    public class WindBolt : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public int dustvortex = 0;
<<<<<<< Updated upstream
=======
        public float scFactor = 0f;
>>>>>>> Stashed changes
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.alpha = 255;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 2;
<<<<<<< Updated upstream
            Projectile.penetrate = 2;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
=======
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 50;
>>>>>>> Stashed changes
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
<<<<<<< Updated upstream
=======
            Projectile.velocity *= 0.95f;

>>>>>>> Stashed changes
            Projectile.ai[0]++;
            Projectile.ai[1]++;

            //Code so it doesnt collide on tiles instantly
            if (Projectile.ai[0] >= 12)
                Projectile.tileCollide = true;

<<<<<<< Updated upstream
            Projectile.rotation += MathHelper.ToRadians(15f);
            Projectile.alpha -= 5;
            if (Projectile.alpha < 50)
=======
            Projectile.rotation += MathHelper.ToRadians(1f);
            Projectile.alpha -= 5;
>>>>>>> Stashed changes
            {
                Projectile.alpha = 50;
                if (Projectile.ai[1] >= 15)
                {
<<<<<<< Updated upstream

                    for (int i = 1; i <= 6; i++)
                    {
                        Vector2 dustspeed = new Vector2(3f, 3f).RotatedBy(MathHelper.ToRadians(dustvortex));
                        int d = Dust.NewDust(Projectile.Center, Projectile.width / 2, Projectile.height / 2, DustID.Smoke, dustspeed.X, dustspeed.Y, 200, new Color(232, 251, 250, 200), 1.3f);
=======
                    scFactor = MathHelper.Lerp(scFactor, 1f, 0.1f);
                    for (int i = 1; i <= 6; i++)
                    {
                        Vector2 dustpos = new Vector2(48f, 48f).RotatedBy(MathHelper.ToRadians(dustvortex + Main.rand.Next(30)));
                        Vector2 dustspeed = new Vector2(-5f, -5f).RotatedBy(MathHelper.ToRadians(dustvortex + Main.rand.Next(30)));
                        int d = Dust.NewDust(Projectile.Center + dustpos, Projectile.width / 2, Projectile.height / 2, DustID.Smoke, dustspeed.X, dustspeed.Y, 200, new Color(232, 251, 250, 50), 1.3f);
>>>>>>> Stashed changes
                        Main.dust[d].noGravity = true;
                        Main.dust[d].velocity = dustspeed;
                        dustvortex += 60;
                    }
                    dustvortex -= 355;
                    Projectile.ai[1] = 0;
                }
            }
            float projX = Projectile.Center.X;
            float projY = Projectile.Center.Y;
            foreach (var npc in Main.ActiveNPCs)
            {
                if (npc.CanBeChasedBy(Projectile, false) && Collision.CanHit(Projectile.Center, 1, 1, npc.Center, 1, 1) && !CalamityPlayer.areThereAnyDamnBosses)
                {
                    float npcCenterX = npc.position.X + (float)(npc.width / 2);
                    float npcCenterY = npc.position.Y + (float)(npc.height / 2);
                    float npcDistance = Math.Abs(Projectile.position.X + (float)(Projectile.width / 2) - npcCenterX) + Math.Abs(Projectile.position.Y + (float)(Projectile.height / 2) - npcCenterY);
<<<<<<< Updated upstream
                    if (npcDistance < 600f)
                    {
                        if (npc.position.X < projX)
                        {
                            npc.velocity.X += 0.05f;
                        }
                        else
                        {
                            npc.velocity.X -= 0.05f;
                        }
                        if (npc.position.Y < projY)
                        {
                            npc.velocity.Y += 0.05f;
                        }
                        else
                        {
                            npc.velocity.Y -= 0.05f;
                        }
                    }
=======
                    if (npcDistance < 300f)
                    {
                        float factor = MathHelper.Lerp(1f,0f,CalamityUtils.SineBumpEasing(npcDistance / 300, 1));

                        npc.velocity += npc.DirectionTo(Projectile.Center) * factor * 0.25f;
                    }
                    if (npcDistance < 40) npc.velocity *= 0.75f;
>>>>>>> Stashed changes
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
<<<<<<< Updated upstream
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor);
=======
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);

            for (int i = 5; i >= 0; i--)
            {
                float c = Math.Max(i, 1);

                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, tex.Frame(), new Color(1f / c, 1f / c, 1f / c, 1f / c), -Projectile.rotation * c, tex.Size() / 2, (float)MathHelper.Lerp(1f, i, scFactor), SpriteEffects.None);
            }

>>>>>>> Stashed changes
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item60 with { Volume = SoundID.Item60.Volume * 0.6f }, Projectile.Center);

            for (int i = 0; i <= 360; i += 3)
            {
<<<<<<< Updated upstream
                Vector2 dustspeed = new Vector2(3f, 3f).RotatedBy(MathHelper.ToRadians(i));
                int d = Dust.NewDust(Projectile.Center, Projectile.width, Projectile.height, DustID.Smoke, dustspeed.X, dustspeed.Y, 200, new Color(232, 251, 250, 200), 1.4f);
=======
                Vector2 dustspeed = new Vector2(12f, 12f).RotatedBy(MathHelper.ToRadians(i));
                int d = Dust.NewDust(Projectile.Center + (dustspeed * 2), Projectile.width, Projectile.height, DustID.Smoke, dustspeed.X, dustspeed.Y, 200, new Color(232, 251, 250, 200), 1.4f);
>>>>>>> Stashed changes
                Main.dust[d].noGravity = true;
                Main.dust[d].position = Projectile.Center;
                Main.dust[d].velocity = dustspeed;
            }
        }
    }
}
