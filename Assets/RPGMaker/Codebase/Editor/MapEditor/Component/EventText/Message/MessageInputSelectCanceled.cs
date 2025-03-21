using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.Message
{
    public class MessageInputSelectCanceled : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand) {
            ret = indent;
            Element.style.flexDirection = FlexDirection.Row;
            Element.style.alignItems = Align.Center;

            var body = $"{EditorLocalize.LocalizeTextFormat("WORD_1207", EditorLocalize.LocalizeText("WORD_1530"))}";
            ret = indent + "◇" + body;

            AddFoldout(indent, eventCommand);

            LabelElement.text = body;
            Element.Add(LabelElement);
            return Element;
        }
    }
}