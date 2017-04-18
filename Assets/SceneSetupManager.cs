using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

public class SceneSetupManager : MonoBehaviour {

    Catalog catalog;
	// Use this for initialization
	void Start () {
        // Read the main library XML file to get all available meshes
        XmlSerializer serializer = new XmlSerializer(typeof(Catalog));
        using (FileStream fileStream = new FileStream("Assets/MeshLibrary.xml", FileMode.Open))
        {
            catalog = (Catalog)serializer.Deserialize(fileStream);
        }

        for (int i = 0; i < catalog.Cases.Count; i++)
        {
            StartCoroutine(setImage(catalog.Cases[i].Thumbnail));
        }
        
    }

    // Update is called once per frame
    void Update () {
		
	}

    IEnumerator setImage(string url)
    {

        WWW www = new WWW(url); 
        yield return www;

        Texture2D texture = new Texture2D(www.texture.width, www.texture.height, TextureFormat.DXT1, false);

        www.LoadImageIntoTexture(texture);
        //Rect rec = new Rect(0, 0, texture.width, texture.height);
        //Sprite spriteToUse = Sprite.Create(texture, rec, new Vector2(0.5f, 0.5f), 100);

        //GameObject panel = GameObject.Find("Panel");
        //GameObject image = Instantiate(Resources.Load("Image")) as GameObject;


        //Renderer renderer = image.GetComponent<Renderer>();
        //renderer.material.mainTexture = www.texture;

        //image.transform.parent = panel.transform;
        //Sprite sp = image.GetComponent<Sprite>();
        //sp = spriteToUse;

        www.Dispose();
        www = null;
    }

}

