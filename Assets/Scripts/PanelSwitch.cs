using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class PanelSwitch : MonoBehaviour
{
    public GameObject MainPanel;
    public GameObject RightPanel;
    public GameObject PositionPanel;
    public GameObject VelocityPanel;
    public GameObject ImpedancePanel;
    public GameObject ZeroforcePanel;


    enum PanelState
    {
        MAIN = 0,
        POSITION = 1,
        VELOCITY = 2,
        IMPEDANCE = 3,
        ZEROFORCE = 4
    }

    private PanelState current_panel = PanelState.MAIN;
    private PanelState request_panel = PanelState.MAIN;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ShowMainPanel(bool show)
    {
        if (show)
        {

            MainPanel.GetComponent<CanvasGroup>().DOFade(1, 0.5f);
            MainPanel.GetComponent<CanvasGroup>().interactable = true;
            MainPanel.GetComponent<CanvasGroup>().blocksRaycasts = true;
        }
        else
        {
            MainPanel.GetComponent<CanvasGroup>().DOFade(0, 0.5f);
            MainPanel.GetComponent<CanvasGroup>().interactable = false;
            MainPanel.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
    }
    
    private void ShowRightPanel(bool show)
    {
        if (show)
        {
            RightPanel.GetComponent<RectTransform>().DOAnchorPos(new Vector2(0, -85), 0.5f);
        }
        else
        {
            RightPanel.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-160, -85), 0.5f);
        }
    }

    private void ShowPositionPanel(bool show)
    {
        if (show)
        {
            PositionPanel.GetComponent<RectTransform>().DOAnchorPos(new Vector2(180 ,-85), 0.5f);
        }
        else
        {
            PositionPanel.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-1029 ,-85), 0.5f);
        }
    }
    
    private void ShowVelocityPanel(bool show)
    {
        if (show)
        {
            VelocityPanel.GetComponent<RectTransform>().DOAnchorPos(new Vector2(180 ,-85), 0.5f);
        }
        else
        {
            VelocityPanel.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-1921 ,-85), 0.5f);
        }
    }
    
    private void ShowImpedancePanel(bool show)
    {
        if (show)
        {
            ImpedancePanel.GetComponent<RectTransform>().DOAnchorPos(new Vector2(180 ,-85), 0.5f);
        }
        else
        {
            ImpedancePanel.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-2822 ,-85), 0.5f);
        }
    }
    
    private void ShowZeroforcePanel(bool show)
    {
        if (show)
        {
            ZeroforcePanel.GetComponent<RectTransform>().DOAnchorPos(new Vector2(180 ,-85), 0.5f);
        }
        else
        {
            ZeroforcePanel.GetComponent<RectTransform>().DOAnchorPos(new Vector2(-3720 ,-85), 0.5f);
        }
    }
    

    public void SwitchToMainPanel()
    {
        request_panel = PanelState.MAIN;
        if (request_panel == current_panel)
        {
            Debug.Log("已打开主面板");
            return;
        }
        else
        {
            ShowMainPanel(true);
            ShowRightPanel(false);
            ShowPositionPanel(false);
            ShowVelocityPanel(false);
            ShowImpedancePanel(false);
            ShowZeroforcePanel(false);

            current_panel = PanelState.MAIN;
        }
    }

    public void SwitchToPositionPanel()
    {
        request_panel = PanelState.POSITION;
        if (request_panel == current_panel)
        {
            Debug.Log("已打开位置面板");
            return;
        }
        else
        {
            ShowMainPanel(false);
            ShowRightPanel(true);
            ShowPositionPanel(true);
            ShowVelocityPanel(false);
            ShowImpedancePanel(false);
            ShowZeroforcePanel(false);


            current_panel = PanelState.POSITION;
        }
    }

    public void SwitchToVelocityPanel()
    {
        request_panel = PanelState.VELOCITY;
        if (request_panel == current_panel)
        {
            Debug.Log("已打开速度面板");
            return;
        }
        else
        {
            ShowMainPanel(false);
            ShowRightPanel(true);
            ShowPositionPanel(false);
            ShowVelocityPanel(true);
            ShowImpedancePanel(false);
            ShowZeroforcePanel(false);

            current_panel = PanelState.VELOCITY;
        }
    }

    public void SwitchToImpedancePanel()
    {
        request_panel = PanelState.IMPEDANCE;
        if (request_panel == current_panel)
        {
            Debug.Log("已打开阻抗面板");
            return;
        }
        else
        {
            ShowMainPanel(false);
            ShowRightPanel(true);
            ShowPositionPanel(false);
            ShowVelocityPanel(false);
            ShowImpedancePanel(true);
            ShowZeroforcePanel(false);

            current_panel = PanelState.IMPEDANCE;
        }
    }

    public void SwitchToZeroforcePanel()
    {
        request_panel = PanelState.ZEROFORCE;
        if (request_panel == current_panel)
        {
            Debug.Log("已打开零力面板");
            return;
        }
        else
        {
            ShowMainPanel(false);
            ShowRightPanel(true);
            ShowPositionPanel(false);
            ShowVelocityPanel(false);
            ShowImpedancePanel(false);
            ShowZeroforcePanel(true);


            current_panel = PanelState.ZEROFORCE;
        }
    }
    
    
}
