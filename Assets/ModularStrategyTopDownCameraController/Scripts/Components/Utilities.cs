
using UnityEngine;

namespace StrategyCamera
{
    public static class CameraToolsUtilities
    {
        public static (bool, Vector3 point) GetPointerWorldPosition(Vector2 screenPosition, Camera camera)
        {
            var viewportPoint = camera.ScreenToViewportPoint(screenPosition);
            return GetViewportToWorldPosition(viewportPoint, camera);
        }

        public static (bool, Vector3 point) GetViewportToWorldPosition(Vector2 viewportPoint, Camera camera)
        {
            Plane p = new Plane(Vector3.up, Vector3.zero);
            Ray ray = camera.ViewportPointToRay(viewportPoint);

            float distance = 0;
            if (p.Raycast(ray, out distance))
            {
                var worldPos = ray.GetPoint(distance);
                return (true, worldPos);
            }

            return (false, Vector3.zero);
        }

        public static void Log(string message)
        {
            Debug.unityLogger.Log(LogType.Log, message);
        }
        public static bool IsDeltaSmall(Vector2 delta)
        {
            return Mathf.Abs(delta.x) < Mathf.Epsilon && Mathf.Abs(delta.y) < Mathf.Epsilon;
        }
    }
}   