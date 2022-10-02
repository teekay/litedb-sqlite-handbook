using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using IdSharp.Tagging.ID3v2;
using TagLib;
using TagLib.Ogg;

namespace MusicLibrary.Metadata.Meta
{
    /// <summary>
    /// Parses, cleans up, and prints tag metadata as primitive values.
    /// </summary>
    public class SongMetadata : IMetadata
    {
        public SongMetadata(ATL.Track tag)
        {
            Year = new TrimmedStringMeta(tag.Year.ToString()).ToString();
            Title = new TrimmedStringMeta(tag.Title).ToString();
            Conductor = new TrimmedStringMeta(tag.Conductor).ToString();
            AlbumArtist = new TrimmedStringMeta(tag.AlbumArtist).ToString();
            Artist = new TrimmedStringMeta(tag.Artist).ToString();
            Album = new TrimmedStringMeta(tag.Album).ToString();
            Comment = new TrimmedStringMeta(tag.Comment).ToString();
            Genre = new TrimmedStringMeta(tag.Genre).ToString();
            tag.AdditionalFields.TryGetValue("BPM", out var bpmString);
            double.TryParse(bpmString ?? "", out var maybeBpm);
            BPM = new SafeDoubleMeta(maybeBpm).Value;
            Rating = new RatingMeta(tag.Popularity).Rating;
            Grouping = string.Empty; //new TrimmedStringMeta(tag.Grouping).ToString();
            var grpTag = tag.AdditionalFields.Keys.FirstOrDefault(k =>
                k.Length == 4 && k.Substring(1).ToUpperInvariant().Equals("GRP"));
            if (grpTag != null && tag.AdditionalFields.TryGetValue(grpTag, out var grpValue))
            {
                Grouping = new TrimmedStringMeta(grpValue).ToString();
            }

            var rgTag = tag.AdditionalFields.Keys.FirstOrDefault(k =>
                k.ToUpperInvariant().Contains("REPLAYGAIN_TRACK_GAIN"));
            tag.AdditionalFields.TryGetValue(rgTag ?? "REPLAYGAIN_TRACK_GAIN", out var rgString);
            var rgNoDecibel = (rgString ?? "").Split(' ').First();
            var parsedRg = double.TryParse(rgNoDecibel, NumberStyles.Number, CultureInfo.InvariantCulture, out var maybeRg);

            if (parsedRg)
            {
                var maybeRgMaybeBs = new SafeDoubleMeta(maybeRg).Value;
                if ((!(maybeRgMaybeBs < -20d) && !(maybeRgMaybeBs > 20d)) || !rgString!.Contains(","))
                {
                    ReplayGain = maybeRgMaybeBs;
                    return;
                }
                parsedRg = double.TryParse(rgNoDecibel, NumberStyles.Number,
                    CultureInfo.CreateSpecificCulture("de"), out var maybeRg2);
                if (parsedRg && maybeRg2 >= -20.0d && maybeRg2 <= 20.0d)
                {
                    ReplayGain = maybeRg2;
                }
                return;
            }

            try
            {
                // try to get the ITunes soundcheck value and convert it to replaygain
                var soundCheckTag = tag.AdditionalFields.Keys.FirstOrDefault(k =>
                    k.ToUpperInvariant().Contains("ITUNNORM"));
                tag.AdditionalFields.TryGetValue(soundCheckTag ?? string.Empty, out var hexNums);
                var soundchecks = hexNums?.Trim()
                    .Split(' ').Take(2)
                    .Select(hex => Convert.ToInt32(hex, 16))
                    .ToList();
                if (soundchecks?.Count > 0)
                {
                    // for algo see here: https://gist.github.com/daveisadork/4717535
                    // and here: https://github.com/stu247/rgToSc/blob/master/rgToSc.py
                    // and here: https://id3.org/iTunes%20Normalization%20settings
                    var maybeRgMaybeBs = new SafeDoubleMeta(Math.Log10(soundchecks.Max() / 1000d) * -10.0).Value;
                    if (maybeRgMaybeBs >= -20.0d && maybeRgMaybeBs <= 20.0d)
                    {
                        ReplayGain = maybeRgMaybeBs;
                    }
                }
            }
            catch
            {
                // ignored
            }
        }

