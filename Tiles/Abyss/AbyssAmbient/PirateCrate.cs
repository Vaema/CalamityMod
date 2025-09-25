using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Abyss.AbyssAmbient
{
    //
    // Pirate Crate With Glow

    public abstract class PirateCrateExplosive : GlowMaskTile
    {
        public abstract string GoreKey { get; }

        public static readonly SoundStyle MineSound = new("CalamityMod/Sounds/Custom/CrateBreak", 3) { Volume = 0.8f };
        public override void SetupStatic()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileWaterDeath[Type] = false;
            Main.tileNoAttach[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(97, 69, 52), CalamityUtils.GetText("Tiles.PirateCrate"));
            DustType = DustID.WoodFurniture;
            HitSound = MineSound;
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            //explode when broken (troll!)
            //also instakill players in for the worthy
            Vector2 spawnPosition = new(i * 16f + 24f, j * 16f - 4f);
            // NOTE: The damage of ProjectileID.Explosives is reduced in CalamityPlayerHitHurt, this actually deals 105 / 210 / 315 damage.
            int blastDamage = (Main.getGoodWorld ? 99999 : 150) * (Main.masterMode ? 3 : Main.expertMode ? 2 : 1);
            Projectile.NewProjectile(new EntitySource_WorldEvent(), spawnPosition.X, spawnPosition.Y, 0, 0, ProjectileID.Explosives, blastDamage, 0f);

            if (Main.dedServ)
                return;

            Gore.NewGore(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16f, Main.rand.NextVector2CircularEdge(3f, 3f), Mod.Find<ModGore>($"{GoreKey}1").Type);
            Gore.NewGore(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16f, Main.rand.NextVector2CircularEdge(3f, 3f), Mod.Find<ModGore>($"{GoreKey}2").Type);
            Gore.NewGore(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16f, Main.rand.NextVector2CircularEdge(3f, 3f), Mod.Find<ModGore>($"{GoreKey}3").Type);
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 2;
        }

        public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
        {
            return Color.White;
        }
    }

    public class PirateCrate1 : PirateCrateExplosive
    {
        public override string GlowMaskAsset => "CalamityMod/Tiles/Abyss/AbyssAmbient/PirateCrate1Glow";
        public override string GoreKey => "PirateCrateL";
    }

    public class PirateCrate2 : PirateCrateExplosive
    {
        public override string GlowMaskAsset => "CalamityMod/Tiles/Abyss/AbyssAmbient/PirateCrate2Glow";
        public override string GoreKey => "PirateCrateM";
    }

    public class PirateCrate3 : PirateCrateExplosive
    {
        public override string GlowMaskAsset => "CalamityMod/Tiles/Abyss/AbyssAmbient/PirateCrate3Glow";
        public override string GoreKey => "PirateCrateR";
    }

    //
    // Pirate Crate Without Glow

    public abstract class PirateCrateGold : ModTile
    {
        public abstract string GoreKey { get; }

        public static readonly SoundStyle MineSound = new("CalamityMod/Sounds/Custom/CrateBreak", 3) { Volume = 0.8f };
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileWaterDeath[Type] = false;
            Main.tileNoAttach[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(97, 69, 52), CalamityUtils.GetText("Tiles.PirateCrate"));
            DustType = DustID.WoodFurniture;
            HitSound = MineSound;

            base.SetStaticDefaults();
        }

        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            if (Main.rand.NextBool())
                Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, ItemID.GoldCoin, Main.rand.Next(1, 2));

            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 32, ItemID.SilverCoin, Main.rand.Next(45, 75));

            if (Main.dedServ)
                return;

            Gore.NewGore(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16f, Main.rand.NextVector2CircularEdge(3f, 3f), Mod.Find<ModGore>($"{GoreKey}1").Type);
            Gore.NewGore(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16f, Main.rand.NextVector2CircularEdge(3f, 3f), Mod.Find<ModGore>($"{GoreKey}2").Type);
            Gore.NewGore(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16f, Main.rand.NextVector2CircularEdge(3f, 3f), Mod.Find<ModGore>($"{GoreKey}3").Type);
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 2;
        }
    }

    public class PirateCrate4 : PirateCrateGold
    {
        public override string GoreKey => "PirateCrateL";
    }

    public class PirateCrate5 : PirateCrateGold
    {
        public override string GoreKey => "PirateCrateM";
    }

    public class PirateCrate6 : PirateCrateGold
    {
        public override string GoreKey => "PirateCrateR";
    }
}
