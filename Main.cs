using HarmonyLib;
using SRML;
using SRML.SR;
using SRML.SR.SaveSystem;
using static SRML.SR.SRCallbacks;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.IO;
using SRML.SR.SaveSystem.Data;
using Console = SRML.Console.Console;
using System;
using SRML.SR.SaveSystem.Format;

namespace FusionCore
{
    public class Main : ModEntryPoint
    {
        public static readonly string worldDataPath = "FusionCoreWorldData";
        public static readonly string worldDataExt = ".txt";

        public override void PreLoad()
        {
            HarmonyInstance.PatchAll();
            EnumTranslator.RegisterFallbackHandler<Identifiable.Id>(Core.ResolveMissingID);
            OnMainMenuLoaded += (_) => Core.worldData.DataList.Clear();
            Console.RegisterCommand(new Command());
        }

        [HarmonyPatch(typeof(SRModLoader), nameof(SRModLoader.LoadMods))]
        public class SRModLoader_LoadMods
        {
            public static void Postfix()
            {
                Core.Setup();
            }
        }

        [HarmonyPatch(typeof(SRModLoader), nameof(SRModLoader.CurrentLoadingStep), MethodType.Getter)]
        public class SRModLoader_get_CurrentLoadingStep
        {
            public static bool _override = false;
            public static SRModLoader.LoadingStep _step = SRModLoader.LoadingStep.PRELOAD;
            public static void Postfix(ref SRModLoader.LoadingStep __result)
            {
                if (_override) __result = _step;
            }
        }

        [HarmonyPatch(typeof(IdentifiableRegistry), nameof(IdentifiableRegistry.CategorizeId))]
        class SR_IdentifiableRegistry_CategorizeId
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var code = instructions.ToList();
                var ind = code.FindIndex((x) => x.opcode == OpCodes.Stloc_0);
                code.Insert(ind++, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Core), nameof(Core.AdjustCategoryName))));
                return code;
            }
        }

        [HarmonyPatch(typeof(SaveHandler), nameof(SaveHandler.LoadModdedSave))]
        class SR_SaveHandler_LoadModdedSave
        {
            public static void Prefix(AutoSaveDirector director, string savename)
            {
                var path = Path.Combine(((FileStorageProvider)director.StorageProvider).SavePath(),
                    worldDataPath, savename.Substring(0, savename.LastIndexOf('_')) + worldDataExt);
                if (File.Exists(path))
                {
                    Core.OnWorldDataLoad(Core.DecodeBlames(string.Join("", File.ReadAllBytes(path).Select(b => (char)b))));
                }
            }
        }

        [HarmonyPatch(typeof(SaveHandler), nameof(SaveHandler.SaveModdedSave))]
        class SR_SaveHandler_SaveModdedSave
        {
            public static void Postfix(AutoSaveDirector director, string nextfilename)
            {
                if (!Core.worldData.DataList.Any()) return;
                var path = Path.Combine(((FileStorageProvider)director.StorageProvider).SavePath(),
                    worldDataPath, nextfilename.Substring(0, nextfilename.LastIndexOf('_')) + worldDataExt);
                var blamestring = Core.EncodeBlames(Core.worldData);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var file = new BinaryWriter(File.Create(path));
                file.Write(blamestring.Select(c => (byte)c).ToArray());
                file.Close();
            }
        }
    }
}