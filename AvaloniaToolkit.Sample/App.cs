using Avalonia;
using Avalonia.Markup.Xaml;

namespace AvaloniaToolkit.Sample
{
    public class App: Application
    {
        public override void Initialize()
        {
            base.Initialize();
            AvaloniaXamlLoader.Load(this);
        }
    }
}