#nullable enable
using System.Collections;
using System.Diagnostics;
using MusicLibrary.Cli.Playlists;
using MusicLibrary.Metadata;
using MusicLibrary.Playlists;

namespace MusicLibrary.Cli
{
    /// <summary>
    /// (Re)Scans music files and updates the database.
    /// </summary>
    internal sealed class Scanner : IDisposable
    {
        public Scanner(string rootDirectory,
            IDBManager dbManager,
            PlaylistFromFiles playlistFromFiles,
            CancellationTokenSource stopFlag)
        {
            if (rootDirectory == null) throw new ArgumentNullException(nameof(rootDirectory));
            if (!Directory.Exists(rootDirectory)) throw new ArgumentException($"{nameof(rootDirectory)} does not exist");

            Root = rootDirectory;
            _supportedExtensions = SupportedFilesAuthority.SupportedExtensions;
            _playlistExtensions = SupportedFilesAuthority.SupportedPlaylistExtensions;
            _allPlaylists = new List<IPersistedPlaylist>();
            _dbManager = dbManager;
            _playlistFromFiles = playlistFromFiles;
            _canContinue = () => true;
            _forceRescan = false;
            _stopFlag = stopFlag;
            _allTrackPaths = new List<string>();
        }

        private readonly List<string> _allTrackPaths;
        private IEnumerable<string>? _allDirectories;
        private IEnumerator? _allDirectoriesEnumerator;
        private readonly IList<string> _supportedExtensions;
        private readonly IList<string> _playlistExtensions;
        private readonly IList<IPersistedPlaylist> _allPlaylists;
        private readonly IDBManager _dbManager;
        private readonly Func<bool> _canContinue;
        private volatile string? _lastScannedDirectory;
        private volatile bool _aboutDone;
        private readonly bool _forceRescan;
        private readonly PlaylistFromFiles _playlistFromFiles;
        public event EventHandler<ScannedTrackEventArgs>? ScannedTrack;
        public event EventHandler<DirectoryScannedEventArgs>? ScanProgress;
        public event EventHandler<EventArgs>? ScanFinished;
        private readonly CancellationTokenSource _stopFlag;

        public string Root { get; }

        public bool IsCompleted
        {
            get => _aboutDone;
            set => _aboutDone = value;
        }

        /// <summary>
        /// Scans the root directory for new / updated files
        /// Pauses when the PauseUpdating parameter is True
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Cannot enumerate the root directory</exception>
        public void Run()
        {
            var allTracks = _dbManager.FromDirectory(Root)
                .OrderBy(t => t.Uri)
                .Select(t => t.Uri)
                .ToList();
            _allTrackPaths.AddRange(allTracks);

            while (true)
            {
                if (IsCompleted || !_canContinue())
                {
                    Console.WriteLine("Scanning completed");
                    ScanFinished?.Invoke(this, EventArgs.Empty);
                    return;
                }

                if (_allDirectories == null)
                {
                    FindAllDirectories();
                }

                if (_allDirectoriesEnumerator == null)
                    throw new InvalidOperationException("Cannot enumerate the root directory");

                if (_lastScannedDirectory == null)
                {
                    ScanDirectory(Root);
                }
                else
                {
                    if (_allDirectoriesEnumerator.MoveNext())
                    {
                        var directoryToScan = _allDirectoriesEnumerator.Current as string;
                        Debug.Assert(directoryToScan != null);
                        ScanDirectory(directoryToScan);
                    }
                    else
                    {
                        // one more thing - go through all tracks in the database and see that they still exist
                        _allTrackPaths.Filter(path => !File.Exists(path))
                            .Iter(path =>
                            {
                                _dbManager.Forget(path);
                                Debug.WriteLine($"Forgot {path}");
                            });
                        _allPlaylists.Where(f => f.Uri.StartsWith(Root) && !File.Exists(f.Uri))
                            .Iter(playlist =>
                            {
                                _dbManager.Forget(playlist);
                                Debug.WriteLine($"Forgot {playlist.Uri}");
                            });
                        IsCompleted = true;
                    }
                }
            }
        }

