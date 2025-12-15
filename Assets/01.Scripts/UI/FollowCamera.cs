using System;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public enum States
    {
        Stopped,
        Holding,
        Following,
    }
    
    [field: SerializeField] private Transform Target { get; set; }
    private Camera Cam { get; set; }
    private States State { get; set; } = States.Stopped;

    [field: SerializeField] private float lerpSpeed = 1.0f; //Damping 값
    [SerializeField] private Vector3 offset;
    
    private void Update()
    {
        //테스트 코드
        if (Input.GetKeyDown(KeyCode.F1))
        {
            ChangeState(States.Stopped);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ChangeState(States.Holding);
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            SetTarget(Player.instance.transform);
            ChangeState(States.Following);
        }
        if (Input.GetKeyDown(KeyCode.F4))
        {
            ChangeState(States.Following);
        }
        
        switch (State)
        {
            case States.Stopped:
                Target = null;
                // 멈춤 상태로 되면 Target을 비워버림
                return;
            case States.Holding:
                // Holding은 Target은 존재하지만 카메라를 이동하지 않는 상태
                break;
            case States.Following:
                // Taget의 위치를 적절히 보간
                Vector3 nextPosition = Vector3.Lerp(transform.position, Target.position + offset, lerpSpeed * Time.deltaTime);
                nextPosition.z = transform.position.z;
                transform.position = nextPosition;
                break;
            default:
                break;
        }
    }

    public void SetTarget(Transform target)
    {
        Target = target;
    }
    
    public void ChangeState(States newState)
    {
        State = newState;
    }
}