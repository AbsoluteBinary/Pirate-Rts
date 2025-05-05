using System;
using System.Collections.Generic;
using System.Linq;
using _Project.Scripts.Inventory.Helpers;
using _Project.Scripts.Utility;
using Sirenix.OdinInspector;
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

public interface IBind<TData> where TData : ISaveable 
{
    SerializableGuid Id { get; set; }
    void Bind(TData data);
}

public class SaveLoadSystem : PersistentSingleton<SaveLoadSystem> 
{
    [SerializeField] public GameData gameData;
    [SerializeField] private bool debugEnabled = false; // Toggle debugging logs

    public void SetGameName(string newName) => gameData.gameName = newName;
    public string GetGameName() => gameData.gameName;
    public string GetLevelName() => gameData.LevelName;
    
    IDataService dataService;

    protected override void Awake() 
    {
        base.Awake();
        dataService = new FileDataService(new JsonSerializer());
    }
    
    void Start() => NewGame();

    [Button]
    private void DisplayPlayerID()
    {
        if (gameData == null || gameData.playerData == null)
        {
            Debug.LogError("gameData or gameData.playerData is null!");
            return;
        }
        if (debugEnabled)
        {
            Debug.Log($"Player ID: {gameData.playerData.Id.ToGuid()}");
        }
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
    {
        if (scene.name == "Menu") return;
        
        if (debugEnabled)
        {
            Debug.Log($"Binding for scene {scene.name}, playerData.Id = {(gameData.playerData != null ? gameData.playerData.Id.ToGuid() : "null")}");
        }
        
        Bind<Hero, PlayerData>(gameData.playerData);
        Bind<Inventory.Inventory, InventoryData>(gameData.inventoryData);
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    void Bind<T, TData>(TData data) where T : MonoBehaviour, IBind<TData> where TData : ISaveable, new() 
    {
        var entity = FindObjectsByType<T>(FindObjectsSortMode.None).FirstOrDefault();
        if (entity != null) 
        {
            if (data == null) 
            {
                data = new TData { Id = entity.Id };
            }
            entity.Bind(data);
            if (typeof(TData) == typeof(PlayerData)) 
            {
                gameData.playerData = data as PlayerData;
                if (debugEnabled)
                {
                    Debug.Log($"Bound Hero to playerData.Id: {gameData.playerData.Id.ToGuid()}");
                }
            }
        }
    }

    void Bind<T, TData>(List<TData> datas) where T : MonoBehaviour, IBind<TData> where TData : ISaveable, new() 
    {
        var entities = FindObjectsByType<T>(FindObjectsSortMode.None);
        foreach (var entity in entities) 
        {
            var data = datas.FirstOrDefault(d => d.Id == entity.Id);
            if (data == null) 
            {
                data = new TData { Id = entity.Id };
                datas.Add(data); 
            }
            entity.Bind(data);
        }
    }

    public void NewGame() 
    {
        gameData = new GameData 
        {
            gameName = "My Game",
            LevelName = "BootstrapScene",
            playerData = new PlayerData { Id = SerializableGuid.NewGuid() },
            inventoryData = new InventoryData()
        };
        if (debugEnabled)
        {
            Debug.Log($"NewGame: Initialized with playerData.Id = {gameData.playerData.Id.ToGuid()}");
        }
        SceneManager.LoadScene(gameData.LevelName);
    }
    
    public void SaveGame() => dataService.Save(gameData);

    public void LoadGame(string gameName) 
    {
        gameData = dataService.Load(gameName);
        if (debugEnabled)
        {
            Debug.Log($"Loaded game: playerData.Id = {(gameData.playerData != null ? gameData.playerData.Id.ToGuid() : "null")}");
        }
        if (string.IsNullOrWhiteSpace(gameData.LevelName)) 
        {
            gameData.LevelName = "Demo";
        }
        SceneManager.LoadScene(gameData.LevelName);
    }
    
    public void ReloadGame() => LoadGame(gameData.gameName);

    public void DeleteGame(string gameName) => dataService.Delete(gameName);
}
}