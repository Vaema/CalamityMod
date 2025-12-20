using System;
using CalamityMod.Effects;
using CalamityMod.Items.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Mechanic;

public class WulfrumPingTileEffect : IPingedTileEffect, ILoadable
{
    public const string EffectName = "WulfrumPing";

    public static WulfrumPingTileEffect Instance { get; private set; }

    internal static Texture2D emptyFrame;
    public const int MaxPingLife = 350;
    public const int MaxPingTravelTime = 60;
    const float PingWaveThickness = 50f;

    public const float MaxPingRadius = 1700f;
    public static Vector2 PingCenter = Vector2.Zero;
    public static int PingTimer = 0;
    public static float PingProgress => (MaxPingLife - PingTimer) / (float)MaxPingLife;

    public bool Active => PingTimer > 0;

    public BlendState BlendState => BlendState.Additive;

    void ILoadable.Load(Mod mod)
    {
        Instance = this;
        TilePingerSystem.RegisterEffect(EffectName, this);
    }

    void ILoadable.Unload()
    {
        Instance = null;
    }

    public bool TryAddPing(Vector2 position, Player pinger)
    {
        //Only one ping at a time
        if (Active)
            return false;

        PingCenter = position;
        PingTimer = MaxPingLife;
        return true;
    }

    public Effect SetupEffect()
    {
        emptyFrame ??= ModContent.Request<Texture2D>("CalamityMod/Projectiles/InvisibleProj").Value;

        Effect tileEffect = Filters.Scene["CalamityMod:WulfrumTilePing"].GetShader().Shader;
        tileEffect.Parameters["pingCenter"].SetValue(PingCenter);
        tileEffect.Parameters["pingRadius"].SetValue(MaxPingRadius);
        tileEffect.Parameters["pingWaveThickness"].SetValue(PingWaveThickness);
        tileEffect.Parameters["pingProgress"].SetValue(PingProgress);
        tileEffect.Parameters["pingTravelTime"].SetValue(MaxPingTravelTime / (float)MaxPingLife);
        tileEffect.Parameters["pingFadePoint"].SetValue(0.9f);
        tileEffect.Parameters["edgeBlendStrength"].SetValue(1f);
        tileEffect.Parameters["edgeBlendOutLength"].SetValue(6f);
        tileEffect.Parameters["tileEdgeBlendStrenght"].SetValue(2f);

        tileEffect.Parameters["waveColor"].SetValue(Color.GreenYellow.ToVector4());
        tileEffect.Parameters["baseTintColor"].SetValue(Color.DeepSkyBlue.ToVector4() * 0.5f);
        tileEffect.Parameters["scanlineColor"].SetValue(Color.YellowGreen.ToVector4() * 1f);
        tileEffect.Parameters["tileEdgeColor"].SetValue(Color.GreenYellow.ToVector3());
        tileEffect.Parameters["Resolution"].SetValue(8f);

        tileEffect.Parameters["time"].SetValue(Main.GameUpdateCount);
        Vector4[] scanLines =
        [
            new Vector4(0f, 4f, 0.1f, 0.5f),
            new Vector4(1f, 4f, 0.1f, 0.5f),
            new Vector4(37f, 60f, 0.4f, 1f),
            new Vector4(2f, 6f, -0.2f, 0.3f),
            new Vector4(0f, 4f, 0.1f, 0.5f), //vertical start
            new Vector4(1f, 4f, 0.1f, 0.5f),
            new Vector4(2f, 6f, -0.2f, 0.3f)
        ];

        tileEffect.Parameters["ScanLines"].SetValue(scanLines);
        tileEffect.Parameters["ScanLinesCount"].SetValue(scanLines.Length);
        tileEffect.Parameters["verticalScanLinesIndex"].SetValue(4);

        return tileEffect;
    }

    public void PerTileSetup(Point pos, ref Effect effect)
    {
        //Up, left, right, down.
        effect.Parameters["cardinalConnections"].SetValue(new bool[] { Connected(pos, 0, -1), Connected(pos, -1, 0), Connected(pos, 1, 0), Connected(pos, 0, 1) });
        effect.Parameters["tilePosition"].SetValue(pos.ToVector2() * 16f);
    }

    public static bool Connected(Point pos, int displaceX, int displaceY)
    {
        return Main.IsTileSpelunkable(pos.X + displaceX, pos.Y + displaceY)
            && Main.tile[pos].TileType == Main.tile[pos.X + displaceX, pos.Y + displaceY].TileType;
    }

    public bool ShouldRegisterTile(int x, int y)
    {
        return Main.IsTileSpelunkable(x, y);
    }

