using System;
using System.Collections.Generic;

namespace UBUI.Animation
{
    public static class UIStateManager
    {
        private static UIState currentState;

        public static event Action<UIState, UIState, Dictionary<string, float>> OnTransitionStart;


        public static void SetState(UIState newState, Dictionary<string, float> delays)
        {
            OnTransitionStart?.Invoke(currentState, newState, delays);
            currentState = newState;
        }


        public static void SetState(UIState oldState, UIState newState, Dictionary<string, float> delays)
        {
            OnTransitionStart?.Invoke(oldState, newState, delays);
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