        public SongMetadata(ID3v2Tag tag)
        {
            Year = new TrimmedStringMeta(tag.Year).ToString();
            Title = new TrimmedStringMeta(tag.Title).ToString();
            Conductor = new TrimmedStringMeta(tag.Conductor).ToString();
            AlbumArtist = new TrimmedStringMeta(tag.AlbumArtist).ToString();
            Artist = new AlternatingMeta(
                new TrimmedStringMeta(
                    tag.UserDefinedText?
                        .FirstOrDefault(frame => frame.Description?.ToUpperInvariant() == "DISPLAY ARTIST")?.Value
                    ?? string.Empty).ToString(),
                new JoinedStringMeta(tag.Artist?.Split('\0') ?? new string[0], CommaSeparator).ToString(),
                AlbumArtist
            ).ToString();
            Album = new TrimmedStringMeta(tag.Album).ToString();
            Comment = new JoinedStringMeta(tag.CommentsList?.Select(c => c.Value).ToArray() ?? new string[0], CommaSeparator).ToString();
            Genre = new TrimmedStringMeta(tag.Genre).ToString();
            double.TryParse(tag.BPM, out var maybeBpm);
            BPM = new SafeDoubleMeta(maybeBpm).Value;
            var rgString = (tag.UserDefinedText?
                .FirstOrDefault(frame => frame.Description.ToUpperInvariant() == "REPLAYGAIN_TRACK_GAIN")?.Value ?? "0")
                .Split(' ')
                .First();
            double.TryParse(rgString, NumberStyles.Number, CultureInfo.InvariantCulture, out var maybeReplayGain);
            var maybeRgMaybeBs = new SafeDoubleMeta(maybeReplayGain).Value;
            if (maybeRgMaybeBs >= -20.0d && maybeRgMaybeBs <= 20.0d)
            {
                ReplayGain = maybeRgMaybeBs;
            }
            var manyRatings = tag.PopularimeterList?.Select(p => p.Rating).Select(r => (double) r / 255 * 100)
                .ToList() ?? new List<double>();
            var rating = manyRatings.Count > 0
                ? manyRatings.Average()
                : 0.0d;
            Rating = new RatingMeta((int)rating).Rating;
            Grouping = new TrimmedStringMeta(tag.ContentGroup).ToString();
        }

        public SongMetadata(Tag tag)
        {
            Year = new TrimmedStringMeta(tag.Year.ToString()).ToString();
            Title = new TrimmedStringMeta(tag.Title).ToString();
            Conductor = new TrimmedStringMeta(tag.Conductor).ToString();
            AlbumArtist = new JoinedStringMeta(tag.AlbumArtists, CommaSeparator).ToString();
            Artist = new AlternatingMeta(
                new JoinedStringMeta(tag.Performers, CommaSeparator).ToString(), 
                AlbumArtist
            ).ToString();
            Album = new TrimmedStringMeta(tag.Album).ToString();
            Comment = new TrimmedStringMeta(tag.Comment).ToString();
            Genre =  new JoinedStringMeta(tag.Genres, SlashSeparator).ToString();
            BPM = new SafeDoubleMeta(tag.BeatsPerMinute).Value;
            ReplayGain = new SafeDoubleMeta(tag.ReplayGainTrackGain).Value;
            Rating = new RatingMeta(tag).Rating;
            Grouping = new TrimmedStringMeta(tag.Grouping).ToString();
            if (string.IsNullOrEmpty(Grouping))
            {
                string GroupingFromFlac(XiphComment xiphComment)
                {
                    var maybeGroups = xiphComment.GetField("CONTENTGROUP");
                    return maybeGroups != null && maybeGroups.Length > 0
                        ? maybeGroups[0]
                        : string.Empty;
                }
                if (tag is XiphComment xiph)
                {
                    Grouping = GroupingFromFlac(xiph);
                } else if (tag is TagLib.Flac.Metadata meta)
                {
                    Grouping = GroupingFromFlac(meta.GetComment(false, meta));
                }
            }
        }

        public SongMetadata(string year, string title, string artist, 
            string album, string comment, string genre, string albumArtist, string conductor,
            double bpm, double replayGain, uint rating, string grouping)
        {
            Year = new TrimmedStringMeta(year).ToString();
            Title = new TrimmedStringMeta(title).ToString();
            Artist = new TrimmedStringMeta(artist).ToString();
            AlbumArtist = new TrimmedStringMeta(albumArtist).ToString();
            Conductor = new TrimmedStringMeta(conductor).ToString();
            Album = new TrimmedStringMeta(album).ToString();
            Comment = new TrimmedStringMeta(comment).ToString();
            Genre = new TrimmedStringMeta(genre).ToString();
            BPM = new SafeDoubleMeta(bpm).Value;
            ReplayGain = new SafeDoubleMeta(replayGain).Value;
            Rating = new RatingMeta(rating).Rating;
            Grouping = grouping;
        }

        private const string CommaSeparator = ", ";
        private const string SlashSeparator = "/";

        // ALL of the properties below should be readonly ... except that we want the DJ to update them at will without ceremony... so?
        public string Title { get; private set; }
        public string Artist { get; private set; }
        public string Conductor { get; private set; }
        public string Genre { get; private set; }
        public string Year { get; private set; }
        public string Album { get; private set; }
        public string AlbumArtist { get; private set; }
        public string Comment { get; private set; }
        public int Rating { get; private set; }
        public string Grouping { get; private set; }
        public double BPM { get; private set; }
        public double ReplayGain { get; private set; }
        
        public void Update(IMetadata source)
        {
            Title = source.Title;
            Artist = source.Artist;
            Conductor = source.Conductor;
            Genre = source.Genre;
            Year = source.Year;
            Album = source.Album;
            AlbumArtist = source.AlbumArtist;
            Comment = source.Comment;
            Rating = source.Rating;
            BPM = source.BPM;
            ReplayGain = source.ReplayGain;
            Grouping = source.Grouping;
        }

        public override string ToString()
        {
            return $"{Artist};{AlbumArtist};{Title};{Genre};{Album};{Comment}";
        }
    }
}
