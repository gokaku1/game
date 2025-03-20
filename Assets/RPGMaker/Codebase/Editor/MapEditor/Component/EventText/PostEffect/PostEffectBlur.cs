﻿using RPGMaker.Codebase.CoreSystem.Knowledge.DataModel.Event;
using RPGMaker.Codebase.Editor.Common;
using UnityEngine.UIElements;

namespace RPGMaker.Codebase.Editor.MapEditor.Component.EventText.PostEffect
{
    public class PostEffectBlur : AbstractEventText, IEventCommandView
    {
        public override VisualElement Invoke(string indent, EventDataModel.EventCommand eventCommand)
        {
            ret = indent;
            ret += "◆" + EditorLocalize.LocalizeText("WORD_6002") + " : ";

            ret += eventCommand.parameters[3] + ", ";

            if (eventCommand.parameters[4] == "1") ret += EditorLocalize.LocalizeText("WORD_6027") + " ";
            if (eventCommand.parameters[5] == "1") ret += EditorLocalize.LocalizeText("WORD_6028") + " ";
            if (eventCommand.parameters[6] == "1") ret += EditorLocalize.LocalizeText("WORD_6029") + " ";
            
            if (eventCommand.parameters[0] == "1")
            {
                // マップが変わるまで
                ret += EditorLocalize.LocalizeText("WORD_6103");
            }
            else
            {
                // フレーム指定
                ret += eventCommand.parameters[1] + " " + EditorLocalize.LocalizeText("WORD_1088") + " ";
                if (eventCommand.parameters[2] == "1") ret += "(" + EditorLocalize.LocalizeText("WORD_1087") + ")";
            }
            
            LabelElement.text = ret;
            Element.Add(LabelElement);
            return Element;
        }
    }
}