using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScoreList : MonoBehaviour
{
    [SerializeField] Text infoTxt;
    [SerializeField] Image infoImg;
    [SerializeField] Image WinnerImg;
    [SerializeField] Text addScoreTxt;
    Animator anim;
    public int score { get; set; }

    private int addScore;
    private float currAddScore=0;
    private float addScoreEffectTime;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }
    public void SetInfoUI(Color _infoImgColor,  string _infoTxt)
    {
        infoImg.color = _infoImgColor;
        //infoTxt.color = _infoTxtColor;
        infoTxt.text = _infoTxt;
    }

    public void SetScore(int _score)
    {
        score = _score;
    }
    public void AddScoreEffect(int _score)
    {
        addScore += _score;
        addScoreEffectTime = 2f;
        anim.SetTrigger("Active");
    }
    public void SetWinnerImg(bool _active)
    {
        WinnerImg.gameObject.SetActive(_active);
    }

    private void Update()
    {
        if (addScoreEffectTime <= 0)
        {
            addScoreTxt.gameObject.SetActive(false);
            currAddScore = 0;
            addScore = 0;
        }
        else
        {
            addScoreEffectTime -= Time.deltaTime;
            addScoreTxt.gameObject.SetActive(true);
            if (currAddScore < addScore)
            {
                currAddScore += Time.deltaTime*20f;
            }
            addScoreTxt.text = "+ "+((int)currAddScore).ToString();
        }
    }
}
