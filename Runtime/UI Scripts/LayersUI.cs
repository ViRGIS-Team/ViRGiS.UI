﻿/* MIT License

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

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

namespace Virgis
{

    /// <summary>
    /// LayersUI is the mediator for all components within the Layers UI GO (i.e. Layers Menu).
    /// </summary>
    /// 
    /// 
    public class LayersUI : MonoBehaviour
    {
        public GameObject layersScrollView;
        public GameObject layerPanelPrefab;
        public GameObject menus;

        private State m_appState;
        private Dictionary<Guid, LayerUIPanel> m_layersMap;

        private List<IDisposable> m_subs = new();

        // Start is called before the first frame update
        void Start()
        {
            m_appState = State.instance;
            m_subs.Add(m_appState.EditSession.StartEvent.Subscribe(OnStartEditSession));
            m_subs.Add(m_appState.EditSession.EndEvent.Subscribe(OnEndEditSession));
            m_subs.Add(m_appState.LayerUpdate.AddEvents.Subscribe(onLayerUpdate));
            m_subs.Add(m_appState.LayerUpdate.DelEvents.Subscribe(onLayerUpdate));
            m_layersMap = new Dictionary<Guid, LayerUIPanel>();
            CreateLayerPanels();
        }

        private void OnDestroy() {
            m_subs.ForEach(sub => sub.Dispose());
        }

        public void OnShowMenuButtonClicked()
        {
            gameObject.SetActive(false);
            menus.SetActive(true);
        }

        public void CreateLayerPanels()
        {
            // Delete any existing panel
            foreach (var panel in m_layersMap) {
                Destroy(panel.Value.gameObject);
            }
            m_layersMap.Clear();

            // appState.layers are actually Layer script (Component)
            State.instance.Layers.ForEach(layer =>
            {
                // create a view panel for this particular layer
                GameObject newLayerPanel = Instantiate(layerPanelPrefab, transform);
                // obtain the panel script
                LayerUIPanel panelScript = newLayerPanel.GetComponentInChildren<LayerUIPanel>();
                LayerUIContainer containerScript = newLayerPanel.GetComponentInChildren<LayerUIContainer>();
                containerScript.m_layersMap = m_layersMap;
                // set the layer in the panel
                panelScript.layer = layer;
                containerScript.layer = layer;

                containerScript.viewLayerToggle.isOn = layer.IsVisible();

                m_layersMap.Add(Guid.NewGuid(), panelScript);
                newLayerPanel.transform.SetParent(layersScrollView.transform, false);
            });
            LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
        }

        private void onLayerUpdate(IVirgisLayer layer) {
            CreateLayerPanels();
        }

        private void OnStartEditSession(bool ignore)
        {
            foreach (LayerUIPanel panel in m_layersMap.Values)
            {
                if (panel.layer.isWriteable && panel.editLayerToggle != null)
                    panel.editLayerToggle.interactable = true;
            }
        }

        private void OnEndEditSession(bool saved)
        {
            foreach (LayerUIPanel panel in m_layersMap.Values)
            {
                if (panel.editLayerToggle != null)
                    panel.editLayerToggle.interactable = false;
            }
        }
    }
}