using HarmonyLib;
using SRML;
using SRML.Console;
using SRML.SR;
using SRML.SR.SaveSystem;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace FusionCore
{
    public class Main : ModEntryPoint
    {
        public override void PreLoad()
        {
            HarmonyInstance.PatchAll();
            SaveRegistry.RegisterWorldDataLoadDelegate(Core.OnWorldDataLoad);
            SaveRegistry.RegisterWorldDataSaveDelegate(Core.OnWorldDataSave);
            EnumTranslator.RegisterFallbackHandler<Identifiable.Id>(Core.ResolveMissingID);
            SRCallbacks.OnMainMenuLoaded += (_) => Core.worldData.DataList.Clear();
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
            public static void Postfix(ref SRModLoader.LoadingStep __result)
            {
                if (_override) __result = SRModLoader.LoadingStep.PRELOAD;
            }
        }

        [HarmonyPatch(typeof(IdentifiableRegistry), "CategorizeId")]
        class SR_IdentifiableRegistry_CategorizeId
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var code = instructions.ToList();
                var ind = code.FindIndex((x) => x.opcode == OpCodes.Stloc_0);
                code.Insert(ind++, new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(Core),nameof(Core.AdjustCategoryName))));
                return code;
            }
        }

    }
}