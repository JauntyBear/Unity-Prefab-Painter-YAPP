using UnityEngine;
using UnityEditor;
using static Yapp.BrushComponent;

namespace Yapp
{
    public class InteractionModuleEditor: ModuleEditorI
    {
        #region Properties

        SerializedProperty magnetStrength;

        #endregion Properties

#pragma warning disable 0414
        PrefabPainterEditor editor;
        PrefabPainter gizmo;
#pragma warning restore 0414

        BrushComponent brushComponent = new BrushComponent();

        /// <summary>
        /// Auto physics only on special condition:
        /// + prefabs were added
        /// + mouse got released
        /// </summary>
        private bool needsPhysicsApplied = false; // TODO property


        public InteractionModuleEditor(PrefabPainterEditor editor)
        {
            this.editor = editor;
            this.gizmo = editor.GetPainter();

            magnetStrength = editor.FindProperty(x => x.interactionSettings.magnetStrength);

        }

        public void OnInspectorGUI()
        {

            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Interaction", GUIStyles.BoxTitleStyle);

            EditorGUILayout.HelpBox("Perform interactive operations on the container children", MessageType.Info);

            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Magnet", GUIStyles.BoxTitleStyle);

            EditorGUILayout.PropertyField(magnetStrength, new GUIContent("Magnet Strength", "Strength of the Magnet"));


            GUILayout.EndVertical();

        }

        public void OnSceneGUI()
        {

            // paint prefabs on mouse drag. don't do anything if no mode is selected, otherwise e.g. movement in scene view wouldn't work with alt key pressed
            if (brushComponent.DrawBrush(gizmo.brushSettings, out BrushMode brushMode, out RaycastHit raycastHit))
            {

                switch (brushMode)
                {
                    case BrushMode.ShiftPressed:

                        Attract(raycastHit);

                        needsPhysicsApplied = true;

                        // don't consume event; mustn't be consumed during layout or repaint
                        //Event.current.Use();
                        break;

                    case BrushMode.ShiftCtrlPressed:

                        Repell(raycastHit);

                        needsPhysicsApplied = true;

                        // don't consume event; mustn't be consumed during layout or repaint
                        //Event.current.Use();
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
                AutoPhysicsSimulation.ApplyPhysics(gizmo.container, gizmo.brushSettings.autoSimulationType, gizmo.brushSettings.autoSimulationStepCountMax, gizmo.brushSettings.autoSimulationStepIterations);
            }
        }

        private void Attract( RaycastHit hit)
        {
            Magnet(hit, true);
        }

        private void Repell(RaycastHit hit)
        {
            Magnet(hit, false);
        }

        /// <summary>
        /// Attract/Repell the gameobjects of the container which are within the brush
        /// </summary>
        /// <param name="hit"></param>
        /// <param name="attract"></param>
        private void Magnet( RaycastHit hit, bool attract)
        {

            Transform[] containerChildren = PrefabUtils.GetContainerChildren(gizmo.container);

            foreach (Transform transform in containerChildren)
            {
                Vector3 distance = hit.point - transform.position;

                // only those within the brush
                if (distance.magnitude > gizmo.brushSettings.brushSize /2f)
                    continue;

                Vector3 direction = distance.normalized;

                // just some arbitrary value depending on the magnet strength which ranges from 0..100
                float magnetFactor = gizmo.interactionSettings.magnetStrength / 1000f;

                transform.position += direction * magnetFactor * (attract ? 1 : -1);
            }
        }
    }
}
