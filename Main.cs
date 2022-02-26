using HarmonyLib;
using SRML;
using SRML.SR;
using SRML.SR.SaveSystem;
using static SRML.SR.SRCallbacks;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.IO;
using Console = SRML.Console.Console;
using static FusionCore.Core;

namespace FusionCore
{
    public class Main : ModEntryPoint
    {
        public static readonly string worldDataPath = "FusionCoreWorldData";
        public static readonly string worldDataExt = ".txt";

        public override void PreLoad()
        {
            HarmonyInstance.PatchAll();
            EnumTranslator.RegisterFallbackHandler<Identifiable.Id>(ResolveMissingID);
            OnMainMenuLoaded += (_) => blames.DataList.Clear();
            Console.RegisterCommand(Fuse.instance);
        }

        [HarmonyPatch(typeof(SRModLoader), nameof(SRModLoader.LoadMods))]
        public class SRModLoader_LoadMods
        {
            public static void Postfix()
            {
                Setup();
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
        public class SR_IdentifiableRegistry_CategorizeId
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var code = instructions.ToList();
                var ind = code.FindIndex((x) => x.opcode == OpCodes.Stloc_0);
                code.Insert(ind++, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Core), nameof(AdjustCategoryName))));
                return code;
            }
        }

        [HarmonyPatch(typeof(SaveHandler), nameof(SaveHandler.LoadModdedSave))]
        public class SR_SaveHandler_LoadModdedSave
        {
            public static void Prefix(AutoSaveDirector director, string savename)
            {
                var path = Path.Combine(((FileStorageProvider)director.StorageProvider).SavePath(),
                    worldDataPath, savename.Substring(0, savename.LastIndexOf('_')) + worldDataExt);
                if (File.Exists(path))
                {
                    OnWorldDataLoad(DecodeBlames(string.Join("", File.ReadAllBytes(path).Select(b => (char)b))));
                }
            }
        }

        [HarmonyPatch(typeof(SaveHandler), nameof(SaveHandler.SaveModdedSave))]
        public class SR_SaveHandler_SaveModdedSave
        {
            public static void Postfix(AutoSaveDirector director, string nextfilename)
            {
                if (!blames.DataList.Any()) return;
                var path = Path.Combine(((FileStorageProvider)director.StorageProvider).SavePath(),
                    worldDataPath, nextfilename.Substring(0, nextfilename.LastIndexOf('_')) + worldDataExt);
                var blamestring = EncodeBlames(blames);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                var file = new BinaryWriter(File.Create(path));
                file.Write(blamestring.Select(c => (byte)c).ToArray());
                file.Close();
            }
        }

        [HarmonyPatch(typeof(SlimeAppearanceDirector), nameof(SlimeAppearanceDirector.GetChosenSlimeAppearance), typeof(SlimeDefinition))]
        public class SlimeAppearanceDirector_GetChosenSlimeAppearance_SlimeDefinition
        {
            public static bool Prefix(SlimeDefinition slimeDefinition, ref SlimeAppearance __result)
            {
                var name = slimeDefinition.GetFullName();
                if (blames.HasPiece(name))
                {
                    var piece = blames.GetCompoundPiece(name);
                    var strat = fusionModes[piece.GetValue<string>("mode")];
                    return !strat.FixAppearance(slimeDefinition, ref __result);
                }
                return true;
            }
        }
    }
}