using UnityEngine;

namespace _Project.Scripts.Harbour.Economy
{
    public class ResourceWalletHolder : MonoBehaviour
    {
        [SerializeField] private ResourceCatalog catalog;

        public ResourceCatalog Catalog => catalog;
        public ResourceWallet Wallet { get; private set; }

        private void OnEnable()
        {
            Wallet = new ResourceWallet();
            Wallet.LoadOrCreate(catalog);
        }

        public void Save() => Wallet?.Save();
    }
}