using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Runtime;
using RPGMaker.Codebase.Runtime.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace RPGMaker.Codebase.Runtime.Battle.Objects
{
    /// <summary>
    /// [パーティ]を定義したクラス
    /// </summary>
    public class GameParty : GameUnit
    {
        /// <summary>
        /// [static] エンカウント半減
        /// </summary>
        public const int AbilityEncounterHalf = 0;
        /// <summary>
        /// [static] エンカウント無効
        /// </summary>
        public const int AbilityEncounterNone = 1;
        /// <summary>
        /// [static] 不意打ち無効
        /// </summary>
        public const int AbilityCancelSurprise = 2;
        /// <summary>
        /// [static] 先制攻撃率アップ
        /// </summary>
        public const int AbilityRaisePreemptive = 3;
        /// <summary>
        /// [static] 獲得金額2倍
        /// </summary>
        public const int AbilityGoldDouble = 4;
        /// <summary>
        /// [static] アイテム入手率2倍
        /// </summary>
        public const int AbilityDropItemDouble = 5;
        /// <summary>
        /// 所持金
        /// </summary>
        public int Gold
        {
            get
            {
                return DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.gold;
            }
            private set
            {
                DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.gold = value;
            }
        }
        /// <summary>
        /// 戦闘テスト用のデータかどうか
        /// </summary>
        private bool _isBattleTest = false;
        /// <summary>
        /// アクターのリスト
        /// </summary>
        private List<GameActor> _actorsTmp = new List<GameActor>();
        /// <summary>
        /// アクターIDの配列
        /// </summary>
        private List<string> _actorIds = new List<string>();
        /// <summary>
        /// GameActor
        /// </summary>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public List<GameActor> Actors
#else
        public async Task<List<GameActor>> GetActors()
#endif
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        {
            get
#else
#endif
            {
                //戦闘テスト中は設定されたデータをそのまま返却
                if (_isBattleTest)
                    return _actorsTmp;

                //現在のアクター一覧
                var actorIds = DataManager.Self().GetRuntimeSaveDataModel()?.runtimePartyDataModel?.actors;
                if (actorIds == null) return null;

                //前回取得時と差分があるかどうかの確認
                bool flg = false;
                if (actorIds.Count == _actorIds.Count)
                {
                    for (int i = 0; i < actorIds.Count; i++)
                    {
                        if (actorIds[i] != _actorIds[i])
                        {
                            flg = true;
                            break;
                        }
                    }
                }
                else
                {
                    flg = true;
                }

                //差分がなければ、前回作成したGameActorをそのまま返却
                if (!flg)
                    return _actorsTmp;

                //現在のアクター情報を保持
                _actorIds.Clear();
                for (int i = 0; i < actorIds.Count; i++)
                {
                    _actorIds.Add(actorIds[i]);
                }

                //RuntimeActorDataModelリスト初期化
                List<RuntimeActorDataModel> actors = new List<RuntimeActorDataModel>();

                //現在のパーティメンバーのIDの並び順に詰める
                foreach (var id in actorIds)
                    foreach (var actor in DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels)
                        if (id == actor.actorId)
                            actors.Add(actor);

                //パーティメンバーの並び順で詰める
                List<GameActor> actorsTmp = new List<GameActor>();
                foreach (var t in actors)
                {
                    flg = false;
                    for (int i = 0; i < _actorsTmp.Count; i++)
                    {
                        if (_actorsTmp[i].ActorId == t.actorId)
                        {
                            //既に生成済みのため、流用する
                            actorsTmp.Add(_actorsTmp[i]);
                            flg = true;
                            break;
                        }
                    }
                    if (!flg)
                    {
                        //不在のため新規作成する
                        var actor = new GameActor(t);
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
#else
                        await actor.InitForConstructor(t);
#endif
                        actor.Id = t.actorId;
                        actorsTmp.Add(actor);
                    }
                }

                //詰め替えたものを保持
                _actorsTmp.Clear();
                for (int i = 0; i < actorsTmp.Count; i++)
                {
                    _actorsTmp.Add(actorsTmp[i]);
                }

                return _actorsTmp;
            }
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            private set
#else
        private void SetActors(List<GameActor> value)
#endif
            {
                if (!_isBattleTest)
                    return;

                _actorsTmp = value;
            }
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        }
#else
#endif
        /// <summary>
        /// 所持アイテム一覧
        /// </summary>
        //private Dictionary<string, int> _items = new Dictionary<string, int>();
        private Dictionary<string, int> _itemsTmp = new Dictionary<string, int>();
        private Dictionary<string, int> _items
        {
            get
            {
                for (int i = 0; i < DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.items.Count; i++)
                {
                    _itemsTmp[DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.items[i].itemId] =
                        DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.items[i].value;
                }
                return _itemsTmp;
            }
        }
        /// <summary>
        /// 所持武器一覧
        /// </summary>
        //private Dictionary<string, int> _weapons = new Dictionary<string, int>();
        private Dictionary<string, int> _weaponsTmp = new Dictionary<string, int>();
        private Dictionary<string, int> _weapons
        {
            get
            {
                for (int i = 0; i < DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.weapons.Count; i++)
                {
                    _weaponsTmp[DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.weapons[i].weaponId] =
                        DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.weapons[i].value;
                }
                return _weaponsTmp;
            }
        }
        /// <summary>
        /// 所持防具一覧
        /// </summary>
        //private Dictionary<string, int> _armors = new Dictionary<string, int>();
        private Dictionary<string, int> _armorsTmp = new Dictionary<string, int>();
        private Dictionary<string, int> _armors
        {
            get
            {
                for (int i = 0; i < DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.armors.Count; i++)
                {
                    _armorsTmp[DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.armors[i].armorId] =
                        DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.armors[i].value;
                }
                return _armorsTmp;
            }
        }

        /// <summary>
        /// 戦闘シーンのプレビューに使用するため、プレビュー用のアクター設定
        /// </summary>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void SetupStartingMembersFromDataBase() {
#else
        public async Task SetupStartingMembersFromDataBase() {
#endif
            //戦闘テスト
            _isBattleTest = true;

            //アクターデータの初期化
            //Uniteはパーティメンバー上限が4人のため、パーティのデータ -> アクターのデータと引いていく必要が無く、単純に今のパーティで設定する
            var actorsWork = DataManager.Self().GetSystemDataModel().initialParty.party;//DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels;

            //パーティメンバー
            var actors = new List<RuntimeActorDataModel>();

            //現在のパーティメンバーのIDの並び順に詰める
            foreach (var id in actorsWork)
            {
                var actorDataModel = DataManager.Self().GetActorDataModels().FirstOrDefault(c => c.uuId == id);
                var actor = new RuntimeActorDataModel(
                    actorDataModel.uuId,
                    actorDataModel.basic.name,
                    actorDataModel.basic.secondName,
                    actorDataModel.basic.profile,
                    actorDataModel.basic.classId,
                    actorDataModel.basic.initialLevel,
                    actorDataModel.image.character,
                    actorDataModel.image.face,
                    actorDataModel.image.battler,
                    actorDataModel.image.adv,
                    new RuntimeActorDataModel.Exp(),
                    9999,
                    9999,
                    100,
                    new RuntimeActorDataModel.ParamPlus(),
                    0);
                actors.Add(actor);
            }

            //アクターデータをバトル用に詰め替え
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            Actors = new List<GameActor>();
#else
            SetActors(new List<GameActor>());
#endif
            foreach (var t in actors)
            {
                var actor = new GameActor(t);
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
#else
                await actor.InitForConstructor(t);
#endif
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                Actors.Add(actor);
#else
                (await GetActors()).Add(actor);
#endif
            }
        }

        /// <summary>
        /// パーティメンバーの数を返す
        /// </summary>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public int Size() {
            return Members().Count;
        }
#else
        public async Task<int> Size() {
            return (await Members()).Count;
        }
#endif

        /// <summary>
        /// パーティメンパーが0人か
        /// </summary>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public bool IsEmpty() {
            return Size() == 0;
        }
