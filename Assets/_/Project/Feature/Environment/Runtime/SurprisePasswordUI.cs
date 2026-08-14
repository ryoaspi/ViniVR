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
    public string m_correctAnswer = "Europa";

    #endregion


    #region API Unity

    private void Awake()
    {
        if (m_uiDocument == null)
        {
            Debug.LogError("[SurprisePasswordUI] UIDocument non assigné.", this);
            return;
        }

        RegisterButtons();
    }

    #endregion


    #region Utils (méthodes publiques)

    public void HandleButtonClick(ClickEvent m_event)
    {
        if (m_event.currentTarget is not Button button)
            return;

        if (button.text == m_correctAnswer)
        {
            ActivateReward();
        }

        DisablePanel();
    }

    #endregion


    #region Main Methods (méthodes private)

    private void RegisterButtons()
    {
        VisualElement root = m_uiDocument.rootVisualElement;

        var buttons = root.Query<Button>().ToList();

        foreach (Button button in buttons)
        {
            button.RegisterCallback<ClickEvent>(HandleButtonClick);
        }
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
}
