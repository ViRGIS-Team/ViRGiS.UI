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

namespace Virgis {
    public class EditModeUI : MonoBehaviour {
        public Toggle snapGridToggle;
        public Toggle snapAnchorToggle;

        private State _mAppState;

        // Start is called before the first frame update
        void Start() {
            _mAppState = State.Instance;

            // There is a bug in Unity where if you set OnValueChange event
            // in Unity inspector, the method will be called with the same
            // boolean value (either true/false depending on what you set)
            // regardless of the actual Toggle value.
            // The workaround is to set onValueChange listener in the script.
            snapGridToggle?.onValueChanged.AddListener(OnSnapGridToggleValueChanged);
            snapAnchorToggle?.onValueChanged.AddListener(OnSnapAnchorToggleValueChanged);
        }

        // Select the EditOn toggle button and invoke StartEdit event.
        protected void OnSnapGridToggleValueChanged(bool enable) {
            if (enable) {
                if (snapAnchorToggle.isOn) {
                    snapAnchorToggle.isOn = false;
                }
                _mAppState.EditSession.mode = EditSession.EditMode.SnapGrid;
                print($"Edit mode = {_mAppState.EditSession.mode}");
            } else {
                if (!snapAnchorToggle.isOn) {
                    _mAppState.EditSession.mode = EditSession.EditMode.None;
                    print($"Edit mode = {_mAppState.EditSession.mode}");
                }
            }
        }

        // Select the EditOff toggle button and invoke EndEdit event.
        protected void OnSnapAnchorToggleValueChanged(bool enable) {
            if (enable) {
                if (snapGridToggle.isOn) {
                    snapGridToggle.isOn = false;
                }
                _mAppState.EditSession.mode = EditSession.EditMode.SnapAnchor;
                print($"Edit mode = {_mAppState.EditSession.mode}");
            } else {
                if (!snapGridToggle.isOn) {
                    _mAppState.EditSession.mode = EditSession.EditMode.None;
                    print($"Edit mode = {_mAppState.EditSession.mode}");
                }
            }
        }
    }
}