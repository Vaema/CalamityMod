using System;
using System.Collections.Generic;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.Items.Accessories;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class Elumphant : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public Player Owner => Main.player[Projectile.owner];
        public static Color color1 = new Color(60, 103, 207);
        public static Color color2 = new Color(103, 188, 214);
        public Color usedColor = Color.White;
        public ref float time => ref Projectile.ai[0];
        public float squashTimerX = 0;
        public float squashTimerY = 0;
        public float fallTimer = 0;
        public int maxFallTimer = 120;
        public int lastHighestFallTimer = 0;
        public ref float attackTimer => ref Projectile.ai[1];

        public float fxFade = 0; // The glow visuals multiplier
        public float followSpeed = 12; // The speed it follows you, lower is faster
        public float lerpDir = 0;
        public float actionSpeed = 1;

        public float verticalSquash = 0;
        public float horizontalSquash = 0;

        public float trunkRotation = 0;
        public bool dashing = false;
        public NPC targeted;
        public bool hitHead = true;
        public int blinkTime = 1;
        public int cryTime = 1;
        public bool mammothFlip = false;
        public bool mammothOops = false;
        public float hopSize = 0;
        public int hopTimer = 0;
        public int attackTime => (int)(FrozenCube.baseAttackSpeed);
        public int cooldownTime => (int)(FrozenCube.baseAttackCooldown / GetPower(0.5f));

        Vector2 goalPosition;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 3;
        }
        public override void SetDefaults()
        {
            Projectile.width = 46;
            Projectile.height = 34;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
        }
        public void SetElumphantPower()
        {
            int usedDefense = (Owner.statDefense / 3);
            Owner.statDefense -= usedDefense;
            Owner.Calamity().frozenCubePower = usedDefense * 0.05f;
        }
        public float GetPower(float efficiency) => 1 + Owner.Calamity().frozenCubePower * efficiency;
        public void GetColor()
        {
            float rate = Main.GlobalTimeWrappedHourly * 5;
            List<Color> eColors = new List<Color>()
            {
                color1,
                color2
            };
            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            usedColor = Color.Lerp(Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f), Color.White, 0.7f);
        }
        public void ManageSquash()
        {
            float resolutionSpeed = 0.02f / Projectile.MaxUpdates;
            verticalSquash = MathHelper.Lerp(verticalSquash, 0, resolutionSpeed);
            horizontalSquash = MathHelper.Lerp(horizontalSquash, 0, resolutionSpeed);
        }
        public void SetDirection(int newDirection)
        {
            if (Projectile.spriteDirection != newDirection)
            {
                trunkRotation *= -1;
            }
            Projectile.spriteDirection = newDirection;
        }
        public void GetTarget() => targeted = Projectile.Center.ClosestNPCAt(500, false);
        public override void AI()
        {
            Projectile.timeLeft++;
            SetElumphantPower();
            GetColor();
            ManageSquash();

            Lighting.AddLight(Projectile.Center, usedColor.ToVector3() * 0.3f);

            if (Owner.controlUseItem)
                Main.NewText(GetPower(1));

            float sine = MathF.Sin(time * 0.04f);
            float sine2 = MathF.Sin(time * 0.08f);
            float sine3 = MathF.Sin(time * 0.1f);
            float fallLerp = Utils.GetLerpValue(0, maxFallTimer, fallTimer, true);

            int hopTimerMax = 16;
            float hop = 1 + (mammothOops ? 1 - MathF.Pow(Utils.GetLerpValue(hopTimerMax, 0, hopTimer, true), 2.5f) : MathF.Pow(Utils.GetLerpValue(0, hopTimerMax, hopTimer, true), 1.0f)) * 1.55f;
            if (hopTimer >= hopTimerMax)
                mammothOops = false;
            else if (mammothOops)
                Projectile.rotation = Projectile.rotation.AngleLerp(0, Utils.GetLerpValue(0, hopTimerMax, hopTimer, true));

            Main.NewText(hopSize);
            Vector2 offsetY = -Vector2.UnitY * (Owner.height / 2 + Projectile.height / 2.5f) * (1 + 0.4f * sine2 * fallLerp + 5 * CalamityUtils.EaseInOutExp(fallLerp, 2f, 2f)) * hop;
            Vector2 offsetX = Vector2.UnitX * ((3 * Projectile.spriteDirection) + 17 * sine3 * fallLerp);
            goalPosition = Owner.MountedCenter + offsetX + offsetY;


            if (dashing)
            {
                Projectile.frame = 2;
                SetDirection(Math.Sign(Projectile.velocity.X));

                Projectile.velocity = (goalPosition - Projectile.Center) / (followSpeed);
            }
            else
            {
                if (fallTimer == 0)
                    SetDirection(Owner.direction);
                Projectile.Center = goalPosition;

                if (targeted == null)
                    GetTarget();
                if (attackTimer > cooldownTime && targeted != null)  // Attack!!!
                {
                    Projectile.frame = 1;
                    if (attackTimer == cooldownTime + 1) // Make start attack sound
                    {
                        SoundStyle attack = new("CalamityMod/Sounds/Item/ElumphantSound");
                        SoundEngine.PlaySound(attack with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, Projectile.Center);
                        Projectile.soundDelay = 0;
                        Projectile.frameCounter = 1;
                    }
                    SetDirection(Math.Sign(Projectile.Center.DirectionTo(targeted.Center).X));
                    horizontalSquash = 0.5f;
                    trunkRotation = Utils.AngleLerp(trunkRotation, Projectile.Center.DirectionTo(targeted.Center).ToRotation() - MathHelper.PiOver2, 0.07f);
                    if (attackTimer == cooldownTime + attackTime)
                        Projectile.frame = 0;

                }
                else // Idle
                {
                    targeted = null;
                    if (Owner.StandingStill()) // If standing still, let the player mess with the mammoth squash for fun
                    {
                        if (Owner.controlDown)
                            horizontalSquash = 0.5f;
                        if (Owner.controlUp)
                            verticalSquash = 0.5f;
                    }
                    if (Math.Abs(Owner.velocity.X) > 4 && fallTimer == 0)
                        verticalSquash = 0.25f * Utils.GetLerpValue(4, 10, Math.Abs(Owner.velocity.X));
                    if (targeted == null && attackTimer > cooldownTime) // If can attack but there is no target, wait to decrease the timer until a target is found
                        attackTimer--;
                    int cryStart = (int)(cryTime - 65);
                    if (Projectile.soundDelay >= cryStart)
                    {
                        if (Projectile.soundDelay == cryStart) // Sound
                        {
                            verticalSquash = 0.3f;
                            SoundStyle cry = new("CalamityMod/Sounds/Item/ElumphantCry");
                            SoundEngine.PlaySound(cry with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, Projectile.Center, _ => new ProjectileAudioTracker(Projectile).IsActiveAndInGame());
                        }
                        trunkRotation = Utils.AngleLerp(trunkRotation, -MathHelper.PiOver2 * 1.1f * Projectile.spriteDirection, 0.025f);
                    }
                    else
                        trunkRotation = Utils.AngleLerp(trunkRotation, 0, 0.11f);
                    if (Projectile.soundDelay >= cryTime)
                    {
                        SetRandCry();
                    }

                    if (Projectile.frameCounter >= blinkTime)
                    {
                        Projectile.frame = 1;
                        Projectile.frameCounter = -6;
                    }
                    if (Projectile.frameCounter == 0)
                    {
                        Projectile.frame = 0;
                        SetRandBlink();
                    }

                    Projectile.frameCounter++;
                    Projectile.soundDelay += 2; // Naturally decreases by 1, so to make it count up, increases it by 2
                }

                if (hopTimer == 0)
                {
                    if (mammothFlip)
                    {
                        Projectile.rotation -= 0.11f * Projectile.spriteDirection * fallLerp;
                    }
                    else
                        Projectile.rotation = Projectile.rotation.AngleLerp(MathHelper.PiOver2 * 0.7f * sine * fallLerp, 0.015f);
                }
            }


            if (Owner.velocity.Y > 8)
            {
                if (fallTimer == 0)
                    mammothFlip = Main.rand.NextBool(3);
                hitHead = false;
                SetRandCry();
                SetRandBlink();
                if (fallTimer < maxFallTimer)
                    fallTimer++;
            }
            else if (fallTimer > 0)
                fallTimer -= 7 - Owner.velocity.Y; if (fallTimer < 0) fallTimer = 0;
            if (fallTimer <= 0 && !hitHead)
            {
                float landPower = (Utils.GetLerpValue(0, maxFallTimer, lastHighestFallTimer, true));
                verticalSquash = 1f * landPower;
                lastHighestFallTimer = 0;
                hitHead = true;
                float rotation = MathHelper.WrapAngle(Projectile.rotation + MathHelper.Pi);
                if (mammothFlip && rotation < MathHelper.PiOver2 && rotation > -MathHelper.PiOver2 && landPower > 0.2f)
                {
                    SoundStyle ahh = new("CalamityMod/Sounds/Item/ElumphantSound");
                    SoundEngine.PlaySound(ahh with { Pitch = Main.rand.NextFloat(0.4f, 0.6f) }, Projectile.Center);
                    verticalSquash = 1.5f * landPower;
                    CombatText.NewText(Projectile.Hitbox, usedColor, "!");
                    mammothOops = true;
                }
                else
                    Projectile.rotation = 0;
                mammothFlip = false;
            }
            if (fallTimer > lastHighestFallTimer)
                lastHighestFallTimer = (int)fallTimer;

            if (attackTimer == attackTime + cooldownTime)
                attackTimer = 0;
            if (!dashing) attackTimer++;
            time++;
            if (mammothOops)
                hopTimer++;
            else if (hopTimer > 0)
                hopTimer--;

            squashTimerX += 0.1f + horizontalSquash;
            squashTimerY += 0.1f + verticalSquash;

            if (!Owner.Calamity().frozenCube)
                Projectile.Kill();
        }
        public void SetRandBlink()
        {
            Projectile.frameCounter = Projectile.frameCounter > 0 ? 1 : 0;
            blinkTime = Main.rand.Next(55, 90 + 1);
        }
        public void SetRandCry()
        {
            Projectile.soundDelay = 0;
            cryTime = Main.rand.Next(420, 600 + 1);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
            Texture2D trunk = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Typeless/ElumphantTrunk").Value;
            Texture2D bTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
            Color drawColor = usedColor;
            Color bodyColor = lightColor;
            Rectangle frame = tex.Value.Frame(1, Main.projFrames[Type], 0, Projectile.frame);


            for (int i = 0; i < 18; i++) // Backglow
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 18f).ToRotationVector2() * 2 * fxFade;
                Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + drawOffset, frame, drawColor with { A = 0 } * 0.2f * fxFade, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            }

            float squashSineV = MathF.Sin(squashTimerY * 0.55f) * verticalSquash;
            float squashSineH = -MathF.Sin(squashTimerX * 0.55f) * horizontalSquash;
            float power = 0.35f;
            float squashX = 1 - power * squashSineV + power * 1.25f * squashSineH;
            float squashY = 1 + power * 1.25f * squashSineV - power * squashSineH;
            Vector2 elumphantSquash = new Vector2(squashX, squashY);

            // Main body
            Vector2 elumphantLocation = new Vector2(Projectile.Center.X, Projectile.Center.Y + ((tex.Height() / 5) * (1 - squashY)));
            Main.EntitySpriteDraw(tex.Value, elumphantLocation - Main.screenPosition, frame, Color.Lerp(bodyColor, drawColor with { A = 0 }, fxFade), Projectile.rotation, frame.Size() * 0.5f, elumphantSquash * Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            float trunkDistX = 10;
            float trunkPosX = (trunkDistX - trunkDistX * (1 - squashX)) * Projectile.spriteDirection;
            Vector2 trunkPos = new Vector2(trunkPosX, 0);
            Main.EntitySpriteDraw(trunk, elumphantLocation - Main.screenPosition + trunkPos.RotatedBy(Projectile.rotation), null, Color.Lerp(bodyColor, drawColor with { A = 0 }, fxFade), Projectile.rotation + trunkRotation, new Vector2(trunk.Width / 2, 0), elumphantSquash * Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);

            return false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {

        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return false;
        }
        public override bool? CanDamage() => false;
    }
}
