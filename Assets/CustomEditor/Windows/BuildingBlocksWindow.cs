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
        MapGrid _mapGridObj;

        public static void Init()
        {
            window = (BuildingBlocksWindow)GetWindow<BuildingBlocksWindow>(false, "场景管理器", true);
            window.titleContent = new GUIContent("场景管理器", EditorGUIUtility.FindTexture("Navigation"));
            DontDestroyOnLoad(window);
            window.minSize = window._winMinSize;
            window.maxSize = window._winMaxSize;
            BlocksWindowPosition.size = Vector2.right * 800f;
            window.position = BlocksWindowPosition;
            window.autoRepaintOnSceneChange = true;
            window.Show();

            GameObject gridObj = new GameObject("Map Grid");
            gridObj.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
            window._mapGridObj = gridObj.AddComponent<MapGrid>();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            BlocksWindowPosition = position;

            _blockPathsDic.Clear();
            _blocksDic.Clear();
            _blockPage.Clear();
            DestroyImmediate(_mapGridObj.gameObject);
            Resources.UnloadUnusedAssets();
        }

        #region 界面参数
        public string SceneName
        {
            get { return _sceneNameLabel._style.name; }
            set { _sceneNameLabel._style.name = string.Format("--- {0} ---", value); }
        }
        public int _sceneSizeX = 30;
        public int _sceneSizeZ = 30;
        public Dictionary<string, int> _counterDic = new Dictionary<string, int>();
        public Vector3 _cubeSize = Vector3.one;
        //public float _prefabAngle = 90f;
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
        public SPEditorLabel _cubeAngleLabel;
        public SPEditorLabel _sceneNameLabel;

        public SPEditorArea _sceneInfoArea;
        public SPEditorScrollView _sceneInfoScroll;

        //预制菜单
        public SPEditorArea _prefabBgArea;
        public SPEditorButton _prefabBtn;
        public SPEditorScrollView _prefabsScroll;

        const int _cellPadding = 4;
        const int _cellSize = 80;
        Vector2 _mPos = Vector2.zero;

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
            _sceneBgArea._rect = new Rect(0, 0, 250, Screen.height);

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
            _cubeAngleLabel = new SPEditorLabel();
            GUIStyle nameStyle = new GUIStyle();
            nameStyle.alignment = TextAnchor.MiddleCenter;
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.fontSize = 13;
            nameStyle.contentOffset = Vector2.up * 0;
            float value = 180 / 255f;
            nameStyle.normal.textColor = new Color(value, value, value, 255f);
            nameStyle.name = "--- 场景名称 ---";
            _sceneNameLabel._style = nameStyle;

            _sceneInfoArea = new SPEditorArea();
            _sceneInfoArea._rect = new Rect(0, 75, _sceneBgArea._rect.width, 100);

            _sceneInfoScroll = new SPEditorScrollView();

            //预制菜单
            _prefabBgArea = new SPEditorArea();
            _prefabBgArea._rect = new Rect(_sceneBgArea._rect.width, 0, Screen.width - _sceneBgArea._rect.width, 200f);
            _prefabBtn = new SPEditorButton();
            _prefabBtn._rect = new Rect(Vector2.zero, Vector2.one * _cellSize);
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
            int blocksMenuIndex = 0;
            _prefabBgArea._rect.width = position.width - _sceneBgArea._rect.width;
            _prefabBgArea.Draw(() =>
            {
                blocksMenuIndex = GUILayout.SelectionGrid(_blockMenuIndex, _blocksTypes.ToArray(), _blocksTypes.Count, EditorStyles.toolbarButton);
                BlocksMenu(blocksMenuIndex);
            });

            Event current = Event.current;
            //Debug.Log(current.type + "  :  " + current.keyCode);
            if (current.type == EventType.KeyUp && current.keyCode == KeyCode.Return)
            {
                Debug.Log(_cubeSize);
                _mapGridObj.ResetBlocks();
            }
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
            _cubeSizeLabel._style.name = "格子尺寸";
            _cubeSizeLabel.Draw();
            float cubeSizeX = EditorGUILayout.FloatField(_cubeSize.x);
            float cubeSizeZ = EditorGUILayout.FloatField(_cubeSize.z);
            cubeSizeX = Mathf.Clamp(cubeSizeX, float.MinValue, float.MaxValue);
            cubeSizeZ = Mathf.Clamp(cubeSizeZ, float.MinValue, float.MaxValue);
            _cubeSize = new Vector3(cubeSizeX, 0, cubeSizeZ);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            _cubeGridSizeLabel._style = new GUIStyle(EditorStyles.label);
            _cubeGridSizeLabel._style.name = "矩阵规格";
            _cubeGridSizeLabel.Draw();
            int sceneSizeX = EditorGUILayout.IntField(_sceneSizeX);
            GUILayout.Label("X", EditorStyles.label);
            int sceneSizeZ = EditorGUILayout.IntField(_sceneSizeZ);
            _sceneSizeX = Mathf.Clamp(sceneSizeX, int.MinValue, int.MaxValue);
            _sceneSizeZ = Mathf.Clamp(sceneSizeZ, int.MinValue, int.MaxValue);
            GUILayout.EndHorizontal();

            //GUILayout.BeginHorizontal();
            //_cubeAngleLabel._style = new GUIStyle(EditorStyles.label);
            //_cubeAngleLabel._style.name = "预制角度";
            //_cubeAngleLabel.Draw();
            //_prefabAngle = EditorGUILayout.FloatField(_prefabAngle);
            //GUILayout.EndHorizontal();

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
            if (index != _blockMenuIndex)
            {
                _blockMenuIndex = index;
                _blockPage.Clear();
                SetBlockMenu();
            }


            ////界面绘制
            _prefabsScroll.Draw(() =>
            {
                float areaWidth = _prefabBgArea._rect.width - _cellPadding - 10;
                float areaHeight = _prefabBgArea._rect.height;
                int xNum = Mathf.FloorToInt((areaWidth) / (_cellPadding + _cellSize));
                int yNum = Mathf.CeilToInt(_blockPage.Count / xNum);
                //float px = _cellPadding;
                //float py = _cellPadding;
                //int spacingX = _cellSize + _cellPadding;
                //int spacingY = spacingX;
                for (int y = 0; y < yNum + 1; y++)
                {
                    GUILayout.BeginHorizontal();
                    for (int x = 0; x < xNum; x++)
                    {
                        int amount = y * xNum + x;
                        if (amount >= _blockPage.Count)
                            break;
                        BlockItem item = _blockPage[amount];
                        item.texture = item.texture == null ? GeneratePreview(item.prefab) : item.texture;

                        //Rect rect = new Rect(px, py, _cellSize, _cellSize - 1);
                        if (GUILayout.Button(item.texture, GUILayout.Width(_cellSize), GUILayout.Height(_cellSize - 1)))
                        {
                            //砖块点击事件处理               
                            //HandleDragAndDrop(item.prefab);
                        }
                        //px += spacingX;
                        //if (px + spacingX > areaWidth)
                        //{
                        //    py += spacingY;
                        //    px = _cellPadding;
                        //}
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
            Texture tex = AssetPreview.GetAssetPreview(prefab);
            if (AssetPreview.IsLoadingAssetPreview(id)) return null;
            return tex;
        }
        void HandleDragAndDrop(GameObject prefab)
        {
            return;
            Event current = Event.current;
            switch (current.type)
            {
                case EventType.MouseDown:
                    Debug.Log("" + DragAndDrop.objectReferences == null + "    " + DragAndDrop.objectReferences.Length);
                    Debug.Log(current.type);
                    break;
                case EventType.MouseUp:
                    DragAndDrop.PrepareStartDrag();
                    break;
                case EventType.MouseDrag:
                    //Debug.Log(current.type);
                    DragAndDrop.StartDrag("Prefab Tool");
                    current.Use();
                    break;
                case EventType.DragUpdated:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    current.Use();
                    break;
                case EventType.DragPerform:
                    Debug.Log(current.type);
                    current.Use();
                    break;
                case EventType.DragExited:
                case EventType.Ignore:
                    break;
            }

            //Debug.Log(DragAndDrop.objectReferences.Length);
            return;
            GameObject dragged = DraggedObject;
            bool isDragging = (dragged != null);

            switch (current.type)
            {
                case EventType.DragUpdated:
                    break;
                case EventType.DragPerform:
                    break;
                case EventType.DragExited:
                case EventType.Ignore:
                    break;
            }

        }
        GameObject DraggedObject
        {
            get
            {
                if (DragAndDrop.objectReferences == null) return null;
                if (DragAndDrop.objectReferences.Length == 1) return DragAndDrop.objectReferences[0] as GameObject;
                return null;
            }
            set
            {
                if (value != null)
                {
                    DragAndDrop.PrepareStartDrag();
                    DragAndDrop.objectReferences = new UnityEngine.Object[1] { value };
                    DraggedObjectIsOurs = true;
                }
                else DragAndDrop.AcceptDrag();
            }
        }
        bool DraggedObjectIsOurs
        {
            get
            {
                object obj = DragAndDrop.GetGenericData("Prefab Tool");
                if (obj == null) return false;
                return (bool)obj;
            }
            set
            {
                DragAndDrop.SetGenericData("Prefab Tool", value);
            }
        }
        bool IsClick()
        {
            return false;
        }
        #endregion
    }
}
#endif