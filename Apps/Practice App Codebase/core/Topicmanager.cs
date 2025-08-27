using System.Collections.Generic;
using UnityEngine;

public class TopicManager : MonoBehaviour
{
    [SerializeField] public OpenAIManager openAIManager;  // ?? Assign this in the Unity Inspector

    public enum Topic { Introduction, StudyWork, Hobby, Ambition, End }
    private Topic currentTopic = Topic.Introduction; // Start with Introduction

    public delegate void OnTopicChange(Topic newTopic);
    public static event OnTopicChange OnTopicChanged;

    // ? Method to Handle Topic Progression
    public void GoToNextTopic()
    {
        switch (currentTopic)
        {
            case Topic.Introduction:
            currentTopic = Topic.StudyWork;
            break;
        case Topic.StudyWork:
            currentTopic = Topic.Hobby;
            break;
        case Topic.Hobby:
            currentTopic = Topic.Ambition;
            break;
        case Topic.Ambition:
            currentTopic = Topic.End; // ? New final topic
            break;
        case Topic.End:
            Debug.Log("?? Conversation Fully Complete!");
            return;
        }

        Debug.Log("New Topic: " + currentTopic);

         // ? Tell OpenAIManager to handle topic transition
        if (openAIManager != null)
        {
            openAIManager.AdvanceToNextTopic();
        }

        // Notify Aditya's conversation logic about the new topic
        OnTopicChanged?.Invoke(currentTopic);
    }

    public Topic GetCurrentTopic()
    {
        return currentTopic;
    }
}
