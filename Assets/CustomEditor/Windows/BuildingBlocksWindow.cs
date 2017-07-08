#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
using EditorClass;
using System.Text;
using System.IO;

namespace EditorClass
{
    [Serializable]
    public class BuildingBlocksWindow : EditorWindowBase
    {
        //currentNew = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Additive);

        Scene currentNew;


        public static BuildingBlocksWindow window;

        static Rect BlocksWindowPosition = new Rect();

        Vector2 _winMinSize = new Vector2(400f, 200f);
        Vector2 _winMaxSize = new Vector2(2500f, 200f);
        GameObject _mapGridObj;

        public static void Init()
        {
            window = (BuildingBlocksWindow)GetWindow(typeof(BuildingBlocksWindow));
            window.titleContent = new GUIContent("场景管理器", EditorGUIUtility.FindTexture("Navigation"));
            UnityEngine.Object.DontDestroyOnLoad(window);
            window.minSize = window._winMinSize;
            window.maxSize = window._winMaxSize;
            BlocksWindowPosition.size = Vector2.right * 800f;
            window.position = BlocksWindowPosition;
            window.autoRepaintOnSceneChange = true;
            window.Show();

            window._mapGridObj = new GameObject("__Map Grid");
            window._mapGridObj.hideFlags = HideFlags.DontSave;
            window._mapGridObj.AddComponent<MapGrid>();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            BlocksWindowPosition = position;

            _blockPathsDic.Clear();
            _blocksDic.Clear();
            _blockPage.Clear();
            DestroyImmediate(_mapGridObj);
            Caching.CleanCache();
        }


        #region 界面参数
        public string SceneName
        {
            get { return _sceneNameLabel._style.name; }
            set { _sceneNameLabel._style.name = string.Format("--- {0} ---", value); }
        }
        public int _sceneSizeX = 30;
        public int _sceneSizeZ = 30;
        public int _sceneSizeY = 0;
        public Dictionary<string, int> _counterDic = new Dictionary<string, int>();
        public Vector3 _cubeSize = Vector3.one;
        #endregion

        #region 界面元素变量
        int _sceneMenuIndex;
        /// <summary>
        /// 文件夹名
        /// </summary>
        List<string> _blocksTypes = new List<string>();
        /// <summary>
        /// key - 预制类型; value - 预制路径
        /// </summary>
        Dictionary<string, string[]> _blockPathsDic = new Dictionary<string, string[]>();
        /// <summary>
        /// key - 预制类型; value - 预制信息
        /// </summary>
        Dictionary<string, List<BlockItem>> _blocksDic = new Dictionary<string, List<BlockItem>>();
        /// <summary>
        /// 当前显示预制页
        /// </summary>
        List<BlockItem> _blockPage = new List<BlockItem>();
        int _blockMenuIndex = 0;


        public SPEditorArea _sceneBgArea;
        public SPEditorArea _sceneMenuArea;
        public SPEditorArea _sceneMenuOptimizeArea;
        public SPEditorArea _sceneMenuContentArea;

        public SPEditorLabel _cubeSizeLabel;
        public SPEditorLabel _cubeGridSizeLabel;
        public SPEditorLabel _sceneNameLabel;

        public SPEditorArea _sceneInfoArea;
        public SPEditorScrollView _sceneInfoScroll;

        //预制菜单
        public SPEditorArea _prefabBgArea;
        public SPEditorButton _prefabBtn;
        public SPEditorScrollView _prefabsScroll;
        public float _blockBtnGrap = 5f;

        class BlockItem
        {
            public GameObject prefab;
            public Texture texture;
        }
        #endregion

        #region 界面默认设置
        /// <summary>
        /// 窗口默认值设置
        /// 注:具体值可通过辅助类来设置
        /// 注:UI界面元素均不能在此调用,因为部分系统UI元素未初始化
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();

            _sceneBgArea = new SPEditorArea();
            _sceneBgArea._rect = new Rect(0, 0, 250, 4000);

            float menuHeight = 20;
            _sceneMenuArea = new SPEditorArea();
            _sceneMenuArea._rect = new Rect(-6, 0, 210, menuHeight);
            _sceneMenuOptimizeArea = new SPEditorArea();
            _sceneMenuOptimizeArea._rect = new Rect(200, 0, 50, menuHeight);
            _sceneMenuContentArea = new SPEditorArea();
            _sceneMenuContentArea._rect = new Rect(0, 20, _sceneBgArea._rect.width, _sceneBgArea._rect.height - menuHeight);

