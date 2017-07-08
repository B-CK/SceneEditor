#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace EditorClass
{
    [System.Serializable]
    public class SPEditorButton : SPEditorComBase
    {
        public bool _isClick = false;

        public override void Draw(Action action = null)
        {
            _isClick = GUILayout.Button(_style.name);
        }
    }
}
#endif