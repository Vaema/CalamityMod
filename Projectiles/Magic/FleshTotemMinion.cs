using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Accessories;
using CalamityMod.NPCs.BrimstoneElemental;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic
{
    public class FleshTotemMinion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";

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
            bool isActive = Projectile.type == ModContent.ProjectileType<FleshTotemMinion>();
            Player player = Main.player[Projectile.owner];
            CalamityPlayer modPlayer = player.Calamity();
            if (!modPlayer.fleshTotemMinion)
            {
                Projectile.active = false;
                return;
            }
            if (isActive)
            {
                if (player.dead)
                {
                    modPlayer.fleshTotemMinion = false;
                }
                if (modPlayer.fleshTotemMinion)
                {
                    Projectile.timeLeft = 2;
                }
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
                for (int k = 0; k < 30; k++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.IceTorch, new Vector2(9, 9).RotatedByRandom(MathHelper.TwoPi) * Main.rand.NextFloat(0.5f, 0.8f), 0, Color.LightSkyBlue, Main.rand.NextFloat(1.2f, 1.4f));
                    dust.noGravity = true;
                    dust.alpha = Main.rand.Next(70, 90 + 1);
                }
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
        public override bool PreDraw(ref Color lightColor)
        {
            Player player = Main.player[Projectile.owner];
            CalamityPlayer modPlayer = player.Calamity();
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = player.Center - Main.screenPosition + Vector2.UnitY * -67f;
            Vector2 origin = texture.Size() * 0.5f;

            float fade = Utils.GetLerpValue(0, modPlayer.fleshTotemManaStorage, FleshTotem.manaStorageMax, true);
            for (int i = 0; i < 10; i++)
            {
                Main.spriteBatch.Draw(texture, drawPosition + Vector2.UnitY * -1.5f, null, Color.Cyan with { A = 0 } * fade, Projectile.rotation, origin, Projectile.scale * (Main.rand.NextFloat(0.0016f, 0.002f) * modPlayer.fleshTotemManaStorage), SpriteEffects.None, 0f);
            }
            for (int i = 0; i < 1; i++)
            {
                int dustType = Main.rand.NextBool() ? 66 : 247;
                float rotMulti = Main.rand.NextFloat(0.3f, 1f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + Vector2.UnitY / 75f, dustType);
                dust.scale = Main.rand.NextFloat(1.2f, 1.8f) * (modPlayer.fleshTotemManaStorage * 0.0009f) - rotMulti * 0.1f;
                dust.noGravity = true;
                dust.velocity = new Vector2(0, -2).RotatedByRandom(rotMulti * 0.3f) * (Main.rand.NextFloat(1f, 3.2f) - rotMulti) * (modPlayer.fleshTotemManaStorage * 0.0009f);
                dust.alpha = 1;
                dust.color = Color.Cyan;
            }
            Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }
        public override bool? CanDamage() => false;
    }
}
