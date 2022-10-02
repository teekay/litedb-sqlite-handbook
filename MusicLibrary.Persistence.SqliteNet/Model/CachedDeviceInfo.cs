using SQLite;

namespace MusicLibrary.Persistence.SqliteNet.Model
{
    /// <summary>
    /// DTO for storing information about devices
    /// </summary>
    internal class CachedDeviceInfo
    {
        private string _identifier = string.Empty;
        private string _name = string.Empty;

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Column("Identifier")]
        [Indexed(Name = "IX_Identifier")]
        [Unique]
        public string Identifier
        {
            get => _identifier;
            set => _identifier = value ?? string.Empty;
        }

        [Indexed]
        public string Name
        {
            get => _name;
            set => _name = value ?? string.Empty;
        }
    }
}
