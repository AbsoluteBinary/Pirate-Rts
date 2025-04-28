using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.Persistence.Editor {
    [CustomEditor(typeof(SaveLoadSystem))]
    public class SaveManagerEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            SaveLoadSystem saveLoadSystem = (SaveLoadSystem)target;
            string gameName = saveLoadSystem.gameData?.gameName ?? "My Game";
            
            DrawDefaultInspector();
            
            if (GUILayout.Button("New Game")) {
                saveLoadSystem.NewGame();
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(saveLoadSystem);
#endif
            }

            if (GUILayout.Button("Save Game")) {
                saveLoadSystem.SaveGame();
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(saveLoadSystem);
#endif
            }

            if (GUILayout.Button("Load Game")) {
                saveLoadSystem.LoadGame(gameName);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(saveLoadSystem);
#endif
            }
            
            if (GUILayout.Button("Delete Game")) {
                saveLoadSystem.DeleteGame(gameName);
#if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(saveLoadSystem);
#endif
            }

#if UNITY_EDITOR
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
#endif
        }
    }
}