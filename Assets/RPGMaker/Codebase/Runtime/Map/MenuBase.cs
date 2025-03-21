using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using RPGMaker.Codebase.Runtime.Map.Menu;
using RPGMaker.Codebase.Runtime.Title;
using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace RPGMaker.Codebase.Runtime.Map
{
    /// <summary>
    /// メニューの管理クラス
    /// </summary>
    public class MenuBase : WindowBase
    {
        //Initで呼び出すObjectName
        public static string ObjName = "_menuIcon";

        public static bool IsEventToSave = false;

        // 背景
        private Canvas _blurCanvas;
        private GameObject _background;
        [SerializeField] private Image _backgroundImage;
        private MenuBlurRendererFeature _menuBlurRendererFeature;

        // メニューを開けるかのフラグ
        private                  bool        _canMenuOpen = true;
        [SerializeField] private GameObject  _equipObject;
        [SerializeField] private GameObject  _gameEndObject = null;
        [SerializeField] private GameObject  _itemObject = null;
        [SerializeField] private GameObject  _mainObject = null;

        //右上のアイコン
        [SerializeField] private GameObject  _menuIcon     = null;
        [SerializeField] private GameObject  _optionObject = null;

        private GameObject _menuIconDefault = null;
        private GameObject _menuIconBack = null;

        private PartyWindow _partyWindow;
        [SerializeField] private GameObject  _saveObject  = null;
        [SerializeField] private GameObject  _skillObject = null;
        [SerializeField] private GameObject  _sortObject  = null;
        [SerializeField] private GameObject  _statusObject;

        //_uiPatternIdの保持
        private string    _uiPatternId;
        public  EquipMenu EquipMenu;
        public  ItemMenu  ItemMenu;

        public MainMenu   MainMenu;
        public SkillMenu  SkillMenu;
        public SortMenu   SortMenu;
        public StatusMenu StatusMenu;

        public bool MenuWillOpen = false;

        /// <summary>
        /// メニューが開けるかどうかの設定を行う
        /// </summary>
        /// <param name="canOpen"></param>
        public void CanMenuOpen(bool canOpen) {
            _canMenuOpen = canOpen;
            _menuIcon.gameObject.SetActive(canOpen);
        }

        /// <summary>
        /// セーブ画面が開けるかどうかの更新
        /// </summary>
        public void CanSave() {
            _mainObject.GetComponent<MainMenu>().CanSave();
        }

        /// <summary>
        /// ソート画面が開けるかどうかの更新
        /// </summary>
        public void CanSort() {
            _mainObject.GetComponent<MainMenu>().CanSort();
        }

        protected void Start() {
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
#else
            StartAsync();
        }
        private async void StartAsync() {
#endif
            IsEventToSave = false;
            MenuManager.MenuBase = this;
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
#else
            await DataManager.WaitForEndOfLoad();
#endif

            // レイヤーを最上位に来るよう対応
            gameObject.GetComponent<Canvas>().sortingOrder = 1000;

            MainMenu = _mainObject.GetComponent<MainMenu>();
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            MainMenu.Init(this);
#else
            await MainMenu.Init(this);
#endif

            //共通UIの適応を開始
            GetComponent<WindowBase>().Init();

            if (!Commons.IsURP())
            {

                _blurCanvas = transform.parent.Find("Canvas").GetComponent<Canvas>();
                _blurCanvas.worldCamera = MapManager.GetCamera();
                _blurCanvas.sortingLayerID =
                    UnityUtil.SortingLayerManager.GetId(UnityUtil.SortingLayerManager.SortingLayerIndex.Runtime_Weather);

                _background = transform.parent.Find("Canvas/BackGround").gameObject;
            } else
            {
                (_, var rendererFeatures) = Commons.GetUniteRendererDataFeatues();
                //Debug.Log($"rendererFeatures.Count: {rendererFeatures.Count}, {string.Join("/", rendererFeatures.Select(x => $"{x}"))}");
                var feature = rendererFeatures[rendererFeatures.Count - Commons.MenuBlurFeatureBottomOffset] as MenuBlurRendererFeature;
                _menuBlurRendererFeature = feature;
                _menuBlurRendererFeature.SetActive(false);
            }
            ShowMenuBlur(false);

            // 背景画像の設定
            if (_backgroundImage == null) _backgroundImage = transform.parent.Find("UICanvas/BgImage").gameObject.GetComponent<Image>();

            // メニューボタン（決め打ち）
            _menuIcon.transform.Find("Icon").transform.GetComponent<Image>().sprite =
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(UI_PATH + "uigw_butt_004_dark" + ".png");
#else
                await UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(UI_PATH + "uigw_butt_004_dark" + ".png");
#endif

            _menuIconDefault = _menuIcon.transform.Find("Icon").gameObject;
            _menuIconBack = _menuIcon.transform.Find("IconBack").gameObject;

            //メニュー内のオブジェクトを全てfalseに
            foreach (Transform child in gameObject.transform) child.gameObject.SetActive(false);
            //最初のwindowの表示
            var InitObject = ObjectNameToGameObject(ObjName);
            if (InitObject != null)
                InitObject.SetActive(true);

            _partyWindow = transform.Find("PartyWindow").GetComponent<PartyWindow>();
            _partyWindow.gameObject.SetActive(false);
            _saveObject.gameObject.SetActive(false);

            if (MapManager.menu == null) MapManager.menu = this;

            gameObject.AddComponent<CanvasResolutionManager>();

            //メニュー表示・非表示切り替え
            CanMenuOpen(DataManager.Self().GetRuntimeSaveDataModel().runtimeSystemConfig.menuEnabled == 1);
            CanSave();
            CanSort();
        }

        /// <summary>
        /// Update処理
        /// </summary>
        public void LateUpdate() {
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
#else
            if (DataManager.IsLoading()) return;
            _ = LateUpdateAsync();
        }
        private async Task LateUpdateAsync() {
#endif
            if (_canMenuOpen)
            {
                if (GameStateHandler.CurrentGameState() == GameStateHandler.GameState.MAP)
                {
                    //マップ表示中
                    _menuIcon.gameObject.SetActive(true);
                }
                else if (GameStateHandler.CurrentGameState() == GameStateHandler.GameState.MENU && _mainObject.activeSelf)
                {
                    //メインメニュー表示中
                    _menuIcon.gameObject.SetActive(true);
                }
                else
                {
                    //その他のあらゆる状態
                    _menuIcon.gameObject.SetActive(false);
                }
            }
            else
            {
                //メニュー自体が禁止の場合
                _menuIcon.gameObject.SetActive(false);
            }

            if (_canMenuOpen || IsEventToSave)
            {
                if (InputHandler.OnDown(Common.Enum.HandleType.Back) || InputHandler.OnDown(Common.Enum.HandleType.RightClick))
                {
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                    BackMenu();
#else
                    await BackMenuAsync();
#endif
                    IsEventToSave = false;
                }
            }

            if (MenuManager.IsMenuActive)
            {
                if (InputHandler.OnDown(Common.Enum.HandleType.PageLeft))
                {
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                    ChangePage(false);
#else
                    await ChangePage(false);
#endif
                }
                else if (InputHandler.OnDown(Common.Enum.HandleType.PageRight))
                {
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                    ChangePage(true);
#else
                    await ChangePage(true);
#endif
                }
            }
        }

        /// <summary>
        /// stringで入ってきたobjectの名前をそのobjectにして返却する
        /// </summary>
        /// <param name="objectName"></param>
        /// <returns></returns>
        private GameObject ObjectNameToGameObject(string objectName) {
            GameObject returnObject;
            switch (objectName)
            {
                case "_mainObject":
                    returnObject = _mainObject;
                    _menuIcon.SetActive(true);
                    break;
                case "_itemObject":
                    returnObject = _itemObject;
                    break;
                case "_skillObject":
                    returnObject = _skillObject;
                    break;
                case "_equipObject":
                    returnObject = _equipObject;
                    break;
                case "_statusObject":
                    returnObject = _statusObject;
                    break;
                case "_optionObject":
                    returnObject = _optionObject;
                    break;
                case "_sortObject":
                    returnObject = _sortObject;
                    break;
                case "_saveObject":
                    returnObject = _saveObject;
                    break;
                case "_gameEndObject":
                    returnObject = _gameEndObject;
                    break;
                case "_menuIcon":
                    returnObject = _menuIcon;
                    break;
                //該当のものが無かった場合メインを返す
                default:
                    returnObject = null;
                    break;
            }

            return returnObject;
        }

        /// <summary>
        /// メニュー表示、非表示切り替え処理
        /// </summary>
        /// <param name="isEvent"></param>
        public void MenuOpen(bool isEvent = false) {
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
#else
            _ = MenuOpenAsync(isEvent);
        }
        public async Task MenuOpenAsync(bool isEvent = false) {
#endif
            //マップ、メニュー、イベントかつ isEvent = true のケース以外は抑制する
            if (!(GameStateHandler.CurrentGameState() == GameStateHandler.GameState.MAP ||
                  GameStateHandler.CurrentGameState() == GameStateHandler.GameState.MENU ||
                  GameStateHandler.CurrentGameState() == GameStateHandler.GameState.EVENT && isEvent))
            {
                return;
            }

            //セーブ画面が開いている際は動かないようにする
            if (!_saveObject.activeSelf)
            {
                //現在メニューを閉じており、メニューが有効である
                //またはイベントからの要求
                if (!_mainObject.activeSelf && _canMenuOpen || isEvent)
                {
                    // イベント中は抑制
                    if (isEvent != true && MapEventExecutionController.Instance.CheckRunningEvent())
                    {
                        return;
                    }

                    // 現在歩行中であれば、メニューを開く予定であるとしてフラグを立てる
                    if (MapManager.IsCharacterMove())
                    {
                        MenuWillOpen = true;
                        return;
                    }

                    SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.ok);
                    SoundManager.Self().PlaySe();

                    //状態の更新
                    MapEventExecutionController.Instance.PauseEvent();
                    ChangeGameState(GameStateHandler.GameState.MENU);

                    MenuManager.IsMenuActive = true;
                    if (_blurCanvas != null && _blurCanvas.worldCamera == null)
                    {
                        _blurCanvas.worldCamera = MapManager.GetCamera();
                        _blurCanvas.sortingLayerID =
                            UnityUtil.SortingLayerManager.GetId(UnityUtil.SortingLayerManager.SortingLayerIndex.Runtime_Weather);
                    }
                    _mainObject.SetActive(true);
                    ShowMenuBlur(true);
                    _backgroundImage.gameObject.SetActive(true);
                    _menuIconDefault.SetActive(false);
                    _menuIconBack.SetActive(true);
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                    MainMenu.UpdateStatus();
#else
                    await MainMenu.UpdateStatus();
#endif
                }
                else
                {
                    SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.cancel);
                    SoundManager.Self().PlaySe();

                    if (MapEventExecutionController.Instance.CheckRunningEvent()) ResumeEvent();

                    //状態の更新
                    MapEventExecutionController.Instance.ResumeEvent();
                    ChangeGameState(GameStateHandler.GameState.MAP);
                    MenuManager.IsMenuActive = false;
                    MenuManager.dateTime = DateTime.Now;
                    _mainObject.SetActive(false);
                    _backgroundImage.gameObject.SetActive(false);
                    ShowMenuBlur(false);
                    _menuIconDefault.SetActive(true);
                    _menuIconBack.SetActive(false);
                }
            }
        }

        /// <summary>
        /// メニュー以下を全部非表示にする
        /// </summary>
        /// <param name="iconDisplay">アイコンも非表示にする場合にtrue</param>
        public void MenuClose(bool iconDisplay) {
            //状態の更新
            MapEventExecutionController.Instance.ResumeEvent();
            ChangeGameState(GameStateHandler.GameState.MAP);
            MenuManager.IsMenuActive = false;

            _mainObject.SetActive(false);
            _itemObject.SetActive(false);
            _skillObject.SetActive(false);
            _equipObject.SetActive(false);
            _statusObject.SetActive(false);
            _optionObject.SetActive(false);
            _saveObject.SetActive(false);
            _gameEndObject.SetActive(false);
            _sortObject.SetActive(false);
            _menuIcon.SetActive(iconDisplay);
            _backgroundImage.gameObject.SetActive(false);
        }
        
        /// <summary>
        /// メニューを全て閉じる
        /// </summary>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void AllClose() {
