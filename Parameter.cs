using System;
using System.Collections.Generic;
using System.Linq;

namespace FusionCore
{
    public class Parameter
    {
        public class Type
        {
            public readonly System.Type type;
            public readonly string hint;
            public readonly Func<string, List<string>> auto;
            public readonly Func<string, object> parse;
            public readonly Func<object, string> represent = (v) => v.ToString();
            public Type(System.Type type, string hint, Func<string, List<string>> auto, Func<string, object> parse, Func<object, string> represent = null)
            {
                this.type = type; this.hint = hint; this.auto = auto; this.parse = parse; this.represent = represent ?? this.represent;
            }

            public static Type Bool = new Type(typeof(bool), "true/false", s => new List<string>{ true.ToString(), false.ToString() }, s => bool.Parse(s), s => s.ToString().ToLower());
            public static Type Int = new Type(typeof(int), "integer", s => new List<string>{ default(int).ToString() }, s => int.Parse(s));
            public static Type Float = new Type(typeof(float), "float", s => new List<string>{ default(float).ToString() }, s => float.Parse(s));
            public static Type Double = new Type(typeof(double), "double", s => new List<string>{ default(double).ToString() }, s => double.Parse(s));
            public static Type String = new Type(typeof(string), "string", s => new List<string>{ }, s => s);
            public static Type Slime = new Type(typeof(SlimeDefinition), "PINK_SLIME etc.",
                s => SRSingleton<GameContext>.Instance.SlimeDefinitions.Slimes.Select(Core.GetFullName).ToList(),
                s => Core.GetSlimeByFullName(s), s => ((SlimeDefinition)s).GetFullName());
            public static Type PureSlime = new Type(typeof(SlimeDefinition), "PINK etc.",
                s => Core.pureSlimes.Keys.ToList(), s => Core.pureSlimes[s], s => Core.PureName(((SlimeDefinition)s).GetFullName()));
            public static Type PurePair = new Type(typeof(List<SlimeDefinition>), "PINK-GOLD etc.",
            s => {
                var chosen = s.Substring(0, s.IndexOf('-') + 1).ToUpper();
                return Core.pureSlimes.Keys.Select(k => chosen + k).ToList();
            }, s => {
                return Core.PureSlimeFullNames(s).Select(Core.GetSlimeByFullName).ToList();
            },  l => {
                return string.Join("-",((List<SlimeDefinition>)l).Select(Core.GetFullName).Select(Core.PureName));
            });
            public static Type PureSlimes = new Type(typeof(List<SlimeDefinition>), "PINK-GOLD-BOOM etc.",
            s => {
                var chosen = s.Substring(0, s.LastIndexOf('-') + 1).ToUpper();
                return Core.pureSlimes.Keys.Select(k => chosen + k).ToList();
            }, s => {
                return Core.PureSlimeFullNames(s).Select(Core.GetSlimeByFullName).ToList();
            },  l => {
                return string.Join("-",((List<SlimeDefinition>)l).Select(Core.GetFullName).Select(Core.PureName));
            });
        }

        public readonly Type type;
        public readonly object value;

        public Parameter(Type type, object value)
        {
            this.type = type; this.value = Convert.ChangeType(value, type.type);
        }

        public static Parameter Parse(Type type, string value)
        {
            return new Parameter(type, type.parse(value));
        }

        public override string ToString()
        {
            return type.represent(value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}