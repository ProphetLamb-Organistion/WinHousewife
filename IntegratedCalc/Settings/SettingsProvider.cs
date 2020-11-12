using Newtonsoft.Json;

using System;
using System.IO;
using System.Threading.Tasks;

namespace IntegratedCalc.Settings
{
    public interface ISettingsProvider
    {
        public object Current { get; }
        public string FileName { get; }
        public Task GetAsync();
        public Task SaveAsync();
    }

    public class SettingsProvider<T> : ISettingsProvider where T : new()
    {
        protected readonly string m_fileName;
        protected readonly JsonSerializer m_serializer;
        protected T m_current;

        public SettingsProvider(string fileName, JsonSerializer serializer, T defaultSettings)
        {
            if (String.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("Filename can not be null or whitespace.", nameof(fileName));
            m_fileName = fileName;
            m_serializer = serializer;
            m_current = defaultSettings ?? new T();
        }

        public string FileName => m_fileName;
        public T Current => m_current;
        object ISettingsProvider.Current => m_current;
        public JsonSerializer Serializer => m_serializer;

        public async Task GetAsync()
        {
            if (File.Exists(m_fileName))
            {
                using var fs = new FileStream(m_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var sr = new StreamReader(fs);
                using var jr = new JsonTextReader(sr);
                m_current = await Task.Run(() => m_serializer.Deserialize<T>(jr)) ?? new T();
            }
            else
            {
                m_current = new T();
            }
        }
        public void Get()
        {
            if (File.Exists(m_fileName))
            {
                using var fs = new FileStream(m_fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var sr = new StreamReader(fs);
                using var jr = new JsonTextReader(sr);
                m_current = m_serializer.Deserialize<T>(jr) ?? new T();
            }
            else
            {
                m_current = new T();
            }
        }

        public async Task SaveAsync()
        {

            using var fs = new FileStream(m_fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            using var sw = new StreamWriter(fs);
            using var jw = new JsonTextWriter(sw);
            await Task.Run(() => m_serializer.Serialize(jw, m_current));
        }

        public void Save()
        {
            using var fs = new FileStream(m_fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            using var sw = new StreamWriter(fs);
            using var jw = new JsonTextWriter(sw);
            m_serializer.Serialize(jw, m_current);
        }
    }
}
