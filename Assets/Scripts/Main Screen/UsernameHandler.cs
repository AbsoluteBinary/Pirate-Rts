using _Project.Scripts;
using _Project.Scripts.Persistence;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Required for UI components

namespace Main_Screen
{
    public class UsernameHandler : MonoBehaviour
    {
        public TMP_InputField usernameInputField; // Assign this in the Inspector
        private string username; // Stores the username

        void Start()
        {
            // Add the onEndEdit listener
            usernameInputField.onEndEdit.AddListener(SetUsername);
        }

        // This method is called when the user finishes editing (presses Enter or clicks away)
        private void SetUsername(string inputText)
        {
            username = inputText;
            Debug.Log("Username set to: " + username);
        }

        // Optional: Method to access the username later
        public string GetUsername()
        {
            return username;
        }
    }
}