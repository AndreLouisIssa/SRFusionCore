using HarmonyLib;
using SRML;
using SRML.SR.SaveSystem;

namespace SRFusionCore
{
    public class Main : ModEntryPoint
    {
        [HarmonyPatch(typeof(SRModLoader),nameof(SRModLoader.LoadMods))]
        public class SRModLoader_LoadMods
        {
            public static void Postfix()
            {
                FusionCore.Setup();
                SaveRegistry.RegisterWorldDataLoadDelegate(FusionCore.OnWorldDataLoad);
                SaveRegistry.RegisterWorldDataSaveDelegate(FusionCore.OnWorldDataSave);
                EnumTranslator.RegisterFallbackHandler<Identifiable.Id>(FusionCore.ResolveMissingID);
            }
        }

        [HarmonyPatch(typeof(SRModLoader), nameof(SRModLoader.CurrentLoadingStep), MethodType.Getter)]
        public class SRModLoader_get_CurrentLoadingStep {
            public static bool _override = false;
            public static void Postfix(ref SRModLoader.LoadingStep __result) {
                if (_override) __result = SRModLoader.LoadingStep.PRELOAD;
            }
        }

    }
}