            _cubeSizeLabel = new SPEditorLabel();
            _cubeGridSizeLabel = new SPEditorLabel();
            _sceneNameLabel = new SPEditorLabel();
            GUIStyle nameStyle = new GUIStyle();
            nameStyle.alignment = TextAnchor.MiddleCenter;
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.fontSize = 13;
            nameStyle.contentOffset = Vector2.up * 6;
            float value = 180 / 255f;
            nameStyle.normal.textColor = new Color(value, value, value, 255f);
            nameStyle.name = "--- 场景名称 ---";
            _sceneNameLabel._style = nameStyle;

            _sceneInfoArea = new SPEditorArea();
            _sceneInfoArea._rect = new Rect(0, 70, _sceneBgArea._rect.width, 100);

            _sceneInfoScroll = new SPEditorScrollView();

            //预制菜单
            _prefabBgArea = new SPEditorArea();
            _prefabBgArea._rect = new Rect(_sceneBgArea._rect.width, 0, position.width - _sceneBgArea._rect.width, 300);
            _prefabBtn = new SPEditorButton();
            _prefabBtn._rect = new Rect(Vector2.zero, Vector2.one * 50f);
            _prefabsScroll = new SPEditorScrollView();
            _blockPathsDic.Clear();
            _blocksTypes.Clear();
            string[] dirPaths = Directory.GetDirectories(EditorConst.BlocksPrefabsDir);
            for (int i = 0; i < dirPaths.Length; i++)
            {
                string blockTypePath = dirPaths[i];
                if (string.IsNullOrEmpty(blockTypePath)) continue;
                string typeName = Path.GetFileName(blockTypePath);
                _blocksTypes.Add(typeName);
                string[] blockPaths = Directory.GetFiles(blockTypePath, "**.prefab");
                if (blockPaths.Length == 0) continue;

                _blockPathsDic.Add(typeName, blockPaths);
            }
            SetBlockMenu();

            //序列话属性类绑定
            BindSerializeClass("场景管理器", typeof(AuxBuildingBlocksWindow));
        }
        #endregion

        #region 界面绘制
        /// <summary>
        /// 编辑界面设置
        /// 注:界面UI内容只能包含在此方法中
        /// 注:外部Area 已指定系统编辑风格,内部Area内的UI元素则不会自动填充整个Area
        /// </summary>
        void OnGUI()
        {
            _sceneBgArea._style = EditorStyles.helpBox;
            _sceneBgArea.Draw(() =>
            {
                //场景菜单栏
                int sceneMenuIndex = 0;
                _sceneMenuArea.Draw(() =>
                {
                    sceneMenuIndex = GUILayout.SelectionGrid(_sceneMenuIndex, new string[] { "信息", "打开", "新建" }, 3, EditorStyles.toolbarButton);
                });

                bool isOptimized = false;
                _sceneMenuOptimizeArea.Draw(() =>
                {
                    isOptimized = GUILayout.Button("优化", EditorStyles.toolbarButton);
                });

                //场景菜单内容        
                _sceneMenuContentArea.Draw(() =>
                {
                    OnClickSceneMenu(sceneMenuIndex);
                });
            });

            //预制菜单
            _prefabBgArea._rect.width = position.width - _sceneBgArea._rect.width;
            _prefabBgArea.Draw(() =>
            {
                int blocksMenuIndex = GUILayout.SelectionGrid(_blockMenuIndex, _blocksTypes.ToArray(), _blocksTypes.Count, EditorStyles.toolbarButton);
                BlocksMenu(blocksMenuIndex);
            });
        }

