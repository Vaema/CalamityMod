using CalamityMod.Systems.Graphic.LiquidSystem;
using Terraria;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems;

public abstract class ModLavaStyle : ModTexturedType
{
    /// <summary>
		/// The ID of the lava style.
		/// </summary>
		public int Slot { get; private set; } = -1;

    public override string Name => base.Name;

    public override string Texture => base.Texture;

    public virtual string BlockTexture => Texture + "_Block";

    public virtual string SlopeTexture => Texture + "_Slope";

    public virtual string WaterfallTexture => Texture + "_Waterfall";

    protected sealed override void Register()
    {
        Slot = ModLavaStyleLoader.Register(this);
    }

    public sealed override void SetupContent()
    {
        SetStaticDefaults();
    }

    /// <summary>
    /// Allows you to determine how much light this lava emits.<br />
    /// It can also let you light up the block in front of this lava.<br />
    /// Keep in mind this also effects what color the lavafalls emit <br />
    /// See <see cref="M:Terraria.Graphics.Light.TileLightScanner.ApplyLiquidLight(Terraria.Tile,Microsoft.Xna.Framework.Vector3@)" /> for vanilla tile light values to use as a reference.<br />
    /// </summary>
    /// <param name="i">The x position in tile coordinates.</param>
    /// <param name="j">The y position in tile coordinates.</param>
    /// <param name="r">The red component of light, usually a value between 0 and 1</param>
    /// <param name="g">The green component of light, usually a value between 0 and 1</param>
    /// <param name="b">The blue component of light, usually a value between 0 and 1</param>
    public virtual void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.55f;
        g = 0.33f;
        b = 0.11f;
    }

    /// <summary>
    /// Allows water styles to manipulate what color the liquid is drawn to, this can allow waters to be see-throughable to see backgrounds (surface and underground backgrounds not walls)
    /// </summary>
    /// <param name="x">X position of the water</param>
    /// <param name="y">Y position of the water</param>
    /// <param name="liquidColor">The vertexColor of the water color, this is both used to get the current color and to set the color of the water</param>
    public virtual void DrawColor(int x, int y, ref VertexColors liquidColor, bool isSlope)
    {
    }

    /// <summary>
    /// The ID of the dust that is created when anything splashes in lava.
    /// </summary>
    public virtual int GetSplashDust()
    {
        return DustID.Lava;
    }

    /// <summary>
    /// The ID of the gore that represents droplets of lava falling down from a block. Return <see cref="F:Terraria.ID.GoreID.LavaDrip" /> (or another existing droplet gore).
    /// </summary>
    public virtual int GetDropletGore()
    {
        return GoreID.LavaDrip;
    }

    /// <summary>
    /// Return true if the player is in the correct zone to activate the lava.
    /// </summary>
    /// <returns></returns>
    public virtual bool IsLavaActive()
    {
        return false;
    }

    /// <summary>
    /// Return false if the waterfall made by the lavastyle should have a glowmask
    /// </summary>
    /// <returns></returns>
    public virtual bool LavafallGlowmask()
    {
        return true;
    }

    /// <summary>
    /// Allows your lavastyle to inflict debuffs to Players and NPCs when they enter your lava style
    /// </summary>
    /// <param name="player">The Player that is inflicted with the debuff apon entering the lavastyle</param>
    /// <param name="onfireDuration">The duration of the OnFire! debuff. This allows for easy replacement of OnFire</param>
    public virtual void InflictDebuff(Player player, int onfireDuration)
    {
    }
}
