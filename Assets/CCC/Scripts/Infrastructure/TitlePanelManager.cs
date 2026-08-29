using UnityEngine;

public class TitlePanelManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private PanelScaleAnimation stagePanel;
    [SerializeField] private PanelScaleAnimation optionPanel;
    [SerializeField] private PanelScaleAnimation creditPanel;


    private void Awake()
    {
        // 各Panelを初期化
        stagePanel.Initialize();
        optionPanel.Initialize();
        creditPanel.Initialize();

        // ゲーム開始時はすべて非表示
        stagePanel.gameObject.SetActive(false);
        optionPanel.gameObject.SetActive(false);
        creditPanel.gameObject.SetActive(false);
    }

    public void ShowStage()
    {
        stagePanel.Show();
    }

    public void HideStage()
    {
        stagePanel.Hide();
    }

    public void ShowOption()
    {
        optionPanel.Show();
    }

    public void HideOption()
    {
        optionPanel.Hide();
    }

    public void ShowCredit()
    {
        creditPanel.Show();
    }

    public void HideCredit()
    {
        creditPanel.Hide();
    }
}