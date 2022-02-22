using System;

namespace SRFusionCore
{
    public class Parameter
    {
        public class Type
        {
            public System.Type type;
            public Func<string, object> parse;
            public Func<object, string> represent = (v) => v.ToString();
            public Type(System.Type type, Func<string, object> parse, Func<object, string> represent = null)
            {
                this.type = type; this.parse = parse; this.represent = represent ?? this.represent;
            }

            public static Type Bool = new Type(typeof(bool), s => bool.Parse(s));
            public static Type Int = new Type(typeof(int), s => int.Parse(s));
            public static Type Float = new Type(typeof(float), s => float.Parse(s));
            public static Type Double = new Type(typeof(double), s => double.Parse(s));
            public static Type String = new Type(typeof(string), s => s);
        }

        public readonly Type type;
        public readonly object value;

        public Parameter(Type type, object value)
        {
            this.type = type; this.value = value;
        }
        public Parameter(Type type, string value)
        {
            this.type = type; this.value = type.parse(value);
        }

        public override string ToString()
        {
            return type.represent(value);
        }
    }
}