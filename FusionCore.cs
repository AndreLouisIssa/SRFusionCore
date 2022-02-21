using SRML;
using SRML.SR;
using SRML.SR.SaveSystem.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SRFusionCore
{
    public static class FusionCore
    {
        public static readonly Dictionary<string,SlimeDefinition> pureSlimes = new Dictionary<string,SlimeDefinition>();
        public static readonly List<Identifiable.Id> exemptSlimes = new List<Identifiable.Id>
        {
            Identifiable.Id.GLITCH_TARR_SLIME,
            Identifiable.Id.TARR_SLIME
        };
        public static readonly List<FusionStrategy> fusionStrats = new List<FusionStrategy>();
        public static readonly CompoundDataPiece worldData = new CompoundDataPiece();

        public static void OnWorldDataSave(CompoundDataPiece data)
        {
            data.data = worldData.data;
        }

        public static void OnWorldDataLoad(CompoundDataPiece data)
        {
            worldData.data = data.data;
        }

        public static readonly List<FusionStrategy> strategies = new List<FusionStrategy>();

        public static bool ResolveMissingID(ref string value)
        {
            bool valid = AllComponentsExist(value);
            if (valid)
            {
                var data = worldData.GetValue<CompoundDataPiece>(value);
                var blame = data.GetValue<string>("blame");
                var strat = fusionStrats.FirstOrDefault(s => s.blame == blame);
                if (strat is null)
                {
                    Log.Error($"{nameof(SRFusionCore)}: No strategy {blame} to fix missing ID {value}!");
                    return false;
                }
                var parameters = GetParameters(data.GetValue<string>("parameters"));
                var components = GetComponents(data.GetValue<Identifiable.Id[]>("components"));
                value = strat.factory(components, parameters).IdentifiableId.ToString();
            }
            return valid;
        }

        public static string AdjustCategoryName(string original)
        {
            if (worldData.HasPiece(original))
                return "_" + worldData.GetValue<CompoundDataPiece>(original).GetValue<string>("category");
            return original;
        }

        private static List<SlimeDefinition> GetComponents(IEnumerable<Identifiable.Id> ids)
        {
            return ids.Select(i => SRSingleton<GameContext>.Instance.SlimeDefinitions.GetSlimeByIdentifiableId(i)).ToList();
        }

        private static List<Parameter> GetParameters(string v)
        {
            //TODO: Implement this
            return new List<Parameter>();
        }

        public static void Setup()
        {
            SlimeDefinitions defns = SRSingleton<GameContext>.Instance.SlimeDefinitions;
            foreach (var pure in defns.Slimes.Where(slime => !slime.IsLargo && !exemptSlimes.Contains(slime.IdentifiableId)))
            {
                pureSlimes.Add(PureName(pure.IdentifiableId.ToString()),pure);
            }
        }

        public static string PureName(string name)
        {
            name = name.Replace("_SLIME", "").Replace("_LARGO", "");
            if (name.Contains('_')) name = '+' + name + '+';
            return name;
        }

        public static List<string> Decompose(string body, IEnumerable<string> parts, out string rest)
        {
            rest = body;
            var found = new List<(int, string)>();
            foreach (var part in parts.OrderByDescending(p => p.Length))
            {
                var pos = rest.IndexOf(part);
                while (pos >= 0)
                {
                    found.Add((pos, part));
                    rest = rest.Remove(pos, part.Length);
                    pos = rest.IndexOf(part);
                }
            }
            return found.OrderBy(p => p.Item1).Select(p => p.Item2).ToList();
        }

        public static List<string> Decompose(string body, IEnumerable<string> parts)
        {
            return Decompose(body, parts, out _);
        }

        public static List<string> GetComponentSlimeNames(Identifiable.Id slimeId)
        {
            return GetComponentSlimeNames(slimeId.ToString());
        }

        public static List<string> GetComponentSlimeNames(string name)
        {
            return Decompose(name.Replace("_SLIME", "").Replace("_LARGO", ""), pureSlimes.Keys);
        }

        public static List<SlimeDefinition> GetPureSlimes(List<string> slimeNames)
        {
            return slimeNames.Select(n => pureSlimes[n]).ToList();
        }

        public static bool AllComponentsExist(string name)
        {
            Decompose(PureName(name), pureSlimes.Keys, out string rest);
            return rest.Replace("_", "") == "";
        }

        public static Identifiable.Id NewIdentifiableID(string name)
        {
            if (Enum.IsDefined(typeof(Identifiable.Id), name)) throw new Exception($"{nameof(SRFusionCore)}: ID already exists: {name}");
            Main.SRModLoader_get_CurrentLoadingStep._override = true;
            var id = IdentifiableRegistry.CreateIdentifiableId(EnumPatcher.GetFirstFreeValue(typeof(Identifiable.Id)), name);
            Main.SRModLoader_get_CurrentLoadingStep._override = false;
            return id;
        }
    }
}