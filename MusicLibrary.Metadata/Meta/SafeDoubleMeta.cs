namespace MusicLibrary.Metadata.Meta
{
    /// <summary>
    /// Boxes a double that will default to zero
    /// if the source value was NaN or Infinity.
    /// </summary>
    internal readonly struct SafeDoubleMeta
    {
        public SafeDoubleMeta(double risky)
        {
            Value = double.IsNaN(risky) || double.IsInfinity(risky)
                ? 0.0d
                : risky;
        }

        public double Value { get; }
    }
}
