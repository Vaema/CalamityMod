using System.Collections.Generic;
using System.Linq;
using CalamityMod.Items.DraedonMisc;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    public sealed class EncryptedSchematicIDRelationshipDict : ModSystem
    {
        public static IDictionary<int, int> Dict { get; private set; }

        public override void OnModLoad()
        {
            var dict = new Dictionary<int, int>()
            {
                [1] = ItemType<EncryptedSchematicPlanetoid>(),
                [2] = ItemType<EncryptedSchematicJungle>(),
                [3] = ItemType<EncryptedSchematicHell>(),
                [4] = ItemType<EncryptedSchematicIce>(),
            };

            Dict = dict;
        }

        public override void Unload()
        {
            Dict?.Clear();
            Dict = null;
        }

        public static bool TryGet(int schematicID, out int schematicItemType)
        {
            return Dict.TryGetValue(schematicID, out schematicItemType);
        }

        public static bool TryGetKey(int schematicItemType, out int schematicID)
        {
            try
            {
                var pair = Dict.First(pair => pair.Value == schematicItemType);
                schematicID = pair.Key;
                return true;
            }
            catch
            {
                schematicID = 0;
                return false;
            }
        }
    }
}
