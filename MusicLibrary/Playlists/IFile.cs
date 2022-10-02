using LanguageExt;

namespace MusicLibrary.Playlists
{
    public interface IFile
    {
        Option<ITrack> Load(string path);
    }
}