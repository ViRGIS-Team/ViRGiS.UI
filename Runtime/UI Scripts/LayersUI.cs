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

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using R3;

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

        private State _mAppState;
        private Dictionary<ulong, LayerUIPanel> _mLayersMap = new();
        private readonly Dictionary<ulong, LayerUIContainer> _mContainersMap= new();
        private readonly List<IDisposable> _mSubs = new();

        // Start is called before the first frame update
        protected virtual void Start()
        {
            _mAppState = State.instance;
            _mSubs.Add(_mAppState.LayerUpdate.AddEvents.Subscribe(OnLayerUpdate));
            _mSubs.Add(_mAppState.LayerUpdate.DelEvents.Subscribe(OnLayerDowndate));
            _mLayersMap = new Dictionary<ulong, LayerUIPanel>();

            foreach (VirgisLayer layer in State.instance.Layers)
            {
                CreateLayerPanel(layer);
            }
        }

        protected virtual void OnDestroy() {
            _mSubs.ForEach(sub => sub.Dispose());
        }

        public void OnShowMenuButtonClicked()
        {
            gameObject.SetActive(false);
            menus.SetActive(true);
        }

        public void CreateLayerPanel(VirgisLayer layer)
        {
            // create a view panel for this particular layer
            GameObject newLayerPanel = Instantiate(layerPanelPrefab, transform);
            // obtain the panel script
            LayerUIPanel panelScript = newLayerPanel.GetComponentInChildren<LayerUIPanel>();
            LayerUIContainer containerScript = newLayerPanel.GetComponentInChildren<LayerUIContainer>();
            _mContainersMap.Add(layer.GetId(), containerScript);
            containerScript.MLayersMap = _mLayersMap;
            // set the layer in the panel
            panelScript.layer = layer;
            containerScript.Layer = layer;
            containerScript.viewLayerToggle.isOn = layer.IsVisible();
            newLayerPanel.transform.SetParent(layersScrollView.transform, false);
            LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
        }

        private void OnLayerUpdate(VirgisLayer layer) {
            CreateLayerPanel(layer);
        }

        private void OnLayerDowndate(VirgisLayer layer)
        {
            _mContainersMap.Remove(layer.GetId() , out LayerUIContainer container);
            if (container != null) Destroy(container.gameObject);
            LayoutRebuilder.MarkLayoutForRebuild(transform as RectTransform);
        }
        
    }
}