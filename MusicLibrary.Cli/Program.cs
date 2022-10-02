using CommandLine;
using MusicLibrary.Cli.Playlists;
using MusicLibrary.Persistence.LiteDb;
using MusicLibrary.Persistence.SqliteNet;
using System.Diagnostics;

namespace MusicLibrary.Cli
{
    internal class Program
    {
        static Stopwatch? watch;
        static IDBManager? library;
        static IDbWriter? writer;

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<ScanOptions, TestOptions>(args)
                .WithParsed<ScanOptions>(Scan)
                .WithParsed<TestOptions>(Test);
        }

        private static void Scan(ScanOptions options)
        {
            var api = options.Database;
            if (api != "sqlite" && api != "litedb")
            {
                Console.WriteLine(@"Please use sqlite or litedb as the database argument");
                return;
            }

            var dbPath = Directory.Exists(options.DatabasePath)
                ? Path.Combine(options.DatabasePath, $"musiclibrary.{api}")
                : options.DatabasePath;

            library = DatabaseFromApiAndPath(api, dbPath);

            var stopFlag = new CancellationTokenSource();
            var scanner = new Scanner(options.LibraryFolder, library, new PlaylistFromFiles(library), stopFlag);

            Console.WriteLine($"Starting scanning of {options.LibraryFolder}");
            scanner.ScanProgress += Scanner_ScanProgress;
            scanner.ScanFinished += Scanner_ScanFinished;

            watch = new Stopwatch();
            watch.Start();
            scanner.Run();
        }

        private static void Scanner_ScanFinished(object? sender, EventArgs e)
        {
            if (sender is Scanner scanner)
            {
                scanner.ScanProgress -= Scanner_ScanProgress;
                scanner.ScanFinished -= Scanner_ScanFinished;
            }

            //writer?.Dispose();
            library?.Dispose();
            watch?.Stop();
            var ts = TimeSpan.FromMilliseconds(watch?.ElapsedMilliseconds ?? 0);
            Console.WriteLine($"Done in {ts}");
        }

        private static IDBManager DatabaseFromApiAndPath(string api, string dbPath)
        {
            var composer = new TrackStubComposer();
            writer = new DirectDbWriter();
            return library = api == "sqlite"
                ? new SqliteNetDbManager(dbPath, composer, writer)
                : new LiteDbMusicLibrary(dbPath, composer, writer);
        }

        private static void Scanner_ScanProgress(object? sender, DirectoryScannedEventArgs e)
        {
            Console.WriteLine($"Scanned {e.Directory}, progress: {Math.Round(e.ProgressPercent * 100, 2)}%");
        }

