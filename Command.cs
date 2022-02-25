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
                Log.Error("you must specify the hyphen-separated list of pure slime names without spaces (ex: 'Pink-Rock')");
                return false;
            }
            args = args.Select(s => s.ToUpper()).ToArray();
            var end = 2;
            List<SlimeDefinition> components = null;
            var parts = string.Join(" ", args.Skip(1).Take(end)).Split('-');
            components = parts.Select(p => Core.pureSlimes[p]).ToList();
            if (args.Length < end + strat.types.Count)
            {
                Log.Error("you must provide parameters in the form of: " + string.Join(" ", strat.types.Select(t => t.type.Name)));
                return false;
            }
            var parameters = Core.GetParameters(strat, args.Skip(end + 1));
            Log.Debug($"Fusing in mode {strat.blame}... (on {string.Join(", ", components.Select(Core.GetFullName))}...) (with {string.Join(", ", parameters)}...)");
            var slime = Core.InvokeStrategy(strat, components, parameters);
            Log.Info($"Fusion resulted in {strat.DebugName(slime)}");
            return true;
        }
        public override List<string> GetAutoComplete(int argIndex, string argText)
        {
            Log.Debug(ConsoleWindow.cmdText);
            if (argIndex == 0)
                return Core.fusionStrats.Select(s => s.blame).ToList();
            if (argIndex == 1) {
                var chosen = argText.Substring(0, argText.LastIndexOf('-') + 1).ToUpper();
                return Core.pureSlimes.Keys.Select(k => chosen + k).ToList();
            }
            return base.GetAutoComplete(argIndex, argText);
        }
    }
}
