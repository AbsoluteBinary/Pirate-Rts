using Unity.Cinemachine;
using UnityEngine;

namespace _Project.Scripts.SceneManagement
{
    public enum VcamChannel
    {
        FreeRoam = 1,
        Harbour = 2,
        Combat = 3
    }

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

        public void SetFollow(VcamChannel channel, Transform target)
        {
            var vcam = CameraFor(channel);
            if (vcam != null)
                vcam.Follow = target;
        }

        private CinemachineCamera CameraFor(VcamChannel channel)
        {
            GameObject go = channel switch
            {
                VcamChannel.FreeRoam => freeRoamCam,
                VcamChannel.Harbour => harbourCam,
                VcamChannel.Combat => combatCam,
                _ => null
            };

            return go != null ? go.GetComponent<CinemachineCamera>() : null;
        }
    }
}