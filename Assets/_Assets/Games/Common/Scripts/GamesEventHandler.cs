using System;

namespace _Assets.Games
{
    public static class GamesEventHandler
    {
        public static event Action GameExited;
        public static event Action<string> GameplayCompleted;


        public static void OnGameExited()
        {
            GameExited?.Invoke();
        }


        public static void OnGameplayCompleted(string timerText)
        {
            GameplayCompleted?.Invoke(timerText);
        }
    }
}