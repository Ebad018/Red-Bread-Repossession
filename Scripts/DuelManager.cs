using System;
using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public enum DuelPhase { Idle, Draw, Cock, Fire, Complete }

public class DuelManager : MonoBehaviour
{
    private bool duelActive = false;
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;
    [SerializeField] private GameObject player1Object;
    [SerializeField] private GameObject player2Object;


    public TextMeshProUGUI player1PromptText;
    public TextMeshProUGUI player2PromptText;
    public TextMeshProUGUI duelResultText;
    public TextMeshProUGUI player1StatsText;
    public TextMeshProUGUI player2StatsText;


    public Button readyButton;
    public Button rematchButton;
    public TextMeshProUGUI countdownText;
    public Animator player1Animator;
    public Animator player2Animator;
    public AudioClip cockingSound;
    public AudioClip firesound;
    public AudioClip magspinsound;
    public AudioSource cockAudioSource;
    private bool player1Finished = false;
    private bool player2Finished = false;
    private float[] player1ReactionTimes = new float[3]; // Draw, Cock, Fire
    private float[] player2ReactionTimes = new float[3];

    private float player1LastPromptTime;
    private float player2LastPromptTime;


    private float duelStartTime;

    private float player1FinishTime = 0f;
    private float player2FinishTime = 0f;

    private bool duelEnded = false;


    private string[] possibleKeys =      { "A", "S", "D", "F", "J", "K", "L" };
    private string[] controllerButtons = { "A", "B", "X", "Y", "LB", "RB", "LT", "RT" };


    private string _player1CurrentKey;
    private string _player2CurrentKey;

    public string Player1CurrentKey => _player1CurrentKey;
    public string Player2CurrentKey => _player2CurrentKey;


    private DuelPhase player1Phase = DuelPhase.Idle;
    private DuelPhase player2Phase = DuelPhase.Idle;

    public void StartDuel()
    {
        readyButton.gameObject.SetActive(false);
        duelActive = true;
        duelStartTime = Time.time;
        duelEnded = false;

        SetNextPhase(0);
        SetNextPhase(1);
    }

    public void OnReadyPressed()
    {
        readyButton.gameObject.SetActive(false);
        StartCoroutine(CountdownAndStartDuel());
    }

    public void SetNextPhase(int playerId)
    {
        string key = possibleKeys[UnityEngine.Random.Range(0, possibleKeys.Length)];
        string color = "white";

        if (playerId == 0)
        {
            _player1CurrentKey = key;
            player1Phase++;
            player1LastPromptTime = Time.time;

            // Set color based on current phase
            switch (player1Phase)
            {
                case DuelPhase.Draw:
                    color = "green";
                    break;
                case DuelPhase.Cock:
                    color = "yellow";
                    break;
                case DuelPhase.Fire:
                    color = "red";
                    StartCoroutine(PulseText(player1PromptText));
                    break;
            }

            if (player1PromptText != null)
                player1PromptText.text = $"<color={color}>PRESS: {key}</color>";
        }
        else
        {
            _player2CurrentKey = controllerButtons[UnityEngine.Random.Range(0, controllerButtons.Length)].ToUpper();

            player2Phase++;
            player2LastPromptTime = Time.time;

            switch (player2Phase)
            {
                case DuelPhase.Draw:
                    color = "green";
                    break;
                case DuelPhase.Cock:
                    color = "yellow";
                    break;
                case DuelPhase.Fire:
                    color = "red";
                    StartCoroutine(PulseText(player2PromptText));
                    break;
            }

            if (player2PromptText != null)
                player2PromptText.text = $"<color={color}>PRESS: <b>{_player2CurrentKey}</b></color>";


        }
    }


    public void HandleKeyPress(int playerId, string keyPressed)
    {
        if (!duelActive) return;

        float reactionTime = 0f;

        if (playerId == 0 && keyPressed.Equals(_player1CurrentKey, StringComparison.OrdinalIgnoreCase))
        {
            reactionTime = Time.time - player1LastPromptTime;
            player1ReactionTimes[(int)player1Phase - 1] = reactionTime;

            AdvancePhase(0);
        }
        else if (playerId == 1)
        {
            Debug.Log($"P2: Expected: {_player2CurrentKey} | Pressed: {keyPressed}");

            if (keyPressed.Equals(_player2CurrentKey, StringComparison.OrdinalIgnoreCase))
            {
                 reactionTime = Time.time - player2LastPromptTime;
                player2ReactionTimes[(int)player2Phase - 1] = reactionTime;

                AdvancePhase(1);
            }
            else
            {
                Debug.LogWarning("P2: Key did not match expected prompt.");
            }
        }


    }


