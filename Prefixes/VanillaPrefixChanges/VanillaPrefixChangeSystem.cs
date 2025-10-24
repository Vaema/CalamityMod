using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
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
