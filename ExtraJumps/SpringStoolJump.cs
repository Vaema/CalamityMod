using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CalamityMod.ExtraJumps
{
    public class SpringStoolJump : ExtraJump
    {
        public override Position GetDefaultPosition() => BeforeBottleJumps;
        public override float GetDurationMultiplier(Player player) => 1f;
        public override void UpdateHorizontalSpeeds(Player player)
        {
            player.runAcceleration *= 2f;
            player.maxRunSpeed *= 1.35f;
        }

        public override void OnStarted(Player player, ref bool playSound)
        {
            playSound = true;
            if (player.wingsLogic <= 0)
            {
                player.velocity.Y *= player.slowFall ? 2.0f : 2.75f;
            }
            else
            {
                player.velocity.Y *= 1.7f;
            }
            player.StopExtraJumpInProgress();
            for (int i = 0; i < 3; i++)
            {
                Vector2 position = player.Center;
                Vector2 velocity = (Vector2.UnitY * -(2.5f + 2f * i)) * player.gravDir;
                int lifetime = 120;
                float scale = MathHelper.Lerp(0.05f, 0.1f, i / 3f);
                Color color = new Color(94, 229, 163);
                Vector2 stretch = new Vector2(0.5f, 1.5f);
                float shrink = -0.3f;
                Particle boostRing = new CustomSpark(position, velocity, "CalamityMod/Particles/HighResHollowCircleHardEdgeAlt", false, lifetime, scale, color, stretch, shrinkSpeed: shrink);
                GeneralParticleHandler.SpawnParticle(boostRing);
            }
        }
        public override void ShowVisuals(Player player)
        {
            player.StopExtraJumpInProgress();
        }
    }
}
