using System;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using PackageManager.Escape;

namespace PackageManager
{
    public class MainWindow : Window
    {
        private StackPanel Root;
        private StackPanel ContentScroller;
        private TextBox Log;
        private EscapeDllHellGenerator escape;
        private Button WriteButton;
        private Button RunButton;

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            Root = this.FindControl<StackPanel>(nameof(Root));
            ContentScroller = this.FindControl<StackPanel>(nameof(ContentScroller));
            Log = this.FindControl<TextBox>(nameof(Log));
            WriteButton = this.FindControl<Button>(nameof(WriteButton));
            RunButton = this.FindControl<Button>(nameof(RunButton));


            var projectDir = FindUnityProjectDirectory(Directory.GetCurrentDirectory());
            Directory.SetCurrentDirectory(projectDir);
            escape = new EscapeDllHellGenerator(projectDir);

            if (Program.options.UpdateDependencies)
            {
                escape.WriteBatch();
                Environment.Exit(0);
            }
            else
            {
                SetData(escape.ModuleStates);

                WriteButton.Click += WriteButton_Click;
                RunButton.Click += RunButton_Click;
                Logger.OnLog += OnLog;
            }
        }

        private static string FindUnityProjectDirectory(string dir)
        {
            string result = dir;

            while (Directory.GetDirectories(result).All(x => Path.GetFileName(x) != "Assets"))
            {
                var parent = Directory.GetParent(result);
                if (parent == null)
                {
                    throw new Exception("Can't find unity project directory");
                }

                result = Directory.GetParent(result).FullName;
            }

            return result + "/";
        }

        private void RunButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            escape.RunBatch();
        }

        private void WriteButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            escape.WriteBatch();
        }

        private void OnLog(LogLevel logLevel, string s)
        {
            var m = $"{logLevel}: {s}";
            Log.Text += m + "\n";
            Console.WriteLine(m);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void SetData(ModuleState[] modules)
        {
            var currentLibraryType = "";
            for (var i = 0; i < modules.Length; i++)
            {
                var m = modules[i];
                if (currentLibraryType != m.LibraryType)
                {
                    currentLibraryType = m.LibraryType;
                    var splitter = new TextBlock();
                    splitter.FontWeight = FontWeight.Bold;
                    splitter.Text = currentLibraryType;
                    ContentScroller.Children.Add(splitter);
                }

                var pm = new PackageModule(m, _ => escape.IsModuleExcluded(_));
                pm.Name += m.Name;
                pm.OnToggle += b => OnModuleTogle(pm, b);
                pm.IsEnabled = m.Name != EscapeDllHellGenerator.DllHellGeneratorProjectName;
                ContentScroller.Children.Add(pm);
            }
        }

        private void RefreshModules()
        {
            foreach (var c in ContentScroller.Children)
            {
                if (c is PackageModule pm)
                {
                    pm.UpdateState();
                }
            }
        }

        private void OnModuleTogle(PackageModule pm, bool isChecked)
        {
            pm.Module.UserEnabled = isChecked;
            if (isChecked)
                escape.AddDependencies(pm.Module, 0);
            else
                escape.RemoveModule(pm.Module);

            RefreshModules();
        }
    }
}
