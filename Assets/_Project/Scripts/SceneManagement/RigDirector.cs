using UnityEngine;

namespace _Project.Scripts.SceneManagement
{
    public class RigDirector : MonoBehaviour
    {
        [SerializeField] private GameObject bootCam;
        [SerializeField] private GameObject freeRoamCam;
        [SerializeField] private GameObject harbourCam;
        [SerializeField] private GameObject combatCam;

        public void Show(int groupIndex)
        {
            if (bootCam != null) bootCam.SetActive(groupIndex == 0);
            if (freeRoamCam != null) freeRoamCam.SetActive(false);
            if (harbourCam != null) harbourCam.SetActive(false);
            if (combatCam != null) combatCam.SetActive(false);

            if (groupIndex == 0)
                GameplayView.Instance?.Follow(null);
        }
    }
}