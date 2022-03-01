using System;
using System.Collections.Generic;
using System.Linq;

namespace FusionCore
{
    public class Parameter
    {
        // @MagicGonads
        public class Form
        {
            public readonly Type type;
            public readonly string hint;
            public readonly Func<string, List<string>> auto;
            public readonly Func<string, object> read;
            public readonly Func<string, bool> check = (v) => true;
            public readonly Func<object, string> show = (v) => v.ToString().ToLower();
            public Form(Type type, string hint, Func<string, List<string>> auto, Func<string, object> read, Func<string, bool> check = null, Func<object, string> show = null)
            {
                this.type = type; this.hint = hint; this.auto = auto; this.read = read; this.check = check ?? this.check; this.show = show ?? this.show;
            }

            public virtual Form Is(string s)
            {
                return check(s) ? this : null;
            }

            public virtual Form Is(object o)
            {
                return type.IsAssignableFrom(o.GetType()) ? this : null;
            }

            public static Form Join(params Form[] types)
            {
                var type = types.First();
                foreach (var t in types.Skip(1))
                    { type = new JoinPair(type, t); }
                return type;
            }

            public class JoinPair : Form
            {
                public readonly Form from;
                public readonly Form to;

                public JoinPair(Form from, Form to) : base(
                    typeof(object), from.hint + "|" + to.hint, s => from.auto(s).Union(to.auto(s)).ToList(),
                    s => (from.Is(s) ?? to.Is(s)).read(s), s => from.check(s) || to.check(s), o => (from.Is(o) ?? to.Is(o)).show(o)
                ) { this.from = from; this.to = to; }

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

            public static Form Bool = new Form(typeof(bool), "true|false", s => new List<string>{ true.ToString(), false.ToString() }, s => bool.Parse(s), s => bool.TryParse(s, out _));
            public static Form Int = new Form(typeof(int), "integer", s => new List<string>{ default(int).ToString() }, s => int.Parse(s), s => int.TryParse(s, out _));
            public static Form Float = new Form(typeof(float), "float", s => new List<string>{ default(float).ToString() }, s => float.Parse(s), s => float.TryParse(s, out _));
            public static Form Double = new Form(typeof(double), "double", s => new List<string>{ default(double).ToString() }, s => double.Parse(s), s => double.TryParse(s, out _));
            public static Form String = new Form(typeof(string), "string", s => new List<string>{ }, s => s);
            
            public static Form Slime = new Form(typeof(SlimeDefinition), "PINK_SLIME etc.",
                s => GameContext.Instance.SlimeDefinitions.Slimes.Select(Core.GetFullName).ToList(),
                s => Core.GetSlimeByFullName(s), s => true, s => ((SlimeDefinition)s).GetFullName());
            public static Form PureSlime = new Form(typeof(SlimeDefinition), "PINK etc.",
                s => Core.pureSlimes.Keys.ToList(), s => Core.pureSlimes[s], s => true, s => Core.PureName(((SlimeDefinition)s).GetFullName()));
            public static Form PurePair = new Form(typeof(List<SlimeDefinition>), "PINK-GOLD etc.",
            s => {
                var chosen = s.Substring(0, s.IndexOf('-') + 1).ToUpper();
                return Core.pureSlimes.Keys.Select(k => chosen + k).ToList();
            }, s => {
                return Core.PureSlimeFullNames(s.ToUpper()).Select(Core.GetSlimeByFullName).ToList();
            }, s=> true, l => {
                return string.Join("-",((List<SlimeDefinition>)l).Select(Core.GetFullName).Select(Core.PureName));
            });
            public static Form PureSlimes = new Form(typeof(List<SlimeDefinition>), "PINK-GOLD etc.",
            s => {
                var chosen = s.Substring(0, s.LastIndexOf('-') + 1).ToUpper();
                return Core.pureSlimes.Keys.Select(k => chosen + k).ToList();
            }, s => {
                return Core.PureSlimeFullNames(s).Select(Core.GetSlimeByFullName).ToList();
            }, s => true, l => {
                return string.Join("-",((List<SlimeDefinition>)l).Select(Core.GetFullName).Select(Core.PureName));
            });
        }

        public readonly Form form;
        public readonly object value;

        public T GetValue<T>() => (T)value;

        public Parameter(Form form, object value)
        {
            this.form = form; this.value = value;
        }

        public static Parameter Parse(Form form, string value)
        {
            return new Parameter(form, form.read(value));
        }

        public override string ToString()
        {
            return form.show(value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}