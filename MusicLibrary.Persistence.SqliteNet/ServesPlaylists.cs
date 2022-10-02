using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MusicLibrary.Persistence.SqliteNet.Model;
using MusicLibrary.Playlists;
using SQLite;

namespace MusicLibrary.Persistence.SqliteNet
{
    internal sealed class ServesPlaylists
    {
        public ServesPlaylists(SQLiteConnection connection, IDbWriter dbWriter)
        {
            _dbWriter = dbWriter;
            Connection = connection;
            Connection.Execute(
                "create table if not exists \"PlaylistTracks\" (\"Id\" integer primary key autoincrement not null, \"PlaylistId\" integer not null, \"TrackId\" integer not null, \"Position\" integer not null, \"CreatedAt\" integer not null)");
            Connection.Execute(
                "create index if not exists \"PlaylistTracks_PlaylistId\" on \"PlaylistTracks\"(\"PlaylistId\")");
            Connection.Execute(
                "create index if not exists \"PlaylistTracks_TrackId\" on \"PlaylistTracks\"(\"TrackId\")");
        }

        private readonly IDbWriter _dbWriter;
        private SQLiteConnection Connection { get; }

        internal Model.Playlist ExistingOrCreated(string path)
        {
            var maybe = Connection.FindWithQuery<Model.Playlist>("select * from Playlist where Filename = ?", path);
            return maybe ?? Created(path);
        }

        private Model.Playlist Created(string path)
        {
            var created = new Model.Playlist { Uri = path };
            Connection.Insert(created);
            return created;
        }

        internal static Model.Playlist PreSaved(IPlaylist plist)
        {
            if (string.IsNullOrEmpty(plist.Uri) || string.IsNullOrWhiteSpace(plist.Uri))
                throw new ArgumentException("Playlist filename cannot be null");

            var model = new Model.Playlist
            {
                Uri = plist.Uri,
                Comment = plist.Comment
            };
            if (plist is IPersistedPlaylist playlist)
            {
                model.Id = playlist.Id;
            }
            return model;
        }

        internal void Populate(IPersistedPlaylist playlist, IEnumerable<Track> tracks)
        {
            var now = DateTime.Now.Ticks;
            const string sql = @"INSERT INTO PlaylistTracks (PlaylistId, TrackId, Position, CreatedAt) VALUES(?, ?, ?, ?)";
            var toInsert = tracks.Select((t, i) => 
                (TrackId: t.Id, Position: i))
                .ToList();
            _dbWriter.Commit(() =>
            {
                Connection.BeginTransaction();
                try
                {
                    Connection.Execute("DELETE FROM PlaylistTracks WHERE PlaylistId=?", playlist.Id);
                    toInsert.ForEach(action: t =>
                    {
                        Connection.Execute(sql, playlist.Id, t.TrackId, t.Position, now);
                    });                    
                } catch (SQLiteException e)
                {
                    Debug.WriteLine(e.ToString());
                    Connection.Rollback();
                    return;
                }
                var updatedPlaylist = new Model.Playlist
                {
                    Id = playlist.Id,
                    Uri = playlist.Uri,
                    Comment = playlist.Comment,
                    LastScannedOn = now
                };
                Connection.Update(updatedPlaylist);
                Connection.Commit();
            });
        }

        internal void Forget(IPersistedPlaylist playlist)
        {
            _dbWriter.Commit(() =>
            {
                Connection.Execute(@"DELETE FROM PlaylistTracks WHERE PlaylistId=?", playlist.Id);
                Connection.Execute(@"DELETE FROM Playlist WHERE Id=?", playlist.Id);
            });
        }

        internal void RemoveFromPlaylists(Track track) => 
            _dbWriter.Commit(() => Connection.Execute(@"DELETE FROM PlaylistTracks WHERE TrackId=?", track.Id));

        public IEnumerable<IPersistedPlaylist> AllPlaylists() => Connection.Table<Model.Playlist>();
    }
}
