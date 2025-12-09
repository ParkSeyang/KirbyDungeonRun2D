using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// 스탯 및 변신관련 로직 및 상호작용 기능 작성
public class Player: PlayerStat
{
    [SerializeField] public AbilityType PlayerAbility;
    public static Player instance;
    
    public void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        Hp = 100.0f;
        Life = 3;
        
        StatSetting();
    }

    public void StatSetting()
    {
        
        switch (PlayerAbility)
        {
            case AbilityType.Normal:
                Name = "NormalKirby";
                Attack = 5.0f;
                break;
            case AbilityType.Sword:
                name = "SwordKirby";
                Attack = 15.0f;
                break;
            case AbilityType.Cutter:
                name = "CutterKirby";
                Attack = 12.0f;
                break;
            case AbilityType.Bomber:
                name = "BomberKirby";
                Attack = 20.0f;
                break;
            default:
                break;
        }
    }
    
    
}
