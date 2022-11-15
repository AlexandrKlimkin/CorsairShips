using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using PackageManager.Escape;

namespace PackageManager
{
    public class PackageModule : UserControl
    {
        private static readonly System.Drawing.Color UserEnabledColorSys = System.Drawing.Color.DarkGreen;
        private static readonly System.Drawing.Color EnabledColorSys = System.Drawing.Color.Orange;
        private static readonly System.Drawing.Color DisabledColorSys = System.Drawing.Color.Black;
        private static readonly IBrush UserEnabledBrush = new SolidColorBrush(Color.FromRgb(UserEnabledColorSys.R, UserEnabledColorSys.G, UserEnabledColorSys.B));
        private static readonly IBrush EnabledBrush = new SolidColorBrush(Color.FromRgb(EnabledColorSys.R, EnabledColorSys.G, EnabledColorSys.B));
        private static readonly IBrush DisabledBrush = new SolidColorBrush(Color.FromRgb(DisabledColorSys.R, DisabledColorSys.G, DisabledColorSys.B));
        private Func<string, bool> _excluded;
        private TextBlock HeaderText;
        private Expander Header;
        private StackPanel Dependencies;
        private CheckBox Toggle;
        public ModuleState Module { get; private set; }
        public event Action<bool> OnToggle = b => { };

        public PackageModule(ModuleState moduleState, Func<string, bool> excluded)
        {
            this.InitializeComponent();
            _excluded = excluded;
            IsEnabled = !excluded(moduleState.Name);

            Module = moduleState;
            Header = this.FindControl<Expander>(nameof(Header));
            Dependencies = this.FindControl<StackPanel>(nameof(Dependencies));
            Toggle = this.FindControl<CheckBox>(nameof(Toggle));

            HeaderText = new TextBlock();
            Header.Header = HeaderText;
            if(moduleState != null)
                SetData();

            Toggle.PropertyChanged += ToggleOnPropertyChanged;
        }

        private void ToggleOnPropertyChanged(object sender, AvaloniaPropertyChangedEventArgs avaloniaPropertyChangedEventArgs)
        {
            if (avaloniaPropertyChangedEventArgs.Property.Name == "IsChecked")
            {
                OnToggle((bool)avaloniaPropertyChangedEventArgs.NewValue);
            }
        }

        public void UpdateState()
        {
            if (Module.Enabled)
            {
                Toggle.IsChecked = Module.UserEnabled;
                HeaderText.FontWeight = FontWeight.Bold;
                HeaderText.Foreground = Module.UserEnabled ? UserEnabledBrush : EnabledBrush;
            }
            else
            {
                HeaderText.FontWeight = FontWeight.Normal;
                HeaderText.Foreground = DisabledBrush;
            }
            Header.IsEnabled = Module.Dependencies.Count > 0;
            HeaderText.Text = Module.Name;
        }

        private void SetData()
        {
            UpdateState();
            for (var i = 0; i < Module.Dependencies.Count; ++i)
            {
                var d = Module.Dependencies[i];
                var tb = new TextBlock() {Text = d};
                tb.Margin = new Thickness(25, 0, 0, 0);
                if (_excluded(d))
                    tb.Text += " (excluded)";

                Dependencies.Children.Add(tb);
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
