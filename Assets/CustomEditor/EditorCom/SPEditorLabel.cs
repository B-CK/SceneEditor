#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace EditorClass
{
    [System.Serializable]
    public class SPEditorLabel : SPEditorComBase
    {
        public override void Draw(Action action = null)
        {
            GUILayout.Label(_style.name, _style);
        }
    }
}
#endif