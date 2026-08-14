using UnityEngine;
using UnityEngine.UIElements;

public class SurprisePasswordUI : MonoBehaviour
{
    #region Publics

    [Header("UI Toolkit")]
    public UIDocument m_uiDocument;

    [Header("Reward")]
    public GameObject m_rewardObject;

    [Header("Password")]
    public string m_correctAnswer = "Opera";

    #endregion


    #region API Unity

    private void Start()
    {
        if (m_uiDocument == null)
        {
            Debug.LogError(
                "[SurprisePasswordUI] UIDocument non assigné.",
                this);

            return;
        }

        RegisterButtons();
    }

    #endregion


    #region Public Methods

    public void ShowPanel()
    {
        if (_hasAnswered)
        {
            return;
        }

        m_uiDocument.gameObject.SetActive(true);
    }

    #endregion


    #region Main Methods

    private void RegisterButtons()
    {
        VisualElement root = m_uiDocument.rootVisualElement;

        root.Query<Button>().ForEach(button =>
        {
            button.clicked += () => HandleButtonClick(button);
        });
    }

    private void HandleButtonClick(Button button)
    {
        if (_hasAnswered)
        {
            return;
        }

        _hasAnswered = true;

        if (button.text == m_correctAnswer)
        {
            ActivateReward();
        }

        DisablePanel();
    }

    private void ActivateReward()
    {
        if (m_rewardObject == null)
        {
            Debug.LogWarning(
                "[SurprisePasswordUI] Aucun GameObject de récompense assigné.",
                this);

            return;
        }

        m_rewardObject.SetActive(true);
    }

    private void DisablePanel()
    {
        m_uiDocument.gameObject.SetActive(false);
    }

    #endregion


    #region Private

    private bool _hasAnswered = false;

    #endregion
}