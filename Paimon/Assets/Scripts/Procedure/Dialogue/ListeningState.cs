using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class ListeningState
{
    public void Enter()
    {
        var dialogueModel = Global.Instance.DialogueGO.GetComponent<DialogueModel>();
        dialogueModel.Name = "旅行者";
        dialogueModel.UserContent = "<color=#218de6>等待用户说话~</color>";
    }

    public void Update()
    {

    }
    public void Exit()
    {
    }
}

