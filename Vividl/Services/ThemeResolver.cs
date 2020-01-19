using System;
using System.Windows;
using System.Windows.Media;
using AdonisUI;
using Vividl.Model;

namespace Vividl.Services
{
    public class ThemeResolver : IThemeResolver
    {
        public void SetColorScheme(Theme colorTheme)
        {
            AccentColorScheme accentColorScheme;
            Uri colorScheme;
            switch (colorTheme)
            {
                case Theme.Dark:
                    accentColorScheme = DarkRed;
                    colorScheme = ResourceLocator.DarkColorScheme;
                    break;
                default:
                    accentColorScheme = LightRed;
                    colorScheme = ResourceLocator.LightColorScheme;
                    break;
            }
            accentColorScheme.ApplyColors(Application.Current.Resources);
            ResourceLocator.SetColorScheme(Application.Current.Resources, colorScheme);
        }

        // predefined accent color schemes for Vividl

        public static readonly AccentColorScheme LightRed = new AccentColorScheme()
        {
            AccentColor = Color.FromRgb(204, 102, 119),
            AccentHighlightColor = Color.FromRgb(168, 36, 58),
            AccentIntenseHighlightColor = Color.FromRgb(168, 36, 58),
            AccentInteractionColor = Color.FromRgb(168, 36, 58)
        };

        public static readonly AccentColorScheme DarkRed = new AccentColorScheme()
        {
            AccentColor = Color.FromRgb(168, 36, 58),
            AccentHighlightColor = Color.FromRgb(204, 102, 119),
            AccentIntenseHighlightColor = Color.FromRgb(204, 102, 119),
            AccentInteractionColor = Color.FromRgb(204, 102, 119)
        };
    }
}
