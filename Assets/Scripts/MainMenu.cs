using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour {

    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private GameObject _creditsPanel;
    [SerializeField] private Text _loadingPctText;
    [SerializeField] private Slider _loadingIndicator;
    private Vector3 _mainPanelOriginalPosition;
    private Vector3 _settingsPanelOriginalPosition;
    private Vector3 _creditsPanelOriginalPosition;

    private float _fadeDuration = 0.25f;


    void Awake ()
    {
        Init();
	}
	
    public void Init()
    {
        _mainPanel = transform.GetChild(2).GetChild(0).gameObject;
        
        _loadingIndicator = transform.GetChild(3).GetComponent<Slider>();
        _loadingPctText = _loadingIndicator.transform.GetChild(2).GetComponent<Text>();
        _settingsPanel = transform.GetChild(2).GetChild(2).gameObject;
        _creditsPanel = transform.GetChild(2).GetChild(1).gameObject;
        ToggleUIElement(_loadingIndicator, false, true);
        _mainPanelOriginalPosition = _mainPanel.transform.position;
        _creditsPanelOriginalPosition = _creditsPanel.transform.position;
        _settingsPanelOriginalPosition = _settingsPanel.transform.position;
    }

    public void MoveToCredits()
    {
        _mainPanel.transform.DOMove(_mainPanel.transform.position + new Vector3(_mainPanel.transform.position.x * 2, 0,0), 0.7f, false);
        _creditsPanel.transform.DOMove(_creditsPanel.transform.position + new Vector3(Mathf.Abs(_creditsPanel.transform.position.x) * 2, 0, 0), 0.7f);
    }

    public void BackToMenu()
    {
        _mainPanel.transform.DOMove(_mainPanelOriginalPosition, 0.7f, false);
        _settingsPanel.transform.DOMove(_settingsPanelOriginalPosition, 0.7f);
        _creditsPanel.transform.DOMove(_creditsPanelOriginalPosition, 0.7f);
    }

    public void MoveToSettings()
    {
        _mainPanel.transform.DOMove(_mainPanel.transform.position + new Vector3(0,_mainPanel.transform.position.y +_mainPanel.transform.position.y, 0), 0.7f, false);
        _settingsPanel.transform.DOMove(_settingsPanel.transform.position + new Vector3(0, Mathf.Abs(_settingsPanel.transform.position.y) * 2, 0), 0.7f, false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void StartGame()
    {
        _mainPanel.transform.DOMove(_mainPanel.transform.position + new Vector3(0, -_mainPanel.transform.position.y * 2, 0), 0.7f, false);
        StartCoroutine(LoadGameScene());
    }

    public void ChangeSliderText(Slider slider)
    {
        gameObject.GetComponentInChildren<Text>().text = slider.value.ToString();
    }

    IEnumerator LoadGameScene()
    {
        ToggleUIElement(_loadingIndicator, true);
        yield return new WaitForSeconds(0.7f);
        AsyncOperation loadGameScene =  SceneManager.LoadSceneAsync(1);
        while (!loadGameScene.isDone)
        {
            _loadingPctText.text = string.Format("{0}%", loadGameScene.progress * 100);
            _loadingIndicator.DOValue(loadGameScene.progress, 0.1f);
            yield return null;
        }
    }

    private void ToggleUIElement(Component s, bool on, bool immediate = false)
    {
        CanvasGroup cv = s.GetComponent<CanvasGroup>();
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
                cv.DOFade(0f, _fadeDuration).OnStart(() =>
                {
                    cv.gameObject.SetActive(false);
                }).OnComplete(() =>
                {
                    cv.interactable = false;
                });
            }
        }
    }
}
