using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Enemy
{
    public class HorribleHogVomitChunk : ModProjectile, ILocalizedModType
    {
        private static Asset<Texture2D>[] VomitChunkTextures;

        private string[] VomitChunkTexturePaths = new string[]
        {
            "Terraria/Images/Gore_3",
            "Terraria/Images/Gore_154",
            "Terraria/Images/Gore_241",
            "Terraria/Images/Gore_243",
            "Terraria/Images/Gore_246",
            "Terraria/Images/Gore_262",
            "Terraria/Images/Gore_722",
            "Terraria/Images/Gore_1214"
        };

        public ref float TextureVariant => ref Projectile.ai[0];

        public new string LocalizationCategory => "Projectiles.Enemy";

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void Load()
        {
            if (!Main.dedServ)
            {
                VomitChunkTextures = new Asset<Texture2D>[VomitChunkTexturePaths.Length];
                for (int i = 0; i < VomitChunkTexturePaths.Length; i++)
                    VomitChunkTextures[i] = ModContent.Request<Texture2D>(VomitChunkTexturePaths[i]);
            }
        }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.scale = 0f;
            Projectile.hostile = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            TextureVariant = Main.rand.Next(VomitChunkTexturePaths.Length);
            Projectile.netUpdate = true;
        }

        public override void AI()
        {
            Projectile.scale = MathHelper.Lerp(Projectile.scale, 1f, 0.25f);
            Projectile.ExpandHitboxBy(Projectile.scale);

            Projectile.spriteDirection = Projectile.direction;
            Projectile.rotation += Projectile.velocity.X * 0.035f;
            if (Projectile.velocity.Y < 8f)
                Projectile.velocity.Y += 0.125f;
        }

        public override void OnKill(int timeLeft)
        {
            int dustAmt = Main.rand.Next(15, 21);
            for (int i = 0; i < dustAmt; i++)
            {
                Vector2 spawnPosition = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);
                int dustType = Utils.SelectRandom(Main.rand, DustID.ToxicBubble, DustID.GreenBlood, DustID.Blood);
                Dust.NewDust(spawnPosition, 0, 0, dustType, velocity.X, velocity.Y, Scale: Main.rand.NextFloat(1.2f, 1.8f));
            }

            SoundEngine.PlaySound(SoundID.NPCDeath21, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = VomitChunkTextures[(int)TextureVariant].Value;
            Vector2 origin = texture.Size() * 0.5f;
            SpriteEffects spriteEffects = Projectile.spriteDirection < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            if (CalamityClientConfig.Instance.Afterimages)
            {
                for (int i = 0; i < Projectile.oldPos.Length; ++i)
                {
                    Vector2 drawPos = Projectile.oldPos[i] + Projectile.Size * 0.5f - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
                    float interpolant = ((float)(Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
                    Color color = Color.GreenYellow * interpolant;

                    Main.spriteBatch.Draw(texture, drawPos, null, Projectile.GetAlpha(color) * 0.6f, Projectile.rotation, origin, Projectile.scale * interpolant * 1.25f, spriteEffects, 0f);
                }
            }

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, Projectile.GetAlpha(lightColor), Projectile.rotation, origin, Projectile.scale, spriteEffects, 0f);
            return false;
        }
    }
}