#else
        public async Task<bool> IsEmpty() {
            return await Size() == 0;
        }
#endif

        /// <summary>
        /// 戦闘中のバトラー生死問わず全て配列で返す
        /// </summary>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public override List<GameBattler> Members() {
#else
        public override async Task<List<GameBattler>> Members() {
#endif
            return InBattle()
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                ? BattleMembers().Aggregate(new List<GameBattler>(), (l, actor) =>
#else
                ? (await BattleMembers()).Aggregate(new List<GameBattler>(), (l, actor) =>
#endif
                {
                    l.Add(actor);
                    return l;
                })
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                : AllMembers().Aggregate(new List<GameBattler>(), (l, actor) =>
#else
                : (await AllMembers()).Aggregate(new List<GameBattler>(), (l, actor) =>
#endif
                {
                    l.Add(actor);
                    return l;
                });
        }

        /// <summary>
        /// パーティの全アクターを配列で返す
        /// </summary>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public List<GameActor> AllMembers() {
            return Actors;
        }
#else
        public async Task<List<GameActor>> AllMembers() {
            return await GetActors();
        }
#endif

        /// <summary>
        /// 戦闘に参加する全アクターを配列で返す
        /// </summary>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public List<GameActor> BattleMembers() {
#else
        public async Task<List<GameActor>> BattleMembers() {
#endif
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            return AllMembers().GetRange(0, Mathf.Min(AllMembers().Count, MaxBattleMembers())).FindAll(actor => actor.IsAppeared());
#else
            var allMembers = await AllMembers();
            return allMembers.GetRange(0, Mathf.Min(allMembers.Count, MaxBattleMembers())).FindAll(actor => actor.IsAppeared());
#endif
        }

        /// <summary>
        /// 戦闘参加メンバーの最大数(規定値:4)を返す
        /// </summary>
        /// <returns></returns>
        public int MaxBattleMembers() {
            return 4;
        }

        /// <summary>
        /// リーダーであるアクターを返す
        /// </summary>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public GameBattler Leader() {
            return BattleMembers()[0];
        }
