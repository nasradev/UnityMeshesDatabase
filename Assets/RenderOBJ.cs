using System.Collections.Generic;
using System.IO;
using UnityEngine;
using HoloToolkit.Unity;
using UnityEngine.UI;
using System.Xml.Serialization;
#if !UNITY_EDITOR
using System.Threading.Tasks;
using System.Threading;
using Windows.Web;
using System.Globalization;

using Windows.Foundation;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
#endif
using System;
// Note: The VertexLitConfigurable (or any other default shader as set in OBJLoader)
// needs to have _UseColor set to true ([Toggle] _UseColor("Enabled?", Float) = 1) 
// for the mesh colours to take effect.

// The other requirement  is for the OBJImport asset to be imported, as downloaded from the
// Unity asset store.
public class RenderOBJ : Singleton<RenderOBJ> {

    int meshCount = 0;
    int meshesDownloaded = 0;
    bool meshesLoaded = false;
    bool meshSelected = false;
    string objPath;
    string serverURL = "http://129.31.13.219:8000/";

    #if !UNITY_EDITOR
    BackgroundDownloader downloader;
    private List<DownloadOperation> activeDownloads;
    private CancellationTokenSource cts;
    #endif

    public Catalog CasesCatalog { get; set; }
    public static string CaseSelectionID { get; set; }
    List<OBJFile> objFiles;

    GameObject meshParent;
    void Start () {
        meshParent = GameObject.Find("MeshParent");
        CaseSelectionID = "";
#if !UNITY_EDITOR

        downloader = new BackgroundDownloader();
        cts = new CancellationTokenSource();
        activeDownloads = new List<DownloadOperation>();
        // Read the main Library XML file
        XmlSerializer serializer = new XmlSerializer(typeof(Catalog));
        string catalogPath = Path.Combine(Application.streamingAssetsPath, "MeshLibrary.xml");
        using (FileStream fileStream = new FileStream(catalogPath, FileMode.Open, FileAccess.Read))
        {
            CasesCatalog = (Catalog)serializer.Deserialize(fileStream);
        }

        // Setup the case panel options
        GameObject panel = GameObject.Find("CaseSelectorPanel");
        CasesCatalog.Cases.ForEach(c =>
           {
               GameObject textButton = Instantiate(Resources.Load("CaseTextComponent",
                                                                   typeof(GameObject))) as GameObject;
               textButton.GetComponent<Text>().text = c.ID;
               textButton.transform.SetParent(panel.transform, false);
           });
        

        //objPath = "C:\\Downloads\\";
        objPath = Application.streamingAssetsPath;
#endif
    }

    // Update is called once per frame
    void Update () {
        // User has picked a case via the UI
#if !UNITY_EDITOR
        if (CaseSelectionID.Length > 0 && !meshSelected)
        {
            //objFiles = CasesCatalog.Cases[0].OBJFiles;
            int idx = CasesCatalog.Cases.FindIndex(item => item.ID == CaseSelectionID);
            if (idx < 0)
            {
                return;
            }
            meshSelected = true;
            objFiles = CasesCatalog.Cases[idx].OBJFiles;
            meshCount = objFiles.Count;
            objFiles.ForEach(o =>
            {
                string objURL = serverURL + o.URL;
                //string objPathName = objPath + o.Name + ".obj";
                string objPathName = Path.Combine(ApplicationData.Current.LocalFolder.Path, o.Name + ".obj9");
                setupMeshFileDownload(o.Name + ".obj9", objURL);
                o.filePath = objPathName;
            });
            
        }    
        

        if (meshSelected && (meshesDownloaded == meshCount) && !meshesLoaded)
        {
            objFiles.ForEach(o =>
            {
                GameObject meshObject = OBJLoader.LoadOBJFile(o.filePath);

                meshObject.transform.parent = meshParent.transform;
                meshObject.transform.localPosition = new Vector3(0, 0, 0);
                meshObject.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);
                string[] col = o.Colour.Split(',');
                Color color;
                if (col.Length > 3)
                {
                    color = new Color(float.Parse(col[0]) / 255, float.Parse(col[1]) / 255, float.Parse(col[2]) / 255, float.Parse(col[3]) / 255);
                } else
                {
                    color = new Color(float.Parse(col[0]) / 255, float.Parse(col[1]) / 255, float.Parse(col[2]) / 255);
                }

                GameObject child = meshObject.transform.GetChild(0).gameObject;
                child.GetComponent<Renderer>().material.SetFloat("_UseColor", 1f);
                child.GetComponent<Renderer>().material.SetColor("_Color", color);
                child.GetComponent<Renderer>().material.EnableKeyword("_USECOLOR_ON");
                child.GetComponent<Renderer>().material.DisableKeyword("_USEMAINTEX_ON");
                child.GetComponent<Renderer>().material.DisableKeyword("_USEEMISSIONTEX_ON");
                child.GetComponent<Renderer>().material.SetColor("_Color", color);
                Debug.Log(meshObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.GetColor("_Color").ToString());
            });
            meshesLoaded = true; 
        }
#endif
	}

