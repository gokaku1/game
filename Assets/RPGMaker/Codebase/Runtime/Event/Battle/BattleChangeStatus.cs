using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Runtime.Battle.Objects;
using RPGMaker.Codebase.Runtime.Common;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RPGMaker.Codebase.Runtime.Event.Battle
{
    /// <summary>
    /// [バトル]-[敵キャラのステート変更]
    /// </summary>
    public class BattleChangeStatus : AbstractEventCommandProcessor
    {
        protected override void Process(string eventID, EventDataModel.EventCommand command) {
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
#else
            ProcessAsync(eventID, command);
        }

        private async void ProcessAsync(string eventID, EventDataModel.EventCommand command) {
#endif
            if (command.parameters[1] == "True")
            {
                var value = OperateValue(
                    command.parameters[2] == "up" ? 0 : 1,
                    command.parameters[4] == "True" ? 0 : 1,
                    int.Parse(command.parameters[4] == "True"
                        ? command.parameters[5]
                        : command.parameters[9]));
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                IterateEnemyIndex(int.Parse(command.parameters[0]),
                    enemy => { ChangeHp(enemy, value, command.parameters[3] == "True"); });
#else
                Func<GameBattler, Task> ChangeHpCallback = async (enemy) =>
                {
                    await ChangeHp(enemy, value, command.parameters[3] == "True");
                };
                await IterateEnemyIndex(int.Parse(command.parameters[0]), ChangeHpCallback);
#endif
            }

            if (command.parameters[10] == "True")
            {
                var value = OperateValue(
                    command.parameters[11] == "up" ? 0 : 1,
                    command.parameters[13] == "True" ? 0 : 1,
                    int.Parse(command.parameters[13] == "True"
                        ? command.parameters[14]
                        : command.parameters[18]));
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                IterateEnemyIndex(int.Parse(command.parameters[0]),
                    enemy => { enemy.GainMp(value); });
#else
                Func<GameBattler, Task> GainHpCallback = async (enemy) =>
                {
                    await UniteTask.Delay(0);
                    enemy.GainMp(value);
                };
                await IterateEnemyIndex(int.Parse(command.parameters[0]), GainHpCallback);
#endif
            }

            if (command.parameters[19] == "True")
            {
                var value = OperateValue(
                    command.parameters[20] == "up" ? 0 : 1,
                    command.parameters[22] == "True" ? 0 : 1,
                    int.Parse(command.parameters[22] == "True"
                        ? command.parameters[22]
                        : command.parameters[25]));
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                IterateEnemyIndex(int.Parse(command.parameters[0]),
                    enemy => { enemy.GainTp(value); });
#else
                Func<GameBattler, Task> GainTpCallback = async (enemy) =>
                {
                    await UniteTask.Delay(0);
                    enemy.GainTp(value);
                };
                await IterateEnemyIndex(int.Parse(command.parameters[0]), GainTpCallback);
#endif
            }

            //次のイベントへ
            ProcessEndAction();
        }

        private int OperateValue(int operation, int operandType, int operand) {
            var value = operandType == 0
                ? operand
                : int.Parse(DataManager.Self().GetRuntimeSaveDataModel().variables.data[operand]);
            return operation == 0 ? value : -value;
        }

#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        private void IterateEnemyIndex(int param, Action<GameBattler> callback) {
#else
        private async Task IterateEnemyIndex(int param, Func<GameBattler, Task> callback) {
#endif
            if (param < 0)
            {
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                DataManager.Self().GetGameTroop().Members().ForEach(callback);
#else
                foreach (var enemy in await DataManager.Self().GetGameTroop().Members())
                {
                    await callback(enemy);
                }
#endif
            }
            else
            {
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                var enemy = DataManager.Self().GetGameTroop().Members().ElementAtOrDefault(param);
#else
                var enemy = (await DataManager.Self().GetGameTroop().Members()).ElementAtOrDefault(param);
#endif
                if (enemy != null)
                {
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                    callback(enemy);
#else
                    await callback(enemy);
#endif
                }
            }
        }

#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void ChangeHp(GameBattler target, int value, bool allowDeath) {
#else
        public async Task ChangeHp(GameBattler target, int value, bool allowDeath) {
#endif
            if (target.IsAlive())
            {
                var maxHP = target.Mhp;
                var minHP = allowDeath ? 0 : 1;
                target.Hp = Math.Max(minHP, Math.Min(maxHP, target.Hp + value));

                if (allowDeath && target.Hp == 0)
                {
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                    target.Refresh();
#else
                    await target.Refresh();
#endif
                    target.PerformCollapse();
                }
            }
        }

        private void ProcessEndAction() {
            SendBackToLauncher.Invoke();
        }
    }
}