#else
        public async Task<GameBattler> Leader() {
            return (await BattleMembers())[0];
        }
#endif

        /// <summary>
        /// 全戦闘参加メンバーを蘇生
        /// </summary>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void ReviveBattleMembers() {
            BattleMembers().ForEach(actor =>
            {
                //死亡ステータスを解除する
                actor.ClearResult();
                actor.ClearStates();
                actor.SetHp(1);
            });
            
            //ランタイム側のステートを全クリア
            DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels.ForEach(actor =>
            {
                actor.AllClearStates();
            });
        }
#else
        public async Task ReviveBattleMembers() {
            foreach (var actor in await BattleMembers())
            {
                //死亡ステータスを解除する
                actor.ClearResult();
                actor.SetBattleEndStates();
                actor.SetHp(1);
                //actor.Refresh();
            }
            //ランタイム側のステートを全クリア
            foreach( var actor in DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels)
            {
                actor.AllClearStates();
            }
        }
#endif

        /// <summary>
        /// 全戦闘参加メンバーを全回復状態で蘇生
        /// </summary>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void ReviveAllBattleMembers() {
#else
        public async Task ReviveAllBattleMembers() {
#endif
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            BattleMembers().ForEach(actor =>
#else
            foreach (var actor in await BattleMembers())
#endif
            {
                //死亡ステータスを解除する
                actor.ClearResult();
                actor.ClearStates();
                actor.SetHp(actor.Mhp);
                actor.SetMp(actor.Mmp);
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            });
#else
            }
#endif

#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels.ForEach(actor =>

#else
            //ランタイム側のステートを全クリア
            foreach (var actor in DataManager.Self().GetRuntimeSaveDataModel().runtimeActorDataModels)
#endif
            {
                actor.AllClearStates();
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            });
#else
            }
