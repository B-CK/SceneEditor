#if UNITY_EDITOR
using EditorClass;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;

namespace EditorClass
{
    [ExecuteInEditMode]
    public class AuxBuildingBlocksWindow : MonoBehaviour
    {
        public SPEditorArea _sceneBgArea;
        public SPEditorArea _sceneMenuArea;
        public SPEditorArea _sceneMenuOptimizeArea;
        public SPEditorArea _sceneMenuContentArea;

        public SPEditorLabel _cubeSizeLabel;
        public SPEditorLabel _cubeGridSizeLabel;
        public SPEditorLabel _sceneNameLabel;

        public SPEditorArea _sceneInfoArea;
        public SPEditorScrollView _sceneInfoScroll;


        BuildingBlocksWindow _window;

        void Awake()
        {
            _window = Resources.FindObjectsOfTypeAll<BuildingBlocksWindow>()[0];
            _sceneBgArea = _window._sceneBgArea;
            _sceneMenuArea = _window._sceneMenuArea;
            _sceneMenuOptimizeArea = _window._sceneMenuOptimizeArea;
            _sceneMenuContentArea = _window._sceneMenuContentArea;
            _cubeSizeLabel = _window._cubeSizeLabel;
            _cubeGridSizeLabel = _window._cubeGridSizeLabel;
            _sceneNameLabel = _window._sceneNameLabel;
            _sceneInfoArea = _window._sceneInfoArea;
            _sceneInfoScroll = _window._sceneInfoScroll;

            Debug.Log("====>> " + name + " 初始化结束");
        }

        /// <summary>
        /// Inspector参数同步到Editor界面
        /// </summary>
        void OnValidate()
        {
            if (BuildingBlocksWindow.window == null) return;
            BuildingBlocksWindow.window.Repaint();
        }

        //var cls = ScriptableObject.CreateInstance<BuildingBlocksWindow>();
        //var sObj = new SerializedObject(cls);
        //var pSceneNameLabel = sObj.FindProperty("_sceneNameLabel");
        //_sceneNameLabel._style.name = pSceneNameLabel.stringValue;
        //属性序列化方式也可设置参数
    }
}
#endif