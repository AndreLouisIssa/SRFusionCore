using SRML.SR.SaveSystem.Registry;
using System;
using System.Collections.Generic;

namespace FusionCore
{
    public class Strategy
    {
        public string blame;
        public string category;
        public List<Parameter.Type> types;
        public Func<List<SlimeDefinition>, List<Parameter>, SlimeDefinition> factory;

        public Strategy(string blame, Func<List<SlimeDefinition>, List<Parameter>, SlimeDefinition> factory, string category = "SLIME", List<Parameter.Type> types = null)
        {
            this.blame = blame;
            this.category = category;
            this.types = types ?? new List<Parameter.Type>();
            this.factory = factory;
        }

        public override int GetHashCode()
        {
            return blame.GetHashCode();
        }
    }
}