#endif
        }

        /// <summary>
        /// アイテム(武器・防具を含まない)を配列で返す
        /// </summary>
        /// <returns></returns>
        public IEnumerable<GameItem> Items() {
            return _items.Select(keyValuePair => new GameItem(keyValuePair.Key, GameItem.DataClassEnum.Item)).ToList();
        }

        /// <summary>
        /// パーティが持つ全武器を配列で返す
        /// </summary>
        /// <returns></returns>
        public IEnumerable<GameItem> Weapons() {
            return _weapons.Select(keyValuePair => new GameItem(keyValuePair.Key, GameItem.DataClassEnum.Weapon)).ToList();
        }

        /// <summary>
        /// パーティが持つ全防具を配列で返す
        /// </summary>
        /// <returns></returns>
        public IEnumerable<GameItem> Armors() {
            return _weapons.Select(keyValuePair => new GameItem(keyValuePair.Key, GameItem.DataClassEnum.Armor)).ToList();
        }

        /// <summary>
        /// パーティが持つ全装備可能アイテムを配列で返す
        /// </summary>
        /// <returns></returns>
        public IEnumerable<GameItem> EquipItems() {
            return Weapons().Concat(Armors());
        }

        /// <summary>
        /// パーティが持つ全アイテムを配列で返す
        /// </summary>
        /// <returns></returns>
        public IEnumerable<GameItem> AllItems() {
            return Items().Concat(EquipItems());
        }

        /// <summary>
        /// 指定アイテムが含まれるカテゴリ全体を配列で返す。
        /// [アイテム][武器][防具]のいずれかのカテゴリ。
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public Dictionary<string, int> ItemContainer(GameItem item) {
            if (item == null) return null;
            if (item.IsItem()) return _items;
            if (item.IsWeapon()) return _weapons;
            return item.IsArmor() ? _armors : null;
        }

        /// <summary>
        /// パーティの名前を返す。
        /// ひとりの時は「アクター名」、複数いる時は「アクター名たち」(規定値)
        /// </summary>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public string Name() {
