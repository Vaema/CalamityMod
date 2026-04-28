using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions
{
    public class TheElixir : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(159, 67, 199),
                new Color(176, 147, 243),
                new Color(84, 50, 185)
            };
        }

        public override void SetDefaults()
        {
            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.Blue;
            Item.DefaultToFood(28, 51, 0, 0, true);
        }

        //The player can't use The Elixir if they have Chaos State. 
        //This is to ensure if that it fails, they can't simply drink another to escape 
        public override bool CanUseItem(Player player)
        {
            //if (player.HasBuff(BuffID.ChaosState))
                //return false;
            return base.CanUseItem(player);
        }

        public override bool? UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                if (Main.rand.NextBool(2))
                {
                    //If it fails, inflict the player with 5 seconds of Chaos State
                    player.AddBuff(BuffID.ChaosState, 300, true);
                    Vector2? abyss = CalamityPlayer.GetAbyssPosition(player);
                    if (!abyss.HasValue)
                        return false;
                    CalamityPlayer.ModTeleport(player, abyss.Value, false, TeleportationStyleID.TeleportationPotion);
                }
                //If it doesn't fail, just act like a Potion of Return
                else
                {
                    player.DoPotionOfReturnTeleportationAndSetTheComebackPoint();
                }
                //Dust and Teleport sounds happen regardless of if it fails or not
                Rectangle rect = player.getRect();
                int dustAmt = rect.Width * rect.Height / 5;
                for (int k = 0; k < dustAmt; k++)
                {
                    Dust dust = Dust.NewDustDirect(new Vector2(rect.X, rect.Y), rect.Width, rect.Height, DustID.TeleportationPotion);
                    dust.scale = Main.rand.NextFloat(0.2f, 0.7f);
                    if (k < 10)
                        dust.scale += 0.25f;
                    if (k < 5)
                        dust.scale += 0.25f;
                }
                for (int k = 0; k < 70; k++)
                {
                    Dust dust = Dust.NewDustDirect(new Vector2(rect.X, rect.Y), rect.Width, rect.Height, DustID.DungeonSpirit);
                    dust.noGravity = true;
                    for (int i = 0; i < 5; i++)
                    {
                        if (Main.rand.NextBool(3))
                            dust.velocity *= 0.75f;
                    }
                    if (Main.rand.NextBool(3))
                    {
                        dust.velocity *= 2f;
                        dust.scale *= 1.2f;
                    }
                    if (Main.rand.NextBool(3))
                    {
                        dust.velocity *= 2f;
                        dust.scale *= 1.2f;
                    }
                    if (Main.rand.NextBool())
                    {
                        dust.fadeIn = Main.rand.NextFloat(0.75f, 1f);
                        dust.scale = Main.rand.NextFloat(0.25f, 0.75f);
                    }
                    dust.scale *= 0.8f;
                }
                SoundEngine.PlaySound(SoundID.Item6, player.Center);
            }
            return true;
        }
    }
}