        void OnClickSceneMenu(int index)
        {
            // 0信息菜单 1新建场景 2打开场景
            if (index != _sceneMenuIndex)
            {
                _sceneMenuIndex = index;
                Debug.Log(_sceneMenuIndex);
            }

            //场景小窗口切换逻辑
            switch (index)
            {
                case 0:
                    SceneInfoArea();
                    break;
                case 1:
                    SceneOpenArea();
                    break;
                case 2:
                    SceneNewArea();
                    break;
            }
        }
        void SceneInfoArea()
        {
            GUILayout.BeginHorizontal();
            _cubeSizeLabel._style = new GUIStyle(EditorStyles.label);
            _cubeSizeLabel._style.name = "方阵元素尺寸";
            _cubeSizeLabel.Draw();
            float cubeSizeX = EditorGUILayout.FloatField(_cubeSize.x);
            float cubeSizeY = EditorGUILayout.FloatField(_cubeSize.y);
            float cubeSizeZ = EditorGUILayout.FloatField(_cubeSize.z);
            _cubeSize = new Vector3(cubeSizeX, cubeSizeY, cubeSizeZ);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            _cubeGridSizeLabel._style = new GUIStyle(EditorStyles.label);
            _cubeGridSizeLabel._style.name = "方阵规格";
            _cubeGridSizeLabel.Draw();
            int x = EditorGUILayout.IntField(_sceneSizeX);
            GUILayout.Label("X", EditorStyles.label);
            int z = EditorGUILayout.IntField(_sceneSizeZ);
            _sceneSizeX = x;
            _sceneSizeZ = z;
            GUILayout.EndHorizontal();

            _sceneNameLabel.Draw();

            _sceneInfoArea._style = new GUIStyle(EditorStyles.helpBox);
            _sceneInfoArea.Draw(() =>
            {
                _sceneInfoScroll.Draw(() =>
                {
                    StringBuilder countMsg = new StringBuilder();
                    int count = 0;
                    foreach (var item in _counterDic)
                    {
                        count++;
                        countMsg.Append(string.Format("{0}: {1}{2}", item.Key, item.Value, count % 3 != 0 ? "\t\t" : "\n"));
                    }
                    GUILayout.Label(countMsg.ToString());
                });
            });
        }
        void SceneOpenArea()
        {

        }
        void SceneNewArea()
        {

        }
        void BlocksMenu(int index)
        {
            Debug.Log("BlocksMenu");
            //AssetPreview.SetPreviewTextureCacheSize(_blockPage.Count);
            if (index != _blockMenuIndex)
            {
                _blockMenuIndex = index;
                Debug.Log("材质界面 - " + _blockMenuIndex);

                _blockPage.Clear();
                SetBlockMenu();
            }

           
            //界面绘制
            _prefabsScroll.Draw(() =>
            {
                float areaWidth = _prefabBgArea._rect.width;
                float areaHeight = _prefabBgArea._rect.height;
                float btnWidth = _prefabBtn._rect.width;
                int xNum = Mathf.FloorToInt((areaWidth - _blockBtnGrap) / (_blockBtnGrap + btnWidth));
                int yNum = Mathf.CeilToInt(_blockPage.Count / xNum);
                for (int y = 0; y < yNum + 1; y++)
                {
                    GUILayout.BeginHorizontal();
                    for (int x = 0; x < xNum; x++)
                    {
                        int amount = y * xNum + x;
                        if (amount >= _blockPage.Count)
                            break;
                        BlockItem item = _blockPage[amount];
                        //_needRepaint |= item.texture == null;
                        item.texture = item.texture == null ? GeneratePreview(item.prefab) : item.texture;
                        bool isClick = GUILayout.Button(item.texture,
                            GUILayout.Width(_prefabBtn._rect.width),
                            GUILayout.Height(_prefabBtn._rect.width));
                        //砖块点击事件处理
                        //TODO
                    }
                    GUILayout.EndHorizontal();
                }
            });
        }

        void SetBlockMenu()
        {
            string blockType = _blocksTypes[_blockMenuIndex];
            if (!_blocksDic.ContainsKey(blockType))
            {
                List<BlockItem> ls = new List<BlockItem>();
                string[] blockPaths = _blockPathsDic[blockType];
                for (int i = 0; i < blockPaths.Length; i++)
                {
                    string path = UtiliTool.UnityRelativePath(blockPaths[i]);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    BlockItem item = new BlockItem();
                    item.prefab = prefab;
                    item.texture = GeneratePreview(prefab);
                    ls.Add(item);
                }
                _blockPage.AddRange(ls);
                _blocksDic.Add(blockType, ls);
            }
            else
            {
                _blockPage.AddRange(_blocksDic[blockType]);
            }
        }
        Texture GeneratePreview(GameObject prefab)
        {
            int id = prefab.GetInstanceID();
            Debug.Log(id);
            if (AssetPreview.IsLoadingAssetPreview(id)) return null;
            Texture tex = AssetPreview.GetAssetPreview(prefab);
            return tex;
        }

        bool _needRepaint = false;
        //void OnInspectorUpdate()
        //{
        //    if (!_needRepaint) return;

        //    for (int i = 0; i < _blockPage.Count; i++)
        //    {
        //        BlockItem item = _blockPage[i];
        //        _needRepaint |= item.texture == null;
        //        if (!_needRepaint)
        //        {
        //            Repaint();
        //            break;
        //        }

        //        item.texture = GeneratePreview(item.prefab);
        //    }
        //}
        #endregion
    }
}
#endif