using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.Editor.Common;
using RPGMaker.Codebase.Editor.Common.View;
using RPGMaker.Codebase.Editor.MapEditor.Component.Canvas;
using RPGMaker.Codebase.Editor.MapEditor.Window.MapEdit;
using System;
using System.Collections.Generic;
using System.IO;
using RPGMaker.Codebase.Editor.DatabaseEditor.ModalWindow;
using RPGMaker.Codebase.Runtime.Map;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Inspector.Map.View
{
    /// <summary>
    /// [マップ設定]-[マップリスト]-[各マップ]-[マップ編集]-[遠景] Inspector
    /// </summary>
    public class DistantViewInspector : AbstractInspectorElement
    {
        protected override string MainUxml { get { return "Assets/RPGMaker/Codebase/Editor/Inspector/Map/Asset/distant_view.uxml"; } }

        private readonly MapDataModel         _mapEntity;
        private readonly Action<MapDataModel> _onChange;
        private readonly float                SLIDER_MAX_VALUE = 32;
        private readonly float                SLIDER_MIN_VALUE = -32;

        // "エディター表示"トグル。
        private Toggle _displayToggle;

        //遠景の画像リストの保持
        private List<string> _distantPicture;

        //画像設定のプルダウン
        private PopupFieldBase<string> _distantViewDropdown;

        private Dictionary<string, string> _distantViewImageDictionary;

        private IntegerField _horizontalIntegerField;
        private Slider       _horizontalSlider;

        //横ループの部分
        private Toggle _horizontalToggle;

        //更新用
        private MapEditWindow _mapEditWindow =
            WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.MapEditWindow) as MapEditWindow;

        private IntegerField _verticalIntegerField;
        private Slider       _verticalSlider;

        //縦ループの部分
        private Toggle _verticalToggle;

        //画像の拡大の部分
        private RadioButton _zoom0Toggle;
        private RadioButton _zoom2Toggle;
        private RadioButton _zoom4Toggle;

        public DistantViewInspector(MapDataModel mapEntity, Action<MapDataModel> onChange) {
            _mapEntity = mapEntity;
            _onChange = onChange;

            Initialize();
        }

        protected override void RefreshContents() {
            base.RefreshContents();
            Initialize();
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        override protected void InitializeContents() {
            base.InitializeContents();

            _horizontalToggle = RootContainer.Query<Toggle>("horizontal_toggle");
            _horizontalSlider = RootContainer.Query<Slider>("horizontal_slider");
            _horizontalIntegerField = RootContainer.Query<IntegerField>("horizontal_integerField");

            _verticalToggle = RootContainer.Query<Toggle>("vertical_toggle");
            _verticalSlider = RootContainer.Query<Slider>("vertical_slider");
            _verticalIntegerField = RootContainer.Query<IntegerField>("vertical_integerField");

            _displayToggle = RootContainer.Query<Toggle>("display_toggle");

            _zoom0Toggle = RootContainer.Query<RadioButton>("radioButton-mapEdit-display11");
            _zoom2Toggle = RootContainer.Query<RadioButton>("radioButton-mapEdit-display12");
            _zoom4Toggle = RootContainer.Query<RadioButton>("radioButton-mapEdit-display13");

            SetEntityToUI();
        }

        private void SetEntityToUI() {
            //遠景の画像の取得
            var selectButton = RootContainer.Q<Button>("distant_view_button");
            if (string.IsNullOrEmpty(_mapEntity.Parallax.name))
                selectButton.text = EditorLocalize.LocalizeText("WORD_1594");
            else
            {
                selectButton.text = Path.ChangeExtension(_mapEntity.Parallax.name, ".png");
            }
            selectButton.clickable = null;
            selectButton.clicked += () =>
            {
                var targetImageModalWindow = ImagePicker.Instantiate(DistantViewManager.PATH, true);
                targetImageModalWindow.ShowWindow(EditorLocalize.LocalizeWindowTitle("Select Background"), data =>
                {
                    var imageFileName = Path.ChangeExtension((string)data, ".png");
                    var imageFilePath = DistantViewManager.PATH + imageFileName;

                    _mapEntity.Parallax.name = Path.GetFileNameWithoutExtension(imageFileName);
                    _onChange?.Invoke(_mapEntity);

                    var tex = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(imageFilePath);
                    var spr = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Sprite>(imageFilePath);

                    //Mapのリロード
                    _mapEditWindow =
                        WindowLayoutManager.GetOrOpenWindow(WindowLayoutManager.WindowLayoutId.MapEditWindow) as
                            MapEditWindow;
                    _mapEditWindow.UpdateDistantView(_mapEntity, spr, tex);
                }, _mapEntity.Parallax.name, true);
            };
            _horizontalToggle.value = _mapEntity.Parallax.loopX;
            _horizontalToggle.RegisterValueChangedCallback(evt =>
            {
                _horizontalSlider.SetEnabled(evt.newValue);
                _horizontalIntegerField.SetEnabled(evt.newValue);

                _mapEntity.Parallax.loopX = evt.newValue;
                _onChange?.Invoke(_mapEntity);
            });
            _horizontalSlider.highValue = SLIDER_MAX_VALUE;
            _horizontalSlider.lowValue = SLIDER_MIN_VALUE;
            _horizontalSlider.value = _mapEntity.Parallax.sx;
            _horizontalSlider.SetEnabled(_horizontalToggle.value);
            _horizontalSlider.RegisterValueChangedCallback(evt =>
            {
                _horizontalIntegerField.value = (int) evt.newValue;
                _mapEntity.Parallax.sx = (int) evt.newValue;
                _onChange?.Invoke(_mapEntity);
            });
            _horizontalIntegerField.value = _mapEntity.Parallax.sx;
            _horizontalIntegerField.SetEnabled(_horizontalToggle.value);
            _horizontalIntegerField.RegisterValueChangedCallback(evt =>
            {
                _horizontalSlider.value = evt.newValue;
                _mapEntity.Parallax.sx = evt.newValue;
                _onChange?.Invoke(_mapEntity);
            });

            _verticalToggle.value = _mapEntity.Parallax.loopY;
            _verticalToggle.RegisterValueChangedCallback(evt =>
            {
                _verticalSlider.SetEnabled(evt.newValue);
                _verticalIntegerField.SetEnabled(evt.newValue);

                _mapEntity.Parallax.loopY = evt.newValue;
                _onChange?.Invoke(_mapEntity);
            });
            _verticalSlider.highValue = SLIDER_MAX_VALUE;
            _verticalSlider.lowValue = SLIDER_MIN_VALUE;
            _verticalSlider.value = _mapEntity.Parallax.sy;
            _verticalSlider.SetEnabled(_verticalToggle.value);
            _verticalSlider.RegisterValueChangedCallback(evt =>
            {
                _verticalIntegerField.value = (int) evt.newValue;
                _mapEntity.Parallax.sy = (int) evt.newValue;
                _onChange?.Invoke(_mapEntity);
            });
            _verticalIntegerField.value = _mapEntity.Parallax.sy;
            _verticalIntegerField.SetEnabled(_verticalToggle.value);
            _verticalIntegerField.RegisterValueChangedCallback(evt =>
            {
                _verticalSlider.value = evt.newValue;
                _mapEntity.Parallax.sy = evt.newValue;
                _onChange?.Invoke(_mapEntity);
            });

            // エディター表示トグル。
            var distantViewSpriteRenderer =
                _mapEntity.GetLayerTransformForEditor(MapDataModel.Layer.LayerType.DistantView).
                    GetComponent<SpriteRenderer>();
            distantViewSpriteRenderer.enabled = _displayToggle.value = _mapEntity.Parallax.show;
            _displayToggle.RegisterValueChangedCallback(evt =>
            {
                distantViewSpriteRenderer.enabled =
                _mapEntity.Parallax.show = evt.newValue;
                _onChange?.Invoke(_mapEntity);
            });

            
            _zoom0Toggle.value = _mapEntity.Parallax.zoom0;
            _zoom2Toggle.value = _mapEntity.Parallax.zoom2;
            _zoom4Toggle.value = _mapEntity.Parallax.zoom4;
            var defaultSelect = _mapEntity.Parallax.zoom0 ? 0 : _mapEntity.Parallax.zoom2 ? 1 : 2;
            new CommonToggleSelector().SetRadioSelector(
                new List<RadioButton> {_zoom0Toggle, _zoom2Toggle, _zoom4Toggle},
                defaultSelect, new List<Action>
                {
                    // 等倍表示トグル。
                    () =>
                    {
                        _mapEntity.Parallax.zoom0 = true;
                        _mapEntity.Parallax.zoom2 = _zoom2Toggle.value = false;
                        _mapEntity.Parallax.zoom4 = _zoom4Toggle.value = false;
                        MapCanvas.UpdateDistantViewDisplayedAndScaleForEditor(_mapEntity);
                        _onChange?.Invoke(_mapEntity);
                    },
                    // 2倍表示トグル。
                    () =>
                    {
                        _mapEntity.Parallax.zoom2 = true;
                        _mapEntity.Parallax.zoom0 = _zoom0Toggle.value = false;
                        _mapEntity.Parallax.zoom4 = _zoom4Toggle.value = false;
                        MapCanvas.UpdateDistantViewDisplayedAndScaleForEditor(_mapEntity);

                        _onChange?.Invoke(_mapEntity);
                    },
                    // 4倍表示トグル。
                    () =>
                    {
                        _mapEntity.Parallax.zoom4 = true;
                        _mapEntity.Parallax.zoom0 = _zoom0Toggle.value = false;
                        _mapEntity.Parallax.zoom2 = _zoom2Toggle.value = false;
                        MapCanvas.UpdateDistantViewDisplayedAndScaleForEditor(_mapEntity);
                        _onChange?.Invoke(_mapEntity);
                    }
                });
        }
    }
}