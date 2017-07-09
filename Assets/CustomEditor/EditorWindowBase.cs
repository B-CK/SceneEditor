#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;


namespace EditorClass
{
    public abstract class EditorWindowBase : EditorWindow
    {

        private const bool _disableSerialize = true;

        /// <summary>
        /// 窗口布局参数可视化对象
        /// </summary>
        private GameObject _pramaObj;
        protected void BindSerializeClass(string name, Type cls)
        {
            if (_disableSerialize) return;

            DestroyImmediate(GameObject.Find(name));
            _pramaObj = new GameObject(name, cls);
            _pramaObj.hideFlags = HideFlags.DontSave;
        }

        protected virtual void OnEnable()
        {

        }

        protected virtual void OnDisable()
        {
            DestroyImmediate(_pramaObj);
        }
        protected virtual void OnProjectChange()
        {
            Debug.Log("OnProjectChange");
        }
    }
}
#endif