using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;

namespace Vividl.Helpers
{
    public class EnumExtension : MarkupExtension
    {
        private readonly Type enumType;

        public int SkipCount { get; set; }

        public EnumExtension(Type enumType)
        {
            this.enumType = enumType;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return Enum.GetValues(enumType).Cast<object>()
                .Skip(SkipCount)
                .Select(o => new { Value = o, Description = GetEnumDescription((Enum)o) });
        }

        public string GetEnumDescription(Enum value)
        {
            var attributes = value.GetType()
                .GetField(value.ToString())
                .GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes.Any())
            {
                return attributes.First().Description;
            }
            return value.ToString();
        }
    }
}
