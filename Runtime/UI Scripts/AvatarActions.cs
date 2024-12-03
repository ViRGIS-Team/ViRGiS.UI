//https://stackoverflow.com/questions/58328209/how-to-make-a-free-fly-camera-script-in-unity-with-acceleration-and-decceleratio
// copyright Runette Software Ltd, 2020. All rights reserved
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using UniRx;

namespace Virgis
{

    /// <summary>
    /// Main Script for controlling the UI behaviour and the movement of the Camera
    /// </summary>
    public class AvatarActions : MonoBehaviour
    {
        [Header ("Avatar Objects")]
        public Transform MovementVector; // reference to the active tracking space
        public Camera MainCamera; // the main camera for ray tracing
        public float Acceleration; // controls how fast you speed up

        protected bool m_editSelected = false; // edit state 
        protected float m_selectedDistance; // distance to the selected marker``
        protected Transform m_currentPointerHit; // current marker selected by pointer
        protected Transform m_currentSelected; // current marker in selected state

        protected State m_appState;
        private Rigidbody m_thisRigidbody;
        protected bool m_axisEdit = false; // Whether we are in AxisEdit mode
        protected Quaternion m_panTarget = Quaternion.identity;
        protected bool m_addVertexState; // current state of the button to add vertex
        protected bool m_delVertexState; // current state of the button to remove vertex
        protected bool m_lightEdit = false; // are we currently editing the lights
        protected List<IDisposable> m_subs = new List<IDisposable>();

        private List<SelectionType> SELECT_SELECTION_TYPES = new List<SelectionType>() { SelectionType.SELECT, SelectionType.SELECTALL, SelectionType.MOVEAXIS };

        public void Start()
        {
            Debug.Log("Avatar awakens");
            m_appState = State.instance;
            m_appState.trackingSpace = MovementVector;
            m_appState.mainCamera = MainCamera;
            m_thisRigidbody = GetComponent<Rigidbody>();
            m_thisRigidbody.detectCollisions = false;
            m_subs.Add(m_appState.ButtonStatus.Event.Subscribe(select));
            m_subs.Add(m_appState.ButtonStatus.Event.Subscribe(unSelect));
            m_subs.Add(m_appState.Project.Event.Subscribe(onProjectLoad));
            m_subs.Add(m_appState.LayerUpdate.AddEvents.Subscribe(LayerAdded));
            StartCoroutine(Orient());
            m_subs.Add(m_appState.ConfigEvent.Subscribe(onConfigLoaded));
            m_subs.Add(m_appState.MapScale.Event.Subscribe(m_Scale));
        }

        public void Update()
        {
            //do nothing
        }

        public void OnDestroy() {
            m_subs.ForEach(sub => sub.Dispose());
        }

        IEnumerator Orient()
        {
            while (true)
            {
                m_appState.Orientation.Set(m_appState.mainCamera.transform.forward);
                yield return new WaitForSeconds(2f);
            }
        }

        /// <summary>
        /// Tasks to be performed when a project is fully loaded
        /// </summary>
        protected virtual void onProjectLoad(ProjectEventType thisEvent)
        {
            // do nothing
        }

        /// <summary>
        /// Tasks to be performed when a layer is loaded
        /// </summary>
        protected virtual void LayerAdded(IVirgisLayer layer) {
            // do nothing
        }

        /// <summary>
        /// Overload this to set actions to be taken when the Config Loaded event is triggered
        /// </summary>
        /// <param name="thisEvent"></param>
        protected virtual void onConfigLoaded(bool thisEvent) {
            // do nothing
        }
        

        //
        // Internal methods common to both UIs
        //
        public void Pan(float pan)
        {
            if (pan != 0)
            {
                m_panTarget *= Quaternion.AngleAxis(pan, Vector3.up);
            }
        }

        public void ScaleRelative(float factor)
        {
            if (factor != 0)
            {
                Scale(State.instance.Zoom.Get() * (1 - factor));
            }
        }

        public void Scale(float zoom)
        {
            m_appState.SetScale(zoom);
        }

        private void m_Scale(float zoom) {
            if (zoom == 0) return;
            transform.localScale = Vector3.one * zoom;
        }

        public void moveTo(MoveArgs args)
        {
            if (!m_axisEdit)
            {
                m_currentSelected?.SendMessage("MoveTo", args, SendMessageOptions.DontRequireReceiver);
            }
        }

        protected virtual void select(ButtonStatus button) 
        {
            if (
                button.activate &&
                SELECT_SELECTION_TYPES.Contains(m_appState.ButtonStatus.SelectionType) &&
                m_appState.InEditSession() &&
                m_currentPointerHit != null &&
                LayerIsEditable())
            {
                m_editSelected = true;
                m_currentSelected = m_currentPointerHit;
                m_selectedDistance = State.instance.lastHit.distance;
                m_currentSelected.SendMessage("Selected", m_appState.ButtonStatus.SelectionType, SendMessageOptions.DontRequireReceiver);
            }
            else if (button.activate &&
                     button.isLhGrip )
            {
                m_lightEdit = true;
            }
        }

        protected virtual void unSelect(ButtonStatus button)
        {
            if (!button.activate)
            {
                m_editSelected = false;
                if (m_currentSelected != null)
                    m_currentSelected?.SendMessage("UnSelected", m_appState.ButtonStatus.SelectionType, SendMessageOptions.DontRequireReceiver);
                m_currentSelected = null;
                m_selectedDistance = 0;
                m_lightEdit = false;
            }
        }

        protected void MoveAxis(MoveArgs args)
        {
            if (m_axisEdit)
            {
                if (m_currentSelected != null)
                    m_currentSelected?.SendMessage("MoveAxis", args, SendMessageOptions.DontRequireReceiver);
            }
        }

        protected bool LayerIsEditable()
        {
            IVirgisLayer layer;
            if (m_currentSelected != null)
            {
                layer = m_currentSelected.GetComponentInParent<IVirgisLayer>();
            }
            else
            {
                layer = m_currentPointerHit?.GetComponentInParent<IVirgisLayer>();
            }
            if (layer == null || layer !=State.instance.EditSession.editableLayer) return false;
            return layer.IsWriteable;
        }

        protected void MoveCamera(Vector3 force)
        {
            m_thisRigidbody.AddForce(m_appState.trackingSpace.rotation * force, ForceMode.Force);
        }

        protected void AddVertex(Vector3 pos)
        {
            if (m_appState.InEditSession() && m_currentPointerHit != null && LayerIsEditable())
            {
                m_currentPointerHit.SendMessage("AddVertex", pos, SendMessageOptions.DontRequireReceiver);
                m_addVertexState = false;
            }
        }

        protected void RemoveVertex()
        {
            if (m_appState.InEditSession() && m_currentSelected != null && LayerIsEditable())
            {
                m_currentSelected.SendMessage("Delete", SendMessageOptions.DontRequireReceiver);
                m_currentSelected = null;
                m_currentPointerHit = null;
            }
        }
    }
}