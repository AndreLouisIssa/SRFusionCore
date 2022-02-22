using SRML;
using SRML.Console;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SRFusionCore
{
    public class FuseCommand : ConsoleCommand
    {
        public override string Usage => "fuse <mode> <components> <parameters>";
        public override string ID => "fuse";
        public override string Description => "Create new slime definition using mod provided modes";
        public override bool Execute(string[] args)
        {

            if (args == null || args.Length < 1)
            {
                Log.Error("you must specify a mode to fuse using");
                return false;
            }
            var strat = FusionCore.fusionStrats.FirstOrDefault(s => s.blame == args[0]);
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
                components = parts.Select(p => p.Contains('_') ? '+' + p + '+' : p).Select(p => FusionCore.pureSlimes[p]).ToList();
            }
            else
            {
                components = FusionCore.GetPureSlimes(FusionCore.GetComponentSlimeNames(args[1]));
            }
            if (args.Length < end + strat.parameters.Count)
            {
                Log.Error("you must provide parameters in the form of: " + string.Join(" ", strat.parameters.Select(t => t.Name)));
                return false;
            }
            var parameters = new List<Parameter>();
            // TODO: implement parameters parsing
            Log.Debug($"Fusing in mode {strat.blame}... (on {string.Join(", ", components.Select(s => s.IdentifiableId.ToString()))}...) (with {string.Join(", ", parameters)}...)");
            var slime = FusionCore.InvokeStrategy(strat, components, parameters);
            Log.Info($"Produced fusion {slime.IdentifiableId}!");
            return true;
        }
        public override List<string> GetAutoComplete(int argIndex, string argText)
        {
            if (argIndex == 0)
                return FusionCore.fusionStrats.Select(s => s.blame).ToList();
            if (argIndex == 1)
                return FusionCore.pureSlimes.Keys.ToList();
            Log.Debug(ConsoleWindow.cmdText);
            return base.GetAutoComplete(argIndex, argText);
        }
    }
}
