using System;
using System.IO;
using MusicLibrary.Metadata;

namespace MusicLibrary
{
    public interface ITrack : IMetadataSource
    {
        /// <summary>
        /// The path to the track on the filesystem
        /// </summary>
        string Uri { get; }

        /// <summary>
        /// Total duration as TimeSpan
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// Signals that the duration has changed, e.g. because silence was trimmed
        /// </summary>
        event EventHandler<EventArgs> DurationChanged;

        /// <summary>
        /// Making this read-write save us a method that could've been called "StartAt"... ?
        /// </summary>
        TimeSpan StartTime { get; }

        /// <summary>
        /// Sets the StartTime to desired value
        /// </summary>
        /// <param name="newStart"></param>
        void TrimStart(TimeSpan newStart);

        /// <summary>
        /// Making this read-write saves us a method that could've been called "EndAt" ... ?
        /// </summary>
        TimeSpan EndTime { get; }
        void TrimEnd(TimeSpan newEnd);

        /// <summary>
        /// Trim leading and trailing silence
        /// </summary>
        /// <param name="zeroVolume">The threshold (in dB) below which silence is assumed</param>
        void TrimSilence(float zeroVolume);

        /// <summary>
        /// Metadata / Description
        /// </summary>
        IMetadata Meta { get; }

        /// <summary>
        /// Calculated waveform data
        /// </summary>
        byte[] WaveformData { get; }

        /// <summary>
        /// Provides an observable that produces waveform data
        /// </summary>
        /// <returns></returns>
        IObservable<float> WaveformStream();

        /// <summary>
        /// Signals that the waveform data was updated
        /// </summary>
        event EventHandler<EventArgs> WaveformUpdated;

        /// <summary>
        /// Get the physical representation in bytes
        /// </summary>
        /// <returns></returns>
        Stream Notes();

        /// <summary>
        /// Whether or not this track was fully read and deemed readable end-to-end
        /// </summary>
        bool ConfirmedReadable { get; }

        /// <summary>
        /// Reads through itself end-to-end. When successful, ConfirmedReadable is set to True
        /// </summary>
        void CheckIfReadable();

        /// <summary>
        /// Signals that a read operation has failed.
        /// This can happen during the TrimSilence() operation
        /// </summary>
        event EventHandler<EventArgs>? ReadFailed;
    }
}