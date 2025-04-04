using RPGMaker.Codebase.Editor.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.Hierarchy.Region.Base.View
{
    public class HierarchyWindow : BaseWindow
    {
        protected void Awake() {
            titleContent = new GUIContent(EditorLocalize.LocalizeText("WORD_1561"));
            rootVisualElement.focusable = true;
            rootVisualElement.RegisterCallback<KeyDownEvent>(e =>
            {
                e.StopPropagation();
                switch (e.keyCode)
                {
                    case KeyCode.UpArrow:
                        Hierarchy.SelectPrevItem();
                        break;
                    case KeyCode.DownArrow:
                        Hierarchy.SelectNextItem();
                        break;
                }
            });
        }

        private void OnFocus() {
            rootVisualElement.Focus();
        }
    }
}