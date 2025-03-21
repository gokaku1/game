using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.AssetManage;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace RPGMaker.Codebase.Runtime.Map.Component.Character
{
    public class VehicleGraphic : MonoBehaviour
    {
        private int _currentSpriteIndex;
        private List<Sprite> _currentSprites;

        // 状態プロパティ
        //--------------------------------------------------------------------------------------------------------------
        private CharacterMoveDirectionEnum _directionEnum;
        private List<Sprite>               _downDirectionSprites;
        private List<Sprite>               _leftDirectionSprites;
        private List<Sprite>               _rightDirectionSprites;

        // 表示要素プロパティ
        //--------------------------------------------------------------------------------------------------------------
        private SpriteRenderer _spriteRenderer;
        private List<Sprite>   _upDirectionSprites;

        // initialize methods
        //--------------------------------------------------------------------------------------------------------------
        /**
         * 初期化
         */
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void Init(string assetId) {
#else
        public async Task Init(string assetId) {
#endif
            if (assetId == "") return;
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            SetUpSprites(assetId);
#else
            await SetUpSprites(assetId);
#endif

            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            ChangeDirection(CharacterMoveDirectionEnum.Down);
        }

        public void ChangeDirection(CharacterMoveDirectionEnum directionEnum) {
            if (directionEnum == _directionEnum) return;

            _directionEnum = directionEnum;
            _currentSprites = _directionEnum switch
            {
                CharacterMoveDirectionEnum.Up => _upDirectionSprites,
                CharacterMoveDirectionEnum.Down => _downDirectionSprites,
                CharacterMoveDirectionEnum.Left => _leftDirectionSprites,
                CharacterMoveDirectionEnum.Right => _rightDirectionSprites,
                _ => throw new ArgumentOutOfRangeException()
            };
            _currentSpriteIndex = 0;
            Render();
        }

        public void Step(CharacterMoveDirectionEnum directionEnum, bool isLockDirection) {
            if (directionEnum != _directionEnum && !isLockDirection) ChangeDirection(directionEnum);
        }


        public CharacterMoveDirectionEnum GetCurrentDirection() {
            return _directionEnum;
        }

        public void StepAnimation() {
            if (_currentSprites == null) return;
            _currentSpriteIndex++;
            if (_currentSpriteIndex >= _currentSprites.Count) _currentSpriteIndex = 0;

            Render();
        }

        private void Render() {
            if (_currentSprites == null || _currentSprites.Count == 0) return;
            _spriteRenderer.sprite = _currentSprites[_currentSpriteIndex];
        }

#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        private void SetUpSprites(string assetId) {
#else
        private async Task SetUpSprites(string assetId) {
#endif
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            var inputString =
                UnityEditorWrapper.AssetDatabaseWrapper
                    .LoadAssetAtPath<TextAsset>(
                        "Assets/RPGMaker/Storage/AssetManage/JSON/Assets/" + assetId + ".json");
            var assetManageData = JsonHelper.FromJson<AssetManageDataModel>(inputString.text);
#else
            var assetManageData =
#if !UNITY_WEBGL
				ScriptableObjectOperator.GetClass<AssetManageDataModel>("SO/" + assetId + ".asset") as AssetManageDataModel;
#else
				(await ScriptableObjectOperator.GetClass<AssetManageDataModel>("SO/" + assetId + ".asset")) as AssetManageDataModel;
#endif
#endif

            //up,left,right,down
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            var down = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                "Assets/RPGMaker/Storage/Images/Objects/" + assetManageData.imageSettings[0].path);
            var left = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                "Assets/RPGMaker/Storage/Images/Objects/" + assetManageData.imageSettings[1].path);
            var right = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                "Assets/RPGMaker/Storage/Images/Objects/" + assetManageData.imageSettings[2].path);
            var up = UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                "Assets/RPGMaker/Storage/Images/Objects/" + assetManageData.imageSettings[3].path);
#else
            var down = await UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                "Assets/RPGMaker/Storage/Images/Objects/" + assetManageData.imageSettings[0].path);
            var left = await UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                "Assets/RPGMaker/Storage/Images/Objects/" + assetManageData.imageSettings[1].path);
            var right = await UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                "Assets/RPGMaker/Storage/Images/Objects/" + assetManageData.imageSettings[2].path);
            var up = await UnityEditorWrapper.AssetDatabaseWrapper.LoadAssetAtPath<Texture2D>(
                "Assets/RPGMaker/Storage/Images/Objects/" + assetManageData.imageSettings[3].path);
#endif

            var downStr = assetManageData.imageSettings[0].path.Replace(".png", "");
            var downIndex = int.Parse(downStr.Substring(downStr.Length - 1));
            var leftStr = assetManageData.imageSettings[1].path.Replace(".png", "");
            var leftIndex = int.Parse(leftStr.Substring(leftStr.Length - 1));
            var rightStr = assetManageData.imageSettings[2].path.Replace(".png", "");
            var rightIndex = int.Parse(rightStr.Substring(rightStr.Length - 1));
            var upStr = assetManageData.imageSettings[3].path.Replace(".png", "");
            var upIndex = int.Parse(upStr.Substring(upStr.Length - 1));


            _downDirectionSprites = ImageUtility.Instance.SliceImageToSprite(down, downIndex, 1);
            _leftDirectionSprites = ImageUtility.Instance.SliceImageToSprite(left, leftIndex, 1);
            _rightDirectionSprites = ImageUtility.Instance.SliceImageToSprite(right, rightIndex, 1);
            _upDirectionSprites = ImageUtility.Instance.SliceImageToSprite(up, upIndex, 1);
        }

        //キャラクターの画像を読み込み直す部分
        //読み込み直すassetsIDが入ります
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void ReloadVehicleImage(string id) {
#else
        public async Task ReloadVehicleImage(string id) {
#endif
            //処理が重複してしまったため「SetUpSprites」を呼び出しています
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            SetUpSprites(id);
#else
            await SetUpSprites(id);
#endif
        }
    }
}