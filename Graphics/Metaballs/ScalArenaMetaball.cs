using System;
using System.Collections.Generic;
using System.Linq;
using CalamityMod.Enums;
using CalamityMod.NPCs.SupremeCalamitas;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Graphics.Metaballs;

public class ScalArenaMetaball : Metaball
{
    public class Particle
    {
        public float Size;

        public Vector2 Velocity;

        public Vector2 Center;

        public Texture2D TextureToUse = null;

        public float rotation = 0;

        public float SizeScaling = 0.85f;
        public int CurrentFrame = 0;
        public int MaxFrames = 1;
        public Vector2 Scale = Vector2.One;
        public int Age = 0;
        public Particle(Vector2 center, Vector2 velocity, float size)
        {
            Center = center;
            Velocity = velocity;
            Size = size;
        }

        public void Update()
        {
            Age++;
            Center += Velocity;
            Velocity *= 0.96f;
            if (Age > 1)
                Size *= SizeScaling;
        }
    }

    public override bool IgnoreFPS => true;
    public static List<Particle> Particles
    {
        get;
        private set;
    } = new();

    public override bool AnythingToDraw => Particles.Any();

    private static Asset<Texture2D> MainLayer;
    private static Texture2D WavyLineLayer; //Not wrapped in Asset<> as we need to edit the texutre post-loading.
    public override IEnumerable<Texture2D> Layers
    {
        get
        {
            yield return MainLayer.Value;
            yield return WavyLineLayer;
        }
    }

    public override List<Vector4> LayerColors => [Color.White.ToVector4(), EdgeColor.ToVector4()];
    public override void Load()
    {
        if (Main.dedServ)
            return;

        MainLayer = ModContent.Request<Texture2D>($"CalamityMod/Graphics/Metaballs/ScalArenaLayerSmoke", AssetRequestMode.ImmediateLoad);
        WavyLineLayer = ModContent.Request<Texture2D>($"CalamityMod/Graphics/Metaballs/ScalArenaLayerWave", AssetRequestMode.ImmediateLoad).Value;

    }
    public override GeneralDrawLayer DrawLayer => GeneralDrawLayer.BeforeNPCs;

    public override Color EdgeColor => SupremeCalamitas.CurrentColor;

    public override void Update()
    {
        for (int i = 0; i < Particles.Count; i++)
            Particles[i].Update();
        Particles.RemoveAll(p => p.Size <= 2f && p.Age > 1);
    }

    public static Particle SpawnParticle(Vector2 position, Vector2 velocity, float size)
    {
        Particle particle = new(position, velocity, size);
        Particles.Add(particle);

        return particle;
    }
    public override Vector2 CalculateManualOffsetForLayer(int layerIndex)
    {
        switch (layerIndex)
        {
            case 0:
                return new Vector2(MathF.Sin(Main.GlobalTimeWrappedHourly*0.03f)*10,MathF.Cos(Main.GlobalTimeWrappedHourly* 0.1f) * 17);
            case 1:
                return Vector2.UnitY * Main.GlobalTimeWrappedHourly * 0.03f;
        }
        return Vector2.Zero;
    }

    bool hasRunTextureCorrection = false;
    public override void PrepareSpriteBatch(SpriteBatch spriteBatch)
    {
        //This only runs once per game session. I put it here as load-related functions use multithreading and therefore don't support drawing functions used here.
        //This converts the layer texture into a transparent version, as-if it was additive. Since it's all greyscale, we can just use the red channel from the original in the calculations.
        if (!hasRunTextureCorrection) 
        {
            var BaseArray = new Color[WavyLineLayer.Width * WavyLineLayer.Height];
            var ColorArray = new Color[WavyLineLayer.Width * WavyLineLayer.Height];
            WavyLineLayer.GetData(BaseArray);
            for (var i = 0; i < BaseArray.Length; i++)
            {
                ColorArray[i] = new Color((int)BaseArray[i].R, (int)BaseArray[i].R, (int)BaseArray[i].R, (int)BaseArray[i].R);
            }
            WavyLineLayer.SetData(ColorArray);
            hasRunTextureCorrection = true;
        }
        base.PrepareSpriteBatch(spriteBatch);
    }
    public override void DrawInstances()
    {
        Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/BasicCircle").Value;

        foreach (Particle particle in Particles)
        {
            var texture2d = particle.TextureToUse ?? tex;
            Vector2 drawPosition = particle.Center - Main.screenPosition;
            Vector2 scale = particle.Scale * particle.Size / texture2d.Width;
            var frame = texture2d.Frame(1, particle.MaxFrames, 0, particle.CurrentFrame);
            Vector2 origin = frame.Size() * 0.5f;
            Main.spriteBatch.Draw(texture2d, drawPosition, frame, Color.White, particle.rotation, origin, scale, SpriteEffects.None, 0f);
        }
    }
}
