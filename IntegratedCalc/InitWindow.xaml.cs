﻿using Blue.Windows;

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
        private SettingsProvider<SettingsObject> _defaultProvider;
        public InitWindow()
        {
            InitializeComponent();
            // Load Settings
            var serializer = new Newtonsoft.Json.JsonSerializer();
            serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
            serializer.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            _defaultProvider = new SettingsProvider<SettingsObject>("settings.json", serializer, null);
            if (!File.Exists("settings.json"))
            {
                _defaultProvider.SaveAsync().ConfigureAwait(false);
            }
            SettingsManager.Add(_defaultProvider);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                StickyWindow.RegisterExternalReferenceForm(this);
                Hide();
            }), null);
            var mw = new MainWindow(_defaultProvider);
            mw.Closed += Mw_Closed;
            mw.Show();
        }

        private void Mw_Closed(object sender, EventArgs e)
        {
            SaveSettings(_defaultProvider);
            SettingsManager.Clear();
            Close();
        }

        internal static async Task SaveSettingsAsync(SettingsProvider<SettingsObject> provider)
        {
            await Task.Run(() => SaveSettings(provider));
        }

        internal static void SaveSettings(SettingsProvider<SettingsObject> provider)
        {
            // The deserializer duplicates webcmdlets once everytime its loads settings.json, I cant figure out the reason why or where
            // Duplicates are removed, when trying to add the commands, but still the json is getting spammed, so here I select the entries with a distinct cmdlet to remove duplicates.
            // Distinct doesnt work either whyever, and it needs a equalitycomparer :(. So I made my own distinct extention using a equalitycomparison which WORKs, tho its literally a copy and paste from the equalitycomparer.... Microsoft pls fix!
            provider.Current.WebCommands = provider.Current.WebCommands.Distinct((x, y) => x.Cmdlet.Equals(y.Cmdlet, StringComparison.InvariantCultureIgnoreCase)).ToList();
            provider.Save();
        }
    }
}