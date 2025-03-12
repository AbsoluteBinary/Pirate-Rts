
using UnityEngine;

namespace StrategyCamera
{
    public interface ISelectable
    {
        void Select(SelectionProperties properties);
        void Deselect();
        Transform GetSelfTransform();
    }
}
