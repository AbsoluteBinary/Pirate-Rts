
using UnityEngine;

namespace StrategyCamera
{
    internal class SelectableCube : MonoBehaviour, ISelectable
    {
        public float speed;
        public float highRange = 5;

        private bool shouldMoveHorizontal = false;
        private Renderer m_renderer;
        private Color m_color;
        private Vector3 anchorPos;
        private Rigidbody rb;
        private void Awake()
        {
            m_renderer = transform.GetChild(0).GetComponent<MeshRenderer>();
            m_color = m_renderer.material.color;
            anchorPos = transform.position;
            rb = GetComponent<Rigidbody>();
            shouldMoveHorizontal = UnityEngine.Random.Range(0f, 1f) > 0.5f;
        }
        public void Deselect()
        {
            m_renderer.material.color = m_color;
        }

        public void Select(SelectionProperties properties)
        {
            m_renderer.material.color = properties.color;
        }

        public Transform GetSelfTransform()
        {
            return transform;
        }

        private void Update()
        {

        }

        private void FixedUpdate()
        {
            float val = Mathf.PingPong(Time.time, highRange) - (highRange / 2);
            Vector3 pos = Vector3.zero;
            if (shouldMoveHorizontal)
            {
                pos.x = val;
            }
            else
            {
                pos.z = val;
            }
            rb.linearVelocity = pos;
        }
    }
}
