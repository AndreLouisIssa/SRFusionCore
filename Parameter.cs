namespace FusionCore
{
    public class Parameter
    {
        public readonly Form form;
        public readonly object value;

        public T GetValue<T>() => (T)value;

        public bool TryGetValue<T>(out T value)
        {
            value = default;
            if (!typeof(T).IsAssignableFrom(this.value.GetType()))
                return false;
            value = (T)this.value;
            return true;
        }
        public Parameter(Form form, object value)
        {
            this.form = form; this.value = value;
        }

        public static Parameter Parse(Form form, string value)
        {
            return new Parameter(form, form.read(value));
        }

        public static bool TryParse(Form form, string value, out Parameter param)
        {
            param = null;
            if (!form.check(value))
                return false;
            param = Parse(form, value);
            return true;
        }

        public Form GetForm()
        {
            return form.Is(value);
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