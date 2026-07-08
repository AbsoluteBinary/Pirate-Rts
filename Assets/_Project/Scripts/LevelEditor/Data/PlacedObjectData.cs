using System;
using UnityEngine;

namespace _Project.Scripts.LevelEditor.Data
{
    [Serializable]
    public class PlacedObjectData
    {
        public GameObject Prefab;
        public Vector3 Position;
        public Quaternion Rotation = Quaternion.identity;
        public Vector3 Scale = Vector3.one;

        // Future expansion example:
        // public float Health;
    }
}