#else
        public async Task<string> Name() {
#endif
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            var numBattleMembers = BattleMembers().Count;
#else
            var numBattleMembers = (await BattleMembers()).Count;
#endif
            switch (numBattleMembers)
            {
                case 0:
                    return "";
                case 1:
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                    return Leader().GetNameNoColChar();
#else
                    return (await Leader()).GetNameNoColChar();
#endif
                default:
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
                    return TextManager.Format(TextManager.partyName, Leader().GetNameNoColChar());
#else
                    return TextManager.Format(TextManager.partyName, (await Leader()).GetNameNoColChar());
#endif
            }
        }

        /// <summary>
        /// パーティメンバー中最高のレベルを返す
        /// </summary>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public int HighestLevel() {
            return Members().Aggregate(new List<int>(), (list, member) =>
#else
        public async Task<int> HighestLevel() {
            return (await Members()).Aggregate(new List<int>(), (list, member) =>
#endif
            {
                list.Add(((GameActor) member).Level);
                return list;
            }).Max();
        }

        /// <summary>
        /// 指定金額ぶん所持金を増やす
        /// </summary>
        /// <param name="amount"></param>
        public void GainGold(int amount) {
            Gold = Math.Min(Math.Max(Gold + amount, 0), MaxGold());
            DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.gold = Gold;
        }

        /// <summary>
        /// 最大所持金(規定値:99999999)を返す
        /// </summary>
        /// <returns></returns>
        public int MaxGold() {
            return 99999999;
        }

        /// <summary>
        /// パーティが持っている指定アイテムの数を返す
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int NumItems(GameItem item) {
            var container = ItemContainer(item);
            return container.ContainsKey(item.ItemId) ? container[item.ItemId] : 0;
        }

        /// <summary>
        /// 指定アイテムの最大数(規定値:99)を返す
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public int MaxItems(GameItem item) {
            return 9999;
        }

        /// <summary>
        /// 指定アイテムをパーティが持っているか
        /// </summary>
        /// <param name="item"></param>
        /// <param name="includeEquip"></param>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public bool HasItem(GameItem item, bool includeEquip = false) {
#else
        public async Task<bool> HasItem(GameItem item, bool includeEquip = false) {
#endif
            if (NumItems(item) > 0)
                return true;
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            if (includeEquip && IsAnyMemberEquipped(item))
#else
            if (includeEquip && await IsAnyMemberEquipped(item))
#endif
                return true;
            return false;
        }

        /// <summary>
        /// 指定アイテムをいずれかのメンバーが装備しているか
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public bool IsAnyMemberEquipped(GameItem item) {
            return Members().Any(actor => { return ((GameActor) actor).Equips().Contains(item); });
        }
#else
        public async Task<bool> IsAnyMemberEquipped(GameItem item) {
            return (await Members()).Any(actor => { return ((GameActor) actor).Equips().Contains(item); });
        }
#endif

        /// <summary>
        /// 指定アイテムを増やす
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        /// <param name="includeEquip"></param>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void GainItem(GameItem item, int amount, bool includeEquip = false) {
#else
        public async Task GainItem(GameItem item, int amount, bool includeEquip = false) {
#endif
            var container = ItemContainer(item);
            if (container == null) return;

            var lastNumber = NumItems(item);
            var newNumber = lastNumber + amount;
            container[item.ItemId] = Math.Min(Math.Max(newNumber, 0), MaxItems(item));
            if (container[item.ItemId] == 0) container.Remove(item.ItemId);
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            if (includeEquip && newNumber < 0) DiscardMembersEquip(item, -newNumber);
#else
            if (includeEquip && newNumber < 0) await DiscardMembersEquip(item, -newNumber);
#endif

            //Runtimeのデータに保存
            if (item.DataClass == GameItem.DataClassEnum.Item)
            {
                var runtimeDataModelItems = DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.items;
                var newItem = new RuntimePartyDataModel.Item { itemId = item.ItemId, value = amount };
                var isFlag = false;
                foreach (var rItem in runtimeDataModelItems)
                    if (rItem.itemId == newItem.itemId)
                    {
                        if (container.ContainsKey(item.ItemId))
                            rItem.value = container[item.ItemId];
                        else
                            rItem.value = 0;
                        isFlag = true;
                        break;
                    }

                if (!isFlag)
                    runtimeDataModelItems.Add(newItem);
            }
            else if (item.DataClass == GameItem.DataClassEnum.Weapon)
            {
                var runtimeDataModelWeapons = DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.weapons;
                var weapon = new RuntimePartyDataModel.Weapon { weaponId = item.ItemId, value = amount };
                var isFlag = false;
                foreach (var rItem in runtimeDataModelWeapons)
                    if (rItem.weaponId == weapon.weaponId)
                    {
                        rItem.value = container[item.ItemId];
                        isFlag = true;
                        break;
                    }

                if (!isFlag)
                    runtimeDataModelWeapons.Add(weapon);
            }
            else if (item.DataClass == GameItem.DataClassEnum.Armor)
            {
                var runtimeDataModelArmors = DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.armors;
                var armor = new RuntimePartyDataModel.Armor { armorId = item.ItemId, value = amount };
                var isFlag = false;
                foreach (var rItem in runtimeDataModelArmors)
                    if (rItem.armorId == armor.armorId)
                    {
                        rItem.value = container[item.ItemId];
                        isFlag = true;
                        break;
                    }

                if (!isFlag)
                    runtimeDataModelArmors.Add(armor);
            }
        }

        /// <summary>
        /// 指定の装備を捨てる
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void DiscardMembersEquip(GameItem item, int amount) {
#else
        public async Task DiscardMembersEquip(GameItem item, int amount) {
#endif
            var n = amount;
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            Members().ForEach(actor =>
#else
            (await Members()).ForEach(actor =>
#endif
            {
                while (n > 0 && ((GameActor) actor).IsEquipped(item))
                {
                    ((GameActor) actor).DiscardEquip(item);
                    n--;
                }
            });
        }

        /// <summary>
        /// 指定アイテムを減らす
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        /// <param name="includeEquip"></param>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void LoseItem(GameItem item, int amount, bool includeEquip = false) {
            GainItem(item, -amount, includeEquip);
        }
#else
        public async Task LoseItem(GameItem item, int amount, bool includeEquip = false) {
            await GainItem(item, -amount, includeEquip);
        }
#endif

        /// <summary>
        /// 指定アイテムを消費
        /// </summary>
        /// <param name="item"></param>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void ConsumeItem(GameItem item) {
            if (item.IsItem() && item.Consumable) LoseItem(item, 1);
        }
#else
        public async Task ConsumeItem(GameItem item) {
            if (item.IsItem() && item.Consumable) await LoseItem(item, 1);
        }
#endif

        /// <summary>
        /// 指定アイテムが使用可能か
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public bool CanUse(GameItem item) {
            return Members().Any(actor => { return actor.CanUse(item); });
        }
#else
        public async Task<bool> CanUse(GameItem item) {
            foreach (var actor in await Members())
            {
                if (await actor.CanUse(item)) return true;
            }
            return false;
        }
#endif

        /// <summary>
        /// 入力可能か
        /// </summary>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public bool CanInput() {
            return Members().Any(actor => { return actor.CanInput(); });
        }
