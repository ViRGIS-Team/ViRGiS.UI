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
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections.Generic;
using UniRx;
using System;
using Unity.Netcode;


namespace Virgis {

    [System.Serializable]
    public class LayerPanelEditSelectedEvent : UnityEvent<LayerUIPanel, bool> {}

    public class LayerUIPanel : MonoBehaviour {
        public Toggle editLayerToggle;
        public Toggle viewLayerToggle;
        public Text layerNameText;
        public GameObject DefaultSpheroid;
        public GameObject DefaultCuboid;
        public GameObject DefaultCylinder;
        public bool IsEditPanel;
        public bool ShowFeature;

        private IVirgisLayer m_layer;
        private LayerPanelEditSelectedEvent m_editSelectedEvent;
        private List<IDisposable> m_subs = new();
        private GameObject feature;

        void Start() {
            if (IsEditPanel)
            {
                if (m_editSelectedEvent == null)
                    m_editSelectedEvent = new LayerPanelEditSelectedEvent();
                if (editLayerToggle != null)
                    editLayerToggle.onValueChanged.AddListener(OnEditToggleValueChange);
                if (viewLayerToggle != null)
                    viewLayerToggle.onValueChanged.AddListener(OnViewToggleValueChange);
            }
            m_subs.Add(State.instance.EditSession.ChangeLayerEvent.Subscribe(OnEditLayerChanged));
        }

        private void OnDestroy()
        {
            if (IsEditPanel)
            {
                editLayerToggle?.onValueChanged.RemoveAllListeners();
                m_editSelectedEvent.RemoveAllListeners();
                Destroy(feature);
                (m_layer as VirgisLayer).m_FeatureShape.OnValueChanged -= OnFeatureShape;
            }
            m_subs.ForEach(sub => sub.Dispose());
            (m_layer as VirgisLayer).m_DefaultCol.OnValueChanged -= UpdateMaterial;
        }

        public IVirgisLayer layer {
            get => m_layer;
            set
            {
                m_layer = value;
                if (layerNameText != null)
                {
                    if (m_layer.sourceName == null || layer.sourceName == "")
                    {
                        layerNameText.text = m_layer.featureType.ToString();
                    }
                    else
                    {
                        layerNameText.text = m_layer.sourceName;
                    }
                }
                if (ShowFeature)
                {
                    if (m_layer.GetFeatureShape() != Shapes.None)
                    {
                        OnFeatureShape(Shapes.None, m_layer.GetFeatureShape());
                    }
                    else
                    {
                        (m_layer as VirgisLayer).m_FeatureShape.OnValueChanged += OnFeatureShape;
                    }
                }
            }
        }

        private void OnFeatureShape(Shapes peviousValue, Shapes featureShape)
        {
            if (featureShape != Shapes.None)
            {
                switch (featureShape)
                {
                    case Shapes.Spheroid:
                        feature = Instantiate(DefaultSpheroid, transform, false);
                        break;
                    case Shapes.Cuboid:
                        feature = Instantiate(DefaultCuboid, transform, false);
                        break;
                    case Shapes.Cylinder:
                        feature = Instantiate(DefaultCylinder, transform, false);
                        break;
                    default:
                        throw new NotImplementedException("Unknown Feature Shape");
                };
                NetworkVariable<SerializableMaterialHash> col = (m_layer as VirgisLayer).m_DefaultCol;
                col.OnValueChanged += UpdateMaterial;
                UpdateMaterial(new SerializableMaterialHash(), col.Value);
                feature.transform.localPosition = new Vector3(100f, 0f, 0f);
                feature.transform.localRotation = Quaternion.identity;
                feature.transform.localScale = new Vector3(20f, 20f, 0.1f);
            }
        }

        public void AddEditSelectedListener(UnityAction<LayerUIPanel, bool> action) {
            if (m_editSelectedEvent == null)
                m_editSelectedEvent = new LayerPanelEditSelectedEvent();
            m_editSelectedEvent.AddListener(action);
        }

        private void OnEditToggleValueChange(bool enabled) {
            m_editSelectedEvent.Invoke(this, enabled);
        }

        private void OnViewToggleValueChange(bool visible)
        {
            if (visible)
            {
                layerNameText.color = new Color32(0, 0, 245, 255);
                m_layer.SetVisible(true);
            }
            else
            {
                layerNameText.color = new Color32(100, 100, 100, 255);
                m_layer.SetVisible(false);
            }
        }

        private void OnEditLayerChanged(IVirgisLayer layer)
        {
            if (editLayerToggle == null) return;
            if (editLayerToggle.isOn && layer == m_layer)
            {
                editLayerToggle.isOn = false;
            }
        }

        public void UpdateMaterial(SerializableMaterialHash previousValue, SerializableMaterialHash newValue)
        {
            if (newValue.Equals(previousValue)) return;
            Material material = default;
            if (feature.TryGetComponent<MeshRenderer>(out MeshRenderer mr)) material = mr.material;
            material?.SetColor("_BaseColor", Color.red);
            if (newValue.properties == null) return;
        }
    }
}