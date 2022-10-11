using SRML;
using SRML.SR.SaveSystem.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using static FusionCore.Core;
using static FusionCore.Parameter;

namespace FusionCore
{
    public abstract class Mode
    {
        // @MagicGonads @Aidanamite
        public static int globalInvokeCounter = 0;
        public int localInvokeCounter = 0;

        public static IEnumerable<(Form form, string label)> EmptyVariadic()
            { yield break; }
        public static IEnumerable<(Form form, string label)> RepeatVariadic(Form form, string label)
            { while (true) yield return (form, label); }

        public abstract SlimeDefinition ProduceRaw(ref List<SlimeDefinition> components, ref List<Parameter> parameters, out bool isNew);
        public virtual bool FixAppearance(SlimeDefinition slime, ref SlimeAppearance appearance) => false;

        public override int GetHashCode() { return Blame.GetHashCode(); }
        public abstract string Blame { get; }
        public virtual Form Fusion => Form.Forms.PureSlimes;
        public virtual List<(Form form, string label)> Required => new List<(Form, string)>();
        public virtual List<(Form form, string label, object init)> Optional => new List<(Form, string, object)>();
        public virtual IEnumerable<(Form form, string label)> Variadic => EmptyVariadic();
        public virtual bool Hidden => false;
        public virtual bool Silent => false;

        public void Register() { fusionModes[Blame] = this; }

        public Mode() { }

        public SlimeDefinition Produce(List<SlimeDefinition> components, List<Parameter> parameters, out bool isNew)
        {
            // @MagicGonads
            components = components.ToList();
            parameters = parameters.ToList();
            var gc = ++globalInvokeCounter; var lc = ++localInvokeCounter;
            if (!Silent) Log.Info($"{nameof(FusionCore)}: [#{gc}|#{lc}] Fusing in mode {Blame}..." +
                $" (on {string.Join(", ", components.Select(Core.GetFullName))}) (with {string.Join(", ", parameters)}...)");
            var hash = UniqueSurnameHash(components, parameters);
            if (cachedHashes.TryGetValue(hash, out var slime)) isNew = false;
            else slime = ProduceRaw(ref components, ref parameters, out isNew);
            if (isNew) RememberID(slime.IdentifiableId, components, parameters);
            cachedHashes[hash] = slime;
            if (!Silent) Log.Info($"{nameof(FusionCore)}: [#{gc}|#{lc}] Fusion resulted in {(isNew ? "a new " : "an existing ")}{DebugName(slime)}");
            return slime;
        }

        public SlimeDefinition Produce(List<SlimeDefinition> components, List<Parameter> parameters)
        {
            return Produce(components, parameters, out _);
        }

        public SlimeDefinition Produce(List<SlimeDefinition> components)
        {
            return Produce(components, new List<Parameter>());
        }

        public bool TryProduce(List<SlimeDefinition> components, List<Parameter> parameters, out SlimeDefinition fusion)
        {
            fusion = Produce(components, parameters, out var isNew);
            return isNew;
        }

        public bool TryProduce(List<SlimeDefinition> components, out SlimeDefinition fusion)
        {
            fusion = Produce(components, new List<Parameter>(), out var isNew);
            return isNew;
        }

        public Form GetArgumentForm(int i)
        {
            if (i < Required.Count) return Required[i].form;
            if ((i -= Required.Count) < Optional.Count) return Optional[i].form;
            return Variadic.Skip(i).First().form;
        }

        public List<SlimeDefinition> ParseComponents(string fusion)
        {
            // @MagicGonads
            return (List<SlimeDefinition>)Fusion.read(fusion);
        }

        public List<SlimeDefinition> ParseComponents(IEnumerable<string> ids)
        {
            // @MagicGonads
            var defns = GameContext.Instance.SlimeDefinitions;
            return ids.Select(i => defns.GetSlimeByIdentifiableId((Identifiable.Id)Enum.Parse(typeof(Identifiable.Id),i))).ToList();
        }

        public List<Parameter> ParseParameters(IEnumerable<string> args)
        {
            // @MagicGonads
            var arglist = args.ToList();
            var parameters = Required.Select((p, i) => Parse(p.form, arglist[i]));
            parameters = parameters.Concat(Optional.Select((p, i) => (p, i + Required.Count))
                .Select(t => Parse(t.p.form, t.Item2 < arglist.Count ? arglist[t.Item2] : t.p.form.show(t.p.init))));
            if (Variadic.Any() && arglist.Count > parameters.Count())
            {
                var e = Variadic.GetEnumerator();
                parameters = parameters.Concat(arglist.Skip(parameters.Count())
                    .Select((a, i) => Parse(Variadic.Skip(i).First().form, a)));
            }
            return parameters.ToList();
        }

        public void RememberID(Identifiable.Id id, List<SlimeDefinition> components, List<Parameter> parameters)
        {
            // @MagicGonads @Aidanamite
            var data = new CompoundDataPiece(id.ToString());
            data.SetValue("mode", Blame);
            data.SetValue("components", components.Select(Core.GetFullName).ToArray());
            data.SetValue("parameters", parameters.Select(p => p.ToString()).ToArray());
            blames.AddPiece(data);
        }

        public string UniqueFirstName(List<SlimeDefinition> components)
        {
            // @MagicGonads
            return string.Join("_", components.SelectMany(c => DecomposePureSlimeNames(c.GetFullName())).Select(PureName));
        }

        public int UniqueSurnameHash(List<SlimeDefinition> components, List<Parameter> parameters = null)
        {
            // @MagicGonads
            var hash = (parameters is null || !parameters.Any()) ? 0 : parameters.Select(p => p.GetHashCode()).Aggregate((h1, h2) => 27 * h1 + h2);
            return 13 * hash + components.Select(c => c.IdentifiableId.GetHashCode()).Aggregate((h1, h2) => 11 * h1 + h2) + 71 * GetHashCode();
        }

        public string UniqueFullName(string suffix, List<SlimeDefinition> components, List<Parameter> parameters = null)
        {
            // @MagicGonads
            return $"{UniqueFirstName(components)}_{suffix}_{EncodeHash(UniqueSurnameHash(components, parameters))}";
        }

        public string DebugName(SlimeDefinition slime)
        {
            // @MagicGonads
            var display = slime.GetDisplayName();
            if (display != null) display = "\"" + display + "\"";
            else display = "<MISSING DISPLAY NAME>";
            return $"{TitleCase(Identifiable.LARGO_CLASS.Contains(slime.IdentifiableId) ? "LARGO" : "SLIME")}: {display} ({slime.IdentifiableId})";
        }
    }
}