#else
        public async Task<bool> CanInput() {
            return (await Members()).Any(actor => { return actor.CanInput(); });
        }
#endif

        /// <summary>
        /// 全バトラーが死亡したか
        /// </summary>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public new bool IsAllDead() {
            if (base.IsAllDead())
                return InBattle() || !IsEmpty();
            return false;
        }
#else
        public new async Task<bool> IsAllDead() {
            if (await base.IsAllDead())
                return InBattle() || !await IsEmpty();
            return false;
        }
#endif

        /// <summary>
        /// 最後に選択されたアイテムを返す
        /// </summary>
        /// <returns></returns>
        public GameItem LastItem() {
            return new GameItem(DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.lastItem.itemId, GameItem.DataClassEnum.Item);
        }

        /// <summary>
        /// 指定アイテムを最後のアイテムに設定
        /// </summary>
        /// <param name="item"></param>
        public void SetLastItem(GameItem item) {
            DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel.lastItem.itemId = item.ItemId;
        }

        /// <summary>
        /// 指定[パーティ能力]を持つアクターがいるか
        /// </summary>
        /// <param name="abilityId"></param>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public bool PartyAbility(int abilityId) {
            return BattleMembers().Any(actor => { return actor.PartyAbility(abilityId); });
        }
#else
        public async Task<bool> PartyAbility(int abilityId) {
            return (await BattleMembers()).Any(actor => { return actor.PartyAbility(abilityId); });
        }
#endif

        /// <summary>
        /// [エンカウント半減]のパーティ能力を持つか
        /// </summary>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public bool HasEncounterHalf() {
            return PartyAbility(AbilityEncounterHalf);
        }
#else
        public async Task<bool> HasEncounterHalf() {
            return await PartyAbility(AbilityEncounterHalf);
        }
#endif

        /// <summary>
        /// [エンカウント無効]のパーティ能力を持つか
        /// </summary>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public bool HasEncounterNone() {
            return PartyAbility(AbilityEncounterNone);
        }
#else
        public async Task<bool> HasEncounterNone() {
            return await PartyAbility(AbilityEncounterNone);
        }
#endif

        /// <summary>
        /// [不意打ち無効]のパーティ能力を持つか
        /// </summary>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public bool HasCancelSurprise() {
            return PartyAbility(AbilityCancelSurprise);
        }
#else
        public async Task<bool> HasCancelSurprise() {
            return await PartyAbility(AbilityCancelSurprise);
        }
#endif

        /// <summary>
        /// [先制攻撃率アップ]のパーティ能力を持つか
        /// </summary>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public bool HasRaisePreemptive() {
            return PartyAbility(AbilityRaisePreemptive);
        }
#else
        public async Task<bool> HasRaisePreemptive() {
            return await PartyAbility(AbilityRaisePreemptive);
        }
#endif

        /// <summary>
        /// [獲得金額2倍]のパーティ能力を持つか
        /// </summary>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public bool HasGoldDouble() {
            return PartyAbility(AbilityGoldDouble);
        }
