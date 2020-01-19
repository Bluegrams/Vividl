using System;
using System.Windows;
using System.Windows.Controls;

namespace Vividl.Helpers
{
    public static class ButtonEx
    {
        public static readonly DependencyProperty CloseWindowProperty =
            DependencyProperty.RegisterAttached(
                "CloseWindow", typeof(bool), typeof(ButtonEx),
                new PropertyMetadata(false, onPropertyChanged)
                );

        public static void SetCloseWindow(UIElement element, bool value)
            => element.SetValue(CloseWindowProperty, value);

        public static bool GetCloseWindow(UIElement element)
            => (bool)element.GetValue(CloseWindowProperty);

        private static void onPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button but)
            {
                bool oldVal = (bool)e.OldValue, newVal = (bool)e.NewValue;
                if (!oldVal && newVal)
                    but.Click += buttonClick;
                else if (oldVal && !newVal)
                    but.Click -= buttonClick;
            }
        }

        private static void buttonClick(object sender, RoutedEventArgs e)
            => Window.GetWindow((DependencyObject)sender).Close();
    }
}
