#nullable enable

using LiteDB;
using System;

namespace MusicLibrary.Persistence.LiteDb.Model
{
    internal class Track
    {
        public Track() { }

        public Track(BsonDocument doc)
        {
            if (doc == null)
            {
                throw new ArgumentNullException(doc);
            }
            Id = doc["_id"].AsInt32;
            Uri = doc["Uri"].AsString;
            Title = doc["Title"].AsString;
            Artist = doc["Artist"].AsString;
            AlbumArtist = doc["AlbumArtist"].AsString;
            Conductor = doc["Conductor"].AsString;
            Album = doc["Album"].AsString;
            Genre = doc["Genre"].AsString;
            Year = doc["Artist"].AsString;
            Duration = doc["Duration"].AsInt64;
            Comment = doc["Comment"].AsString;
            BPM = doc["BPM"].AsDouble;
            ReplayGain = doc["ReplayGain"].AsDouble;
            Rating = doc["Rating"].AsInt32;
            StartTime = doc["StartTime"].AsInt64;
            EndTime = doc["EndTime"].AsInt64;
            WaveformData = doc["WaveformData"].AsBinary;
            LastScannedOn = doc["LastScannedOn"].AsInt64;
            ConfirmedReadable = doc["ConfirmedReadable"].AsBoolean;
            SearchIndex = doc["SearchIndex"].AsString;
            Grouping = doc["Grouping"].AsString;
        }

        public BsonValue AsDocument()
        {
            var doc = new BsonDocument();
            doc["_id"] = Id;
            doc["Uri"] = Uri;
            doc["Title"] = Title;
            doc["Artist"] = Artist;
            doc["AlbumArtist"] = AlbumArtist;
            doc["Conductor"] = Conductor;
            doc["Album"] = Album;
            doc["Genre"] = Genre;
            doc["Artist"] = Year;
            doc["Duration"] = Duration;
            doc["Comment"] = Comment;
            doc["BPM"] = BPM;
            doc["ReplayGain"] = ReplayGain;
            doc["Rating"] = Rating;
            doc["StartTime"] = StartTime;
            doc["EndTime"] = EndTime;
            doc["WaveformData"] = WaveformData;
            doc["LastScannedOn"] = LastScannedOn;
            doc["ConfirmedReadable"] = ConfirmedReadable;
            doc["SearchIndex"] = SearchIndex;
            doc["Grouping"] = Grouping;

            return doc;
        }

        public int Id { get; set; }

        public string? Uri { get; set; }

        public string? Title { get; set; }

        public string? Artist { get; set; }

        public string? AlbumArtist { get; set; }

        public string? Conductor { get; set; }

        public string? Album { get; set; }

        public string? Genre { get; set; }

        public string? Grouping { get; set; }

        public string? Year { get; set; }

        public long Duration { get; set; }

        public string? Comment { get; set; }

        public double BPM { get; set; }

        public double ReplayGain { get; set; }

        public int Rating { get; set; }

        public long StartTime { get; set; }

        public long EndTime { get; set; }

        public byte[]? WaveformData { get; set; }

        public long LastScannedOn { get; set; }

        public bool ConfirmedReadable { get; set; }

        public string? SearchIndex { get; set; }


    }
}
