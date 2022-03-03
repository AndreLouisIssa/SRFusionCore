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
        public static readonly Form modeForm = Form.Forms.Mode;
        public static readonly Form helpForm = Form.Forms.Singleton("help",0);
        public static readonly Form helpOrModeForm = Form.Forms.Join(helpForm, modeForm);

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
            if (!Parameter.TryParse(helpOrModeForm, args[0].ToLower(), out var blame))
                { Log.Error($"the mode \"{args[0].ToLower()}\" was not found"); return false; }
            if (blame.GetForm() == helpForm)
            {
                if (args.Length < 2)
                    { Log.Error("you must specify a mode to fuse using"); return false; }
                if (!Parameter.TryParse(modeForm, args[1].ToLower(), out blame))
                    { Log.Error($"the mode \"{args[1].ToLower()}\" was not found"); return false; }
                Help(blame.GetValue<Mode>(), null, (s) => Log.Info(s)); return true;
            }
            var mode = blame.GetValue<Mode>();
            if (args.Length < 2)
                { Help(mode, "too few arguments given"); return false; }
            List<SlimeDefinition> components = mode.ParseComponents(args[1]);
            if (args.Length < 2 + mode.Required.Count)
                { Help(mode, "too many arguments given"); return false; }
            if (!mode.Variadic.Any() && args.Length > 2 + mode.Required.Count + mode.Optional.Count)
                { Help(mode, "too many arguments given"); return false; }
            var parameters = mode.ParseParameters(args.Skip(2));
            mode.Produce(components, parameters);
            return true;
        }

        public override List<string> GetAutoComplete(int argIndex, string argText)
        {
            // @MagicGonads
            var args = ConsoleWindow.cmdText.Split(' ').Skip(1).ToList();
            if (argIndex == 0) return helpOrModeForm.auto(argText);
            if (!Parameter.TryParse(helpOrModeForm, args[0].ToLower(), out var blame))
                return base.GetAutoComplete(argIndex, argText);
            if (blame.GetForm() == helpForm && argIndex == 1) return modeForm.auto(argText);
            var mode = blame.GetValue<Mode>();
            if (argIndex == 1) return mode.Fusion.auto(argText);
            if (args.Skip(2).Any(s => s.Contains("\""))) return base.GetAutoComplete(argIndex, argText);
            int i = 1 + mode.Required.Count; int j = i + mode.Optional.Count;
            if (argIndex <= i) return mode.Required[argIndex - 1].form.auto(argText);
            if (mode.Variadic.Any() || argIndex <= j) return mode.Optional[argIndex - i - 1].form.auto(argText);
            if (mode.Variadic.Any()) return mode.Variadic.SkipWhile((t, k) => k < argIndex - j)
                    .TakeWhile((t, k) => k == 0).SelectMany(t => t.form.auto(argText)).ToList();
            return base.GetAutoComplete(argIndex, argText);
        }
    }
}
