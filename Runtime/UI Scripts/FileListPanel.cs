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
using System.IO;

namespace Virgis {

    public class FileListPanel : MonoBehaviour {

        public Text text;
        public Image icon;
        public bool isDirectory = false;
        public bool isServer = false;

        [System.Serializable]
        public class FileSelectedEvent : UnityEvent<FileListPanel> {
        }

        private string m_file;
        private VirgisServerDetails m_server;
        private FileSelectedEvent m_fileSelected = new FileSelectedEvent();


        public string File {
            get => m_file;
            set {
                m_file = value;

                // name to be displayed is the filename part without extension, 

                string displayName = Path.GetFileNameWithoutExtension(m_file);
                text.text = displayName;
            }
        }

        public string Directory {
            set {
                File = value;
                icon.gameObject.SetActive(true);
                isDirectory = true;
            }
        }

        public VirgisServerDetails Server { 
            get { return m_server; }
            set {
                m_server = value;
                icon.gameObject.SetActive(false);
                isServer = true;

                // name to be displayed is the Server name : Model Name, 

                string displayName = value.ServerName + " : " + value.ModelName;
                text.text = displayName;
            }
        }

        public void addFileSelectedListerner(UnityAction<FileListPanel> action) {
            m_fileSelected.AddListener(action);
        }

        public void onFileSelected() {
            m_fileSelected.Invoke(this);
        }
    }
}
