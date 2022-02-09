using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Ghostscript.NET.Processor;
using Ghostscript.NET;
using System.Threading;
using UnityEngine.UI;

//https://github.com/jhabjan/Ghostscript.NET
/*
https://3dmpengines.tistory.com/1745
Unity 파일 경로
Application.dataPath (R) : Asset폴더
Application.persistentDataPath (RW) : C:\Users\[user name]\AppData\LocalLow\[company name]\[product name]

*/
public class PdfController : MonoBehaviour
{
    // Start is called before the first frame update

    private Thread thread;

    private Queue<ImageInfo> queue = new Queue<ImageInfo>();

    private GhostscriptProcessor proc;

    private List<string> switches;


    // 로딩관련
    public Image image_fill;
    private bool isEnded = true;

    private int totCnt=0;

    private List<ImageInfo> imgList;

    private string savePath;

    private string fileName;

    private class ImageInfo{
        public int page;
        public string path;

        public ImageInfo(int page, string path){
            this.page = page;
            this.path = path;
        }
    }

    void Start()
    {
        GhostscriptVersionInfo gvi = new GhostscriptVersionInfo(@"C:\Program Files\gs\gs9.55.0\bin\gsdll64.dll");
        proc = new GhostscriptProcessor(gvi);
        image_fill.fillAmount = 0;
        imgList = new List<ImageInfo>();
    }   

    // Update is called once per frame
    void Update()
    {        
        if(queue.Count>0){
            ImageInfo curPageInfo = queue.Dequeue();
            imgList.Add(curPageInfo);
            float percent = curPageInfo.page/(float)totCnt;
            Debug.Log("tot>> " + totCnt + "current >> "+curPageInfo.page + "percent >> "+percent);
            image_fill.fillAmount = (percent);
        }

        if(isEnded){
            Debug.Log("Done >> "+imgList);
            return;
        }

    }

    public void click(){
        fileName = "test.pdf";
        string inputFile = Application.dataPath+"/"+fileName;
        string outputFile = Application.persistentDataPath+"/"+fileName+"-%d.png";
        savePath = Application.persistentDataPath;

        proc.Processing += new GhostscriptProcessorProcessingEventHandler(ghostscript_Processing);        
        
        Debug.Log("input >> " +inputFile);
        Debug.Log("output >> " +outputFile);

        switches = new List<string>();
        switches.Add("-empty");
        switches.Add("-dSAFER");
        switches.Add("-dBATCH");
        switches.Add("-dNOPAUSE");
        switches.Add("-dNOPROMPT");
        switches.Add("-sDEVICE=png16m");
        switches.Add("-r200");
        switches.Add("-dTextAlphaBits=4");
        switches.Add("-dGraphicsAlphaBits=4");
        switches.Add(@"-sOutputFile=" + outputFile);
        switches.Add(@"-f");
        switches.Add(inputFile);

        thread = new Thread(Run);
        thread.Start();
    }

    private void Run(){
        proc.Process(switches.ToArray());
    }

    void ghostscript_Processing(object sender, GhostscriptProcessorProcessingEventArgs e)
    {   
        Debug.Log(e.CurrentPage.ToString() + " / " + e.TotalPages.ToString());
        ImageInfo imageInfo = new ImageInfo(e.CurrentPage,savePath+"/"+fileName+"-"+e.CurrentPage.ToString()+".png");

        if(totCnt==0){
            totCnt = e.TotalPages;
            isEnded = false;
        }
        if(e.CurrentPage == e.TotalPages){
            isEnded = true;
            if(thread != null){
                thread.Abort();
            }
        }
        queue.Enqueue(imageInfo);
        
    }

    void OnApplicationQuit(){
        if(thread != null){
            thread.Abort();
        }
    }
    
}
