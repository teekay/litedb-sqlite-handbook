using System.Collections.Generic;
using System.Linq;
using MusicLibrary.Persistence.SqliteNet.Model;
using SQLite;

namespace MusicLibrary.Persistence.SqliteNet
{
    internal sealed class DeviceCache: IDeviceCache
    {
        public DeviceCache(SQLiteConnection connection, IDbWriter dbWriter)
        {
            _dbWriter = dbWriter;
            Connection = connection;
        }

        private readonly IDbWriter _dbWriter;
        private bool _isInitialized;
        private SQLiteConnection Connection { get; }
        private List<CachedDeviceInfo>? _knownDevices;
        private List<CachedDeviceInfo> KnownDevices => _knownDevices ??= Connection.Query<CachedDeviceInfo>(
            @"SELECT Identifier, Name FROM CachedDeviceInfo");

        public string DeviceName(object id)
        {
            InitOnce();
            var maybe = KnownDevices.FirstOrDefault(device => device.Identifier?.Equals(id.ToString()) ?? false);
            return maybe?.Name ?? string.Empty;
        }

        public void Cache(object id, string name)
        {
            InitOnce();
            _dbWriter.Commit(() => 
                Connection.Execute("INSERT INTO CachedDeviceInfo(Identifier, Name) VALUES(?, ?)", id.ToString(), name));
        }

        private void InitOnce()
        {
            if (_isInitialized) return;
            Connection.CreateTable<Model.CachedDeviceInfo>();
            _isInitialized = true;
        }
    }
}