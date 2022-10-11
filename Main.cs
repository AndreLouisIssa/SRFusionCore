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
        // @MagicGonads @Aidanamite
        public static readonly string worldDataPath = "FusionCoreWorldData";
        public static readonly string worldDataExt = ".txt";

        public override void PreLoad()
        {
            // @MagicGonads @Aidanamite
            // prepare all the side effects
            HarmonyInstance.PatchAll();
            EnumTranslator.RegisterFallbackHandler<Identifiable.Id>(ResolveMissingID);
            OnMainMenuLoaded += (_) => blames.DataList.Clear();
            Console.RegisterCommand(Fuse.instance);
        }

        [HarmonyPatch(typeof(SRModLoader), nameof(SRModLoader.LoadMods))]
        public class SRModLoader_LoadMods
        {
            // @MagicGonads @Aidanamite
            // load after all the regular mods should have finished creating all their slimes so we can cache them
            public static void Postfix() { Setup(); }
        }

        [HarmonyPatch(typeof(SaveHandler), nameof(SaveHandler.LoadModdedSave))]
        public class SR_SaveHandler_LoadModdedSave
        {
            public static void Prefix(AutoSaveDirector director, string savename)
            {
                // @MagicGonads @Aidanamite
                // read world data before the enum patcher has a chance to resolve missing IDs
                // so we have the data necessary to fix those IDs based on blamed modes
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
                // @MagicGonads
                // save world data blaming modes for the fusion IDs that are present by the time of this save
                // this covers at least all the IDs necessary to be able to reload that save safely
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
                // @MagicGonads @Aidanamite
                // override cached slime appearance to fix an issue where slime definitions
                // created while a world is loaded would be invisible and cause an NRE
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