    public void ModifyTileLight(int x, int y, Color tileLight, ref Color resultColor)
    {
        float distanceFromCenter = (new Point(x, y).ToWorldCoordinates() - PingCenter).Length();
        float currentExpansion = MathHelper.Clamp(PingProgress * MaxPingLife / (float)MaxPingTravelTime, 0f, 1f) * MaxPingRadius;

        if (distanceFromCenter - 8 > currentExpansion)
            return;

        float brightness = 1f;
        Tile tile = Framing.GetTileSafely(x, y);
        //Counteracts slopes and half tiles being too bright
        if (tile.Slope != SlopeType.Solid || tile.IsHalfBlock)
            brightness = 0.64f;

        //Fade on the edges
        if (distanceFromCenter + 8 > currentExpansion)
            brightness *= 1 - (distanceFromCenter - currentExpansion + 8f) / 16f;

        //Fade away with the effect
        brightness *= 1 - Math.Max(PingProgress - 0.9f, 0) / (0.1f);

        if (tileLight.R < 200 * brightness) tileLight.R = (byte)(200 * brightness);
        if (tileLight.G < 200 * brightness) tileLight.G = (byte)(200 * brightness);
        if (tileLight.B < 200 * brightness) tileLight.B = (byte)(200 * brightness);
        resultColor = tileLight;
    }

    public void DrawTile(Point pos)
    {
        Main.spriteBatch.Draw(emptyFrame, pos.ToWorldCoordinates() - Main.screenPosition, null, Color.White, 0, new Vector2(emptyFrame.Width / 2f, emptyFrame.Height / 2f), 16f, 0, 0);
    }

    // CIT 16JUL2025: Tile lighting override is now applied via an On edit; this code is duplicated there, and thus is no longer needed here.
    /*public void EditDrawData(int i, int j, ref TileDrawInfo drawData)
    {
        float distanceFromCenter = (new Point(i, j).ToWorldCoordinates() - PingCenter).Length();
        float currentExpansion = MathHelper.Clamp(PingProgress * MaxPingLife / (float)MaxPingTravelTime, 0f, 1f) * MaxPingRadius;

        if (distanceFromCenter - 8 > currentExpansion)
            return;

        float brightness = 1f;
        Tile tile = Framing.GetTileSafely(i, j);
        //Counteracts slopes and half tiles being too bright
        if (tile.Slope != SlopeType.Solid || tile.IsHalfBlock)
            brightness = 0.64f;

        //Fade on the edges
        if (distanceFromCenter + 8 > currentExpansion)
            brightness *= 1 - (distanceFromCenter - currentExpansion + 8f) / 16f;

        //Fade away with the effect
        brightness *= 1 - Math.Max(PingProgress - 0.9f, 0) / (0.1f);

        if (drawData.tileLight.R < 200 * brightness) drawData.tileLight.R = (byte)(200 * brightness);
        if (drawData.tileLight.G < 200 * brightness) drawData.tileLight.G = (byte)(200 * brightness);
        if (drawData.tileLight.B < 200 * brightness) drawData.tileLight.B = (byte)(200 * brightness);
    }*/

    public void UpdateEffect()
    {
        if (PingTimer > 0)
        {
            PingTimer--;

            //if the effect ended (and the player has a treasure pigner in their inventory, of course), play a recharge beep
            if (PingTimer == 0 && Main.LocalPlayer.InventoryHas(ModContent.ItemType<WulfrumTreasurePinger>()))
                SoundEngine.PlaySound(WulfrumTreasurePinger.RechargeBeepSound);
        }
    }
}

public class BurrowerPingTileEffect : IPingedTileEffect, ILoadable
{
    public const string EffectName = "BurrowerPing";

    public static BurrowerPingTileEffect Instance { get; private set; }

    internal static Texture2D emptyFrame;
    public const int MaxPingLife = 60;
    public const int MaxPingTravelTime = 10;
    const float PingWaveThickness = 50f;

    public const float MaxPingRadius = 160;
    public static Vector2 PingCenter = Vector2.Zero;
    public static int PingTimer = 0;
    public static float PingProgress => (MaxPingLife - PingTimer) / (float)MaxPingLife;

    public bool Active => PingTimer > 0;

    public BlendState BlendState => BlendState.Additive;

    void ILoadable.Load(Mod mod)
    {
        Instance = this;
        TilePingerSystem.RegisterEffect(EffectName, this);
    }

    void ILoadable.Unload()
    {
        Instance = null;
    }

    public bool TryAddPing(Vector2 position, Player pinger)
    {
        if (Active)
        {
            PingCenter = position;
            PingTimer = Math.Max(PingTimer, MaxPingLife - MaxPingTravelTime);
            return false;
        }
        PingCenter = position;
        PingTimer = MaxPingLife;
        return true;
    }


