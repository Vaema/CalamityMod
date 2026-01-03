using System;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.DamageOverTime
{
    public class SeaKingsAssurance : ModBuff
    {
        public static Color BaseColor => new Color(31, 78, 155);
        public static Color LightColor => new Color(31, 108, 225);
        public static void Apply(NPC npc, Vector2 orig)
        {
            int stacks = 1;
            if (npc.HasBuff<SeaKingsAssurance>())
            {
                int buffTimer = npc.buffTime[npc.FindBuffIndex(ModContent.BuffType<SeaKingsAssurance>())];
                npc.buffTime[npc.FindBuffIndex(ModContent.BuffType<SeaKingsAssurance>())] += FramesPerStack;
                npc.buffTime[npc.FindBuffIndex(ModContent.BuffType<SeaKingsAssurance>())] = ((int)Math.Ceiling((double)buffTimer / FramesPerStack) + 1) * FramesPerStack;
                buffTimer = npc.buffTime[npc.FindBuffIndex(ModContent.BuffType<SeaKingsAssurance>())];
                stacks += ((int)Math.Ceiling((double)buffTimer / FramesPerStack));
            }
            else
            {
                npc.AddBuff(ModContent.BuffType<SeaKingsAssurance>(), FramesPerStack);
            }

            npc.GetGlobalNPC<SeaKingsAssuranceNPC>().StackGlow = 1f;
            npc.GetGlobalNPC<SeaKingsAssuranceNPC>().StackScale = 1.2f;

            SoundStyle style = new SoundStyle("CalamityMod/Sounds/Item/WaterSplash1");
            SoundEngine.PlaySound(style.WithPitchOffset(1f + ((float)stacks / 6f)).WithVolumeScale(0.2f));

            float sc = 0.05f + ((float)stacks * 0.06f);

            for (int i = 0; i < 5; i++)
            {
                float rot = Main.rand.NextFloat(MathHelper.TwoPi) + MathHelper.PiOver2;
                if (orig != Vector2.Zero)
                {
                    rot = orig.AngleTo(npc.Center) + MathHelper.PiOver2;
                }

                GeneralParticleHandler.SpawnParticle(new CustomPulse(
                    npc.Center, (rot - MathHelper.PiOver2).ToRotationVector2() * 6f, Color.Lerp(BaseColor, LightColor, sc * 3f), "CalamityMod/Particles/ForwardSmear", new Vector2(Main.rand.NextFloat(0.015f, 0.1f), 0.1f),
                    rot, sc, sc * 2f, 8
                    ));
            }

            float range = 20 + ((float)stacks * 15f);

            if (orig == Vector2.Zero)
            {
                foreach (NPC npc2 in Main.npc)
                {
                    if (npc2 != npc)
                    {
                        if (npc2.active && !npc2.friendly && !npc2.isLikeATownNPC)
                        {
                            if (npc2.Distance(npc.Center) < range)
                            {
                                Apply(npc2, npc.Center);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// The number of frames per stack of this buff. 
        /// For instance, if the buff has less than one of this number's worth of time left,
        /// it will count as one stack.
        /// </summary>
        public static int FramesPerStack => 60;
        public static SoundStyle ImpactSound => new SoundStyle("CalamityMod/Sounds/Item/WaterSplash1");
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            BuffID.Sets.LongerExpertDebuff[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.buffTime[buffIndex] = Math.Min(npc.buffTime[buffIndex], 10 * FramesPerStack);

            npc.GetGlobalNPC<SeaKingsAssuranceNPC>().assuredStacks = (int)Math.Ceiling((double)(npc.buffTime[buffIndex] / FramesPerStack));
        }
    }

    public class SeaKingsAssuranceNPC : GlobalNPC
    {
        public static readonly Asset<Texture2D> Stacks = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/AmidiasTrident_Stacks");

        public override bool InstancePerEntity => true;
        public int assuredStacks = 0;
        public int StackFrame = 0;
        public float StackRotation = 0f;
        public float StackScale = 0f;
        public float StackGlow = 0f;
        public override void ResetEffects(NPC npc)
        {
            if (assuredStacks == 0)
            {
                StackScale *= 0.5f;
            }
            else
            {
                StackScale = MathHelper.Lerp(StackScale, 1f, 0.2f);
            }
            StackGlow *= 0.7f;
            assuredStacks = 0;
            base.ResetEffects(npc);
        }
        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            base.DrawEffects(npc, ref drawColor);
        }
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
        }
        public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            StackFrame = 0;
            if (assuredStacks > 5) StackFrame = 1;
            if (assuredStacks > 8) StackFrame = 2;

            Rectangle frame = Stacks.Frame(3, 1, StackFrame);

            if (StackFrame == 2)
            {
                StackRotation = MathHelper.Lerp(StackRotation, MathHelper.Pi, 0.2f);
            }
            else
            {
                StackRotation = MathHelper.Lerp(StackRotation, 0f, 0.1f);
            }

            float offset = -28;

            for (int b = 0; b < CalamityGlobalNPC.moddedDebuffTextureList.Count; b++)
            {
                if (CalamityGlobalNPC.moddedDebuffTextureList[b].Item2.Invoke(npc))
                {
                    offset = -48;
                }
            }

            if (StackScale > 0.2f && assuredStacks > 2)
            {
                spriteBatch.Draw(Stacks.Value, npc.Top - Main.screenPosition + new Vector2(0, offset), frame, Color.White, StackRotation, frame.Size() / 2f, StackScale, SpriteEffects.None, 0f);

                for (int i = 0; i < 4; i++)
                {
                    spriteBatch.Draw(Stacks.Value, npc.Top - Main.screenPosition + new Vector2(0, offset), frame, Color.White.MultiplyRGBA(new(StackGlow, StackGlow, StackGlow, 0f)), StackRotation, frame.Size() / 2f, StackScale, SpriteEffects.None, 0f);
                }
            }
            else
            {
                StackRotation = 0f;
            }
        }
    }
}
