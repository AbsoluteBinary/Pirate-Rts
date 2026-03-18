using _Project.Scripts.Persistence;
using _Project.Scripts.Utility;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.Save_Load_Info
{
    public class SaveLoadAdmin : PersistentSingleton<SaveLoadAdmin>
    {
        [PropertyTooltip("SaveLoadSystem Reference")]
        [SerializeField]  SaveLoadSystem saveLoadSystem;
        
        [PropertyTooltip("Game Data")]
        [SerializeField]  TMP_Text gameNameText;
        [SerializeField]  TMP_Text gameLevelText;
        
        [PropertyTooltip("Player Data")]
        [SerializeField]  Text playerId;
        [SerializeField]  TMP_Text idText;
        [SerializeField]  TMP_Text positionText;
        
        [PropertyTooltip("Harbour Data")]
        [SerializeField]  TMP_Text harbourIdText;
        [SerializeField]  TMP_Text harbourCoinsText;

        string playerIdString;
        // public void FixedUpdate()
        // {
        //     playerIdString = SaveLoadSystem.Instance.gameData.playerData.Id.ToGuid().ToString();
        // }

        public void SetText()
        {
            Debug.Log(SaveLoadSystem.Instance.gameData.playerData.Id.ToGuid().ToString());
            //playerIdString = saveLoadSystem.gameData.playerData.Id.ToGuid().ToString();
            
            
            gameNameText.text = SaveLoadSystem.Instance.GetGameName();
            gameLevelText.text = SaveLoadSystem.Instance.GetLevelName();
            idText.text = SaveLoadSystem.Instance.gameData.playerData.Id.ToGuid().ToString();
            //harbourIdText.text = saveLoadSystem.gameData.harbourData.Id.ToString();
            //harbourCoinsText.text = saveLoadSystem.gameData.harbourData.Coins.ToString();
        }

        public void UpdateTxtFields()
        {
            SetText();
        }
    }
}
