using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items.Fishing.FishingRods;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Sounds;
using CalamityMod.World;
using FullSerializer.Internal;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class GildedGathererHarpoon : BaseCustomUseStyleProjectile, ILocalizedModType, IPixelatedPrimitiveRenderer
    {
        List<Vector2> positions = new List<Vector2>();
        public override Vector2 HitboxSize => new Vector2(16, 16);
        public NPC npcToTarget = null;
        public Vector2 latchOffset;
        public Vector2 mousePos;
        public override Vector2 SpriteOrigin => new(4,4);
        public override int AssignedItemID => ModContent.ItemType<GildedGatherer>();

        public int DamageOverTime => 12;
        public int AirblastDamageMax => 50;
        public int AirblastDamageMin => 10;
        public int DoTDelay => 5;

        public override void SetDefaults()
        {
            base.SetDefaults();
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.penetrate = -1;
            Projectile.height = 6;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Player player = Main.player[Projectile.owner];

            SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact, player.Center);

            latchOffset = Vector2.Zero;

            player.Calamity().mouseWorldListener = true;
            Vector2 mW = player.Calamity().mouseWorld;

            Vector2 v2 = mousePos - player.Center;

            player.direction = Math.Sign(v2.X);

            mousePos = player.Center + v2.RotatedBy(MathHelper.ToRadians(-player.direction * 45));
        }
        public void DamageNPCsInLine(Vector2 pos1, Vector2 pos2, float range)
        {
            float w = range;

            float e = pos1.Distance(pos2) / w;

            for (float i = 0; i < e; i++)
            {
                Vector2 p = Vector2.Lerp(pos1, pos2, i / e);

                if (!Collision.CanHitLine(pos1, 1, 1, p, 1, 1)) break;

                Player player = Main.player[Projectile.owner];

                foreach (NPC npc in Main.npc)
                {
                    if (npc.Distance(p) < w)
                    {
                        if (npc.immune[0] == 0)
                        {
                            Projectile.NewProjectile(new EntitySource_ItemUse(player, player.HeldItem), npc.Center, Vector2.Zero, ModContent.ProjectileType<DirectStrike>(), (int)MathHelper.Lerp((float)AirblastDamageMax, (float)AirblastDamageMin, i / e), 4f, player.whoAmI, npc.whoAmI);

                            npc.immune[0] = 10;
                        }
                    }
                }
            }
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.velocity != Vector2.Zero)
            {
                for (int i = 0; i < 10; i++)
                {
                    GeneralParticleHandler.SpawnParticle(new SparkParticle(Projectile.Center + (oldVelocity * 2), new Vector2(Main.rand.NextFloat(10), 0).RotatedBy(Projectile.rotation + MathHelper.ToRadians(Main.rand.NextFloat(-20, 20)) + MathHelper.ToRadians(180)), false, 12, Main.rand.NextFloat(0.5f, 1f), Main.rand.NextBool(4) ? Color.White : Color.LightSteelBlue, true));
                }

                Player player = Main.player[Projectile.owner];
                SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact, Projectile.Center);
            }
            Projectile.velocity = Vector2.Zero;
            return false;
        }
        public override bool? CanHitNPC(NPC target)
        {
            return base.CanHitNPC(target).Equals(true) && (npcToTarget == null) && (Projectile.velocity != Vector2.Zero);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (npcToTarget == null && Projectile.ai[1] == 1)
            {
                npcToTarget = target;
                latchOffset = (Projectile.DirectionFrom(npcToTarget.Center) * 14);
            }
            base.OnHitNPC(target, hit, damageDone);
        }
        public Vector2 ArmOffset()
        {
            return new Vector2(0, -6f * Main.player[Projectile.owner].direction).RotatedBy(Projectile.rotation);
        }
        public override void UseStyle()
        {
            DrawUnconditionally = true;

            Player player = Main.player[Projectile.owner];

            player.Calamity().mouseWorldListener = true;

            player.direction = 1;
            if (AbsolutePosition == Vector2.Zero) player.direction = (player.Calamity().mouseWorld.X > player.Center.X) ? 1 : -1;
            else player.direction = (Projectile.Center.X > player.Center.X) ? 1 : -1;

            ArmRotationOffset = MathHelper.ToRadians(-90f);

            if (npcToTarget != null)
            {
                if (!npcToTarget.active)
                {
                    Projectile.velocity = new Vector2(6,0).RotatedBy(Projectile.rotation);
                    npcToTarget = null;
                }
            }

            if (!player.controlUseItem && Projectile.ai[1] == 0)
            {
                // bhop air blast!!!

                Vector2 vel = player.velocity * 3;
                if (!Collision.CanHitLine(player.Center, 1, 1, player.Center + vel + (player.DirectionTo(mousePos) * 52), 1, 1))
                {
                    Vector2 velTo = (player.DirectionFrom(mousePos) * 10) * new Vector2(1f, 1.6f);
                    player.velocity += velTo;
                    player.velocity.Y = velTo.Y;
                    player.fallStart = (int)Projectile.Bottom.Y;
                    player.fallStart2 = (int)Projectile.Bottom.Y;
                    Projectile.ai[1] = 3;
                    Projectile.ai[2] = 35f;
                    player.itemAnimation = 30;
                    SoundEngine.PlaySound(SoundID.Item89.WithPitchOffset(0.7f), Projectile.Center);
                    SoundEngine.PlaySound(SoundID.DD2_DrakinShot.WithPitchOffset(0.4f), Projectile.Center);

                    for (int i = 0; i < 6; i++)
                    {
                        GeneralParticleHandler.SpawnParticle(
                            new HeavySmokeParticle(player.Center, player.DirectionTo(mousePos).RotatedBy(Main.rand.NextFloat(-0.1f, 0.1f)) * Main.rand.NextFloat(7, 15), Color.LightSkyBlue, 50, Main.rand.NextFloat(0.4f, 1f), 1f, Main.rand.NextFloat(-0.01f, 0.01f))
                            );
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        GeneralParticleHandler.SpawnParticle(
                            new SparkParticle(player.Center, player.DirectionTo(mousePos).RotatedBy(Main.rand.NextFloat(-0.1f, 0.1f) + MathHelper.ToRadians(90f)) * Main.rand.NextFloat(10, 20), false, 10, 1f, Color.LightSkyBlue)
                            );
                        GeneralParticleHandler.SpawnParticle(
                            new SparkParticle(player.Center, player.DirectionTo(mousePos).RotatedBy(Main.rand.NextFloat(-0.1f, 0.1f) + MathHelper.ToRadians(-90f)) * Main.rand.NextFloat(10, 20), false, 10, 1f, Color.LightSkyBlue)
                            );
                    }
                }
                else
                {
                    // fire harpoon out

                    Vector2 vec = player.Center + new Vector2(15, 0).RotatedBy(Projectile.rotation);

                    for (int i = 0; i < 10; i++)
                    {
                        GeneralParticleHandler.SpawnParticle(new SparkParticle(vec, new Vector2(Main.rand.NextFloat(15), 0).RotatedBy(Projectile.rotation + MathHelper.ToRadians(Main.rand.NextFloat(-20, 20))), false, 12, Main.rand.NextFloat(0.5f, 1f), Main.rand.NextBool(4) ? Color.White : Color.LightSteelBlue, true));
                    }
                    for (int i = 0; i < 10; i++)
                    {
                        GeneralParticleHandler.SpawnParticle(new SmallSmokeParticle(vec, new Vector2(Main.rand.NextFloat(5, 20), 0).RotatedBy(Projectile.rotation + MathHelper.ToRadians(Main.rand.NextFloat(-5, 5))), Color.CornflowerBlue, new Color(100, 100, 170), Main.rand.NextFloat(0.5f, 1f), 150f));
                    }
                    SoundEngine.PlaySound(SoundID.Item99.WithPitchOffset(-0.5f), Projectile.Center);
                    Projectile.ai[1] = 1;
                    Projectile.ai[2] = 0.45f;
                    player.itemAnimation = 3;
                    AbsolutePosition = player.Center;
                    Projectile.velocity = (player.DirectionTo(mousePos) * 20);

                    for (int i = 0; i < 25; i++)
                    {
                        positions.Add(Projectile.Center);
                    }
                }
            }
            if (player.controlUseItem && Projectile.ai[1] == 1 && Projectile.velocity == Vector2.Zero)
            {
                // retract harpoon back to player

                Projectile.ai[1] = 2;
                player.itemAnimation = 3;
                Projectile.ai[2] = 50;

                // air blast here

                float range = 150;

                if (npcToTarget != null)
                {
                    SoundEngine.PlaySound(SoundID.DD2_BookStaffCast.WithPitchOffset(0.5f), Projectile.Center);
                    SoundEngine.PlaySound(SoundID.DD2_DrakinShot.WithPitchOffset(1f), Projectile.Center);

                    Projectile.NewProjectile(new EntitySource_ItemUse(player, player.HeldItem), npcToTarget.Center, Vector2.Zero, ModContent.ProjectileType<DirectStrike>(), Projectile.damage * 2, 4f, player.whoAmI, npcToTarget.whoAmI);

                    DamageNPCsInLine(Projectile.Center, Projectile.Center + new Vector2(range, 0).RotatedBy(Projectile.rotation), 15);

                    for (int i = 0; i < 23; i++)
                    {
                        GeneralParticleHandler.SpawnParticle(new SmallSmokeParticle(Projectile.Center + new Vector2(7, 0).RotatedBy(Projectile.rotation), new Vector2(i, 0).RotatedBy(Projectile.rotation), Color.White, Color.LightBlue, 0.07f * i, 150f));
                    }
                    for (int i = 0; i < 6; i++)
                    {
                        GeneralParticleHandler.SpawnParticle(new SmallSmokeParticle(Projectile.Center + new Vector2(7, 0).RotatedBy(Projectile.rotation), new Vector2(Main.rand.NextFloat(8), 0).RotatedBy(Projectile.rotation + MathHelper.ToRadians(Main.rand.NextFloat(-20f, 20f))), Color.White, Color.LightBlue, Main.rand.NextFloat(1f, 1.5f), 150f));
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        GeneralParticleHandler.SpawnParticle(
                            new SparkParticle(Projectile.Center, player.DirectionTo(mousePos).RotatedBy(Main.rand.NextFloat(-0.1f, 0.1f) + MathHelper.ToRadians(90f)) * Main.rand.NextFloat(10, 20), false, 10, 1f, Color.LightSkyBlue)
                            );
                        GeneralParticleHandler.SpawnParticle(
                            new SparkParticle(Projectile.Center, player.DirectionTo(mousePos).RotatedBy(Main.rand.NextFloat(-0.1f, 0.1f) + MathHelper.ToRadians(-90f)) * Main.rand.NextFloat(10, 20), false, 10, 1f, Color.LightSkyBlue)
                            );
                    }
                }
            }

            switch (Projectile.ai[1])
            {
                case 0:
                    {
                        spriteEffects = mousePos.X < player.Center.X ? SpriteEffects.FlipVertically : SpriteEffects.None;
                        mousePos = player.Calamity().mouseWorld;
                        Projectile.rotation = player.AngleTo(mousePos);
                        Projectile.direction = Math.Sign(mousePos.X - player.Center.X);
                        player.direction = Projectile.direction;
                        player.itemAnimation = 2;
                        Offset = ArmOffset();
                        break;
                    }
                case 1:
                    {
                        Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], 1f, 0.35f);
                        Projectile.tileCollide = true;
                        Projectile.direction = Math.Sign(Projectile.Center.X - player.Center.X);
                        player.direction = Projectile.direction;
                        player.itemAnimation = 2;
                        Offset = Vector2.Zero;

                        if (npcToTarget != null)
                        {
                            Projectile.ai[0]++;
                            if (Projectile.ai[0] % DoTDelay == 0)
                            {
                                Projectile.NewProjectile(new EntitySource_ItemUse(player, player.HeldItem), npcToTarget.Center, Vector2.Zero, ModContent.ProjectileType<DirectStrike>(), DamageOverTime, 4f, player.whoAmI, npcToTarget.whoAmI);
                            }

                            AbsolutePosition = npcToTarget.Center + latchOffset;
                            Projectile.velocity = Vector2.Zero;
                            spriteEffects = Projectile.Center.X > player.Center.X ? SpriteEffects.None : SpriteEffects.FlipVertically;

                            Projectile.rotation = player.AngleTo(Projectile.Center);
                        }
                        else if (Projectile.velocity != Vector2.Zero)
                        {
                            spriteEffects = Projectile.velocity.X > 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;

                            Projectile.velocity.Y += 0.2f;
                            Projectile.velocity.X *= 0.99f;
                            Projectile.rotation = Vector2.Zero.AngleTo(Projectile.velocity);
                        }
                        else
                        {
                            Projectile.rotation = Projectile.AngleFrom(player.Center);
                        }

                        break;
                    }
                case 2:
                    {
                        spriteEffects = Projectile.velocity.X < 0 ? SpriteEffects.None : SpriteEffects.FlipVertically;

                        if (Projectile.ai[2] > 30)
                        {
                            Projectile.ai[2] -= 2;
                            if (Projectile.ai[2] % 4 == 0)
                            {
                                SoundEngine.PlaySound(SoundID.Item73.WithPitchOffset(Projectile.ai[2] / 50), Projectile.Center);
                            }
                        }

                        Projectile.tileCollide = false;
                        Projectile.direction = Math.Sign(Projectile.Center.X - player.Center.X);
                        player.direction = Projectile.direction;
                        Projectile.velocity = Projectile.DirectionTo(player.Center) * Projectile.ai[2];
                        Projectile.rotation = Vector2.Zero.AngleTo(Projectile.velocity) + MathHelper.ToRadians(180f);
                        if (Projectile.Distance(player.Center) > Projectile.ai[2] + 5)
                        {
                            player.itemAnimation = 2;
                        }
                        else
                        {
                            player.itemAnimation = 0;
                            Projectile.Kill();
                            Projectile.ai[1] = 4;
                        }
                        break;
                    }
                case 3:
                    {
                        GeneralParticleHandler.SpawnParticle(new SparkParticle(player.Center - (player.velocity) + new Vector2(Main.rand.NextFloat(5, 25), 0).RotatedByRandom(MathHelper.TwoPi), player.velocity, false, 8, Main.rand.NextFloat(0.2f, 1.2f), Color.White, true));

                        Vector2 v2 = mousePos - player.Center;

                        mousePos = player.Center + v2.RotatedBy(MathHelper.ToRadians(-player.direction * Projectile.ai[2]));
                        Projectile.rotation = player.AngleTo(mousePos);
                        Projectile.ai[2] *= 0.85f;
                        if (Projectile.ai[2] < 2)
                        {
                            player.itemAnimation = 0;
                            Projectile.Kill();
                            Projectile.ai[1] = 4;
                        }
                        break;
                    }
            }

            for (float i = 0; i < positions.Count; i++)
            {
                positions[(int)i] = Vector2.Lerp(positions[(int)i], Vector2.Lerp(AbsolutePosition + new Vector2(0, 6), player.Center + ArmOffset(), (i / (float)positions.Count)),
                    MathHelper.Clamp(CalamityUtils.CircInEasing(i / (float)positions.Count, 1), 0f, 1f)
                    );
                positions[(int)i] = Vector2.Lerp(positions[(int)i], Vector2.Lerp(AbsolutePosition + new Vector2(0, 6), player.Center + ArmOffset(), (i / (float)positions.Count)),
                    MathHelper.Lerp(1f,0f,CalamityUtils.CircOutEasing(i / (float)positions.Count, 1))
                    );
            }

            if (player.itemAnimation <= 0) Projectile.Kill();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
            if (AbsolutePosition == Vector2.Zero)
            {
                Player player = Main.player[Projectile.owner];
                Main.EntitySpriteDraw(tex.Value, Main.player[Projectile.owner].Center + new Vector2(0,player.gfxOffY) + new Vector2(0, -2 * player.direction) + new Vector2(0, -6 * player.direction).RotatedBy(Projectile.rotation) - Main.screenPosition, tex.Frame(), lightColor, Projectile.rotation, new Vector2(12, player.direction < 0 ? 2 : 4), 1f, spriteEffects);
            }
            else
            {
                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, tex.Frame(), lightColor, Projectile.rotation, new Vector2(12, 6), 1f, spriteEffects);
            }
            return false;
        }
        public override void ResetStyle()
        {

        }

        void IPixelatedPrimitiveRenderer.RenderPixelatedPrimitives(SpriteBatch spriteBatch)
        {
            List<Vector2> poss = LinePositions;

            if (poss.Count > 0)
            {
                poss[0] = AbsolutePosition + new Vector2(0, 6);

                if (poss.Count > 1)
                {
                    poss[poss.Count - 1] = Main.player[Projectile.owner].Center + ArmOffset();
                }
                if (poss.Count > 2)
                {
                    poss[poss.Count - 2] = Main.player[Projectile.owner].Center + ArmOffset();
                }
            }

            for (int i = 0; i < poss.Count; i++)
            {
                Vector2 vec2 = poss[i];

                vec2.Y += Main.player[Projectile.owner].gfxOffY;

                poss[i] = vec2;
            }

            PrimitiveRenderer.RenderTrail(poss, new PrimitiveSettings(W => { return 3f; },
                C => { return Lighting.GetColor((poss[Math.Clamp((int)((float)C * (float)poss.Count), 0, poss.Count)] / 16).ToPoint()).MultiplyRGB(new Color(50, 55, 85)); },
                null, false, true));
            PrimitiveRenderer.RenderTrail(poss, new PrimitiveSettings(W => { return 1f; },
                C => { return Lighting.GetColor((poss[Math.Clamp((int)((float)C * (float)poss.Count), 0, poss.Count)] / 16).ToPoint()).MultiplyRGB(new Color(155, 200, 255)); },
                null, false, true));
        }
    }
}
