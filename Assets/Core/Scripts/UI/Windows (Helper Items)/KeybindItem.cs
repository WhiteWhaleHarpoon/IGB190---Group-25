using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using static System.Runtime.CompilerServices.RuntimeHelpers;

public class KeybindItem : MonoBehaviour
{
    public TextMeshProUGUI keybindTitle;
    public TextMeshProUGUI keybindKey;
    public int keybindIndex = 0;

    private bool isRecording;

    private void OnEnable()
    {
        UpdateKeybindDisplay();
    }

    private void OnDisable()
    {
        StopRecordingKeybind();
    }

    private void Update()
    {
        if (!isRecording) return;
        if (!Input.anyKeyDown) return;
        if (Input.GetKeyDown(KeyCode.Mouse1)) return;
        if (Input.GetKeyDown(KeyCode.Mouse0) && isRecording)
        {
            StopRecordingKeybind();
            return;
        }

        foreach (KeyCode code in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(code))
            {
                GameManager.settings.keybindings[keybindIndex] = code;
                AbilitySlotUI[] slots = GameObject.FindObjectsByType<AbilitySlotUI>(FindObjectsSortMode.None);
                foreach (AbilitySlotUI slot in slots)
                {
                    slot.RedrawHotkey();
                }
                StopRecordingKeybind();
                break;
            }
        }
    }

    public void StartRecordingKeybind ()
    {
        keybindKey.text = "<color=yellow>Press Any Key</color>";
        isRecording = true;
    }

    public void StopRecordingKeybind ()
    {
        isRecording = false;
        UpdateKeybindDisplay();
    }

    public void UpdateKeybindDisplay ()
    {
        keybindKey.text = GameManager.settings.keybindings[keybindIndex].ToString();
    }
}
