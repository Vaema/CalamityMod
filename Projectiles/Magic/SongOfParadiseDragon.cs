using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Graphics.Primitives;
using CalamityMod.NPCs.TownNPCs;
using CalamityMod.Particles;
using Microsoft.Build.Construction;
using Microsoft.Build.Tasks;
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
using Terraria.Utilities.Terraria.Utilities;

namespace CalamityMod.Projectiles.Magic
{
    public class SongOfParadiseDragon : ModProjectile, ILocalizedModType
    {
        public static SoundStyle CollideSound = new SoundStyle("CalamityMod/Sounds/Item/SongWyvernCollide");
        public override string Texture => "CalamityMod/Projectiles/Magic/Jimmy";
        Vector2 cen;
        NPC excen = null;
        public new string LocalizationCategory => "Projectiles.Magic";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 50;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 3;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                Projectile.oldPos[i] = Projectile.position;
            }
            cen = Main.player[Projectile.owner].Center;

            player.Calamity().mouseWorldListener = true;

            excen = CalamityUtils.ClosestNPCAt(cen, 800, true, true);
        
            if (excen != null)
                Projectile.timeLeft = 150;
            else
                Projectile.extraUpdates = 0;

            Projectile.ai[1] = Projectile.timeLeft;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.scale = 1f;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 3;
            Projectile.extraUpdates = 1;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }
        public override bool? CanDamage()
        {
            bool bb = true;
            if (Projectile.ai[1] > 90 && Projectile.timeLeft < 30) bb = false;

            return bb;
        }
        public override void AI()
        {
            if (Projectile.ai[1] <= 90 || Projectile.timeLeft > 90)
            {
                Projectile.ai[0] = MathHelper.Lerp(Projectile.ai[0], 120, 0.05f);
                Projectile.velocity = Vector2.Lerp(Projectile.Center, cen + new Vector2(Projectile.ai[0] * 2, 0).RotatedBy(MathHelper.ToRadians((Projectile.ai[0] * 6) + (Projectile.ai[2] * 90))), 0.3f) - Projectile.Center;
            }
            else if (Projectile.timeLeft > 30)
            {
                Projectile.ai[0] = MathHelper.Lerp(Projectile.ai[0], 120, -0.05f);
                Projectile.velocity = Vector2.Lerp(Projectile.Center, cen + new Vector2(Projectile.ai[0] * 2, 0).RotatedBy(MathHelper.ToRadians((-Projectile.ai[0] * 6) + (Projectile.ai[2] * 90) - 55f)), 0.3f) - Projectile.Center;
            }
            else if (Projectile.timeLeft == 29)
            {
                Projectile.velocity = Vector2.Zero;
                SoundEngine.PlaySound(CollideSound, Projectile.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath14, Projectile.Center);
                GeneralParticleHandler.SpawnParticle(new BloomParticle(cen, Vector2.Zero, Color.White, 3.5f, 0f, 30));
                for (int i = 0; i < 6; i++)
                {
                    Vector2 vec2 = cen + new Vector2(Main.rand.Next(30, 100), 0).RotatedBy(Main.rand.NextFloat(MathHelper.TwoPi));
                    Color col = Projectile.ai[2] >= 0 ? Color.SkyBlue : Color.Goldenrod;
                    GeneralParticleHandler.SpawnParticle(new GlowSparkParticle(Vector2.Lerp(vec2, cen, 0.5f), vec2.DirectionFrom(cen) * (vec2.Distance(cen) * 0.2f), false, 30, Main.rand.NextFloat(0.02f, 0.1f), col, Vector2.One));
                    GeneralParticleHandler.SpawnParticle(new SparkParticle(vec2, vec2.DirectionFrom(cen) * 10f, false, Main.rand.Next(20, 50), Main.rand.NextFloat(1f, 2f), col));
                }
            }

            if (excen != null)
            {
                cen = Vector2.Lerp(cen, excen.Center, CalamityUtils.CircInEasing(MathHelper.Clamp(Projectile.ai[0] / 40f, 0f, 1f), 1) * 0.2f);
            }
        }
        public override void Kill(int timeLeft)
        {

        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
            if (!Main.getGoodWorld)
            {
                tex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Magic/" + (Projectile.ai[2] > 0 ? "MelodyOfParadiseDragon" : "MelodyOfParadiseDragon_Alt"));
            }

            float Width = CalamityUtils.CircOutEasing(Projectile.ai[1] < 90 ? ((float)Projectile.timeLeft / 60) : 1f, 1) * tex.Height();
            if (Projectile.ai[1] > 90 && Projectile.timeLeft < 30)
            {
                Width *= MathHelper.Lerp(0.01f, 1f, (float)Projectile.timeLeft / 30f);
            }
            Width /= 2;

            List<Vector2> positions = new();

            for (int i = 0; i < 50; i++)
            {
                positions.Add(Projectile.oldPos[i] + (Projectile.Size / 2));
            }

            GameShaders.Misc["CalamityMod:PrimitiveTexture"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/Projectiles/Magic/" + (Projectile.ai[2] > 0 ? "Jimmy" : "Timmy")));
            if (!Main.getGoodWorld)
            {
                GameShaders.Misc["CalamityMod:PrimitiveTexture"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/Projectiles/Magic/" + (Projectile.ai[2] > 0 ? "MelodyOfParadiseDragon" : "MelodyOfParadiseDragon_Alt")));
            }
            GameShaders.Misc["CalamityMod:PrimitiveTexture"].Shader.Parameters["uPrimitiveSize"].SetValue(tex.Width());
            GameShaders.Misc["CalamityMod:PrimitiveTexture"].Shader.Parameters["flipVertically"].SetValue(true);
            PrimitiveRenderer.RenderTrail(positions, new PrimitiveSettings(W => { return Width; }, C => { return Lighting.GetColor((positions[Math.Clamp((int)((float)C * (float)positions.Count), 0, positions.Count)] / 16).ToPoint()); }, O => { return Vector2.Zero; }, true, false, GameShaders.Misc["CalamityMod:PrimitiveTexture"]), 75);

            return false;
        }
    }
}
