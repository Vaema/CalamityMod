using CalamityMod.Events;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Furniture.CraftingStations;
using CalamityMod.Items.Potions.Food;
using CalamityMod.Items.SummonItems;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.TownNPCs;
using CalamityMod.Projectiles.Boss;
using CalamityMod.Projectiles.Typeless;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Furniture.CraftingStations
{
    public class SCalAltar : ModTile
    {
        public static readonly SoundStyle SummonSound = new("CalamityMod/Sounds/Custom/SCalAltarSummon");

        public const int Width = 4;
        public const int Height = 3;
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;

            // Various data sets to protect this tile from unintentional death
            TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
            TileID.Sets.PreventsTileHammeringIfOnTopOfIt[Type] = true;
            TileID.Sets.PreventsSandfall[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Width = 4;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.Origin = new Point16(1, 2);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(43, 19, 42), CalamityUtils.GetItemName<AltarOfTheAccursedItem>());
            TileID.Sets.DisableSmartCursor[Type] = true;

            // This cannot be placed and only exists for backwards compatibility, so item has to be returned to the player manually if broken.
            RegisterItemDrop(ModContent.ItemType<AltarOfTheAccursedItem>());
        }

        public override bool CanExplode(int i, int j) => false;

        public override bool CreateDust(int i, int j, ref int type)
        {
            // Red torch dust.
            type = 60;
            return true;
        }

        public override bool RightClick(int i, int j) => AttemptToSummonSCal(i, j);
        public override void MouseOver(int i, int j) => HoverItemIcon(i, j);
        public override void MouseOverFar(int i, int j) => HoverItemIcon(i, j);
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.type == ModContent.ProjectileType<SCalAltarArenaVisual>())
                {
                    p.Kill();
                    break;
                }
            }
        }

        public static void HoverItemIcon(int i, int j)
        {
            bool vodka = Main.LocalPlayer.HeldItem.type == ModContent.ItemType<DeliciousMeat>() && Main.zenithWorld;
            if (vodka)
                Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<DeliciousMeat>();
            else if (Main.LocalPlayer.HasItem(ModContent.ItemType<CeremonialUrn>()))
                Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<CeremonialUrn>();
            else
                Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<AshesofCalamity>();

            Main.LocalPlayer.noThrow = 2;
            Main.LocalPlayer.cursorItemIconEnabled = true;

            // Checks if the player has the Ruler lines or Ruler grid toggled
            if (Main.LocalPlayer.builderAccStatus[0] == 0 || (Main.LocalPlayer.builderAccStatus[1] == 0 && Main.LocalPlayer.rulerGrid))
            {
                // Don't spawn the arena visual if one already exists or if SCal is alive or spawning
                if (CalamityUtils.AnyProjectiles(ModContent.ProjectileType<SCalAltarArenaVisual>()) ||
                    CalamityUtils.AnyProjectiles(ModContent.ProjectileType<SCalRitualDrama>()) ||
                    NPC.AnyNPCs(ModContent.NPCType<SupremeCalamitas>()))
                    return;

                Tile t = Main.tile[i, j];
                Vector2 arenaCenter = new Vector2(i - t.TileFrameX / 18 + Width / 2, j - t.TileFrameY / 18).ToWorldCoordinates() - Vector2.UnitY * 24f;
                Projectile.NewProjectile(new EntitySource_WorldEvent(), arenaCenter, Vector2.Zero, ModContent.ProjectileType<SCalAltarArenaVisual>(), 0, 0f, Main.myPlayer, CalamityWorld.death.ToInt());
            }
        }

        public static bool AttemptToSummonSCal(int i, int j)
        {
            if (!Main.LocalPlayer.HasItem(ModContent.ItemType<AshesofCalamity>()) &&
                !Main.LocalPlayer.HasItem(ModContent.ItemType<CeremonialUrn>()) && !(Main.LocalPlayer.HeldItem.type == ModContent.ItemType<DeliciousMeat>() && Main.zenithWorld))
            {
                return true;
            }

            bool meat = Main.LocalPlayer.HeldItem.type == ModContent.ItemType<DeliciousMeat>() && Main.zenithWorld;

            if (NPC.AnyNPCs(ModContent.NPCType<SupremeCalamitas>()) || BossRushEvent.BossRushActive)
                return true;

            if (CalamityUtils.CountProjectiles(ModContent.ProjectileType<SCalRitualDrama>()) > 0)
                return true;

            bool usingSpecialItem = Main.LocalPlayer.HasItem(ModContent.ItemType<CeremonialUrn>());

            Tile tile = Main.tile[i, j];
            int left = i - tile.TileFrameX / 18;
            int top = j - tile.TileFrameY / 18;
            Vector2 ritualSpawnPosition = new Vector2(left + Width / 2, top).ToWorldCoordinates();
            ritualSpawnPosition += new Vector2(0f, -24f);

            SoundEngine.PlaySound(SummonSound, ritualSpawnPosition);
            Projectile.NewProjectile(new EntitySource_WorldEvent(), ritualSpawnPosition, Vector2.Zero, ModContent.ProjectileType<SCalRitualDrama>(), 0, 0f, Main.myPlayer, 0, meat.ToInt());

            if (meat)
            {
                Main.LocalPlayer.ConsumeItem(ModContent.ItemType<DeliciousMeat>(), true);
                foreach (NPC n in Main.ActiveNPCs)
                {
                    if (n.type == ModContent.NPCType<Archmage>())
                        n.active = false;
                }
            }
            else if (!usingSpecialItem)
            {
                Main.LocalPlayer.ConsumeItem(ModContent.ItemType<AshesofCalamity>(), true);
            }
            return true;
        }
    }

    public class SCalAltarLarge : ModTile
    {
        public const int Width = 5;
        public const int Height = 3;
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLavaDeath[Type] = false;

            // Various data sets to protect this tile from unintentional death
            TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
            TileID.Sets.PreventsTileHammeringIfOnTopOfIt[Type] = true;
            TileID.Sets.PreventsSandfall[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.Width = 5;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.Origin = new Point16(1, 2);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.newTile.StyleHorizontal = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(43, 19, 42), CalamityUtils.GetItemName<AltarOfTheAccursedItem>());
            TileID.Sets.DisableSmartCursor[Type] = true;
            AdjTiles = new int[] { ModContent.TileType<SCalAltar>() };
        }

        public override bool CanExplode(int i, int j) => false;

        public override bool CreateDust(int i, int j, ref int type)
        {
            // Red torch dust.
            type = 60;
            return true;
        }

        public override bool RightClick(int i, int j) => SCalAltar.AttemptToSummonSCal(i, j);
        public override void MouseOver(int i, int j) => SCalAltar.HoverItemIcon(i, j);
        public override void MouseOverFar(int i, int j) => SCalAltar.HoverItemIcon(i, j);
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            foreach (Projectile p in Main.ActiveProjectiles)
            {
                if (p.type == ModContent.ProjectileType<SCalAltarArenaVisual>())
                {
                    p.Kill();
                    break;
                }
            }
        }
    }
}
