using System.Collections.Generic;
using System.Linq;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Utilities.Daybreak;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Mechanic;

public class StarburstEntity
{
    private sealed class StarburstManager : ModPlayer
    {
        //Done with a ref to CalamityPlayer so that it's easy for other code to get this value with Player.Calamity()
        public ref List<StarburstEntity> StarburstEntities => ref Player.Calamity().StarburstEntities;
        public override void PostUpdate()
        {

            for (var i = 0; i < StarburstEntities.Count; i++)
            {
                StarburstEntity star = StarburstEntities[i];
                star.AI(Player, i);
                star.UpdatePosition();
                star.UpdateAnimation();
                var safeStarLooper = star.MergeChildren.ToList();
                foreach (var ministar in safeStarLooper)
                {
                    ministar.AI(Player, i);
                    ministar.UpdatePosition();
                    ministar.UpdateAnimation();
                }
            }
        }
        public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
        {
            if (StarburstEntities.Count > 0 && drawInfo.shadow == 0f)
            {
                using (Main.spriteBatch.Scope())
                {
                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    //This uses the same star and glow textures that the constellations use, so we're just getting the static textures Draco uses to prevent needless requests
                    var tex = TextureAssets.Projectile[ModContent.ProjectileType<DracoConstellation>()].Value;
                    var glowTex = DracoConstellation.GetGlowTex();
                    for (var i = 0; i < StarburstEntities.Count; i++)
                    {
                        StarburstEntity star = StarburstEntities[i];
                        var color = star.color;
                        var value = star.value;
                        value -= star.MergeChildren.Count;
                        var ScaleMod = MathHelper.Lerp(0.4f, 0.8f, value / 10f);

                        Main.spriteBatch.Draw(glowTex, star.Center - Main.screenPosition, null, star.color * 0.66f, 0, glowTex.Size() * 0.5f, 0.2f * star.scale * ScaleMod, SpriteEffects.None, 1);
                        Main.spriteBatch.Draw(tex, star.Center - Main.screenPosition, null, star.color * 0.66f, MathHelper.WrapAngle(Main.GlobalTimeWrappedHourly + i), tex.Size() * 0.5f, 0.75f * star.scale * ScaleMod, SpriteEffects.None, 1);
                        for (var i2 = 0; i2 < star.MergeChildren.Count; i2++)
                        {
                            var ministar = star.MergeChildren[i2];
                            ScaleMod = MathHelper.Lerp(0.4f, 0.8f, ministar.value / 10f);
                            Main.spriteBatch.Draw(glowTex, ministar.Center - Main.screenPosition, null, ministar.color * 0.66f, 0, glowTex.Size() * 0.5f, 0.2f * ministar.scale * ScaleMod, SpriteEffects.None, 1);
                            Main.spriteBatch.Draw(tex, ministar.Center - Main.screenPosition, null, ministar.color * 0.66f, MathHelper.WrapAngle(Main.GlobalTimeWrappedHourly + i), tex.Size() * 0.5f, 0.75f * ministar.scale * ScaleMod, SpriteEffects.None, 1);
                        }
                    }
                    Main.spriteBatch.End();
                }
            }
        }
    }

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
            Velocity += Center.DirectionTo(owner.Center + new Vector2(16, 0).RotatedBy(index)).RotatedByRandom(0.3f);
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
