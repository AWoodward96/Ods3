using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPawn  {

    List<PawnCommand> MyCommands
    {
        get;
    }

}
