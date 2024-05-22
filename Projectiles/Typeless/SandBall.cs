using System;
using CalamityMod.Tiles.Abyss;
using CalamityMod.Tiles.AstralDesert;
using CalamityMod.Tiles.SunkenSea;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class AstralSandBallFalling : SandBall
    {
        public override string Texture => "CalamityMod/Projectiles/Typeless/SandBallAstral";
        public override bool Fired => false;
        public override int TileType => ModContent.TileType<AstralSand>();
        public override int ItemType => ModContent.ItemType<Items.Placeables.AstralSand>();
        public override int DustType => 108;
    }

    public class AstralSandBallGun : SandBall
    {
        public override string Texture => "CalamityMod/Projectiles/Typeless/SandBallAstral";
        public override int TileType => ModContent.TileType<AstralSand>();
        public override int ItemType => ModContent.ItemType<Items.Placeables.AstralSand>();
        public override int DustType => 108;
    }

    public class EutrophicSandBallGun : SandBall
    {
        public override string Texture => "CalamityMod/Projectiles/Typeless/SandBallEutrophic";
        public override int TileType => ModContent.TileType<EutrophicSand>();
        public override int ItemType => ModContent.ItemType<Items.Placeables.EutrophicSand>();
        public override int DustType => 108; // Weirdly same dusts as Astral
    }

    public class SulphurousSandBallGun : SandBall
    {
        public override string Texture => "CalamityMod/Projectiles/Typeless/SandBallSulphurous";
        public override int TileType => ModContent.TileType<SulphurousSand>();
        public override int ItemType => ModContent.ItemType<Items.Placeables.SulphurousSand>();
        // Uses normal sand dust
    }

    // All the setups go here to prevent mass blocks of copypasting
    public abstract class SandBall : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";

        // Whether or not it is a fired projectile
        public virtual bool Fired => true;
        // Associated tile type
        public virtual int TileType => TileID.Sand;
        // Associated item type
        public virtual int ItemType => ItemID.SandBlock;
        // Associated dust type
        public virtual int DustType => DustID.Sand;

        public override void SetDefaults()
        {
            Projectile.knockBack = 6f;
            Projectile.width = Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.penetrate = -1;

            if (Fired)
            {
                Projectile.MaxUpdates = 2;
                Projectile.DamageType = DamageClass.Ranged;
            }
            else
                Projectile.hostile = true;
        }

        // Using clones will not allow for custom dust types sadly
        public override void AI()
        {
            if (Main.rand.NextBool())
            {
                int i = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType);
                Main.dust[i].velocity.X *= 0.4f;
                Main.dust[i].velocity.Y += Fired ? 0f : Projectile.velocity.Y * 0.5f;
            }

            Projectile.ai[1]++;
            Projectile.rotation += 0.1f;
            if (Projectile.ai[1] >= 60f || !Fired)
            {
                Projectile.ai[1] = 60f;
                Projectile.velocity.Y += 0.2f;
            }
            if (Projectile.velocity.Y > 10f)
                Projectile.velocity.Y = 10f;

            Point p = Projectile.Center.ToTileCoordinates();
            // Don't check out of bounds
            if (p.X < 0 || p.X >= Main.maxTilesX || p.Y < 0 || p.Y >= Main.maxTilesY)
                return;
            Tile placer = Main.tile[p.X, p.Y + 1];
            if (placer.HasTile && TileID.Sets.Platforms[placer.TileType] && Projectile.ai[1] >= 60f)
                Projectile.Kill();
        }

        public override void OnKill(int timeLeft)
        {
            Point p = Projectile.Center.ToTileCoordinates();
            // If the sand is dying outside the world border, cancel placing sand.
            if (p.X < 0 || p.X >= Main.maxTilesX || p.Y < 0 || p.Y >= Main.maxTilesY)
                return;
            Tile placer = Main.tile[p.X, p.Y];

            // If the sand hit a half brick, but was mostly going downwards (at a lower than 45 degree angle), then stack atop the half brick.
            if (placer.IsHalfBlock && Projectile.velocity.Y > 0f && Math.Abs(Projectile.velocity.Y) > Math.Abs(Projectile.velocity.X))
                placer = Main.tile[p.X, --p.Y];

            bool ValidTileBelow = true;
            bool SlopeTileBelow = false;

            // Attempt to place sand and unslope tile below if available
            // Under no circumstances can falling sand destroy minecart tracks.
            if (!placer.HasTile && placer.TileType != TileID.MinecartTrack)
            {
                if (p.Y + 1 < Main.maxTilesY)
                {
                    Tile under = Main.tile[p.X, p.Y + 1];
                    if (under.HasTile)
                    {
                        if (under.TileType == TileID.MinecartTrack)
                            ValidTileBelow = false;
                        else if (under.IsHalfBlock || under.Slope != 0)
                            SlopeTileBelow = true;
                    }
                }

                if (ValidTileBelow)
                {
                    bool PlacedBlock = WorldGen.PlaceTile(p.X, p.Y, TileType, false, true);
                    WorldGen.SquareTileFrame(p.X, p.Y);

                    if (PlacedBlock && SlopeTileBelow)
                    {
                        WorldGen.SlopeTile(p.X, p.Y + 1);
                        if (Main.netMode != NetmodeID.SinglePlayer)
                            NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 14, p.X, p.Y + 1);
                    }
                    if (PlacedBlock && Main.netMode != NetmodeID.SinglePlayer)
                        NetMessage.SendData(MessageID.TileManipulation, -1, -1, null, 1, p.X, p.Y, TileType);
                }
            }
            // Give the block back if you literally can't place it
            else
                Item.NewItem(Projectile.GetSource_DropAsItem(), Projectile.position, Projectile.width, Projectile.height, ItemType);
        }
    }
}
