using UnityEngine;

namespace _Project.Scripts
{
    public class ManaComponent : MonoBehaviour
    {
        [SerializeField, Min(0)] private int maxMana = 50;
        [SerializeField, Min(0)] private int currentMana = 50;

        private void Awake()
        {
            // Ensure current mana doesn't exceed max mana on start
            currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        }

        public int GetMana()
        {
            return currentMana;
        }

        public void SetMana(int mana)
        {
            currentMana = Mathf.Clamp(mana, 0, maxMana);
        }

        public int GetMaxMana()
        {
            return maxMana;
        }

        // Optional: Method to modify mana (e.g., for casting spells or regeneration)
        public void ModifyMana(int amount)
        {
            currentMana = Mathf.Clamp(currentMana + amount, 0, maxMana);
        }
    }
}