        private static void Test(TestOptions options)
        {
            var api = options.Database;
            if (api != "sqlite" && api != "litedb")
            {
                Console.WriteLine(@"Please use sqlite or litedb as the database argument");
                return;
            }

            var dbPath = options.DatabasePath;
            if (!File.Exists(dbPath))
            {
                Console.WriteLine($"Path {options.DatabasePath} is not accessible");
                return;
            }

            library = DatabaseFromApiAndPath(api, dbPath);
            Stopwatch testWatch;// = Stopwatch.StartNew();
            TimeSpan testExecutionTime = TimeSpan.Zero;
            List<ITrack> tracks;
            switch (options.Command)
            {
                case @"genres":
                    {
                        testWatch = Stopwatch.StartNew();
                        var genres = library.Genres();
                        testWatch.Stop();
                        testExecutionTime = testWatch.Elapsed;
                        Console.WriteLine($"Read {genres.Count()} genres");
                    }
                    break;
                case @"bygenre":
                    testWatch = Stopwatch.StartNew();
                    tracks = library.ByGenre(options.Arguments).ToList();
                    Console.WriteLine($"Retrieved {tracks.Count} tracks");
                    testWatch.Stop();
                    testExecutionTime = testWatch.Elapsed;
                    break;
                case @"batchinsert":
                    {
                        var numTracks = int.Parse(options.Arguments);
                        tracks = library.Search(@"").Take(numTracks).ToList();
                        var step1Watch = Stopwatch.StartNew();
                        tracks.ForEach(t => library.Forget(t));
                        step1Watch.Stop();
                        Console.WriteLine($"Deleted in {step1Watch.Elapsed}");

                        testWatch = Stopwatch.StartNew();
                        library.Save(tracks);
                        testWatch.Stop();
                        testExecutionTime = testWatch.Elapsed;
                        Console.WriteLine($"Inserted {numTracks} tracks");
                    }
                    break;
                case @"singledelete":
                    {
                        var numTracks = int.Parse(options.Arguments);
                        tracks = library.Search(@"").Take(numTracks).ToList();
                        testWatch = Stopwatch.StartNew();
                        tracks.ForEach(track => library.Forget(track));
                        testWatch.Stop();
                        testExecutionTime = testWatch.Elapsed;
                        tracks.ForEach(track =>
                        {
                            if (library.ByFilename(track.Uri).IsSome)
                            {
                                throw new Exception($"Oops, track {track.Uri} was not actually deleted");
                            }
                        });
                        Console.WriteLine($"Deleted {numTracks} tracks");
                    }
                    break;
                case @"batchdelete":
                    {
                        var numTracks = int.Parse(options.Arguments);
                        tracks = library.Search(@"").Take(numTracks).ToList();
                        testWatch = Stopwatch.StartNew();
                        library.Forget(tracks);
                        testWatch.Stop();
                        testExecutionTime = testWatch.Elapsed;
                        tracks.ForEach(track =>
                        {
                            if (library.ByFilename(track.Uri).IsSome)
                            {
                                throw new Exception($"Oops, track {track.Uri} was not actually deleted");
                            }
                        });
                        Console.WriteLine($"Deleted {numTracks} tracks");
                    }
                    break;
                case @"readplaylist":
                    testWatch = Stopwatch.StartNew();
                    var playlist = library.GetOrCreate(options.Arguments);
                    tracks = library.Contents(playlist).ToList();
                    testWatch.Stop();
                    testExecutionTime = testWatch.Elapsed;
                    Console.WriteLine($"Fetched {tracks.Count} tracks");
                    break;
                case @"readtrack":
                    {
                        var track = library.ByFilename(options.Arguments);
                        track.IfSome(t => Console.WriteLine($"Waveform length: {t.WaveformData.Length}"));
                    }
                    break;
                default:
                    Console.WriteLine($"Command {options.Command} is not implemented");
                    break;
            }

            Console.WriteLine($"Test executed in {testExecutionTime}");
            //writer?.Dispose();
            library?.Dispose();
        }

        [Verb("scan", HelpText = @"Add music to your library")]
        public class ScanOptions
        {
            [Option('d', "database", Required = true, HelpText = "Database to use (sqlite or litedb)")]
            public string Database { get; set; } = @"";

            [Option('i', "input", Required = true, HelpText = "Path to a folder containing your music")]
            public string LibraryFolder { get; set; } = @"";

            [Option('o', "output", Required = true, HelpText = "Path to where the database should be stored")]
            public string DatabasePath { get; set; } = @"";
        }

        [Verb("test", HelpText = "Test performance of various operations")]
        public class TestOptions
        {
            [Option('d', "database", Required = true, HelpText = "Database to use (sqlite or litedb)")]
            public string Database { get; set; } = @"";

            [Option('i', "input", Required = true, HelpText = "Path to your database")]
            public string DatabasePath { get; set; } = @"";

            [Option('c', "command", Required = true, HelpText = "Command to run (bygenre)")]
            public string Command { get; set; } = @"";

            [Option('a', "args", Required = true, HelpText = "Command argument(s)")]
            public string Arguments { get; set; } = @"";

        }
    }
}