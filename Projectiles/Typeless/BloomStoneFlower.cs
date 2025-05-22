using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class BloomStoneFlower : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/Magic/BeamingBolt";

        public ref float HookIndex => ref Projectile.ai[0];
        public ref float FlowerPart => ref Projectile.ai[1];
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 30;
            Projectile.scale = 1.2f;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
        }

        public override void AI()
        {
            switch (FlowerPart)
            {
                case 0:
                    Projectile hook = Main.projectile[(int)HookIndex];
                    if (!(hook.active && hook.aiStyle == ProjAIStyleID.Hook && hook.ai[0] == 2f))
                    {
                        Projectile.Kill();
                        return;
                    }

                    Projectile.timeLeft = 5;
                    if (Vector2.DistanceSquared(Projectile.Center, Main.player[Projectile.owner].Center) < 1024f)
                    {
                        SoundEngine.PlaySound(SoundID.Item60, Projectile.Center);
                        if (Main.myPlayer == Projectile.owner)
                        {
                            for (int i = 0; i < 5; i++)
                                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2Circular(4f, 4f), Type, 0, 0f, Projectile.owner, 0f, 1f);
                        }
                        Projectile.Kill();
                    }
                    break;
                case 1:
                    if (Projectile.width < 90)
                        Projectile.ExpandHitboxBy(90);
                    Projectile.velocity *= 0.98f;

                    // Visual effect
                    if (Projectile.timeLeft % 10 == 0)
                    {
                        MediumMistParticle pollenCloud = new(Projectile.Center, Main.rand.NextVector2Circular(1f, 1f), Color.Yellow, Color.Gold, 3f, 100f);
                        GeneralParticleHandler.SpawnParticle(pollenCloud);
                    }
                    if (Projectile.timeLeft % 4 == 0)
                    {
                        Dust pollenDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, ModContent.DustType<LightDust>(), newColor: Color.Gold, Scale: 0.6f);
                        pollenDust.noLightEmittence = true;
                        pollenDust.noGravity = true;
                    }

                    // Check for buffing
                    Player owner = Main.player[Projectile.owner];
                    foreach (Player p in Main.ActivePlayers)
                    {
                        if (!(p == owner || (p.team == owner.team && owner.team != 0)))
                            continue;

                        if (Projectile.Hitbox.Intersects(p.Hitbox))
                            p.Calamity().bloomStoneDR = 360;
                    }
                    break;
            }
        }

        public override bool PreDraw(ref Color lightColor) => FlowerPart == 0f;
    }
}
