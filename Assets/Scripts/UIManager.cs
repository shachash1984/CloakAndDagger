using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using DG.Tweening;
using UnityEngine.SceneManagement;

[Serializable]
public struct Message
{
    public string header;
    [TextArea(1,6)]public string content;
}

public class UIManager : MonoBehaviour
{
    #region Public Fields
    static public UIManager S;
    #endregion

    #region Private Fields
    [SerializeField] private Text _moneyText;
    [SerializeField] private Text _informationText;
    [SerializeField] private Text _apText;
    [SerializeField] private Text _xpText;
    [SerializeField] private Text _lordsOwnedText;
    [SerializeField] private Button _moveButton;
    [SerializeField] private Button _bribeButton;
    [SerializeField] private Button _blackmailButton;
    [SerializeField] private Button _farmButton;
    [SerializeField] private Slider _suspicionSlider;
    [SerializeField] private Image _sliderFillImage;
    [SerializeField] private Button _messageConfirmButton;
    [SerializeField] private Text _messageHeaderText;
    [SerializeField] private Text _messageContentText;
    [SerializeField] private CanvasGroup _messagePanel;
    [SerializeField] private CanvasGroup _quitPanel;
    [SerializeField] private Button _quitButton;
    [SerializeField] private Button _cancelQuitButton;
    [SerializeField] private CanvasGroup _helpPanel;
    [SerializeField] private Text _nameText;
    [SerializeField] private Text _helpText;
    [SerializeField] private CanvasGroup _popPanel;
    [SerializeField] private Text _popText;
    [SerializeField] private CanvasGroup _endPanel;
    [SerializeField] private Text _endHeader;
    [SerializeField] private Text _endContent;
    [SerializeField] private Button _endOKButton;
    [SerializeField] private Button _endCancelButton;


    [Space][Header("Messages")]
    [SerializeField] private Message[] _playerMessages;
    private Dictionary<string, string> _messageDictionary;
    private float _fadeDuration = 0.25f;
    #endregion

    #region Events And Delegates
    public static event Action OnActionButtonPressed;
    #endregion

    #region Monobehaviour Callbacks
    private void Reset()
    {
        Init();
    }

    private void Awake()
    {
        S = this;
    }

    private void OnEnable()
    {
        GameManager.OnGameStateChanged += HandleGameState;
        SpyMaster.OnAPChanged += UpdateAPText;
        Tile.OnTileSelected += DisplayMoveButton;
        Farmable.OnFarmableSelected += DisplayFarmButton;
        LordCastle.OnLordCastleSelected += DisplayLordCastleButtons;
        SpyMaster.OnMoneyChanged += UpdateMoneyText;
        SpyMaster.OnInformationChanged += UpdateInformationText;
        SpyMaster.OnXPChanged += UpdateXPText;
        SpyMaster.OnLordsOwnedChanged += UpdateLordsOwnedText;
        King.OnSuspicionChanged += UpdateSuspicion;
        GameManager.OnPlayerPressedEscape += DisplayQuitPanel;
        SpyMaster.OnFarmSucceeded += PopPanel;
        SpyMaster.OnWin += DisplayWinPanel;
        SpyMaster.OnLose += DisplayLosePanel;
        Init();
    }

    private void OnDisable()
    {
        GameManager.OnGameStateChanged -= HandleGameState;
        SpyMaster.OnAPChanged -= UpdateAPText;
        Tile.OnTileSelected -= DisplayMoveButton;
        Farmable.OnFarmableSelected -= DisplayFarmButton;
        LordCastle.OnLordCastleSelected -= DisplayLordCastleButtons;
        SpyMaster.OnMoneyChanged -= UpdateMoneyText;
        SpyMaster.OnInformationChanged -= UpdateInformationText;
        SpyMaster.OnXPChanged -= UpdateXPText;
        SpyMaster.OnLordsOwnedChanged -= UpdateLordsOwnedText;
        King.OnSuspicionChanged -= UpdateSuspicion;
        GameManager.OnPlayerPressedEscape -= DisplayQuitPanel;
        SpyMaster.OnFarmSucceeded -= PopPanel;
        SpyMaster.OnWin -= DisplayWinPanel;
        SpyMaster.OnLose -= DisplayLosePanel;
    }