#else
        public async Task<bool> HasGoldDouble() {
            return await PartyAbility(AbilityGoldDouble);
        }
#endif

        /// <summary>
        /// [アイテム入手率2倍]のパーティ能力を持つか
        /// </summary>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public bool HasDropItemDouble() {
            return PartyAbility(AbilityDropItemDouble);
        }
#else
        public async Task<bool> HasDropItemDouble() {
            return await PartyAbility(AbilityDropItemDouble);
        }
#endif

        /// <summary>
        /// 指定敵素早さに対して先制攻撃の確率を返す
        /// </summary>
        /// <param name="troopAgi"></param>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public double RatePreemptive(double troopAgi) {
#else
        public async Task<double> RatePreemptive(double troopAgi) {
#endif
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
            var rate = Agility() >= troopAgi ? 0.05 : 0.03;
            if (HasRaisePreemptive()) rate *= 4;
#else
            var rate = await Agility() >= troopAgi ? 0.05 : 0.03;
            if (await HasRaisePreemptive()) rate *= 4;
#endif
            return rate;
        }

        /// <summary>
        /// 指定敵素早さに対して不意打ちの確率を返す
        /// </summary>
        /// <param name="troopAgi"></param>
        /// <returns></returns>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public double RateSurprise(double troopAgi) {
            var rate = Agility() >= troopAgi ? 0.03 : 0.05;
            if (HasCancelSurprise()) rate = 0;
            return rate;
        }
#else
        public async Task<double> RateSurprise(double troopAgi) {
            var rate = await Agility() >= troopAgi ? 0.03 : 0.05;
            if (await HasCancelSurprise()) rate = 0;
            return rate;
        }
#endif

        /// <summary>
        /// パーティ全体の勝利モーションを開始
        /// </summary>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void PerformVictory() {
            Members().ForEach(actor => { ((GameActor) actor).PerformVictory(); });
        }
#else
        public async Task PerformVictory() {
            (await Members()).ForEach(actor => { ((GameActor) actor).PerformVictory(); });
        }
#endif

        /// <summary>
        /// パーティ全体の逃亡モーションを開始
        /// </summary>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void PerformEscape() {
            Members().ForEach(actor => { ((GameActor) actor).PerformEscape(); });
        }
#else
        public async Task PerformEscape() {
            (await Members()).ForEach(actor => { ((GameActor) actor).PerformEscape(); });
        }
#endif

        /// <summary>
        /// 全パーティメンバーのステートを削除
        /// </summary>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void RemoveBattleStates() {
            Members().ForEach(actor => { actor.RemoveBattleStates(); });
        }
#else
        public async Task RemoveBattleStates() {
            foreach (var actor in await Members()) {
                await actor.RemoveBattleStates();
            }
        }
#endif

        /// <summary>
        /// 全パーティメンバーのモーションを初期化
        /// </summary>
#if (UNITY_EDITOR && !UNITE_WEBGL_TEST) || !UNITY_WEBGL
        public void RequestMotionRefresh() {
            Members().ForEach(actor => { actor.RequestMotionRefresh(); });
        }
#else
        public async Task RequestMotionRefresh() {
            (await Members()).ForEach(actor => { actor.RequestMotionRefresh(); });
        }
#endif

        /// <summary>
        /// 所持アイテム（アイテム、武器、防具）の再設定
        /// </summary>
        public void ResetPartyItems() {
            //所持アイテム初期化
            _items.Clear();
            _weapons.Clear();
            _armors.Clear();

            //保持しているアイテムデータを設定
            new List<RuntimePartyDataModel.Item>(DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel
                .items).ForEach(item => { _items[item.itemId] = item.value; });
            //保持している武器データを設定
            new List<RuntimePartyDataModel.Weapon>(DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel
                .weapons).ForEach(
                item => { _weapons[item.weaponId] = item.value; });
            //保持している防具データを設定
            new List<RuntimePartyDataModel.Armor>(DataManager.Self().GetRuntimeSaveDataModel().runtimePartyDataModel
                .armors).ForEach(
                item => { _armors[item.armorId] = item.value; });
        }
    }
}