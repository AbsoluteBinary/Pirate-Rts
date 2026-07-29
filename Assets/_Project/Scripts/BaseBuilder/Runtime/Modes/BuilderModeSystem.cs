using _Project.Scripts.BaseBuilder.Runtime.Data;
using UnityEngine;

namespace _Project.Scripts.BaseBuilder.Runtime.Modes
{
    public class BuilderModeSystem
    {
        public BuilderMode CurrentMode { get; private set; } = BuilderMode.Select;
        public SelectFilter CurrentSelectFilter { get; private set; } = SelectFilter.All;

        public event System.Action<BuilderMode> OnModeChanged;
        public event System.Action<SelectFilter> OnSelectFilterChanged;

        public void SetMode(BuilderMode newMode)
        {
            if (CurrentMode == newMode) return;

            CurrentMode = newMode;
            OnModeChanged?.Invoke(CurrentMode);
        }

        public void SetSelectFilter(SelectFilter newFilter)
        {
            if (CurrentSelectFilter == newFilter) return;

            CurrentSelectFilter = newFilter;
            OnSelectFilterChanged?.Invoke(CurrentSelectFilter);
        }

        public bool IsSelectMode => CurrentMode == BuilderMode.Select;
        public bool IsBuildOnWater => CurrentMode == BuilderMode.BuildOnWater;
        public bool IsBuildOnLand => CurrentMode == BuilderMode.BuildOnLand;
    }
}