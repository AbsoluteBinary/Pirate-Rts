using UnityEngine;

namespace _Project.Scripts.Managers.Registry
{
    public class RegisterHarbourObject : MonoBehaviour
    {
        [SerializeField] private string registryKey = "Default"; // "IdleCamera", "HarbourBuildCamera", "TGSGrid", "BootIOBox", etc.

        private void OnEnable()
        {
            if (registryKey.StartsWith("Camera"))
                HarbourObjectRegistry.RegisterCamera(registryKey, GetComponent<Camera>());
            else if (registryKey == "TGSGrid")
                HarbourObjectRegistry.RegisterTGSGrid(gameObject);
            else
                HarbourObjectRegistry.RegisterIOBox(registryKey, gameObject);
        }

        private void OnDisable()
        {
            if (registryKey.StartsWith("Camera"))
                HarbourObjectRegistry.UnregisterCamera(registryKey);
            else if (registryKey == "TGSGrid")
                HarbourObjectRegistry.UnregisterTGSGrid();
            else
                HarbourObjectRegistry.UnregisterIOBox(registryKey);
        }
    }
}