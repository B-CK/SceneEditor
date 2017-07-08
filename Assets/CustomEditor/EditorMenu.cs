#if UNITY_EDITOR
using UnityEditor;

namespace EditorClass
{
    public class EditorMenu
    {
        /// <summary>
        /// 地形管理器
        /// </summary>
        [MenuItem("CustomTools/Scenes", false, 1)]
        static void BuildBlocks()
        {
            EditorUtility.UnloadUnusedAssetsImmediate();
            if (!EditorApplication.isCompiling)
                BuildingBlocksWindow.Init();
        }

        /// <summary>
        /// 材质管理器
        /// </summary>
        //[MenuItem("CustomTools/Materials", false, 1)]
        static void BuildBlocksMaterials()
        {
            //BuildingBlocksMatWindow.Init();
        }
    }
}
#endif