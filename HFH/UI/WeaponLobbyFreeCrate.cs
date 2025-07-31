public class WeaponLobbyFreeCrate : MonoBehaviour, IPointerClickHandler
{
    public GameObject rewardBoxPrefab;
    public GameObject m_Canvas;
    public TextMeshProUGUI m_Text;
    [Header("Box Reward 연출 시 숨겨야 할 UI")]
    public GameObject[] WeaponLobbyUI;

    // Test 버전에서는 Basic만 활용, 추후 변경 요망
    SupplyBoxType supplyBoxType = SupplyBoxType.SupplyBoxBasic;

    const string CooldownKey = "CrateLastUsedTime";
    const string RefilledBoxCount = "WaitingLobbyWeaponADCrate";
    const int BoxMaxCount = 3;
    TimeSpan cooldownDuration = TimeSpan.FromHours(5); // 5시간의 무료 보상 쿨타임 체크

    private void OnEnable()
    {
        if (PlayerPrefs.HasKey(RefilledBoxCount))
        {
            int num = PlayerPrefs.GetInt(RefilledBoxCount);
            m_Text.text = string.Format("Free({0}/{1})", BoxMaxCount - num, BoxMaxCount);
            hasCooldownKey = true;
            if (BoxMaxCount == num)
                allBoxReceived = true;
            else
                allBoxReceived = false;
            lastBoxReceived = DateTime.Parse(PlayerPrefs.GetString(CooldownKey)).ToUniversalTime();
        }
            else
        {
            m_Text.text = string.Format("Free({0}/{0})", BoxMaxCount);
            hasCooldownKey = false;
            allBoxReceived = false;
            //lastBoxReceived;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ClickAdButton();
    }

    GameObject clickedButton;
    GameObject rewardBox;

    public void ClickAdButton(GameObject go = null)
    {
        if (PlayerPrefs.GetInt(RefilledBoxCount) == BoxMaxCount)
            return;

        rewardBox = Instantiate(rewardBoxPrefab, m_Canvas.transform);
        rewardBox.GetComponent<SupplyBoxReward>().continueBtn.onClick.AddListener(ResetWeaponLobby); // 프리팹을 새로 생성하므로, 내부 continue 버튼의 onclick을 따로 이어줘야 함

        // 광고 시청과 연결된 함수, 회사 내부 정보로 인한 함수 모양 변경
        ShowRewardedAd("Write Log Here", OnSuccessRewardAdResult_ShowBox);
    }

    void OnSuccessRewardAdResult_ShowBox(bool result) // 상자 보상과 관련된 광고 보상 함수
    {
        if (result)
        {
            foreach (var UIObject in WeaponLobbyUI) // UI 숨기기
            {
                UIObject.SetActive(false);
            }
            gameObject.GetComponent<RectTransform>().position = new Vector3(gameObject.transform.position.x * -1, gameObject.transform.position.y, gameObject.transform.position.z); // 광고 버튼은 살아있어야 함수가 작동하므로 화면 밖으로 이동

            StartCoroutine(rewardBox.GetComponent<SupplyBoxReward>().ShowRewardBoxAnim(supplyBoxType));

            // Box 재사용 쿨타임 체크
            DateTime now = DateTime.UtcNow;
            if (!PlayerPrefs.HasKey(RefilledBoxCount))
            {
                PlayerPrefs.SetInt(RefilledBoxCount, 1); // 현재 쿨타임 기다리는 박스 1개 추가
                PlayerPrefs.SetString(CooldownKey, now.ToString("o"));
                hasCooldownKey = true;
                lastBoxReceived = now;
                m_Text.text = string.Format("FREE({0}/{1})", BoxMaxCount - 1, BoxMaxCount);
            }
            else
            {
                int usedBoxCount = PlayerPrefs.GetInt(RefilledBoxCount) + 1;
                if (usedBoxCount == BoxMaxCount)
                    allBoxReceived = true;
                PlayerPrefs.SetInt(RefilledBoxCount, usedBoxCount);
                m_Text.text = string.Format("FREE({0}/{1})", BoxMaxCount - usedBoxCount, BoxMaxCount);
            }
            PlayerPrefs.Save();
        }
        else
        {
            GameUI.Instance.toastmessage.SetInfo(TableManager.Instance.GetStringData("RewardFail"));
        }
    }

    TimeSpan remain;
    float deltaTime = 0f;
    DateTime lastBoxReceived;
    bool hasCooldownKey; // 현재 쿨타임 관련 작업이 필요함을 표시
    bool allBoxReceived; // 모든 박스(Test 기준 3개)가 쿨타임이 돌고 있는지

    private void Update()
    {
        if (!hasCooldownKey)
            return;

        // Timer 테스트 원하시면 시간초로 변경해서 체크하면 됩니다.
        //remain = lastBoxReceived.AddSeconds(cooldownDuration.Hours * 6) - DateTime.UtcNow;
        remain = lastBoxReceived.AddHours(cooldownDuration.Hours) - DateTime.UtcNow;

        if (allBoxReceived)
        {
            string formatted = string.Format("{0:D2}:{1:D2}:{2:D2}", (int)remain.Hours, (int)remain.Minutes, (int)remain.Seconds);
            m_Text.text = formatted;
        }

        if (remain < TimeSpan.Zero)
        {
            SetADButtonText();
            return;
        }
    }

    // 박스가 한개라도 채워져 있다면 쿨타임 대신 숫자로 체크되어 있습니다.
    void SetADButtonText()
    {
        int count = PlayerPrefs.GetInt(RefilledBoxCount) - 1; // 쿨타임으로 채워야 할 박스 갯수 감소 (박스가 채워졌다는 의미)
        allBoxReceived = false;
        PlayerPrefs.SetInt(RefilledBoxCount, count);
        if (count >= 0 && count < BoxMaxCount)
        {
            m_Text.text = string.Format("FREE({0}/{1})", BoxMaxCount - count, BoxMaxCount);
        }

        if (count == 0)
        {
            PlayerPrefs.DeleteKey(RefilledBoxCount);
            PlayerPrefs.DeleteKey(CooldownKey);
            hasCooldownKey = false;
        }
        else
        {
            DateTime now = DateTime.UtcNow;
            PlayerPrefs.SetString(CooldownKey, now.ToString("o")); // 기존 시간은 쿨타임이 지나서 채워졌으니 지금 시간으로 변경
            lastBoxReceived = now;
        }

        PlayerPrefs.Save();
    }

    public void ResetWeaponLobby()
    {
        foreach (var UIObject in WeaponLobbyUI) // UI 재배치
        {
            UIObject.SetActive(true);
        }
        gameObject.GetComponent<RectTransform>().position = new Vector3(gameObject.transform.position.x * -1, gameObject.transform.position.y, gameObject.transform.position.z);

        Destroy(rewardBox.GetComponent<SupplyBoxReward>().RewardBoxPrefab);
        Destroy(rewardBox.GetComponent<SupplyBoxReward>().RewardBoxBG);
        Destroy(rewardBox);
    }

    public void ResetRewardboxCooldown()
    {
        PlayerPrefs.DeleteKey(RefilledBoxCount);
        PlayerPrefs.DeleteKey(CooldownKey);
        hasCooldownKey = false;
        allBoxReceived = false;
        m_Text.text = string.Format("FREE({0}/{0})", BoxMaxCount);
        PlayerPrefs.Save();
    }
}
