using SRML.Console;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FusionCore
{
    public class Command : ConsoleCommand
    {
        public override string Usage => "fuse <mode> <components> <required> <optional> <variadic>";
        public override string ID => "fuse";
        public override string Description => "Create new slime definition using mod provided modes";

        public void Help(Strategy strat, string preface = null, Action<string> log = null)
        {
            if (log == null) log = (s) => Log.Error(s);
            var clamp = !strat.variadic.Any();
            var any = strat.required.Any() || (clamp && strat.optional.Any());
            log($"{(preface != null ? preface + " as " : "")}the {Core.TitleCase(strat.blame)} mode takes {(!clamp ? "at least " : "") }{1 + strat.required.Count}" +
                $"{(clamp ? $" to {1 + strat.required.Count + strat.optional.Count}" : "")} arg{(any ? "s" : "")}");
            Log.Info($"required: {$"<fusion ({strat.fusion.hint})>"} {string.Join(" ", strat.required.Select(t => $"<{t.label ?? ""} ({t.type.hint})>"))}");
            if (strat.optional.Any()) Log.Info($"optional: {string.Join(" ", strat.optional.Select(t => $"<{t.label ?? ""} ({t.type.hint}) = {t.init}>"))}");
            if (strat.variadic.Any()) Log.Info($"variadic: {string.Join(" ", strat.variadic.TakeWhile((t, i) => i < 6).Select(t => $"<{t.label ?? ""} ({t.type.hint})>"))}...");
        }

        public override bool Execute(string[] args)
        {
            if (args == null || args.Length < 1)
                { Log.Error("you must specify a mode to fuse using"); return false; }
            var blame = args[0].ToUpper();
            var help = false;
            if (blame == "HELP" && args.Length == 2)
                { help = true; blame = args[1].ToUpper(); }
            var strat = Core.fusionStrats.FirstOrDefault(s => s.blame == blame);
            if (strat is null)
                { Log.Error($"the mode \"{blame}\" was not found"); return false; }
            if (help)
                { Help(strat, null, (s) => Log.Info(s)); return true; }
            if (args.Length < 2)
                { Help(strat, "too few arguments given"); return false; }
            List<SlimeDefinition> components = (List<SlimeDefinition>)strat.fusion.parse(args[1].ToUpper());
            if (args.Length < 2 + strat.required.Count)
                { Help(strat, "too many arguments given"); return false; }
            if (!strat.variadic.Any() && args.Length > 2 + strat.required.Count + strat.optional.Count)
                { Help(strat, "too many arguments given"); return false; }
            strat.Invoke(components, strat.ParseParameters(args.Skip(2)));
            return true;
        }

        public List<string> currentArgs = null;

        public override List<string> GetAutoComplete(int argIndex, string argText)
        {
            currentArgs = ConsoleWindow.cmdText.Split(' ').Skip(1).ToList();
            if (argIndex == 0) return Core.fusionStrats.Select(s => Core.TitleCase(s.blame)).Append("help").ToList();
            var blame = currentArgs[0].ToUpper();
            if (blame == "HELP" && argIndex == 1) return Core.fusionStrats.Select(s => Core.TitleCase(s.blame)).ToList();
            var strat = Core.fusionStrats.FirstOrDefault(s => s.blame == blame);
            if (strat == null) return base.GetAutoComplete(argIndex, argText);
            if (argIndex == 1) return strat.fusion.auto(argText);
            if (currentArgs.Skip(2).Any(s => s.Contains("\""))) return base.GetAutoComplete(argIndex, argText);
            int i = 1 + strat.required.Count; int j = i + strat.optional.Count;
            if (argIndex <= i) return strat.required[argIndex - 1].type.auto(argText);
            if (strat.variadic.Any() || argIndex <= j) return strat.optional[argIndex - i - 1].type.auto(argText);
            if (strat.variadic.Any()) return strat.variadic.SkipWhile((t, k) => k < argIndex - j)
                    .TakeWhile((t, k) => k == 0).SelectMany(t => t.type.auto(argText)).ToList();
            return base.GetAutoComplete(argIndex, argText);
        }
    }
}
