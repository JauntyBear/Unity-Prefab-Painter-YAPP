using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Yapp
{
    public class SpawnExtension
    {
        #region Properties

        SerializedProperty autoSimulationType;
        SerializedProperty autoSimulationHeightOffset;
        SerializedProperty autoSimulationStepCountMax;
        SerializedProperty autoSimulationStepIterations;

        #endregion Properties

#pragma warning disable 0414
        PrefabPainterEditor editor;
#pragma warning restore 0414

        PrefabPainter editorTarget;

        public SpawnExtension(PrefabPainterEditor editor)
        {
            this.editor = editor;
            this.editorTarget = editor.GetPainter();

            autoSimulationType = editor.FindProperty(x => x.spawnSettings.autoSimulationType);
            autoSimulationHeightOffset = editor.FindProperty(x => x.spawnSettings.autoSimulationHeightOffset);
            autoSimulationStepCountMax = editor.FindProperty(x => x.spawnSettings.autoSimulationStepCountMax);
            autoSimulationStepIterations = editor.FindProperty(x => x.spawnSettings.autoSimulationStepIterations);
        }

        public void OnInspectorGUI()
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Spawn", GUIStyles.BoxTitleStyle);

            // auto physics
            EditorGUILayout.PropertyField(autoSimulationType, new GUIContent("Physics Simulation"));
            if (autoSimulationType.enumValueIndex != (int)SpawnSettings.AutoSimulationType.None)
            {
                EditorGUI.indentLevel++;
                {
                    EditorGUILayout.PropertyField(autoSimulationHeightOffset, new GUIContent("Height Offset"));
                    EditorGUILayout.PropertyField(autoSimulationStepCountMax, new GUIContent("Step Count Max", "Maximum number of simulation steps to perform"));
                    EditorGUILayout.PropertyField(autoSimulationStepIterations, new GUIContent("Step Iterations", "Number of physics steps to perform in a single simulation step. lower = smoother, higher = faster"));
                }
                EditorGUI.indentLevel--;
            }

            GUILayout.EndVertical();
        }

       
    }
}