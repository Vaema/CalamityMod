using System;
using CalamityMod.DataStructures;
using CalamityMod.ExtraTextures;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
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
        public static int FinalDamageAmount => 60;
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

        void DamageEffect(NPC npc, int damageAmount)
        {
            Projectile.NewProjectileDirect(new EntitySource_Buff(npc, ModContent.BuffType<SeaKingsAssurance>(), 
                npc.FindBuffIndex(ModContent.BuffType<SeaKingsAssurance>())), npc.Center, Vector2.Zero, ModContent.ProjectileType<DirectStrike>(),
                FinalDamageAmount, 0f, ai0: npc.whoAmI);

            npc.SimpleStrikeNPC(damageAmount, 0);

            for (int i = 0; i < 5; i++)
            {
                GeneralParticleHandler.SpawnParticle(new CustomPulse(
                    npc.Center, Vector2.Zero, LightColor, "CalamityMod/Particles/ShineExplosion2", new Vector2(Main.rand.NextFloat(0.2f, 0.3f), Main.rand.NextFloat(0.1f, 0.15f)),
                    0f, 0.2f, 0.8f, 6
                    ));
            }
        }
    }

    public class SeaKingsAssuranceNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;
        public int assuredStacks = 0;
        public override void ResetEffects(NPC npc)
        {
            assuredStacks = 0;
            base.ResetEffects(npc);
        }
        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            base.DrawEffects(npc, ref drawColor);
        }
    }
}