#else
        public async Task AllClose() {
#endif
            //状態の更新
            MapEventExecutionController.Instance.ResumeEvent();
            ChangeGameState(GameStateHandler.GameState.MAP);
            MenuManager.IsMenuActive = false;
            MenuManager.dateTime = DateTime.Now;
            
            if (_partyWindow.gameObject.activeSelf)
            {
                _partyWindow.Back();
            }
            
            if (EquipMenu != null && EquipMenu.GetWindowStatus() != EquipMenu.Window.STATUS)
            {
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                EquipMenu.BackMenu();
#else
                await EquipMenu.BackMenuAsync();
#endif
            }
            if (SkillMenu != null && SkillMenu.WindowType != SkillMenu.Window.SkillType)
            {
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                SkillMenu.Back();
#else
                await SkillMenu.BackAsync();
#endif
            }
            if (ItemMenu != null && ItemMenu.State != ItemMenu.Window.ItemType)
            {
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                ItemMenu.Back();
#else
                await ItemMenu.BackAsync();
#endif
            }
            MainMenu.BackMenu();
            ShowMenuBlur(false);
            _mainObject.SetActive(false);
            _itemObject.SetActive(false);
            _skillObject.SetActive(false);
            _equipObject.SetActive(false);
            _statusObject.SetActive(false);
            _optionObject.SetActive(false);
            _saveObject.SetActive(false);
            _gameEndObject.SetActive(false);
            _sortObject.SetActive(false);
            _menuIcon.SetActive(false);
            _menuIconDefault.SetActive(true);
            _menuIconBack.SetActive(false);
            _backgroundImage.gameObject.SetActive(false);
        }

        private void ShowMenuBlur(bool show) {
            if (show)
            {
                if (Commons.IsURP())
                {
                    _menuBlurRendererFeature.SetActive(true);
                }
                else
                {
                    _background.SetActive(true);
                }
            }
            else
            {
                if (Commons.IsURP())
                {
                    _menuBlurRendererFeature.SetActive(false);
                }
                else
                {
                    _background.SetActive(false);
                }
            }
        }

        /// <summary>
        /// メニューアイコンを非表示にする
        /// </summary>
        /// <param name="iconDisplay"></param>
        public void MenuHidden(bool iconDisplay) {
            _menuIcon.SetActive(iconDisplay);
        }

        /// <summary>
        /// イベントに復帰する
        /// </summary>
        private async void ResumeEvent() {
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            await Task.Delay(500);
#else
            await UniteTask.Delay(500);
#endif
            MapEventExecutionController.Instance.ResumeEvent();
        }

        /// <summary>
        /// アイテムを表示する
        /// </summary>
        public void ItemOpen() {
            _mainObject.SetActive(false);
            ItemMenu = _itemObject.GetComponent<ItemMenu>();
            ItemMenu.Init(this);
            _itemObject.SetActive(true);
        }

        /// <summary>
        /// スキルを表示する
        /// </summary>
        /// <param name="actorId"></param>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void SkillOpen(string actorId) {
