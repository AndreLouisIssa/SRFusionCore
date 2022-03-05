using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FusionCore
{
    // @MagicGonads
    public class Form
    {
        public readonly Type type;
        public readonly bool isEnum;
        public readonly string hint;
        public readonly Func<string, List<string>> auto;
        public readonly Func<string, object> read = (s) => s;
        public readonly Func<string, bool> check = (s) => true;
        public readonly Func<object, string> show = (v) => v.ToString();
            

        public Form(Type type, bool isEnum, string hint, Func<string, List<string>> auto,
            Func<string, object> read, Func<string, bool> check = null, Func<object, string> show = null)
        {
            this.type = type; this.isEnum = isEnum; this.hint = hint; this.auto = auto;
            this.read = read; this.check = check ?? this.check; this.show = show ?? this.show;
        }

        public virtual Form Is(string s)
        {
            return check(s) ? this : null;
        }

        public virtual Form Is(object o)
        {
            return type.IsAssignableFrom(o.GetType()) ? this : null;
        }

        public class Join : Form
        {
            public readonly Form from;
            public readonly Form to;

            public Join(Form from, Form to) : base(
                typeof(object), from.isEnum && to.isEnum, from.hint + "|" + to.hint, s => from.auto(s).Union(to.auto(s)).ToList(),
                s => (from.Is(s) ?? to.Is(s)).read(s), s => from.check(s) || to.check(s), o => (from.Is(o) ?? to.Is(o)).show(o)
            )
            { this.from = from; this.to = to; }

            public override Form Is(string s)
            {
                var t = from.Is(s);
                if (t == null)
                    return to.Is(s);
                return t;
            }

            public override Form Is(object o)
            {
                var t = from.Is(o);
                if (t == null)
                    return to.Is(o);
                return t;
            }
        }

        public class Singleton<T> : Form
        {
            public readonly string name;
            public readonly T value;

            public Singleton(string name, T value) : base(
                typeof(T), false, $"'{name}'", s => new List<string>{ name },
                s => value, s => s == name, o => name
            ) { this.name = name; this.value = value; }

            public override Form Is(string s)
            {
                return (s == name) ? this : null;
            }

            public override Form Is(object o)
            {
                var t = value.GetType();
                if (o.GetType() != t) return null;
                if (t.IsValueType) return value.Equals(o) ? this : null;
                return ReferenceEquals(o,value) ? this : null;
            }
        }

        public static Dictionary<string, Form> instances = new Dictionary<string, Form>();

        public static class Forms
        {
            public static Form Join(params Form[] forms) { return Join(forms.AsEnumerable()); }
            public static Form Join(IEnumerable<Form> forms)
            {
                var type = forms.First();
                foreach (var t in forms.Skip(1))
                { type = new Join(type, t); }
                return type;
            }

            public static Form Singleton<T>(string name, T value)
            {
                return new Singleton<T>(name, value);
            }

            public static Form Null = new Singleton<object>("null", null);

            public static Form Bool = new Form(typeof(bool), false, "true|false", s => new List<string> { true.ToString(), false.ToString() }, s => bool.Parse(s), s => bool.TryParse(s, out _));
            public static Form Int = new Form(typeof(int), false, "integer", s => new List<string> { default(int).ToString() }, s => int.Parse(s), s => int.TryParse(s, out _));
            public static Form Float = new Form(typeof(float), false, "float", s => new List<string> { default(float).ToString() }, s => float.Parse(s), s => float.TryParse(s, out _));
            public static Form Double = new Form(typeof(double), false, "double", s => new List<string> { default(double).ToString() }, s => double.Parse(s), s => double.TryParse(s, out _));
            public static Form String = new Form(typeof(string), false, "string", s => new List<string> { }, s => s);

            public static Form Mode = new Form(typeof(Mode), false, "mode",
                s => Core.fusionModes.Keys.ToList(),
                s => Core.fusionModes[s], s => Core.fusionModes.ContainsKey(s), o => ((Mode)o).Blame);

            public static Form Slime = new Form(typeof(SlimeDefinition), true, "'PINK_SLIME' etc.",
                s => GameContext.Instance.SlimeDefinitions.Slimes.Select(Core.GetFullName).ToList(),
                s => Core.GetSlimeByFullName(s),s => GameContext.Instance.SlimeDefinitions.Slimes.Any(d => d.GetFullName() == s),
                s => ((SlimeDefinition)s).GetFullName());
            public static Form SlimePair = new Form(typeof(List<SlimeDefinition>), true, "'PINK_SLIME-GOLD_SLIME' etc.",
                s =>
                {
                    var chosen = s.Substring(0, s.IndexOf('-') + 1).ToUpper();
                    return GameContext.Instance.SlimeDefinitions.Slimes.Select(Core.GetFullName).Select(k => chosen + k).ToList();
                }, s =>
                {
                    return s.ToUpper().Split('-').Select(Core.GetSlimeByFullName).ToList();
                }, s => s.ToUpper().Split('-').All(n => GameContext.Instance.SlimeDefinitions.Slimes.Any(d => d.GetFullName() == n)), l =>
                {
                    return string.Join("-", ((List<SlimeDefinition>)l).Select(Core.GetFullName));
                });
            public static Form Slimes = new Form(typeof(List<SlimeDefinition>), false, "'PINK_SLIME-GOLD_SLIME-SABER_SLIME' etc.",
                s =>
                {
                    var chosen = s.Substring(0, s.LastIndexOf('-') + 1).ToUpper();
                    return GameContext.Instance.SlimeDefinitions.Slimes.Select(Core.GetFullName).Select(k => chosen + k).ToList();
                }, s =>
                {
                    return s.ToUpper().Split('-').Select(Core.GetSlimeByFullName).ToList();
                }, s => s.ToUpper().Split('-').All(n => GameContext.Instance.SlimeDefinitions.Slimes.Any(d => d.GetFullName() == n)), l =>
                {
                    return string.Join("-", ((List<SlimeDefinition>)l).Select(Core.GetFullName));
                });

            public static Form PureSlime = new Form(typeof(SlimeDefinition), false, "'PINK' etc.",
                s => Core.pureSlimes.Keys.ToList(), s => Core.pureSlimes[s], s => true, s => Core.PureName(((SlimeDefinition)s).GetFullName()));
            public static Form PureSlimePair = new Form(typeof(List<SlimeDefinition>), false, "'PINK-GOLD' etc.",
                s =>
                {
                    var chosen = s.Substring(0, s.IndexOf('-') + 1).ToUpper();
                    return Core.pureSlimes.Keys.Select(k => chosen + k).ToList();
                }, s =>
                {
                    return Core.PureSlimeFullNames(s.ToUpper()).Select(Core.GetSlimeByFullName).ToList();
                }, s => true, l =>
                {
                    return string.Join("-", ((List<SlimeDefinition>)l).Select(Core.GetFullName).Select(Core.PureName));
                });
            public static Form PureSlimes = new Form(typeof(List<SlimeDefinition>), false, "'PINK-GOLD-SABER' etc.",
                s =>
                {
                    var chosen = s.Substring(0, s.LastIndexOf('-') + 1).ToUpper();
                    return Core.pureSlimes.Keys.Select(k => chosen + k).ToList();
                }, s =>
                {
                    return Core.PureSlimeFullNames(s).Select(Core.GetSlimeByFullName).ToList();
                }, s => true, l =>
                {
                    return string.Join("-", ((List<SlimeDefinition>)l).Select(Core.GetFullName).Select(Core.PureName));
                });
        }
    }
}
