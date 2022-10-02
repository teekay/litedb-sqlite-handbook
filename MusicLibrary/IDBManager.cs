using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using MusicLibrary.Playlists;

namespace MusicLibrary
{
    public interface IDBManager : IDisposable
    {
        IEnumerable<string> Genres();

        /// <summary>
        /// Search the database for a track by its path
        /// </summary>
        /// <param name="filepath">Path to track including the filename</param>
        /// <returns>Track if found, otherwise null</returns>
        Option<ITrack> ByFilename(string filepath);

        /// <summary>
        /// Find tracks in a directory
        /// </summary>
        /// <param name="filepath"></param>
        /// <returns></returns>
        IEnumerable<ITrack> FromDirectory(string filepath);
        
        /// <summary>
        /// Search by the grouping metadata tag
        /// </summary>
        /// <param name="grouping"></param>
        /// <returns></returns>
        IEnumerable<ITrack> ByGrouping(string grouping);

        /// <summary>
        /// Search by the grouping metadata tag async
        /// </summary>
        /// <param name="grouping"></param>
        /// <returns></returns>
        Task<IEnumerable<ITrack>> ByGroupingAsync(string grouping);

        /// <summary>
        /// Find tracks in multiple paths
        /// </summary>
        /// <param name="filepaths"></param>
        /// <returns></returns>
        IEnumerable<ITrack> FromPaths(IEnumerable<string> filepaths);

        /// <summary>
        /// Search for tracks given a keyword
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        IEnumerable<ITrack> Search(string keywords);

        /// <summary>
        /// Async implementation of Search
        /// </summary>
        /// <param name="keywords"></param>
        /// <returns></returns>
        Task<IEnumerable<ITrack>> SearchAsync(string keywords);

        /// <summary>
        /// Find tracks for Genre
        /// </summary>
        /// <param name="genreName"></param>
        /// <returns></returns>
        IEnumerable<ITrack> ByGenre(string genreName);

        /// <summary>
        /// Async implementation of ByGenre
        /// </summary>
        /// <param name="genreName"></param>
        /// <returns></returns>
        Task<IEnumerable<ITrack>> ByGenreAsync(string genreName);

        /// <summary>
        /// Get a subset of tracks in Genre async
        /// </summary>
        /// <param name="genreName"></param>
        /// <param name="offset"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<IEnumerable<ITrack>> ByGenreAsync(string genreName, int offset, int limit);

        /// <summary>
        /// Save Track info to database. Inserts or updates as necessary.
        /// </summary>
        /// <param name="track">Track to save</param>
        void Save(ITrack track);

        /// <summary>
        /// Save tracks in bulk
        /// </summary>
        /// <param name="tracks"></param>
        void Save(IEnumerable<ITrack> tracks);

        /// <summary>
        /// Remove track from the database
        /// </summary>
        /// <param name="path"></param>
        void Forget(string path);

        /// <summary>
        /// Remove track from the database
        /// </summary>
        /// <param name="source"></param>
        void Forget(ITrack source);

        /// <summary>
        /// Delete a batch of tracks
        /// </summary>
        /// <param name="tracks"></param>
        void Forget(IEnumerable<ITrack> tracks);

        /// <summary>
        /// Remove a playlist from the database
        /// </summary>
        /// <param name="playlist"></param>
        void Forget(IPersistedPlaylist playlist);

        /// <summary>
        /// Return all playlists
        /// </summary>
        /// <returns></returns>
        IEnumerable<IPersistedPlaylist> AllPlaylists();

        /// <summary>
        /// Gets a playlist by its path,
        /// or creates a new one if not found
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        IPersistedPlaylist GetOrCreate(string path);
        
        /// <summary>
        /// Return a playlist by its ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        IPersistedPlaylist? ByPlaylistId(int id);

        /// <summary>
        /// Playlists or search terms persisted from the last session
        /// </summary>
        /// <returns></returns>
        IEnumerable<(int PlaylistId, string? SearchTerm)> PlaylistSession();

        /// <summary>
        /// Persist playlists or search terms
        /// </summary>
        /// <param name="openPlaylists"></param>
        void PlaylistSession(IEnumerable<(int PlaylistId, string SearchTerm)> openPlaylists);

        /// <summary>
        /// Save a playlist to the database
        /// </summary>
        void Save(IPlaylist plist);

        /// <summary>
        /// Persist playlist contents in the database
        /// </summary>
        /// <param name="playlist"></param>
        /// <param name="tracks"></param>
        void PopulatePlaylist(IPersistedPlaylist playlist, IEnumerable<ITrack> tracks);

        /// <summary>
        /// Return the contents of a playlist
        /// </summary>
        /// <param name="playlist"></param>
        /// <returns></returns>
        IEnumerable<ITrack> Contents(IPersistedPlaylist playlist);
    }
}