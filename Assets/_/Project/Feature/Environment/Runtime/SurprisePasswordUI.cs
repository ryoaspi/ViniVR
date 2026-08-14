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
            Debug.LogError(
                "[SurprisePasswordUI] UIDocument non assigné.",
                this);

            return;
        }

        // La récompense doit être cachée au départ.
        if (m_rewardObject != null)
        {
            m_rewardObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning(
                "[SurprisePasswordUI] Aucun GameObject de récompense assigné.",
                this);
        }

        RegisterButtons();
    }

    #endregion


    #region UI Events

    private void HandleButtonClick(ClickEvent m_event)
    {
        Debug.Log("[SurprisePasswordUI] CLICK détecté !");

        if (m_event.currentTarget is not Button button)
        {
            Debug.LogWarning(
                "[SurprisePasswordUI] L'élément cliqué n'est pas un Button.",
                this);

            return;
        }

        Debug.Log(
            $"[SurprisePasswordUI] Réponse sélectionnée : \"{button.text}\"");

        if (button.text == m_correctAnswer)
        {
            Debug.Log(
                "[SurprisePasswordUI] BONNE RÉPONSE ! Activation de la récompense.");

            ActivateReward();
        }
        else
        {
            Debug.Log(
                $"[SurprisePasswordUI] MAUVAISE RÉPONSE. " +
                $"Réponse attendue : \"{m_correctAnswer}\"");

            // On s'assure que la récompense reste désactivée.
            DeactivateReward();
        }

        // Dans tous les cas, on ferme l'interface.
        DisablePanel();
    }

    #endregion


    #region Main Methods

    private void RegisterButtons()
    {
        VisualElement root = m_uiDocument.rootVisualElement;

        if (root == null)
        {
            Debug.LogError(
                "[SurprisePasswordUI] Impossible de récupérer le rootVisualElement.",
                this);

            return;
        }

        var buttons = root.Query<Button>().ToList();

        Debug.Log(
            $"[SurprisePasswordUI] Nombre de boutons trouvés : {buttons.Count}");

        foreach (Button button in buttons)
        {
            Debug.Log(
                $"[SurprisePasswordUI] Enregistrement du bouton : " +
                $"Name = {button.name}, Text = \"{button.text}\"");

            button.RegisterCallback<ClickEvent>(HandleButtonClick);
        }

        if (buttons.Count == 0)
        {
            Debug.LogWarning(
                "[SurprisePasswordUI] Aucun Button trouvé dans le UIDocument.",
                this);
        }
        
        foreach (Button button in buttons)
        {
            button.clicked += () =>
            {
                Debug.Log("🔥 BOUTON CLIQUÉ : " + button.text);
            };
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

        Debug.Log(
            $"[SurprisePasswordUI] Reward activée : {m_rewardObject.name}");
    }


    private void DeactivateReward()
    {
        if (m_rewardObject == null)
            return;

        m_rewardObject.SetActive(false);

        Debug.Log(
            $"[SurprisePasswordUI] Reward désactivée : {m_rewardObject.name}");
    }


    private void DisablePanel()
    {
        if (m_uiDocument == null)
            return;

        m_uiDocument.gameObject.SetActive(false);

        Debug.Log(
            "[SurprisePasswordUI] Interface UI Toolkit désactivée.");
    }

    #endregion
}