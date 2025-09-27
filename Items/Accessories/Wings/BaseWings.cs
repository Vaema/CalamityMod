using System.Collections.Generic;
using System.Linq;
using System.Text;
using CalamityMod.Balancing;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories.Wings
{
    public abstract class BaseWings : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories.Wings";

        /// <summary>
        /// Bonus vertical acceleration per frame while player velocity is below 0.<br/>
        /// Defaults to 0.5f.
        /// </summary>
        public virtual float BonusAscentWhileFalling => 0.5f;

        /// <summary>
        /// Bonus vertical acceleration per frame while player velocity is below a velocity threshold determined by RisingSpeedThreshold.<br/>
        /// Defaults to 0.1f.
        /// </summary>
        public virtual float BonusAscentWhileRising => 0.1f;

        /// <summary>
        /// Vertical velocity threshold for activating bonus acceleration from BonusAscentWhileRising when multiplied by the player's jump speed.<br/>
        /// Defaults to 0.5f.
        /// </summary>
        public virtual float RisingSpeedThreshold => 0.5f;

        /// <summary>
        /// Max vertical velocity threshold when multiplied by the player's jump speed.<br/>
        /// Defaults to 1.5f.
        /// </summary>
        public virtual float MaxAscentSpeed => 1.5f;

        /// <summary>
        /// Base vertical acceleration per frame.<br/>
        /// Defaults to 0.1f.
        /// </summary>
        public virtual float BaseAscent => 0.1f;

        public override void SetDefaults()
        {
            Item.accessory = true;
        }

        public override void VerticalWingSpeeds(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend)
        {
            if (Item.wingSlot == -1) return;
            ascentWhenFalling = BonusAscentWhileFalling;
            ascentWhenRising = BonusAscentWhileRising;
            maxCanAscendMultiplier = RisingSpeedThreshold;
            maxAscentMultiplier = MaxAscentSpeed;
            constantAscend = BaseAscent;

            AdditionalFlightMovement(player, ref ascentWhenFalling, ref ascentWhenRising, ref maxCanAscendMultiplier, ref maxAscentMultiplier, ref constantAscend);
        }

        /// <summary>
        /// Addition for any deviations in regular wing movement.<br/>
        /// This is typically for UP boost or hovers.
        /// </summary>
        public virtual void AdditionalFlightMovement(Player player, ref float ascentWhenFalling, ref float ascentWhenRising, ref float maxCanAscendMultiplier, ref float maxAscentMultiplier, ref float constantAscend) { }

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            if (Item.wingSlot == -1) return;
            WingStats stats = new();
            if (Item.type == ModContent.ItemType<TracersCelestial>())
                stats = ArmorIDs.Wing.Sets.Stats[TracersCelestial.wingSlot];
            else if (Item.type == ModContent.ItemType<TracersElysian>())
                stats = ArmorIDs.Wing.Sets.Stats[TracersElysian.wingSlot];
            else if (Item.type == ModContent.ItemType<TracersSeraph>())
                stats = ArmorIDs.Wing.Sets.Stats[TracersSeraph.wingSlot];
            else stats = ArmorIDs.Wing.Sets.Stats[Item.wingSlot];
            int time = stats.FlyTime;
            float run = stats.AccRunSpeedOverride;
            float rAcc = stats.AccRunAccelerationMult * 0.08f;
            bool hover = stats.HasDownHoverStats;
            float hSpeed = stats.DownHoverSpeedOverride;
            float hAcc = stats.DownHoverAccelerationMult * 0.08f;
            float baseJumpSpeed = (CalamityServerConfig.Instance.FasterJumpSpeed ? BalancingConstants.ConfigBoostedBaseJumpSpeed : 5.01f) + 1f;
            StringBuilder sb = new StringBuilder(512);
            sb.Append('\n');
            sb.Append(CalamityUtils.GetText($"Common.WingStats").Format(time.FramesToSeconds(), run.ToMph(), (MaxAscentSpeed * baseJumpSpeed).ToMph()));
            sb.Append('\n');
            if (Main.keyState.PressingShift())
            {
                sb.Append(CalamityUtils.GetText($"Common.WingStatsAcceleration").Format(rAcc.ToMphps(), BaseAscent.ToMphps(),
                (BaseAscent + BonusAscentWhileRising).ToMphps(), (RisingSpeedThreshold * baseJumpSpeed).ToMph(),
                (BaseAscent + BonusAscentWhileFalling).ToMphps()));
                if (hover)
                {
                    sb.Append('\n');
                    sb.Append(CalamityUtils.GetText($"Common.WingStatsHover").Format(hSpeed.ToMph(), hAcc.ToMphps()));
                }
            }
            else
                sb.Append($"[c/B8B8B8:{CalamityUtils.GetTextValue("UI.HoldShiftTooltipExtensionIndicator")}]");

            // Add stats below the common "Allows flight" line
            var wingTooltip = list.FirstOrDefault(x => x.Name == "Tooltip0" && x.Mod == "Terraria");
            if (wingTooltip != null)
                wingTooltip.Text += sb.ToString();
        }
    }
}
