using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml.Serialization;
using UnityEngine;

// Note: The VertexLitConfigurable (or any other default shader as set in OBJLoader)
// needs to have _UseColor set to true ([Toggle] _UseColor("Enabled?", Float) = 1) 
// for the mesh colours to take effect.

public class RenderOBJ : MonoBehaviour {

    int meshCount = 0;
    int meshesDownloaded = 0;
    bool meshesLoaded = false;
    string objPath;
    string serverURL = "http://192.168.1.87:8000/";

    Catalog catalog;
    List<OBJFile> objFiles;

    GameObject meshParent;
    void Start () {
        meshParent = GameObject.Find("MeshParent");

        // Read the main Library XML file
        XmlSerializer serializer = new XmlSerializer(typeof(Catalog));
        using (FileStream fileStream = new FileStream("Assets/MeshLibrary.xml", FileMode.Open))
        {
            catalog = (Catalog)serializer.Deserialize(fileStream);
        }

        // Pick a case 
        // TODO: Do this via UI
        objPath = "C:\\Downloads\\";
        objFiles = catalog.Cases[0].OBJFiles;
        meshCount = objFiles.Count;
        objFiles.ForEach(o =>
        {
            string objURL = serverURL + o.URL;
            string objPathName = objPath + o.Name + ".obj";
            setupMeshFileDownload(objPathName, objURL);
            o.filePath = objPathName;
        });
        
        
        
    }
	
	// Update is called once per frame
	void Update () {
		if ((meshesDownloaded == meshCount) && !meshesLoaded)
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
            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadFileAsync(new System.Uri(objURL), objPath);
            }
        }
    }
    void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    {
        if(e.ProgressPercentage == 100)
        {
            meshesDownloaded++;
        }
            
    }
}



[XmlRoot("catalog")]
public class Catalog
{
    [XmlElement("case")]
    public List<Case> Cases { get; set; }
}

public class Case
{
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

public class CaseObject
{
    string id;
    string[] objURLs;

}