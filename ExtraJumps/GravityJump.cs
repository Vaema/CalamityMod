using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.ExtraJumps
{
    public class GravityJump : ExtraJump
    {
        public override Position GetDefaultPosition() => BeforeBottleJumps;
        public override float GetDurationMultiplier(Player player) => 3f; 
        public override void UpdateHorizontalSpeeds(Player player)
        {
            player.runAcceleration *= 2f;
            player.maxRunSpeed *= 4f;
        }

        public override void OnStarted(Player player, ref bool playSound)
        {
            playSound = true;
            if (player.wingsLogic <= 0) {
                player.velocity.Y *= 2.5f;
            } else {
                player.velocity.Y *= 1.7f;
            }
            player.StopExtraJumpInProgress();
            for (int i = 0; i < 3; i++)
                {
                    Vector2 position = player.Center;
                    Vector2 velocity = (Vector2.UnitY * (0.5f + 2f*i)) * player.gravDir;
                    int lifetime = 60;
                    float scale = MathHelper.Lerp(0.03f,0.05f,i/5f);
                    Color color = i % 2 <= 3 ? new Color(94,229,163) : new Color(84,84,84);
                    Vector2 stretch = new Vector2(0.7f, i == 0 ? 1.3f : 0.9f);
                    float shrink = -0.3f - 0.1f*i;
                    Particle boostRing = new CustomSpark(position, velocity, "CalamityMod/Particles/HighResHollowCircleHardEdgeAlt", false,lifetime,scale,color,stretch,shrinkSpeed: shrink);
                    GeneralParticleHandler.SpawnParticle(boostRing);
                }
        }
        public override void ShowVisuals(Player player)
        {
            player.StopExtraJumpInProgress();
        }
    }
}
