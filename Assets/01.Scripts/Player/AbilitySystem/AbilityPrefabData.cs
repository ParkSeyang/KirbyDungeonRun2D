using System;
using UnityEngine;

[Serializable]
public class AbilityPrefabData
{
    // 능력타입 과 대조해서 커비 변신 프리팹 매핑용 데이터 묶음.
    public BaseStat.AbilityType abilityType;
    public GameObject abilityPrefab;

}
