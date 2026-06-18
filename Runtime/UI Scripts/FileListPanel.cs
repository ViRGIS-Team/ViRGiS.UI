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

    public class FileListPanel : Panel<VirgisServerDetails> {
        
        public Image icon;

        public string File {
            get => panelValue.ServerName;
            set
            {

                panelValue = new VirgisServerDetails
                {
                    Endpoint = null,
                    ServerName = value,
                    ModelName = null,
                    IsDirectory = false,
                    IsServer = false,
                    IsFile = true
                };

                // name to be displayed is the filename part without extension, 
                icon.gameObject.SetActive(false);
                panelNameText.text = Path.GetFileNameWithoutExtension(value);
            }
        }

        public string Directory {
            get => panelValue.ServerName;
            set {
                
                panelValue = new VirgisServerDetails
                {
                    Endpoint = null,
                    ServerName = value,
                    ModelName = null,
                    IsDirectory = true,
                    IsServer = false,
                    IsFile = false
                };

                icon.gameObject.SetActive(true);
                if (value == "..")
                {
                    panelNameText.text = "..";
                    return;
                }
                panelNameText.text = new DirectoryInfo(value).Name;
            }
        }

        public VirgisServerDetails Server { 
            get => panelValue;
            set {
                panelValue = value;
                icon.gameObject.SetActive(false);

                // name to be displayed is the Server name : Model Name, 
                
                panelNameText.text = value.ModelName;
            }
        }
    }
}
