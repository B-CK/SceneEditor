#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace EditorClass
{
    public class EditorClass
    {
        //反射底层类
        //public static Texture2D FindTexture(string name)
        //{
        //    Type tyEditorGUIUtility = Type.GetType("UnityEditor.EditorGUIUtility,UnityEditor.dll");
        //    MethodInfo method = tyEditorGUIUtility.GetMethod("FindTexture", BindingFlags.Static | BindingFlags.Public);

        //    return (Texture2D)method.Invoke(null, new object[] { name });
        //}
    }
}

#endif