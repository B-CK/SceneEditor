#if UNITY_EDITOR
using UnityEngine;

namespace EditorClass
{
    public class EditorConst
    {
        //Windows路径分隔符均使用 "\\";Unity 加载路径分格符 "/"
        public static string BuildingBlocksDir = Application.dataPath.Replace("/","\\") + "\\Building Blocks";
        public static string BlocksScenesDir = BuildingBlocksDir + "\\Scenes";
        public static string BlocksPrefabsDir = BuildingBlocksDir + "\\Prefabs";
        public static string BlocksTexturesDir = BuildingBlocksDir + "\\Textures";
        public static string BlocksMaterialsDir = BuildingBlocksDir + "\\Materials";
        public static string BlocksConfigFilesDir = BuildingBlocksDir + "\\ConfigFiles";


    }
}
#endif