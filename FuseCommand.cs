using SRML.Console;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SRFusionCore
{
    public class FuseCommand : ConsoleCommand
    {
        public override string Usage => "fuse <mode> <joined slime name / component slime names> <extra parameters...>";
        public override string ID => "fuse";
        public override string Description => "Create new slime definition using modes added by plugins to this core mod";
        public override bool Execute(string[] args)
        {

            if (args == null || args.Length < 2)
            {
                Log.Error("Not enough arguments, need at least 2");
                return false;
            }
            var strat = FusionCore.fusionStrats.FirstOrDefault(s => s.blame == args[0]);
            if (strat is null)
            {
                Log.Error($"\"{args[0]}\" is not a valid mode");
                return false;
            }
            List<SlimeDefinition> components = null;
            if (args[1].StartsWith("["))
            {
                var end = args.Select((x, i) => (x, i)).First(p => p.Item1.EndsWith("]")).Item2;
                var parts = Regex.Replace(string.Join(" ", args.Skip(1).Take(end)), @"[, ][ ]*", " ").Replace("[", "").Replace("]", "").Split(' ');
                components = parts.Select(p => p.Contains('_') ? '+' + p + '+' : p).Select(p => FusionCore.pureSlimes[p]).ToList();
            }
            else
            {
                components = FusionCore.GetPureSlimes(FusionCore.GetComponentSlimeNames(args[1]));
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
            return base.GetAutoComplete(argIndex, argText);
        }
    }
}