#else
        public async Task SkillOpen(string actorId) {
#endif
            _mainObject.SetActive(false);
            _skillObject.SetActive(true);
            SkillMenu = _skillObject.GetComponent<SkillMenu>();
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            SkillMenu.Init(this, actorId);
#else
            await SkillMenu.Init(this, actorId);
#endif
        }

        /// <summary>
        /// 装備を表示する
        /// </summary>
        /// <param name="actorId"></param>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void EquipmentOpen(string actorId) {
#else
        public async Task EquipmentOpen(string actorId) {
#endif
            if (gameObject.transform.Find(_equipObject.name + _uiPatternId) != null)
                _equipObject = gameObject.transform.Find(_equipObject.name + _uiPatternId).gameObject;

            _mainObject.SetActive(false);
            _equipObject.SetActive(true);
            EquipMenu = _equipObject.GetComponent<EquipMenu>();
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            EquipMenu.Init(this, actorId);
#else
            await EquipMenu.Init(this, actorId);
#endif
        }

        /// <summary>
        /// ステータスを表示する
        /// </summary>
        /// <param name="actorId"></param>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void StatusOpen(string actorId) {
#else
        public async Task StatusOpen(string actorId) {
#endif
            if (gameObject.transform.Find(_statusObject.name + _uiPatternId) != null)
                _statusObject = gameObject.transform.Find(_statusObject.name + _uiPatternId).gameObject;

            _mainObject.SetActive(false);
            _statusObject.SetActive(true);
            StatusMenu = _statusObject.GetComponent<StatusMenu>();
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            StatusMenu.Init(this, actorId);
#else
            await StatusMenu.Init(this, actorId);
#endif
        }

        /// <summary>
        /// 並べ替えを表示する
        /// </summary>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void SortOpen() {
