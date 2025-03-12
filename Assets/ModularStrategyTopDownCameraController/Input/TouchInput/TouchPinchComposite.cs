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
    [DisplayName("T1Delta/T2Delta/T1Position/T2Position Binding")]

    public class TouchPinchComposite : InputBindingComposite<float>
    {

        [InputControl(layout = "Vector2")] public int deltaTouchOne;
        [InputControl(layout = "Vector2")] public int deltaTouchTwo;

        [InputControl(layout = "Vector2")] public int posTouchOne;
        [InputControl(layout = "Vector2")] public int posTouchTwo;
        public override float ReadValue(ref InputBindingCompositeContext context)
        {

            if (Touch.activeTouches.Count != 2)
            {
                return 0;
            }


            var delta1 = context.ReadValue<Vector2, Vector2MagnitudeComparer>(this.deltaTouchOne);
            var delta2 = context.ReadValue<Vector2, Vector2MagnitudeComparer>(this.deltaTouchTwo);
            var pos1 = context.ReadValue<Vector2, Vector2MagnitudeComparer>(this.posTouchOne);
            var pos2 = context.ReadValue<Vector2, Vector2MagnitudeComparer>(this.posTouchTwo);

            var dot = Vector2.Dot(delta1, delta2);
            if (dot > 0)
            {
                return 0;
            }
            //return Mathf.Sign(delta2.x);
            var v1 = pos1 - delta1;
            var v2 = pos2 - delta2;
            var distanceold = (v1 - v2).sqrMagnitude;
            var distanceNew = (pos2 - pos1).sqrMagnitude;

            var distanceChange = distanceNew - distanceold;
            if (Mathf.Abs(distanceChange) < Mathf.Epsilon)
            {
                return 0;
            }

            return  Mathf.Sqrt(Mathf.Abs(distanceChange)) * Mathf.Sign(distanceChange) * 0.1f;
        }

#if UNITY_EDITOR
        static TouchPinchComposite()
        {
            Register();
        }

#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Register()
        {
            InputSystem.RegisterBindingComposite<TouchPinchComposite>();
        }

    }
}
#endif