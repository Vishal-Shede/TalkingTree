using UnityEngine;
using TMPro;

public class TrainingPanelManager : MonoBehaviour
{
    [SerializeField] private TMP_Text feedbackText;

    public void ShowFeedback(string feedback)
    {
        feedbackText.text = feedback;
        feedbackText.gameObject.SetActive(true);
    }

    public void ClearFeedback()
    {
        feedbackText.text = "";
        feedbackText.gameObject.SetActive(false);
    }
}
