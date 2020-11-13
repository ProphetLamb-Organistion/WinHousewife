using Blue.Windows;

using CefSharp;
using CefSharp.DevTools;

using Hardcodet.Wpf.TaskbarNotification;

using IntegratedCalc.CommandLineIO;
using IntegratedCalc.Settings;

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace IntegratedCalc
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HotkeyHelper _helper;
        TaskbarIcon _taskbarIcon;
        StickyWindow _sticky;
        bool _forceClose;
        TextInOutHandler _clio;
        KeyData _showHotkey = new KeyData { Key.C, Key.LWin };
        KeyData _hideHotkey = new KeyData { Key.Escape };
        Size _restoreSize = s_reservedSize;
        static readonly Size s_reservedSize = new Size(Double.NaN, Double.NaN);
        KeyConverter _keyConverter = new KeyConverter();
        SettingsProvider<SettingsObject> _provider;

        public MainWindow(SettingsProvider<SettingsObject> provider)
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
            _provider = provider;
            _taskbarIcon = (TaskbarIcon)FindResource("TaskbarIcon");
            // TextInOutHandler
            _clio = new TextInOutHandler(RichTextOutput, TextInput);
            TextInput.PreviewKeyDown += _clio.InputKeyDown;
            TextInput.PreviewKeyDown += WebBrowserOutput_Hide_PreviewKeyDown;
            _clio.Close += ForceClose;
            _clio.Collpse += Collapse;
            _clio.Resize += (s, e) => SnapResize(e.Width, e.Height);
            _clio.Topmost += (s, topmost) => Topmost = topmost;
            _clio.NavigateBrowser += NavigateBrowser;
            _clio.ReloadSettings += ReloadSettings;
        }

        public void SnapResize(double width, double height)
        {
            WindowHelper.SnapOrigins origins = WindowHelper.GetSnapOrigins(this);
            WindowHelper.ResizeFitToScreen(this, Math.Max(MinWidth, width), Math.Max(MinHeight, height));
            WindowHelper.SnapToOrigins(this, origins);
        }

        private void Collapse(object sender = null, EventArgs e = null)
        {
            Visibility = Visibility.Collapsed;
            HideBrowser();
            ShowInTaskbar = false;
            TextInput.Clear();
        }

        private void Uncollapse(object sender = null, EventArgs e = null)
        {
            Visibility = Visibility.Visible;
            ShowInTaskbar = true;
            WindowHelper.SetFocus(new WindowInteropHelper(this).Handle);
            Activate();
        }

        private void ForceClose(object sender = null, EventArgs e = null)
        {
            _forceClose = true;
            Close();
        }

        private void NavigateBrowser(object sender, string url)
        {
            WebBrowserOutput.Address = url;
            ShowBrowser();
        }
        private void ReloadSettings(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                ApplySettings();
            });
        }

        private void ApplySettings()
        {
            Topmost = _provider.Current.IsTopmost;
            WindowHelper.ResizeFitToScreen(this, Math.Max(MinWidth, _provider.Current.StartupSize.Width), Math.Max(MinHeight, _provider.Current.StartupSize.Height));
            WindowHelper.SnapToOrigins(this, _provider.Current.StartupLocation);
            Resources["BackgroundBrush"] = new SolidColorBrush(_provider.Current.BackgroundColor);
            Resources["ForegroudBrush"] = new SolidColorBrush(_provider.Current.ForegroundColor);
            Resources["LightBackgroundBrush"] = new SolidColorBrush(_provider.Current.LightBackgroundColor);
            Resources["DisabledForegroudBrush"] = new SolidColorBrush(_provider.Current.DisabledForegroundColor);
        }

        private void ShowBrowser()
        {
            (WebBrowserOutput.Parent as UIElement).Visibility = Visibility.Visible;
            WebBrowserOutput.Focus();
            _restoreSize = new Size(Width, Height);
            SnapResize(_provider.Current.WebbrowserSize.Width, _provider.Current.WebbrowserSize.Height);

        }

        private void HideBrowser()
        {
            if (_restoreSize != s_reservedSize)
            {
                WebBrowserOutput.Address = "about:blank";
                (WebBrowserOutput.Parent as UIElement).Visibility = Visibility.Hidden;
                TextInput.Focus();
                SnapResize(_restoreSize.Width, _restoreSize.Height);
                _restoreSize = s_reservedSize;
            }
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
            this.HideMinimizeAndMaximizeButtons();
            FocusManager.SetFocusedElement(this, TextInput);
            _helper = new HotkeyHelper();
            _helper.RegisterHotkey(_showHotkey, this, OnShowHotkeyPressed);
            _helper.RegisterHotkey(_hideHotkey, this, OnHideHotkeyPressed);
            _sticky = new StickyWindow(this);
            _sticky.StickToScreen = true;
            _sticky.StickToOther = true;
            _sticky.StickOnResize = true;
            _sticky.StickOnMove = true;
            _sticky.StickGap = WindowHelper.SnapGap;
            ApplySettings();
        }

        public void OnShowHotkeyPressed(object sender, HotkeyCallbackArgs e)
        {
            Uncollapse();
        }
        public void OnHideHotkeyPressed(object sender, HotkeyCallbackArgs e)
        {
            Collapse();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!_forceClose)
            {
                Visibility = Visibility.Collapsed;
                e.Cancel = true;
            }
            else
            {
                _helper.UnregisterHotkey(_showHotkey, this);
                _helper.UnregisterHotkey(_hideHotkey, this);
                _taskbarIcon.Dispose();
                e.Cancel = false;
            }
            base.OnClosing(e);
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            ForceClose();
        }

        private void WebBrowserOutput_Hide_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.W && (e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0)
            {
                HideBrowser();
            }
        }

        private void WebBrowserOutput_IsBrowserInitializedChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (WebBrowserOutput.IsInitialized && !WebBrowserOutput.IsDisposed)
            {
                DevToolsClient devClient = WebBrowserOutput.GetDevToolsClient();
                string useragent = "Mozilla/5.0 (Linux; Android 9; SM-A102U) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.136 Mobile Safari/537.36";
                devClient.Emulation.SetUserAgentOverrideAsync(useragent).ConfigureAwait(false);
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _provider.Current.StartupSize = e.NewSize;
        }

        private void RichTextOutput_KeyDown(object sender, KeyEventArgs e)
        {
            ModifierKeys mods = e.KeyboardDevice.Modifiers;
            // Key text
            string text = _keyConverter.ConvertToString(e.Key);
            // Pass through specific keydowns to the input textbox
            bool passThrough = true;
            // Ignore modifier only events
            if ((String.IsNullOrEmpty(text) || text.Length > 1) && mods != 0)
                passThrough = false;
            // Keep Cntr & (A | C | V)
            else if (mods == ModifierKeys.Control && (e.Key == Key.A || e.Key == Key.C || e.Key == Key.V))
                passThrough = false;

            if (passThrough)
            {
                TextInput.Focus();
                e.Handled = true;
                TextCompositionManager.StartComposition(new TextComposition(
                    InputManager.Current,
                    TextInput,
                    // Account for case sensetivity
                    (mods & ModifierKeys.Shift) == 0 ? text.ToLower() : text.ToUpper()));
            }
        }

        private void RichTextOutput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down)
            {
                TextInput.Focus();
                e.Handled = true;
            }
        }

        private void ButtonOpenExternal_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start(WebBrowserOutput.Address);
        }

        private void TextAddress_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox address)
                address.SelectAll();
        }

        private void Window_LocationChanged(object sender, EventArgs e)
        {
            _provider.Current.StartupLocation = WindowHelper.GetSnapOrigins(this);
        }
    }
}