#else
        public async Task SortOpen() {
#endif
            _mainObject.SetActive(false);
            _sortObject.SetActive(true);
            SortMenu = _sortObject.GetComponent<SortMenu>();
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            SortMenu.Init(this);
#else
            await SortMenu.Init(this);
#endif
        }

        /// <summary>
        /// オプションを表示する
        /// </summary>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void OptionOpen() {
#else
        public async Task OptionOpen() {
#endif
            _mainObject.SetActive(false);
            _optionObject.SetActive(true);
            _optionObject.GetComponent<OptionController>().Init(this);
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            _optionObject.GetComponent<OptionController>().OpenOption();
#else
            await _optionObject.GetComponent<OptionController>().OpenOption();
#endif
        }

        /// <summary>
        /// セーブを表示する
        /// </summary>
        public void SaveOpen() {
            _mainObject.SetActive(false);
            _saveObject.SetActive(true);
            _saveObject.GetComponent<SaveMenu>().Init(this);
        }

        public void SaveOpenToEvent() {
            ChangeGameState(GameStateHandler.GameState.MENU);

            MenuManager.IsMenuActive = true;
            _mainObject.SetActive(false);
            _saveObject.SetActive(true);
            _saveObject.GetComponent<SaveMenu>().Init(this);
        }

        /// <summary>
        /// ゲーム終了を表示する
        /// </summary>
        public void EndOpen() {
            _mainObject.SetActive(false);
            _gameEndObject.SetActive(true);
            _gameEndObject.GetComponent<GameEndMenu>().Init(this);
            _gameEndObject.GetComponent<GameEndMenu>().OpenEnd();
        }

        /// <summary>
        /// ゲームを終了する
        /// </summary>
        public void EndGame() {
            MenuManager.IsMenuActive = false;
            SceneManager.LoadScene("Title");
        }

        /// <summary>
        /// 前ページ、次ページ処理
        /// </summary>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void ChangePage(bool isNext) {
