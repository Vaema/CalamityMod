using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.CalPlayer;
using CalamityMod.NPCs.BrimstoneElemental;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class FleshTotemMinion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Items/Accessories/FleshTotem";

        public int pulseTimer = 0;
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 26;
            Projectile.netImportant = true;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 18000;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft *= 5;
            Projectile.DamageType = DamageClass.Magic;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            CalamityPlayer modPlayer = player.Calamity();
            if (!modPlayer.fleshTotem)
            {
                Projectile.active = false;
                return;
            }
            if (player.dead)
            {
                modPlayer.fleshTotem = false;
            }
            if (modPlayer.fleshTotem)
            {
                Projectile.timeLeft = 2;
            }
            Lighting.AddLight(Projectile.Center, 0f, 0.25f, 1.5f);
            Projectile.Center = player.Center + Vector2.UnitY * (player.gfxOffY - 85f);
            if (player.gravDir == -1f)
            {
                Projectile.position.Y += 170f;
                Projectile.rotation = MathHelper.Pi;
            }
            else
            {
                Projectile.rotation = 0f;
            }
            for (int i = 0; i < 2; i++)
            {
                int dustType = Main.rand.NextBool() ? 66 : 247;
                float rotMulti = Main.rand.NextFloat(0.3f, 1f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Vector2.UnitY / 65f, dustType);
                dust.scale = Main.rand.NextFloat(1.2f, 1.8f) * (modPlayer.fleshTotemManaStorage * 0.0009f) - rotMulti * 0.1f;
                dust.noGravity = true;
                dust.velocity = new Vector2(0, -2).RotatedByRandom(rotMulti * 0.3f) * (Main.rand.NextFloat(1f, 3.2f) - rotMulti) * (modPlayer.fleshTotemManaStorage * 0.0009f);
                dust.alpha = 1;
                dust.color = Color.Cyan;
            }
            int pulseMax = 360;
            if (modPlayer.fleshTotemManaStorage == 0)
                pulseMax = 360;
            else
            {
                pulseMax = 360 - (modPlayer.fleshTotemManaStorage / 2);

                if (pulseMax < 60)
                {
                    pulseMax = 60;
                }
            }
            if (pulseTimer >= pulseMax)
            {
                //Main.NewText(pulseMax);
                SoundEngine.PlaySound(BrimstoneElemental.HellfireballSound, Projectile.Center);
                int manaGained = 30;
                player.statMana += manaGained;
                if (Main.myPlayer == player.whoAmI)
                    player.ManaEffect(manaGained);

                if (player.statMana > player.statManaMax2)
                    player.statMana = player.statManaMax2;
                pulseTimer = 0;
            }
            pulseTimer++;
        }
        public override bool? CanDamage() => false;
    }
}
