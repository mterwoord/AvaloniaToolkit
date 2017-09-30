using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Diagnostics;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using AvaloniaToolkit.Controls;
using AvaloniaToolkit.Imaging;

namespace AvaloniaToolkit.Sample
{
    public class MainWindow: Window
    {
        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);
            DevTools.Attach(this);

            var xPicker = this.FindControl<HueRingPicker>("colorPicker");
            xPicker.PropertyChanged += XPickerOnPropertyChanged;
        }

        private void XPickerOnPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == RangeBase.ValueProperty)
            {
                var xColor = ColorExtensions.FromHsv((double)e.NewValue, 1, 1);
                Background = new SolidColorBrush(xColor);
            }
        }
    }
}