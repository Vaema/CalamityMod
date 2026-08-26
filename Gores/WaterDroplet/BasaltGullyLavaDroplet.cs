using Microsoft.Xna.Framework;

namespace CalamityMod.Gores.WaterDroplet;

public class BasaltGullyLavaDroplet : LiquidDropletGore
{
    public override bool lavaDroplet => true;

    public override Vector3 lavaColor => new(2.5f, 1.3f, 0.1f);
}
