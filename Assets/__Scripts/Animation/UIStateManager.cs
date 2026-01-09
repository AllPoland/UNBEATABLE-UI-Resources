using System;

namespace UBUI.Animation
{
    public static class UIStateManager
    {
        private static UIState currentState;

        public static event Action<UIState, UIState> OnStateChanged;


        public static void SetState(UIState newState)
        {
            OnStateChanged?.Invoke(currentState, newState);
            currentState = newState;
        }


        public static void SetState(UIState oldState, UIState newState)
        {
            OnStateChanged?.Invoke(oldState, newState);
            currentState = newState;
        }
    }


    public enum UIState
    {
        None,
        TitleScreen,
        MainMenu,
        SongSelect,
        DifficultySelect,
        FilterSelect,
        Leaderboard,
        Modifiers,
        FolioView,
        ChallengeView,
        CharacterSelect,
        PlayerLeaderboard,
        PlayerStats,
        Any = int.MaxValue
    }
}