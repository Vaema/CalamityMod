using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.DataStructures
{
    public class StarburstEntity
    {
        public Vector2 Center = Vector2.Zero;
        public Vector2 Velocity = Vector2.Zero;
        /// <summary>
        /// How long before the starburst resumes normal following AI
        /// </summary>
        public int AICooldown = 0;
        public int frameCounter = 0;
        public int frame = 0;
        public int value = 1;
        public float scale = 1f;
        public float opacity = 1;
        public Color color = Color.Black;
        public StarburstEntity MergeTarget = null;
        public List<StarburstEntity> MergeChildren = new();
        public bool ShouldRemoveFromList = false;

        public StarburstEntity(Vector2 position, bool shouldRandomize = true)
        {
            Center = position;
            if (shouldRandomize)
            {
                Velocity = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi) * 3;
            }
            switch (Main.rand.Next(1, 7))
            {
                case 1:
                    color = Color.HotPink;
                    break;
                case 2:
                    color = Color.Yellow;
                    break;
                case 3:
                    color = Color.LimeGreen;
                    break;
                case 4:
                    color = Color.SkyBlue;
                    break;
                case 5:
                    color = Color.Lavender;
                    break;
                case 6:
                    color = Color.White;
                    break;
            }
            color = Color.Lerp(Color.White, color, 0.5f);
        }
        public void AI(Player owner, int index = 0)
        {
            if (AICooldown > 0)
            {
                MergeChildren = new();
                AICooldown--;
                return;
            }
            if (MergeTarget != null)
            {
                Velocity += Center.DirectionTo(MergeTarget.Center) * 2.5f;
                Velocity *= 0.925f;
                if (Center.Distance(MergeTarget.Center) < 12)
                    MergeTarget.MergeChildren.Remove(this);
                return;
            }
            if (Center.Distance(owner.Center) > 100)
            {
                Velocity += Center.DirectionTo(owner.Center + new Vector2(16,0).RotatedBy(index)).RotatedByRandom(0.3f);
                Velocity *= 0.95f;
            }
        }

        public void UpdatePosition()
        {
            Center += Velocity;
        }

        public void UpdateAnimation()
        {
            frameCounter++;
            if (frameCounter > 6)
            {
                frame++;
                frameCounter = 0;
            }
            if (frame > 5)
                frame = 0;
        }
    }
}
