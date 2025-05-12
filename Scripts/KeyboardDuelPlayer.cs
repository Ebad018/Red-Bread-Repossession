using UnityEngine;

public class KeyboardDuelPlayer : MonoBehaviour
{
    public DuelManager duelManager;
    private float promptStartTime;

    void Start()
    {
        promptStartTime = Time.time;
       // Debug.Log("KeyboardDuelPlayer active and listening.");
    }

    void Update()
    {
        if (duelManager == null)
        {
            //Debug.LogWarning("DuelManager is not assigned.");
            return;
        }

        string expectedKey = duelManager.Player1CurrentKey;
        if (string.IsNullOrEmpty(expectedKey)) return;

        if (Input.anyKeyDown)
        {
            //Debug.Log("A key was pressed!");

            for (KeyCode kc = KeyCode.A; kc <= KeyCode.Z; kc++)
            {
                if (Input.GetKeyDown(kc))
                {
                    //Debug.Log("Key Pressed: " + kc);
                }
            }

            if (Input.GetKeyDown(expectedKey.ToLower()))
            {
                float timeTaken = Time.time - promptStartTime;
                //Debug.Log("Correct key pressed: " + expectedKey + " | Time: " + timeTaken);

                duelManager.HandleKeyPress(0, expectedKey);
            }
            else
            {
               // Debug.Log("Incorrect key. Expected: " + expectedKey);
            }
        }
    }
}
