using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// 스탯 및 변신관련 로직 및 상호작용 기능 작성
public class Player: PlayerStat
{
    [SerializeField] public AbilityType PlayerAbility;
    public static Player instancePlayer;
    
    public void Awake()
    {
        if (instancePlayer == null)
        {
            instancePlayer = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        StatSetting();
        
    }

    public void StatSetting()
    {
        
        Hp = 100.0f;
        Life = 3;
        switch (PlayerAbility)
        {
            case AbilityType.Normal:
                Attack = 5;
                break;
            case AbilityType.Sword:
                Attack = 15;
                break;
            case AbilityType.Cutter:
                Attack = 12;
                break;
            case AbilityType.Bomber:
                Attack = 20;
                break;
            default:
                break;
        }
    }
    
    
}