    private void AdvancePhase(int playerId)
    {
        if (playerId == 0)
        {
            switch (player1Phase)
            {
                case DuelPhase.Draw:
                    player1Animator.SetTrigger("Draw");
                    Debug.Log("Player 1 has drawn weapon.");
                    AudioSource.PlayClipAtPoint(magspinsound, transform.position);
                    break;
                case DuelPhase.Cock:
                    Debug.Log("Player 1 is cocking weapon.");
                    cockAudioSource.Play();
                    break;
                case DuelPhase.Fire:
                    player1Animator.SetTrigger("Fire");
                    Debug.Log("Player 1 is firing weapon.");
                    AudioSource.PlayClipAtPoint(firesound, transform.position);
                    EndDuel(0); // Player 1 fired!
                    return;
            }

            SetNextPhase(0);
        }
        else
        {
            switch (player2Phase)
            {
                case DuelPhase.Draw:
                    player2Animator.SetTrigger("Draw");
                    Debug.Log("Player 2 has drawn weapon.");
                    AudioSource.PlayClipAtPoint(magspinsound, transform.position);
                    break;
                case DuelPhase.Cock:
                    Debug.Log("Player 2 is cocking weapon.");
                    cockAudioSource.Play();
                    break;
                case DuelPhase.Fire:
                    player2Animator.SetTrigger("Fire");
                    Debug.Log("Player 2 is firing weapon.");
                    AudioSource.PlayClipAtPoint(firesound, transform.position);
                    EndDuel(1); // Player 2 fired!
                    return;
            }

            SetNextPhase(1);
        }

        CheckDuelResult();
    }


    private void ShowStats()
    {
        if (player1StatsText == null || player2StatsText == null) return;

        string[] phaseNames = { "Draw", "Cock", "Fire" };

        string player1Result = "<b>PLAYER 1</b>\n";
        string player2Result = "<b>PLAYER 2</b>\n";

        float total1 = 0f;
        float total2 = 0f;

        for (int i = 0; i < 3; i++)
        {
            float p1 = player1ReactionTimes[i];
            float p2 = player2ReactionTimes[i];

            total1 += p1;
            total2 += p2;

            player1Result += $"{phaseNames[i]}: {p1:F2}s\n";
            player2Result += $"{phaseNames[i]}: {p2:F2}s\n";
        }

        player1Result += "\n<b>Total: " + total1.ToString("F2") + "s</b>";
        player2Result += "\n<b>Total: " + total2.ToString("F2") + "s</b>";

        player1StatsText.text = player1Result;
        player2StatsText.text = player2Result;

        player1StatsText.gameObject.SetActive(true);
        player2StatsText.gameObject.SetActive(true);
    }
    

    public void Rematch()
    {
        // Reset everything
        player1Phase = DuelPhase.Idle;
        player2Phase = DuelPhase.Idle;

        player1Finished = false;
        player2Finished = false;
        duelEnded = false;
        duelActive = false;

        _player1CurrentKey = "";
        _player2CurrentKey = "";
        // Reset transform
        if (player1Object != null && player1SpawnPoint != null)
        {
            player1Object.transform.position = player1SpawnPoint.position;
            player1Object.transform.rotation = player1SpawnPoint.rotation;

            player1Animator.Rebind();   // Resets Animator to default pose
            player1Animator.Update(0f); // Applies it immediately
        }

        if (player2Object != null && player2SpawnPoint != null)
        {
            player2Object.transform.position = player2SpawnPoint.position;
            player2Object.transform.rotation = player2SpawnPoint.rotation;

            player2Animator.Rebind();
            player2Animator.Update(0f);
        }


        for (int i = 0; i < 3; i++)
        {
            player1ReactionTimes[i] = 0f;
            player2ReactionTimes[i] = 0f;
        }

        // Reset UI
        if (player1PromptText != null)
        {
            player1PromptText.text = "";
            player1PromptText.rectTransform.localScale = Vector3.one;
            player1PromptText.color = new Color(player1PromptText.color.r, player1PromptText.color.g, player1PromptText.color.b, 1);
        }

        if (player2PromptText != null)
        {
            player2PromptText.text = "";
            player2PromptText.rectTransform.localScale = Vector3.one;
            player2PromptText.color = new Color(player2PromptText.color.r, player2PromptText.color.g, player2PromptText.color.b, 1);
        }

        duelResultText.text = "";
        duelResultText.gameObject.SetActive(false);

        player1StatsText.text = "";
        player2StatsText.text = "";
        player1StatsText.gameObject.SetActive(false);
        player2StatsText.gameObject.SetActive(false);

        rematchButton.gameObject.SetActive(false);
        countdownText.text = "";
        countdownText.gameObject.SetActive(false);

        readyButton.gameObject.SetActive(true);
    }


