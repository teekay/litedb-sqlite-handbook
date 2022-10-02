using LanguageExt;

namespace MusicLibrary.Cli
{
    public static class LanguageExtEx
    {
        public static Unit WhenNone<T>(this Option<T> source, Action callback)
        {
            if (source.IsNone) callback.Invoke();
            return Unit.Default;
        }

        public static async Task<Unit> WhenNoneAsync<T>(this Option<T> source, Func<Task> callback)
        {
            if (source.IsNone) await callback.Invoke();
            return Unit.Default;
        }
    }
}
