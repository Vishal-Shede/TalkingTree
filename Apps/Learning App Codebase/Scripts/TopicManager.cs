using System.Collections.Generic;
using UnityEngine;

public class TopicManager : MonoBehaviour
{
    public enum Topic { Introduction, StudyWork, Hobby, Experience, Ambition }
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
                currentTopic = Topic.Experience;
                break;
            case Topic.Experience:
                currentTopic = Topic.Ambition; // Final topic
                break;
            case Topic.Ambition:
                Debug.Log("Conversation Complete!"); // Optionally add an ending flow
                return;
        }

        Debug.Log("New Topic: " + currentTopic);

        // Notify Aditya's conversation logic about the new topic
        OnTopicChanged?.Invoke(currentTopic);
    }

    public Topic GetCurrentTopic()
    {
        return currentTopic;
    }
}
