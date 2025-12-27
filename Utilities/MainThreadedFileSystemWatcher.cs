using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using Terraria;

namespace CalamityMod.Utilities
{
    internal sealed class MainThreadedFileSystemWatcher : IDisposable
    {
        public string Path
        {
            get => _FSW.Path;
            set => _FSW.Path = value;
        }

        public string Filter
        {
            get => _FSW.Filter;
            set => _FSW.Filter = value;
        }

        public Collection<string> Filters
        {
            get => _FSW.Filters;
        }

        public NotifyFilters NotifyFilter
        {
            get => _FSW.NotifyFilter;
            set => _FSW.NotifyFilter = value;
        }

        public bool IncludeSubdirectories
        {
            get => _FSW.IncludeSubdirectories;
            set => _FSW.IncludeSubdirectories = value;
        }

        public bool EnableRaisingEvents
        {
            get => _FSW.EnableRaisingEvents;
            set => _FSW.EnableRaisingEvents = value;
        }

        public Regex FileNameFilter { get; set; } = null;

        public event FileSystemEventHandler Changed;
        public event RenamedEventHandler Renamed;

        public TimeSpan ChangedEventCooldown { get; set; } = TimeSpan.FromSeconds(0.1f);
        public TimeSpan RenamedEventCooldown { get; set; } = TimeSpan.FromSeconds(0.1f);

        private FileSystemWatcher _FSW;
        private DateTime _LastChangedEventDateTime = DateTime.Now;
        private DateTime _LastRenamedEventDateTime = DateTime.Now;
        private bool disposedValue;

        public MainThreadedFileSystemWatcher()
        {
            _FSW = new();
            _FSW.Changed += (o, arg) =>
            {
                if (DateTime.Now - _LastChangedEventDateTime < ChangedEventCooldown)
                    return;

                if (!(FileNameFilter?.IsMatch(System.IO.Path.GetFileName(arg.Name)) ?? true))
                    return;

                Main.QueueMainThreadAction(() => Changed?.Invoke(o, arg));
                _LastChangedEventDateTime = DateTime.Now;
            };

            _FSW.Renamed += (o, arg) =>
            {
                if (DateTime.Now - _LastRenamedEventDateTime < RenamedEventCooldown)
                    return;

                if (!(FileNameFilter?.IsMatch(System.IO.Path.GetFileName(arg.Name)) ?? true))
                    return;

                Main.QueueMainThreadAction(() => Renamed?.Invoke(o, arg));
                _LastRenamedEventDateTime = DateTime.Now;
            };
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _FSW?.Dispose();
                }

                _FSW = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
