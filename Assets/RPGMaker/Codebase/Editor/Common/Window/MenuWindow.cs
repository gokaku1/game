using RPGMaker.Codebase.Editor.Additional;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.Common.Window
{
    public class MenuWindowParams : ScriptableSingleton<MenuWindowParams>
    {
        public bool IsRpgEditorPlayMode;
    }

    /// <summary>
    /// メニュー用のWindow
    /// </summary>
    public class MenuWindow : BaseWindow
    {
        public enum BtnType
        {
            New = 1,
            Open,
            Save,
            Cat,
            Paste,
            Map,
            Back,
            Event,

            Pen = 15,
            Rectangle,
            Ellipse,
            Fill,
            Shadow,
            ZoomIn,
            ZoomOut,
            ActualSize,

            Addon = 24,
            SoundTest,
            EventSearch,
            Material,

            Play = 29,
            Close,
            Stop = 31,

            Deploy,
            App,

            //Play,
            Store,
            Package,

            Search,

            // 消しゴム
            Eraser = 33,

            HierarchyHistory = 37,
            Help,
        }

        private MenuEditorView _menuEditorView;

        public void Init() {
            //表示されていたら、UniteのイベントのWindowを閉じる
            WindowLayoutManager.CloseWindow(WindowLayoutManager.WindowLayoutId.MapEventExecutionContentsWindow);
            WindowLayoutManager.CloseWindow(WindowLayoutManager.WindowLayoutId.MapEventCommandSettingWindow);

            rootVisualElement.Clear();
            maxSize = new Vector2(600f, 100f);
            minSize = new Vector2(370f, 100f);

            var root = rootVisualElement;
            _menuEditorView = new MenuEditorView(this);
            root.Add(_menuEditorView);
        }

        public void Select(int btnIndex) {
            switch ((BtnType) btnIndex)
            {
                case BtnType.Deploy:
                    ProjectDeploy();
                    break;
                case BtnType.App:
                case BtnType.Pen:
                    ProjectApp();
                    break;
                case BtnType.Play:
                    TestPlay();
                    break;
                case BtnType.Stop:
                    TestPlay();
                    break;
                case BtnType.Store:
                    AssetStoreOpen();
                    break;
                case BtnType.Package:
                    PackageManagerOpen();
                    break;
                case BtnType.Addon:
                    ShowAddonList();
                    break;

                case BtnType.HierarchyHistory:
                    HierarchyHistoryWindow.ShowHierarchyHistoryWindow();
                    break;

                case BtnType.Search:
                    EventSearcher_Main.ShowEventSearcher();
                    break;
                case BtnType.Help:
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1503")), false, () => { MenuEditorView.Help(); });
                menu.AddItem(new GUIContent(EditorLocalize.LocalizeText("WORD_1507")), false, () => { MenuEditorView.About(); });
                menu.ShowAsContext();
                    break;
            }
        }

        /// <summary>
        /// ボタンの状態更新
        /// </summary>
        /// <param name="flg"></param>
        public void SetButtonEnabled(bool flg) {
            var playButton = _menuEditorView.GetIconImage((int)BtnType.Play);
            var penButton = _menuEditorView.GetIconImage((int)BtnType.Pen);
            playButton?.SetEnabled(flg);
            penButton?.SetEnabled(flg);
        }
        
        /// <summary>
        /// プロジェクトの保存
        /// </summary>
        private void ProjectSave() {
            EditorApplication.ExecuteMenuItem("File/Save");

            AnalyticsManager.Instance.PostEvent(
                AnalyticsManager.EventName.action,
                AnalyticsManager.EventParameter.save);
        }

        /// <summary>
        /// プロジェクトのデプロイ
        /// </summary>
        private void ProjectDeploy() {
#if UNITY_6000_0_OR_NEWER
            EditorApplication.ExecuteMenuItem("File/Build Profiles");
#else
            EditorApplication.ExecuteMenuItem("File/Build Settings...");
#endif
        }

        /// <summary>
        /// ゲームをアプリ化
        /// </summary>
        private void ProjectApp() {
            //現在の設定値をBuildSettingsに反映
            var databaseManagementService = Editor.Hierarchy.Hierarchy.databaseManagementService;
            var systemSettingDataModel = databaseManagementService.LoadSystem();

            var screenWidth = systemSettingDataModel.DisplaySize[systemSettingDataModel.displaySize].x;
            var screenHeight = systemSettingDataModel.DisplaySize[systemSettingDataModel.displaySize].y;

            PlayerSettings.defaultScreenWidth = screenWidth;
            PlayerSettings.defaultScreenHeight = screenHeight;
            PlayerSettings.defaultWebScreenWidth = screenWidth;
            PlayerSettings.defaultWebScreenHeight = screenHeight;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
            PlayerSettings.allowFullscreenSwitch = false;

            //今はBuild Settingsを開く
#if UNITY_6000_0_OR_NEWER
            EditorApplication.ExecuteMenuItem("File/Build Profiles");
#else
            EditorApplication.ExecuteMenuItem("File/Build Settings...");
#endif
            AnalyticsManager.Instance.PostEvent(
                AnalyticsManager.EventName.action,
                AnalyticsManager.EventParameter.deploy);
        }

        /// <summary>
        /// アドオンリストの呼び出し
        /// </summary>
        private void ShowAddonList() {
            var addonListModalWindow = new AddonListModalWindow();
            addonListModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Add-on List"), data => { });
        }

        private void TestPlay() {
            if (!EditorApplication.isPlaying)
            {
                RpgMakerEditor.UpdateLastActiveMenuEvent();
                if (RpgMakerEditor.IsImportEffekseer)
                {
                    EditorUtility.RequestScriptReload();
                    RpgMakerEditor.IsImportEffekseer = false;
                }

                //ウィンドウが閉じられる前にUIイベントが処理できるよう1フレーム待ちを入れる。
                System.Action delayFunc = async () =>
                {
                    await Task.Delay(1);
                    //ヒエラルキーのイベントを選択していないとき
                    WindowLayoutManager.CloseWindows(new List<WindowLayoutManager.WindowLayoutId>()
                    {
                        WindowLayoutManager.WindowLayoutId.MapEventExecutionContentsWindow,
                        WindowLayoutManager.WindowLayoutId.MapEventRouteWindow,
                        WindowLayoutManager.WindowLayoutId.MapEventCommandSettingWindow,
                        WindowLayoutManager.WindowLayoutId.MapEventMoveCommandWindow
                    });

                    MenuWindowParams.instance.IsRpgEditorPlayMode = true;
                    EditorSceneManager.OpenScene("Assets/RPGMaker/Codebase/Runtime/Title/Title.unity");
                    Hierarchy.Hierarchy.SelectButton("title_button");
                    EditorApplication.isPlaying = true;
                };
                delayFunc.Invoke();
            }
            else
            {
                EditorApplication.isPlaying = false;
                MenuWindowParams.instance.IsRpgEditorPlayMode = false;
            }

            AnalyticsManager.Instance.PostEvent(
                AnalyticsManager.EventName.action,
                AnalyticsManager.EventParameter.testplay);
        }

        private void TestStop() {
        }

        /// <summary>
        /// AssetStoreを開く
        /// </summary>
        private void AssetStoreOpen() {
            EditorApplication.ExecuteMenuItem("Window/Asset Store");
        }

        /// <summary>
        /// PackageManagerを開く
        /// </summary>
        private void PackageManagerOpen() {
            EditorApplication.ExecuteMenuItem("Window/Package Manager");
        }

    }
}