    public Effect SetupEffect()
    {
        if (emptyFrame == null)
            emptyFrame = ModContent.Request<Texture2D>("CalamityMod/Projectiles/InvisibleProj").Value;

        Effect tileEffect = Filters.Scene["CalamityMod:WulfrumTilePing"].GetShader().Shader;
        tileEffect.Parameters["pingCenter"].SetValue(PingCenter);
        tileEffect.Parameters["pingRadius"].SetValue(MaxPingRadius);
        tileEffect.Parameters["pingWaveThickness"].SetValue(PingWaveThickness);
        tileEffect.Parameters["pingProgress"].SetValue(PingProgress);
        tileEffect.Parameters["pingTravelTime"].SetValue(MaxPingTravelTime / (float)MaxPingLife);
        tileEffect.Parameters["pingFadePoint"].SetValue(0.9f);
        tileEffect.Parameters["edgeBlendStrength"].SetValue(1f);
        tileEffect.Parameters["edgeBlendOutLength"].SetValue(6f);
        tileEffect.Parameters["tileEdgeBlendStrenght"].SetValue(2f);

        tileEffect.Parameters["waveColor"].SetValue(ArsenalEffects.ArsenalGaussColor.ToVector4());
        tileEffect.Parameters["baseTintColor"].SetValue(Color.Orange.ToVector4() * 0.5f);
        tileEffect.Parameters["scanlineColor"].SetValue(ArsenalEffects.ArsenalLaserColor.ToVector4() * 1f);
        tileEffect.Parameters["tileEdgeColor"].SetValue(ArsenalEffects.ArsenalGaussColor.ToVector3());
        tileEffect.Parameters["Resolution"].SetValue(8f);

        tileEffect.Parameters["time"].SetValue(Main.GameUpdateCount);
        Vector4[] scanLines =
        [
            new Vector4(0f, 4f, 0.1f, 0.5f),
            new Vector4(1f, 4f, 0.1f, 0.5f),
            new Vector4(37f, 60f, 0.4f, 1f),
            new Vector4(2f, 6f, -0.2f, 0.3f),
            new Vector4(0f, 4f, 0.1f, 0.5f), //vertical start
            new Vector4(1f, 4f, 0.1f, 0.5f),
            new Vector4(2f, 6f, -0.2f, 0.3f)
        ];

        tileEffect.Parameters["ScanLines"].SetValue(scanLines);
        tileEffect.Parameters["ScanLinesCount"].SetValue(scanLines.Length);
        tileEffect.Parameters["verticalScanLinesIndex"].SetValue(4);

        return tileEffect;
    }

    public void PerTileSetup(Point pos, ref Effect effect)
    {
        //Up, left, right, down.
        effect.Parameters["cardinalConnections"].SetValue(new bool[] { Connected(pos, 0, -1), Connected(pos, -1, 0), Connected(pos, 1, 0), Connected(pos, 0, 1) });
        effect.Parameters["tilePosition"].SetValue(pos.ToVector2() * 16f);
    }

    public static bool Connected(Point pos, int displaceX, int displaceY)
    {
        return TileID.Sets.Ore[Main.tile[pos.X + displaceX, pos.Y + displaceY].TileType]
            && Main.tile[pos].TileType == Main.tile[pos.X + displaceX, pos.Y + displaceY].TileType;
    }

    public bool ShouldRegisterTile(int x, int y)
    {
        if (!Main.tile[x, y].HasTile)
            return false;
        return TileID.Sets.Ore[Main.tile[x, y].TileType];
    }

    public void ModifyTileLight(int x, int y, Color tileLight, ref Color resultColor)
    {
        float distanceFromCenter = (new Point(x, y).ToWorldCoordinates() - BurrowerPingTileEffect.PingCenter).Length();
        float currentExpansion = MathHelper.Clamp(BurrowerPingTileEffect.PingProgress * BurrowerPingTileEffect.MaxPingLife / (float)BurrowerPingTileEffect.MaxPingTravelTime, 0f, 1f) * BurrowerPingTileEffect.MaxPingRadius;

        if (distanceFromCenter - 8 > currentExpansion)
            return;

        float brightness = 1f;
        Tile tile = Framing.GetTileSafely(x, y);
        //Counteracts slopes and half tiles being too bright
        if (tile.Slope != SlopeType.Solid || tile.IsHalfBlock)
            brightness = 0.64f;

        //Fade on the edges
        if (distanceFromCenter + 8 > currentExpansion)
            brightness *= 1 - (distanceFromCenter - currentExpansion + 8f) / 16f;

        //Fade away with the effect
        brightness *= 1 - Math.Max(BurrowerPingTileEffect.PingProgress - 0.9f, 0) / (0.1f);

        if (tileLight.R < 200 * brightness) tileLight.R = (byte)(200 * brightness);
        if (tileLight.G < 200 * brightness) tileLight.G = (byte)(200 * brightness);
        if (tileLight.B < 200 * brightness) tileLight.B = (byte)(200 * brightness);
        resultColor = tileLight;
    }

    public void DrawTile(Point pos)
    {
        Main.spriteBatch.Draw(emptyFrame, pos.ToWorldCoordinates() - Main.screenPosition, null, Color.White, 0, new Vector2(emptyFrame.Width / 2f, emptyFrame.Height / 2f), 16f, 0, 0);
    }

    public void UpdateEffect()
    {
        if (PingTimer > 0)
        {
            PingTimer--;
        }
    }
}
