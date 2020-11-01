using UnityEngine;
using UnityEditor;
using static Yapp.BrushComponent;

namespace Yapp
{
    public class InteractionModuleEditor: ModuleEditorI
    {
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

        private float magnetFactor = 0.1f; // TODO property

        public InteractionModuleEditor(PrefabPainterEditor editor)
        {
            this.editor = editor;
            this.gizmo = editor.GetPainter();
        }

        public void OnInspectorGUI()
        {

            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Interaction", GUIStyles.BoxTitleStyle);

            EditorGUILayout.HelpBox("Perform brush operations on the container children", MessageType.Info);

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
                ApplyPhysics();
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

                transform.position += direction * magnetFactor * (attract ? 1 : -1);
            }
        }

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

            if (gizmo.brushSettings.autoSimulationType == BrushSettings.AutoSimulationType.Once)
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
