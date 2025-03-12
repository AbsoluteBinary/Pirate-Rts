
using UnityEngine;

namespace StrategyCamera
{
    public class CubeMaker : MonoBehaviour
    {
        public GameObject cubePrefab;
        public int cubeCount = 100;
        public Vector3 lowerLeftlimit, upperRightLimit;
        void Start()
        {
            for (int i = 0; i < cubeCount; i++)
            {
                Vector3 pos = new Vector3(Random.Range(lowerLeftlimit.x, upperRightLimit.x), Random.Range(lowerLeftlimit.y, upperRightLimit.y), Random.Range(lowerLeftlimit.z, upperRightLimit.z));
                var go = Instantiate(cubePrefab, pos, Quaternion.identity);
                //GetRendererAndRandomColor(go);
                go.transform.SetParent(this.transform);
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        void GetRendererAndRandomColor(GameObject go)
        {
            var renderer = go.transform.GetChild(0).GetComponent<Renderer>();
            renderer.material.color = new Color(Random.value, Random.value, Random.value);
        }
    }
}