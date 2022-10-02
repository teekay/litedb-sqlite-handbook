using System;
using TagLib.Id3v2;
using TagLib.Ogg;
using File = TagLib.File;

namespace MusicLibrary.Metadata
{
    public sealed class SavedMetadata
    {
        public SavedMetadata(IFileAbstractionFactory readsFiles, ITrack track)
        {
            _readsFiles = readsFiles;
            _track = track;
        }

        private readonly IFileAbstractionFactory _readsFiles;
        private readonly ITrack _track;

        public void Save()
        {
            var fileReadWrite = _readsFiles.GetFileAbstraction(_track.Uri, true);
            var file = File.Create(fileReadWrite);
            if (file == null) return;
            var tags = file.AllTags();
            if (tags.Count < 1) return;
            var tag = tags[0];
            tag.Title = _track.Meta.Title ?? string.Empty;
            tag.Comment = _track.Meta.Comment ?? string.Empty;
            tag.BeatsPerMinute = (uint)Math.Max(0, _track.Meta.BPM);
            switch (tag)
            {
                case TagLib.Id3v2.Tag id3:
                {
                    var frame = PopularimeterFrame.Get(id3, @"Bewitched", true);
                    frame.Rating = (byte)((double) _track.Meta.Rating / 100 * 255);
                    break;
                }
                case XiphComment xiph:
                    SaveXiphRating(xiph);
                    break;
                case TagLib.Flac.Metadata flac:
                    SaveXiphRating(flac.GetComment(true, flac));
                    break;
            }

            file.Save();
            (fileReadWrite as IDisposable)?.Dispose();
        }

        private void SaveXiphRating(XiphComment xiph)
        {
            var ratings = xiph.GetField("RATING");
            if (ratings == null || ratings.Length == 0)
            {
                xiph.SetField("RATING", new[] {_track.Meta.Rating.ToString()});
            }
            else
            {
                ratings[0] = _track.Meta.Rating.ToString();
                xiph.SetField("RATING", ratings);
            }
        }
    }
}
