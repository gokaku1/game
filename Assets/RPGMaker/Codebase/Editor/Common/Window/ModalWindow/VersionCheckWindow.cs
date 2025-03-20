using RPGMaker.Codebase.CoreSystem.Helper;
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Common.Window.ModalWindow
{
    public class VersionCheckWindow : BaseModalWindow
    {
        // 表示文字列
        private const string CopyRight = "©Gotcha Gotcha Games Inc.";

        //画像パス
        private const string PicturePath = "Assets/RPGMaker/Storage/System/Images/About/AboutLogo_Lsize.png";
        private Label _copyRightLabel;
        private VisualElement _pistureArea;

        //表示要素
        private Label _versionNumberLabel;

        protected override string ModalUxml => "Assets/RPGMaker/Codebase/Editor/Common/Window/ModalWindow/Uxml/version_check_window.uxml";

        public void ShowWindow() {
            var wnd = GetWindow<VersionCheckWindow>();

            // 処理タイトル名適用
            wnd.titleContent = new GUIContent(EditorLocalize.LocalizeText("WORD_2005"));
            wnd.Init();
            //サイズ固定用
            var size = new Vector2(280, 250);
            wnd.minSize = size;
            wnd.maxSize = size;
            wnd.maximized = false;
        }
        class UniteVersionJson
        {
            public string version = "";
        }

        public override void Init() {
            var unit_version_json = "RPGMaker/UNITE.json";
            var json = "{}";
            try
            {
                var path = Path.Combine(Application.dataPath, unit_version_json);
                json = File.ReadAllText(path);
            }
            catch (Exception)
            {
            }
            var version = JsonHelper.FromJson<UniteVersionJson>(json);

            var root = rootVisualElement;
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ModalUxml);
            VisualElement labelFromUxml = visualTree.CloneTree();
            EditorLocalize.LocalizeElements(labelFromUxml);
            root.Add(labelFromUxml);

            //element取得
            _versionNumberLabel = labelFromUxml.Query<Label>("version_number");
            _copyRightLabel = labelFromUxml.Query<Label>("copy_right");
            _pistureArea = labelFromUxml.Query<VisualElement>("picture");

            //情報の表示
            _versionNumberLabel.text = $"Version {version.version} : {Application.unityVersion}";
            _copyRightLabel.text = EditorLocalize.LocalizeText(CopyRight);

            //ロゴの表示
            var logo = new Image();
            logo.image = AssetDatabase.LoadAssetAtPath<Texture>(PicturePath);
            logo.style.width = logo.image.width;
            logo.style.height = logo.image.height;
            _pistureArea.Add(logo);
        }
    }
}