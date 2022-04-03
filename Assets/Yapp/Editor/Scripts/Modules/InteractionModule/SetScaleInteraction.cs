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

        private void SetScale(RaycastHit hit)
        {
            switch (editor.GetPainter().brushSettings.spawnTarget)
            {
                case BrushSettings.SpawnTarget.PrefabContainer:

                    SetScalePrefabs(hit);

                    break;

                case BrushSettings.SpawnTarget.TerrainTrees:

                    float brushSize = editorTarget.brushSettings.brushSize;

                    float scaleValueX = editorTarget.interactionSettings.setScale.setScaleValue;
                    float scaleValueY = editorTarget.interactionSettings.setScale.setScaleValue;

                    UnityTerrainUtils.SetScale(hit.point, brushSize, scaleValueX, scaleValueY);

                    break;

                case BrushSettings.SpawnTarget.TerrainDetails:
                    Debug.LogError("Not implemented");
                    break;

                case BrushSettings.SpawnTarget.VegetationStudioPro:
                    Debug.LogError("Not implemented");
                    break;
            }
        }

        // TODO: check performance; currently invoked multiple times in the editor loop
        private void SetScalePrefabs(RaycastHit hit)
        {
            float scaleValue = editorTarget.interactionSettings.setScale.setScaleValue;
            Vector3 scaleVector = new Vector3(scaleValue, scaleValue, scaleValue);

            Transform[] containerChildren = PrefabUtils.GetContainerChildren(editorTarget.container, hit, editorTarget.brushSettings.brushSize);

            foreach (Transform transform in containerChildren)
            {
                Undo.RegisterCompleteObjectUndo(transform, "Set scale");

                transform.localScale = scaleVector;
            }
        }
    }
}
