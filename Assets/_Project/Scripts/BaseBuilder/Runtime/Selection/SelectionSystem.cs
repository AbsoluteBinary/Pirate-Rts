using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.BaseBuilder.Runtime.Selection
{
    public class SelectionSystem
    {
        private readonly List<GameObject> selected = new List<GameObject>();
        private readonly HashSet<GameObject> locked = new HashSet<GameObject>();

        public IReadOnlyList<GameObject> SelectedObjects => selected;
        public int Count => selected.Count;

        public event System.Action OnSelectionChanged;

        public bool IsSelected(GameObject obj)
        {
            return obj != null && selected.Contains(obj);
        }

        public bool IsLocked(GameObject obj)
        {
            return obj != null && locked.Contains(obj);
        }

        public void Select(GameObject obj, bool addToSelection = false)
        {
            if (obj == null || IsLocked(obj)) return;

            if (!addToSelection)
                selected.Clear();

            if (!selected.Contains(obj))
            {
                selected.Add(obj);
                OnSelectionChanged?.Invoke();
            }
        }
        
        public void SetSelection(IReadOnlyList<GameObject> objects)
        {
            selected.Clear();
            if (objects != null)
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    var obj = objects[i];
                    if (obj == null || IsLocked(obj)) continue;
                    if (!selected.Contains(obj))
                        selected.Add(obj);
                }
            }
            OnSelectionChanged?.Invoke();
        }

        public void Deselect(GameObject obj)
        {
            if (obj == null) return;

            if (selected.Remove(obj))
                OnSelectionChanged?.Invoke();
        }

        public void Clear()
        {
            if (selected.Count == 0) return;

            selected.Clear();
            OnSelectionChanged?.Invoke();
        }

        public void LockSelected()
        {
            foreach (var obj in selected)
            {
                if (obj != null)
                    locked.Add(obj);
            }
            Clear();
        }

        public void UnlockSelected()
        {
            foreach (var obj in selected)
            {
                if (obj != null)
                    locked.Remove(obj);
            }
        }

        public void ToggleLock(GameObject obj)
        {
            if (obj == null) return;

            if (locked.Contains(obj))
                locked.Remove(obj);
            else
                locked.Add(obj);
        }
    }
}