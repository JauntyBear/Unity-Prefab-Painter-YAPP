using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Yapp
{
    public class PrefabModuleEditor : ModuleEditorI
    {
        #pragma warning disable 0414
        PrefabPainterEditor editor;
        PrefabPainter gizmo;
        #pragma warning restore 0414


        struct PrefabPreset
        {
            public enum Type
            {
                Default, Object, Plant, Rock, House, Fence
            }

            public Type TypeId { get; set; }
            public string Name { get; set; }
            public bool RandomRotation { get; set; }
        }

        static PrefabPreset presetDefault = new PrefabPreset() { TypeId = PrefabPreset.Type.Default, Name = "", RandomRotation = false };
        static PrefabPreset presetObject = new PrefabPreset() { TypeId = PrefabPreset.Type.Object, Name = "Object", RandomRotation = false };
        static PrefabPreset presetPlant = new PrefabPreset() { TypeId = PrefabPreset.Type.Plant, Name = "Plant", RandomRotation = false };
        static PrefabPreset presetRock = new PrefabPreset() { TypeId = PrefabPreset.Type.Rock, Name = "Rock", RandomRotation = true };
        static PrefabPreset presetHouse = new PrefabPreset() { TypeId = PrefabPreset.Type.House, Name = "House", RandomRotation = false };
        static PrefabPreset presetFence = new PrefabPreset() { TypeId = PrefabPreset.Type.Fence, Name = "Fence", RandomRotation = false };

        // register the available prefab presets
        List<PrefabPreset> prefabPresets = new List<PrefabPreset>()
        {
            presetPlant,
            presetRock,
            presetFence,
            presetHouse,
            presetObject,
            // presetDefault, // default isn't registered, it's just used as a filler
        };

        // number of prefab preset drop targets per row in the preset grid
        private int prefabPresetGridColumnCount = 4;

        public PrefabModuleEditor(PrefabPainterEditor editor)
        {
            this.editor = editor;
            this.gizmo = editor.GetPainter();
        }

        public void OnInspectorGUI()
        {

            GUILayout.BeginVertical("box");
            {

                EditorGUILayout.LabelField("Prefabs", GUIStyles.BoxTitleStyle);

                #region preset drop targets
                GUILayout.BeginHorizontal();
                {
                    GUILayout.BeginVertical();
                    {
                        // change background color in case there are no prefabs yet
                        if (gizmo.prefabSettingsList.Count == 0)
                        {
                            EditorGUILayout.HelpBox("Drop prefabs on the prefab type boxes in order to use them.", MessageType.Info);

                            editor.SetErrorBackgroundColor();
                        }

                        int gridRows = Mathf.CeilToInt( (float) prefabPresets.Count / prefabPresetGridColumnCount);

                        for (int row = 0; row < gridRows; row++)
                        {
                            GUILayout.BeginHorizontal();
                            {
                                for (int column = 0; column < prefabPresetGridColumnCount; column++)
                                {
                                    int index = column + row * prefabPresetGridColumnCount;

                                    PrefabPreset preset = index < prefabPresets.Count ? preset = prefabPresets[index] : presetDefault;
                                    string name = preset.Name;

                                    // drop area
                                    Rect prefabDropArea = GUILayoutUtility.GetRect(0.0f, 34.0f, GUIStyles.DropAreaStyle , GUILayout.ExpandWidth(true));

                                    // drop area box with background color and info text
                                    GUI.color = GUIStyles.DropAreaBackgroundColor;
                                    GUI.Box(prefabDropArea, name, GUIStyles.DropAreaStyle);
                                    GUI.color = GUIStyles.DefaultBackgroundColor;

                                    Event evt = Event.current;
                                    switch (evt.type)
                                    {
                                        case EventType.DragUpdated:
                                        case EventType.DragPerform:

                                            if (prefabDropArea.Contains(evt.mousePosition))
                                            {

                                                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                                                if (evt.type == EventType.DragPerform)
                                                {
                                                    DragAndDrop.AcceptDrag();

                                                    // list of new prefabs that should be created via drag/drop
                                                    // we can't do it in the drag/drop code itself, we'd get exceptions like
                                                    //   ArgumentException: Getting control 12's position in a group with only 12 controls when doing dragPerform. Aborting
                                                    // followed by
                                                    //   Unexpected top level layout group! Missing GUILayout.EndScrollView/EndVertical/EndHorizontal? UnityEngine.GUIUtility:ProcessEvent(Int32, IntPtr)
                                                    // they must be added when everything is done (currently at the end of this method)
                                                    editor.newDraggedPrefabs = new List<PrefabSettings>();

                                                    foreach (Object droppedObject in DragAndDrop.objectReferences)
                                                    {

                                                        // allow only prefabs
                                                        if (PrefabUtility.GetPrefabAssetType(droppedObject) == PrefabAssetType.NotAPrefab)
                                                        {
                                                            Debug.Log("Not a prefab: " + droppedObject);
                                                            continue;
                                                        }

                                                        // add the prefab to the list using the preset
                                                        AddPrefab(droppedObject as GameObject, preset);

                                                    }
                                                }
                                            }
                                            break;
                                    }
                                }
                            }
                            GUILayout.EndHorizontal();
                        }

                        editor.SetDefaultBackgroundColor();

                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();

                #endregion preset drop targets

                EditorGUILayout.Space();

                for (int i = 0; i < gizmo.prefabSettingsList.Count; i++)
                {
                    if (i > 0)
                        editor.addGUISeparator();

                    PrefabSettings prefabSettings = this.gizmo.prefabSettingsList[i];

                    GUILayout.BeginHorizontal();
                    {
                        // preview
                        // try to get the asset preview
                        Texture2D previewTexture = AssetPreview.GetAssetPreview(prefabSettings.prefab);
                        // if no asset preview available, try to get the mini thumbnail
                        if (!previewTexture)
                        {
                            previewTexture = AssetPreview.GetMiniThumbnail(prefabSettings.prefab);
                        }
                        // if a preview is available, paint it
                        if (previewTexture)
                        {
                            //GUILayout.Label(previewTexture, EditorStyles.objectFieldThumb, GUILayout.Width(50), GUILayout.Height(50)); // without border, but with size
                            GUILayout.Label(previewTexture, GUILayout.Width(50), GUILayout.Height(50)); // without border, but with size

                            //GUILayout.Box(previewTexture); // with border
                            //GUILayout.Label(previewTexture); // no border
                            //GUILayout.Box(previewTexture, GUILayout.Width(50), GUILayout.Height(50)); // with border and size
                            //EditorGUI.DrawPreviewTexture(new Rect(25, 60, 100, 100), previewTexture); // draws it in absolute coordinates

                        }

                        // right alin the buttons
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button("Add", EditorStyles.miniButton))
                        {
                            this.gizmo.prefabSettingsList.Insert(i + 1, new PrefabSettings());
                        }
                        if (GUILayout.Button("Duplicate", EditorStyles.miniButton))
                        {
                            PrefabSettings newPrefabSettings = prefabSettings.Clone();
                            this.gizmo.prefabSettingsList.Insert(i + 1, newPrefabSettings);
                        }
                        if (GUILayout.Button("Reset", EditorStyles.miniButton))
                        {
                            // remove existing
                            this.gizmo.prefabSettingsList.RemoveAt(i);

                            // add new
                            this.gizmo.prefabSettingsList.Insert(i, new PrefabSettings());

                        }
                        if (GUILayout.Button("Remove", EditorStyles.miniButton))
                        {
                            this.gizmo.prefabSettingsList.Remove(prefabSettings);
                        }

                    }
                    GUILayout.EndHorizontal();

                    GUILayout.Space(4);

                    prefabSettings.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", prefabSettings.prefab, typeof(GameObject), true);

                    prefabSettings.active = EditorGUILayout.Toggle("Active", prefabSettings.active);
                    prefabSettings.probability = EditorGUILayout.Slider("Probability", prefabSettings.probability, 0, 1);

                    // scale
                    prefabSettings.changeScale = EditorGUILayout.Toggle("Change Scale", prefabSettings.changeScale);

                    if (prefabSettings.changeScale)
                    {
                        prefabSettings.scaleMin = EditorGUILayout.FloatField("Scale Min", prefabSettings.scaleMin);
                        prefabSettings.scaleMax = EditorGUILayout.FloatField("Scale Max", prefabSettings.scaleMax);
                    }

                    // position
                    prefabSettings.positionOffset = EditorGUILayout.Vector3Field("Position Offset", prefabSettings.positionOffset);
                    
                    // rotation
                    prefabSettings.rotationOffset = EditorGUILayout.Vector3Field("Rotation Offset", prefabSettings.rotationOffset);
                    prefabSettings.randomRotation = EditorGUILayout.Toggle("Random Rotation", prefabSettings.randomRotation);

                    // rotation limits
                    if (prefabSettings.randomRotation)
                    {
                        EditorGuiUtilities.MinMaxEditor("  Rotation Limit X", ref prefabSettings.rotationMinX, ref prefabSettings.rotationMaxX, 0, 360);
                        EditorGuiUtilities.MinMaxEditor("  Rotation Limit Y", ref prefabSettings.rotationMinY, ref prefabSettings.rotationMaxY, 0, 360);
                        EditorGuiUtilities.MinMaxEditor("  Rotation Limit Z", ref prefabSettings.rotationMinZ, ref prefabSettings.rotationMaxZ, 0, 360);
                    }

                    // VS Pro Id
#if VEGETATION_STUDIO_PRO
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUILayout.TextField("Asset GUID", prefabSettings.assetGUID);
                    EditorGUILayout.TextField("VSPro Id", prefabSettings.vspro_VegetationItemID);
                    EditorGUI.EndDisabledGroup();
#endif
                }
            }

            GUILayout.EndVertical();

        }

        public void OnSceneGUI()
        {
        }

        private void AddPrefab(GameObject prefab, PrefabPreset preset)
        {
            // new settings
            PrefabSettings prefabSettings = new PrefabSettings();

            // apply preset to prefab settings
            // TODO: might be better in the constructor, be it might be necessary to do some aftermath in the editor, so leaving it at this
            ApplyPreset(prefabSettings, preset);

            // initialize with dropped prefab
            prefabSettings.prefab = prefab;

            editor.newDraggedPrefabs.Add(prefabSettings);

        }

        private void ApplyPreset(PrefabSettings prefabSettings, PrefabPreset preset)
        {
            prefabSettings.randomRotation = preset.RandomRotation;
        }
    }
}
