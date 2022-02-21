using SRML;
using SRML.SR;
using SRML.SR.SaveSystem.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Console = SRML.Console.Console;

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
            data.SetValue(worldData.GetValue());
        }

        public static void OnWorldDataLoad(CompoundDataPiece data)
        {
            worldData.SetValue(data.GetValue());
        }

        public static readonly List<FusionStrategy> strategies = new List<FusionStrategy>();

        public static bool ResolveMissingID(ref string value)
        {
            bool valid = AllComponentsExist(value);
            if (valid)
            {
                var blame = worldData.GetValue<CompoundDataPiece>("B").GetValue<string>(value);
                var parameters = GetParameters(worldData.GetValue<CompoundDataPiece>("P").GetValue<string>(value));
                var components = GetPureSlimes(GetComponentSlimeNames(value));
                foreach (var strat in fusionStrats.Where(s => s.blame == blame))
                {
                    strat.factory(NewIdentifiableID(value), components, parameters);
                }
            }
            return valid;
        }

        private static List<Parameter> GetParameters(string v)
        {
            //TODO: Implement this
            return new List<Parameter>();
        }

        public static void Setup()
        {
            worldData.SetValue("B", new CompoundDataPiece());
            worldData.SetValue("P", new CompoundDataPiece());
            SlimeDefinitions defns = SRSingleton<GameContext>.Instance.SlimeDefinitions;
            foreach (var pure in defns.Slimes.Where(slime => !slime.IsLargo && !exemptSlimes.Contains(slime.IdentifiableId)))
            {
                pureSlimes.Add(PureName(pure.IdentifiableId.ToString()),pure);
            }
        }

        public static string PureName(string name)
        {
            return name.Replace("_SLIME", "").Replace("_LARGO", "");
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
            if (Enum.IsDefined(typeof(Identifiable.Id), name)) throw new Exception($"ID already exists: {name}");
            Main.SRModLoader_get_CurrentLoadingStep._override = true;
            var id = IdentifiableRegistry.CreateIdentifiableId(EnumPatcher.GetFirstFreeValue(typeof(Identifiable.Id)), name);
            Main.SRModLoader_get_CurrentLoadingStep._override = false;
            return id;
        }
    }
}