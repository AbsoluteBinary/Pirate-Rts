using UnityEngine;
using UnityEditor;
using _Project.Scripts.Harbour.ShipBuilder;

namespace _Project.Editor
{
    // [CustomEditor(typeof(HullData))]
    // public class HullDataEditor : UnityEditor.Editor
    // {
    //     private Vector2 _scrollPos;
    //     private Rect _previewRect;
    //     private const float PreviewSize = 520f;
    //
    //     public override void OnInspectorGUI()
    //     {
    //         DrawDefaultInspector();
    //
    //         EditorGUILayout.Space();
    //         EditorGUILayout.LabelField("🎯 Interactive Slot Position Tool", EditorStyles.boldLabel);
    //         EditorGUILayout.HelpBox(
    //             "1. Assign a Hull Image (Sprite).\n" +
    //             "2. Click on the preview below to move the LAST slot.\n" +
    //             "Green dots = current slots", 
    //             MessageType.Info);
    //
    //         HullData hull = (HullData)target;
    //
    //         if (hull.hullImage == null)
    //         {
    //             EditorGUILayout.HelpBox("Assign a Hull Image to enable the preview.", MessageType.Warning);
    //             return;
    //         }
    //
    //         EditorGUILayout.Space();
    //
    //         _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Height(PreviewSize + 60));
    //
    //         Rect previewArea = GUILayoutUtility.GetRect(PreviewSize, PreviewSize);
    //         _previewRect = previewArea;
    //
    //         // FIXED: Use .texture instead of the Sprite directly
    //         if (hull.hullImage.texture != null)
    //         {
    //             EditorGUI.DrawPreviewTexture(previewArea, hull.hullImage.texture, null, ScaleMode.ScaleToFit);
    //         }
    //         else
    //         {
    //             EditorGUI.DrawRect(previewArea, new Color(0.25f, 0.25f, 0.25f));
    //             EditorGUI.LabelField(previewArea, "Sprite has no texture", EditorStyles.centeredGreyMiniLabel);
    //         }
    //
    //         // Draw slot markers
    //         if (hull.moduleSlots != null)
    //         {
    //             for (int i = 0; i < hull.moduleSlots.Length; i++)
    //             {
    //                 var slot = hull.moduleSlots[i];
    //                 if (slot == null) continue;
    //
    //                 Vector2 screenPos = new Vector2(
    //                     previewArea.x + slot.positionPercent.x * previewArea.width,
    //                     previewArea.y + slot.positionPercent.y * previewArea.height
    //                 );
    //
    //                 Handles.color = Color.green;
    //                 Handles.DrawSolidDisc(screenPos, Vector3.forward, 10f);
    //
    //                 GUI.color = Color.yellow;
    //                 GUI.Label(new Rect(screenPos.x + 15, screenPos.y - 12, 200, 20), 
    //                          $"{i}: {slot.slotId}");
    //             }
    //         }
    //
    //         EditorGUILayout.EndScrollView();
    //
    //         // Click handling
    //         Event e = Event.current;
    //         if (e.type == EventType.MouseDown && e.button == 0)
    //         {
    //             if (_previewRect.Contains(e.mousePosition))
    //             {
    //                 float xPercent = (e.mousePosition.x - _previewRect.x) / _previewRect.width;
    //                 float yPercent = (e.mousePosition.y - _previewRect.y) / _previewRect.height;
    //
    //                 xPercent = Mathf.Clamp01(xPercent);
    //                 yPercent = Mathf.Clamp01(yPercent);
    //
    //                 if (hull.moduleSlots != null && hull.moduleSlots.Length > 0)
    //                 {
    //                     int lastIndex = hull.moduleSlots.Length - 1;
    //                     if (hull.moduleSlots[lastIndex] == null)
    //                         hull.moduleSlots[lastIndex] = new ModuleSlot();
    //
    //                     hull.moduleSlots[lastIndex].positionPercent = new Vector2(xPercent, yPercent);
    //
    //                     EditorUtility.SetDirty(hull);
    //                 }
    //
    //                 e.Use();
    //             }
    //         }
    //
    //         if (GUI.changed)
    //             EditorUtility.SetDirty(hull);
    //     }
    // }
}