using System.Collections;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class LanguageSwitcher : MonoBehaviour
{
    public void SetRussian()
    {
        StartCoroutine(SetLocale("ru", "ru-RU"));
    }

    public void SetKazakh()
    {
        StartCoroutine(SetLocale("kk", "kk-KZ"));
    }

    public void SetEnglish()
    {
        StartCoroutine(SetLocale("en", "en-US"));
    }

    private IEnumerator SetLocale(params string[] localeCodes)
    {
        yield return LocalizationSettings.InitializationOperation;

        Locale localeToSet = null;

        foreach (var locale in LocalizationSettings.AvailableLocales.Locales)
        {
            foreach (var code in localeCodes)
            {
                string localeCode = locale.Identifier.Code;

                if (localeCode == code || localeCode.StartsWith(code))
                {
                    localeToSet = locale;
                    break;
                }
            }

            if (localeToSet != null)
                break;
        }

        if (localeToSet == null)
        {
            Debug.LogWarning("Locale not found.");
            yield break;
        }

        LocalizationSettings.SelectedLocale = localeToSet;
        PlayerPrefs.SetString("SelectedLanguage", localeToSet.Identifier.Code);
        PlayerPrefs.Save();
    }
}