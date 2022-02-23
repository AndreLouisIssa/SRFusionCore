using SRML;
using SRML.SR;
using SRML.SR.SaveSystem.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FusionCore
{
    public static class Core
    {
        public static readonly List<Strategy> fusionStrats = new List<Strategy>();
        public static readonly Dictionary<string, SlimeDefinition> pureSlimes = new Dictionary<string, SlimeDefinition>();
        public static readonly List<Identifiable.Id> exemptSlimes = new List<Identifiable.Id>
        {
            Identifiable.Id.GLITCH_TARR_SLIME,
            Identifiable.Id.TARR_SLIME
        };

        public static readonly CompoundDataPiece worldData = new CompoundDataPiece("");
        public static void OnWorldDataSave(CompoundDataPiece data) { data.data = worldData.data; }
        public static void OnWorldDataLoad(CompoundDataPiece data) { worldData.data = data.data; }

        public static string AdjustCategoryName(string original)
        {
            if (worldData.HasPiece(original))
                return "_" + worldData.GetValue<CompoundDataPiece>(original).GetValue<string>("category");
            return original;
        }

        public static Identifiable.Id NewIdentifiableID(string name, SRMod mod = null)
        {
            if (Enum.IsDefined(typeof(Identifiable.Id), name)) throw new Exception($"{nameof(FusionCore)}: ID already exists: {name}");
            var id = InvokeAsStep(() => IdentifiableRegistry.CreateIdentifiableId(EnumPatcher.GetFirstFreeValue(typeof(Identifiable.Id)), name));
            if (mod is null) mod = SRMod.GetCurrentMod();
            IdentifiableRegistry.moddedIdentifiables[id] = mod;
            return id;
        }

        public static void SetDisplayName(this SlimeDefinition slime, string name)
        {
            var entry = "l." + slime.IdentifiableId.ToString().ToLower();
            if (TranslationPatcher.doneDictionaries["actor"].ContainsKey(entry))
                TranslationPatcher.doneDictionaries["actor"].Remove(entry);
            InvokeAsStep(() => TranslationPatcher.AddActorTranslation(entry, name));
            TranslationPatcher.doneDictionaries["actor"][entry] = name;
            TranslationPatcher.patches["actor"][entry] = name;
        }

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
                    Log.Error($"{nameof(FusionCore)}: No strategy {blame} to fix missing ID {value}!");
                    return false;
                }
                var parameters = GetParameters(strat, data.GetValue<string>("parameters"));
                var components = GetComponents(data.GetValue<Identifiable.Id[]>("components"));
                value = strat.factory(components, parameters).IdentifiableId.ToString();
            }
            return valid;
        }

        public static void BlameStrategyForID(this Strategy strategy, Identifiable.Id id, List<SlimeDefinition> components, List<Parameter> parameters)
        {
            var data = new CompoundDataPiece(id.ToString());
            data.SetValue("blame", strategy.blame);
            data.SetValue("category", strategy.category);
            data.SetValue("components", components.Select(s => s.IdentifiableId).ToArray());
            data.SetValue("parameters", string.Join(" ", parameters.Select(p => p.ToString())));
        }

        public static SlimeDefinition InvokeStrategy(this Strategy strategy, List<SlimeDefinition> components, List<Parameter> parameters = null)
        {
            parameters = parameters ?? new List<Parameter>();
            var slime = strategy.factory(components, parameters);
            if (!Levels.isMainMenu()) BlameStrategyForID(strategy, slime.IdentifiableId, components, parameters);
            return slime;
        }

        public static List<SlimeDefinition> GetComponents(IEnumerable<Identifiable.Id> ids)
        {
            var defns = SRSingleton<GameContext>.Instance.SlimeDefinitions;
            return ids.Select(i => defns.GetSlimeByIdentifiableId(i)).ToList();
        }

        public static List<Parameter> GetParameters(this Strategy strat, string msg)
        {
            return GetParameters(strat, msg.Split(' '));
        }

        public static List<Parameter> GetParameters(this Strategy strat, IEnumerable<string> args)
        {
            return args.Select((s, i) => new Parameter(strat.types[i], s)).ToList();
        }

        public static void Setup()
        {
            SlimeDefinitions defns = SRSingleton<GameContext>.Instance.SlimeDefinitions;
            foreach (var pure in defns.Slimes.Where(slime => !slime.IsLargo && !exemptSlimes.Contains(slime.IdentifiableId)))
            {
                pureSlimes.Add(PureName(pure.IdentifiableId.ToString()), pure);
            }
        }

        public static string PureName(string name)
        {
            name = Regex.Replace(name, @"((_SLIME)|(_LARGO)).*", "");
            //if (name.Contains('_')) name = '+' + name + '+';
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

        public static List<SlimeDefinition> GetPureSlimes(string name)
        {
            return GetPureSlimes(GetComponentSlimeNames(name));
        }

        public static List<SlimeDefinition> GetPureSlimes(Identifiable.Id slimeId)
        {
            return GetPureSlimes(GetComponentSlimeNames(slimeId));
        }

        public static bool AllComponentsExist(string name)
        {
            Decompose(PureName(name), pureSlimes.Keys, out string rest);
            return rest.Replace("_", "") == "";
        }

        public static bool PureNameExists(string name)
        {
            return pureSlimes.Keys.Contains(PureName(name));
        }

        public static string UniquePureName(List<SlimeDefinition> components)
        {
            return string.Join("_", components.SelectMany(c => GetComponentSlimeNames(c.IdentifiableId.ToString())).Select(PureName));
        }

        public static int UniqueNameHash(this Strategy strategy, List<SlimeDefinition> components, List<Parameter> parameters = null)
        {
            var hash = (parameters is null) ? 0 : parameters.Select(p => p.GetHashCode()).Aggregate((h1, h2) => 13 * h1 + h2);
            return hash + components.Select(c => c.IdentifiableId.GetHashCode()).Aggregate((h1, h2) => 11 * h1 + h2) + 27 * strategy.GetHashCode();
        }

        public static string UniqueName(this Strategy strategy, string suffix, List<SlimeDefinition> components, List<Parameter> parameters = null)
        {
            return $"{UniquePureName(components)}_{suffix}_{EncodeHash(UniqueNameHash(strategy, components, parameters))}";
        }

        public static string EncodeHash(int value)
        {
            //https://stackoverflow.com/a/33729594
            var chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            var n = chars.Length;
            var result = new StringBuilder(value < 0 ? "-" : "");
            value = Math.Abs(value);

            while (value != 0)
            {
                result.Append(chars[value % n]);
                value = value / n;
            }

            return result.ToString();
        }

        public static string CamelCase(string s)
        {
            if (s == "") return s;
            return s.First().ToString().ToUpper() + s.Substring(1).ToLower();
        }

        public static string DisplayName(string suffix, List<SlimeDefinition> components)
        {
            var name = Regex.Replace(UniquePureName(components) + "_" + suffix, "[_+]+", " ");
            return string.Join(" ", name.Split(' ').Select(CamelCase));
        }

        public static void InvokeAsStep(Action run, SRModLoader.LoadingStep step = SRModLoader.LoadingStep.PRELOAD)
        {
            Main.SRModLoader_get_CurrentLoadingStep._override = true;
            Main.SRModLoader_get_CurrentLoadingStep._step = step;
            run();
            Main.SRModLoader_get_CurrentLoadingStep._override = false;
        }

        public static T InvokeAsStep<T>(Func<T> run, SRModLoader.LoadingStep step = SRModLoader.LoadingStep.PRELOAD)
        {
            Main.SRModLoader_get_CurrentLoadingStep._override = true;
            Main.SRModLoader_get_CurrentLoadingStep._step = step;
            var value = run();
            Main.SRModLoader_get_CurrentLoadingStep._override = false;
            return value;
        }
    }
}