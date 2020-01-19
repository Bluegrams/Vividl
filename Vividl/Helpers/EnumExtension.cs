using System;
using System.Linq;
using System.Windows.Markup;

namespace Vividl.Helpers
{
    public class EnumExtension : MarkupExtension
    {
        private readonly Type enumType;

        public EnumExtension(Type enumType)
        {
            this.enumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues(enumType).Cast<object>()
                .Select(o => new { Value = o, Description = o.ToString() });
        }
    }
}
