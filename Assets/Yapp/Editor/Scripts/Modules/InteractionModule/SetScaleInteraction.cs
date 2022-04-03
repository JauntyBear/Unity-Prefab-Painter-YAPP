using Rowlan.Yapp;
using UnityEditor;
using UnityEngine;
using static Rowlan.Yapp.BrushComponent;

namespace Rowlan.Yapp
{
    public class SetScaleInteraction : InteractionModuleI
    {
        SerializedProperty setScaleValue;

#pragma warning disable 0414
        PrefabPainterEditor editor;
        PrefabPainter editorTarget;
#pragma warning restore 0414

        public SetScaleInteraction(PrefabPainterEditor editor)
        {
            this.editor = editor;
            this.editorTarget = editor.GetPainter();

            setScaleValue = editor.FindProperty(x => x.interactionSettings.setScale.setScaleValue);
        }

        public void OnInspectorGUI()
        {
            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Set Scale", GUIStyles.BoxTitleStyle);

            EditorGUILayout.PropertyField(setScaleValue, new GUIContent("Value", "The scale value to set"));

            GUILayout.EndVertical();
        }

        public bool OnSceneGUI(BrushMode brushMode, RaycastHit raycastHit, out bool applyPhysics)
        {
            applyPhysics = false;

            switch (brushMode)
            {
                case BrushMode.ShiftPressed:

                    SetScale(raycastHit);

                    // don't consume event; mustn't be consumed during layout or repaint
                    //Event.current.Use();

                    applyPhysics = true;
                    return true;

            }

            return false;
        }

        // TODO: check performance; currently invoked multiple times in the editor loop
        private void SetScale(RaycastHit hit)
        {
            float scaleValue = editorTarget.interactionSettings.setScale.setScaleValue;
            Vector3 scaleVector = new Vector3(scaleValue, scaleValue, scaleValue);

            Transform[] containerChildren = PrefabUtils.GetContainerChildren(editorTarget.container);

            foreach (Transform transform in containerChildren)
            {
                Vector3 distance = hit.point - transform.position;

                // only those within the brush
                if (distance.magnitude > editorTarget.brushSettings.brushSize / 2f)
                    continue;

                Undo.RegisterCompleteObjectUndo(transform, "Set scale");

                transform.localScale = scaleVector;
            }
        }
    }
}
