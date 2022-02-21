using SRML.SR.SaveSystem.Registry;
using System;
using System.Collections.Generic;

namespace SRFusionCore
{
    public class FusionStrategy
    {
        public string blame;
        public string category;
        public List<Type> parameters;
        public Func<List<SlimeDefinition>, List<Parameter>, SlimeDefinition> factory;

        public FusionStrategy(string blame, string category, List<Type> parameters, Func<List<SlimeDefinition>, List<Parameter>, SlimeDefinition> factory)
        {
            this.blame = blame;
            this.category = category;
            this.parameters = parameters;
            this.factory = factory;
        }

        public override bool Equals(object obj)
        {
            return obj is FusionStrategy other &&
                   blame == other.blame &&
                   category == other.category &&
                   EqualityComparer<List<Type>>.Default.Equals(parameters, other.parameters) &&
                   EqualityComparer<Func<List<SlimeDefinition>, List<Parameter>, SlimeDefinition>>.Default.Equals(factory, other.factory);
        }

        public override int GetHashCode()
        {
            int hashCode = 56328339;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(blame);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(category);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<Type>>.Default.GetHashCode(parameters);
            hashCode = hashCode * -1521134295 + EqualityComparer<Func<List<SlimeDefinition>, List<Parameter>, SlimeDefinition>>.Default.GetHashCode(factory);
            return hashCode;
        }

        public void Deconstruct(out string blame, out string category, out List<Type> parameters, out Func<List<SlimeDefinition>, List<Parameter>, SlimeDefinition> factory)
        {
            blame = this.blame;
            category = this.category;
            parameters = this.parameters;
            factory = this.factory;
        }

        public static implicit operator (string blame, string category, List<Type> parameters, Func<List<SlimeDefinition>, List<Parameter>, SlimeDefinition> factory)(FusionStrategy value)
        {
            return (value.blame, value.category, value.parameters, value.factory);
        }

        public static implicit operator FusionStrategy((string blame, string category, List<Type> parameters, Func<List<SlimeDefinition>, List<Parameter>, SlimeDefinition> factory) value)
        {
            return new FusionStrategy(value.blame, value.category, value.parameters, value.factory);
        }
    }
}