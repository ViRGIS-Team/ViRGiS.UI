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

using System.IO;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;


namespace Virgis {

    public class FileMenuPrototype : MonoBehaviour
    {

        public GameObject fileListPanelPrefab;
        public GameObject fileScrollView;
        public GameObject serverListPanelPrefab;
        public string searchPattern;

        protected string MProjectDirectory;
        protected State MAppState;
        protected readonly List<IDisposable> MSubs = new List<IDisposable>();

        protected SearchOption MSearchOptions = SearchOption.TopDirectoryOnly;
        

        // Start is called before the first frame update
        protected virtual void Start()
        {
            MAppState = State.Instance;
        }

        private void OnDestroy()
        {
            MSubs.ForEach(sub => sub.Dispose());
        }

        /// <summary>
        /// Action to be Taken when the File has loaded. Normally just Hide the panels.
        /// </summary>
        /// <param name="thisEvent"></param>
        protected void OnFileLoad(ProjectEventType thisEvent)
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Call this to create the panels for file access
        ///
        /// Note - this will expect projectDirectory and searchPattern to be set.
        /// This will not set the GameObject as Visible. You have to do that
        /// </summary>
        public void CreateFilePanels()
        {
            ClearPanels();

            MProjectDirectory ??= Environment.GetFolderPath(
                Environment.SpecialFolder.MyDocuments
            );

            GameObject newFilePanel = Instantiate(fileListPanelPrefab, fileScrollView.transform);

            // obtain the panel script
            FileListPanel panelScript = newFilePanel.GetComponentInChildren<FileListPanel>();

            // set the filein the panel
            panelScript.Directory = "..";

            panelScript.AddListener(OnFileSelected);

            if (MSearchOptions == SearchOption.TopDirectoryOnly)
            {
                foreach (string directory in Directory.GetDirectories(MProjectDirectory))
                {

                    if (!Regex.Match(Path.GetFileName(directory), @"^\..*").Success)
                    {

                        //Create this filelist panel
                        newFilePanel = Instantiate(fileListPanelPrefab, fileScrollView.transform);

                        // obtain the panel script
                        panelScript = newFilePanel.GetComponentInChildren<FileListPanel>();

                        // set the filein the panel
                        panelScript.Directory = directory;

                        panelScript.AddListener(OnFileSelected);
                    }
                }
            }

            // get the file list
            foreach (string file in Directory.GetFiles(MProjectDirectory, "*", MSearchOptions))
            {

                if (!Regex.Match(Path.GetFileName(file), @"^\..*").Success && Regex.Match(Path.GetFileName(file), searchPattern).Success)
                {

                    //Create this filelist panel
                    newFilePanel = Instantiate(fileListPanelPrefab, fileScrollView.transform);

                    // obtain the panel script
                    panelScript = newFilePanel.GetComponentInChildren<FileListPanel>();

                    // set the filein the panel
                    panelScript.File = file;

                    panelScript.AddListener(OnFileSelected);
                }
            }
            gameObject.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 1f;
        }

        /// <summary>
        /// Call this to clear the panels
        ///
        /// </summary>
        public void ClearPanels()
        {
            if (fileScrollView.transform.childCount > 0)
            {
                for (int i = 0; i < fileScrollView.transform.childCount; i++)
                {
                    Destroy(fileScrollView.transform.GetChild(i).gameObject);
                }
            }
        }
        
        /// <summary>
        /// Actions that are taken when the user clicks on an item
        /// </summary>
        /// <param name="event"></param>
        protected virtual void OnFileSelected(VirgisServerDetails @event)
        {
            throw  new NotImplementedException();
        }
    }
}
