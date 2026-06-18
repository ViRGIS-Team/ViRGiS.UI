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
using R3;


namespace Virgis {

    public class StartFacade : MonoBehaviour
    {

        public GameObject fileListPanelPrefab;
        public GameObject fileScrollView;
        public GameObject serverListPanelPrefab;
        public string searchPattern;

        protected string m_projectDirectory;
        protected State m_appState;
        protected List<IDisposable> m_subs = new List<IDisposable>();

        protected SearchOption m_searchOptions = SearchOption.AllDirectories;
        

        // Start is called before the first frame update
        protected virtual void Start()
        {
            m_appState = State.instance;
            m_subs.Add(m_appState.Project.Event.Subscribe(OnProjectLoad));
            m_subs.Add(State.instance.ServerEvent.Subscribe(OnServerRegistered));
            m_subs.Add(State.instance.Client.Event.Subscribe(OnClientConnect));
        }

        private void OnDestroy()
        {
            m_subs.ForEach(sub => sub.Dispose());
        }

        /// <summary>
        /// Action to be Taken when the Project has loaded. Normally just Hide the panels.
        /// </summary>
        /// <param name="proj"></param>
        private void OnProjectLoad(ProjectEventType thisEvent)
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
            GameObject newFilePanel;

            ClearPanels();
            try
            {
                if (m_projectDirectory == null)
                {
                    m_projectDirectory = Path.GetPathRoot(
                        Environment.GetFolderPath(
                            Environment.SpecialFolder.MyDocuments)
                        );
                }
            }
            catch (Exception e)
            {
                _ = e;
                m_projectDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }


            newFilePanel = Instantiate(fileListPanelPrefab, fileScrollView.transform);

            // obtain the panel script
            FileListPanel panelScript = newFilePanel.GetComponentInChildren<FileListPanel>();

            // set the filein the panel
            panelScript.Directory = "..";

            panelScript.AddListener(OnFileSelected);

            if (m_searchOptions == SearchOption.TopDirectoryOnly)
            {
                foreach (string directory in Directory.GetDirectories(m_projectDirectory))
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
            foreach (string file in Directory.GetFiles(m_projectDirectory, "*", m_searchOptions))
            {

                if (!Regex.Match(Path.GetFileName(file), @"^\..*").Success && Regex.Match(Path.GetFileName(file), searchPattern).Success)
                {

                    //Create this filelist panel
                    newFilePanel = (GameObject)Instantiate(fileListPanelPrefab, fileScrollView.transform);

                    // obtain the panel script
                    panelScript = newFilePanel.GetComponentInChildren<FileListPanel>();

                    // set the filein the panel
                    panelScript.File = file;

                    panelScript.AddListener(OnFileSelected);
                }
            };
            gameObject.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 1f;
        }

        /// <summary>
        /// Call this to clear  the panels for server access
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
        protected void OnFileSelected(VirgisServerDetails @event)
        {
            if (@event.IsDirectory)
            {
                // If directory - expand Directory
                if (@event.ServerName == "..")
                {
                    m_projectDirectory = Path.GetDirectoryName(m_projectDirectory);
                }
                else
                {
                    m_projectDirectory = @event.ServerName;
                }
                m_appState.SetConfig("CurrentFolder", @event.ServerName);
                CreateFilePanels();
                return;
            }
            if (@event.IsServer)
            {

                // if Server - connect to the server
                m_appState.ConnectClient(@event);
                return;
            }
            //otherwise - open the file
            Debug.Log($"File selected : {@event.ServerName}");
            gameObject.SetActive(false);

            // Kill off all of the existing layers
            m_appState.UnloadProject(() => m_loadProject(@event.ServerName));
        }

        public void m_loadProject(string file)
        {
            //create the new layers
            if (!m_appState.LoadProject(file))
            {
                gameObject.SetActive(true);
            }
        }

        protected void OnServerRegistered(VirgisServerDetails details)
        {
            if (details.Endpoint == null) return;
            //Create this filelist panel
            GameObject newServerPanel = Instantiate(serverListPanelPrefab, fileScrollView.transform);

            // obtain the panel script  
            ContainerPanel serverPanel = newServerPanel.GetComponentInChildren<ContainerPanel>();

            //set the server name
            
            serverPanel.SetPanelText(details.ServerName);
            serverPanel.Expand(true);
            
            // Add the model panel
            
            FileListPanel modelPanel = serverPanel.AddPanel<FileListPanel>();
            modelPanel.Server =  details;
            
            modelPanel.AddListener(OnFileSelected);
            gameObject.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 1f;
        }

        protected void OnClientConnect(ClientEventType eventType)
        {
            switch (eventType)
            {
                case ClientEventType.Started:
                    gameObject.SetActive(false);
                    break;
                case ClientEventType.Complete:
                    break;
                case ClientEventType.Failed:
                    gameObject.SetActive(true);
                    break;
            }
        }
    }
}
