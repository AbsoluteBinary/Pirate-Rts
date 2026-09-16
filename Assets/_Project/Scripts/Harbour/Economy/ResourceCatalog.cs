using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.Harbour.Economy
{
    public enum ResourceKind
    {
        Material,
        Currency,
        Token
    }

    [Serializable]
    public class ResourceDef
    {
        public string id;
        public string displayName;
        public ResourceKind kind;
        public Sprite icon;
    }

    [CreateAssetMenu(menuName = "Harbour/Resource Catalog", fileName = "ResourceCatalog")]
    public class ResourceCatalog : ScriptableObject
    {
        public List<ResourceDef> resources = new List<ResourceDef>();

        public ResourceDef Get(string id)
        {
            if (string.IsNullOrEmpty(id) || resources == null) return null;
            foreach (var r in resources)
                if (r != null && r.id == id) return r;
            return null;
        }

        public IEnumerable<ResourceDef> ByKind(ResourceKind kind)
        {
            if (resources == null) yield break;
            foreach (var r in resources)
                if (r != null && r.kind == kind) yield return r;
        }

        private void Reset()
        {
            resources = new List<ResourceDef>
            {
                Mat("Oil"), Mat("Iron"), Mat("Steel"),
                Mat("Energy"), Mat("Aluminium"), Mat("Lumber"),
                Mat("Alloy"), Mat("Cloth"), Mat("Uranium"),
                Cur("Gold"), Cur("Silver"),
                Tok("PieceOfEight", "Piece of Eight"),
                Tok("Shilling", "Shilling")
            };
        }

        private static ResourceDef Mat(string name) =>
            new ResourceDef { id = name, displayName = name, kind = ResourceKind.Material };

        private static ResourceDef Cur(string name) =>
            new ResourceDef { id = name, displayName = name, kind = ResourceKind.Currency };

        private static ResourceDef Tok(string id, string display) =>
            new ResourceDef { id = id, displayName = display, kind = ResourceKind.Token };
    }
}