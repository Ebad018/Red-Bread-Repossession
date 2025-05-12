using UnityEngine;

public class ControllerDuelPlayer : MonoBehaviour
{
    public DuelManager duelManager;
    private float promptStartTime;

    private float lastTriggerValue = 0f;

    void Start()
    {
        promptStartTime = Time.time;
    }

    void Update()
    {
        if (duelManager == null || string.IsNullOrEmpty(duelManager.Player2CurrentKey)) return;

        string expected = duelManager.Player2CurrentKey;

        if (CheckControllerInput(expected))
        {
            float reactionTime = Time.time - promptStartTime;
            Debug.Log($"Controller: Correct key pressed: {expected} | Time: {reactionTime}");
            duelManager.HandleKeyPress(1, expected.ToUpper());

            promptStartTime = Time.time;
        }

        lastTriggerValue = Input.GetAxis("Triggers"); // always update this
    }

    private bool CheckControllerInput(string expected)
    {
        switch (expected)
        {
            case "A": return Input.GetKeyDown(KeyCode.Joystick1Button0);
            case "B": return Input.GetKeyDown(KeyCode.Joystick1Button1);
            case "X": return Input.GetKeyDown(KeyCode.Joystick1Button2);
            case "Y": return Input.GetKeyDown(KeyCode.Joystick1Button3);
            case "LB": return Input.GetKeyDown(KeyCode.Joystick1Button4);
            case "RB": return Input.GetKeyDown(KeyCode.Joystick1Button5);

            case "LT":
                return lastTriggerValue >= -0.1f && Input.GetAxis("Triggers") < -0.5f;

            case "RT":
                return lastTriggerValue <= 0.1f && Input.GetAxis("Triggers") > 0.5f;

            default: return false;
        }
    }
}
