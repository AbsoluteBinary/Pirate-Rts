using UnityEngine;

namespace _Project.Scripts.Fleet.Data
{
    [CreateAssetMenu(menuName = "Fleet/Formation Data", fileName = "New Formation")]
    public class FormationData : ScriptableObject
    {
        [Header("Formation Type")]
        public FormationType formationType = FormationType.Line;

        [Header("General Settings")]
        [Tooltip("Horizontal spacing between ships")]
        public float spacing = 8f;

        [Tooltip("Depth spacing (how far back each row is)")]
        public float depthSpacing = 10f;

        [Tooltip("Radius used for Circle formation")]
        public float radius = 15f;

        [Tooltip("How tightly ships try to stay in position")]
        [Range(0.5f, 3f)]
        public float tightness = 1.2f;

        public enum FormationType
        {
            Line,       // Side by side
            Column,     // Single file behind flagship
            Wedge,      // V shape
            Arrowhead,  // Inverted V
            Circle,     // Defensive ring around flagship
            Echelon     // Diagonal line
        }

        /// <summary>
        /// Returns the local offset position for a ship at given index
        /// </summary>
        public Vector3 GetOffset(int index, int totalShips)
        {
            if (index == 0) return Vector3.zero; // Flagship is always at center

            Vector3 offset = Vector3.zero;

            switch (formationType)
            {
                case FormationType.Line:
                    float x = (index - totalShips / 2f) * spacing;
                    offset = new Vector3(x, 0, -depthSpacing * 0.5f);
                    break;

                case FormationType.Column:
                    offset = new Vector3(0, 0, -index * depthSpacing);
                    break;

                case FormationType.Wedge:
                    x = (index % 2 == 0 ? 1 : -1) * (index * spacing * 0.5f);
                    float z = -index * depthSpacing * 0.8f;
                    offset = new Vector3(x, 0, z);
                    break;

                case FormationType.Circle:
                    float angle = (index / (float)totalShips) * Mathf.PI * 2f;
                    offset = new Vector3(Mathf.Sin(angle) * radius, 0, Mathf.Cos(angle) * radius);
                    break;

                case FormationType.Echelon:
                    offset = new Vector3(index * spacing * 0.7f, 0, -index * depthSpacing * 0.7f);
                    break;

                default:
                    offset = new Vector3(index * 6f, 0, -index * 8f);
                    break;
            }

            return offset * tightness;
        }
    }
}