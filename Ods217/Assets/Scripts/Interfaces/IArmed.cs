using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IArmed : IDamageable {

    HealthBar myVisualizer
    {
        get;
    }

    IWeapon myWeapon
    {
        get;
    }
     
    
}
