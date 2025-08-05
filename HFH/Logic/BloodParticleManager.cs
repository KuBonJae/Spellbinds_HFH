using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BloodParticleManager : MonoBehaviour
{
    ParticleSystem particleSystem;
    [Header("부모에 있는 MonsterBase 할당")]
    public MonsterBase monsterBase;
    [Header("바닥에서 실행 되어야 하는 Particle을 가진 자식 Object")]
    public GameObject[] particleChildrenGround;
    [Header("타격 방향에 따라 회전이 들어가야 하는가")]
    public bool mustRotate = false;

    private void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
        Debug.Assert(particleSystem != null, "No ParticleSystem Component is set.");
        if (monsterBase == null)
            monsterBase = GetComponentInParent<MonsterBase>();
        Debug.Assert(monsterBase != null, "No MonsterBase Script is set");
    }

    public void PlayParticle()
    {
        if (particleSystem != null)
            particleSystem.Play();
    }

    Vector3 yDelete = Vector3.zero;

    public void PlayParticleWithAdjustingPosition()
    {
        if (monsterBase != null)
        {
            float particleYPos = gameObject.transform.position.y;
            float yOffset = particleYPos - monsterBase.hitPosition.y;
            gameObject.transform.position = monsterBase.hitPosition;

            for (int i = 0; i < particleChildrenGround.Length; i++)
            {
                particleChildrenGround[i].transform.position += Vector3.up * yOffset; // 변경되는 높이 만큼 ground 값을 보정해줘야 땅바닥에 particle이 생성됨

                var main_ground = particleChildrenGround[i].GetComponent<ParticleSystem>().main;
                main_ground.startDelay = Mathf.Sqrt((2 * gameObject.transform.position.y) / Physics.gravity.magnitude * particleGravityScale) / 3f; // 바닥에 생기는 particle은 중력에 따라 떨어지는 속도를 계산해 시간을 조정, 임의로 3을 나눠 좀 더 빠르게 바닥에 생기도록 조정
            }

            if (mustRotate)
            {
                Quaternion parentRot = Quaternion.Euler(monsterBase.gameObject.transform.rotation.eulerAngles);
                Vector3 parentDir = parentRot * Vector3.forward; // 현재 부모가 바라보고 있는 world에서의 방향
                yDelete.y = monsterBase.hitDirection.y;
                Vector3 hitDirection = monsterBase.hitDirection - yDelete; // 총알 방향 중에 y 값을 없애 수평 방향으로 particle이 튀어나가도록 변경

                Quaternion rotation1 = Quaternion.FromToRotation(parentDir, hitDirection.normalized);
                Quaternion rotation2 = gameObject.transform.localRotation;
                gameObject.transform.localRotation = rotation1 * Quaternion.Inverse(rotation2) * gameObject.transform.localRotation; // 타격 direction에 맞춰 그만큼 회전시켜준다
            }
        }

        PlayParticle();
    }

    const float particleGravityScale = 2f; // 현재 Particle의 중력 가속도 배율, 자주 변경된다면 Prefab을 미리 받아서 사용할 것
}
