using CalamityMod.Items.Accessories;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue;

public class InkBombProjectile : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public static readonly SoundStyle Explode = new("CalamityMod/Sounds/Custom/PlantyMushMine", 3);
    public override void SetDefaults()
    {
        Projectile.width = 14;
        Projectile.height = 22;
        Projectile.friendly = true;
        Projectile.alpha = 0;
        Projectile.penetrate = 1;
        Projectile.tileCollide = true;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = 20;
        Projectile.DamageType = RogueDamageClass.Instance;
    }

    public override void AI()
    {
        Projectile.velocity.Y += 0.1f;
        Projectile.rotation += Projectile.velocity.X * 0.1f;

        if (Projectile.timeLeft == 1)
            CreateInk();
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (!target.friendly)
            CreateInk();
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info) => CreateInk();

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        CreateInk();
        return true;
    }

    private void CreateInk()
    {
        Player player = Main.player[Projectile.owner];
        SoundEngine.PlaySound(SoundID.NPCHit25, Projectile.Center);
        for (int i = 0; i < 4; i++)
        {
            int damage = (int)player.GetTotalDamage<RogueDamageClass>().ApplyTo(InkBomb.InkDamage);

            int inkID = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Main.rand.NextVector2Circular(2f, 2f), ModContent.ProjectileType<InkCloud>(), damage, 7, Projectile.owner, Main.rand.Next(3) + 1);
            Main.projectile[inkID].timeLeft += Main.rand.Next(-15, 15 + 1);
        }
        Projectile.Kill();
    }
}
