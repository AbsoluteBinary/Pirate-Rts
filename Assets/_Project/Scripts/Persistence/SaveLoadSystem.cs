using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Inventory.Helpers;
using Systems.Inventory;
using Systems.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Project.Scripts.Persistence {
    [Serializable] 
    public class GameData 
    { 
        public string gameName;
        public string LevelName;  
        
        public PlayerData playerData;
        public InventoryData inventoryData;
    }
        
    public interface ISaveable  
    {
        SerializableGuid Id { get; set; }
    }
    
    public interface IBind<TData> where TData : ISaveable {
        SerializableGuid Id { get; set; }
        void Bind(TData data);
    }

    
    
    public class SaveLoadSystem : PersistentSingleton<SaveLoadSystem> {
        [SerializeField] public GameData gameData;
        public void SetGameName(string newName)
        {
            gameData.gameName = newName;
        }
        public string GetGameName()
        {
            return gameData.gameName;
        }

        public string GetLevelName()
        {
            return gameData.LevelName;
        }
        
        IDataService dataService;
        protected override void Awake() {
            base.Awake();
            dataService = new FileDataService(new JsonSerializer());
        }
        
        void Start()
        {
            NewGame();
        }

        private void LateUpdate()
        {
            //Debug.Log(gameData.playerData.Id.ToGuid().ToString());
        }

        void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
        void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
        
        void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            if (scene.name == "Menu") return;
            
            Bind<Hero, PlayerData>(gameData.playerData);
            Bind<Inventory.Inventory, InventoryData>(gameData.inventoryData);
        }
        
        void Bind<T, TData>(TData data) where T : MonoBehaviour, IBind<TData> where TData : ISaveable, new() {
            var entity = FindObjectsByType<T>(FindObjectsSortMode.None).FirstOrDefault();
            if (entity != null) {
                if (data == null) {
                    data = new TData { Id = entity.Id };
                }
                entity.Bind(data);
            }
        }

        void Bind<T, TData>(List<TData> datas) where T: MonoBehaviour, IBind<TData> where TData : ISaveable, new() {
            var entities = FindObjectsByType<T>(FindObjectsSortMode.None);

            foreach(var entity in entities) {
                var data = datas.FirstOrDefault(d=> d.Id == entity.Id);
                if (data == null) {
                    data = new TData { Id = entity.Id };
                    datas.Add(data); 
                }
                entity.Bind(data);
            }
        }
        


        public void NewGame() {
            gameData = new GameData {
                gameName = "My Game",
                LevelName = "Demo"
            };
            SceneManager.LoadScene(gameData.LevelName);
        }
        
        public void SaveGame() => dataService.Save(gameData);

        public void LoadGame(string gameName) {
            gameData = dataService.Load(gameName);

            if (String.IsNullOrWhiteSpace(gameData.LevelName)) {
                gameData.LevelName = "Demo";
            }

            SceneManager.LoadScene(gameData.LevelName);
        }
        
        public void ReloadGame() => LoadGame(gameData.gameName);

        public void DeleteGame(string gameName) => dataService.Delete(gameName);
    }
}