#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace EditorClass
{
    public class EditorUtiliTool
    {
        public static string ProjectPath = Application.dataPath;

        /// <summary>
        /// 路径分格符转换 0 = "/";  1 = "\\"
        /// </summary>
        public static string PathSwapSplitSign(string path, int n = 0)
        {
            return n == 0 ? path.Replace("\\", "/") : path.Replace("/", "\\");
        }
        /// <summary>
        /// Windows路径转换Unity项目路径   0 = "/";  1 = "\\" {例:E:\\..}
        /// </summary>
        public static string WinAbs2UnityPath(string path, int n = 0)
        {
            string p = PathSwapSplitSign(path, n);
            return p.Replace(ProjectPath, "Assets");
        }
        /// <summary>
        /// Unity项目路径转换Windows路径   0 = "/"; 1 = "\\"  {例:Assets/Scene}
        /// </summary>
        public static string UnityPath2WinAbs(string path, int n = 1)
        {
            string p = path.Replace("Assets", ProjectPath);
            return PathSwapSplitSign(p, n);
        }
        public static bool ContainsExt(string path, params string[] exts)
        {
            string ext = Path.GetExtension(path);
            bool contain = true;
            for (int i = 0; i < exts.Length; i++)
            {
                contain = exts[i] == ext;
            }

            return contain;
        }
        public static string Path2ResourcesPath(string path)
        {
            string p = WinAbs2UnityPath(path);
            string ext = Path.GetExtension(p);
            return p.Replace("Assets/Resources/", "").Replace(ext, "");
        }

        /// <summary>
        /// 加载文件夹中所有资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dirPath">Unity 路径</param>
        /// <returns></returns>
        public static List<T> LoadAllAssets<T>(string dirPath, string pattern = "", SearchOption searchOption = SearchOption.TopDirectoryOnly) where T : Object
        {
            List<T> assets = new List<T>();
            string path = UnityPath2WinAbs(dirPath);
            string[] fs = Directory.GetFiles(dirPath, pattern, searchOption);
            for (int i = 0; i < fs.Length; i++)
            {
                string p = WinAbs2UnityPath(fs[i]);
                assets.Add(AssetDatabase.LoadAssetAtPath<T>(p));
            }
            return assets;
        }

        public static void AddChildToP(Transform parent, Transform child)
        {
            child.parent = parent;
            child.localPosition = Vector3.zero;
        }
        public static void AddChildToPR(Transform parent, Transform child)
        {
            child.parent = parent;
            child.localPosition = Vector3.zero;
            child.localEulerAngles = Vector3.zero;
        }
        public static void AddChildToPRS(Transform parent, Transform child)
        {
            child.parent = parent;
            child.localPosition = Vector3.zero;
            child.localEulerAngles = Vector3.zero;
            child.localScale = Vector3.zero;
        }
    }
}
#endif