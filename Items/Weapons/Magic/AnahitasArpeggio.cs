using System.Collections.Generic;
using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    [LegacyName("SirensSong")]
    public class AnahitasArpeggio : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public float RotationOffset;
        public static int MusicNoteAmt = 0;

        public override void SetDefaults()
        {
            Item.width = 56;
            Item.height = 50;
            Item.damage = 77;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 7;
            Item.useTime = 20;
            Item.useAnimation = 20;
            Item.attackSpeedOnlyAffectsWeaponAnimation = true;
            Item.useStyle = ItemUseStyleID.Guitar;
            Item.channel = true;
            Item.noMelee = true;
            Item.knockBack = 6.5f;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<AnahitasArpeggioNote>();
            Item.shootSpeed = 13f;
        }

        public override bool CanUseItem(Player player) => player.Calamity().arpeggioCooldown <= 0;

        public override bool? UseItem(Player player)
        {
            // I FUCKING HATE ATTACK SPEED MULTIPLIERS
            // Setting ItemID.Sets.BonusAttackSpeedMultiplier to 0f did not work
            // Setting Item.attackSpeedOnlyAffectsWeaponAnimation to true did not work
            // Using UseSpeedMultiplier to force it to 1f did not work
            // Even making an ENTIRE FUCKING DAMAGE CLASS that was just magic damage with no attack speed inheritance did not work
            // So yes, I literally have to force its use time to be set to a certain value to work properly
            // Good fucking lord what is wrong with this game.
            if (Item.useTime != 20)
            {
                Item.useTime = 20;
                Item.useAnimation = 20;
            }
            return base.UseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Max music note check is in Shoot instead of CanUseItem so that the weapon can still be visually played while at the cap
            int musicNoteCap = Main.zenithWorld ? 7 : 6;
            if (MusicNoteAmt >= musicNoteCap)
            {
                Main.musicPitch = -0.5f;
                SoundEngine.PlaySound(SoundID.Item26 with { Volume = 0.8f }, player.Center);
                return false;
            }
            else
            {
                if (MusicNoteAmt <= 0)
                    RotationOffset = Main.rand.NextFloat(0f, MathHelper.TwoPi);

                int note = Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI, 0f, 0f, RotationOffset);
                Main.projectile[note].localAI[1] = MusicNoteAmt;
                MusicNoteAmt++;
                return false;
            }
        }

        public override void UseStyle(Player player, Rectangle heldItemFrame)
        {
            player.itemLocation.X -= 15f * player.direction;
            player.itemLocation.Y += 15f * player.gravDir;
        }

        // Consume much less mana while the maximum number of notes are present
        public override void ModifyManaCost(Player player, ref float reduce, ref float mult)
        {
            if (MusicNoteAmt >= 6)
                mult *= 0.25f;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            var line = tooltips.FirstOrDefault(x => x.Text.Contains("[GFB]") && x.Mod == "Terraria");
            if (line != null)
            {
                line.Text = Lang.SupportGlyphs(this.GetLocalizedValue(Main.zenithWorld ? "TooltipGFB" : "TooltipNormal"));
                if (Main.zenithWorld)
                    line.OverrideColor = Main.DiscoColor;
            }
        }
    }
}
