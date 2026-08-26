using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.RegularExpressions;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Utilities;

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

    public event Action<FileSystemEventArgs> Changed;
    public event Action<RenamedEventArgs> Renamed;

    private Dictionary<string, FileSystemEventArgs> _ChangedQueue = [];
    private Dictionary<string, RenamedEventArgs> _RenamedQueue = [];

    private FileSystemWatcher _FSW;
    private bool _HasQueueInFrame = false;
    private bool _Disposed;

    public MainThreadedFileSystemWatcher()
    {
        _FSW = new();
        _FSW.Changed += (o, arg) =>
        {
            if (FileNameFilter != null && !FileNameFilter.IsMatch(System.IO.Path.GetFileName(arg.Name)))
                return;

            lock (_ChangedQueue)
            {
                _ChangedQueue[arg.FullPath] = arg;
                _HasQueueInFrame = true;
            }
        };

        _FSW.Renamed += (o, arg) =>
        {
            if (FileNameFilter != null && !FileNameFilter.IsMatch(System.IO.Path.GetFileName(arg.Name)))
                return;

            lock (_RenamedQueue)
            {
                _RenamedQueue[arg.FullPath] = arg;
                _HasQueueInFrame = true;
            }
        };

        MainThreadedFileSystemWatcherSystem.Register(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                _FSW?.Dispose();
            }

            _FSW = null;
            _Disposed = true;
            _ChangedQueue?.Clear();
            _RenamedQueue?.Clear();
            _ChangedQueue = null;
            _RenamedQueue = null;
            _HasQueueInFrame = false;
            MainThreadedFileSystemWatcherSystem.Unregister(this);
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private sealed class MainThreadedFileSystemWatcherSystem : ILoadable
    {
        private static MainThreadedFileSystemWatcher[] _Watchers = [];
        private static readonly HashSet<MainThreadedFileSystemWatcher> _WatchersList = [];

        public static void Register(MainThreadedFileSystemWatcher watcher)
        {
            _WatchersList.Add(watcher);
            _Watchers = [.. _WatchersList];
        }

        public static void Unregister(MainThreadedFileSystemWatcher watcher)
        {
            _WatchersList.Remove(watcher);
            _Watchers = [.. _WatchersList];
        }

        void ILoadable.Load(Mod mod)
        {
            Main.OnTickForThirdPartySoftwareOnly += Tick;
        }

        void ILoadable.Unload()
        {
            Main.OnTickForThirdPartySoftwareOnly -= Tick;
            _WatchersList?.Clear();
            _Watchers = [];
        }

        private void Tick()
        {
            foreach (var watcher in _Watchers)
            {
                if (!watcher._HasQueueInFrame)
                    continue;

                HandleQueuedEvents(watcher);
            }
        }

        private static void HandleQueuedEvents(MainThreadedFileSystemWatcher watcher)
        {
            lock (watcher._ChangedQueue)
            {
                foreach (var changed in watcher._ChangedQueue.Values)
                {
                    watcher.Changed?.Invoke(changed);
                }
                watcher._ChangedQueue.Clear();
            }

            lock (watcher._RenamedQueue)
            {
                foreach (var renamed in watcher._RenamedQueue.Values)
                {
                    watcher.Renamed?.Invoke(renamed);
                }
                watcher._RenamedQueue.Clear();
            }

            watcher._HasQueueInFrame = false;
        }
    }
}
