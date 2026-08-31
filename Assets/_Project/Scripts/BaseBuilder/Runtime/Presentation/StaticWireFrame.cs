using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.BaseBuilder.Runtime.Presentation
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter))]
    public class StaticWireFrame : MonoBehaviour
    {
        [Header("Look")]
        [SerializeField] private Color color = Color.cyan;
        [SerializeField, Min(1f)] private float size = 1.001f;
        [SerializeField] private Material wireMaterial;

        [Header("Source")]
        [SerializeField] private MeshFilter sourceFilter;
        [SerializeField] private bool rebuildOnEnable = true;

        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private List<Vector3> _edges;
        private Material _runtimeMat;
        private Mesh _builtFrom;

        public Color Color
        {
            get => color;
            set
            {
                color = value;
                ApplyColorToMaterial();
            }
        }

        public float Size
        {
            get => size;
            set => size = Mathf.Max(1f, value);
        }

        private void Awake()
        {
            if (sourceFilter == null)
                sourceFilter = GetComponent<MeshFilter>();
            EnsureMaterial();
            BuildEdges();
        }

        private void OnEnable()
        {
            if (rebuildOnEnable)
                BuildEdges();
        }

        private void OnDestroy()
        {
            if (_runtimeMat != null)
                Destroy(_runtimeMat);
        }

        private void OnValidate()
        {
            size = Mathf.Max(1f, size);
            ApplyColorToMaterial();
        }

        public void Rebuild()
        {
            _builtFrom = null;
            BuildEdges();
        }

        private void EnsureMaterial()
        {
            if (wireMaterial != null)
            {
                ApplyColorToMaterial();
                return;
            }

            if (_runtimeMat == null)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored")
                                ?? Shader.Find("Sprites/Default")
                                ?? Shader.Find("Unlit/Color");
                _runtimeMat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
                _runtimeMat.SetInt("_ZWrite", 0);
                _runtimeMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _runtimeMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }

            ApplyColorToMaterial();
        }

        private void ApplyColorToMaterial()
        {
            Material mat = wireMaterial != null ? wireMaterial : _runtimeMat;
            if (mat == null) return;
            if (mat.HasProperty(ColorId))
                mat.SetColor(ColorId, color);
        }

        private void BuildEdges()
        {
            if (sourceFilter == null)
                sourceFilter = GetComponent<MeshFilter>();

            Mesh mesh = sourceFilter != null ? sourceFilter.sharedMesh : null;
            if (mesh == null)
            {
                Debug.LogError($"[StaticWireFrame] No mesh on {name}", this);
                _edges = null;
                return;
            }

            if (_edges != null && _builtFrom == mesh)
                return;

            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            _edges = new List<Vector3>(triangles.Length * 2);
            _builtFrom = mesh;

            for (int i = 0; i + 2 < triangles.Length; i += 3)
            {
                Vector3 a = vertices[triangles[i]];
                Vector3 b = vertices[triangles[i + 1]];
                Vector3 c = vertices[triangles[i + 2]];
                _edges.Add(a);
                _edges.Add(b);
                _edges.Add(b);
                _edges.Add(c);
                _edges.Add(c);
                _edges.Add(a);
            }
        }

        private void OnRenderObject()
        {
            if (!isActiveAndEnabled) return;
            if (_edges == null || _edges.Count == 0)
                BuildEdges();
            if (_edges == null || _edges.Count == 0) return;

            EnsureMaterial();
            Material mat = wireMaterial != null ? wireMaterial : _runtimeMat;
            if (mat == null) return;

            mat.SetPass(0);

            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);
            GL.Begin(GL.LINES);
            GL.Color(color);

            float s = size;
            for (int i = 0; i < _edges.Count; i++)
            {
                Vector3 v = _edges[i] * s;
                GL.Vertex3(v.x, v.y, v.z);
            }

            GL.End();
            GL.PopMatrix();
        }
    }
}