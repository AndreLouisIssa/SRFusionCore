using SRML;
using SRML.Console;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace FusionCore
{
    public class Command : ConsoleCommand
    {
        public override string Usage => "fuse <mode> <components> <parameters>...";
        public override string ID => "fuse";
        public override string Description => "Create new slime definition using mod provided modes";
        public override bool Execute(string[] args)
        {

            if (args == null || args.Length < 1)
            {
                Log.Error("you must specify a mode to fuse using");
                return false;
            }
            var strat = Core.fusionStrats.FirstOrDefault(s => s.blame == args[0]);
            if (strat is null)
            {
                Log.Error($"the mode \"{args[0]}\" was not found");
                return false;
            }
            if (args.Length < 2)
            {
                Log.Error("you must specify the desired combined slime name (ex: PINK_ROCK) or a list of pure slime names enclosed in brackets (ex: '[PINK, ROCK]')");
                return false;
            }
            var end = 1;
            List<SlimeDefinition> components = null;
            if (args[1].StartsWith("["))
            {
                end = args.Select((x, i) => (x, i)).First(p => p.Item1.EndsWith("]")).Item2;
                var parts = Regex.Replace(string.Join(" ", args.Skip(1).Take(end)), @"[, ][ ]*", " ").Replace("[", "").Replace("]", "").Split(' ');
                components = parts.Select(p => p.Contains('_') ? '+' + p + '+' : p).Select(p => Core.pureSlimes[p]).ToList();
            }
            else
            {
                components = Core.GetPureSlimes(Core.GetComponentSlimeNames(args[1]));
            }
            if (args.Length < end + strat.types.Count)
            {
                Log.Error("you must provide parameters in the form of: " + string.Join(" ", strat.types.Select(t => t.type.Name)));
                return false;
            }
            var parameters = args.Skip(end).Select((s,i) => new Parameter(strat.types[i], s)).ToList();
            Log.Debug($"Fusing in mode {strat.blame}... (on {string.Join(", ", components.Select(s => s.IdentifiableId.ToString()))}...) (with {string.Join(", ", parameters)}...)");
            var slime = Core.InvokeStrategy(strat, components, parameters);
            Log.Info($"Produced fusion {slime.IdentifiableId}!");
            return true;
        }
        public override List<string> GetAutoComplete(int argIndex, string argText)
        {
            Log.Debug(ConsoleWindow.cmdText);
            if (argIndex == 0)
                return Core.fusionStrats.Select(s => s.blame).ToList();
            if (argIndex == 1)
                return Core.pureSlimes.Keys.ToList();
            return base.GetAutoComplete(argIndex, argText);
        }
    }
}
