/* MIT License

Copyright (c) 2020 - 21 Runette Software

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice (and subsidiary notices) shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. */

using UnityEngine;
using UnityEngine.UI;
using Virgis;
using System;
using System.Collections.Generic;
using UniRx;

public class HudFacade : MonoBehaviour
{
    public Text HudLeftText;
    public Text HudRightText;
    public Text HudCentreText;

    private readonly List<IDisposable> m_Subs = new();
    
    // Start is called before the first frame update
    void Start()
    {
        State appState = State.instance;
        m_Subs.Add(appState.EditSession.StartEvent.Subscribe(OnEditSessionStart));
        m_Subs.Add(appState.EditSession.EndEvent.Subscribe(OnEditSessionEnd));
        m_Subs.Add(appState.Zoom.Event.Subscribe(OnZoomChanged));
        m_Subs.Add(appState.Orientation.Event.Subscribe(onOrientation));
    }

    private void OnDestroy() {
        m_Subs.ForEach( sub => sub.Dispose());
    }

    public void onPosition(float position) {
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, position);
    }

    public void onOrientation(Vector3 current) {
        if(State.instance.map == null) { return; }
        current.y = 0;
        double angle = Math.Floor(Vector3.SignedAngle(State.instance.map.transform.forward, current, State.instance.map.transform.up)/5)*5;
        if (angle < 0)
            angle = 360 + angle;
        HudCentreText.text = angle.ToString();
    }

    public void OnEditSessionStart(bool ignore) {
        HudRightText.text = "Editing";
        HudRightText.color = Color.red;
    }

    public void OnEditSessionEnd(bool saved) {
        HudRightText.text = "";
    }

    public void OnZoomChanged(float scale) {
        HudLeftText.text = $"1 : {Mathf.Round(scale *10) / 10}";
    }

    public void SetStatus(string text, Color color) {
        HudRightText.text = text;
        HudRightText.color = color;
    }
}
