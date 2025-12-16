using _Project.Scripts.SceneManagement;
using UI.WorldMap.HUDInteractions;
using UnityEngine;

namespace UI.WorldMap
{
    public class WorldMapHUD : MonoBehaviour
    {
        // No serialized fields needed yet – expand later

        private void Awake()
        {
            //_task1 = SceneLoader.Instance.BeginSceneTransition(1);
            HUDMenuButtonsEventBus.TriggerHUDEnterBaseClicked();;
        }
        private void OnGUI()
        {
            // TOP RIGHT: Enter Harbour button
            GUILayout.BeginArea(new Rect(Screen.width - 220, 20, 200, 50));
            if (GUILayout.Button("Enter Harbour", GUILayout.Height(40)))
            {
                Debug.Log("Enter Harbour clicked – call scene transition here");
                // Example: SceneLoader.Instance.BeginSceneTransition(2);  // Adjust index
                SceneLoader.Instance.BeginSceneTransition(2);

            }
            GUILayout.EndArea();
        }
    }
}
