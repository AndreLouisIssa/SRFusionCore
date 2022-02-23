using System;

namespace FusionCore
{
    public class Parameter
    {
        public class Type
        {
            public readonly System.Type type;
            public readonly Func<string, object> parse;
            public readonly Func<object, string> represent = (v) => v.ToString();
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
        public readonly dynamic value;

        public Parameter(Type type, dynamic value)
        {
            this.type = type; this.value = (dynamic)Convert.ChangeType(value, type.type);
        }
        
        public static Parameter Parse(Type type, string value)
        {
            return new Parameter(type, type.parse(value));
        }

        public override string ToString()
        {
            return type.represent(value);
        }
    }
}