using System;
using System.Collections.Generic;
using UnityEngine;
using pumpkin.display;
using pumpkin.events;
using pumpkin.text;


public class LocalizedButton
{
    #region Protected members
    public MovieClip mc;
    protected TextField textfield;
    protected string text;
    protected MovieClip centerMc;
    protected MovieClip leftMc;
    protected MovieClip rightMc;
    protected MovieClip iconMc;
    protected MovieClip bgMc;
    protected Alignment alignment;
    protected ButtonState buttonState;
    protected EventDispatcher.EventCallback btnClickCallback;
    protected bool resizable;
    protected bool icon;
    protected string iconLabel;

    protected float centerX;
    protected float leftX;
    protected float rightX;

    protected CQ_Interface interfaces;

    public enum Alignment
    {
        center = 0,
        left,
        right
    }

    public enum ButtonState
    {
        up = 0,
        down,
        over,
        ghost
    }
    #endregion

    #region Public properties
    
    #endregion

    #region Ctor
    public LocalizedButton(MovieClip _mc, string _text, EventDispatcher.EventCallback _btnClickCallback, Alignment _alignment, bool _resizable, bool _icon, string _iconLabel)
    {
        mc = _mc;
        text = _text;
        //buttonType = _buttonType;
        btnClickCallback = _btnClickCallback;
        alignment = _alignment;
        resizable = _resizable;
        icon = _icon;
        iconLabel = _iconLabel;

        if (resizable)
        {
            rightMc = mc.getChildByName<MovieClip>("mcButtonRight");
            leftMc = mc.getChildByName<MovieClip>("mcButtonLeft");
            centerMc = mc.getChildByName<MovieClip>("mcButtonFill");

            centerX = centerMc.x;
            leftX = leftMc.x;
            rightX = rightMc.x;
        }
        else 
        {
            bgMc = mc.getChildByName<MovieClip>("mcButtonBg");
        }

        iconMc = mc.getChildByName<MovieClip>("mcIcon");

        if (!icon)
        {
            if (iconMc != null)
                iconMc.visible = false;
            //arrowMc = null;
        }

        interfaces = GameObject.FindGameObjectWithTag("Interface").GetComponent<CQ_Interface>();
        
        SetButtonEvents();
    }

    public void SetButtonEvents()
    {
        SetButtonState(ButtonState.up);
        if (!mc.hasEventListener(MouseEvent.CLICK))
            mc.addEventListener(MouseEvent.CLICK, btnClickCallback);
        if (!mc.hasEventListener(MouseEvent.MOUSE_DOWN))
            mc.addEventListener(MouseEvent.MOUSE_DOWN, OnBtnsDown);
        if (!mc.hasEventListener(MouseEvent.MOUSE_ENTER))
            mc.addEventListener(MouseEvent.MOUSE_ENTER, OnBtnsEnter);
        if (!mc.hasEventListener(MouseEvent.MOUSE_LEAVE))
            mc.addEventListener(MouseEvent.MOUSE_LEAVE, OnBtnsLeave);
        if (!mc.hasEventListener(MouseEvent.MOUSE_UP))
            mc.addEventListener(MouseEvent.MOUSE_UP, OnBtnsLeave);
    }

    public void DisableLocalizedButton()
    {
        mc.removeAllEventListeners(MouseEvent.CLICK);
        mc.removeAllEventListeners(MouseEvent.MOUSE_DOWN);
        mc.removeAllEventListeners(MouseEvent.MOUSE_ENTER);
        mc.removeAllEventListeners(MouseEvent.MOUSE_LEAVE);
        mc.removeAllEventListeners(MouseEvent.MOUSE_UP);
        SetButtonState(ButtonState.ghost);
    }

    public void OnBtnsEnter(CEvent evt)
    {
        SetButtonState(ButtonState.over);
    }
    public void OnBtnsLeave(CEvent evt)
    {
        SetButtonState(ButtonState.up);
    }
    public void OnBtnsDown(CEvent evt)
    {
        SetButtonState(ButtonState.down);
    }

    public void SetButtonState(ButtonState _buttonState)
    {
        buttonState = _buttonState;
        string frameLabel = "up";
        switch (buttonState)
        {
            case ButtonState.up:
                frameLabel = "up";
                break;
            case ButtonState.down:
                frameLabel = "dn";
                break;
            case ButtonState.over:
                frameLabel = "ov";
                break;
            case ButtonState.ghost:
                frameLabel = "gh";
                break;
        }

        mc.gotoAndStop(frameLabel);

        TextField txtField = mc.getChildByName<TextField>("tfLabel");
        LocalizedText localizedText = new LocalizedText(txtField, text);

        if (resizable)
        {
            txtField.multiline = false;
            centerMc.gotoAndStop(frameLabel);
            leftMc.gotoAndStop(frameLabel);
            rightMc.gotoAndStop(frameLabel);

            string translatedStr = localizedText.BaseText;

            float letterSpacing = txtField.textFormat.letterSpacing;
            float strLength = 0.0f;
            for (int k = 0; k < translatedStr.Length; k++)
                strLength = strLength + txtField.getGlyph(translatedStr[k]).charWidth + letterSpacing;

            int textFieldLength = (int)(strLength + 20); // 16 * translationText.BaseText.Length;

            float textLength;
            if (icon)
                textLength = textFieldLength + iconMc.width;
            else
                textLength = textFieldLength;

            centerMc.scaleX = 1.0f;
            centerMc.scaleX = textLength / 20; // larghezza base di centerMc

            int centerWidth = Mathf.CeilToInt(centerMc.width);
            if (centerWidth % 2 > 0)
                centerWidth--;

            if (alignment == Alignment.center)
            {
                centerMc.x = centerX;
                rightMc.x = centerMc.x + centerWidth * 0.5f;
                leftMc.x = centerMc.x - centerWidth * 0.5f - leftMc.width;
            }
            else if (alignment == Alignment.right)
            {
                leftMc.x = leftX;
                centerMc.x = leftMc.x + leftMc.width + centerMc.width * 0.5f;
                rightMc.x = centerMc.x + centerWidth * 0.5f;
            }
            else if (alignment == Alignment.left)
            {
                rightMc.x = rightX;
                centerMc.x = rightMc.x - centerWidth * 0.5f;
                leftMc.x = centerMc.x - centerWidth * 0.5f - leftMc.width;
            }

            txtField.x = centerMc.x - textLength * 0.5f;
            txtField.width = textFieldLength;


            if (iconMc != null)
                iconMc.x = Mathf.FloorToInt(txtField.x + txtField.width + 10.0f );
        }
        else
            bgMc.gotoAndStop(frameLabel);

        if (iconMc != null)
        {
            iconMc.visible = icon;
            iconMc.gotoAndStop(iconLabel);
        }
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
        SetButtonState(buttonState);
    }
    #endregion
}