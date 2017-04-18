
using UnityEngine;
using HoloToolkit.Unity;
using UnityEngine.UI;
// Note: The VertexLitConfigurable (or any other default shader as set in OBJLoader)
// needs to have _UseColor set to true ([Toggle] _UseColor("Enabled?", Float) = 1) 
// for the mesh colours to take effect.

// The other requirement  is for the OBJImport asset to be imported, as downloaded from the
// Unity asset store.
public class RenderOBJ : Singleton<RenderOBJ> {

    int meshCount = 0;
    int meshesDownloaded = 0;
    bool meshesLoaded = false;
    string objPath;
    string serverURL = "http://192.168.1.87:8000/";

    public Catalog CasesCatalog { get; set; }
    public static string CaseSelectionID { get; set; }
    List<OBJFile> objFiles;

    GameObject meshParent;
    void Start () {
        meshParent = GameObject.Find("MeshParent");
        CaseSelectionID = "";
        // Read the main Library XML file
        XmlSerializer serializer = new XmlSerializer(typeof(Catalog));
        using (FileStream fileStream = new FileStream("Assets/MeshLibrary.xml", FileMode.Open))
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

    }

    // Update is called once per frame
    void Update () {
        // User has picked a case via the UI
        if (CaseSelectionID.Length > 0)
        {
            //objFiles = CasesCatalog.Cases[0].OBJFiles;
            int idx = CasesCatalog.Cases.FindIndex(item => item.ID == CaseSelectionID);
            if (idx < 0)
            {
                return;
            }

            objFiles = CasesCatalog.Cases[idx].OBJFiles;
            meshCount = objFiles.Count;
            objFiles.ForEach(o =>
            {
                string objURL = serverURL + o.URL;
                string objPathName = objPath + o.Name + ".obj";
                setupMeshFileDownload(objPathName, objURL);
                o.filePath = objPathName;
            });
        }    
        

        if ((CaseSelectionID.Length > 0) && (meshesDownloaded == meshCount) && !meshesLoaded)
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

                
                //meshObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.SetFloat("_UseColor", 1f);
                meshObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.SetColor("_Color", color);
                
            });
            meshesLoaded = true; 
        }
	}

    void setupMeshFileDownload(string objPath, string objURL)
    {
        if (File.Exists(objPath))
        {
            meshesDownloaded++;
        }
        else
        {
            //using (WebClient wc = new WebClient())
            //{
            //    wc.DownloadProgressChanged += wc_DownloadProgressChanged;
            //    wc.DownloadFileAsync(new System.Uri(objURL), objPath);
            //}
            //try
            //{
                //System.Uri source = new Uri(objURL);
                //StorageFile destinationFile = KnownFolders 
                //    .CreateFileAsync(
                //title.Text, CreationCollisionOption.GenerateUniqueName);
            //}
        }
    }
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