
// 광고 시청 후 시청 여부에 따른 callback
void OnSuccessRewardAdResult(bool result) // 5 스테이지마다 얻는 상자 보상과 관련된 광고 보상 함수
{
    if (result)
    {
        int rvrewardmulti = SystemValueScript.Instance.GetRVRewardMulti(); // 보상을 몇 배로 줄 것인가?

        LinkADButton linkADButton = clickedAdButton.GetComponent<LinkADButton>(); // 누른 버튼의 게임 데이터 활용
        int cardNum = linkADButton.buttonNum; // 몇 번째 버튼인지 확인
        int cardReward = linkADButton.rewardNum; // 보상 갯수 확인

        if (clickedAdButton.GetComponent<LinkADButton>().buttonType == ADRewardType.Weapon)
        {
            PlayerScript.Instance.AddWeaponCard(linkADButton.weaponCode, cardReward * (rvrewardmulti - 1)); // 해당 무기의 카드 갯수를 한번 더 더해줌

            BoxRewardPage.CardRewards[cardNum].RewardCount.text = (cardReward * rvrewardmulti).ToString();

            if (PlayerScript.Instance.GetWeaponCard(linkADButton.weaponCode) > WeaponScript.Instance.GetWeaponData(linkADButton.weaponCode).UpgradeCard) // 광고로 인해 추가 카드 보상 획득 후 업그레이드가 가능하면 업그레이드 버튼 활성화
                BoxRewardPage.CardRewards[cardNum].UpgradeButton.SetActive(true);
        }
        else if (clickedAdButton.GetComponent<LinkADButton>().buttonType == ADRewardType.Dollar)
        {
            PlayerScript.Instance.GainGold(cardReward * (rvrewardmulti - 1), "");

            BoxRewardPage.DollarReward.RewardCount.text = (cardReward * rvrewardmulti).ToString();

            SetRewardGoldAni(PlayerScript.Instance.Gold - (cardReward * (rvrewardmulti - 1)), PlayerScript.Instance.Gold, BoxRewardPage.DollarReward.transform, endGoldTransform);
        }
        else
        {
            PlayerScript.Instance.LauncherRocketCount += cardReward * (rvrewardmulti - 1);

            BoxRewardPage.LauncherReward.RewardCount.text = (cardReward * rvrewardmulti).ToString();
        }

        BoxRewardPage.CheckWeaponUpgradeable();

        clickedAdButton.SetActive(false); // 눌렀던 버튼 비활성화

        int onebased_stagenumber = StageScript.Instance.PlayerStage; // 결과창은 다음 스테이지
        SupercentClient.Instance.GAProgressionEventRewardAdSuccess(onebased_stagenumber);
    }
    else
    {
        GameUI.Instance.toastmessage.SetInfo(TableManager.Instance.GetStringData("RewardFail"));
    }
}

/// <summary>
/// 
/// </summary>
/// <param name="dollarOffset">로직 상 획득할 달러 보상이 들어오기 전에 업그레이드 가능 여부를 확인한다면, 해당 값에 획득할 달러 보상 값을 미리 넣어 확인 가능</param>
public void CheckWeaponUpgradeable(int dollarOffset = 0)
{
    for (int i = 0; i < CardRewards.Length; i++)
    {
        if (CardRewards[i].gameObject.activeSelf) // 켜져 있다 -> 총 업그레이드 여부 검사
        {
            string weaponcode = cardrewardweapons[i % cardrewardweapons.Count];
            WeaponData weapondata = WeaponScript.Instance.GetWeaponData(weaponcode);

            if (PlayerScript.Instance.GetWeaponCard(weaponcode) >= weapondata.UpgradeCard && PlayerScript.Instance.Gold + dollarOffset >= weapondata.UpgradeCost) // 업그레이드 가능하면, 업그레이드 버튼 활성화
                CardRewards[i].UpgradeButton.SetActive(true);
        }
    }
}

