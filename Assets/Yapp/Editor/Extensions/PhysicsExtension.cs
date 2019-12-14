using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Yapp
{

    public class PhysicsExtension 
    {

        #pragma warning disable 0414
        PrefabPainterEditor editor;
        #pragma warning restore 0414

        PrefabPainter gizmo;

        public PhysicsExtension(PrefabPainterEditor editor)
        {
            this.editor = editor;
            this.gizmo = editor.GetPainter();

            if (this.gizmo.physicsSimulation == null)
            {
                this.gizmo.physicsSimulation = ScriptableObject.CreateInstance<PhysicsSimulation>();
            }
        }

        public void OnInspectorGUI()
        {
            // separator
            GUILayout.BeginVertical("box");
            //addGUISeparator();

            EditorGUILayout.LabelField("Physics Settings", GUIStyles.BoxTitleStyle);

            #region Settings

            this.gizmo.physicsSimulation.forceApplyType = (PhysicsSimulation.ForceApplyType) EditorGUILayout.EnumPopup("Force Apply Type", this.gizmo.physicsSimulation.forceApplyType);
            this.gizmo.physicsSimulation.forceMinMax = EditorGUILayout.Vector2Field("Force Min/Max", this.gizmo.physicsSimulation.forceMinMax);
            this.gizmo.physicsSimulation.forceAngleInDegrees = EditorGUILayout.FloatField("Force Angle (Degrees)", this.gizmo.physicsSimulation.forceAngleInDegrees);
            this.gizmo.physicsSimulation.randomizeForceAngle = EditorGUILayout.Toggle("Randomize Force Angle", this.gizmo.physicsSimulation.randomizeForceAngle);

            #endregion Settings

            EditorGUILayout.Space();

            #region Simulate Once

            EditorGUILayout.LabelField("Simulate Once", GUIStyles.GroupTitleStyle);

            this.gizmo.physicsSimulation.maxIterations = EditorGUILayout.IntField("Max Iterations", this.gizmo.physicsSimulation.maxIterations);

            if (GUILayout.Button("Simulate Once"))
            {
                RunSimulation();
            }

            #endregion Simulate Once

            EditorGUILayout.Space();

            #region Simulate Continuously

            EditorGUILayout.LabelField("Simulate Continuously", GUIStyles.GroupTitleStyle);

            EditorGUILayout.IntField("Simulation Step", this.gizmo.physicsSimulation.simulationStepCount);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Start"))
            {
                StartSimulation();
            }

            if (GUILayout.Button("Stop"))
            {
                StopSimulation();
            }

            GUILayout.EndHorizontal();

            #endregion Simulate Continuously

            EditorGUILayout.Space();

            #region Undo
            EditorGUILayout.LabelField("Undo", GUIStyles.GroupTitleStyle);

            if (GUILayout.Button("Undo Last Simulation"))
            {
                ResetAllBodies();
            }
            #endregion Undo

            GUILayout.EndVertical();
        }

        #region Physics Simulation

        private void RunSimulation()
        {
            this.gizmo.physicsSimulation.RunSimulationOnce(getContainerChildren());
        }

        private void ResetAllBodies()
        {
            this.gizmo.physicsSimulation.UndoSimulation();
        }

        #endregion Physics Simulation

        // TODO: create common class
        private Transform[] getContainerChildren()
        {
            if (gizmo.container == null)
                return new Transform[0];

            Transform[] children = gizmo.container.transform.Cast<Transform>().ToArray();

            return children;
        }

        private void StartSimulation()
        {
            this.gizmo.physicsSimulation.StartSimulation(getContainerChildren());
        }

        private void StopSimulation()
        {
            this.gizmo.physicsSimulation.StopSimulation();
        } 

    }
}
