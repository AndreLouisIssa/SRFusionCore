using SRML.SR.SaveSystem.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using static FusionCore.Core;

namespace FusionCore
{
    public class Strategy
    {
        public static int globalInvokeCounter = 0;
        public int localInvokeCounter = 0;
        public static IEnumerable<(Parameter.Type type, string label)> EmptyVariadic()
            { yield break; }
        public static IEnumerable<(Parameter.Type type, string label)> RepeatVariadic(Parameter.Type type, string label)
            { while (true) yield return (type, label); }

        public delegate SlimeDefinition Factory(ref List<SlimeDefinition> components, ref List<Parameter> parameters);

        public override int GetHashCode() { return blame.GetHashCode(); }
        public string blame;
        public string category;
        public List<(Parameter.Type type, string label)> required;
        public List<(Parameter.Type type, string label, object init)> optional;
        public IEnumerable<(Parameter.Type type, string label)> variadic;
        public Factory factory;

        public Strategy(string blame, Factory factory, string category = "SLIME",
            List<(Parameter.Type type, string label)> required = null,
            List<(Parameter.Type type, string label, object init)> optional = null,
            IEnumerable<(Parameter.Type type, string label)> variadic = null)
        {
            this.blame = blame.ToUpper();
            this.category = category.ToUpper();
            this.required = required ?? new List<(Parameter.Type type, string label)>();
            this.optional = optional ?? new List<(Parameter.Type type, string label, object init)>();
            this.variadic = variadic ?? EmptyVariadic();
            this.factory = factory;
        }

        public SlimeDefinition Invoke(List<SlimeDefinition> components, List<Parameter> parameters = null)
        {
            parameters = parameters ?? new List<Parameter>();
            components = components.SelectMany(c => PureSlimeFullNames(c.GetFullName()).Select(GetSlimeByFullName)).ToList();
            var gc = ++globalInvokeCounter; var lc = ++localInvokeCounter;
            Log.Info($"{nameof(FusionCore)}: [#{gc}|#{lc}] Fusing in mode {blame}..." +
                $" (on {string.Join(", ", components.Select(Core.GetFullName))}) (with {string.Join(", ", parameters)}...)");
            var slime = factory(ref components, ref parameters);
            RememberID(slime.IdentifiableId, components, parameters);
            Log.Info($"{nameof(FusionCore)}: [#{gc}|#{lc}] Fusion resulted in {DebugName(slime)}");
            return slime;
        }

        public List<SlimeDefinition> ParseComponents(IEnumerable<string> ids)
        {
            var defns = SRSingleton<GameContext>.Instance.SlimeDefinitions;
            return ids.Select(i => defns.GetSlimeByIdentifiableId((Identifiable.Id)Enum.Parse(typeof(Identifiable.Id),i))).ToList();
        }

        public List<Parameter> ParseParameters(IEnumerable<string> args)
        {
            var arglist = args.ToList();
            var parameters = required.Select((p, i) => Parameter.Parse(p.type, arglist[i]));
            parameters = parameters.Concat(optional.Select((p, i) => (p, i + required.Count))
                .Select(t => Parameter.Parse(t.p.type, t.Item2 < arglist.Count ? arglist[t.Item2] : t.p.type.represent(t.p.init))));
            if (variadic.Any() && arglist.Count > parameters.Count())
            {
                var e = variadic.GetEnumerator();
                parameters = parameters.Concat(arglist.Skip(parameters.Count())
                    .Select((a, i) => Parameter.Parse(variadic.Skip(i).First().type, a)));
            }
            return parameters.ToList();
        }

        public void RememberID(Identifiable.Id id, List<SlimeDefinition> components, List<Parameter> parameters)
        {
            var data = new CompoundDataPiece(id.ToString());
            data.SetValue("blame", blame);
            data.SetValue("category", category);
            data.SetValue("components", components.Select(Core.GetFullName).ToArray());
            data.SetValue("parameters", parameters.Select(p => p.ToString()).ToArray());
            worldData.AddPiece(data);
        }

        public string UniqueFirstName(List<SlimeDefinition> components)
        {
            return string.Join("_", components.SelectMany(c => DecomposePureSlimeNames(c.GetFullName())).Select(PureName));
        }

        public int UniqueSurnameHash(List<SlimeDefinition> components, List<Parameter> parameters = null)
        {
            var hash = (parameters is null) ? 0 : parameters.Select(p => p.GetHashCode()).Aggregate((h1, h2) => 27 * h1 + h2);
            return 13 * hash + components.Select(c => c.IdentifiableId.GetHashCode()).Aggregate((h1, h2) => 11 * h1 + h2) + 71 * GetHashCode();
        }

        public string UniqueFullName(string suffix, List<SlimeDefinition> components, List<Parameter> parameters = null)
        {
            return $"{UniqueFirstName(components)}_{suffix}_{EncodeHash(UniqueSurnameHash(components, parameters))}";
        }

        public string DebugName(SlimeDefinition slime)
        {
            var display = slime.GetDisplayName();
            if (display != null) display = "\"" + display + "\"";
            else display = "<MISSING DISPLAY NAME>";
            return $"{TitleCase(category)}: {display} ({slime.IdentifiableId})";
        }
    }
}