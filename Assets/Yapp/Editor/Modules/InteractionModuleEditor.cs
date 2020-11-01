using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Yapp
{
    public class InteractionModuleEditor: ModuleEditorI
    {
        #pragma warning disable 0414
        PrefabPainterEditor editor;
        PrefabPainter gizmo;
        #pragma warning restore 0414
        
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
        }
    }
}
