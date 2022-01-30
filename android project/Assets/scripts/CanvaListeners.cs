using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization;

public class CanvaListeners : MonoBehaviour
{
    // Start is called before the first frame update
    public string sceneName;
    public Text Level;
    void Start()
    {
            LocalizationSettings settings = LocalizationSettings.Instance;
            LocaleIdentifier localeCode = new LocaleIdentifier("en");//can be "en" "de" "ja" etc.
            for (int i = 0; i < LocalizationSettings.AvailableLocales.Locales.Count; i++)
            {
                Locale aLocale = LocalizationSettings.AvailableLocales.Locales[i];
                LocaleIdentifier anIdentifier = aLocale.Identifier;
                if (anIdentifier == localeCode)
                {
                    LocalizationSettings.SelectedLocale = aLocale;
                }
            }
        if (ChangeButton != null)
            ChangeButton.GetComponent<Button>().onClick.AddListener(() => { ChangeScene(this.sceneName); });
        if (ExitButton != null)
            ExitButton.GetComponent<Button>().onClick.AddListener(() => { Exit(); });
        if (RestartButton != null)
            RestartButton.GetComponent<Button>().onClick.AddListener(() => { RestartScene(); });
    }

    // Update is called once per frame
    void Update()
    {

    }
    [SerializeField]
    GameObject ChangeButton;

    [SerializeField]
    GameObject ExitButton;

    [SerializeField]
    GameObject RestartButton;
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
    public void Exit()
    {
        Application.Quit();
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(Level.text);

    }


}
