using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using static Yapp.BrushComponent;
#if VEGETATION_STUDIO_PRO
using AwesomeTechnologies.Vegetation.PersistentStorage;
using AwesomeTechnologies.VegetationSystem;
using AwesomeTechnologies.VegetationStudio;
#endif

namespace Yapp
{
    public class BrushModuleEditor: ModuleEditorI
    {
        #region Properties

        SerializedProperty brushSize;
        SerializedProperty brushRotation;
        SerializedProperty allowOverlap;
        SerializedProperty alignToTerrain;
        SerializedProperty distribution;
        SerializedProperty poissonDiscSize;
        SerializedProperty fallOffCurve;
        SerializedProperty fallOff2dCurveX;
        SerializedProperty fallOff2dCurveZ;
        SerializedProperty curveSamplePoints;
        SerializedProperty spawnToVSPro;

        SerializedProperty autoSimulationType;
        SerializedProperty autoSimulationHeightOffset;
        SerializedProperty autoSimulationStepCountMax;
        SerializedProperty autoSimulationStepIterations;


        #endregion Properties

#pragma warning disable 0414
        PrefabPainterEditor editor;
        #pragma warning restore 0414
         
        PrefabPainter gizmo;

        BrushComponent brushComponent = new BrushComponent();

        /// <summary>
        /// Auto physics only on special condition:
        /// + prefabs were added
        /// + mouse got released
        /// </summary>
        private bool needsPhysicsApplied = false;




        public BrushModuleEditor(PrefabPainterEditor editor)
        {
            this.editor = editor;
            this.gizmo = editor.GetPainter();

            brushSize = editor.FindProperty( x => x.brushSettings.brushSize);
            brushRotation = editor.FindProperty(x => x.brushSettings.brushRotation);

            alignToTerrain = editor.FindProperty(x => x.brushSettings.alignToTerrain);
            distribution = editor.FindProperty(x => x.brushSettings.distribution);
            poissonDiscSize = editor.FindProperty(x => x.brushSettings.poissonDiscSize);
            fallOffCurve = editor.FindProperty(x => x.brushSettings.fallOffCurve);
            fallOff2dCurveX = editor.FindProperty(x => x.brushSettings.fallOff2dCurveX);
            fallOff2dCurveZ = editor.FindProperty(x => x.brushSettings.fallOff2dCurveZ);
            curveSamplePoints = editor.FindProperty(x => x.brushSettings.curveSamplePoints);
            allowOverlap = editor.FindProperty(x => x.brushSettings.allowOverlap);

            autoSimulationType = editor.FindProperty(x => x.brushSettings.autoSimulationType);
            autoSimulationHeightOffset = editor.FindProperty(x => x.brushSettings.autoSimulationHeightOffset);
            autoSimulationStepCountMax = editor.FindProperty(x => x.brushSettings.autoSimulationStepCountMax);
            autoSimulationStepIterations = editor.FindProperty(x => x.brushSettings.autoSimulationStepIterations);

            spawnToVSPro = editor.FindProperty(x => x.brushSettings.spawnToVSPro);

        }

