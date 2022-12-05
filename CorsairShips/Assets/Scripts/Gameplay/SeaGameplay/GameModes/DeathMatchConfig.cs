using System;
using System.Collections;
using System.Collections.Generic;
using Tools;
using UnityEngine;

public class DeathMatchConfig : SingletonScriptableObject<DeathMatchConfig> {
    [SerializeField]
    private DeathMatchParameters _DeathMatchParameters;

    public DeathMatchParameters DeathMatchParameters => _DeathMatchParameters;
}

[Serializable]
public class DeathMatchParameters {
    [Range(1, 50)]
    public int PlayersCount;
    public float EndScreenShowDelay;
}
