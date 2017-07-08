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
    public class SPEditorScrollView : SPEditorComBase
    {
        float _space;

        public SPEditorScrollView(float space = 2f)
        {
            base._rect = new Rect(Vector2.zero, Vector2.zero);
            this._space = space;
        }

        public override void Draw(Action action = null)
        {
            if (action == null)
                return;

            GUILayout.Space(_space);
            _rect.position = GUILayout.BeginScrollView(_rect.position, false, false);
            action();
            GUILayout.EndScrollView();
            GUILayout.Space(_space);
        }

    }
}
#endif