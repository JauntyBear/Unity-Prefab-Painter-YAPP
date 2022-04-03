using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Rowlan.Yapp
{
    public class UnityTerrainTreesIntegration
    {
        // unfiltered
        private const int PROTOTYPE_FILTER_DEFAULT = -1;

        // internal properties, maybe we'll make them public
        private bool randomTreeColor = false;
        private float treeColorAdjustment = 0.8f;

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

                EditorGUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("Log Instances", GUILayout.Width(100)))
                    {
                        ExtractPrefabs();
                    }

                    if (GUILayout.Button("Remove All", GUILayout.Width(100)))
                    {
                        RemoveAll();
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }

        private void ExtractPrefabs()
        {
            TerrainData terrainData = GetTerrainData();

            if (terrainData == null)
                return;

            TreePrototype[] trees = terrainData.treePrototypes;

            foreach(TreePrototype prototype in trees)
            {
                Debug.Log("prototype: " + prototype.prefab);
            }
        }

        private Terrain GetTerrain()
        {
            return Terrain.activeTerrain; // TODO: multi terrain
        }

        private TerrainData GetTerrainData()
        {
            Terrain terrain = GetTerrain();

            if (terrain == null)
            {
                Debug.LogError("Terrain not found");

                return null;
            }

            return terrain.terrainData;
        }

        private void RemoveAll()
        {
            TerrainData terrainData = GetTerrainData();

            if (terrainData == null)
                return;

            UnityTerrainUtils.RemoveAllTreeInstances(terrainData);
        }

        public void AddNewPrefab(PrefabSettings prefabSettings, Vector3 newPosition, Quaternion newRotation, Vector3 newLocalScale)
        {
            Terrain terrain = GetTerrain();

            if (terrain == null)
                return;

            // brush mode
            float brushSize = editor.GetPainter().brushSettings.brushSize;

            // poisson mode: use the discs as brush size
            if (editor.GetPainter().brushSettings.distribution == BrushSettings.Distribution.Poisson_Any || editor.GetPainter().brushSettings.distribution == BrushSettings.Distribution.Poisson_Terrain)
            {
                brushSize = editor.GetPainter().brushSettings.poissonDiscSize;
            }

            GameObject prefab = prefabSettings.prefab;

            UnityTerrainUtils.PlaceTree(terrain, prefab, newPosition, newLocalScale, newRotation, brushSize, randomTreeColor, treeColorAdjustment);
        }

        public void RemovePrefabs( RaycastHit raycastHit)
        {
            Terrain terrain = GetTerrain();

            if (terrain == null)
                return;

            Vector3 position = raycastHit.point;
            float brushSize = editor.GetPainter().brushSettings.brushSize;

            UnityTerrainUtils.RemoveOverlapping(terrain, position, brushSize);

        }
    }
}