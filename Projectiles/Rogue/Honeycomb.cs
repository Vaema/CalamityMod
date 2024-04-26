using CalamityMod.Items.Weapons.Rogue;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class Honeycomb : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/HardenedHoneycomb";

        private const float radius = 15f;

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.aiStyle = ProjAIStyleID.ThrownProjectile;
            Projectile.timeLeft = 300;
            AIType = ProjectileID.ThrowingKnife;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Main.myPlayer];
            if (Projectile.Calamity().stealthStrike)
                player.AddBuff(BuffID.Honey, 600);
            SpawnProjectiles();
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            Player player = Main.player[Main.myPlayer];
            if (Projectile.Calamity().stealthStrike)
                player.AddBuff(BuffID.Honey, 600);
            SpawnProjectiles();
        }

        public void SpawnProjectiles()
        {
            Player player = Main.player[Main.myPlayer];
            int fragAmt = 2;
            for (int i = 0; i < fragAmt; i++)
            {
                //Calculate the velocity of the projectile
                Vector2 shardVelocity = CalamityUtils.RandomVelocity(100f, 35f, 55f);

                //Spawn the projectile
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shardVelocity, ModContent.ProjectileType<HoneycombFragment>(), Projectile.damage, Projectile.knockBack, Projectile.owner, Main.rand.Next(3), 0f);
            }
            if (Projectile.Calamity().stealthStrike)
            {
                int beeAmt = 4;
                for (int j = 0; j < beeAmt; j++)
                {
                    Vector2 beeVelocity = CalamityUtils.RandomVelocity(100f, 35f, 55f);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, beeVelocity, player.beeType(), player.beeDamage(Projectile.damage), player.beeKB(0.25f), player.whoAmI);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, tex.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.NPCDeath1, Projectile.position);
            //Dust on impact
            int dust_splash = 0;
            while (dust_splash < 9)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Copper, -Projectile.velocity.X * 0.15f, -Projectile.velocity.Y * 0.15f, 159, default, 1.5f);
                dust_splash += 1;
            }
        }
    }
}
