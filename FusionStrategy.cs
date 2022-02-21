using SRML.SR.SaveSystem.Registry;
using System;
using System.Collections.Generic;

namespace SRFusionCore
{
    public class FusionStrategy : IDataRegistryMember
    {
        public Action<Identifiable.Id, List<SlimeDefinition>, List<Parameter>> factory;
        public (string, string) category;
        public string blame;

        public FusionStrategy(Action<Identifiable.Id, List<SlimeDefinition>, List<Parameter>> item1, (string, string) item2, string item3)
        {
            factory = item1;
            category = item2;
            blame = item3;
        }

        public override bool Equals(object obj)
        {
            return obj is FusionStrategy other &&
                   EqualityComparer<Action<Identifiable.Id, List<SlimeDefinition>, List<Parameter>>>.Default.Equals(factory, other.factory) &&
                   category.Equals(other.category) &&
                   blame == other.blame;
        }

        public override int GetHashCode()
        {
            int hashCode = 341329424;
            hashCode = hashCode * -1521134295 + EqualityComparer<Action<Identifiable.Id, List<SlimeDefinition>, List<Parameter>>>.Default.GetHashCode(factory);
            hashCode = hashCode * -1521134295 + category.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(blame);
            return hashCode;
        }

        public void Deconstruct(out Action<Identifiable.Id, List<SlimeDefinition>, List<Parameter>> item1, out (string, string) item2, out string item3)
        {
            item1 = factory;
            item2 = category;
            item3 = blame;
        }

        Type IDataRegistryMember.GetModelType()
        {
            throw new NotImplementedException();
        }

        public static implicit operator (Action<Identifiable.Id, List<SlimeDefinition>, List<Parameter>>, (string, string), string)(FusionStrategy value)
        {
            return (value.factory, value.category, value.blame);
        }

        public static implicit operator FusionStrategy((Action<Identifiable.Id, List<SlimeDefinition>, List<Parameter>>, (string, string), string) value)
        {
            return new FusionStrategy(value.Item1, value.Item2, value.Item3);
        }
    }
}