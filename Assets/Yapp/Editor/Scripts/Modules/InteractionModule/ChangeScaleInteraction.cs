using Rowlan.Yapp;
using UnityEditor;
using UnityEngine;
using static Rowlan.Yapp.BrushComponent;

namespace Rowlan.Yapp
{
    public class ChangeScaleInteraction : InteractionModuleI
    {
        SerializedProperty changeScaleStrength;

#pragma warning disable 0414
        PrefabPainterEditor editor;
        PrefabPainter editorTarget;
#pragma warning restore 0414

        public ChangeScaleInteraction(PrefabPainterEditor editor)
        {
            this.editor = editor;
            this.editorTarget = editor.GetPainter();

            changeScaleStrength = editor.FindProperty(x => x.interactionSettings.changeScale.changeScaleStrength);
        }

        public void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(new GUIContent("Shift = Grow, Ctrl+Shift = Shrink"));

            GUILayout.BeginVertical("box");

            EditorGUILayout.LabelField("Change Scale", GUIStyles.BoxTitleStyle);

            EditorGUILayout.PropertyField(changeScaleStrength, new GUIContent("Strength", "Strength of the scale adjustment"));

            GUILayout.EndVertical();
        }

        public bool OnSceneGUI(BrushMode brushMode, RaycastHit raycastHit, out bool applyPhysics)
        {
            applyPhysics = false;

            switch (brushMode)
            {
                case BrushMode.ShiftPressed:

                    Grow(raycastHit);

                    // don't consume event; mustn't be consumed during layout or repaint
                    //Event.current.Use();

                    applyPhysics = true;
                    return true;

                case BrushMode.ShiftCtrlPressed:

                    Shrink(raycastHit);

                    // don't consume event; mustn't be consumed during layout or repaint
                    //Event.current.Use();

                    applyPhysics = true;
                    return true;

            }

            return false;
        }
        private void Grow(RaycastHit hit)
        {
            ChangeScale(hit, true);
        }

        private void Shrink(RaycastHit hit)
        {
            ChangeScale(hit, false);
        }

        // TODO: check performance; currently invoked multiple times in the editor loop
        private void ChangeScale(RaycastHit hit, bool grow)
        {
            // just some arbitrary value depending on the magnet strength which ranges from 0..100
            float adjustFactor = editorTarget.interactionSettings.changeScale.changeScaleStrength / 1000f;

            Transform[] containerChildren = PrefabUtils.GetContainerChildren(editorTarget.container);

            foreach (Transform transform in containerChildren)
            {
                Vector3 distance = hit.point - transform.position;

                // only those within the brush
                if (distance.magnitude > editorTarget.brushSettings.brushSize / 2f)
                    continue;

                Undo.RegisterCompleteObjectUndo(transform, "Change scale");

                transform.localScale += transform.localScale * adjustFactor * (grow ? 1 : -1);
            }
        }
    }
}
