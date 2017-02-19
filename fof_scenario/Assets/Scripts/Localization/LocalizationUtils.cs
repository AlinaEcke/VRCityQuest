using System;
using System.Collections.Generic;
using UnityEngine;
using pumpkin.display;
using pumpkin.events;
using pumpkin.text;


public class LocalizationUtils: MonoBehaviour
{
    #region Protected members
    public GameObject interfaces;
    protected Dictionary<TextField, LocalizedText> localizedTextList;
    protected Dictionary<MovieClip, LocalizedButton> localizedButtonList;
    #endregion

    #region Ctor
    void Awake()
    {
        localizedTextList = new Dictionary<TextField, LocalizedText>(); // List<TranslationText>();
        localizedButtonList = new Dictionary<MovieClip, LocalizedButton>();
    }
    #endregion

    #region Public methods
    public void AddTranslationText(TextField _textfield, string txt )
    {
        if (_textfield == null)
            return;

        //Debug.Log("AddTranslationText " + _textfield.name);

        if (localizedTextList.ContainsKey(_textfield))
        {
            LocalizedText _translationText;
            localizedTextList.TryGetValue(_textfield, out _translationText);
            _translationText = new LocalizedText(_textfield, txt);
            //Debug.Log("C?E? GIA? " + _textfield.name);
        }
        else
        {
            localizedTextList.Add(_textfield, new LocalizedText(_textfield, txt));
        }
    }

    public LocalizedButton AddTranslationButton(MovieClip _mc, string _text, EventDispatcher.EventCallback BtnClickCallback, LocalizedButton.Alignment _alignment, bool _resizable = true, bool _icon = false, string _iconLabel = "")
    {
        LocalizedButton localizedButton;
        if (localizedButtonList.ContainsKey(_mc))
        {
            localizedButtonList.TryGetValue(_mc, out localizedButton);
            //_translationText = new TranslationText(_textfield, txt); TODO
        }
        else
        {
            localizedButton = new LocalizedButton(_mc, _text, BtnClickCallback, _alignment, _resizable, _icon, _iconLabel);
            localizedButtonList.Add(_mc, localizedButton);
        }
        return localizedButton;
    }

    public LocalizedButton GetLocalizedButton(MovieClip mc)
    {
        return localizedButtonList[mc];
    }

    public void Destroy()
    {
        //Translations.Instance.RemoveElement(this);
        //base.Destroy();
    }


    public void OnLanguageChanged(string lang)
    {
        foreach (KeyValuePair<TextField, LocalizedText> _translationText in localizedTextList)
        {
            if (_translationText.Value != null)
            {
                _translationText.Value.OnLanguageChanged(lang);
            }
        }

        foreach (KeyValuePair<MovieClip, LocalizedButton> _translationButton in localizedButtonList)
        {
            if (_translationButton.Value != null)
            {
                _translationButton.Value.OnLanguageChanged(lang);
            }
        }
    }
    #endregion
}