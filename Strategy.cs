using System;
using System.Collections.Generic;

namespace FusionCore
{
    public class Strategy
    {
        public string blame;
        public string category;
        public List<(Parameter.Type, string)> required;
        public List<(Parameter.Type, string, object)> optional;
        public (Parameter.Type, string)? variadic;
        public Func<List<SlimeDefinition>, List<Parameter>, SlimeDefinition> factory;

        public Strategy(string blame, Func<List<SlimeDefinition>, List<Parameter>, SlimeDefinition> factory, string category = "SLIME",
            List<(Parameter.Type, string)> required = null, List<(Parameter.Type, string, object)> optional = null, (Parameter.Type, string)? variadic = null)
        {
            this.blame = blame.ToUpper();
            this.category = category.ToUpper();
            this.required = required ?? new List<(Parameter.Type, string)>();
            this.optional = optional ?? new List<(Parameter.Type, string, object)>();
            this.variadic = variadic;
            this.factory = factory;
        }

        public override int GetHashCode()
        {
            return blame.GetHashCode();
        }
    }
}