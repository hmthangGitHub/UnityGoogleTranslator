using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using GTranslatorAPI;
using UnityEngine;

public class TranslatorScheduler
{
    private static TranslatorScheduler instance;
    private static TranslatorScheduler Instance => instance ??= new TranslatorScheduler();
    private Translator translator;
    private Translator Translator => translator ??= new Translator(new Settings()
    {
        SplitStringBeforeTranslate = false
    });
    
    private readonly List<string> requestList = new List<string>();
    private readonly Dictionary<int, Translation> translateResult = new Dictionary<int, Translation>();
    private UniTaskCompletionSource ucs = default;
    private string paragraphReplacement = "---";

    public static async UniTask<string> RequestTranslate(string originalText)
    {
        var translatedText = Instance.GetTranslatedText(originalText);
        if (!string.IsNullOrEmpty(translatedText))
        {
            return translatedText;
        }
        else
        {
            Instance.AddToRequestQueue(originalText);
            await Instance.Translate();
            translatedText = Instance.GetTranslatedText(originalText);
            return translatedText;
        }
    }

    private async UniTask Translate()
    {
        if (ucs == default)
        {
            ucs = new UniTaskCompletionSource();
            await UniTask.WhenAny(UniTask.WaitUntil(() => requestList.Count >= 500), UniTask.Delay(5000));
            var result = await Translator.TranslateCombineAsync(Languages.auto, Languages.en, requestList.ToArray());
            try
            {
                foreach (var translation in result)
                {
                    translation.TranslatedText = translation.TranslatedText.Replace(paragraphReplacement, "\n")
                        .Replace("--", ".");
                    translateResult.Add(translation.OriginalText.GetHashCode(), translation);
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                ucs.TrySetResult();
                ucs = default;
                requestList.Clear();    
            }
        }
        else
        {
            await ucs.Task;
        }
    }

    private void AddToRequestQueue(string originalText)
    {
        var replace = GetReplacementText(originalText);
        var hashCode = GetHashCodeFromOriginalText(originalText);
        if(!translateResult.ContainsKey(hashCode) && !requestList.Contains(replace))
        {
            requestList.Add(replace);
        }
    }
    
    private string GetTranslatedText(string originalText)
    {
        translateResult.TryGetValue(GetHashCodeFromOriginalText(originalText), out var result);
        return result?.TranslatedText ?? string.Empty;
    }

    private int GetHashCodeFromOriginalText(string originalText)
    {
        return GetReplacementText(originalText)
            .GetHashCode();
    }

    private string GetReplacementText(string originalText)
    {
        return originalText
            .Replace(" \r\n\r\n", paragraphReplacement)
            .Replace("\r\n\r\n", paragraphReplacement)
            .Replace(" \n", paragraphReplacement)
            .Replace(".", "--")
            .Replace("\n", paragraphReplacement);
    }
}
