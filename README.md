# TalkingTree 2.0 – Project Overview

## 🎯 What is the Project?

My M.Tech project, called **TalkingTree**, is a **VR-based learning intervention** for practicing English group discussions.  

Through **interviews with learners**, I understood that many from rural and semi-urban backgrounds, whose native language is not English, learn the language academically. Even if they achieve a decent level of proficiency, they often hesitate to speak in group settings due to **fear of making mistakes and being judged**. This hesitation affects their **confidence and participation** in professional or academic discussions.  

To address this, I created a **virtual reality environment** where a learner can practice discussions on different topics with **AI-driven avatars**. These avatars have **distinct personalities** and not only talk but also animate like real people, so the interaction feels natural rather than robotic. The conversations are **unscripted and powered by Generative AI**, making each discussion dynamic and realistic.  

The idea is to provide learners with a **judgment-free space** where they can practice English discussions safely and build confidence.

---

## ✨ Features

### 🔹 Learning Scene
- One-on-one interaction with trainer avatar **Aditya**.  
- Aditya usually starts with a **probing question**, so the learner must respond.  
- Learners can **take their time before answering** to think and prepare a response.  
- After responding, the **real-time feedback panel** shows the spoken sentence alongside an improved version.  
- Feedback is **supportive, not evaluative**, focusing on improvement without judgment.

---

### 🔹 Practice Scene
- Learner moves into a **group discussion with two avatars**: Aditya and Anjali.  
- Both avatars have **distinct personalities**, creating realistic dynamics.  

---

### 🔹 Generative AI Integration
- Discussions are **unscripted, dynamic conversations** powered by GPT → no two sessions are identical.  
- **Speech-to-Text (Whisper API):** Converts learner’s speech to text.  
- **Text-to-Speech (TTS):** Generates natural voices for avatars.  
- The entire interaction is **fully voice-based** — learners actually speak and listen, like in real group discussions.

---

### 🔹 Avatar Personality
- Each avatar speaks in a **personalized, conversational style** (not robotic like Siri/Alexa).  
- They have their own **name, background, and role**:  
  - Aditya: a witty, casual graphic designer from Delhi.  
  - Anjali: an empathetic, reflective teacher from Mumbai.  
- Avatars always respond in **first person** and never say “I am an AI agent.”  
- This makes interactions feel like **real conversations with humans**.

---

### 🔹 Structured Discussion
- Discussions follow a **structured flow of topics**:  
  **Introduction → Work/Study → Hobbies → Ambitions**

---

### 🔹 Interaction Buttons
- **Start Button** → begins the session; avatars introduce themselves.  
- **Speak/Stop Button (toggle)** → learner records input and stops when ready.  
- **Next Button** → moves to the next topic at the learner’s pace (stays on the current one until pressed).  

---

### 🔹 Animation
- Avatars don’t remain static while speaking.  
- A **basic body/lip animation** is triggered during speech, adding naturalness and realism.  

---

### 🔹 VR Environment
- Built in **Unity 3D with C# scripts**.  
- Features a **home-like discussion setup** (sofa, casual living-room style) created with Unity assets.  
- Designed to make the environment **comfortable and engaging** for learners.  

---

### 🔹 Conversation Log
- The system **saves the entire conversation history** between the learner and avatars.  
- Logs can later be used for **reflection** and **further analysis** (e.g., grammar review, researcher evaluation).  

---

### 🔹 Multi-Person Discussion with Turn-Taking
- Supports **multi-person conversations** with two AI avatars.  
- **Turn-taking** is managed automatically in a round-robin style:  
  *User → Aditya → User → Anjali → User → Aditya → …*  
- Ensures balanced interaction and simulates real group discussions.  

---
