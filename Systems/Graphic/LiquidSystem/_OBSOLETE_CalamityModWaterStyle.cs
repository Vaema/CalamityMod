using System;
using CalamityMod.Systems.Graphic.LiquidSystem;
using Terraria;
using Terraria.Graphics;
using Terraria.ModLoader;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace CalamityMod.Systems
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    [Obsolete($"Use {nameof(IPaintableWaterStyle)} and {nameof(IEmittableWaterStyle)} Instead")]
    public abstract class CalamityModWaterStyle : ModWaterStyle, IPaintableWaterStyle, IEmittableWaterStyle
    {
        public void ModifyDrawColor(in Tile tile, int x, int y, ref VertexColors liquidColor, bool isSlope) => DrawColor(x, y, ref liquidColor, isSlope);

        /// <summary>
        /// Allows water styles to manipulate what color the liquid is drawn to, this can allow waters to be see-throughable to see backgrounds (surface and underground backgrounds not walls)
        /// </summary>
        /// <param name="x">X position of the water</param>
        /// <param name="y">Y position of the water</param>
        /// <param name="liquidColor">The vertexColor of the water color, this is both used to get the current color and to set the color of the water</param>
        public virtual void DrawColor(int x, int y, ref VertexColors liquidColor, bool isSlope)
        {
        }

        public void ModifyLight(in Tile tile, int x, int y, ref float r, ref float g, ref float b) => ModifyLight(x, y, ref r, ref g, ref b);

        /// <summary>
        /// Allows you to determine how much light this water emits.<br />
        /// It can also let you light up the block in front of this water.<br />
        /// See <see cref="M:Terraria.Graphics.Light.TileLightScanner.ApplyLiquidLight(Terraria.Tile,Microsoft.Xna.Framework.Vector3@)" /> for vanilla tile light values to use as a reference.<br />
        /// </summary>
        /// <param name="i">The x position in tile coordinates.</param>
        /// <param name="j">The y position in tile coordinates.</param>
        /// <param name="r">The red component of light, usually a value between 0 and 1</param>
        /// <param name="g">The green component of light, usually a value between 0 and 1</param>
        /// <param name="b">The blue component of light, usually a value between 0 and 1</param>
        public virtual void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {

        }
    }
}
