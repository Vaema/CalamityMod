using System.Collections.Generic;
using CalamityMod.Projectiles.Magic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> which contains an ProjectileID Set, created with the appropriate factory.<br />
    /// <br />
    /// This is populated from a much more human readable set of "shared iframe lists", contained in OnModLoad. 
    /// </summary>
    public sealed class SharedStaticIFrames : ModSystem
    {
        public static IDictionary<int, IList<int>> SharedIFrameSets { get; private set; }
        public static int[] SharedIFrameFactorySet { get; private set; }

        public override void PostSetupContent()
        {
            // This transient structure is used to make the setup of intended shared-iframe sets as easy as possible.
            //
            // If you want to add a new projectile to share iframes with any existing sets, just add it to that [ area ].
            //
            // If you want to create a brand new shared iframe set, just add a new [ area ] at the bottom.
            IList<IList<int>> setupLists = 
            [
                [ // Set 1: Vanilla bees (Hive Pack)
                    ProjectileID.Bee,
                    ProjectileID.GiantBee
                ],

                [ // Set 2: Vilethorn
                    ProjectileID.VilethornBase,
                    ProjectileID.VilethornTip
                ],

                [ // Set 3: Nettle Burst
                    ProjectileID.NettleBurstLeft,
                    ProjectileID.NettleBurstRight,
                    ProjectileID.NettleBurstEnd
                ],

                [ // Set 4: Harp notes
                    ProjectileID.QuarterNote,
                    ProjectileID.EighthNote,
                    ProjectileID.TiedEighthNote
                ],

                [ // Set 5: The North Pole
                    ProjectileID.NorthPoleWeapon,
                    ProjectileID.NorthPoleSpear,
                    ProjectileID.NorthPoleSnowflake,
                ],

                [ // Set 6: All spore gas clouds
                    ProjectileID.SporeTrap,
                    ProjectileID.SporeTrap2,
                    ProjectileID.SporeGas,
                    ProjectileID.SporeGas2,
                    ProjectileID.SporeGas3
                ],

                [ // Set 7: Astral Staff
                    ModContent.ProjectileType<AstralCrystal>(),
                    ModContent.ProjectileType<AstralCrystalInvisibleExplosion>()
                ],

                [ // Set 8: Keelhaul
                    ModContent.ProjectileType<KeelhaulGeyserBottom>(),
                    ModContent.ProjectileType<KeelhaulGeyserTop>()
                ],

                [ // Set 8: All toxic clouds
                    ProjectileID.ToxicCloud,
                    ProjectileID.ToxicCloud2,
                    ProjectileID.ToxicCloud3
                ]
            ];

            // Convert the above list-of-lists into a dictionary, where the key is the shared iframe projectile ID.
            SharedIFrameSets = new SortedDictionary<int, IList<int>>();
            foreach (var innerList in setupLists)
            {
                int sharedIDForThisSet = innerList[0];
                SharedIFrameSets.Add(sharedIDForThisSet, innerList);
            }

            // Now construct a vanilla Terraria Projectile ID Set using the appropriate factory.

            // Calculate the total length of the input buffer to the factory
            int totalLength = 0;
            foreach(var l in SharedIFrameSets.Values)
                totalLength += l.Count;

            // Create the buffer and fill it
            int[] buffer = new int[totalLength * 2];
            int idx = 0;
            foreach (var sharedFrameList in setupLists)
            {
                int sharedIDForThisSet = sharedFrameList[0];

                foreach (var projID in sharedFrameList)
                {
                    buffer[idx++] = projID;
                    buffer[idx++] = sharedIDForThisSet;
                }
            }

            SharedIFrameFactorySet = ProjectileID.Sets.Factory.CreateIntSet(-1, buffer);
        }

        public override void Unload()
        {
            SharedIFrameSets = null;
            SharedIFrameFactorySet = null;
        }

        /// <summary>
        /// A shorthand method to check if this projectile shares static iframes in any way.
        /// </summary>
        public static bool Includes(int projType) => SharedIFrameFactorySet[projType] != -1;

        /// <summary>
        /// The intended 
        /// </summary>
        /// <param name="projType">The projectile type to get the set of shared iframe projectile IDs for.</param>
        /// <returns>A list of projectile IDs that share static iframes with this projectile ID. <b>This list may be empty.</b></returns>
        public static IList<int> GetSharedStaticIFrames(int projType)
        {
            int sharedSetID = SharedIFrameFactorySet[projType];
            bool setExists = SharedIFrameSets.TryGetValue(sharedSetID, out var ret);

            return setExists ? ret : [];
        }
    }
}
