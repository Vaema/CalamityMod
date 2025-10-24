using System;
using System.Collections.Generic;
using System.Linq;
using MonoMod.Cil;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Prefixes.VanillaPrefixChanges
{
    public sealed class VanillaPrefixChangeSystem : ModSystem
    {
        public static bool PrefixReworkEnabled = true;

        public static readonly Dictionary<int, VanillaPrefixChange> PrefixChanges = [];

        public override void Load()
        {
            ReflectionHelper.IterateCalamityTypes<VanillaPrefixChange>(includeBaseType: false, type =>
            {
                var changeToAdd = (VanillaPrefixChange)Activator.CreateInstance(type);
                PrefixChanges[changeToAdd.TargetPrefix] = changeToAdd;
            });

            On_Player.GrantPrefixBenefits += OnGrantBenefits;
            IL_Item.Prefix += IL_Item_Prefix;
        }

        private void IL_Item_Prefix(ILContext il)
        {
            var cursor = new ILCursor(il);
            if (!cursor.TryGotoNext(x => x.MatchCallOrCallvirt<ModPrefix>(nameof(ModPrefix.ModifyValue))))
            {
                return;
            }

            if (!cursor.Prev.MatchLdloca(out var multLocaIdx))
            {
                return;
            }

            if (!cursor.TryGotoPrev(x => x.MatchLdsfld<PrefixID>(nameof(PrefixID.Count))))
            {
                return;
            }

            if (!cursor.Prev.MatchLdloc(out var prefixLocaIdx))
            {
                return;
            }

            cursor.GotoPrev(MoveType.AfterLabel);
            cursor.EmitLdloc(prefixLocaIdx);
            cursor.EmitLdloca(multLocaIdx);
            cursor.EmitDelegate((int prefixID, ref float value) =>
            {
                if (!PrefixReworkEnabled)
                    return;

                if (PrefixChanges.TryGetValue(prefixID, out var prefixChange))
                {
                    prefixChange.ModifyValue(ref value);
                }
            });
        }

        private void OnGrantBenefits(On_Player.orig_GrantPrefixBenefits orig, Player self, Item item)
        {
            if (!PrefixReworkEnabled)
            {
                orig(self, item);
            }
            else if (PrefixChanges.TryGetValue(item.prefix, out var prefixChange))
            {
                var stats = prefixChange.PopulateStats();
                while (stats.MoveNext())
                {
                    var stat = stats.Current;
                    stat.ApplyEffects(self);
                }
                prefixChange.PostApplyEffects(self);
            }
            else
            {
                PrefixLoader.ApplyAccessoryEffects(self, item);
            }
        }

        public sealed class VanillaPrefixChangeTooltipModify : GlobalItem
        {
            public override bool InstancePerEntity => false;

            public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
            {
                if (!PrefixReworkEnabled)
                    return;

                if (!PrefixChanges.TryGetValue(item.prefix, out var change))
                    return;

                var tooltip = tooltips.FirstOrDefault(x => x.Name.Equals(change.TargetTooltipName));
                if (tooltip == null)
                    return;

                tooltip.Text = string.Empty;

                var stats = change.PopulateStats();
                while (stats.MoveNext())
                {
                    var stat = stats.Current;
                    stat.ModifyTooltip(tooltip);
                }
                change.PostModifyTooltip(tooltip);
                tooltip.Text = tooltip.Text.Trim();
            }
        }
    }
}
