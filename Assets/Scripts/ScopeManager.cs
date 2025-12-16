using UnityEngine;
using UnityEngine.UI;

public class ScopeManager : MonoBehaviour
{
    public static ScopeManager Instance;

    [SerializeField]
    private Text boyCountText;
    [SerializeField]
    private Text girlCountText;

    private int boyCnt;
    private int girlCnt;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        boyCnt = 0;
        girlCnt = 0;
    }

    public void IncreaseScope()
    {
        boyCnt++;
        girlCnt++;
        SetCountText();
    }

    public void AddScoreNam(int score)
    {
        boyCnt += score;
        SetCountText();
    }

    public void AddScoreNu(int score)
    {
        girlCnt += score;
        SetCountText();
    }
    /// Vẽ điểm
    void SetCountText()
    {
        boyCountText.text = "Scope = " + boyCnt.ToString();
        girlCountText.text = "Scope = " + girlCnt.ToString();
    }
}
