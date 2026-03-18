using Packages.Crest.Crest.Scripts;
using UnityEngine;

namespace _Project.Scripts.Ocean
{
    public class OceanFollower : MonoBehaviour
    {
        [SerializeField] private Transform playerTarget;
        private OceanRenderer _ocean;

        private void Awake() => _ocean = GetComponent<OceanRenderer>();

        private void LateUpdate()
        {
            if (playerTarget == null) return;
            Vector3 pos = _ocean.transform.position;
            pos.x = playerTarget.position.x;
            pos.z = playerTarget.position.z;
            _ocean.transform.position = pos;
        }
    }
}