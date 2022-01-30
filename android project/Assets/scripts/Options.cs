using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class Options : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if(LocalizationSettings.SelectedLocale == null)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[0];
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenYoutube()
    {
        Application.OpenURL("https://www.youtube.com/watch?v=o-YBDTqX_ZU&ab_channel=MusRest");
    }
    public void LoadLocale(string languageIdentifier)
    {
        LocalizationSettings settings = LocalizationSettings.Instance;
        LocaleIdentifier localeCode = new LocaleIdentifier(languageIdentifier);//can be "en" "de" "ja" etc.
        for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
        {
            Locale aLocale = LocalizationSettings.AvailableLocales.Locales[i];
            LocaleIdentifier anIdentifier = aLocale.Identifier;
            if (anIdentifier == localeCode)
            {
                LocalizationSettings.SelectedLocale = aLocale;
            }
        }
    }
}
