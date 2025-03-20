using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.YamlDotNet.Core.Tokens;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RPGMaker.Codebase.Editor.Common
{

    [FilePath("ProjectSettings/RPGMakerDefaultConfigFlags.asset", FilePathAttribute.Location.ProjectFolder)]
    public class RPGMakerDefaultConfigSingleton : ScriptableSingleton<RPGMakerDefaultConfigSingleton>
    {
        public enum INITIALIZE_STATE
        {
            UNINITIALIZED,
            INITIALIZED,
            INITIALIZING,
            INITIALIZING_REQUIRE_REBOOT,
        }

        public const string InitializeModeId = "initialize";
        public const string RpgMakerUniteModeId = "rpgmaker";
        public const string DefaultEditorModeId = "default";
        public const string RpgMakerUniteWindowModeId = "rpgmaker_window";

        [SerializeField] private bool m_defaultSettingsConfigured;
        [SerializeField] private string m_uniteMode = DefaultEditorModeId;
        [SerializeField] private bool m_revertLayoutSetting = true;
        [SerializeField] private SystemLanguage m_uniteEditorLanguage = SystemLanguage.Unknown;

        [SerializeField] private int m_EventComanedMode = 1;//初期時にコマンド一覧を有効へ
        [SerializeField] private bool m_bEventComanedOptionalCommandsFast = false;//よく使うポンタを先頭へ

        private const string DIALOG_TITLE = "RPGMaker Unite";
        private const string CONFIG_OVERWRITE_TEXT = "WORD_5015"; //RPGMaker Unite requires to overwrite your project...
        private const string OVERWRITE_TEXT = "WORD_5017"; //Overwrite
        private const string CANCEL_TEXT = "WORD_5020"; //Cancel

        public int EventComanedMod
        {
            get { return m_EventComanedMode; }
            set { m_EventComanedMode = value; }
        }

        public bool EventComanedOptionalCommandsFast
        {
            get { return m_bEventComanedOptionalCommandsFast; }
            set
            {
                var oldValue = m_bEventComanedOptionalCommandsFast;
                m_bEventComanedOptionalCommandsFast = value;
                if (oldValue != value)
                {
                    Save(true);
                }
            }
        }

        private bool DefaultSettingsConfigured
        {
            get => m_defaultSettingsConfigured;
            set
            {
                var oldValue = m_defaultSettingsConfigured;
                m_defaultSettingsConfigured = value;
                if (oldValue != value)
                {
                    Save(true); 
                }
            }
        }

        public string UniteMode
        {
            get => m_uniteMode;
            set
            {
                m_uniteMode = value;
                Save(true);
            }
        }

        public bool RevertLayoutSetting
        {
            get => m_revertLayoutSetting;
            set
            {
                var oldValue = m_revertLayoutSetting;
                m_revertLayoutSetting = value;
                if (oldValue != value)
                {
                    Save(true);
                }
            }
        }

        public SystemLanguage UniteEditorLanguage
        {
            get
            {
                if (m_uniteEditorLanguage == SystemLanguage.Unknown)
                {
                    m_uniteEditorLanguage = EditorLocalize.GetNowLanguage();
                }
                return m_uniteEditorLanguage;
            }
            set
            {
                m_uniteEditorLanguage = value;
                Save(true);
            }
        }

        static internal INITIALIZE_STATE InitializeDefaultSettingsForRPGMakerUnite()
        {
            // OVERWRITEのダイアログにOKを一度でもしたら初期化済み扱いになる = INITIALIZED
            // ダイアログに「OK」したときにPlayerSettingsが見つからなかった場合 = INITIALIZING
            // ダイアログに「OK」したときにPlayerSettingsが見つかってactiveInputHandlerが2以外の時 = INITIALIZING_REQUIRE_REBOOT
            // ダイアログに「OK」しなかったとき = UNINITIALIZED
            if (instance.DefaultSettingsConfigured)
            {
                // 初期化済み
                return INITIALIZE_STATE.INITIALIZED;
            }

            SortingLayerHelper.ConfigureDefaultSettings();
            TagManagerHelper.ConfigureDefaultSettings();
            ProjectSettingsHelper.ConfigureDefaultSettings();
            GraphicsSettingsHelper.ConfigureDefaultSettings();
            EditorBuildSettingsHelper.ConfigureDefaultSettings();

            AssetDatabase.SaveAssets();

            instance.DefaultSettingsConfigured = true;
            instance.UniteMode = InitializeModeId;

            // Uniteは、新InputSystemが有効ではないと動かないため、Activeにする
            var playerSettings = Resources.FindObjectsOfTypeAll<PlayerSettings>().FirstOrDefault();
            if (playerSettings != null)
            {
                var playerSettingsObject = new SerializedObject(playerSettings);
                var property = playerSettingsObject.FindProperty("activeInputHandler");
                if (property.intValue != 2)
                {
                    property.intValue = 2;
                    property.serializedObject.ApplyModifiedProperties();
                }
            }
            //初期化、要再起動
            return INITIALIZE_STATE.INITIALIZING_REQUIRE_REBOOT;
        }
    }
}