    private void Start()
    {
        DisplayMessage("Welcome");
    }
    #endregion

    #region Public Methods
    public void Init()
    {
        //Assign stats texts 
        Transform panel = transform.GetChild(1);
        _apText = panel.GetChild(0).GetComponent<Text>();
        _xpText = panel.GetChild(1).GetComponent<Text>();
        _lordsOwnedText = panel.GetChild(2).GetComponent<Text>();

        //Assign resources texts
        panel = transform.GetChild(2);
        _moneyText = panel.GetChild(0).GetComponent<Text>();
        _informationText = panel.GetChild(1).GetComponent<Text>();

        //Assign buttons
        panel = transform.GetChild(0);
        _moveButton = panel.GetChild(0).GetComponent<Button>();
        _bribeButton = panel.GetChild(1).GetComponent<Button>();
        _blackmailButton = panel.GetChild(2).GetComponent<Button>();
        _farmButton = panel.GetChild(3).GetComponent<Button>();

        //Assign suspicion slider
        panel = transform.GetChild(3);
        _suspicionSlider = panel.GetChild(1).GetComponent<Slider>();
        _sliderFillImage = panel.GetChild(1).GetChild(1).GetChild(0).GetComponent<Image>();
        _suspicionSlider.value = 0f;

        //Assign message panel
        panel = transform.GetChild(4);
        _messagePanel = panel.GetComponent<CanvasGroup>();
        _messageHeaderText = panel.GetChild(1).GetComponent<Text>();
        _messageContentText = panel.GetChild(2).GetComponent<Text>();
        _messageConfirmButton = panel.GetChild(3).GetComponent<Button>();
        _messageConfirmButton.onClick.RemoveAllListeners();
        _messageConfirmButton.onClick.AddListener(() =>
        {
            ConfirmMessage();
        });

        //Assign Move button
        AssignMoveButton();

        //Assign Quit Panel
        panel = transform.GetChild(5);
        _quitPanel = panel.GetComponent<CanvasGroup>();
        _quitButton = panel.GetChild(3).GetComponent<Button>();
        _quitButton.onClick.RemoveAllListeners();
        _quitButton.onClick.AddListener(() => Application.Quit());
        _cancelQuitButton = panel.GetChild(4).GetComponent<Button>();
        _cancelQuitButton.onClick.RemoveAllListeners();
        _cancelQuitButton.onClick.AddListener(() => ToggleUIElement(_quitPanel, false));
        ToggleUIElement(_quitPanel, false, true);

        //Assign help panel
        panel = transform.GetChild(6);
        _helpPanel = panel.GetComponent<CanvasGroup>();
        _nameText = panel.GetChild(1).GetComponent<Text>();
        _helpText = panel.GetChild(2).GetComponent<Text>();
        ToggleUIElement(_helpPanel, false, true);

        //Make sure pop panel is hidden
        ToggleUIElement(_popPanel, false, true);

        //Assign End Game Panel
        panel = transform.GetChild(7);
        _endPanel = panel.GetComponent<CanvasGroup>();
        _endHeader = panel.GetChild(1).GetComponent<Text>();
        _endContent = panel.GetChild(2).GetComponent<Text>();
        _endOKButton = panel.GetChild(3).GetComponent<Button>();
        _endCancelButton = panel.GetChild(4).GetComponent<Button>();
        _endOKButton.onClick.RemoveAllListeners();
        _endOKButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(1);
        });
        _endCancelButton.onClick.RemoveAllListeners();
        _endCancelButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(0);
        });
        ToggleUIElement(_endPanel, false, true);

        //Initialize Message Dictionary
        _messageDictionary = new Dictionary<string, string>();
        foreach (Message m in _playerMessages)
        {
            _messageDictionary.Add(m.header, m.content);
        }
    }

    private void ConfirmMessage()
    {
        ToggleUIElement(_messagePanel, false);
    }

    public void AssignFarmButton(Farmable f)
    {
        _farmButton.onClick.RemoveAllListeners();
        _farmButton.onClick.AddListener(() =>
        {
            SpyMaster.S.Farm(f);
            if (OnActionButtonPressed != null)
                OnActionButtonPressed();
        });
    }

    public void AssignBribeButton(LordCastle l)
    {
        _bribeButton.onClick.RemoveAllListeners();
        _bribeButton.onClick.AddListener(() =>
        {
            SpyMaster.S.BribeLord(l);
            if (OnActionButtonPressed != null)
                OnActionButtonPressed();
        });
        //Debug.Log("AssignBribeButton");
    }

    public void AssignBlackmailButton(LordCastle l)
    {
        _blackmailButton.onClick.RemoveAllListeners();
        _blackmailButton.onClick.AddListener(() =>
        {
            SpyMaster.S.BlackmailLord(l);
            if (OnActionButtonPressed != null)
                OnActionButtonPressed();
        });
        //Debug.Log("AssignBlackmailButton");
    }

    public void AssignMoveButton()
    {
        _moveButton.onClick.RemoveAllListeners();
        _moveButton.onClick.AddListener(() =>
        {
            SpyMaster.S.MoveSpy(Map.S.selectedTile);
            if (OnActionButtonPressed != null)
                OnActionButtonPressed();
        });
    }

    public void UpdateAPText(int amount)
    {
        _apText.text = string.Format("Action points: {0}", amount);
    }

    public void UpdateMoneyText(int amount)
    {
        _moneyText.text = string.Format("Money: {0}", amount);
    }

    public void UpdateInformationText(int amount)
    {
        _informationText.text = string.Format("Information: {0}", amount);
    }

    public void UpdateXPText(int amount)
    {
        _xpText.text = string.Format("EXP: {0}", amount);
    }

    public void UpdateLordsOwnedText(int amount)
    {
        _lordsOwnedText.text = string.Format("Lords Owned: {0}", amount);
    }

    public void UpdateSuspicion(float amount)
    {
        
        Color c = _sliderFillImage.color;
        if(amount >= 0)
        {
            c.g -= amount;
            c.b -= amount;
        }
        else
        {
            c.g += amount;
            c.b += amount;
        }
        _suspicionSlider.DOValue(amount, 0.25f);
        _sliderFillImage.DOColor(c, 0.25f);
    }

    public void DisplayFarmButton(Farmable f)
    {
        //Debug.Log("Setting Player Action Buttons");
        Spy s = SpyMaster.S.GetSelectedSpy();
        if (s != null && Map.S.IsAdjacent(Map.S.GetTile(f.mapPosition), Map.S.GetTile(s.mapPosition)))
        {
            AssignFarmButton(f);
            ToggleAllButtons(false);
            ToggleUIElement(_farmButton, true);
        }
        else
        {
            ToggleAllButtons(false);
        }
    }

    public void DisplayLordCastleButtons(LordCastle l)
    {
        //Debug.Log("Setting Player Action Buttons");
        Spy s = SpyMaster.S.GetSelectedSpy();
        if (s && Map.S.IsAdjacent(Map.S.GetTile(l.mapPosition), Map.S.GetTile(s.mapPosition)))
        {
            //AssignFarmButton(f);
            AssignBribeButton(l);
            AssignBlackmailButton(l);
            ToggleAllButtons(false);
            ToggleUIElement(_bribeButton, true);
            ToggleUIElement(_blackmailButton, true);
        }
        else
        {
            ToggleAllButtons(false);
        }
    }

    public void DisplayMoveButton(Tile t, Spy selectedSpy)
    {
        TileStatus tS = t.tileStatus;
        switch (tS)
        {
            case TileStatus.Empty:
                //Debug.Log("Empty");
                ToggleAllButtons(false);
                ToggleUIElement(_moveButton, true);
                break;
            default:
                break;
        }
    }

    public void DisplayMessage(string headerText)
    {
        _messageHeaderText.text = headerText;
        _messageContentText.text = _messageDictionary[headerText];
        ToggleUIElement(_messagePanel, true);
    }

    public void DisplayHelpMessage(string objectName)
    {
        _nameText.text = objectName;
        _helpText.text = _messageDictionary[objectName];
        Vector3 wantedPos = Input.mousePosition;
        wantedPos.x += 150f;
        _helpPanel.transform.position = wantedPos;
        DOTween.Kill(_helpPanel, true);
        ToggleUIElement(_helpPanel, true);
    }

    public void HideHelpMessage()
    {
        ToggleUIElement(_helpPanel, false);
    }

    public void DisplayWinPanel()
    {
        DisplayEndPanel("You Win");
    }

    public void DisplayLosePanel()
    {
        DisplayEndPanel("You Lose");
    }

    #endregion

    #region Private Methods
    private void DisplayEndPanel(string header)
    {
        _endHeader.text = header;
        _endContent.text = _messageDictionary[header];
    }

    private void DisplayQuitPanel()
    {
        ToggleUIElement(_quitPanel, true);
    }

    private void HandleGameState(GameState previous, GameState current)
    {
        switch (current)
        {
            case GameState.Standby:
                ToggleAllButtons(false);
                break;
            case GameState.SpyMasterTurn:
                ToggleAllButtons(false);
                break;
            case GameState.KingTurn:
                ToggleAllButtons(false);
                break;
            case GameState.GameOver:
                break;
            default:
                break;
        }
    }

    private void ToggleAllButtons(bool on)
    {
        ToggleUIElement(_moveButton, on, true);
        ToggleUIElement(_bribeButton, on, true);
        ToggleUIElement(_blackmailButton, on, true);
        ToggleUIElement(_farmButton, on, true);
    }

    private void ToggleUIElement(Component s, bool on, bool immediate = false)
    {
        CanvasGroup cv;
        if (s is CanvasGroup)
            cv = s as CanvasGroup;
        else
            cv = s.GetComponent<CanvasGroup>();

        //DOTween.Kill(cv);
        if (!cv)
            cv = s.gameObject.AddComponent<CanvasGroup>();
        if (immediate)
        {
            if (on)
            {
                cv.alpha = 1f;
                cv.gameObject.SetActive(true);
            }
            else
            {
                cv.alpha = 0f;
                cv.gameObject.SetActive(false);
            }
        }
        else
        {
            if (on)
            {
                cv.DOFade(1f, _fadeDuration).OnStart(() =>
                {
                    cv.gameObject.SetActive(true);
                }).OnComplete(() =>
                {
                    cv.interactable = true;
                });
            }
            else
            {
                cv.DOFade(0f, _fadeDuration).OnComplete(() =>
                {
                    cv.gameObject.SetActive(false);
                    cv.interactable = false;
                });
            }
        }
    }

    private void PopPanel(Vector3 pos, string popText)
    {
        StartCoroutine(PopPanelCoroutine(pos, popText));
    }

    private IEnumerator PopPanelCoroutine(Vector3 pos, string popText)
    {
        _popText.text = popText;
        pos.y += 2;
        _popPanel.transform.position = pos;
        DOTween.Kill(_popPanel);
        DOTween.Kill(_popPanel.transform);
        ToggleUIElement(_popPanel, true);
        _popPanel.transform.DOMoveY(pos.y + 1f, 1f);
        yield return new WaitUntil(() => !DOTween.IsTweening(_popPanel.transform));
        ToggleUIElement(_popPanel, false);
    }
    #endregion
}
