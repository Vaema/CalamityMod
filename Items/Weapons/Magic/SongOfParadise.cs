using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Items.BaseItems;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Ranged;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    public class SongOfParadise : CustomUseProjItem, ILocalizedModType
    {
        public bool RetunedToMelody = false;
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override LocalizedText Tooltip => null;
        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine tt = new TooltipLine(CalamityMod.Instance, "Melody", Language.GetTextValue("Mods.CalamityMod.Items.Weapons.Magic.SongOfParadise.Tooltip" + (RetunedToMelody ? "2" : "")));

            tooltips.Add(tt);
        }
        public override void SetDefaults()
        {
            Item.noUseGraphic = true;
            Item.damage = 28;
            Item.DamageType = DamageClass.Magic;
            Item.width = 38;
            Item.height = 68;
            Item.useTime = 48;
            Item.useAnimation = 48;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.noMelee = true;
            Item.channel = true;
            Item.knockBack = 3f;
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<SongOfParadiseHoldout>();
            Item.scale = 1f;
        }
        public override bool AltFunctionUse(Player player)
        {
            return true;
        }
        public override bool CanRightClick()
        {
            return true;
        }
        public override bool CanShoot(Player player)
        {
            if (player.altFunctionUse != 2)
            {
                return base.CanShoot(player);
            }
            else
            {
                if (player.ItemAnimationJustStarted)
                {
                    Retune();
                    player.itemAnimation = 0;
                }
                return false;
            }
        }
        public override void RightClick(Player player)
        {
            Item.stack++;

            Retune();
        }
        public void Retune()
        {
            if (!Main.dedServ)
            {
                SoundEngine.PlaySound(SongOfParadiseHoldout.WaveSound.WithPitchOffset(Main.rand.NextFloat(0.5f, 0.8f)));
            }

            if (RetunedToMelody) RetunedToMelody = false;
            else RetunedToMelody = true;
        }
    }
}
