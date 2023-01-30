using GooglePlayGames;

public static class GooglePlay
{
    public static void SafeIncrementEvent(string eventId, uint value)
    {
        try
        {
            PlayGamesPlatform.Instance.Events.IncrementEvent(eventId, value);
        }
        catch { }
    }
}
