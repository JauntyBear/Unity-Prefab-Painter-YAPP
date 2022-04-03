using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rowlan.Yapp
{
    public class UnityTerrainTreesIntegration
    {
        PrefabPainterEditor editor;

        public UnityTerrainTreesIntegration(PrefabPainterEditor editor)
        {
            this.editor = editor;
        }

        public void OnInspectorGUI()
        {
            GUILayout.BeginVertical("box");
            {
                EditorGUILayout.LabelField("Terrain Trees", GUIStyles.BoxTitleStyle);

                EditorGUILayout.HelpBox("Terrain Trees is experimental and not fully implemented yet!", MessageType.Warning);

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
            Debug.LogError("Not implemented");
        }

        public void RemovePrefabs( RaycastHit raycastHit)
        {
            Debug.LogError("Not implemented");

        }
    }
}