        /// <summary>
        /// Scans for new files in known root directories
        /// </summary>
        /// <param name="dir"></param>
        private void ScanDirectory(string dir)
        {
            if (!new DirectoryInfo(dir).Attributes.HasFlag(FileAttributes.Hidden))
            {
                IEnumerable<string>? filePaths = null;
                try
                {
                    // take all files we find in the subdirectory
                    filePaths = Directory.EnumerateFiles(dir, "*.*", SearchOption.TopDirectoryOnly);
                       // .Where(f => _supportedExtensions.IndexOf(f.Split('.').Last()) > -1);
                }
                catch (UnauthorizedAccessException uaEx)
                {
                    Debug.WriteLine(uaEx.Message);
                }
                catch (PathTooLongException pathEx)
                {
                    Debug.WriteLine(pathEx.Message);
                }

                if (filePaths != null)
                {
                    foreach (var path in filePaths.TakeWhile(_ => !_stopFlag.IsCancellationRequested))
                    {
                        if (_supportedExtensions.IndexOf(path.Split('.').Last()) > -1)
                        {
                            RescanFileIfNeeded(path);
                        }
                        else if (_playlistExtensions.IndexOf(path.Split('.').Last()) > -1)
                        {
                            RescanPlaylistIfNeeded(path);
                        }
                    }
                }
            }
            _lastScannedDirectory = dir;
            Debug.Assert(_allDirectories != null, nameof(_allDirectories) + " != null");
            var listWeHaveSoFar = _allDirectories.ToList();
            ScanProgress?.Invoke(this, new DirectoryScannedEventArgs(_lastScannedDirectory, 
                ((double)listWeHaveSoFar.IndexOf(_lastScannedDirectory) + 1) / listWeHaveSoFar.Count));
        }

        private void RescanFileIfNeeded(string filePath)
        {
            var cachedTrack = _dbManager.ByFilename(filePath)
                .IfNone(() => 
                    new TrackStub(filePath, 
                        new MetadataSource(
                            new LocalFileAbstraction(filePath))));
            if (_forceRescan)
            {
                UpdateMetadataInDb(cachedTrack, DateTime.Now);
                return;
            }
            var lastScannedOn = cachedTrack is IPersistedTrack persisted
                ? persisted.LastScannedOn
                : DateTime.MinValue;
            var lastWritten = File.GetLastWriteTime(filePath);
            if (lastScannedOn == DateTime.MinValue || lastScannedOn < lastWritten)
            {
                UpdateMetadataInDb(cachedTrack, lastWritten);
            }
        }

        private void RescanPlaylistIfNeeded(string filePath)
        {
            var dbPlaylist = _dbManager.GetOrCreate(filePath);
            var lastUpdated = new DateTime(dbPlaylist.LastScannedOn);
            var lastWritten = File.GetLastWriteTime(filePath);
            if (lastUpdated >= lastWritten && !_forceRescan) return;
            // read the playlist
            try
            {
                Debug.WriteLine($"Populating {filePath}");
                var tracks = _playlistFromFiles.Playlist(filePath);
                _dbManager.PopulatePlaylist(dbPlaylist, tracks);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        private void UpdateMetadataInDb(ITrack trackInDb, DateTime lastWritten)
        {
            Debug.WriteLine($"Updating meta of {trackInDb.Uri}");
            PersistedSong toPersist;
            if (trackInDb is TrackStub song)
            {
                toPersist = new PersistedSong(song);
            }
            else
            {
                try
                {
                    // Here we should re-read the metadata, and keep the waveform if we have it,
                    var trackOnDisk = new TrackStub(trackInDb.Uri,
                        new MetadataSource(new LocalFileAbstraction(trackInDb.Uri)));
                    var merger = new MergesMetadataFromTracks(trackInDb, trackOnDisk);
                    var mergedTrack = (ITrack) new TrackStub(trackInDb.Uri,
                        new CachedMetadataSource(merger.MergedMeta(), trackOnDisk.Duration),
                        merger.LastKnownWaveform(), trackInDb.StartTime, trackInDb.EndTime,
                        trackInDb.ConfirmedReadable);
                    var id = trackInDb is IPersistedTrack persisted ? persisted.Id : 0;
                    toPersist = new PersistedSong(mergedTrack, id);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Trouble with {trackInDb.Uri}: {e.Message}");
                    return;
                }
            }
            toPersist.LastScannedOn = lastWritten;
            _dbManager.Save(toPersist);
            ScannedTrack?.Invoke(this, new ScannedTrackEventArgs(trackInDb));
        }

        private void FindAllDirectories()
        {
            try
            {
                _allDirectories = Directory.GetDirectories(Root, "*", SearchOption.AllDirectories);
                _allDirectoriesEnumerator = _allDirectories.GetEnumerator();
            }
            catch (UnauthorizedAccessException uaEx)
            {
                Debug.WriteLine(uaEx.Message);
            }
            catch (PathTooLongException pathEx)
            {
                Debug.WriteLine(pathEx.Message);
            }

        }

        public void Dispose()
        {
            IsCompleted = true;
        }
    }

}
