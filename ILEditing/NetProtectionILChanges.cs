using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace CalamityMod.ILEditing
{
    // We want to set this as Separate ILChages type for reasons:
    // - To easily filter ILChanges callstack from StackTrace
    // - To make no impact on ILChanges type which is already clustered
    public sealed class NetProtectionILChanges : ModSystem
    {
        private static string[] _ModNames;

        public override void OnModLoad()
        {
            _ModNames = ModLoader.Mods.Select(mod => mod.Name).ToArray();

            On_NPC.NewNPC += NewNPCRule;
            On_NetMessage.SendData += SendDataRule;
        }

        public override void OnModUnload()
        {
            _ModNames = null;
        }

        private int NewNPCRule(On_NPC.orig_NewNPC orig, IEntitySource source, int X, int Y, int Type, int Start, float ai0, float ai1, float ai2, float ai3, int Target)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                CalamityMod.Instance.Logger.Warn($"{nameof(NPC.NewNPC)} was called from Client! {GetSimplifiedStackTrace()}");
                return Main.maxNPCs;
            }

            return orig(source, X, Y, Type, Start, ai0, ai1, ai2, ai3, Target);
        }

        private static void SendDataRule(On_NetMessage.orig_SendData orig, int msgType, int remoteClient, int ignoreClient, NetworkText text, int number, float number2, float number3, float number4, int number5, int number6, int number7)
        {
            // Sending SyncNPC as Client will cause Server to not read their bytes
            // So we simply stop that from happening
            if (msgType == MessageID.SyncNPC && Main.netMode == NetmodeID.MultiplayerClient)
            {
                CalamityMod.Instance.Logger.Warn($"{nameof(NetMessage.SendData)} ({nameof(MessageID.SyncNPC)}) was called from Client! {GetSimplifiedStackTrace()}");
                return;
            }

            orig(msgType, remoteClient, ignoreClient, text, number, number2, number3, number4, number5, number6, number7);
        }

        #region Simplified StackTrace Util Method

        private static bool ContainsAnyModName(string str)
        {
            return _ModNames?.Any(str.Contains) ?? false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)] // Force No Inlining to easily filter current stack (skipFrames: 1)
        private static string GetSimplifiedStackTrace()
        {
            var stringBuilder = new StringBuilder();
            try
            {
                stringBuilder.AppendLine("STACKTRACE:");

                var stacktrace = new StackTrace(skipFrames: 1);
                var frames = stacktrace.GetFrames();
                var didPrintSomethingUselessLastTime = false;
                var didEverPrintSomethingUseful = false;
                foreach (var frame in frames)
                {
                    var method = frame.GetMethod();
                    var methodName = method?.ToString() ?? "UNKNOWN_METHOD";
                    var typeName = method?.DeclaringType?.FullName ?? "UNKNOWN_TYPE";

                    // Logging Rule:
                    // - It should contains ANY ModName in their FullName
                    // - It should NOT contains NetProtectionILChanges in their FullName (Calamity Codebase Exclusive)
                    if (ContainsAnyModName(typeName) && !typeName.Contains(nameof(NetProtectionILChanges)))
                    {
                        didPrintSomethingUselessLastTime = false;
                        didEverPrintSomethingUseful = true;
                        stringBuilder.AppendFormat(" at {0}::{1}", typeName, methodName);
                        stringBuilder.AppendLine();
                    }
                    else if (!didPrintSomethingUselessLastTime && didEverPrintSomethingUseful)
                    {
                        stringBuilder.AppendLine(" ...");
                        didPrintSomethingUselessLastTime = true;
                    }
                }

                stringBuilder.AppendLine("END OF STACKTRACE");
                return stringBuilder.ToString();
            }
            catch
            {
                return "";
            }
            finally
            {
                stringBuilder?.Clear();
            }
        }

        #endregion
    }
}
