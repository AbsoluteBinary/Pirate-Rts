using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.BaseBuilder.Runtime.Placement
{
    public class WireframePreview
    {
        private readonly List<LineRenderer> _lines = new List<LineRenderer>();
        private Material _cyanMaterial;
        private readonly Transform _parent;
        private readonly float _lineWidth;

        public WireframePreview(Transform parent = null, float lineWidth = 0.14f)
        {
            _parent = parent;
            _lineWidth = lineWidth;
            CreateMaterial();
        }

        private void CreateMaterial()
        {
            Shader shader = Shader.Find("Unlit/Color");
            if (shader == null)
                shader = Shader.Find("Sprites/Default");

            _cyanMaterial = new Material(shader);
            _cyanMaterial.color = Color.cyan;
        }

        public void Clear()
        {
            foreach (var lr in _lines)
            {
                if (lr != null)
                    Object.Destroy(lr.gameObject);
            }
            _lines.Clear();
        }
        
        /// <summary>
        /// Draws a cyan wireframe cube at each centre position.
        /// size = full width of one tile (usually grid.cellSize.x)
        /// </summary>
        public void ShowCubes(IReadOnlyList<Vector3> centers, float size)
        {
            Clear();
            if (centers == null || centers.Count == 0) return;

            float half = size * 0.5f;

            foreach (var center in centers)
            {
                // Bottom 4 corners (y = center.y)
                Vector3 b0 = center + new Vector3(-half, 0f, -half);
                Vector3 b1 = center + new Vector3( half, 0f, -half);
                Vector3 b2 = center + new Vector3( half, 0f,  half);
                Vector3 b3 = center + new Vector3(-half, 0f,  half);

                // Top 4 corners (y = center.y + size)
                Vector3 t0 = center + new Vector3(-half, size, -half);
                Vector3 t1 = center + new Vector3( half, size, -half);
                Vector3 t2 = center + new Vector3( half, size,  half);
                Vector3 t3 = center + new Vector3(-half, size,  half);

                // Bottom square
                DrawEdge(b0, b1);
                DrawEdge(b1, b2);
                DrawEdge(b2, b3);
                DrawEdge(b3, b0);

                // Top square
                DrawEdge(t0, t1);
                DrawEdge(t1, t2);
                DrawEdge(t2, t3);
                DrawEdge(t3, t0);

                // Vertical edges
                DrawEdge(b0, t0);
                DrawEdge(b1, t1);
                DrawEdge(b2, t2);
                DrawEdge(b3, t3);
            }
        }

        private void DrawEdge(Vector3 a, Vector3 b)
        {
            var go = new GameObject("WireEdge");
            if (_parent != null)
                go.transform.SetParent(_parent, true);

            var lr = go.AddComponent<LineRenderer>();
            lr.sharedMaterial = _cyanMaterial;
            lr.startWidth = _lineWidth;
            lr.endWidth = _lineWidth;
            lr.positionCount = 2;
            lr.useWorldSpace = true;
            lr.alignment = LineAlignment.View;          // always face camera
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.numCapVertices = 2;
            lr.SetPosition(0, a);
            lr.SetPosition(1, b);

            _lines.Add(lr);
        }
    }
}