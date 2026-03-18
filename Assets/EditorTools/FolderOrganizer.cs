// Put this in Assets/Editor/

using System.IO;
using UnityEditor;
using UnityEngine;

namespace EditorTools
{
    public class FolderOrganizer : EditorWindow
    {
        [MenuItem("Tools/Folder Organizer")]
        private static void OpenWindow()
        {
            GetWindow<FolderOrganizer>("Folder Organizer");
        }

        private void OnGUI()
        {
            GUILayout.Label("WARNING: Make a backup before using!", EditorStyles.boldLabel);

            if (GUILayout.Button("Scan & Show Preview (Dry Run)"))
            {
                Debug.Log("Would move all .cs files from root to Assets/Scripts/");
                // Add real scanning logic here later
            }

            GUI.color = Color.red;
            if (GUILayout.Button("APPLY CHANGES (DANGEROUS)"))
            {
                if (EditorUtility.DisplayDialog("Confirm",
                        "This will MOVE files in the project.\nAre you really sure?", "Yes", "Cancel"))
                {
                    PerformMove();
                }
            }
            GUI.color = Color.white;
        }

        private void PerformMove()
        {
            string sourceFolder = "Assets";
            string targetFolder = "Assets/Scripts";

            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);

            foreach (string file in Directory.GetFiles(sourceFolder, "*.cs", SearchOption.TopDirectoryOnly))
            {
                string fileName = Path.GetFileName(file);
                string dest = Path.Combine(targetFolder, fileName);

                AssetDatabase.RenameAsset(file, dest);
                Debug.Log($"Moved: {fileName}");
            }

            AssetDatabase.Refresh();
            Debug.Log("Move finished. Check console for errors.");
        }
    }
}