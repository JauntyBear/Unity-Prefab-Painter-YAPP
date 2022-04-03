using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rowlan.Yapp
{
    public class TerrainDetailsIntegration
    {
        PrefabPainterEditor editor;

        public TerrainDetailsIntegration(PrefabPainterEditor editor)
        {
            this.editor = editor;
        }

        public void OnInspectorGUI()
        {
            GUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("Terrain Details", GUIStyles.BoxTitleStyle);

                EditorGUILayout.HelpBox("Terrain Details is experimental and not fully implemented yet!", MessageType.Warning);

                if (GUILayout.Button("Extract Prefabs", GUILayout.Width(100)))
                {
                    ExtractPrefabs();
                }
            }
            GUILayout.EndVertical();
        }

        private void ExtractPrefabs()
        {
            Terrain terrain = Terrain.activeTerrain;

            if( terrain == null|| terrain.terrainData == null)
            {
                Debug.Log("No Terrain");
            }

            TreePrototype[] trees = terrain.terrainData.treePrototypes;
            foreach(TreePrototype pt in trees)
            {
                Debug.Log("pt: " + pt.prefab);
            }
        }

        public void AddNewPrefab(PrefabSettings prefabSettings, Vector3 newPosition, Quaternion newRotation, Vector3 newLocalScale)
        {


            Debug.Log("Spawn to Terrain Details");

#if VEGETATION_STUDIO_PRO

                // ensure the prefab has a VegetationItemID
                updateVSProSettings( prefabSettings, true);

                if( !string.IsNullOrEmpty( prefabSettings.vspro_VegetationItemID))
                {
                    string vegetationItemID = prefabSettings.vspro_VegetationItemID;
                    Vector3 worldPosition = newPosition;
                    Vector3 scale = newLocalScale; // TODO local or world?
                    Quaternion rotation = newRotation;
                    bool applyMeshRotation = true; // TODO ???
                    float distanceFalloff = 1f; // TODO ???
                    bool clearCellCache = true; // TODO ???

                    byte vegetationSourceID = Constants.VegetationStudioPro_SourceId;

                    VegetationStudioManager.AddVegetationItemInstance(vegetationItemID, worldPosition, scale, rotation, applyMeshRotation, vegetationSourceID, distanceFalloff, clearCellCache);

                }
#endif

        }

        /// <summary>
        /// Ensure the prefab has a VegetationItemID
        /// </summary>
        /// <param name="prefabSettings"></param>
        private void updateVSProSettings(PrefabSettings prefabSettings, bool forceVegetationItemIDUpdate)
        {
#if VEGETATION_STUDIO_PRO

            GameObject prefab = prefabSettings.prefab;

            // check if we have a VegetationItemID, otherwise create it using the current prefab
            if (string.IsNullOrEmpty(prefabSettings.vspro_VegetationItemID) || forceVegetationItemIDUpdate)
            {
                // get the asset guid
                if (string.IsNullOrEmpty(prefabSettings.assetGUID))
                {
                    string assetPath = AssetDatabase.GetAssetPath(prefab);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        string assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
                        prefabSettings.assetGUID = assetGUID;
                    }
                }

                // if we have a guid, get the vs pro id
                if (!string.IsNullOrEmpty(prefabSettings.assetGUID))
                {
                    // get the VegetationItemID
                    prefabSettings.vspro_VegetationItemID = VegetationStudioManager.GetVegetationItemID(prefabSettings.assetGUID);

                    // if the vegetation item id doesn't exist, create a new vegetation item
                    if (string.IsNullOrEmpty(prefabSettings.vspro_VegetationItemID))
                    {
                        VegetationType vegetationType = VegetationType.Objects;
                        bool enableRuntimeSpawn = false; // no runtime spawn, we want it spawned from persistent storage
                        BiomeType biomeType = BiomeType.Default;

                        prefabSettings.vspro_VegetationItemID = VegetationStudioManager.AddVegetationItem(prefab, vegetationType, enableRuntimeSpawn, biomeType);
                    }

                }
                else
                {
                    Debug.LogError("Can't get assetGUID for prefab " + prefab);
                }
            }

            if (string.IsNullOrEmpty(prefabSettings.vspro_VegetationItemID))
            {
                Debug.LogError("Can't get VegetationItemId for prefab " + prefab);
            }
#endif
        }
    }
}