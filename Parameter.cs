using System;
using System.Collections.Generic;
using System.Linq;

namespace FusionCore
{
    public class Parameter
    {
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