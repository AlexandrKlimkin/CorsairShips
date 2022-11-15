using Avalonia;
using Avalonia.Markup.Xaml;

namespace PackageManager
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
