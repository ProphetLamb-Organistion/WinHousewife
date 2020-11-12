using Gma.UserActivityMonitor;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace IntegratedCalc
{
    public class HotkeyCallbackArgs
    {
        public KeyData Keys { get; set; }
    }

    public class HotkeyHelper : IDisposable
    {
        private ILookup<KeyData, (object, EventHandler<HotkeyCallbackArgs>)> _lookup;
        private IList<(KeyData, (object, EventHandler<HotkeyCallbackArgs>))> _source;
        private KeyData _pressed;

        public HotkeyHelper()
        {
            _source = new List<(KeyData, (object, EventHandler<HotkeyCallbackArgs>))>();
            _pressed = new KeyData();
            ConstructLookup();
            HookManager.KeyDown += KeyDown;
            HookManager.KeyUp += KeyUp;
        }

        private void KeyDown(object sender, KeyEventArgs e)
        {
            _pressed.Add(e.KeyData);
            InvokeHotkeys();
            e.Handled = false;
        }

        private void KeyUp(object sender, KeyEventArgs e)
        {
            _pressed.Remove(e.KeyData);
            e.Handled = false;
        }

        private void InvokeHotkeys()
        {
            var args = new HotkeyCallbackArgs { Keys = _pressed };
            foreach ((object sender, EventHandler<HotkeyCallbackArgs> callback) in _lookup.Where(group => _pressed.Contains(group.Key)).SelectMany(group => group))
            {
                callback.Invoke(sender, args);
            }
        }

        private void ConstructLookup()
        {
            _lookup = _source.ToLookup(tuple => tuple.Item1, tuple => tuple.Item2);
        }

        public void RegisterHotkey(KeyData keys, object instance, EventHandler<HotkeyCallbackArgs> callback)
        {
            _source.Add((keys, (instance, callback)));
            ConstructLookup();
        }

        public void UnregisterHotkey(KeyData keys, object instance)
        {
            _source.Remove(_source.First(tuple => tuple.Item1 == keys && tuple.Item2.Item1 == instance));
            ConstructLookup();
        }

        #region IDisposable members
        private bool _disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    HookManager.KeyDown -= KeyDown;
                    HookManager.KeyUp -= KeyUp;
                }
                _source = null;
                _lookup = null;
                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
