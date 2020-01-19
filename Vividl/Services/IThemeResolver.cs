using System;
using Vividl.Model;

namespace Vividl.Services
{
    /// <summary>
    /// Provides methods related to application theming.
    /// </summary>
    public interface IThemeResolver
    {
        /// <summary>
        /// Applies the given color theme to the UI.
        /// </summary>
        void SetColorScheme(Theme colorTheme);
    }
}
