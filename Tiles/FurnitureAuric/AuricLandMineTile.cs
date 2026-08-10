using CalamityMod.Particles;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.FurnitureAuric;

public class AuricLandMineTile : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileLighted[Type] = true;
        Main.tileFrameImportant[Type] = true;
        // Land Mines break on all entity contact, and so does this
        TileID.Sets.PressurePlate[Type] = 0;
        TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsTileHammeringIfOnTopOfIt[Type] = true;
        TileID.Sets.PreventsSandfall[Type] = true;
        Main.tileNoAttach[Type] = true;
        Main.tileLavaDeath[Type] = false;
        Main.tileWaterDeath[Type] = false;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateHeights = new int[] { 18 };
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.addTile(Type);
        MinPick = 250;
    }

    public override void HitSwitch(int i, int j)
    {
        Vector2 tileCenter = new Vector2(i, j) * 16f + Vector2.One * 8f;
        SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Item/DudFire") with { Pitch = 0.8f }, tileCenter);
        GenericSparkle sparkle = new(tileCenter, Vector2.Zero, Color.Goldenrod, Color.Gold, 2.5f, 9, Main.rand.NextFloat(-0.01f, 0.01f), 2.68f);
        GeneralParticleHandler.SpawnParticle(sparkle);
        WorldGen.KillTile(i, j, noItem: true);
        NetMessage.SendTileSquare(-1, i, j);
        // Remove all player iframes
        Main.LocalPlayer.RemoveAllIFrames();
        Projectile.NewProjectile(new EntitySource_TileInteraction(Main.LocalPlayer, i, j), tileCenter, Vector2.Zero, ModContent.ProjectileType<AuricLandMineExplosion>(), 40000, 0f);
    }

    public override void HitWire(int i, int j) => Wiring.HitSwitchAndSync(i, j);

    public override bool IsTileDangerous(int i, int j, Player player) => true;

    public override bool CanExplode(int i, int j) => false;

    public override bool CreateDust(int i, int j, ref int type)
    {
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Electric);
        return false;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }
}
