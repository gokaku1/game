using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Common;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Map;
using RPGMaker.Codebase.CoreSystem.Knowledge.Enum;
using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using RPGMaker.Codebase.CoreSystem.Service.MapManagement;
using RPGMaker.Codebase.CoreSystem.Service.RuntimeDataManagement;
using RPGMaker.Codebase.Runtime.Map;
using RPGMaker.Codebase.Runtime.Map.Component.Character;
using System;
using System.Collections.Generic;
using UnityEngine; //バトルでは本コマンドは利用しない
using System.Threading.Tasks;
using RPGMaker.Codebase.CoreSystem.Helper;

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.Character
{
    public class MovePlace : MonoBehaviour
    {
        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        private Action _closeAction;

        private DatabaseManagementService _databaseManagementService;

        // 表示要素プロパティ
        //--------------------------------------------------------------------------------------------------------------
        private GameObject _prefab;

        // initialize methods
        //--------------------------------------------------------------------------------------------------------------
        /**
         * 初期化
         */
        public void Init() {
            if (_prefab != null)
                return;
            _databaseManagementService = new DatabaseManagementService();
        }

#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void MovePlaceProcess(string eventId, EventDataModel.EventCommand command, Action close) {
#else
        public async Task MovePlaceProcess(string eventId, EventDataModel.EventCommand command, Action close) {
            var tcs = new TaskCompletionSource<bool>();
            StartCoroutine(Cleanup(tcs));
            await tcs.Task;
#endif
            Init();
            var runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();
            _closeAction = close;

            var direction = MapManager.GetOperatingCharacterGameObject().GetComponent<CharacterOnMap>().GetCurrentDirection();
            switch (int.Parse(command.parameters[5]))
            {
                case 1:
                    direction = CharacterMoveDirectionEnum.Down;
                    break;
                case 2:
                    direction = CharacterMoveDirectionEnum.Left;
                    break;
                case 3:
                    direction = CharacterMoveDirectionEnum.Right;
                    break;
                case 4:
                    direction = CharacterMoveDirectionEnum.Up;
                    break;
            }

            var pos = new Vector2(0, 0);
            var mapManagementService = new MapManagementService();
            MapDataModel initialMap = null;

            if (command.parameters[0] == "0")
            {
                // 直接指定
                // マップID
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                initialMap = mapManagementService.LoadMapById(command.parameters[2]);
                if (initialMap == null)
                    initialMap = mapManagementService.LoadMapById(command.parameters[7]);
#else
                initialMap = await mapManagementService.LoadMapById(command.parameters[2]);
                if (initialMap == null)
                    initialMap = await mapManagementService.LoadMapById(command.parameters[7]);
#endif

                // 座標
                if (float.TryParse(command.parameters[3], out pos.x))
                    pos.x = Mathf.Abs(pos.x);
                if (float.TryParse(command.parameters[4], out pos.y))
                    pos.y = Mathf.Abs(pos.y) * -1f;
            }
            else
            {
                // 変数で指定
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                var flagDataModel = _databaseManagementService.LoadFlags();
#else
                var flagDataModel = await _databaseManagementService.LoadFlags();
#endif

                // マップID
                var mapIndex = flagDataModel.variables.FindIndex(v => v.id == command.parameters[2]);
                int mapSerialNumber = 0;
                if (mapIndex != -1)
                    mapSerialNumber = int.Parse(runtimeSaveDataModel.variables.data[mapIndex]);

#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                List<MapBaseDataModel> work = mapManagementService.LoadMapBase();
#else
                List<MapBaseDataModel> work = await mapManagementService.LoadMapBase();
#endif
                for (int i = 0; i < work.Count; i++)
                {
                    if (work[i].SerialNumber == mapSerialNumber)
                    {
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                        initialMap = mapManagementService.LoadMapById(work[i].id);
#else
                        initialMap = await mapManagementService.LoadMapById(work[i].id);
#endif
                        break;
                    }
                }

                if (initialMap == null)
                {
                    //MVでは存在しないマップを指定した場合、フェードアウト後にエラーになってしまうが、
                    //移動先が存在しなかった場合は、本イベントを処理せずに先に進むように変更する
                    TimeHandler.Instance.AddTimeAction(0.1f, Wait, false);
                    return;
                }

                // 座標
                var posXIndex = flagDataModel.variables.FindIndex(v => v.id == command.parameters[3]);
                if (posXIndex != -1)
                    pos.x = int.Parse(runtimeSaveDataModel.variables.data[posXIndex]);

                var posYIndex = flagDataModel.variables.FindIndex(v => v.id == command.parameters[4]);
                if (posYIndex != -1)
                    pos.y = int.Parse(runtimeSaveDataModel.variables.data[posYIndex]) * -1;
            }

            runtimeSaveDataModel.runtimePlayerDataModel.map.mapId = initialMap.id;
            runtimeSaveDataModel.runtimePlayerDataModel.map.x = Mathf.RoundToInt(pos.x);
            runtimeSaveDataModel.runtimePlayerDataModel.map.y = Mathf.RoundToInt(pos.y);

            if (MapManager.CurrentMapDataModel.id != initialMap.id)
            {
                //移動前にデータを保持
                MapEventExecutionController.Instance.SetCarryEventOnMap(eventId);
            }

            // オートセーブの実施
            var systemSettingDataModel = DataManager.Self().GetSystemDataModel();
            if (systemSettingDataModel.optionSetting.enabledAutoSave == 1)
            {
                var runtimeDataManagementService = new RuntimeDataManagementService();
                runtimeDataManagementService.SaveAutoSaveData(runtimeSaveDataModel);
            }

            //フェードの種類に応じて処理を変更
            int fadeType = int.Parse(command.parameters[6]);

            //フェードアウト
            if (fadeType == 0)
            {
                //黒フェード
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                HudDistributor.Instance.StaticHudHandler().DisplayInit();
                HudDistributor.Instance.StaticHudHandler().FadeOut(() => {
#else
                await HudDistributor.Instance.StaticHudHandler().DisplayInit();
                HudDistributor.Instance.StaticHudHandler().FadeOut(async () => {
#endif
                    // カメラ位置を初期化
                    DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.cameraX = 0;
                    DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.cameraY = 0;

                    //マップ切り替え
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                    MapManager.ChangeMapForRuntime(initialMap, pos, direction);
#else
                    await MapManager.ChangeMapForRuntime(initialMap, pos, direction);
#endif
                    //次のマップのBGM、BGS設定を反映させる
                    ReflectionBgmBgs(initialMap);
                    //マップの読込で引っかかることを考慮した待ち
                    TimeHandler.Instance.AddTimeAction(0.1f, () =>
                    {
                        HudDistributor.Instance.StaticHudHandler().FadeIn(() => {
                            TimeHandler.Instance.AddTimeAction(0.1f, Wait, false);
                        });
                    }, false);
                }, UnityEngine.Color.black);
            }
            else if (fadeType == 1)
            {
                //白フェード
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                HudDistributor.Instance.StaticHudHandler().DisplayInit();
                HudDistributor.Instance.StaticHudHandler().FadeOut(() => {
#else
                await HudDistributor.Instance.StaticHudHandler().DisplayInit();
                HudDistributor.Instance.StaticHudHandler().FadeOut(async () => {
#endif
                    // カメラ位置を初期化
                    DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.cameraX = 0;
                    DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.cameraY = 0;

                    //マップ切り替え
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                    var mapPrefabControl = MapManager.ChangeMapForRuntime(initialMap, pos, direction);
#else
                    var mapPrefabControl = await MapManager.ChangeMapForRuntime(initialMap, pos, direction);
#endif
                    //次のマップのBGM、BGS設定を反映させる
                    ReflectionBgmBgs(initialMap);
                    //マップの読込で引っかかることを考慮した待ち
                    TimeHandler.Instance.AddTimeAction(0.1f, FadeInWait, true);

                    void FadeInWait() {
                        if (mapPrefabControl.IsInstantiate)
                        {
                            TimeHandler.Instance.AddTimeAction(0.1f, () =>
                            {
                                HudDistributor.Instance.StaticHudHandler().FadeIn(() => {
                                    TimeHandler.Instance.AddTimeAction(0.1f, Wait, false);
                                });
                            }, false);
                            TimeHandler.Instance.RemoveTimeAction(FadeInWait);
                        }
                    }

                }, UnityEngine.Color.white);
            }
            else
            {
                //フェードなし
                // カメラ位置を初期化
                DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.cameraX = 0;
                DataManager.Self().GetRuntimeSaveDataModel().runtimePlayerDataModel.map.cameraY = 0;

                //マップ切り替え
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                MapManager.ChangeMapForRuntime(initialMap, pos, direction);
#else
                await MapManager.ChangeMapForRuntime(initialMap, pos, direction);
#endif
                //次のマップのBGM、BGS設定を反映させる
                ReflectionBgmBgs(initialMap);
                TimeHandler.Instance.AddTimeAction(0.1f, Wait, false);
            }
        }

#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
#else
        private System.Collections.IEnumerator Cleanup(TaskCompletionSource<bool> tcs) {
            AddressableManager.Load.MarkForRelease();
            AddressableManager.Load.IncrementTimestamp();

            yield return Resources.UnloadUnusedAssets();

            System.GC.Collect();
            tcs.TrySetResult(true);
        }
#endif

        private void Wait() {
            _closeAction.Invoke();
        }

        private void ReflectionBgmBgs(MapDataModel nextMap) {
            if (nextMap.autoPlayBGM) {
                // BGMを変更する
                SoundManager.Self().ChangeBGM(
                    new SoundCommonDataModel(nextMap.bgmID, nextMap.bgmState.pan, nextMap.bgmState.pitch, nextMap.bgmState.volume));
            }

            if (nextMap.autoPlayBgs)
            {
                // BGSを変更する
                SoundManager.Self().ChangeBGS(
                    new SoundCommonDataModel(nextMap.bgsID, nextMap.bgsState.pan, nextMap.bgsState.pitch, nextMap.bgsState.volume));
            }
        }
    }
}