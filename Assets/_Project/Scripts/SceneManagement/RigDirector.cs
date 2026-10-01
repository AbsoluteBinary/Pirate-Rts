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
            if (freeRoamCam != null) freeRoamCam.SetActive(groupIndex == 1);
            if (harbourCam != null) harbourCam.SetActive(groupIndex == 2);
            if (combatCam != null) combatCam.SetActive(groupIndex == 3);
        }
    }
}