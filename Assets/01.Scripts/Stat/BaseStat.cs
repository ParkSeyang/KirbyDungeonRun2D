using UnityEngine;

// 기본적인 스탯 기능을 추가
public class BaseStat : MonoBehaviour
{
    [field: SerializeField] public string Name { get; set; }
    [field: SerializeField] public int Life { get; set; }

    [field: SerializeField] public float Hp { get; set; }
    
    [field: SerializeField] public float Attack { get; set; }
    
    public enum AbilityType
    { 
        Normal,
        Sword,
        Cutter,
        Bomber,
    }
      
    /* Player 에게 들어 가야할것
     * 1. 싱글톤을 만들기
     * 2. Stat들 작성
     */
    

}
