using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using static Rowlan.Yapp.BrushComponent;


namespace Rowlan.Yapp
{
    public class BrushModuleEditor: ModuleEditorI
    {
        #region Properties

        SerializedProperty brushSize;
        SerializedProperty brushRotation;
        SerializedProperty sizeGuide;
        SerializedProperty normalGuide;
        SerializedProperty rotationGuide;
        SerializedProperty allowOverlap;
        SerializedProperty alignToTerrain;
        SerializedProperty layerMask;
        SerializedProperty distribution;
        SerializedProperty poissonDiscSize;
        SerializedProperty poissonDiscRaycastOffset;
        SerializedProperty fallOffCurve;
        SerializedProperty fallOff2dCurveX;
        SerializedProperty fallOff2dCurveZ;
        SerializedProperty curveSamplePoints;

        #endregion Properties

        #region Integration to external applications
        VegetationStudioProIntegration vegetationStudioProIntegration;
        #endregion Integration to external applications

        #pragma warning disable 0414
        PrefabPainterEditor editor;
        #pragma warning restore 0414
         
        PrefabPainter editorTarget;

        BrushComponent brushComponent = new BrushComponent();

        /// <summary>
        /// Auto physics only on special condition:
        /// + prefabs were added
        /// + mouse got released
        /// </summary>
        private bool needsPhysicsApplied = false;

        private List<GameObject> autoPhysicsCollection = new List<GameObject>();

        private BrushDistribution brushDistribution;

        public BrushModuleEditor(PrefabPainterEditor editor)
        {
            this.editor = editor;
            this.editorTarget = editor.GetPainter();

            brushDistribution = new BrushDistribution( this);

            brushSize = editor.FindProperty( x => x.brushSettings.brushSize);
            brushRotation = editor.FindProperty(x => x.brushSettings.brushRotation);

            sizeGuide = editor.FindProperty(x => x.brushSettings.sizeGuide);
            normalGuide = editor.FindProperty(x => x.brushSettings.normalGuide);
            rotationGuide = editor.FindProperty(x => x.brushSettings.rotationGuide);

            alignToTerrain = editor.FindProperty(x => x.brushSettings.alignToTerrain);
            distribution = editor.FindProperty(x => x.brushSettings.distribution);
            poissonDiscSize = editor.FindProperty(x => x.brushSettings.poissonDiscSize);
            poissonDiscRaycastOffset = editor.FindProperty(x => x.brushSettings.poissonDiscRaycastOffset);
            fallOffCurve = editor.FindProperty(x => x.brushSettings.fallOffCurve);
            fallOff2dCurveX = editor.FindProperty(x => x.brushSettings.fallOff2dCurveX);
            fallOff2dCurveZ = editor.FindProperty(x => x.brushSettings.fallOff2dCurveZ);
            curveSamplePoints = editor.FindProperty(x => x.brushSettings.curveSamplePoints);
            allowOverlap = editor.FindProperty(x => x.brushSettings.allowOverlap);
            layerMask = editor.FindProperty(x => x.brushSettings.layerMask);

            // initialize integrated applications
            vegetationStudioProIntegration = new VegetationStudioProIntegration( editor);

        }

