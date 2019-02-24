#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace EditorClass
{
    [InitializeOnLoad]
    public class MapBlockWindow : Editor
    {
        [MenuItem("场景编辑器/打开编辑器", false, 1)]
        static void OpenEditor()
        {
            if (!EditorApplication.isCompiling)
                OpenSceneEditor();
        }

        [MenuItem("场景编辑器/关闭编辑器", false, 2)]
        static void CloseEditor()
        {
            if (!EditorApplication.isCompiling)
                CloseSceneEditor();
        }


        public static bool _openEditor;
        public static int _barIndex = 0;
        public static Vector3 _cubeSize;
        public static int _sceneSizeX = 30;
        public static int _sceneSizeZ = 30;
        public static int _floorNum = 0;
        public static bool _showGrid = true;

        //场景物体资源路径
        public const string BlocksPrefabsDir = "Assets/Resources/Blocks";

        string[] selectedBtns = new string[] { };

        /// <summary>
        /// 工具菜单选项
        /// </summary>
        public class BlockItem
        {
            public GameObject prefab;
            public Texture texture;
            public bool isToggle;
        }

        public static void ResetBlockItem(int index)
        {
            if (-1 < _indexBlockItem && _indexBlockItem < _blockPage.Count)
            {
                BlockItem b = _blockPage[_indexBlockItem];
                b.isToggle = false;
            }
            _indexBlockItem = index;
        }
        public static BlockItem GetBlockItem(int index)
        {
            if (-1 < index && index < _blockPage.Count)
            {
                BlockItem item = _blockPage[index];
                return item;
            }

            return null;
        }
        public static BlockItem CurrentBlockItem
        {
            get
            {
                if (-1 < _indexBlockItem && _indexBlockItem < _blockPage.Count)
                {
                    BlockItem item = _blockPage[_indexBlockItem];
                    return item;
                }
                return null;
            }
        }
        public static string GetBlockType()
        {
            return _blocksTypes[_blockMenuIndex]; ;
        }

        private static string _sceneName = "";
  
        public static void OpenSceneEditor()
        {
            Selection.activeTransform = null;
            Init();
            _openEditor = true;
            SceneView.RepaintAll();
        }
        public static void CloseSceneEditor()
        {
            Clear();
            _openEditor = false;
            Tools.hidden = false;
            SceneView.RepaintAll();
            Selection.activeTransform = null;
        }

        static void Init()
        {
            Clear();

            MapBlockTool.Init();
            float cubeX = EditorPrefs.GetFloat("BlockCube X", 1f);
            float cubeY = EditorPrefs.GetFloat("BlockCube Y", 1);
            float cubeZ = EditorPrefs.GetFloat("BlockCube Z", 1.75f);
            _cubeSize = new Vector3(cubeX, cubeY, cubeZ);
            _sceneSizeX = EditorPrefs.GetInt("BlockSceneSize X", 30);
            _sceneSizeZ = EditorPrefs.GetInt("BlockSceneSize Z", 30);


            string winPath = EditorUtiliTool.UnityPath2WinAbs(BlocksPrefabsDir);
            string[] dirPaths = Directory.GetDirectories(winPath);
            for (int i = 0; i < dirPaths.Length; i++)
            {
                string blockTypePath = dirPaths[i];
                if (string.IsNullOrEmpty(blockTypePath)) continue;
                string typeName = Path.GetFileName(blockTypePath);
                _blocksTypes.Add(typeName);
                string[] blockPaths = Directory.GetFiles(blockTypePath, "**.prefab", SearchOption.AllDirectories);

                _blockPathsDic.Add(typeName, blockPaths);
            }
            SetBlockMenuData();
        }
        static void Clear()
        {
            MapBlockTool.Clear();

            _blocksTypes.Clear();
            _blockPathsDic.Clear();
            _blocksDic.Clear();
            _blockPage.Clear();
        }

        static MapBlockWindow()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;

            EditorSceneManager.sceneOpened -= SceneOpened;
            EditorSceneManager.sceneOpened += SceneOpened;
            EditorSceneManager.sceneSaved -= SceneSaved;
            EditorSceneManager.sceneSaved += SceneSaved;
        }

        private static void SceneSaved(Scene scene)
        {
            if (EditorApplication.isPlaying) return;
            if (!_openEditor) return;

            Debug.Log("Save Scene " + scene.name);
        }

        private static void SceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (EditorApplication.isPlaying) return;
            if (!_openEditor) return;

            CloseSceneEditor();
            OpenSceneEditor();
        }

        static void OnSceneGUI(SceneView sceneView)
        {
            if (!_openEditor) return;

            Rect sceneRect = sceneView.position;
            Rect boxRect = new Rect(0, sceneRect.height - 200, sceneRect.width, 200);
            Rect infoRect = new Rect(0, 0, 210, 200);
            Rect blocksRect = new Rect(infoRect.width, 0, sceneRect.width - infoRect.width, infoRect.height);
            float barHeight = sceneRect.height - 200 - 3 * EditorGUIUtility.singleLineHeight;
            Rect barStateRect = new Rect(0, barHeight, sceneRect.width, 3 * EditorGUIUtility.singleLineHeight);

            Event current = Event.current;
            bool isInEditor = boxRect.Contains(current.mousePosition);
            if (isInEditor)
            {
                int controlId = GUIUtility.GetControlID(FocusType.Passive);
                HandleUtility.AddDefaultControl(controlId);
            }

            Handles.BeginGUI();

            //编辑器BOX
            GUIStyle style = new GUIStyle(EditorStyles.helpBox);
            style.normal.textColor = Color.black;
            //状态栏
            DrawBarStateUI(barStateRect, isInEditor);
            GUILayout.BeginArea(boxRect, EditorStyles.helpBox);
            {
                //场景信息界面
                DrawSceneInfoUI(infoRect);

                //方块选择界面
                DrawBlockSelectUI(blocksRect);
            }
            GUILayout.EndArea();
            Handles.EndGUI();


            OnKeyBoardDown(sceneView, current);
        }

        static void OnKeyBoardDown(SceneView sceneView, Event current)
        {
            switch (current.keyCode)
            {
                //case KeyCode.F1:
                //    _barIndex = 0;
                //    break;
                case KeyCode.F1:
                    _barIndex = 0;
                    break;
                default:
                    break;
            }
        }

        #region Bar状态控制
        static bool _isPaint = false;
        static void DrawBarStateUI(Rect barStateRect, bool isInEditor)
        {
            GUILayout.BeginArea(barStateRect);
            {
                int index = GUILayout.SelectionGrid(_barIndex, new string[] { /*"移动[F1]",*/ "删除[F1]", "绘制" }, 2, GUI.skin.button, GUILayout.Width(210), GUILayout.Height(barStateRect.height));
                if (index == 1 && _isPaint)
                    _barIndex = 1;
                else if (index != 1)
                {
                    _barIndex = index;
                    _isPaint = false;
                }
                switch (_barIndex)
                {
                    case 0:
                    //    {
                    //        Tools.hidden = isInEditor;
                    //        ResetBlockItem(-1);
                    //        break;
                    //    }
                    //case 1:
                        {//紫色
                            Tools.hidden = true;
                            ResetBlockItem(-1);
                            EditorPrefs.SetFloat("Cube Painter R", 0.862745f);
                            EditorPrefs.SetFloat("Cube Painter G", 0);
                            EditorPrefs.SetFloat("Cube Painter B", 1f);
                            break;
                        }
                    case 1:
                        {//蓝色
                            Tools.hidden = true;
                            EditorPrefs.SetFloat("Cube Painter R", 0f);
                            EditorPrefs.SetFloat("Cube Painter G", 0.1725f);
                            EditorPrefs.SetFloat("Cube Painter B", 1f);
                            break;
                        }
                    default:
                        Tools.hidden = false;
                        _barIndex = 0;
                        break;
                }
            }
            GUILayout.EndArea();
        }
        #endregion
        #region 场景信息界面
        static void DrawSceneInfoUI(Rect infoRect)
        {
            GUILayout.BeginArea(infoRect, EditorStyles.helpBox);
            {
                int saveIndex = EditorPrefs.GetInt("Info Menu Index", 0);
                int select = GUILayout.SelectionGrid(saveIndex, new string[] { "信息", "打开", "新建", "优化" }, 4, EditorStyles.toolbarButton);

                if (select != saveIndex)
                {
                    EditorPrefs.SetInt("Info Menu Index", select);
                }

                //场景小窗口切换逻辑
                switch (select)
                {
                    case 0:
                        SceneInfoArea();
                        break;
                    case 1:
                        break;
                    case 2:
                        break;
                    case 3:
                        break;
                }
            }
            GUILayout.EndArea();
        }

        static void SceneInfoArea()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("场景格子", GUILayout.Width(50));
                if (GUILayout.Button("显示"))
                {
                    _showGrid = true;
                }
                if (GUILayout.Button("隐藏"))
                {
                    _showGrid = false;
                }
            }
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            {
                EditorGUI.BeginChangeCheck();
                GUILayout.Label("格子尺寸", GUILayout.Width(50));
                float cubeSizeX = EditorGUILayout.FloatField(_cubeSize.x);
                float cubeSizeY = EditorGUILayout.FloatField(_cubeSize.y);
                float cubeSizeZ = EditorGUILayout.FloatField(_cubeSize.z);
                cubeSizeX = Mathf.Clamp(cubeSizeX, float.MinValue, float.MaxValue);
                cubeSizeY = Mathf.Clamp(cubeSizeY, float.MinValue, float.MaxValue);
                cubeSizeZ = Mathf.Clamp(cubeSizeZ, float.MinValue, float.MaxValue);
                EditorPrefs.SetFloat("BlockCube X", cubeSizeX);
                EditorPrefs.SetFloat("BlockCube Y", cubeSizeY);
                EditorPrefs.SetFloat("BlockCube Z", cubeSizeZ);
                _cubeSize = new Vector3(cubeSizeX, cubeSizeY, cubeSizeZ);
                if (EditorGUI.EndChangeCheck())
                {
                    MapBlockTool.UpdateBlockPos();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("矩阵规格", GUILayout.Width(50));
                int sceneSizeX = EditorGUILayout.IntField(_sceneSizeX);
                GUILayout.Label("X", EditorStyles.label);
                int sceneSizeZ = EditorGUILayout.IntField(_sceneSizeZ);
                sceneSizeX = Mathf.Clamp(sceneSizeX, int.MinValue, int.MaxValue);
                sceneSizeZ = Mathf.Clamp(sceneSizeZ, int.MinValue, int.MaxValue);
                EditorPrefs.SetInt("BlockSceneSize X", sceneSizeX);
                EditorPrefs.SetInt("BlockSceneSize Z", sceneSizeZ);
                _sceneSizeX = sceneSizeX;
                _sceneSizeZ = sceneSizeZ;
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("层次调节", GUILayout.Width(50));
                int floorNum = EditorGUILayout.IntField(_floorNum);
                _floorNum = Mathf.Clamp(floorNum, 0, 10);
            }
            GUILayout.EndHorizontal();
        }
        #endregion
        #region 方块选择界面
        /// <summary>
        /// 文件夹名
        /// </summary>
        static List<string> _blocksTypes = new List<string>();
        /// <summary>
        /// key - 预制类型; value - 预制路径
        /// </summary>
        static Dictionary<string, string[]> _blockPathsDic = new Dictionary<string, string[]>();
        /// <summary>
        /// key - 预制类型; value - 预制信息
        /// </summary>
        static Dictionary<string, List<BlockItem>> _blocksDic = new Dictionary<string, List<BlockItem>>();
        /// <summary>
        /// 当前显示预制页
        /// </summary>
        static List<BlockItem> _blockPage = new List<BlockItem>();
        static int _blockMenuIndex;
        static float _cellPadding = 4;
        static float _cellSize = 80;
        static Vector2 _scrollPos;
        static int _indexBlockItem = -1;

        static void DrawBlockSelectUI(Rect sceneRect)
        {
            GUILayout.BeginArea(sceneRect, EditorStyles.helpBox);
            {
                int blocksMenuIndex = GUILayout.SelectionGrid(_blockMenuIndex, _blocksTypes.ToArray(), _blocksTypes.Count, EditorStyles.toolbarButton);
                if (blocksMenuIndex != _blockMenuIndex)
                {
                    _blockMenuIndex = blocksMenuIndex;
                    _blockPage.Clear();
                    SetBlockMenuData();
                }

                _scrollPos = GUILayout.BeginScrollView(_scrollPos);
                {
                    float areaWidth = sceneRect.width - _cellPadding - 10;
                    float areaHeight = sceneRect.height;
                    int xNum = Mathf.FloorToInt((areaWidth) / (_cellPadding + _cellSize));
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
                            item.texture = item.texture == null ? GeneratePreview(item.prefab) : item.texture;

                            item.isToggle = GUILayout.Toggle(item.isToggle, new GUIContent(item.texture), GUI.skin.button, GUILayout.Width(_cellSize), GUILayout.Height(_cellSize - 1));
                            if (item.isToggle && _indexBlockItem != amount)
                            {
                                ResetBlockItem(amount);

                                _isPaint = true;
                                _barIndex = 1;
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                }
                GUILayout.EndScrollView();
            }
            GUILayout.EndArea();
        }
        static void SetBlockMenuData()
        {
            if (_blocksTypes.Count == 0) return;
            
            string blockType = _blocksTypes[_blockMenuIndex];
            if (!_blocksDic.ContainsKey(blockType))
            {
                List<BlockItem> ls = new List<BlockItem>();
                string[] blockPaths = _blockPathsDic[blockType];
                for (int i = 0; i < blockPaths.Length; i++)
                {
                    string path = EditorUtiliTool.WinAbs2UnityPath(blockPaths[i]);
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
        static Texture GeneratePreview(GameObject prefab)
        {
            int id = prefab.GetInstanceID();
            Texture tex = AssetPreview.GetAssetPreview(prefab);
            if (AssetPreview.IsLoadingAssetPreview(id)) return null;
            return tex;
        }
        #endregion
    }
}
#endif