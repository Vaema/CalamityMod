using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.SunkenSea.WhisperingPearls;
using CalamityMod.TileEntities;
using CalamityMod.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.SunkenSea
{
    public class WhisperingPearl : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileWaterDeath[Type] = false;

            TileID.Sets.HasOutlines[Type] = true;
            TileID.Sets.DisableSmartCursor[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.WaterDeath = false;
            TileObjectData.newTile.WaterPlacement = LiquidPlacement.Allowed;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.LavaPlacement = LiquidPlacement.NotAllowed;
            TileObjectData.addTile(Type);

            AddMapEntry(Color.LightCyan, CalamityUtils.GetText("Tiles.WhisperingPearl"));

            DustType = DustID.BlueCrystalShard;
            HitSound = SoundID.Item27;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0f;
            g = 0.3f;
            b = 0.3f;
        }

        public override bool RightClick(int i, int j)
        {
            int frameNum = Main.tile[i, j].TileFrameX / 18;
            string key = frameNum switch
            {
                1 => "Red",
                2 => "Orange",
                3 => "Yellow",
                4 => "LightGreen",
                5 => "LightBlue",
                6 => "DarkBlue",
                7 => "Purple",
                8 => "DarkGreen",
                9 => "Black",
                _ => "Basic"
            };
            if (!WhisperingPearlUI.IsActive)
            {
                WhisperingPearlUI.StartDialogue(new Vector2(i, j) * 16, key);
            }
            else
            {
                WhisperingPearlUI.ProgressDialogue(new Vector2(i, j) * 16, key);
            }
            return true;
        }
        public override bool HasSmartInteract(int i, int j, Terraria.GameContent.ObjectInteractions.SmartInteractScanSettings settings) => true;

        public override void MouseOver(int i, int j)
        {
            int frameNum = Main.tile[i, j].TileFrameX / 18;
            int pearl = frameNum switch
            {
                1 => ModContent.ItemType<WhisperingPearlRed>(),
                2 => ModContent.ItemType<WhisperingPearlOrange>(),
                3 => ModContent.ItemType<WhisperingPearlYellow>(),
                4 => ModContent.ItemType<WhisperingPearlLightGreen>(),
                5 => ModContent.ItemType<WhisperingPearlLightBlue>(),
                6 => ModContent.ItemType<WhisperingPearlDarkBlue>(),
                7 => ModContent.ItemType<WhisperingPearlPurple>(),
                8 => ModContent.ItemType<WhisperingPearlDarkGreen>(),
                9 => ModContent.ItemType<WhisperingPearlBlack>(),
                _ => ModContent.ItemType<Items.Placeables.SunkenSea.WhisperingPearls.WhisperingPearl>(),
            };
            Main.LocalPlayer.cursorItemIconID = pearl;
            Main.LocalPlayer.noThrow = 2;
            Main.LocalPlayer.cursorItemIconEnabled = true;
        }
    }
}
