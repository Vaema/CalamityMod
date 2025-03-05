using System.IO;
using CalamityMod.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee.Yoyos
{
    public class RiptideYoyo : ModProjectile
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<Riptide>();

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.YoyosLifeTimeMultiplier[Type] = Riptide.Duration;
            ProjectileID.Sets.YoyosMaximumRange[Type] = Riptide.Reach;
            ProjectileID.Sets.YoyosTopSpeed[Type] = Riptide.Speed;
        }

        public override void SetDefaults()
        {
            Projectile.aiStyle = ProjAIStyleID.Yoyo;
            Projectile.width = Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.alpha = 150;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[1]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[1] = reader.ReadSingle();
        }

        public override void AI()
        {
            if ((Projectile.position - Main.player[Projectile.owner].position).Length() > 3200f) // 200 blocks
                Projectile.Kill();

            Projectile.localAI[1]++;
            if (Projectile.localAI[1] % 32f == 0f)
            {
                SoundEngine.PlaySound(SoundID.Item21, Projectile.position);

                // ai[0] is the yoyo's index, ai[1] is the rotation, ai[2] is the texture
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<RiptideWave>(), Projectile.damage, 10f, Projectile.owner, Projectile.whoAmI, Main.rand.NextFloat(MathHelper.TwoPi), Main.rand.Next(3));
            }
        }
    }
}
