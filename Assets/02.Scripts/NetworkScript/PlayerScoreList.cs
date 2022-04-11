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
    public int actorNum;
    public int score;

    public int addScore;
    public float currAddScore=0;
    public float addScoreEffectTime;
    private void Start()
    {
        anim = GetComponent<Animator>();
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
    public void SetScore(int _score)
    {
        score = _score;
    }
    public void AddScoreEffect(int _score)
    {
        print("AddScoreEffect " + _score);
        addScore += _score;
        addScoreEffectTime = 2f;
        anim.SetTrigger("Active");
    }
    public void SetWinnerImg(bool _active)
    {
        WinnerImg.gameObject.SetActive(_active);
    }
    public void SetInfoUI(Color _infoImgColor, Color _infoTxtColor, string _infoTxt)
    {
        infoImg.color = _infoImgColor;

        infoTxt.color = _infoTxtColor;
        infoTxt.text = _infoTxt;
    }
}
