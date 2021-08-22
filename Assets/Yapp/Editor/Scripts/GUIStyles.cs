using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rowlan.Yapp
{
    public class GUIStyles
    {

        private static GUIStyle _boxTitleStyle;
        public static GUIStyle BoxTitleStyle
        {
            get
            {
                if (_boxTitleStyle == null)
                {
                    _boxTitleStyle = new GUIStyle("Label");
                    _boxTitleStyle.fontStyle = FontStyle.BoldAndItalic;
                }
                return _boxTitleStyle;
            }
        }

        private static GUIStyle _groupTitleStyle;
        public static GUIStyle GroupTitleStyle
        {
            get
            {
                if (_groupTitleStyle == null)
                {
                    _groupTitleStyle = new GUIStyle("Label");
                    _groupTitleStyle.fontStyle = FontStyle.Bold;
                }
                return _groupTitleStyle;
            }
        }

        private static GUIStyle _dropAreaStyle;
        public static GUIStyle DropAreaStyle
        {
            get
            {
                if (_dropAreaStyle == null)
                {
                    _dropAreaStyle = new GUIStyle("box");
                    _dropAreaStyle.fontStyle = FontStyle.Italic;
                    _dropAreaStyle.alignment = TextAnchor.MiddleCenter;
                }
                return _dropAreaStyle;
            }
        }

        private static GUIStyle _separatorStyle;
        public static GUIStyle SeparatorStyle
        {
            get
            {
                if (_separatorStyle == null)
                {
                    _separatorStyle = new GUIStyle("box");
                    _separatorStyle.normal.background = CreateColorPixel(Color.gray);
                    _separatorStyle.stretchWidth = true;
                    _separatorStyle.border = new RectOffset(0, 0, 0, 0);
                    _separatorStyle.fixedHeight = 1f;
                }
                return _separatorStyle;
            }
        }

        /// <summary>
        /// Creates a 1x1 texture
        /// </summary>
        /// <param name="Background">Color of the texture</param>
        /// <returns></returns>
        public static Texture2D CreateColorPixel(Color color)
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        public static Color DefaultBackgroundColor = GUI.backgroundColor;
        public static Color ErrorBackgroundColor = new Color( 1f,0f,0f,0.7f); // red tone

        public static Color BrushNoneInnerColor = new Color(0f, 0f, 1f, 0.05f); // blue tone
        public static Color BrushNoneOuterColor = new Color(0f, 0f, 1f, 1f); // blue tone

        public static Color BrushAddInnerColor = new Color(0f, 1f, 0f, 0.05f); // green tone
        public static Color BrushAddOuterColor = new Color(0f, 1f, 0f, 1f); // green tone

        public static Color BrushRemoveInnerColor = new Color(1f, 0f, 0f, 0.05f); // red tone
        public static Color BrushRemoveOuterColor = new Color(1f, 0f, 0f, 1f); // red tone

        public static Color DropAreaBackgroundColor = new Color(0.8f, 0.8f, 0.8f, 1f); // gray tone

        public static Color PhysicsRunningButtonBackgroundColor = new Color(1f, 0f, 0f, 0.7f); // red tone

    }
}