#if !UNITY_EDITOR
    async void setupMeshFileDownload(string objName, string objURL)
    {
        //if (File.Exists(objPath))
        //{
        //    meshesDownloaded++;
        //}
        //else
        //{
        //    using (WebClient wc = new WebClient())
        //    {
        //        wc.DownloadProgressChanged += wc_DownloadProgressChanged;
        //        wc.DownloadFileAsync(new System.Uri(objURL), objPath);
        //    }
        //    try
        //    {
        //        System.Uri source = new Uri(objURL);
        //        StorageFile destinationFile = KnownFolders
        //            .CreateFileAsync(
        //        title.Text, CreationCollisionOption.GenerateUniqueName);
        //    }
        //}
        StorageFolder sf = ApplicationData.Current.LocalFolder;
        //var f = await sf.TryGetItemAsync(objName);
        //if (f != null)
        //{
        //    meshesDownloaded++;
        //}
        try
        {
            Uri source = new Uri(objURL);

            StorageFile destinationFile = await sf.CreateFileAsync(
                objName, CreationCollisionOption.FailIfExists);

                
            DownloadOperation download = downloader.CreateDownload(source, destinationFile);

            // Attach progress and completion handlers.
            HandleDownloadAsync(download, true);
        }
        catch (Exception ex)
        {
            Debug.Log("Download Error" + ex.Message);
            // File exists
            meshesDownloaded++;
        }
    }

    private async void HandleDownloadAsync(DownloadOperation download, bool start)
    {
        try {
            // Store the download so we can pause/resume.
            activeDownloads.Add(download);

            Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(DownloadProgress);
            if (start)
            {
                // Start the download and attach a progress handler.
                await download.StartAsync().AsTask(cts.Token, progressCallback);
            }
        } catch (TaskCanceledException)
        {
            Debug.Log("Canceled: " + download.Guid +" . ");
        }
        catch (Exception ex)
        {
            if (!IsExceptionHandled("Execution error", ex, download))
            {
                throw;
            }
        }
        finally
        {
            activeDownloads.Remove(download);
        }
    }



    private bool IsExceptionHandled(string title, Exception ex, DownloadOperation download = null)
    {
        WebErrorStatus error = BackgroundTransferError.GetStatus(ex.HResult);
        if (error == WebErrorStatus.Unknown)
        {
            return false;
        }

        if (download == null)
        {
            Debug.Log(String.Format(CultureInfo.CurrentCulture, "Error: {0}: {1}", title, error));
        }
        else
        {
            Debug.Log(String.Format(CultureInfo.CurrentCulture, "Error: {0} - {1}: {2}", download.Guid, title,
                error));
        }

        return true;
    }


    private void DownloadProgress(DownloadOperation download)
    {
        BackgroundDownloadProgress currentProgress = download.Progress;
        Debug.Log(currentProgress.TotalBytesToReceive);
        if (currentProgress.TotalBytesToReceive > 0 && 
                currentProgress.BytesReceived >= currentProgress.TotalBytesToReceive)
        {
            meshesDownloaded++;
        }
    }
#endif
    //void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    //{
    //    if(e.ProgressPercentage == 100)
    //    {
    //        meshesDownloaded++;
    //    }

    //}
}



[XmlRoot("catalog")]
public class Catalog
{
    [XmlElement("case")]
    public List<Case> Cases { get; set; }
}

public class Case
{
    [XmlAttribute("id")]
    public string ID { get; set; }
    [XmlElement("thumbnail")]
    public string Thumbnail { get; set; }
    [XmlElement("date")]
    public string Date { get; set; }
    [XmlElement("ObjFile")]
    public List<OBJFile> OBJFiles { get; set; }
}

public class OBJFile
{
    [XmlElement("name")]
    public string Name { get; set; }
    [XmlElement("url")]
    public string URL { get; set; }
    [XmlElement("colour")]
    public string Colour { get; set; }
    public string filePath { get; set; }
}