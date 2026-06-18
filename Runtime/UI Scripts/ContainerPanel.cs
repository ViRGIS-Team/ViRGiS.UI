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
using System;
using System.Collections.Generic;

namespace Virgis
{
    public class ContainerPanel : MonoBehaviour
    {

        public GameObject subPanel;
        public GameObject subBox;
        public List<GameObject> subPanels = new List<GameObject>();
        public Text panelNameText;
        public Toggle expandToggle;

        void Start()
        {
            expandToggle?.onValueChanged.AddListener(Expand);
        }

        private void OnDestroy()
        {
            expandToggle?.onValueChanged.RemoveListener(Expand);
        }

        public void Expand(bool thisEvent)
        {
            subBox.SetActive(thisEvent);
            if (expandToggle != null && expandToggle.isOn != thisEvent)
            {
                expandToggle.isOn = thisEvent;
            }
            RectTransform trans = transform as RectTransform;
            if (thisEvent) {
                trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 40 + 40 * subPanels.Count);
            } else {
                trans.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 40);
            }
            trans.ForceUpdateRectTransforms();
        }

        public T AddPanel<T>()
        {
            GameObject panel = Instantiate(subPanel, subBox.transform);
            subPanels.Add(panel);
            return panel.GetComponentInChildren<T>();
            
        }

        public void SetPanelText(string text)
        {
            panelNameText.text = text;
        }
    }
}
