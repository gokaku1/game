using System.Collections.Generic;
using RPGMaker.Codebase.CoreSystem.Helper;
using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Enemy;
using RPGMaker.Codebase.CoreSystem.Lib.RepositoryCore;
using System.Threading.Tasks;

namespace RPGMaker.Codebase.CoreSystem.Service.DatabaseManagement.Repository
{
    public class EnemyRepository : AbstractDatabaseRepository<EnemyDataModel>
    {
        protected override string JsonPath => "Assets/RPGMaker/Storage/Character/JSON/enemy.json";
#if !UNITY_EDITOR || UNITE_WEBGL_TEST
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public new List<EnemyDataModel> Load() {
#else
        public new async Task<List<EnemyDataModel>> Load() {
#endif
            if (DataModels != null)
            {
                // キャッシュがあればそれを返す
                return DataModels;
            }
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            DataModels = ScriptableObjectOperator.GetClass<EnemyDataModel>(JsonPath) as List<EnemyDataModel>;
#else
            DataModels = (await ScriptableObjectOperator.GetClass<EnemyDataModel>(JsonPath)) as List<EnemyDataModel>;
#endif
            SetSerialNumbers();
            return DataModels;
        }
#endif
    }
}