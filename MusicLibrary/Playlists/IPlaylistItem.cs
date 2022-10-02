using System;

namespace MusicLibrary.Playlists
{
    /// <summary>
    /// This wraps a track and adds runtime controls / properties that 
    /// extend a) what is in the metadata.
    /// Perhaps THIS is the only object that the UI layer should care about!
    /// </summary>
    public interface IPlaylistItem : ITrack
    {
        /// <summary>
        /// A hack that gives us reliable (if we can count) positioning of 
        /// this within a playlist. Not that this object knows about playlists!
        /// </summary>
        int Index { get; set; }

        /// <summary>
        /// Not easily generalizable - when True the playback system
        /// might prematurely end playback of this and move on to the next thing.
        /// </summary>
        bool IsCortina { get; }

        /// <summary>
        /// A signal that this is contained in a playlist that is
        /// considered "main" - obviously this object knows nothing about any playlists!
        /// </summary>
        bool IsEnqueued { get; set; }

        /// <summary>
        /// A flag, used in the UI layer, used to make selections
        /// of multiple objects of this class, which can be fed to
        /// various Move / Delete methods of the playlist, of which
        /// this class knows nothing.
        /// </summary>
        bool IsSelected { get; set; }

        /// <summary>
        /// Indicate whether the track is deemed fit to be played
        /// This should only be set to False for tracks that were analyzed
        /// and found to be corrupt.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Runtime property that controls playback speed, eventually,
        /// used as a pre-set so that when the playback control reads this,
        /// it can adjust Player's Effects accordingly.
        /// </summary>
        int TempoAdjustment { get; set; }

        /// <summary>
        /// Ditto but about pitch adjustment.
        /// </summary>
        float PitchAdjustment { get; set; }

        /// <summary>
        /// Calculated property that provides the original BPM * TempoAdjustment / 100
        /// </summary>
        int AdjustedBPM { get; }

        /// <summary>
        /// Calculated property that provides Duration * TempoAdjustment
        /// </summary>
        TimeSpan AdjustedDuration { get; }

        IPlaylistItem Clone();
    }
}