//https://stackoverflow.com/questions/58328209/how-to-make-a-free-fly-camera-script-in-unity-with-acceleration-and-decceleratio
// copyright Runette Software Ltd, 2020. All rights reserved
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using R3;

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

        protected State _mappState;
        private Rigidbody _mThisRigidbody;
        protected bool _maxisEdit = false; // Whether we are in AxisEdit mode
        protected Quaternion _mpanTarget = Quaternion.identity;
        protected bool m_addVertexState; // current state of the button to add vertex
        protected bool m_delVertexState; // current state of the button to remove vertex
        protected bool m_lightEdit = false; // are we currently editing the lights
        protected readonly List<IDisposable> _msubs = new ();
        protected readonly List<Coroutine> _mcos = new();

        private List<SelectionType> SELECT_SELECTION_TYPES = new List<SelectionType>() { SelectionType.SELECT, SelectionType.SELECTALL, SelectionType.MOVEAXIS };

        public void Start()
        {
            Debug.Log("Avatar awakens");
            _mappState = State.instance;
            _mappState.trackingSpace = MovementVector;
            _mappState.mainCamera = MainCamera;
            _mThisRigidbody = GetComponent<Rigidbody>();
            _mThisRigidbody.detectCollisions = false;
            _msubs.Add(_mappState.ButtonStatus.Event.Subscribe(select));
            _msubs.Add(_mappState.ButtonStatus.Event.Subscribe(unSelect));
            _msubs.Add(_mappState.Project.Event.Subscribe(onProjectLoad));
            _msubs.Add(_mappState.LayerUpdate.AddEvents.Subscribe(LayerAdded));
            _mcos.Add(StartCoroutine(Orient()));
            _msubs.Add(_mappState.ConfigEvent.Subscribe(onConfigLoaded));
            _msubs.Add(_mappState.MapScale.Event.Subscribe(m_Scale));
        }

        public void Update()
        {
            //do nothing
        }

        public virtual void OnDestroy() {
            _msubs.ForEach(sub => sub.Dispose());
            _mcos.ForEach(co =>
            {
                if (co != null)
                    StopCoroutine(co);
            });
        }

        IEnumerator Orient()
        {
            while (true)
            {
                _mappState.Orientation.Set(_mappState.mainCamera.transform.forward);
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
                _mpanTarget *= Quaternion.AngleAxis(pan, Vector3.up);
            }
        }

        public void ScaleRelative(float factor)
        {
            if (factor != 0)
            {
                Scale(State.instance.MapScale.Get() * (1 - factor));
            }
        }

        public void Scale(float zoom)
        {
            _mappState.SetScale(zoom);
        }

        private void m_Scale(float zoom) {
            if (zoom == 0) return;
            transform.localScale = Vector3.one * zoom;
        }

        public void moveTo(MoveArgs args)
        {
            if (!_maxisEdit && m_currentSelected != null)
            {
                m_currentSelected.SendMessage("MoveTo", args, SendMessageOptions.DontRequireReceiver);
            }
        }

        protected virtual void select(ButtonStatus button) 
        {
            if (
                button.activate &&
                SELECT_SELECTION_TYPES.Contains(_mappState.ButtonStatus.SelectionType) &&
                _mappState.InEditSession() &&
                m_currentPointerHit != null &&
                LayerIsEditable())
            {
                m_editSelected = true;
                m_currentSelected = m_currentPointerHit;
                m_selectedDistance = State.instance.lastHit.distance;
                m_currentSelected.SendMessage("Selected", _mappState.ButtonStatus.SelectionType, SendMessageOptions.DontRequireReceiver);
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
                    m_currentSelected?.SendMessage("UnSelected", _mappState.ButtonStatus.SelectionType, SendMessageOptions.DontRequireReceiver);
                m_currentSelected = null;
                m_selectedDistance = 0;
                m_lightEdit = false;
            }
        }

        protected void MoveAxis(MoveArgs args)
        {
            if (_maxisEdit)
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
            _mThisRigidbody.AddForce(_mappState.trackingSpace.rotation * force, ForceMode.Force);
        }

        protected void AddVertex(Vector3 pos)
        {
            if (_mappState.InEditSession() && m_currentPointerHit != null && LayerIsEditable())
            {
                m_currentPointerHit.SendMessage("AddVertex", pos, SendMessageOptions.DontRequireReceiver);
                m_addVertexState = false;
            }
        }

        protected void RemoveVertex()
        {
            if (_mappState.InEditSession() && m_currentSelected != null && LayerIsEditable())
            {
                m_currentSelected.SendMessage("RemoveVertex", m_currentSelected, SendMessageOptions.DontRequireReceiver);
                m_currentSelected = null;
                m_currentPointerHit = null;
            }
        }
    }
}