#else
        public async Task ChangePage(bool isNext) {
#endif
            if (_statusObject.activeSelf)
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                _statusObject.GetComponent<StatusMenu>().changeCharactor(isNext == true ? "plus" : "minus");
#else
                await _statusObject.GetComponent<StatusMenu>().changeCharactorAsync(isNext == true ? "plus" : "minus");
#endif
            else if (_equipObject.activeSelf)
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                _equipObject.GetComponent<EquipMenu>().CharacterChange(isNext == true ? "plus" : "minus");
#else
                await _equipObject.GetComponent<EquipMenu>().CharacterChangeAsync(isNext == true ? "plus" : "minus");
#endif
            else if (_skillObject.activeSelf)
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                _skillObject.GetComponent<SkillMenu>().CharacterChange(isNext);
#else
                await _skillObject.GetComponent<SkillMenu>().CharacterChangeAsync(isNext);
#endif
        }

        /// <summary>
        /// 各メニューの戻る操作を行う
        /// </summary>
        public void BackMenu() {
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
#else
            _ = BackMenuAsync();
        }
        public async Task BackMenuAsync() {
#endif
            if (MenuManager.IsShopActive || MenuManager.IsEndGameToTitle)
            {
                return;
            }
            if (_partyWindow != null && _partyWindow.gameObject.activeSelf)
            {
                SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.cancel);
                SoundManager.Self().PlaySe();
                _partyWindow.Back();
            }
            else if (_mainObject != null && !_mainObject.activeSelf && MenuManager.IsMenuActive)
            {
                if (EquipMenu != null && EquipMenu.GetWindowStatus() != EquipMenu.Window.STATUS)
                {
                    SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.cancel);
                    SoundManager.Self().PlaySe();
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                    EquipMenu.BackMenu();
#else
                    await EquipMenu.BackMenuAsync();
#endif
                }
                else if (SkillMenu != null && SkillMenu.WindowType != SkillMenu.Window.SkillType)
                {
                    SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.cancel);
                    SoundManager.Self().PlaySe();
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                    SkillMenu.Back();
#else
                    await SkillMenu.BackAsync();
