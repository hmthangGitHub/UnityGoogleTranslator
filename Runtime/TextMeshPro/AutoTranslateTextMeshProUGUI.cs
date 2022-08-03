using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using GTranslatorAPI;
using Newtonsoft.Json;
using TMPro;

public class AutoTranslateTextMeshProUGUI : MonoBehaviour
{
    private TextMeshProUGUI textMeshProUGUI;
    public string original;
    public string translated;
    void Start()
    {
        textMeshProUGUI = GetComponent<TextMeshProUGUI>();
        original = textMeshProUGUI.text;
        TranslateAsync().Forget();
    }

    private async UniTask TranslateAsync()
    {
        translated = await TranslatorScheduler.RequestTranslate(textMeshProUGUI.text);
        Toggle();
    }

    public void Toggle()
    {
        if (textMeshProUGUI.text == original)
        {
            if (!string.IsNullOrEmpty(translated))
            {
                if (textMeshProUGUI)
                {
                    textMeshProUGUI.text = translated;
                }
            }    
        }
        else
        {
            textMeshProUGUI.text = original;
        }
    }

    public void ForceChange()
    {
        original = textMeshProUGUI.text;
        TranslateAsync().Forget();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            ForceChange();
        }
        
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Toggle();
        }
    }
}
