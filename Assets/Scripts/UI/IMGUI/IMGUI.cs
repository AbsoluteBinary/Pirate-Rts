using UnityEngine;

namespace UI.IMGUI
{
    public class SimpleIMGUI : MonoBehaviour
    {
        public float health = 75f;
        public int ammo = 32;

        private void OnGUI()
        {
            // Top-left HUD
            GUILayout.BeginArea(new Rect(20, 20, 250, 150));

            GUILayout.Label("<size=24>Ship Status</size>");
            GUILayout.Label($"Health: {health:F0} / 100");
            GUILayout.Label($"Cannon Ammo: {ammo}");

            if (GUILayout.Button("Fire Broadside!"))
            {
                Debug.Log("BOOM!");
                ammo -= 8;
            }

            GUILayout.EndArea();
        }
    }
}