    private void EndDuel(int winnerId)
    {
        if (duelEnded) return;

        duelEnded = true;
        float timeNow = Time.time;

        if (!player1Finished && !player2Finished)
        {
            // This is the first shot
            if (winnerId == 0)
            {
                player1Finished = true;
                player1FinishTime = timeNow;
            }
            else
            {
                player2Finished = true;
                player2FinishTime = timeNow;
            }

            // Now check if the other player *also* fired at nearly the same time
            float otherTime = (winnerId == 0) ? player2FinishTime : player1FinishTime;
            bool otherFinished = (winnerId == 0) ? player2Finished : player1Finished;

            if (otherFinished && Mathf.Abs(timeNow - otherTime) <= 0.15f)
            {
                Debug.Log("DRAW! Bullets collided mid-air.");
                ShowDuelResult("DRAW! Bullets Collided!");
            }
            else
            {
                Debug.Log($"PLAYER {winnerId + 1} WINS THE DUEL!");
                ShowDuelResult($"PLAYER {winnerId + 1} WINS THE DUEL!");
            }
        }
        ShowStats();
        rematchButton.gameObject.SetActive(true);


        // Clean up prompt text
        StartCoroutine(FadeOutText(player1PromptText));
        StartCoroutine(FadeOutText(player2PromptText));

        // TODO: Trigger bullet visuals, camera slowmo, etc.
    }


    private IEnumerator PulseText(TextMeshProUGUI text)
    {
        float pulseDuration = 0.5f;
        float maxScale = 1.5f;
        float minScale = 1f;
        float time = 0;

        while (true)
        {
            time += Time.deltaTime;
            float scale = Mathf.Lerp(minScale, maxScale, Mathf.PingPong(time * 2f, 1));
            text.rectTransform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }
    }
    private IEnumerator CountdownAndStartDuel()
    {
        countdownText.gameObject.SetActive(true);

        string[] countdownSteps = { "3", "2", "1", "DRAW!" };

        foreach (string step in countdownSteps)
        {
            countdownText.text = step;
            yield return new WaitForSeconds(1f);
        }

        countdownText.gameObject.SetActive(false);

        StartDuel();
    }


    private IEnumerator TriggerAfterFire(Animator anim)
    {
        yield return new WaitForSeconds(0.6f); // Delay long enough for Fire animation to play
        anim.SetTrigger("FireFinished");
    }
    private IEnumerator FadeOutText(TextMeshProUGUI text, float delay = 0.5f)
    {
        yield return new WaitForSeconds(delay);

        Color originalColor = text.color;
        float duration = 1f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, time / duration);
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        text.text = ""; // Clear after fade
    }
    private IEnumerator FadeInText(TextMeshProUGUI text)
    {
        float duration = 1f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, time / duration);
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            yield return null;
        }
    }
    


    private void ShowDuelResult(string message)
    {
        if (duelResultText != null)
        {
            duelResultText.text = message;
            duelResultText.color = Color.black;
            duelResultText.alpha = 0;
            duelResultText.gameObject.SetActive(true);
            StartCoroutine(FadeInText(duelResultText));
        }
    }

    private void CheckDuelResult()
    {
        if (player1Phase == DuelPhase.Complete && player2Phase == DuelPhase.Complete)
        {
            // Compare who finished first, or mark Draw
            Debug.Log("Duel complete!");

            StartCoroutine(FadeOutText(player1PromptText));
            StartCoroutine(FadeOutText(player2PromptText));
        }
    }
}
