using System;
using System.Collections.Generic;
using UnityEngine;
using pumpkin.display;
using pumpkin.events;
using pumpkin.text;


public class LocalizedText
{
    #region Protected members
    protected TextField textfield;
    protected string text;
    #endregion

    #region Public properties
    public string Text
    {
        get
        {
            return text;
        }
        set
        {
            text = value;
            textfield.text = Localizations.Instance.replaceText(text);
        }
    }

    public string BaseText
    {
        get
        {
            return textfield.text;
        }
    }
    #endregion

    #region Ctor
    public LocalizedText(TextField _textfield, string _text)
    {
        //Debug.Log("new TranslationText _textfield " + _textfield.name + " _text " + _text);
        textfield = _textfield;
        Text = _text;
        textfield.text = Localizations.Instance.replaceText(text);
        //Translations.Instance.AddElement(this);
    }
    #endregion

    #region Public methods
    public void Destroy()
    {
        //Translations.Instance.RemoveElement(this);
        //base.Destroy();
    }

    public void OnLanguageChanged(string lang)
    {
        //Debug.Log("OnLanguageChanged " + textfield.name );
        textfield.text = Localizations.Instance.replaceText(text);
    }
    #endregion
}