#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace EditorClass
{
    /// <summary>
    /// 注:不受自动布局限制
    /// </summary>
    [System.Serializable]
    public class SPEditorArea : SPEditorComBase
    {
        public override void Draw(Action action = null)
        {
            if (action == null)
                return;

            if (_style != null)
                GUILayout.BeginArea(_rect, _style);
            else
                GUILayout.BeginArea(_rect);
            action();
            GUILayout.EndArea();
        }
    }
}
#endif