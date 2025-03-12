
using UnityEngine;
namespace StrategyCamera
{
    [RequireComponent(typeof(StrategyCameraController))]
    public class TerrainContourFollowComponent : MonoBehaviour
    {
        public LayerMask terrainMask;
        private StrategyCameraController controller;
        private float distanceToGround = 0;

        void Start()
        {
            this.controller = GetComponent<StrategyCameraController>();
            distanceToGround = GetDistanceToSurface(controller.cameraTransform.position, terrainMask).Item2;
        }

        // Update is called once per frame
        void Update()
        {
            var hitDetails = GetDistanceToSurface(controller.cameraTransform.position, terrainMask);
            if(hitDetails.Item1 == false)
            {
                return;
            }
            float tempDistance = hitDetails.Item2;
            float deltaDistance = distanceToGround - tempDistance;
            var worldRigPos = controller.transform.position;
            worldRigPos.y = worldRigPos.y + deltaDistance;
            controller.transform.position = worldRigPos;
        }

        public (bool,float) GetDistanceToSurface(Vector3 origin, LayerMask layerMask)
        {
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                return (true, hit.distance); 
            }

            return (false, 0);
        }
    }
}