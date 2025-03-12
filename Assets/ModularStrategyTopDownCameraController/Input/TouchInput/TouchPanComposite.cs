#if ENABLE_INPUT_SYSTEM
using System.ComponentModel;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.InputSystem;
using UnityEngine;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StrategyCamera
{
    [DisplayStringFormat("{TouchOne}/{TouchTwo}")]
    [DisplayName("T1Delta/T2Delta Binding")]

    public class TouchPanComposite : InputBindingComposite<Vector2>
    {

        [InputControl(layout = "Vector2")] public int deltaTouchOne;
        [InputControl(layout = "Vector2")] public int deltaTouchTwo;
        public override Vector2 ReadValue(ref InputBindingCompositeContext context)
        {
            if (Touch.activeTouches.Count != 2)
            {
                return Vector2.zero;
            }

            var delta1 = context.ReadValue<Vector2, Vector2MagnitudeComparer>(this.deltaTouchOne);
            var delta2 = context.ReadValue<Vector2, Vector2MagnitudeComparer>(this.deltaTouchTwo);

            var dir = Vector2.Dot(delta1.normalized, delta2.normalized);
            if (dir < 0.707f)
            {
                return Vector2.zero;
            }

            return delta1;
        }


#if UNITY_EDITOR
        static TouchPanComposite()
        {
            Register();
        }

#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            InputSystem.RegisterBindingComposite<TouchPanComposite>();
        }

    }
}
#endif