using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.Prefixes;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Tools
{
    public static class VoucherReforgeSystem
    {
        public static int ForcedPrefix = -1;
        public static bool RollWon = false;
    }

    public class VoucherGlobalItem : GlobalItem
    {
        public override void PreReforge(Item item)
        {
            Player player = Main.LocalPlayer;
            bool isAccessory = item.accessory;

            VoucherReforgeSystem.ForcedPrefix = -1;

            for (int i = 0; i < player.inventory.Length; i++)
            {
                Item invItem = player.inventory[i];

                if (invItem.ModItem is VoucherItem voucher)
                {
                    if (voucher.IsUsableVoucher(player, item, isAccessory))
                    {
                        VoucherReforgeSystem.ForcedPrefix = voucher.GetPrefixForItem(player, item, isAccessory);

                        invItem.stack--;
                        if (invItem.stack <= 0)
                            invItem.TurnToAir();

                        if (Main.netMode != NetmodeID.Server)
                        {
                            if (VoucherReforgeSystem.RollWon)
                            {
                                for (int d = 0; d < 24; d++)
                                {
                                    int dust = Dust.NewDust(player.position, player.width, player.height, DustID.Confetti, 0f, 0f, 100, default, 1f);
                                    Main.dust[dust].velocity *= 2f;
                                    Main.dust[dust].noGravity = false;
                                }

                                SoundEngine.PlaySound(SoundID.Item4, player.position);
                            }
                            else
                            {
                                for (int d = 0; d < 20; d++)
                                {
                                    int dust = Dust.NewDust(player.position, player.width, player.height, DustID.Smoke, 0f, 0f, 100, default, 1.2f);
                                    Main.dust[dust].velocity *= 1.5f;
                                }

                                SoundEngine.PlaySound(SoundID.Item16, player.position);
                            }
                        }
                        break;
                    }
                }
            }
        }
    }

    public abstract class VoucherItem : ModItem
    {
        #region CyclePrefixes
        protected virtual int[] prefixChoices => Array.Empty<int>();
        protected int chosenPrefix = 0;
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            string selected = GetSelectedPrefixName();
            string all = GetAllPrefixNames();

            foreach (var line in tooltips)
            {
                if (line.Mod == "Terraria" && line.Name.StartsWith("Tooltip"))
                {
                    line.Text = line.Text
                        .Replace("{0}", selected)
                        .Replace("{1}", all);
                }
            }
        }

        public string GetAllPrefixNames()
        {
            var pool = prefixChoices;
            if (pool.Length == 0)
                return null;

            List<string> names = new();

            foreach (int p in pool)
            {
                if (p >= 0 && p < Lang.prefix.Length && Lang.prefix[p] != null)
                    names.Add(Lang.prefix[p].Value);
            }

            return string.Join(", ", names);
        }

        public string GetSelectedPrefixName()
        {
            var pool = prefixChoices;
            if (pool.Length == 0)
                return null;

            int p = pool[chosenPrefix];

            if (p >= 0 && p < Lang.prefix.Length && Lang.prefix[p] != null)
                return Lang.prefix[p].Value;

            return null;
        }
        #endregion

        public bool IsUsableVoucher(Player player, Item itemBeingReforged, bool isAccessory)
        {
            return Item.favorited && CanApply(player, itemBeingReforged, isAccessory);
        }

        public override bool CanRightClick() => Main.keyState.PressingShift();

        public override void RightClick(Player player)
        {
            if (prefixChoices.Length == 0)
                return;

            chosenPrefix++;
            if (chosenPrefix >= prefixChoices.Length)
                chosenPrefix = 0;

            Item.NetStateChanged();
        }

        public override bool ConsumeItem(Player player) => false;

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(chosenPrefix);
        }

        public override void NetReceive(BinaryReader reader)
        {
            chosenPrefix = reader.ReadInt32();
        }

        public virtual bool CanApply(Player player, Item itemBeingReforged, bool isAccessory)
        {
            return true;
        }

        public virtual int GetPrefixForItem(Player player, Item itemBeingReforged, bool isAccessory)
        {
            if (prefixChoices.Length == 0)
                return -1;

            if (isAccessory)
            {
                if (Main.rand.NextBool())
                {
                    VoucherReforgeSystem.RollWon = true;
                    return prefixChoices[chosenPrefix];
                }
                else
                {
                    VoucherReforgeSystem.RollWon = false;
                    return ModContent.PrefixType<Friendly>();
                }
            }

            VoucherReforgeSystem.RollWon = true;
            return prefixChoices[chosenPrefix];
        }
    }

    public class CombatVoucher : VoucherItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Tools";

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 20;
            Item.rare = ItemRarityID.Green;
            Item.value = 0;
        }

        public override bool CanApply(Player player, Item itemBeingReforged, bool isAccessory)
        {
            return !isAccessory;
        }

        public override int GetPrefixForItem(Player player, Item itemBeingReforged, bool isAccessory)
        {
            int prefix;

            if (itemBeingReforged.DamageType == DamageClass.Melee)
                prefix = PrefixID.Legendary;
            else if (itemBeingReforged.DamageType == DamageClass.Ranged)
                prefix = PrefixID.Unreal;
            else if (itemBeingReforged.DamageType == DamageClass.Magic)
                prefix = PrefixID.Mythical;
            else if (itemBeingReforged.DamageType == DamageClass.Summon)
                prefix = PrefixID.Ruthless;
            else
                prefix = PrefixID.Godly;

            if (Main.rand.NextBool())
            {
                VoucherReforgeSystem.RollWon = true; 
                return prefix;
            }
            else
            {
                VoucherReforgeSystem.RollWon = false;
                return ModContent.PrefixType<Horrible>();
            }
        }
    }

    public class AggressiveVoucher : VoucherItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Tools";

        protected override int[] prefixChoices => new int[]
        {
            PrefixID.Jagged,
            PrefixID.Angry,
            PrefixID.Spiked,
            PrefixID.Menacing,
            PrefixID.Precise,
            PrefixID.Lucky
        };

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 20;
            Item.rare = ItemRarityID.Green;
            Item.value = 0;
        }

        public override bool CanApply(Player player, Item itemBeingReforged, bool isAccessory)
        {
            return isAccessory;
        }
    }

    public class UnbreakableVoucher : VoucherItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Tools";

        protected override int[] prefixChoices => new int[]
        {
            PrefixID.Hard,
            PrefixID.Armored,
            PrefixID.Guarding,
            PrefixID.Warding,
            ModContent.PrefixType<Invigorating>(),
            ModContent.PrefixType<Dauntless>()
        };

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 20;
            Item.rare = ItemRarityID.Green;
            Item.value = 0;
        }

        public override bool CanApply(Player player, Item itemBeingReforged, bool isAccessory)
        {
            return isAccessory;
        }
    }

    public class HurriedVoucher : VoucherItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Tools";

        protected override int[] prefixChoices => new int[]
        {
            PrefixID.Brisk,
            PrefixID.Fleeting,
            PrefixID.Hasty,
            PrefixID.Quick
        };

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 20;
            Item.rare = ItemRarityID.Green;
            Item.value = 0;
        }

        public override bool CanApply(Player player, Item itemBeingReforged, bool isAccessory)
        {
            return isAccessory;
        }
    }

    public class OddVoucher : VoucherItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Tools";

        protected override int[] prefixChoices => new int[]
        {
            PrefixID.Arcane,
            PrefixID.Wild,
            PrefixID.Rash,
            PrefixID.Intrepid,
            PrefixID.Violent,
            ModContent.PrefixType<Silent>()
        };

        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 20;
            Item.rare = ItemRarityID.Green;
            Item.value = 0;
        }

        public override bool CanApply(Player player, Item itemBeingReforged, bool isAccessory)
        {
            return isAccessory;
        }
    }
}
