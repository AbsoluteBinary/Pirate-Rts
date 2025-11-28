using UnityEditor;
using UnityEngine;

namespace Editor_Shapes
{
    public class CreateCone : ScriptableWizard
    {
        [Header("Cone Settings")]
        public int numVertices = 12;        // More = smoother (8-16 is good)
        public float radiusTop = 0f;        // 0 = pointy tip
        public float radiusBottom = 1f;     // Base width
        public float height = 4f;           // Total height

        [MenuItem("GameObject/3D Object/Create Cone")]
        static void Create()
        {
            ScriptableWizard.DisplayWizard<CreateCone>("Create Cone", "Create");
        }

        private void OnWizardCreate()
        {
            // Step 1: Create the GameObject
            GameObject coneGO = new GameObject("Cone");
            Undo.RegisterCreatedObjectUndo(coneGO, "Create Cone"); // Teaching: Undo support!

            // Step 2: Add MeshFilter & MeshRenderer
            MeshFilter meshFilter = coneGO.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = coneGO.AddComponent<MeshRenderer>();

            // Step 3: Generate the cone mesh (teaching: procedural basics)
            Mesh coneMesh = GenerateConeMesh();
            meshFilter.mesh = coneMesh;

            // Step 4: Assign a DEFAULT material (no runtime creation = no Odin error!)
            // Use Unity's built-in primitive material (asset-safe)
            Material defaultMat = Resources.Load<Material>("Default-Material"); // Or drag your own
            if (defaultMat != null)
            {
                meshRenderer.material = defaultMat;
            }
            else
            {
                // Fallback: Let Unity auto-assign (no error)
                Debug.Log("Using Unity's auto material — add your own for glow!");
            }

            // Step 5: Select it for easy tweaking
            Selection.activeGameObject = coneGO;

            Debug.Log("Cone created! Tip: Add a glowing material for combat polish.");
        }

        private Mesh GenerateConeMesh()
        {
            // Teaching: Basic procedural mesh (tip + base circle + sides)
            Mesh mesh = new Mesh { name = "ConeMesh" };
            int segments = numVertices;

            // Vertices: Tip (0) + base circle (1 to segments)
            Vector3[] vertices = new Vector3[segments + 1];
            vertices[0] = Vector3.zero; // Tip at top

            for (int i = 0; i < segments; i++)
            {
                float angle = (i / (float)segments) * 2 * Mathf.PI;
                vertices[i + 1] = new Vector3(
                    Mathf.Sin(angle) * radiusBottom,
                    -height,  // Base at bottom
                    Mathf.Cos(angle) * radiusBottom
                );
            }

            // Triangles: Connect tip to base edges
            int[] triangles = new int[segments * 3];
            int triIndex = 0;
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                triangles[triIndex++] = 0;          // Tip
                triangles[triIndex++] = i + 1;      // Current base
                triangles[triIndex++] = next + 1;   // Next base
            }

            // Apply to mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateNormals(); // For lighting
            mesh.RecalculateBounds();

            return mesh;
        }
    }
}