// 광고 버튼이 보여질 카드 선택
void ShuffleForAdButtonActivate(SupplyBoxType boxType, bool emptyWorstWeapon, bool haveDollarReward, bool haveLauncherReward)
{
    int adBtnTotal = 0;
    switch (boxType) // 총 보여질 광고 버튼 갯수
    {
        case SupplyBoxType.SupplyBoxBasic:
            adBtnTotal = 2;
            break;
        case SupplyBoxType.SupplyBoxNormal:
        case SupplyBoxType.SupplyBoxMedium:
            adBtnTotal = 3;
            break;
    }

    // 광고 보상이 붙을 수 있는 카드를 위한 shuffle 진행
    int totalRewardBtn = (emptyWorstWeapon ? CardRewards.Length - 1 : CardRewards.Length) // worstweapon이 비어있다면 보상 카드는 3장, 존재한다면 카드는 4장
                            + (haveDollarReward ? 1 : 0) // 달러 보상 카드가 존재할 것인가
                            + (haveLauncherReward ? 1 : 0); // 런처 보상 카드가 존재할 것인가

    List<int> adBtnAttach = new List<int>();
    for (int i = 0; i < totalRewardBtn; i++)
        adBtnAttach.Add(i);
    Shuffle(adBtnAttach);

    for (int i = 0; i < adBtnTotal; i++) // 셔플 된 버튼들의 광고 버튼 활성화
    {
        if (adBtnAttach[i] < CardRewards.Length - 1) // 총 보상 카드
        {
            CardRewards[adBtnAttach[i]].ADButton.SetActive(true); // 해당 버튼의 광고 버튼 active true
        }
        else
        {
            if (emptyWorstWeapon) // CardRewards.Length - 1-> dollar, CardRewards.Length -> launcher
            {
                // 가장 안좋은 카드 보상이 없다 -> 코드 로직 상 맨 마지막 무기 보상 카드는 activate 되지 않는다.
                if (adBtnAttach[i] == CardRewards.Length - 1)
                    DollarReward.ADButton.SetActive(true); // 현재 달러 버튼 쪽 광고 버튼 없음, 추가 시 해당 코드로 변경
                else
                {
                    if (haveLauncherReward)
                        LauncherReward.ADButton.SetActive(true);
                    else
                        Debug.Assert(haveLauncherReward, "No Launcher Reward, but reward card was set");
                }

            }
            else // CardRewards.Length - 1 -> weapon, CardRewards.Length -> dollar, CardRewards.Length + 1 -> launcher
            {
                if (adBtnAttach[i] == CardRewards.Length - 1)
                {
                    CardRewards[adBtnAttach[i]].ADButton.SetActive(true); // 해당 버튼의 광고 버튼 active true
                }
                else if (adBtnAttach[i] == CardRewards.Length)
                {
                    //Debug.Log("Dollar Ad Button Activate");
                    DollarReward.ADButton.SetActive(true);
                }
                else
                {
                    if (haveLauncherReward)
                        LauncherReward.ADButton.SetActive(true);
                    else
                        Debug.Assert(haveLauncherReward, "No Launcher Reward, but reward card was set");
                }
            }
        }
    }
}

// Fisher-Yates 셔플 알고리즘
void Shuffle(List<int> list)
{
    for (int i = list.Count - 1; i > 0; i--)
    {
        int j = Random.Range(0, i + 1);
        int temp = list[i];
        list[i] = list[j];
        list[j] = temp;
    }
}

// 보상 상자 생성 및 연출
public IEnumerator ShowRewardBoxAnim(SupplyBoxType supplyboxtype)
{
    // 스테이지 클리어 후 변경된 timescale 롤백
    Time.timeScale = 1f;

    // 1. 씬에 box prefab 생성
    // 2. 해당 프리팹은 virtualCamera 및 fov 50 (기존 카메라 fov 값이 50이라 배경 크기가 해당 fov에 맞춰져 있음), priority를 0으로 세팅
    // 3. HuntCamera (CinemachineBrain)에서 RewardBox 쪽 카메라로 전환되는 방식을 양방향 Cut으로 바꿀 것 -> 기본적으로 세팅해주지 않으면 Cut으로 되어있네요.
    // 4. 총 게이지 차는거 보여준 후, HuntResult의 Dim (1) 을 끄고 RewardBox_01 우선순위를 높여 상자를 보여줄 것

    GameObject boxObject;
    switch (supplyboxtype)
    {
        case SupplyBoxType.SupplyBoxBasic:
            boxObject = RewardBox_Basic;
            break;
        case SupplyBoxType.SupplyBoxNormal:
            boxObject = RewardBox_Normal;
            break;
        case SupplyBoxType.SupplyBoxMedium:
            boxObject = RewardBox_Medium;
            break;
        default:
            boxObject = RewardBox_Basic;
            break;
    }

    GameObject rewardBox = Instantiate(boxObject, new Vector3(0f, 500f, 0f), Quaternion.identity); // 높이에 따라 너무 낮으면 맵 오브젝트에 가려서 어둡게 보일 수 있다. 조절 요망

    // 우선순위를 높여 박스쪽 카메라 활성화
    CinemachineVirtualCamera cvc = rewardBox.GetComponentInChildren<CinemachineVirtualCamera>();
    cvc.m_Priority = 100;

    yield return null; // 오브젝트가 생성되고 카메라 우선순위도 바뀌었음을 확정하기 위한 1프레임 대기

    // GameObject인 Background 을 잠시 activeFalse해 ui가 아닌 상자 object가 보이도록 함
    UIBackground.SetActive(false);

    rewardBox.GetComponent<Animator>().enabled = true;

    UIAudio.Instance.PlayUISound(SoundScript.Instance.RewardBoxOpen);

    yield return new WaitForSecondsRealtime(rewardBox.GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length + 0.5f); // 해당 애니메이션이 끝난 후

    ShowSupplyBoxReward(supplyboxtype); // 그 후에 리워드 표시
}
    }