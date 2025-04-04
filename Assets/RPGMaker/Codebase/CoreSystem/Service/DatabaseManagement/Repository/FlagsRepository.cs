using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Helper.SO;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Flag;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System;
using System.IO;
using UnityEngine;
using JsonHelper = RPGMaker.Codebase.CoreSystem.Helper.JsonHelper;
using System.Threading.Tasks;

namespace RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository
{
    public class FlagsRepository
    {
        private static readonly string JSON_PATH = "Assets/RPGMaker/Storage/Flags/JSON/flags.json";

        private static FlagDataModel _flagDataModel;

        public void Save(FlagDataModel flagDataModel) {
            if (flagDataModel == null)
                throw new Exception("Tried to save null data model.");

            File.WriteAllText(JSON_PATH, JsonUtility.ToJson(flagDataModel));

            // キャッシュを更新
            _flagDataModel = flagDataModel;

            SetSerialNumbers();
        }

#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void SaveVariable(FlagDataModel.Variable variable) {
#else
        public async Task SaveVariable(FlagDataModel.Variable variable) {
#endif
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            if (_flagDataModel != null) Load();
#else
            if (_flagDataModel != null) await Load();
#endif

            var targetIndex = _flagDataModel.variables.FindIndex(item => item.id == variable.id);
            _flagDataModel.variables[targetIndex] = variable;

            SetSerialNumbers();

            Save(_flagDataModel);
        }

#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void SaveSwitch(FlagDataModel.Switch sw) {
#else
        public async Task SaveSwitch(FlagDataModel.Switch sw) {
#endif
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            if (_flagDataModel != null) Load();
#else
            if (_flagDataModel != null) await Load();
#endif

            var targetIndex = _flagDataModel.switches.FindIndex(item => item.id == sw.id);
            _flagDataModel.switches[targetIndex] = sw;

            SetSerialNumbers();

            Save(_flagDataModel);
        }

#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public FlagDataModel Load() {
#else
        public async Task<FlagDataModel> Load() {
#endif
            if (_flagDataModel != null)
                // キャッシュがあればそれを返す
                return _flagDataModel;
#if UNITY_EDITOR && !UNITE_WEBGL_TEST
            var jsonString = UnityEditorWrapper.AssetDatabaseWrapper.LoadJsonString(JSON_PATH);
            _flagDataModel = JsonHelper.FromJson<FlagDataModel>(jsonString);
#else
#if !UNITY_WEBGL
            _flagDataModel = AddressableManager.Load.LoadAssetSync<FlagsSO>(JSON_PATH).dataModel;
#else
            _flagDataModel = (await AddressableManager.Load.LoadAssetSync<FlagsSO>(JSON_PATH)).dataModel;
#endif
#endif
            SetSerialNumbers();

            return _flagDataModel;
        }

        private void SetSerialNumbers() {
            // serial numberフィールドがあるモデルには連番を設定する
            Action<WithSerialNumberDataModel, int> func = (withSerialNumberDataModel, index) =>
            {
                withSerialNumberDataModel.SerialNumber = index + 1;
            };
            for (var i = 0; i < _flagDataModel.switches.Count; i++) func(_flagDataModel.switches[i], i);
            for (var i = 0; i < _flagDataModel.variables.Count; i++) func(_flagDataModel.variables[i], i);
        }
    }
}