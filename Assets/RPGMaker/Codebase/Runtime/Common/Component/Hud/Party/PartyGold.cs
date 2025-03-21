using RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement;
using System.Threading.Tasks;

namespace RPGMaker.Codebase.Runtime.Common.Component.Hud.Party
{
    public class PartyGold
    {
        private DatabaseManagementService _databaseManagementService;

        // 状態プロパティ
        //--------------------------------------------------------------------------------------------------------------
        //所持金の上限下限
        private readonly int _maxGold = 99999999;
        private readonly int _minGold = 0;

        // データプロパティ
        //--------------------------------------------------------------------------------------------------------------
        private int    _type;
        private string _value;
        private int    _variable;

        // initialize methods
        //--------------------------------------------------------------------------------------------------------------
        /**
         * 初期化
         */
        public void Init(int type, int variable, string value) {
            _type = type;
            _variable = variable;
            _value = value;

            _databaseManagementService = new DatabaseManagementService();
        }

#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void SetPartyGold() {
#else
        public async Task SetPartyGold() {
#endif
            //実際に変動する値
            var value = 0;

            if (_variable == 1)
            {
                var runtimeSaveDataModel = DataManager.Self().GetRuntimeSaveDataModel();

#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                var flagDataModel = _databaseManagementService.LoadFlags();
#else
                var flagDataModel = await _databaseManagementService.LoadFlags();
#endif
                for (var i = 0; i < flagDataModel.variables.Count; i++)
                    if (flagDataModel.variables[i].id == _value)
                    {
                        value = int.Parse(runtimeSaveDataModel.variables.data[i]);
                        break;
                    }
            }
            else
            {
                value = int.Parse(_value);
            }

            if (_type == 0)
            {
                DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.gold += value;
                if (DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.gold > _maxGold)
                    DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.gold = _maxGold;
            }
            else
            {
                DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.gold -= value;
                if (DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.gold < _minGold)
                    DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.gold = _minGold;
            }
        }
    }
}