        public void OnInspectorGUI()
        {
            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Brush settings", GUIStyles.BoxTitleStyle);

            EditorGUILayout.PropertyField(brushSize, new GUIContent("Brush Size"));
            EditorGUILayout.PropertyField(brushRotation, new GUIContent("Brush Rotation"));

            EditorGUILayout.PropertyField(alignToTerrain, new GUIContent("Align To Terrain"));
            EditorGUILayout.PropertyField(allowOverlap, new GUIContent("Allow Overlap", "Center Mode: Check against brush size.\nPoisson Mode: Check against Poisson Disc size"));

            EditorGUILayout.PropertyField(distribution, new GUIContent("Distribution"));

            switch (gizmo.brushSettings.distribution)
            {
                case BrushSettings.Distribution.Center:
                    break;
                case BrushSettings.Distribution.Poisson:
                    //EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(poissonDiscSize, new GUIContent("Poisson Disc Size"));
                    //EditorGUI.indentLevel--;
                    break;
                case BrushSettings.Distribution.FallOff:
                    EditorGUILayout.PropertyField(curveSamplePoints, new GUIContent("Curve Sample Points"));
                    EditorGUILayout.PropertyField(fallOffCurve, new GUIContent("FallOff"));
                    break;
                case BrushSettings.Distribution.FallOff2d:
                    EditorGUILayout.PropertyField(curveSamplePoints, new GUIContent("Curve Sample Points"));
                    EditorGUILayout.PropertyField(fallOff2dCurveX, new GUIContent("FallOff X"));
                    EditorGUILayout.PropertyField(fallOff2dCurveZ, new GUIContent("FallOff Z"));
                    break;
            }

            // TODO: how to create a minmaxslider with propertyfield?
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Slope");
            EditorGUILayout.MinMaxSlider(ref gizmo.brushSettings.slopeMin, ref gizmo.brushSettings.slopeMax, gizmo.brushSettings.slopeMinLimit, gizmo.brushSettings.slopeMaxLimit);
            EditorGUILayout.EndHorizontal();

            // auto physics
            EditorGUILayout.PropertyField(autoSimulationType, new GUIContent("Physics Simulation"));
            if (autoSimulationType.enumValueIndex != (int) BrushSettings.AutoSimulationType.None)
            {
                EditorGUI.indentLevel++;
                {
                    EditorGUILayout.PropertyField(autoSimulationHeightOffset, new GUIContent("Height Offset"));
                    EditorGUILayout.PropertyField(autoSimulationStepCountMax, new GUIContent("Step Count Max", "Maximum number of simulation steps to perform"));
                    EditorGUILayout.PropertyField(autoSimulationStepIterations, new GUIContent("Step Iterations", "Number of physics steps to perform in a single simulation step. lower = smoother, higher = faster"));
                }
                EditorGUI.indentLevel--;
            }

            // vegetation studio pro
#if VEGETATION_STUDIO_PRO
                EditorGUILayout.PropertyField(spawnToVSPro, new GUIContent("Spawn to VS Pro"));
#endif

            // consistency check
            float minDiscSize = 0.01f;
            if( poissonDiscSize.floatValue < minDiscSize)
            {
                Debug.LogError("Poisson Disc Size is too small. Setting it to " + minDiscSize);
                poissonDiscSize.floatValue = minDiscSize;
            }

            GUILayout.EndVertical();

        }



        public void OnSceneGUI()
        {

            // paint prefabs on mouse drag. don't do anything if no mode is selected, otherwise e.g. movement in scene view wouldn't work with alt key pressed
            if ( brushComponent.DrawBrush(gizmo.brushSettings, out BrushMode brushMode, out RaycastHit raycastHit))
            {
                switch( brushMode)
                {
                    case BrushMode.ShiftDrag:

                        AddPrefabs(raycastHit);

                        needsPhysicsApplied = true;

                        // consume event
                        Event.current.Use();
                        break;

                    case BrushMode.ShiftCtrlDrag:

                        RemovePrefabs(raycastHit);

                        // consume event
                        Event.current.Use();
                        break;

                }
            }

            // info for the scene gui; used to be dynamic and showing number of prefabs (currently is static until refactoring is done)
            string[] guiInfo = new string[] { "Add prefabs: shift + drag mouse\nRemove prefabs: shift + ctrl + drag mouse\nBrush size: ctrl + mousewheel, Brush rotation: ctrl + shift + mousewheel" };
            brushComponent.Layout(guiInfo);

            // auto physics
            bool applyAutoPhysics = needsPhysicsApplied && gizmo.brushSettings.autoSimulationType != BrushSettings.AutoSimulationType.None && Event.current.type == EventType.MouseUp;
            if (applyAutoPhysics)
            {
                ApplyPhysics();
            }

        }



        #region Paint Prefabs

        private void AddPrefabs(RaycastHit hit)
        {
            if (!editor.IsEditorSettingsValid())
                return;

            switch (gizmo.brushSettings.distribution)
            {
                case BrushSettings.Distribution.Center:
                    AddPrefabs_Center(hit.point, hit.normal);
                    break;
                case BrushSettings.Distribution.Poisson:
                    AddPrefabs_Poisson(hit.point, hit.normal);
                    break;
                case BrushSettings.Distribution.FallOff:
                    Debug.Log("Not implemented yet: " + gizmo.brushSettings.distribution);
                    break;
                case BrushSettings.Distribution.FallOff2d:
                    Debug.Log("Not implemented yet: " + gizmo.brushSettings.distribution);
                    break;
            }

        }