#endif
                }
                else if (ItemMenu != null && ItemMenu.State != ItemMenu.Window.ItemType)
                {
                    SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.cancel);
                    SoundManager.Self().PlaySe();
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                    ItemMenu.Back();
#else
                    await ItemMenu.BackAsync();
#endif
                }
                else if (IsEventToSave)
                {
                    IsEventToSave = false;
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                    AllClose();
#else
                    await AllClose();
#endif
                }
                else
                {
                    SoundManager.Self().SetData(SoundManager.SYSTEM_AUDIO_SE, systemSettingDataModel.soundSetting.cancel);
                    SoundManager.Self().PlaySe();
                    _mainObject.SetActive(true);
                    _itemObject.SetActive(false);
                    _skillObject.SetActive(false);
                    _equipObject.SetActive(false);
                    _statusObject.SetActive(false);
                    _optionObject.SetActive(false);
                    _saveObject.SetActive(false);
                    _gameEndObject.SetActive(false);
                    _sortObject.SetActive(false);
                    MainMenu.BackMenu();
                }
            }
            else if (_mainObject != null && _mainObject.activeSelf && MenuManager.IsMenuActive)
            {
                if (MainMenu.BackWindow())
                {
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                    MenuOpen();
#else
                    await MenuOpenAsync();
#endif
                }
            }
            else
            {
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                MenuOpen();
#else
                await MenuOpenAsync();
#endif
            }
        }

        /// <summary>
        /// 各メニューのステータス表示を更新する
        /// </summary>
        public void AllUpdateStatus() {
            MainMenu?.UpdateStatus();
            ItemMenu?.UpdateStatus();
            SkillMenu?.UpdateStatus();
        }

        /// <summary>
        /// パーティメンバー選択Windowを表示する
        /// </summary>
        /// <param name="type"></param>
        /// <param name="useActorId"></param>
        /// <param name="useId"></param>
        /// <param name="gameItem"></param>
        /// <param name="callback"></param>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void OpenPartyWindow(PartyWindow.PartyType type, string useActorId, string useId, GameItem gameItem, Action callback) {
#else
        public async Task OpenPartyWindow(PartyWindow.PartyType type, string useActorId, string useId, GameItem gameItem, Action callback) {
#endif
            //表示させるパーティウィンドウの変更用
            var partyWindowObject = _partyWindow.gameObject;

            if (gameObject.transform.Find(_partyWindow.name + _uiPatternId) != null)
                partyWindowObject = gameObject.transform.Find(_partyWindow.name + _uiPatternId).gameObject;

            partyWindowObject.SetActive(true);
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            partyWindowObject.transform.GetComponent<PartyWindow>().Init(type, useActorId, useId, gameItem, callback);
#else
            await partyWindowObject.transform.GetComponent<PartyWindow>().Init(type, useActorId, useId, gameItem, callback);
#endif
        }

        /// <summary>
        /// GameState切り替え処理
        /// </summary>
        private async void ChangeGameState(GameStateHandler.GameState state) {
            GameStateHandler.SetGameState(state);
            //次の入力までの間にDelayを設ける 1/60単位でキー受付するのは早すぎるため
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            await Task.Delay(100);
#else
            await UniteTask.Delay(100);
#endif
        }
    }
}