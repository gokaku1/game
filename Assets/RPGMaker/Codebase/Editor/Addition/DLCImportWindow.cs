using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using RPGMaker.Codebase.Editor.Common.Window;
using RPGMaker.Codebase.Editor.Common.AddonUIUtil;
using System.Linq;

namespace RPGMaker.Codebase.Editor.Addition
{
    public class DLCImportWindow : BaseModalWindow {
        protected override string ModalUxml =>
            "Assets/RPGMaker/Codebase/Editor/Addition/Uxml/dlc_import_window.uxml";

        ListView _listView;
        List<string> _packagePathList;

        [MenuItem("RPG Maker/DLC Import...", priority = 801)]
        private static void DLCImport() {
            ShowWindow();
        }

        private string GetDLCPath() {
            return RPGMaker.CoreSystem.Lib.DLC.BasePath;
        }

        public static void ShowWindow() {
            var wnd = GetWindow<DLCImportWindow>();

            wnd.titleContent = new GUIContent("DLC Import");
            wnd.Init();

            var size = new Vector2(400, 250);
            //wnd.minSize = size;
            //wnd.maxSize = size;
            wnd.maximized = false;
        }

        public override void Init() {
            _packagePathList = GetDlcPackagePaths();

            var root = rootVisualElement;

            // 要素作成
            //----------------------------------------------------------------------
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
            var labelFromUxml = visualTree.CloneTree();
            root.Add(labelFromUxml);

            labelFromUxml.style.flexGrow = 1;
            var list_window = labelFromUxml.Query<VisualElement>($"system_window_list_window").AtIndex(0);
            _listView = CreateListView();
            list_window.Add(_listView);

            // Import DLC、Cancelボタン
            //----------------------------------------------------------------------
            var buttonImport = labelFromUxml.Query<Button>("Import").AtIndex(0);
            buttonImport.style.alignContent = Align.FlexEnd;
            buttonImport.SetEnabled(false);
            buttonImport.clicked += () =>
            {
                foreach (var index in _listView.selectedIndices)
                {
                    ImportDlc(_packagePathList[index]);
                    break;
                }
                Close();
            };
            var buttonCancel = labelFromUxml.Query<Button>("Cancel").AtIndex(0);
            buttonCancel.style.alignContent = Align.FlexEnd;
            buttonCancel.clicked += () =>
            {
                Close();
            };

            _listView.onSelectionChange += (items) =>
            {
                _ = items;
                buttonImport.SetEnabled(true);
            };

        }

        List<string> GetDlcPackagePaths() {
            var dlcPath = GetDLCPath();
            if (!dlcPath.EndsWith('/')){
                dlcPath += "/";
            }
            var list = Directory.GetFiles(dlcPath, "*.unitypackage", SearchOption.AllDirectories).ToList();
            list.Sort();
            return list;
        }

        private void ImportDlc(string packagePath) {
            //Import
            AssetDatabase.ImportPackage(packagePath, true);
        }

        private ListView CreateListView() {
            StripedListView<int> listView = null;
            Action<VisualElement, int> bindItem = (e, i) =>
            {
                e.Clear();
                {
                    var index = (listView.itemsSource as List<int>)[i];
                    VisualElement visualElement = new IndexVisualElement(index);
                    //visualElement.style.flexDirection = FlexDirection.Row;
        
                    listView.SetVisualElementStriped(visualElement, index);
                    if (index == listView.itemsSource.Count - 1) listView.AddVisualElementStriped(e);
        
                    if (index >= 0)
                    {
                        var text = _packagePathList[index];
                        //var match = Regex.Match(text, ".*/([^/]+)/[^/]+\\.unitypackage$");
                        var match = Regex.Match(text, ".*/([^/]+)\\.unitypackage$");
                        if (match.Success){
                            text = match.Groups[1].Value;
                        }
        
                        // Name
                        var label = new Label(text);
                        label.AddToClassList("text_ellipsis");
                        label.AddToClassList("list_view_item_name_label");
                        visualElement.Add(label);
                    }
        
                    e.Add(visualElement);
                }
            };
        
            Func<VisualElement> makeItem = () => new Label();
            listView = new StripedListView<int>(new string[_packagePathList.Count], 16, makeItem, bindItem);
            listView.AddToClassList("list_view");
            listView.SolidQuad();
            listView.name = "list";
            listView.selectionType = SelectionType.Single;
            listView.reorderable = false;
            var list = new List<int>();
            for (int i = 0; i < _packagePathList.Count; i++)
            {
                list.Add(i);
            }
            listView.itemsSource = list;
            return listView;
        }

    }
}
