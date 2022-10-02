namespace MusicLibrary
{
    public interface IDeviceCache
    {
        string DeviceName(object id);
        void Cache(object id, string name);
    }
}
