using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPawn {


    void MoveTo(Vector3 _destination);
    void Look(Vector3 _look);
    void SetAggro(bool _b);

    CController cc
    {
        get;
    }
}
