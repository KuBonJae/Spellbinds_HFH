// 가챠 연출 구현

[Header("Reward Box Prefab")]
public GameObject RewardBox;
[Header("UI Background")]
public GameObject UIBackground;

IEnumerator ShowRewardBoxAnim(StageData stagedata)
{
    // 1. 씬에 box prefab 생성
    // 2. 해당 프리팹은 virtualCamera 및 fov 50 (기존 카메라 fov 값이 50이라 배경 크기가 해당 fov에 맞춰져 있음), priority를 0으로 세팅
    // 3. HuntCamera (CinemachineBrain)에서 RewardBox 쪽 카메라로 전환되는 방식을 양방향 Cut으로 바꿀 것 -> 기본적으로 세팅해주지 않으면 Cut으로 되어있네요.
    // 4. 총 게이지 차는거 보여준 후, HuntResult의 Dim (1) 을 끄고 RewardBox_01 우선순위를 높여 상자를 보여줄 것

    GameObject rewardBox = Instantiate(RewardBox, new Vector3(0f, -100f, 0f), Quaternion.identity); // 오브젝트가 생성되는 scene이 변경되어야 한다면 activescene 변경 필요
    GameObject boxPrefab = rewardBox.GetComponent<RewardBoxChildData>().boxModel;

    // 우선순위를 높여 박스쪽 카메라 활성화
    CinemachineVirtualCamera cvc = rewardBox.GetComponentInChildren<CinemachineVirtualCamera>();
    cvc.m_Priority = 100;

    yield return null; // 오브젝트가 생성되고 카메라 우선순위도 바뀌었음을 확정하기 위한 1프레임 대기

    // GameObject인 Dim (1) 을 잠시 activeFalse해 ui가 아닌 상자 object가 보이도록 함
    UIBackground.SetActive(false);

    // 타임스케일이 다르므로 속도 조절
    rewardBox.GetComponent<Animator>().enabled = true;
    rewardBox.GetComponent<Animator>().speed /= Time.timeScale; // 상자 애니메이션 시작 및 타임 스케일에 따른 속도 조절

    // 상자의 RingLine_02 파티클은 특정 지점에 폭죽처럼 터지는 파티클이라 실행되면 이동이 불가능함, 파티클 속도를 조정해줘야 할듯
    ParticleSystem[] fireworkParticles = rewardBox.GetComponent<RewardBoxChildData>().firework.GetComponentsInChildren<ParticleSystem>();
    ParticleSystem[] pillarParticles = rewardBox.GetComponent<RewardBoxChildData>().lightpillar.GetComponentsInChildren<ParticleSystem>();

    for (int i = 0; i < (fireworkParticles.Length > pillarParticles.Length ? fireworkParticles.Length : pillarParticles.Length); i++)
    {

        if (i < fireworkParticles.Length)
        {
            var psMain = fireworkParticles[i].main;
            psMain.simulationSpeed /= Time.timeScale;
        }

        if (i < pillarParticles.Length)
        {
            var psMain = pillarParticles[i].main;
            psMain.simulationSpeed /= Time.timeScale;
        }
    }

    yield return new WaitForSecondsRealtime(rewardBox.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length + 0.5f); // 해당 애니메이션이 끝난 후

    BoxRewardPage.ShowSupplyBoxReward(stagedata.SpecialReward[1]); // 그 후에 리워드 표시
}


// 코드 유지 보수 용 class 추가
public class RewardBoxChildData : MonoBehaviour
{
    // RewardBox의 내부 Child를 가져오기 위해 만들어진 Script 입니다.

    [Header("Box Model")]
    public GameObject boxModel;

    [Header("Firework Particle")]
    public GameObject firework;

    [Header("Lightpillar Particle")]
    public GameObject lightpillar;
}