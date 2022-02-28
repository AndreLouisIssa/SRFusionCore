using SRML.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using static FusionCore.Core;

namespace FusionCore
{
    public class Fuse : ConsoleCommand
    {
        // @MagicGonads @Aidanamite
        public static readonly Fuse instance = new Fuse();

        public override string Usage => "fuse <mode> <components> <required> <optional> <variadic>";
        public override string ID => "fuse";
        public override string Description => "Create new slime definition using mod provided modes";

        public void Help(Mode strat, string preface = null, Action<string> log = null)
        {
            // @MagicGonads
            if (log == null) log = (s) => Log.Error(s);
            var clamp = !strat.Variadic.Any();
            var any = strat.Required.Any() || (clamp && strat.Optional.Any());
            log($"{(preface != null ? preface + " as " : "")}the {TitleCase(strat.Blame)} mode takes {(!clamp ? "at least " : "") }{1 + strat.Required.Count}" +
                $"{(clamp ? $" to {1 + strat.Required.Count + strat.Optional.Count}" : "")} arg{(any ? "s" : "")}");
            Log.Info($"required: {$"<fusion ({strat.Fusion.hint})>"} {string.Join(" ", strat.Required.Select(t => $"<{t.label ?? ""} ({t.form.hint})>"))}");
            if (strat.Optional.Any()) Log.Info($"optional: {string.Join(" ", strat.Optional.Select(t => $"<{t.label ?? ""} ({t.form.hint}) = {t.init}>"))}");
            if (strat.Variadic.Any()) Log.Info($"variadic: {string.Join(" ", strat.Variadic.TakeWhile((t, i) => i < 6).Select(t => $"<{t.label ?? ""} ({t.form.hint})>"))}...");
        }

        public override bool Execute(string[] args)
        {
            // @MagicGonads
            if (args == null || args.Length < 1)
                { Log.Error("you must specify a mode to fuse using"); return false; }
            var blame = args[0].ToLower();
            var help = false;
            if (blame == "help" && args.Length == 2)
                { help = true; blame = args[1].ToLower(); }
            if (!fusionModes.TryGetValue(blame, out var strat))
                { Log.Error($"the mode \"{blame}\" was not found"); return false; }
            if (help)
                { Help(strat, null, (s) => Log.Info(s)); return true; }
            if (args.Length < 2)
                { Help(strat, "too few arguments given"); return false; }
            List<SlimeDefinition> components = strat.ParseComponents(args[1]);
            if (args.Length < 2 + strat.Required.Count)
                { Help(strat, "too many arguments given"); return false; }
            if (!strat.Variadic.Any() && args.Length > 2 + strat.Required.Count + strat.Optional.Count)
                { Help(strat, "too many arguments given"); return false; }
            var parameters = strat.ParseParameters(args.Skip(2));
            strat.Produce(components, parameters);
            return true;
        }

        public override List<string> GetAutoComplete(int argIndex, string argText)
        {
            // @MagicGonads
            var args = ConsoleWindow.cmdText.Split(' ').Skip(1).ToList();
            if (argIndex == 0) return fusionModes.Keys.Select(s => TitleCase(s)).Append("help").ToList();
            var blame = args[0].ToLower();
            if (blame == "help" && argIndex == 1) return fusionModes.Keys.Select(s => TitleCase(s)).ToList();
            if (!fusionModes.TryGetValue(blame, out var strat)) return base.GetAutoComplete(argIndex, argText);
            if (argIndex == 1) return strat.Fusion.auto(argText);
            if (args.Skip(2).Any(s => s.Contains("\""))) return base.GetAutoComplete(argIndex, argText);
            int i = 1 + strat.Required.Count; int j = i + strat.Optional.Count;
            if (argIndex <= i) return strat.Required[argIndex - 1].form.auto(argText);
            if (strat.Variadic.Any() || argIndex <= j) return strat.Optional[argIndex - i - 1].form.auto(argText);
            if (strat.Variadic.Any()) return strat.Variadic.SkipWhile((t, k) => k < argIndex - j)
                    .TakeWhile((t, k) => k == 0).SelectMany(t => t.form.auto(argText)).ToList();
            return base.GetAutoComplete(argIndex, argText);
        }
    }
}
