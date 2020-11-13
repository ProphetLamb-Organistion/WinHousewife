using Blue.Windows;

using IntegratedCalc.Settings;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace IntegratedCalc
{
    /// <summary>
    /// Interaction logic for InitWindow.xaml
    /// </summary>
    public partial class InitWindow : Window
    {
        private SettingsProvider<SettingsObject> _provider;
        public InitWindow()
        {
            InitializeComponent();
            // Load Settings
            var serializer = new Newtonsoft.Json.JsonSerializer();
            serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
            serializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            _provider = new SettingsProvider<SettingsObject>("settings.json", serializer, null);
            SettingsManager.Add(_provider);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ExceptionExtentions.Try(() =>
            {
                if (!_provider.Get())
                {
                    _provider.Current = SettingsObject.Default;
                    _provider.SaveAsync().ConfigureAwait(false);
                }
            });
            Dispatcher.BeginInvoke(new Action(() =>
            {
                StickyWindow.RegisterExternalReferenceForm(this);
                Hide();
            }), null);
            var mw = new MainWindow(_provider);
            mw.Closed += Mw_Closed;
            mw.Show();
        }

        private void Mw_Closed(object sender, EventArgs e)
        {
            _provider.Save();
            SettingsManager.Clear();
            Close();
        }

        internal static async Task SaveSettingsAsync(SettingsProvider<SettingsObject> provider)
        {
            await Task.Run(provider.Save);
        }
    }
}