        public void OnInspectorGUI()
        {
            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Brush settings", GUIStyles.BoxTitleStyle);

            EditorGUILayout.PropertyField(brushSize, new GUIContent("Brush Size"));
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PropertyField(brushRotation, new GUIContent("Brush Rotation"));
                if (GUILayout.Button("Reset", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                {
                    brushRotation.intValue = 0;
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.PrefixLabel("Brush Visual");

                GUILayout.Label("Size", GUILayout.ExpandWidth(false));
                EditorGUILayout.PropertyField(sizeGuide, GUIContent.none, GUILayout.Width(20));

                GUILayout.Label("Normal", GUILayout.ExpandWidth(false));
                EditorGUILayout.PropertyField(normalGuide, GUIContent.none, GUILayout.Width(20));

                GUILayout.Label("Rotation", GUILayout.ExpandWidth(false));
                EditorGUILayout.PropertyField(rotationGuide, GUIContent.none, GUILayout.Width(20));
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(alignToTerrain, new GUIContent("Align To Terrain"));
            EditorGUILayout.PropertyField(allowOverlap, new GUIContent("Allow Overlap", "Center Mode: Check against brush size.\nPoisson Mode: Check against Poisson Disc size"));
            EditorGUILayout.PropertyField(layerMask, new GUIContent("Layer Mask", "Layer mask for the brush raycast"));

            EditorGUILayout.PropertyField(distribution, new GUIContent("Distribution"));

            switch (editorTarget.brushSettings.distribution)
            {
                case BrushSettings.Distribution.Center:
                    break;
                case BrushSettings.Distribution.ScaleToBrushSize:
                    break;
                case BrushSettings.Distribution.Poisson_Any:
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(poissonDiscSize, new GUIContent("Poisson Disc Size"));
                    EditorGUILayout.PropertyField(poissonDiscRaycastOffset, new GUIContent("Raycast Offset", "If any collider (not only terrain) is used for the raycast, then this will used as offset from which the ray will be cast against the collider"));
                    EditorGUI.indentLevel--;
                    break;
                case BrushSettings.Distribution.Poisson_Terrain:
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(poissonDiscSize, new GUIContent("Poisson Disc Size"));
                    EditorGUI.indentLevel--;
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
            EditorGUILayout.MinMaxSlider(ref editorTarget.brushSettings.slopeMin, ref editorTarget.brushSettings.slopeMax, editorTarget.brushSettings.slopeMinLimit, editorTarget.brushSettings.slopeMaxLimit);
            EditorGUILayout.EndHorizontal();

            // vegetation studio pro
            vegetationStudioProIntegration.OnInspectorGUI();

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
            if ( brushComponent.DrawBrush(editorTarget.brushSettings, out BrushMode brushMode, out RaycastHit raycastHit))
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
            bool applyAutoPhysics = needsPhysicsApplied 
                && autoPhysicsCollection.Count > 0
                && editorTarget.spawnSettings.autoSimulationType != SpawnSettings.AutoSimulationType.None 
                && Event.current.type == EventType.MouseUp;
            if (applyAutoPhysics)
            {
                AutoPhysicsSimulation.ApplyPhysics(editorTarget.physicsSettings, autoPhysicsCollection, editorTarget.spawnSettings.autoSimulationType);
                
                autoPhysicsCollection.Clear();
            }

        }
        


        #region Paint Prefabs

        private void AddPrefabs(RaycastHit hit)
        {
            if (!editor.IsEditorSettingsValid())
                return;

            switch (editorTarget.brushSettings.distribution)
            {
                case BrushSettings.Distribution.Center:
                    brushDistribution.AddPrefabs_Center(hit.point, hit.normal);
                    break;
                case BrushSettings.Distribution.ScaleToBrushSize:
                    brushDistribution.AddPrefabs_Center(hit.point, hit.normal);
                    break;
                case BrushSettings.Distribution.Poisson_Any:
                    brushDistribution.AddPrefabs_Poisson_Any(hit.point, hit.normal);
                    break;
                case BrushSettings.Distribution.Poisson_Terrain:
                    brushDistribution.AddPrefabs_Poisson_Terrain(hit.point, hit.normal);
                    break;
                case BrushSettings.Distribution.FallOff:
                    Debug.Log("Not implemented yet: " + editorTarget.brushSettings.distribution);
                    break;
                case BrushSettings.Distribution.FallOff2d:
                    Debug.Log("Not implemented yet: " + editorTarget.brushSettings.distribution);
                    break;
            }

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
            GameObject container = editorTarget.container as GameObject;

            float radius = editorTarget.brushSettings.brushSize / 2f;

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

        public void PersistPrefab(PrefabSettings prefabSettings, Vector3 position, Quaternion rotation, Vector3 scale)
        {

            // spawn item to vs pro
            if (editorTarget.brushSettings.spawnToVSPro)
            {
                vegetationStudioProIntegration.AddNewPrefab( prefabSettings, position, rotation, scale);
            }
            // spawn item to scene
            else
            {

                // new prefab
                GameObject instance = PrefabUtility.InstantiatePrefab( prefabSettings.prefab) as GameObject;

                instance.transform.position = position;
                instance.transform.rotation = rotation;
                instance.transform.localScale = scale;

                // attach as child of container
                instance.transform.parent = editorTarget.container.transform;

                Undo.RegisterCreatedObjectUndo(instance, "Instantiate Prefab");

                if (editorTarget.spawnSettings.autoSimulationType != SpawnSettings.AutoSimulationType.None)
                {
                    autoPhysicsCollection.Add(instance);
                }
            }
        }

        #endregion Paint Prefabs

        public PrefabPainter GetPainter()
        {
            return editorTarget;
        }

    }

}
