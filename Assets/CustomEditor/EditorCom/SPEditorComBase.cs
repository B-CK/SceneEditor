#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace EditorClass
{
    public class SPEditorComBase
    {
        public Rect _rect;
        public GUIStyle _style;

        public virtual void Draw(Action action = null)
        {

        }
    }
}
#endif