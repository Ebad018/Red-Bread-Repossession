using UnityEngine;

public class ControllerInputDebugger : MonoBehaviour
{
    void Update()
    {
        // Check Joystick1 buttons 0–19 (typical range)
        for (int i = 0; i <= 19; i++)
        {
            if (Input.GetKeyDown((KeyCode)System.Enum.Parse(typeof(KeyCode), "Joystick1Button" + i)))
            {
                Debug.Log("Joystick1Button" + i + " pressed");
            }
        }

        // Check trigger axis (Axis 3 typically includes both LT and RT)
        float triggers = Input.GetAxis("Triggers"); // Rename your input axis to "Triggers"

        if (triggers < -0.1f)
        {
            Debug.Log("LT pressed (Triggers axis value: " + triggers + ")");
        }
        else if (triggers > 0.1f)
        {
            Debug.Log("RT pressed (Triggers axis value: " + triggers + ")");
        }
    }
}
