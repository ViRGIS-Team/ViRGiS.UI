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
using R3;

namespace Virgis {

    /// <summary>
    /// MenuFacade is the mediator for all components within the Menus GO.
    /// </summary>
    /// 

    public class MenuFacade : MonoBehaviour {

        public Button startEditButton;
        public Button stopSaveEditButton;
        public Button stopDiscardEditButton;
        public Button fileButton;
        public Button quitButton;
        public GameObject layersUI;
        public GameObject startMenu;

        public bool allowFileButton;

        protected State MAppState;
        private IDisposable _startsub;
        private IDisposable _stopsub;

        // Start is called before the first frame update
        protected virtual void Start() {
            MAppState = State.instance;
            if (MAppState.EditSession.IsActive()) {
                startEditButton.interactable = false;
                stopSaveEditButton.interactable = true;
                stopDiscardEditButton.interactable = true;
            } else {
                startEditButton.interactable = true;
                stopSaveEditButton.interactable = false;
                stopDiscardEditButton.interactable = false;
            }
            
            if (!allowFileButton)  fileButton.interactable = false;

            _startsub = MAppState.EditSession.StartEvent.Subscribe(OnEditSessionStart);
            _stopsub = MAppState.EditSession.EndEvent.Subscribe(OnEditSessionEnd);
        }

        private void OnDestroy() {
            _startsub.Dispose();
            _stopsub.Dispose();
        }

        public virtual void Visible(bool thisEvent) {
            bool isActive = gameObject.activeSelf;
            if (isActive) {
                gameObject.SetActive(false);
            } else {
                gameObject.SetActive(true);
            }
        }

        public virtual void OnShowLayersButtonClicked() {
            gameObject.SetActive(false);
            layersUI.SetActive(true);
        }

        public virtual void OnStartEditButtonClicked() {
            MAppState.StartEditSession();
        }

        public virtual void OnStopSaveEditButtonClicked() {
            MAppState.StopSaveEditSession();
        }

        public virtual void OnStopDiscardEditButtonClicked() {
            MAppState.StopDiscardEditSession();
        }

        public virtual void onFileClicked() {
            startMenu.SetActive(!startMenu.activeSelf);
            startMenu.GetComponent<FileMenuPrototype>().CreateFilePanels();
        }

        public virtual void OnAddDataButtonClicked()
        {
            //do nothing
        }

        public virtual void OnNewProjectButtonClicked()
        {
            //do nothing
        }
        
        public void OnQuitButtonClicked() {
            StartCoroutine(State.instance.Exit().AsIEnumerator());
        }


        // Changes the state of menu buttons when edit session starts.
        // 1) Disable Start Edit button
        // 2) Enable both Stop Edit buttons
        //
        // This method is triggered when:
        // 1) StartEdit action is triggered
        // 2) Start Edit button is clicked
        protected virtual void OnEditSessionStart(bool ignore) {
            startEditButton.interactable = false;
            stopSaveEditButton.interactable = true;
            stopDiscardEditButton.interactable = true;
            fileButton.interactable = false;
            quitButton.interactable = false;
        }

        // Changes the state of menu buttons when edit session ends.
        // 1) Enable Start Edit button
        // 2) Disable both Stop Edit buttons
        //
        // This method is triggered when:
        // 1) EndEdit action is triggered
        // 2) One of the Stop Edit buttons is clicked
        protected virtual void OnEditSessionEnd(bool saved) {
            startEditButton.interactable = true;
            stopSaveEditButton.interactable = false;
            stopDiscardEditButton.interactable = false;
            if (allowFileButton) fileButton.interactable = true;
            quitButton.interactable = true;
        }
    }
}