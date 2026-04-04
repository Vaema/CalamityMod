using System;
using System.Collections.Generic;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.Items.Accessories;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Ranged;
using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;

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
        public float followSpeed = 7; // The speed it follows you, lower is faster
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
        public bool attackedThisFrame = false;
        public int attackDirection = 1;
        public float mistShootTimer = 0;
        public int attacksDone = 0;
        public bool recoiling = false;
        public float maxTargetingDistance => 500 * GetPower(0.25f);
        public float maxTrunkRot => -MathHelper.PiOver2 * 1.1f;
        public int attackTime => (int)(FrozenCube.baseAttackSpeed);
        public int attackTimeAdjusted => (int)(FrozenCube.baseAttackSpeed / (GetPower(0.5f) + 0.23f * Projectile.numHits));
        public int cooldownTime => (int)(FrozenCube.baseAttackCooldown / GetPower(0.5f));
        public List<NPC> hitNPCs = new List<NPC>();
        public Vector2 lastHitNPCPos;
        public Vector2 lastProjPos;
        public bool spawnJumpDusts = true;

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
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.DamageType = DamageClass.Generic;
        }
        public void SetElumphantPower()
        {
            int usedDefense = (Owner.statDefense / 3);
            if (time == 0)
                CombatText.NewText(Projectile.Hitbox, color2, -usedDefense,false, true);
            Owner.statDefense -= usedDefense;
            Owner.Calamity().frozenCubePower = usedDefense * 0.05f;
            Owner.Calamity().ColdDebuffMultiplier += Owner.Calamity().frozenCubePower;
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
            if (newDirection == 0)
            {
                Projectile.spriteDirection = Owner.direction;
                return;
            }
            if (Projectile.spriteDirection != newDirection)
            {
                trunkRotation *= -1;
                Projectile.rotation *= -1;
            }
            Projectile.spriteDirection = newDirection;
        }
        public void GetTarget(bool excludeHitNPCs) // Target closest NPC to mouse, unless they're too far, then do closest to Elumphant
        {
            if (excludeHitNPCs)
            {
                float distance = maxTargetingDistance * 5;
                NPC chosenTarget = null;
                for (int index = 0; index < Main.npc.Length; index++) // look for a target that isnt one it has already hit in the last two hits.
                {
                    NPC searchedTarget = Main.npc[index];
                    if (searchedTarget.CanBeChasedBy(null, false))
                    {
                        float extraDistance = (searchedTarget.width / 2) + (searchedTarget.height / 2);

                        bool canHit = true;
                        if (extraDistance < distance)
                            canHit = Collision.CanHit(Projectile.Center, 1, 1, Main.npc[index].Center, 1, 1); // blocked by walls

                        if (searchedTarget.HasBuff(ModContent.BuffType<WindChilled>()) && Vector2.Distance(Projectile.Center, searchedTarget.Center) < distance && !hitNPCs.Contains(searchedTarget) && searchedTarget.active && searchedTarget.life > 0 && canHit)
                        {
                            distance = Vector2.Distance(Projectile.Center, searchedTarget.Center);
                            chosenTarget = searchedTarget;
                        }
                    }
                }
                targeted = chosenTarget;
            }
            else
            {
                targeted = (Owner.ClampedMouseWorld()).ClosestNPCAt(maxTargetingDistance, true);
                if (!ValidDistance())
                    targeted = (Projectile.Center).ClosestNPCAt(maxTargetingDistance, true);
                if (!ValidDistance())
                    targeted = null;
            }
        }
        public bool ValidDistance() => (targeted != null && targeted.Distance(Projectile.Center) < maxTargetingDistance);
        public override void AI()
        {
            if (time == 0)
            {
                lastHitNPCPos = lastProjPos = Owner.MountedCenter;
            }
            Projectile.timeLeft++;
            GetColor();
            SetElumphantPower();
            ManageSquash();

            if (targeted != null && (targeted.life <= 0 || !targeted.active || !targeted.CanBeChasedBy(Projectile)))
                targeted = null;

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
            else if (hopTimer > 0)
                trunkRotation = MathHelper.WrapAngle(MathHelper.Lerp(0, maxTrunkRot * Projectile.spriteDirection - Projectile.rotation, MathF.Pow(Utils.GetLerpValue(0, hopTimerMax / 2, hopTimer, true), mammothOops ? 1f : 1f)));

            Vector2 offsetY = -Vector2.UnitY * (Owner.height / 2 + Projectile.height / 2.5f * Projectile.scale) * (1 + 0.4f * sine2 * fallLerp + 5 * CalamityUtils.EaseInOutExp(fallLerp, 2f, 2f)) * hop;
            Vector2 offsetX = Vector2.UnitX * ((3 * Projectile.spriteDirection) + 17 * sine3 * fallLerp) * Projectile.scale;
            goalPosition = Owner.MountedCenter + offsetX + offsetY;
            if (dashing)
            {
                if (attackTimer == 0)
                {
                    GetTarget(true);
                }
                if (attacksDone != 0)
                    Projectile.frame = 2;

                float attackLerp = Utils.GetLerpValue((int)(attackTimeAdjusted / 2), attackTimeAdjusted, attackTimer, true);
                int direction = Owner.ItemAnimationActive ? (Math.Sign(Projectile.Center.DirectionTo(Owner.ClampedMouseWorld()).X)) : (Owner.direction);
                fxFade = attackLerp;
                if (targeted == null || (targeted != null && (targeted.life <= 0 || !targeted.active)))
                {
                    if (attackLerp > 0 && !recoiling)
                    {
                        lastHitNPCPos = Projectile.Center;
                        attackTimer = attackTimeAdjusted;
                    }
                    recoiling = true;
                    targeted = null;
                }

                Vector2 ownerPos = (Owner.MountedCenter + new Vector2((3 * Projectile.spriteDirection), -(Owner.height / 2 + Projectile.height / 2.5f * Projectile.scale)));
                Vector2 basePosition = recoiling ? ownerPos : Projectile.numHits == 0 ? ownerPos : (lastHitNPCPos);
                Vector2 targetPos = recoiling ? lastHitNPCPos : targeted.Center - (Vector2.UnitY * targeted.height / 2);

                float jumpHeight = Math.Max(250 - Projectile.numHits * 40, recoiling ? 250 : 60);
                float jumpLerp = attackLerp > 0.5f ? 1 - (MathF.Pow(Utils.GetLerpValue(0.5f, 1f, attackLerp), 4)) : 1 - (MathF.Pow(Utils.GetLerpValue(0.5f, 0f, attackLerp), 2));
                float jumpOffsetY = MathHelper.Lerp(basePosition.Y, targetPos.Y, attackLerp) - (jumpHeight + Math.Max(targeted != null ? (Owner.Center.Y - targeted.Center.Y) : 0, 0)) * jumpLerp;
                float jumpOffsetX = MathHelper.Lerp(basePosition.X, targetPos.X, attackLerp);
                goalPosition = new Vector2(jumpOffsetX, jumpOffsetY);

                Projectile.Center = goalPosition;

                if (attackLerp > 0f && spawnJumpDusts)
                {
                    bool clrChoose = Main.rand.NextBool();
                    int halfDusts = 14;
                    for (int i = -halfDusts; i <= halfDusts; i++)
                    {
                        Vector2 dustVel = -Vector2.UnitY.RotatedByRandom(0.9f) * Main.rand.NextFloat(0.6f, 1.8f);
                        Vector2 dustPos = Owner.MountedCenter + Vector2.UnitY * -(Owner.height / 2 + Projectile.height / 2.5f * Projectile.scale) + dustVel * 1.5f;
                        Dust dust2 = Dust.NewDustPerfect(dustPos, ModContent.DustType<SquashDustPixelated>(),
                            dustVel.RotatedByRandom(0.55f) * (Math.Abs(i) * 0.2f) * (Main.rand.NextBool(5) ? 3 : 1), 0, default, Main.rand.NextFloat(0.2f, 0.45f) * 2.5f);
                        dust2.noGravity = true;
                        dust2.color = Main.rand.NextBool() ? color1 : color2;
                        dust2.customData = new Vector2(0.6f, 1.5f);
                        dust2.fadeIn = -0.7f;

                        if (i == -1)
                            i = 1;
                    }
                    spawnJumpDusts = false;
                }

                if (attackLerp > 0.6f && !recoiling)
                {
                    Vector2 vel = lastProjPos.DirectionTo(Projectile.Center);
                    bool clrChoose = Main.rand.NextBool();
                    float opacity = 0.3f * attackLerp;
                    Particle smoke = new CustomColorChangeSpark(Projectile.Center - vel * 18, vel * Main.rand.NextFloat(0.3f, 0.45f), "CalamityMod/Particles/BloomCircle", false, Main.rand.Next(12, 17), (Main.rand.NextFloat(0.35f, 0.45f) + 0.35f * attackLerp) * Projectile.scale, (clrChoose ? color1 : color2) with { A = 0 } * opacity, (clrChoose ? color2 : color1) with { A = 0 } * opacity, new Vector2(0.65f, 1f), false, shrinkSpeed: 0.1f);
                    GeneralParticleHandler.SpawnParticle(smoke, true, Enums.GeneralDrawLayer.BeforeProjectiles);
                }

                float goalAngle = (attackLerp > 0.5f ? basePosition.DirectionTo(Projectile.Center) : Projectile.Center.DirectionTo(targetPos)).ToRotation() + (attackLerp > 0.5f ? MathHelper.PiOver2 : -MathHelper.PiOver2);
                if (!recoiling)
                    Projectile.rotation = MathHelper.WrapAngle(Utils.Remap(attackLerp, 0, 1, -MathHelper.PiOver2 * Projectile.spriteDirection, MathHelper.PiOver2 * Projectile.spriteDirection)) * (1 - MathF.Pow(1 - attackLerp, 3));
                else
                    Projectile.rotation = MathHelper.WrapAngle(Utils.Remap(attackLerp, 0, 1, MathHelper.TwoPi * 2 * Projectile.spriteDirection, 0));
                trunkRotation = trunkRotation.AngleLerp(Projectile.rotation, recoiling ? 1 : 0.15f);

                int attemptDir = Math.Sign((recoiling ? basePosition.X : targetPos.X) - Projectile.Center.X);
                SetDirection(attemptDir);

                if (attackTimer <= (int)(attackTimeAdjusted / 2) && recoiling)
                {
                    Projectile.frame = 0;
                    hitNPCs.Clear();
                    Projectile.velocity = Vector2.Zero;
                    recoiling = false;
                    dashing = false;
                    Projectile.numHits = 0;
                    attackTimer = (int)(cooldownTime / 2);
                    attacksDone = 0;
                    spawnJumpDusts = true;
                    for (int i = 0; i < Main.maxNPCs; i++)
                        Projectile.localNPCImmunity[i] = 0;
                    GetTarget(true);
                }
            }
            else
            {
                int direction = Owner.ItemAnimationActive ? (Math.Sign(Projectile.Center.DirectionTo(Owner.ClampedMouseWorld()).X)) : Math.Sign(Owner.velocity.X);
                if (fallTimer == 0)
                    SetDirection(direction);
                Projectile.Center = goalPosition;
                if (targeted == null)
                    GetTarget(false);
                if (attackTimer == cooldownTime && !ValidDistance())
                    attackTimer--;
                if (attackTimer > cooldownTime && ValidDistance()) // Attack!!!
                {
                    Projectile.frame = 1;
                    float angleSweep = MathF.Sin(attackTimer * 0.085f) * attackDirection;
                    float goalAngle = Projectile.Center.DirectionTo(targeted.Center).ToRotation() - MathHelper.PiOver2;
                    if (attackTimer == cooldownTime + 1) // Make start attack sound
                    {
                        SoundStyle attack = new("CalamityMod/Sounds/Item/ElumphantSound");
                        SoundEngine.PlaySound(attack with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, Projectile.Center);

                        Projectile.soundDelay = 0;
                        Projectile.frameCounter = 1;
                    }
                    SetDirection(Math.Sign(Projectile.Center.DirectionTo(targeted.Center).X));
                    horizontalSquash = 0.5f;
                    Projectile.rotation = Projectile.rotation.AngleLerp((goalAngle + MathHelper.PiOver2 * Projectile.spriteDirection), 0.07f) * MathF.Pow(fallLerp, 0.2f);
                    float angleMax = MathHelper.PiOver4 * 0.75f * Utils.GetLerpValue(maxTargetingDistance, 30, Projectile.Center.Distance(targeted.Center), true);
                    if (hopTimer == 0)
                    trunkRotation = trunkRotation.AngleLerp(goalAngle - Projectile.rotation - angleMax * angleSweep, 0.23f);

                    if (attackTimer > (cooldownTime * 1.1f))
                    {
                        if (mistShootTimer == 0 && Main.myPlayer == Projectile.owner)
                        {
                            Vector2 shootVel = (trunkRotation + Projectile.rotation + MathHelper.PiOver2).ToRotationVector2();
                            int damage = (int)Owner.GetTotalDamage<GenericDamageClass>().ApplyTo(FrozenCube.mistBaseDamage * GetPower(4));
                            int projectile = ModContent.ProjectileType<ElumphantMist>();
                            Vector2 shootPosition = Projectile.Center + shootVel * 5 * Projectile.scale;
                            Projectile mist = Projectile.NewProjectileDirect(Owner.GetSource_FromThis(), shootPosition, (shootVel * shootPosition.Distance(targeted.Center) / 25) * Main.rand.NextFloat(0.9f, 1.1f), projectile, damage, 0, Owner.whoAmI, 0, Owner.ownedProjectileCounts[projectile] % 3, GetPower(1));
                        }
                        mistShootTimer += 0.35f * GetPower(0.25f);
                        if (mistShootTimer >= 2f)
                            mistShootTimer = 0;
                    }
                    mammothFlip = false;

                    attackedThisFrame = true;
                    if (attackTimer == cooldownTime + attackTime)
                    {
                        attackDirection *= -1;
                        Projectile.frame = 0;
                        attacksDone++;
                        if (attacksDone >= 2) // Every 2 attacks, do a dash slam
                        {
                            dashing = true;
                            attackTimer = -1;
                        }
                    }
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
                            SoundEngine.PlaySound(cry with { Pitch = Main.rand.NextFloat(-0.2f, 0.2f) }, Projectile.Center);
                        }
                        trunkRotation = Utils.AngleLerp(trunkRotation, maxTrunkRot * Projectile.spriteDirection, 0.025f);
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

                if (hopTimer == 0 && !attackedThisFrame)
                {
                    if (mammothFlip)
                    {
                        Projectile.rotation = MathHelper.WrapAngle(Projectile.rotation - 0.11f * Projectile.spriteDirection * fallLerp);
                    }
                    else
                        Projectile.rotation = Projectile.rotation.AngleLerp(MathHelper.PiOver2 * 0.7f * sine * fallLerp, 0.015f);
                }
            }


            if (Owner.velocity.Y > 8)
            {
                if (fallTimer == 0)
                    mammothFlip = true;//Main.rand.NextBool(3);
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
                    SoundStyle bonk = new("CalamityMod/Sounds/Item/Bonk");
                    SoundEngine.PlaySound(bonk with { Pitch = Main.rand.NextFloat(0.1f, 0.2f) }, Projectile.Center);

                    int halfDusts = 7;
                    Owner.SetScreenshake(3);
                    Projectile.frame = 1;
                    SetRandBlink();
                    for (int i = -halfDusts; i <= halfDusts; i++)
                    {
                        Vector2 dustVel = Vector2.UnitX.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.6f, 1.8f);
                        Vector2 dustPos = Owner.MountedCenter - (Vector2.UnitY * Owner.height / 2) + dustVel * 1.5f;
                        Dust dust2 = Dust.NewDustPerfect(dustPos, ModContent.DustType<SquashDustPixelated>(),
                            dustVel * (i * 0.4f), 0, default, Main.rand.NextFloat(0.2f, 0.45f) * 3);
                        dust2.noGravity = true;
                        dust2.color = Main.rand.NextBool() ? color1 : color2;
                        dust2.customData = new Vector2(0.6f, 1.5f);
                        dust2.fadeIn = -0.4f;
                        if (i == -1)
                            i = 1;
                    }

                    trunkRotation = maxTrunkRot * Projectile.spriteDirection;
                    SoundStyle ahh = new("CalamityMod/Sounds/Item/ElumphantSound");
                    SoundEngine.PlaySound(ahh with { Pitch = Main.rand.NextFloat(0.4f, 0.6f), volume = 0.6f }, Projectile.Center);
                    CombatText.NewText(Projectile.Hitbox, usedColor, "!");

                    verticalSquash = 1.5f * landPower;
                    mammothOops = true;
                }
                else
                {
                    Projectile.rotation = 0;
                }
                mammothFlip = false;
            }

            if (fxFade > 0)
                Lighting.AddLight(Projectile.Center, usedColor.ToVector3() * 0.6f * fxFade);

            if (fallTimer > lastHighestFallTimer)
                lastHighestFallTimer = (int)fallTimer;

            if (attackTimer >= attackTime + cooldownTime)
                attackTimer = 0;

            attackTimer += recoiling ? -0.75f : 1;
            time++;
            if (mammothOops)
                hopTimer++;
            else if (hopTimer > 0)
                hopTimer--;

            squashTimerX += 0.1f + horizontalSquash;
            squashTimerY += 0.1f + verticalSquash;

            attackedThisFrame = false;

            lastProjPos = Projectile.Center;

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

            Color bodyColor = lightColor;
            Rectangle frame = tex.Value.Frame(1, Main.projFrames[Type], 0, Projectile.frame);

            float squashSineV = MathF.Sin(squashTimerY * 0.55f) * verticalSquash;
            float squashSineH = -MathF.Sin(squashTimerX * 0.55f) * horizontalSquash;
            float power = 0.35f;
            float squashX = 1 - power * squashSineV + power * 1.25f * squashSineH;
            float squashY = 1 + power * 1.25f * squashSineV - power * squashSineH;
            Vector2 elumphantSquash = new Vector2(squashX, squashY);
            Vector2 elumphantLocation = new Vector2(Projectile.Center.X, Projectile.Center.Y + ((tex.Height() / 5) * (1 - squashY))) + new Vector2(0f, Owner.gfxOffY);

            float trunkDistX = 10;
            float trunkPosX = (trunkDistX - trunkDistX * (1 - squashX)) * Projectile.spriteDirection;
            Vector2 trunkPos = new Vector2(trunkPosX * Projectile.scale, 0);

            for (int i = 0; i < 18; i++) // Backglow
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 18f).ToRotationVector2() * 6 * fxFade;
                Main.EntitySpriteDraw(tex.Value, elumphantLocation - Main.screenPosition + drawOffset, frame, usedColor with { A = 0 } * 0.2f * fxFade, Projectile.rotation, frame.Size() * 0.5f, elumphantSquash * Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
                Main.EntitySpriteDraw(trunk, elumphantLocation - Main.screenPosition + trunkPos.RotatedBy(Projectile.rotation), null, usedColor with { A = 0 } * 0.2f * fxFade, Projectile.rotation + trunkRotation, new Vector2(trunk.Width / 2, 0), elumphantSquash * Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            }

            // Main body
            Main.EntitySpriteDraw(tex.Value, elumphantLocation - Main.screenPosition, frame, bodyColor, Projectile.rotation, frame.Size() * 0.5f, elumphantSquash * Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            
            Main.EntitySpriteDraw(trunk, elumphantLocation - Main.screenPosition + trunkPos.RotatedBy(Projectile.rotation), null, bodyColor, Projectile.rotation + trunkRotation, new Vector2(trunk.Width / 2, 0), elumphantSquash * Projectile.scale, Projectile.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);

            return false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            if (dashing)
            {
                float attackMult = 0.7f * GetPower(0.5f);
                SoundStyle attack = new("CalamityMod/Sounds/Item/Bonk");
                SoundEngine.PlaySound(attack with { Pitch = -0.2f + 0.05f * Projectile.numHits, volume = 0.75f, MaxInstances = -1 }, Projectile.Center);

                target.MoveNPC(Projectile.Center.DirectionTo(target.Center), 8, false, Owner);

                hitNPCs.Add(target);
                GetTarget(true);
                lastHitNPCPos = target.Center - Vector2.UnitY * (target.height / 2);

                bool lastHit = false;
                if (targeted == null)
                {
                    Projectile.extraUpdates = 0;
                    attackMult *= 1.25f;
                    Owner.SetScreenshake(3 * attackMult);
                    lastHit = true;
                    recoiling = true;
                    modifiers.SourceDamage *= 2f * GetPower(5);
                }
                else
                {
                    attackTimer = (int)(attackTimeAdjusted / 2) - 4;
                    float minMult = 0.1f;
                    int hitsToMinMult = 3;
                    float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
                    modifiers.SourceDamage *= damageMult * GetPower(4);
                }

                if (lastHit)
                {
                    SoundStyle finalHit = new("CalamityMod/Sounds/Item/ElumphantSound");
                    SoundEngine.PlaySound(finalHit with { Pitch = -0.5f, volume = 0.8f }, Projectile.Center);
                }
                verticalSquash = lastHit ? 0.85f : 0.5f;

                int halfDusts = (int)((lastHit ? 12 : 8) * attackMult);
                for (int i = -halfDusts; i <= halfDusts; i++)
                {
                    Vector2 dustVel = Vector2.UnitX * Main.rand.NextFloat(0.6f, 1.8f) * attackMult;
                    Vector2 dustPos = Projectile.Center + dustVel * 1.5f;
                    Dust dust2 = Dust.NewDustPerfect(dustPos, ModContent.DustType<SquashDustPixelated>(),
                        dustVel.RotatedByRandom(0.35f) * (i * 0.4f), 0, default, Main.rand.NextFloat(0.2f, 0.45f) * (lastHit ? 5 : 3.5f) * attackMult);
                    dust2.noGravity = true;
                    dust2.color = Main.rand.NextBool() ? color1 : color2;
                    dust2.customData = new Vector2(0.6f, 1.5f);
                    dust2.fadeIn = -0.4f / attackMult;

                    float opacity = 0.7f;
                    if (i % 2 == 0)
                    {
                        bool clrChoose = Main.rand.NextBool();
                        Particle smoke = new CustomColorChangeSpark(dustPos, dustVel.RotatedByRandom(0.3f) * Main.rand.NextFloat(0.3f, 0.45f) * i * (lastHit ? 2 : 1), Main.rand.NextBool(3) ? "CalamityMod/Particles/WaterFoam" : "CalamityMod/Particles/BloomCircle", false, Main.rand.Next(22, 27), Main.rand.NextFloat(0.45f, 0.65f) * Projectile.scale * (lastHit ? 2 : 1) * attackMult, (clrChoose ? color1 : color2) * opacity, (clrChoose ? color2 : color1) * opacity, new Vector2(0.25f, 1.2f));
                        GeneralParticleHandler.SpawnParticle(smoke, true);
                    }

                    if (lastHit)
                    {
                        bool clrChoose = Main.rand.NextBool();
                        float velMult = Main.rand.NextFloat(1.2f, 1.35f) * i;
                        Vector2 velocity = dustVel.RotatedByRandom(0.3f) * velMult;
                        Particle mist = new CustomPulsingSpark(dustPos, velocity, "CalamityMod/Particles/ThinSparkle", "CalamityMod/Particles/BloomCircle", false, 55, Main.rand.NextFloat(1.05f, 1.45f) * Projectile.scale * attackMult, (clrChoose ? color1 : color2) * opacity, (clrChoose ? color2 : color1) * opacity,
                            new Vector2(0.6f, 1.2f), true, true, Main.rand.Next(4, 7 + 1), colorFadeSpeed: 0.85f, noShrink: true, extraRotation: 0, shrinkSpeed: 0.1f, turnRate: (Main.rand.NextBool() ? -1 : 1) * Main.rand.NextFloat(0.0028f, 0.0035f) * velMult,
                            sineRate: Main.rand.NextFloat(0.09f, 0.12f), sineIntensity: (int)(4 + Main.rand.Next(15, 20 + 1)) * Projectile.scale, sineRotation: MathHelper.PiOver2 + velocity.ToRotation());
                        GeneralParticleHandler.SpawnParticle(mist, true, Main.rand.NextBool() ? Enums.GeneralDrawLayer.AfterNPCs : Enums.GeneralDrawLayer.BeforeNPCs);
                    }

                    if (i == -1)
                        i = 1;
                }
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return CalamityUtils.CircularHitboxCollision(Projectile.Center, Projectile.width * Projectile.scale, targetHitbox);
        }
        public override bool? CanHitNPC(NPC target)
        {
            if (targeted == null)
                return false;
            return (dashing && attackTimer >= attackTimeAdjusted + 3 && target == targeted) ? null : false;
        }
    }
}
