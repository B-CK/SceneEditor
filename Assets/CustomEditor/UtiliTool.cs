using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EditorClass
{
    public class UtiliTool
    {
        /// <summary>
        /// 路径分格符转为 "/"
        /// </summary>
        public static string Win2UnityPath(string path)
        {
            return path.Replace("\\", "/");
        }
        /// <summary>
        /// 路径分格符转为 "\\"
        /// </summary>
        public static string Unity2WinPath(string path)
        {
            return path.Replace("/", "\\");
        }
        public static string UnityRelativePath(string path)
        {
            string p = Win2UnityPath(path);
            return p.Replace(Application.dataPath, "Assets");
        }

        public static void AddChildToZero(Transform parent, Transform child)
        {
            child.parent = parent;
            child.position = Vector3.zero;
        }
    }
}