using System;
using System.Collections.Generic;
using System.Linq;

namespace FusionCore
{
    public class Parameter
    {
        public class Form
        {
            public readonly System.Type type;
            public readonly string hint;
            public readonly Func<string, List<string>> auto;
            public readonly Func<string, object> parse;
            public readonly Func<object, string> represent = (v) => v.ToString();
            public Form(System.Type type, string hint, Func<string, List<string>> auto, Func<string, object> parse, Func<object, string> represent = null)
            {
                this.type = type; this.hint = hint; this.auto = auto; this.parse = parse; this.represent = represent ?? this.represent;
            }

            public T Parse<T>(string s) => (T)parse(s);

            public static Form Bool = new Form(typeof(bool), "true/false", s => new List<string>{ true.ToString(), false.ToString() }, s => bool.Parse(s), s => s.ToString().ToLower());
            public static Form Int = new Form(typeof(int), "integer", s => new List<string>{ default(int).ToString() }, s => int.Parse(s));
            public static Form Float = new Form(typeof(float), "float", s => new List<string>{ default(float).ToString() }, s => float.Parse(s));
            public static Form Double = new Form(typeof(double), "double", s => new List<string>{ default(double).ToString() }, s => double.Parse(s));
            public static Form String = new Form(typeof(string), "string", s => new List<string>{ }, s => s);
            public static Form Slime = new Form(typeof(SlimeDefinition), "PINK_SLIME etc.",
                s => SRSingleton<GameContext>.Instance.SlimeDefinitions.Slimes.Select(Core.GetFullName).ToList(),
                s => Core.GetSlimeByFullName(s.ToUpper()), s => ((SlimeDefinition)s).GetFullName());
            public static Form PureSlime = new Form(typeof(SlimeDefinition), "PINK etc.",
                s => Core.pureSlimes.Keys.ToList(), s => Core.pureSlimes[s.ToUpper()], s => Core.PureName(((SlimeDefinition)s).GetFullName()));
            public static Form PurePair = new Form(typeof(List<SlimeDefinition>), "PINK-GOLD etc.",
            s => {
                var chosen = s.Substring(0, s.IndexOf('-') + 1).ToUpper();
                return Core.pureSlimes.Keys.Select(k => chosen + k).ToList();
            }, s => {
                return Core.PureSlimeFullNames(s.ToUpper()).Select(Core.GetSlimeByFullName).ToList();
            },  l => {
                return string.Join("-",((List<SlimeDefinition>)l).Select(Core.GetFullName).Select(Core.PureName));
            });
            public static Form PureSlimes = new Form(typeof(List<SlimeDefinition>), "PINK-GOLD-BOOM etc.",
            s => {
                var chosen = s.Substring(0, s.LastIndexOf('-') + 1).ToUpper();
                return Core.pureSlimes.Keys.Select(k => chosen + k).ToList();
            }, s => {
                return Core.PureSlimeFullNames(s.ToUpper()).Select(Core.GetSlimeByFullName).ToList();
            },  l => {
                return string.Join("-",((List<SlimeDefinition>)l).Select(Core.GetFullName).Select(Core.PureName));
            });
        }

        public readonly Form type;
        public readonly object value;

        public T GetValue<T>() => (T)value;

        public Parameter(Form type, object value)
        {
            this.type = type; this.value = Convert.ChangeType(value, type.type);
        }

        public static Parameter Parse(Form type, string value)
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