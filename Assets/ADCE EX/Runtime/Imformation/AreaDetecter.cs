using System.Collections;
using System.Collections.Generic;
using AD.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace AD.Experimental.GameEditor
{
    public class AreaDetecter : ADUI
    {
        public string Message = "";

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);
            GameEditorApp.instance.GetController<Information>().Log(Message);
        }
    }
}
