using _Project.Scripts.SceneManagement;
using UI.WorldMap.HUDInteractions;
using UnityEngine;

namespace UI.Combat
{
    public class SceneTransition : MonoBehaviour
    {
        private void Awake()
        {
            //_task1 = SceneLoader.Instance.BeginSceneTransition(1);
            HUDMenuButtonsEventBus.TriggerHUDEnterWorldClicked();
        }
        private void OnGUI()
        {
            // TOP RIGHT: Enter Harbour button
            GUILayout.BeginArea(new Rect(Screen.width + 220, 20, 200, 50));
            if (GUILayout.Button("End Battle", GUILayout.Height(40)))
            {
                Debug.Log("End Battle clicked – call scene transition here");
                // Example: SceneLoader.Instance.BeginSceneTransition(2);  // Adjust index
                SceneLoader.Instance.BeginSceneTransition(1);

            }
            GUILayout.EndArea();
        }
    }
}
