#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace EditorClass
{
    [System.Serializable]
    public class SPEditorSelectionGrid : SPEditorComBase
    {
        public SPEditorSelectionGrid(bool isMultiSelected, List<string> contents)
        {
            _isMultiSelected = isMultiSelected;
            _contents = contents;
        }

        public bool _isMultiSelected = false;
        public int SingleIndex { get { return _indexs[0]; } }
        public List<int> MultiIndex { get { return _indexs; } }

        private List<int> _indexs = new List<int>();
        private List<string> _contents = new List<string>();

        public virtual void Draw()
        {
            GUILayout.BeginArea(_rect);
            if (_isMultiSelected)
            {

            }
            else
            {
                _indexs[0] = GUILayout.SelectionGrid(_indexs[0], _contents.ToArray(), _contents.Count, base._style);
            }
            GUILayout.EndArea();
        }
    }
}
#endif