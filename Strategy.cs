using System;
using System.Collections;
using System.Collections.Generic;

namespace FusionCore
{
    public class Strategy
    {
        public static IEnumerable<(Parameter.Type, string)> EmptyVariadic() { yield break; }
        public static IEnumerable<(Parameter.Type, string)> RepeatVariadic(Parameter.Type type, string label) { while (true) yield return (type, label); }

        public delegate SlimeDefinition Factory(ref List<SlimeDefinition> components, ref List<Parameter> parameters);

        public string blame;
        public string category;
        public List<(Parameter.Type, string)> required;
        public List<(Parameter.Type, string, object)> optional;
        public IEnumerable<(Parameter.Type, string)> variadic;
        public Factory factory;

        public Strategy(string blame, Factory factory, string category = "SLIME",
            List<(Parameter.Type, string)> required = null, List<(Parameter.Type, string, object)> optional = null, IEnumerable<(Parameter.Type, string)> variadic = null)
        {
            this.blame = blame.ToUpper();
            this.category = category.ToUpper();
            this.required = required ?? new List<(Parameter.Type, string)>();
            this.optional = optional ?? new List<(Parameter.Type, string, object)>();
            this.variadic = variadic ?? EmptyVariadic();
            this.factory = factory;
        }

        public override int GetHashCode()
        {
            return blame.GetHashCode();
        }
    }
}