        /// <summary>
        /// Add prefabs, mode Center
        /// </summary>
        private void AddPrefabs_Center( Vector3 position, Vector3 normal)
        {

            // check if a gameobject is already within the brush size
            // allow only 1 instance per bush size
            GameObject container = gizmo.container as GameObject;


            // check if a prefab already exists within the brush
            bool prefabExists = false;

            // check overlap
            if (!gizmo.brushSettings.allowOverlap)
            {
                float brushRadius = gizmo.brushSettings.brushSize / 2f;

                foreach (Transform child in container.transform)
                {
                    float dist = Vector3.Distance(position, child.transform.position);

                    // check against the brush
                    if (dist <= brushRadius)
                    {
                        prefabExists = true;
                        break;
                    }

                }
            }

            if (!prefabExists)
            {
                AddNewPrefab(position, normal);
            }
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

        private void AddNewPrefab( Vector3 position, Vector3 normal)
        {

            GameObject container = gizmo.container as GameObject;

            PrefabSettings prefabSettings = this.gizmo.GetPrefabSettings();

            GameObject prefab = prefabSettings.prefab;

            ///
            /// Calculate position / rotation / scale
            /// 

            // get new position
            Vector3 newPosition = position;

            // add offset
            newPosition += prefabSettings.positionOffset;

            // auto physics height offset
            newPosition = ApplyAutoPhysicsHeightOffset(newPosition);

            Vector3 newLocalScale = prefabSettings.prefab.transform.localScale;

            // size
            if (prefabSettings.changeScale)
            {
                newLocalScale = Vector3.one * Random.Range(prefabSettings.scaleMin, prefabSettings.scaleMax);
            }

            // rotation
            Quaternion newRotation;
            if (prefabSettings.randomRotation)
            {
                
                float rotationX = Random.Range( prefabSettings.rotationMinX, prefabSettings.rotationMaxX);
                float rotationY = Random.Range( prefabSettings.rotationMinY, prefabSettings.rotationMaxY);
                float rotationZ = Random.Range( prefabSettings.rotationMinZ, prefabSettings.rotationMaxZ);

                newRotation = Quaternion.Euler( rotationX, rotationY, rotationZ);

            }
            else if (this.gizmo.brushSettings.alignToTerrain)
            {
                newRotation = Quaternion.FromToRotation(Vector3.up, normal);
            }
            else
            {
                newRotation = Quaternion.Euler(prefabSettings.rotationOffset);
                //rotation = Quaternion.identity;
            }

            ///
            /// create instance and apply position / rotation / scale
            /// 

            // spawn item to vs pro
            if ( gizmo.brushSettings.spawnToVSPro)
            {
#if VEGETATION_STUDIO_PRO

                // ensure the prefab has a VegetationItemID
                updateVSProSettings( prefabSettings, true);

                if( !string.IsNullOrEmpty( prefabSettings.vspro_VegetationItemID))
                {
                    string vegetationItemID = prefabSettings.vspro_VegetationItemID;
                    Vector3 worldPosition = position;
                    Vector3 scale = newLocalScale; // TODO local or world?
                    Quaternion rotation = newRotation;
                    bool applyMeshRotation = true; // TODO ???
                    byte vegetationSourceID = 5; // TODO see PersistentVegetationStorageTools for constants. 5 = "Vegetation Studio - Painted"
                    float distanceFalloff = 1f; // TODO ???
                    bool clearCellCache = true; // TODO ???

                    VegetationStudioManager.AddVegetationItemInstance(vegetationItemID, worldPosition, scale, rotation, applyMeshRotation, vegetationSourceID, distanceFalloff, clearCellCache);

                }
#endif
            }
            // spawn item to scene
            else
            {

                // new prefab
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

                instance.transform.position = newPosition;
                instance.transform.rotation = newRotation;
                instance.transform.localScale = newLocalScale;

                // attach as child of container
                instance.transform.parent = container.transform;

                Undo.RegisterCreatedObjectUndo(instance, "Instantiate Prefab");

            }

        }

        /// <summary>
        /// Add prefabs, mode Center
        /// </summary>
        private void AddPrefabs_Poisson(Vector3 position, Vector3 normal)
        {
            GameObject container = gizmo.container as GameObject;

            float brushSize = gizmo.brushSettings.brushSize;
            float brushRadius = brushSize / 2.0f;
            float discRadius = gizmo.brushSettings.poissonDiscSize / 2;

            PoissonDiscSampler sampler = new PoissonDiscSampler(brushSize, brushSize, discRadius);

            foreach (Vector2 sample in sampler.Samples()) {

                // brush is currenlty a disc => ensure the samples are within the disc
                if (Vector2.Distance(sample, new Vector2(brushRadius, brushRadius)) > brushRadius)
                    continue;

                // x/z come from the poisson sample 
                float x = position.x + sample.x - brushRadius;
                float z = position.z + sample.y - brushRadius;

                // y depends on the terrain height
                Vector3 terrainPosition = new Vector3(x, position.y, z);

                // get terrain y position
                float y = Terrain.activeTerrain.SampleHeight(terrainPosition);

                // create position vector
                Vector3 prefabPosition = new Vector3( x, y, z);

                // auto physics height offset
                prefabPosition = ApplyAutoPhysicsHeightOffset(prefabPosition);

                // check if a prefab already exists within the brush
                bool prefabExists = false;

                // check overlap
                if (!gizmo.brushSettings.allowOverlap)
                {
                    foreach (Transform child in container.transform)
                    {
                        float dist = Vector3.Distance(prefabPosition, child.transform.position);

                        // check against a single poisson disc
                        if (dist <= discRadius)
                        {
                            prefabExists = true;
                            break;
                        }

                    }
                }

                // add prefab
                if( !prefabExists)
                {
                    AddNewPrefab(prefabPosition, normal);
                }
                

            }

           
        }

        /// <summary>
        /// Add additional height offset if auto physics is enabled
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private Vector3 ApplyAutoPhysicsHeightOffset( Vector3 position)
        {
            if (gizmo.brushSettings.autoSimulationType == BrushSettings.AutoSimulationType.None)
                return position;

            // auto physics: add additional height offset
            position.y += gizmo.brushSettings.autoSimulationHeightOffset;

            return position;
        }

        /// <summary>
        /// Remove prefabs
        /// </summary>
        private void RemovePrefabs( RaycastHit raycastHit)
        {

            if (!editor.IsEditorSettingsValid())
                return;

            Vector3 position = raycastHit.point;

            // check if a gameobject of the container is within the brush size and remove it
            GameObject container = gizmo.container as GameObject;

            float radius = gizmo.brushSettings.brushSize / 2f;

            List<Transform> removeList = new List<Transform>();

            foreach (Transform transform in container.transform)
            {
                float dist = Vector3.Distance(position, transform.transform.position);

                if (dist <= radius)
                {
                    removeList.Add(transform);
                }

            }

            // remove gameobjects
            foreach( Transform transform in removeList)
            {
                PrefabPainter.DestroyImmediate(transform.gameObject);
            }
           
        }

        #endregion Paint Prefabs

        #region Physics
        private void ApplyPhysics()
        {
            PhysicsSimulation physicsSimulation = ScriptableObject.CreateInstance<PhysicsSimulation>();

            PhysicsSettings physicsSettings = new PhysicsSettings();
            physicsSettings.simulationStepCountMax = gizmo.brushSettings.autoSimulationStepCountMax;
            physicsSettings.simulationStepIterations = gizmo.brushSettings.autoSimulationStepIterations;

            physicsSimulation.ApplySettings(physicsSettings);

            // TODO: use only the new added ones?
            Transform[] containerChildren = PrefabUtils.GetContainerChildren(gizmo.container);

            if( gizmo.brushSettings.autoSimulationType == BrushSettings.AutoSimulationType.Once)
            {
                physicsSimulation.RunSimulationOnce(containerChildren);
            }
            else if (gizmo.brushSettings.autoSimulationType == BrushSettings.AutoSimulationType.Continuous)
            {
                physicsSimulation.StartSimulation(containerChildren);
            }

        }
        #endregion Physics
    }

}
