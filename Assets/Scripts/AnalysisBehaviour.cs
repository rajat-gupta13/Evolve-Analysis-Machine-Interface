using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Phidget22;
using Phidget22.Events;
using System.IO;
using UnityEngine.UI;

public class AnalysisBehaviour : MonoBehaviour
{
    OscOut _oscOut;
    OscIn _oscIn;
    //private OSCController osc;
    private DigitalInput ScanButton = new DigitalInput();
    WebCamTexture webcamTexture = null;
    Texture2D loadTexture = null;
    byte[] fileData;
    private string savePath = "Z:\\";
    int captureCounter = 0;
    const string address = "/puppet";
    public RawImage rawimageNoMatch;
    public RawImage rawimageMatch;
    public GameObject instructions;
    public GameObject scanning;
    public GameObject panelLeft;
    public GameObject noMatch, resetButtonFullScreen, noMatchPanelImage;
    public GameObject panelMatchesPelvis, panelMatchesHand;
    public Text scannedSampleTitle;
    public Shader greyscale;
    public GameObject panelPelvis1, panelPelvis2, panelPelvis3, panelPelvis4;
    public GameObject panelHand1, panelHand2, panelHand3, panelHand4;
    private bool pelvis, hand = false;

    public List<string> pelvisFunctions = new List<string>();
    public List<string> handFunctions = new List<string>();
    public string pelvisHabitat;
    public string handHabitat;

    public Button panelLeftFunction1, panelLeftFunction2, panelLeftFunction3, panelLeftHabitat;
    private int matches = 0;
    public string captureName = "capturedScan";

    public Button[] pelvisWalkingUpright;
    public Button[] pelvisLoweredCOM;
    public Button[] pelvisTravellingLongDistances;
    public Button[] pelvisLiveBirthing;
    public Button[] pelvisBalancing;
    public Button[] pelvisFourLegged;
    public Button[] pelvisLargeMasses;
    public Button[] pelvisOccasionalUpright;
    public Button[] pelvisHabitatGrassland;
    public Button[] pelvisHabitatMountain;
    public Button[] pelvisHabitatTropical;

    public Button[] handDigging;
    public Button[] handBreaking;
    public Button[] handPiercing;
    public Button[] handThrowing;
    public Button[] handPrecisionGripping;
    public Button[] handReaching;
    public Button[] handGrabbing;
    public Button[] handClawing;
    public Button[] handTearing;
    public Button[] handHabitatUnderground;
    public Button[] handHabitatGrassland;
    public Button[] handHabitatTropical;

    public GameObject panelFunctionsFilled;
    public GameObject panelHabitatFilled;
    public GameObject panelVerificationScore;
    public Text score;
    public GameObject dataNumber;

    private List<string> pelvisFunctionsSelected = new List<string>();
    private List<string> handFunctionsSelected = new List<string>();
    private string pelvisHabitatSelected;
    private string handHabitatSelected;
    private int dataCounter = 1;
    public string addressBDC = "/bdc";

    public AudioSource bcdSFX;
    public AudioClip bcdTakePicture, bcdScanNoResult, bcdScanResult;

    private bool receivedResult = true;

    // Start is called before the first frame update
    void Start()
    {
        //osc = GameObject.Find("OSC").GetComponent<OSCController>();
        _oscOut = gameObject.AddComponent<OscOut>();
        _oscIn = gameObject.AddComponent<OscIn>();

        _oscOut.Open(8000, "255.255.255.255");
        _oscIn.Open(8000);

        //Save get the camera devices, in case you have more than 1 camera.
        WebCamDevice[] camDevices = WebCamTexture.devices;

        //Get the used camera name for the WebCamTexture initialization.
        string camName = camDevices[0].name;
        webcamTexture = new WebCamTexture(camName);

        ScanButton.DeviceSerialNumber = 523573;
        ScanButton.Channel = 0;
        ScanButton.IsLocal = true;
        ScanButton.Attach += digitalInput_Attach;
        ScanButton.StateChange += digitalInput_StateChange;
        //Render the image in the screen.
        //rawimage.texture = webcamTexture;
        //rawimage.material.mainTexture = webcamTexture;
        _oscIn.MapString("/scan", AnalysisResult);
        _oscIn.MapInt("/resetbcd", ResetMachineCalled);
        try
        {
            ScanButton.Open(5000);
        }
        catch (PhidgetException e)
        {
            Debug.Log("Failed: " + e.Message);
        }

        webcamTexture.Play();
        dataNumber.SetActive(false);
    }

    public void BackButton()
    {
        panelHand1.SetActive(false);
        panelHand2.SetActive(false);
        panelHand3.SetActive(false);
        panelHand4.SetActive(false);
        panelPelvis1.SetActive(false);
        panelPelvis2.SetActive(false);
        panelPelvis3.SetActive(false);
        panelPelvis4.SetActive(false);
        if (pelvis && !hand)
        {
            panelMatchesPelvis.SetActive(true);
        }
        else if (hand && !pelvis)
        {
            panelMatchesHand.SetActive(true);
        }
    }

    public void Pelvis1()
    {
        panelMatchesPelvis.SetActive(false);
        panelPelvis1.SetActive(true);
    }
    public void Pelvis2()
    {
        panelMatchesPelvis.SetActive(false);
        panelPelvis2.SetActive(true);
    }
    public void Pelvis3()
    {
        panelMatchesPelvis.SetActive(false);
        panelPelvis3.SetActive(true);
    }
    public void Pelvis4()
    {
        panelMatchesPelvis.SetActive(false);
        panelPelvis4.SetActive(true);
    }
    public void Hand1()
    {
        panelMatchesHand.SetActive(false);
        panelHand1.SetActive(true);
    }
    public void Hand2()
    {
        panelMatchesHand.SetActive(false);
        panelHand2.SetActive(true);
    }
    public void Hand3()
    {
        panelMatchesHand.SetActive(false);
        panelHand3.SetActive(true);
    }
    public void Hand4()
    {
        panelMatchesHand.SetActive(false);
        panelHand4.SetActive(true);
    }

    private void ResetMachineCalled(int args)
    {
        ResetButton();
    }

    private void AnalysisResult(string scannedObject)
    {
        receivedResult = true;
        if (scannedObject == "pelvis")
        {
            bcdSFX.clip = bcdScanResult;    
            bcdSFX.Play();
            print("Pelvis Scanned");
            instructions.SetActive(false);
            scanning.SetActive(false);
            panelLeft.SetActive(true);
            noMatch.SetActive(false);
            resetButtonFullScreen.SetActive(false);
            noMatchPanelImage.SetActive(false);
            scannedSampleTitle.text = "Scanned Sample #" + captureCounter.ToString("000");
            panelMatchesPelvis.SetActive(true);
            LoadImage(rawimageMatch);
            pelvis = true;
            hand = false;
        }
        else if (scannedObject == "hand")
        {
            bcdSFX.clip = bcdScanResult;
            bcdSFX.Play();
            print("Hand Scanned");
            instructions.SetActive(false);
            scanning.SetActive(false);
            panelLeft.SetActive(true);
            noMatch.SetActive(false);
            resetButtonFullScreen.SetActive(false);
            noMatchPanelImage.SetActive(false);
            scannedSampleTitle.text = "Scanned Sample #" + captureCounter.ToString("000");
            panelMatchesHand.SetActive(true);
            LoadImage(rawimageMatch);
            pelvis = false;
            hand = true;
        }
        else if (scannedObject == "incomplete")
        {
            bcdSFX.clip = bcdScanNoResult;
            bcdSFX.Play();
            print("Incomplete Scan");
            instructions.SetActive(false);
            scanning.SetActive(false);
            noMatch.SetActive(true);
            resetButtonFullScreen.SetActive(true);
            noMatchPanelImage.SetActive(true);
            LoadImage(rawimageNoMatch);
            ResetPanels();
            pelvis = false;
            hand = false;
        }
    }

    private void ResetPanels()
    {
        panelLeft.SetActive(false);
        panelMatchesPelvis.SetActive(false);
        panelPelvis1.SetActive(false);
        panelPelvis2.SetActive(false);
        panelPelvis3.SetActive(false);
        panelPelvis4.SetActive(false);
        panelMatchesHand.SetActive(false);
        panelHand1.SetActive(false);
        panelHand2.SetActive(false);
        panelHand3.SetActive(false);
        panelHand4.SetActive(false);
    }

    public void ResetButton()
    {
        ResetPanels();
        noMatch.SetActive(false);
        resetButtonFullScreen.SetActive(false);
        noMatchPanelImage.SetActive(false);
        instructions.SetActive(true);
        pelvis = false;
        hand = false;
        dataNumber.SetActive(false);
        matches = 0;
        ResetSampleFunctionsAndHabitat();
    }

    private void ResetSampleFunctionsAndHabitat()
    {
        PanelLeftPelvisBalancing();
        PanelLeftPelvisFourLegged();
        PanelLeftPelvisHabitatGrasslands();
        PanelLeftPelvisHabitatMountain();
        PanelLeftPelvisHabitatTropical();
        PanelLeftPelvisLargeMasses();
        PanelLeftPelvisLiveBirthing();
        PanelLeftPelvisLoweredCOM();
        PanelLeftPelvisOccasionalUpright();
        PanelLeftPelvisTravellingLongDistances();
        PanelLeftPelvisWalkingUpright();
        PanelLeftHandBreaking();
        PanelLeftHandClawing();
        PanelLeftHandDigging();
        PanelLeftHandGrabbing();
        PanelLeftHandHabitatGrasslands();
        PanelLeftHandHabitatTropical();
        PanelLeftHandHabitatUnderground();
        PanelLeftHandPiercing();
        PanelLeftHandPrecisionGripping();
        PanelLeftHandReaching();
        PanelLeftHandTearing();
        PanelLeftHandThrowing();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ScanButton.Attach -= digitalInput_Attach;
            ScanButton.StateChange -= digitalInput_StateChange;
            ScanButton.Close();
            ScanButton = null;

            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            AnalysisResult("pelvis");
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            AnalysisResult("hand");
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            AnalysisResult("incomplete");
        }

    }

    void OnApplicationQuit()
    {
        if (Application.isEditor)
            Phidget.FinalizeLibrary(0);
        else
            Phidget.FinalizeLibrary(0);
    }

    void digitalInput_Attach(object sender, Phidget22.Events.AttachEventArgs e)
    {
        DigitalInput attachedDevice = ((DigitalInput)sender);
        int deviceSerial = attachedDevice.DeviceSerialNumber;
        Debug.Log("Attached device " + attachedDevice.DeviceSerialNumber);
    }


    void digitalInput_StateChange(object sender, Phidget22.Events.DigitalInputStateChangeEventArgs e)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(ThisWillBeExecutedOnTheMainThread(ScanButton.State));
    }

    void SaveImage()
    {
        //Create a Texture2D with the size of the rendered image on the screen.
        Texture2D texture = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.ARGB32, false);

        //Save the image to the Texture2D
        texture.SetPixels(webcamTexture.GetPixels());
        texture.Apply();

        //Encode it as a PNG.
        byte[] bytes = texture.EncodeToPNG();
        ++captureCounter;
        //Save it in a file.
        System.IO.File.WriteAllBytes(savePath + captureName + ".png", bytes);

        //Debug.Log(captureCounter);
    }

    void LoadImage(RawImage rawimage)
    {
        string filePath = savePath + captureName + ".png";
        loadTexture = new Texture2D(693, 692, TextureFormat.ARGB32, false);
        if (File.Exists(filePath))
        {
            fileData = File.ReadAllBytes(filePath);
            loadTexture.LoadImage(fileData);
        }
        rawimage.texture = loadTexture;
        rawimage.material.mainTexture = loadTexture;
    }

    public IEnumerator ThisWillBeExecutedOnTheMainThread(bool state)
    {
        Debug.Log("This is executed from the main thread");

        switch (state)
        {

            case true:                    //MicroscopeClickSource.PlayOneShot(MicroscopeClick);
                if (receivedResult)
                {
                    receivedResult = false;
                    bcdSFX.clip = bcdTakePicture;
                    bcdSFX.Play();
                    SaveImage();
                    _oscOut.Send(address, captureCounter);
                    //osc.SendOSCMessage(address + " " + captureCounter.ToString());
                    instructions.SetActive(false);
                    scanning.SetActive(true);
                    panelLeft.SetActive(false);
                    panelMatchesPelvis.SetActive(false);
                    panelPelvis1.SetActive(false);
                    panelPelvis2.SetActive(false);
                    panelPelvis3.SetActive(false);
                    panelPelvis4.SetActive(false);
                    panelMatchesHand.SetActive(false);
                    panelHand1.SetActive(false);
                    panelHand2.SetActive(false);
                    panelHand3.SetActive(false);
                    panelHand4.SetActive(false);
                    ResetSampleFunctionsAndHabitat();
                }
                break;
            case false:
                //RFIDMicroscope.AntennaEnabled = false;
                break;
        }

        yield return null;
    }

    public void PelvisWalkingUpright()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction1.interactable = true;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "Walking upright";
            panelLeftFunction1.onClick.AddListener(PanelLeftPelvisWalkingUpright);
            ToggleButtonsInteractable(pelvisWalkingUpright, false);
            pelvisFunctionsSelected.Add("Walking upright");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction2.interactable = true;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "Walking upright";
            panelLeftFunction2.onClick.AddListener(PanelLeftPelvisWalkingUpright);
            ToggleButtonsInteractable(pelvisWalkingUpright, false);
            pelvisFunctionsSelected.Add("Walking upright");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction3.interactable = true;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "Walking upright";
            panelLeftFunction3.onClick.AddListener(PanelLeftPelvisWalkingUpright);
            ToggleButtonsInteractable(pelvisWalkingUpright, false);
            pelvisFunctionsSelected.Add("Walking upright");
        }
        else
        {
            panelFunctionsFilled.SetActive(true);
        }
    }

    public void PanelLeftPelvisWalkingUpright()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "Walking upright")
        {
            panelLeftFunction1.interactable = false;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "";
            panelLeftFunction1.onClick.RemoveListener(PanelLeftPelvisWalkingUpright);
            ToggleButtonsInteractable(pelvisWalkingUpright, true);
            pelvisFunctionsSelected.Remove("Walking upright");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "Walking upright")
        {
            panelLeftFunction2.interactable = false;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "";
            panelLeftFunction2.onClick.RemoveListener(PanelLeftPelvisWalkingUpright);
            ToggleButtonsInteractable(pelvisWalkingUpright, true);
            pelvisFunctionsSelected.Remove("Walking upright");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "Walking upright")
        {
            panelLeftFunction3.interactable = false;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "";
            panelLeftFunction3.onClick.RemoveListener(PanelLeftPelvisWalkingUpright);
            ToggleButtonsInteractable(pelvisWalkingUpright, true);
            pelvisFunctionsSelected.Remove("Walking upright");
        }
    }

    public void PelvisLoweredCOM()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction1.interactable = true;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "Lowered center of mass";
            panelLeftFunction1.onClick.AddListener(PanelLeftPelvisLoweredCOM);
            ToggleButtonsInteractable(pelvisLoweredCOM, false);
            pelvisFunctionsSelected.Add("Lowered center of mass");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction2.interactable = true;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "Lowered center of mass";
            panelLeftFunction2.onClick.AddListener(PanelLeftPelvisLoweredCOM);
            ToggleButtonsInteractable(pelvisLoweredCOM, false);
            pelvisFunctionsSelected.Add("Lowered center of mass");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction3.interactable = true;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "Lowered center of mass";
            panelLeftFunction3.onClick.AddListener(PanelLeftPelvisLoweredCOM);
            ToggleButtonsInteractable(pelvisLoweredCOM, false);
            pelvisFunctionsSelected.Add("Lowered center of mass");
        }
        else
        {
            panelFunctionsFilled.SetActive(true);
        }
    }

    public void PanelLeftPelvisLoweredCOM()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "Lowered center of mass")
        {
            panelLeftFunction1.interactable = false;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "";
            panelLeftFunction1.onClick.RemoveListener(PanelLeftPelvisLoweredCOM);
            ToggleButtonsInteractable(pelvisLoweredCOM, true);

            pelvisFunctionsSelected.Remove("Lowered center of mass");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "Lowered center of mass")
        {
            panelLeftFunction2.interactable = false;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "";
            panelLeftFunction2.onClick.RemoveListener(PanelLeftPelvisLoweredCOM);
            ToggleButtonsInteractable(pelvisLoweredCOM, true);
            pelvisFunctionsSelected.Remove("Lowered center of mass");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "Lowered center of mass")
        {
            panelLeftFunction3.interactable = false;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "";
            panelLeftFunction3.onClick.RemoveListener(PanelLeftPelvisLoweredCOM);
            ToggleButtonsInteractable(pelvisLoweredCOM, true);
            pelvisFunctionsSelected.Remove("Lowered center of mass");
        }
    }

    public void PelvisTravellingLongDistances()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction1.interactable = true;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "Travelling long distances";
            panelLeftFunction1.onClick.AddListener(PanelLeftPelvisTravellingLongDistances);
            ToggleButtonsInteractable(pelvisTravellingLongDistances, false);
            pelvisFunctionsSelected.Add("Travelling long distances");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction2.interactable = true;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "Travelling long distances";
            panelLeftFunction2.onClick.AddListener(PanelLeftPelvisTravellingLongDistances);
            ToggleButtonsInteractable(pelvisTravellingLongDistances, false);
            pelvisFunctionsSelected.Add("Travelling long distances");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction3.interactable = true;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "Travelling long distances";
            panelLeftFunction3.onClick.AddListener(PanelLeftPelvisTravellingLongDistances);
            ToggleButtonsInteractable(pelvisTravellingLongDistances, false);
            pelvisFunctionsSelected.Add("Travelling long distances");
        }
        else
        {
            panelFunctionsFilled.SetActive(true);
        }
    }

    public void PanelLeftPelvisTravellingLongDistances()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "Travelling long distances")
        {
            panelLeftFunction1.interactable = false;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "";
            panelLeftFunction1.onClick.RemoveListener(PanelLeftPelvisTravellingLongDistances);
            ToggleButtonsInteractable(pelvisTravellingLongDistances, true);
            pelvisFunctionsSelected.Remove("Travelling long distances");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "Travelling long distances")
        {
            panelLeftFunction2.interactable = false;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "";
            panelLeftFunction2.onClick.RemoveListener(PanelLeftPelvisTravellingLongDistances);
            ToggleButtonsInteractable(pelvisTravellingLongDistances, true);
            pelvisFunctionsSelected.Remove("Travelling long distances");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "Travelling long distances")
        {
            panelLeftFunction3.interactable = false;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "";
            panelLeftFunction3.onClick.RemoveListener(PanelLeftPelvisTravellingLongDistances);
            ToggleButtonsInteractable(pelvisTravellingLongDistances, true);
            pelvisFunctionsSelected.Remove("Travelling long distances");
        }
    }

    public void PelvisLiveBirthing()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction1.interactable = true;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "Live birthing";
            panelLeftFunction1.onClick.AddListener(PanelLeftPelvisLiveBirthing);
            ToggleButtonsInteractable(pelvisLiveBirthing, false);
            pelvisFunctionsSelected.Add("Live birthing");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction2.interactable = true;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "Live birthing";
            panelLeftFunction2.onClick.AddListener(PanelLeftPelvisLiveBirthing);
            ToggleButtonsInteractable(pelvisLiveBirthing, false);
            pelvisFunctionsSelected.Add("Live birthing");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction3.interactable = true;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "Live birthing";
            panelLeftFunction3.onClick.AddListener(PanelLeftPelvisLiveBirthing);
            ToggleButtonsInteractable(pelvisLiveBirthing, false);
            pelvisFunctionsSelected.Add("Live birthing");
        }
        else
        {
            panelFunctionsFilled.SetActive(true);
        }
    }

    public void PanelLeftPelvisLiveBirthing()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "Live birthing")
        {
            panelLeftFunction1.interactable = false;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "";
            panelLeftFunction1.onClick.RemoveListener(PanelLeftPelvisLiveBirthing);
            ToggleButtonsInteractable(pelvisLiveBirthing, true);
            pelvisFunctionsSelected.Remove("Live birthing");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "Live birthing")
        {
            panelLeftFunction2.interactable = false;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "";
            panelLeftFunction2.onClick.RemoveListener(PanelLeftPelvisLiveBirthing);
            ToggleButtonsInteractable(pelvisLiveBirthing, true);
            pelvisFunctionsSelected.Remove("Live birthing");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "Live birthing")
        {
            panelLeftFunction3.interactable = false;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "";
            panelLeftFunction3.onClick.RemoveListener(PanelLeftPelvisLiveBirthing);
            ToggleButtonsInteractable(pelvisLiveBirthing, true);
            pelvisFunctionsSelected.Remove("Live birthing");
        }
    }

    public void PelvisBalancing()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction1.interactable = true;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "Balancing";
            panelLeftFunction1.onClick.AddListener(PanelLeftPelvisBalancing);
            ToggleButtonsInteractable(pelvisBalancing, false);
            pelvisFunctionsSelected.Add("Balancing");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction2.interactable = true;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "Balancing";
            panelLeftFunction2.onClick.AddListener(PanelLeftPelvisBalancing);
            ToggleButtonsInteractable(pelvisBalancing, false);
            pelvisFunctionsSelected.Add("Balancing");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction3.interactable = true;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "Balancing";
            panelLeftFunction3.onClick.AddListener(PanelLeftPelvisBalancing);
            ToggleButtonsInteractable(pelvisBalancing, false);
            pelvisFunctionsSelected.Add("Balancing");
        }
        else
        {
            panelFunctionsFilled.SetActive(true);
        }
    }

    public void PanelLeftPelvisBalancing()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "Balancing")
        {
            panelLeftFunction1.interactable = false;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "";
            panelLeftFunction1.onClick.RemoveListener(PanelLeftPelvisBalancing);
            ToggleButtonsInteractable(pelvisBalancing, true);
            pelvisFunctionsSelected.Remove("Balancing");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "Balancing")
        {
            panelLeftFunction2.interactable = false;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "";
            panelLeftFunction2.onClick.RemoveListener(PanelLeftPelvisBalancing);
            ToggleButtonsInteractable(pelvisBalancing, true);
            pelvisFunctionsSelected.Remove("Balancing");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "Balancing")
        {
            panelLeftFunction3.interactable = false;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "";
            panelLeftFunction3.onClick.RemoveListener(PanelLeftPelvisBalancing);
            ToggleButtonsInteractable(pelvisBalancing, true);
            pelvisFunctionsSelected.Remove("Balancing");
        }
    }

    public void PelvisFourLegged()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction1.interactable = true;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "Four-legged movement";
            panelLeftFunction1.onClick.AddListener(PanelLeftPelvisFourLegged);
            ToggleButtonsInteractable(pelvisFourLegged, false);
            pelvisFunctionsSelected.Add("Four-legged movement");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction2.interactable = true;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "Four-legged movement";
            panelLeftFunction2.onClick.AddListener(PanelLeftPelvisFourLegged);
            ToggleButtonsInteractable(pelvisFourLegged, false);
            pelvisFunctionsSelected.Add("Four-legged movement");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction3.interactable = true;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "Four-legged movement";
            panelLeftFunction3.onClick.AddListener(PanelLeftPelvisFourLegged);
            ToggleButtonsInteractable(pelvisFourLegged, false);
            pelvisFunctionsSelected.Add("Four-legged movement");
        }
        else
        {
            panelFunctionsFilled.SetActive(true);
        }
    }

    public void PanelLeftPelvisFourLegged()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "Four-legged movement")
        {
            panelLeftFunction1.interactable = false;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "";
            panelLeftFunction1.onClick.RemoveListener(PanelLeftPelvisFourLegged);
            ToggleButtonsInteractable(pelvisFourLegged, true);

            pelvisFunctionsSelected.Remove("Four-legged movement");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "Four-legged movement")
        {
            panelLeftFunction2.interactable = false;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "";
            panelLeftFunction2.onClick.RemoveListener(PanelLeftPelvisFourLegged);
            ToggleButtonsInteractable(pelvisFourLegged, true);

            pelvisFunctionsSelected.Remove("Four-legged movement");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "Four-legged movement")
        {
            panelLeftFunction3.interactable = false;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "";
            panelLeftFunction3.onClick.RemoveListener(PanelLeftPelvisFourLegged);
            ToggleButtonsInteractable(pelvisFourLegged, true);

            pelvisFunctionsSelected.Remove("Four-legged movement");
        }
    }

    public void PelvisLargeMasses()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction1.interactable = true;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "Supporting large masses";
            panelLeftFunction1.onClick.AddListener(PanelLeftPelvisLargeMasses);
            ToggleButtonsInteractable(pelvisLargeMasses, false);

            pelvisFunctionsSelected.Add("Supporting large masses");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction2.interactable = true;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "Supporting large masses";
            panelLeftFunction2.onClick.AddListener(PanelLeftPelvisLargeMasses);
            ToggleButtonsInteractable(pelvisLargeMasses, false);
            pelvisFunctionsSelected.Add("Supporting large masses");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction3.interactable = true;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "Supporting large masses";
            panelLeftFunction3.onClick.AddListener(PanelLeftPelvisLargeMasses);
            ToggleButtonsInteractable(pelvisLargeMasses, false);
            pelvisFunctionsSelected.Add("Supporting large masses");
        }
        else
        {
            panelFunctionsFilled.SetActive(true);
        }
    }

    public void PanelLeftPelvisLargeMasses()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "Supporting large masses")
        {
            panelLeftFunction1.interactable = false;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "";
            panelLeftFunction1.onClick.RemoveListener(PanelLeftPelvisLargeMasses);
            ToggleButtonsInteractable(pelvisLargeMasses, true);
            pelvisFunctionsSelected.Remove("Supporting large masses");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "Supporting large masses")
        {
            panelLeftFunction2.interactable = false;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "";
            panelLeftFunction2.onClick.RemoveListener(PanelLeftPelvisLargeMasses);
            ToggleButtonsInteractable(pelvisLargeMasses, true);
            pelvisFunctionsSelected.Remove("Supporting large masses");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "Supporting large masses")
        {
            panelLeftFunction3.interactable = false;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "";
            panelLeftFunction3.onClick.RemoveListener(PanelLeftPelvisLargeMasses);
            ToggleButtonsInteractable(pelvisLargeMasses, true);
            pelvisFunctionsSelected.Remove("Supporting large masses");
        }
    }

    public void PelvisOccasionalUpright()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction1.interactable = true;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "Occasional upright movement";
            panelLeftFunction1.onClick.AddListener(PanelLeftPelvisOccasionalUpright);
            ToggleButtonsInteractable(pelvisOccasionalUpright, false);
            pelvisFunctionsSelected.Add("Occasional upright movement");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction2.interactable = true;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "Occasional upright movement";
            panelLeftFunction2.onClick.AddListener(PanelLeftPelvisOccasionalUpright);
            ToggleButtonsInteractable(pelvisOccasionalUpright, false);
            pelvisFunctionsSelected.Add("Occasional upright movement");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction3.interactable = true;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "Occasional upright movement";
            panelLeftFunction3.onClick.AddListener(PanelLeftPelvisOccasionalUpright);
            ToggleButtonsInteractable(pelvisOccasionalUpright, false);
            pelvisFunctionsSelected.Add("Occasional upright movement");
        }
        else
        {
            panelFunctionsFilled.SetActive(true);
        }
    }

    public void PanelLeftPelvisOccasionalUpright()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "Occasional upright movement")
        {
            panelLeftFunction1.interactable = false;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "";
            panelLeftFunction1.onClick.RemoveListener(PanelLeftPelvisOccasionalUpright);
            ToggleButtonsInteractable(pelvisOccasionalUpright, true);
            pelvisFunctionsSelected.Remove("Occasional upright movement");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "Occasional upright movement")
        {
            panelLeftFunction2.interactable = false;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "";
            panelLeftFunction2.onClick.RemoveListener(PanelLeftPelvisOccasionalUpright);
            ToggleButtonsInteractable(pelvisOccasionalUpright, true);
            pelvisFunctionsSelected.Remove("Occasional upright movement");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "Occasional upright movement")
        {
            panelLeftFunction3.interactable = false;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "";
            panelLeftFunction3.onClick.RemoveListener(PanelLeftPelvisOccasionalUpright);
            ToggleButtonsInteractable(pelvisOccasionalUpright, true);
            pelvisFunctionsSelected.Remove("Occasional upright movement");
        }
    }

    public void PelvisHabitatGrasslands()
    {
        if (panelLeftHabitat.GetComponentInChildren<Text>().text == "")
        {
            panelLeftHabitat.interactable = true;
            panelLeftHabitat.GetComponentInChildren<Text>().text = "Grassland terrain";
            panelLeftHabitat.onClick.AddListener(PanelLeftPelvisHabitatGrasslands);
            ToggleButtonsInteractable(pelvisHabitatGrassland, false);
            pelvisHabitatSelected = "Grassland terrain";
        }
        else
        {
            panelHabitatFilled.SetActive(true);
        }
    }

    public void PanelLeftPelvisHabitatGrasslands()
    {
        panelLeftHabitat.interactable = false;
        panelLeftHabitat.GetComponentInChildren<Text>().text = "";
        panelLeftHabitat.onClick.RemoveListener(PanelLeftPelvisHabitatGrasslands);
        ToggleButtonsInteractable(pelvisHabitatGrassland, true);
        pelvisHabitatSelected = "";
    }

    public void PelvisHabitatMountain()
    {
        if (panelLeftHabitat.GetComponentInChildren<Text>().text == "")
        {
            panelLeftHabitat.interactable = true;
            panelLeftHabitat.GetComponentInChildren<Text>().text = "Mountain forest";
            panelLeftHabitat.onClick.AddListener(PanelLeftPelvisHabitatMountain);
            ToggleButtonsInteractable(pelvisHabitatMountain, false);
            pelvisHabitatSelected = "Mountain forest";
        }
        else
        {
            panelHabitatFilled.SetActive(true);
        }
    }

    public void PanelLeftPelvisHabitatMountain()
    {
        panelLeftHabitat.interactable = false;
        panelLeftHabitat.GetComponentInChildren<Text>().text = "";
        panelLeftHabitat.onClick.RemoveListener(PanelLeftPelvisHabitatMountain);
        ToggleButtonsInteractable(pelvisHabitatMountain, true);
        pelvisHabitatSelected = "";
    }

    public void PelvisHabitatTropical()
    {
        if (panelLeftHabitat.GetComponentInChildren<Text>().text == "")
        {
            panelLeftHabitat.interactable = true;
            panelLeftHabitat.GetComponentInChildren<Text>().text = "Tropical forest";
            panelLeftHabitat.onClick.AddListener(PanelLeftPelvisHabitatTropical);
            ToggleButtonsInteractable(pelvisHabitatTropical, false);
            pelvisHabitatSelected = "Tropical forest";
        }
        else
        {
            panelHabitatFilled.SetActive(true);
        }
    }

    public void PanelLeftPelvisHabitatTropical()
    {
        panelLeftHabitat.interactable = false;
        panelLeftHabitat.GetComponentInChildren<Text>().text = "";
        panelLeftHabitat.onClick.RemoveListener(PanelLeftPelvisHabitatTropical);
        ToggleButtonsInteractable(pelvisHabitatTropical, true);
        pelvisHabitatSelected = "";
    }

    public void HandDigging()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction1.interactable = true;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "Digging";
            panelLeftFunction1.onClick.AddListener(PanelLeftHandDigging);
            ToggleButtonsInteractable(handDigging, false);
            handFunctionsSelected.Add("Digging");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction2.interactable = true;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "Digging";
            panelLeftFunction2.onClick.AddListener(PanelLeftHandDigging);
            ToggleButtonsInteractable(handDigging, false);
            handFunctionsSelected.Add("Digging");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction3.interactable = true;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "Digging";
            panelLeftFunction3.onClick.AddListener(PanelLeftHandDigging);
            ToggleButtonsInteractable(handDigging, false);
            handFunctionsSelected.Add("Digging");
        }
        else
        {
            panelFunctionsFilled.SetActive(true);
        }
    }

    public void PanelLeftHandDigging()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "Digging")
        {
            panelLeftFunction1.interactable = false;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "";
            panelLeftFunction1.onClick.RemoveListener(PanelLeftHandDigging);
            ToggleButtonsInteractable(handDigging, true);
            handFunctionsSelected.Remove("Digging");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "Digging")
        {
            panelLeftFunction2.interactable = false;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "";
            panelLeftFunction2.onClick.RemoveListener(PanelLeftHandDigging);
            ToggleButtonsInteractable(handDigging, true);
            handFunctionsSelected.Remove("Digging");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "Digging")
        {
            panelLeftFunction3.interactable = false;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "";
            panelLeftFunction3.onClick.RemoveListener(PanelLeftHandDigging);
            ToggleButtonsInteractable(handDigging, true);
            handFunctionsSelected.Remove("Digging");
        }
    }

    public void HandBreaking()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction1.interactable = true;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "Breaking";
            panelLeftFunction1.onClick.AddListener(PanelLeftHandBreaking);
            ToggleButtonsInteractable(handBreaking, false);
            handFunctionsSelected.Add("Breaking");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction2.interactable = true;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "Breaking";
            panelLeftFunction2.onClick.AddListener(PanelLeftHandBreaking);
            ToggleButtonsInteractable(handBreaking, false);
            handFunctionsSelected.Add("Breaking");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction3.interactable = true;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "Breaking";
            panelLeftFunction3.onClick.AddListener(PanelLeftHandBreaking);
            ToggleButtonsInteractable(handBreaking, false);
            handFunctionsSelected.Add("Breaking");
        }
        else
        {
            panelFunctionsFilled.SetActive(true);
        }
    }

    public void PanelLeftHandBreaking()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "Breaking")
        {
            panelLeftFunction1.interactable = false;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "";
            panelLeftFunction1.onClick.RemoveListener(PanelLeftHandBreaking);
            ToggleButtonsInteractable(handBreaking, true);
            handFunctionsSelected.Remove("Breaking");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "Breaking")
        {
            panelLeftFunction2.interactable = false;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "";
            panelLeftFunction2.onClick.RemoveListener(PanelLeftHandBreaking);
            ToggleButtonsInteractable(handBreaking, true);
            handFunctionsSelected.Remove("Breaking");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "Breaking")
        {
            panelLeftFunction3.interactable = false;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "";
            panelLeftFunction3.onClick.RemoveListener(PanelLeftHandBreaking);
            ToggleButtonsInteractable(handBreaking, true);
            handFunctionsSelected.Remove("Breaking");
        }
    }

    public void HandPiercing()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction1.interactable = true;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "Piercing";
            panelLeftFunction1.onClick.AddListener(PanelLeftHandPiercing);
            ToggleButtonsInteractable(handPiercing, false);
            handFunctionsSelected.Add("Piercing");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction2.interactable = true;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "Piercing";
            panelLeftFunction2.onClick.AddListener(PanelLeftHandPiercing);
            ToggleButtonsInteractable(handPiercing, false);
            handFunctionsSelected.Add("Piercing");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction3.interactable = true;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "Piercing";
            panelLeftFunction3.onClick.AddListener(PanelLeftHandPiercing);
            ToggleButtonsInteractable(handPiercing, false);
            handFunctionsSelected.Add("Piercing");
        }
        else
        {
            panelFunctionsFilled.SetActive(true);
        }
    }

    public void PanelLeftHandPiercing()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "Piercing")
        {
            panelLeftFunction1.interactable = false;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "";
            panelLeftFunction1.onClick.RemoveListener(PanelLeftHandPiercing);
            ToggleButtonsInteractable(handPiercing, true);
            handFunctionsSelected.Remove("Piercing");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "Piercing")
        {
            panelLeftFunction2.interactable = false;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "";
            panelLeftFunction2.onClick.RemoveListener(PanelLeftHandPiercing);
            ToggleButtonsInteractable(handPiercing, true);
            handFunctionsSelected.Remove("Piercing");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "Piercing")
        {
            panelLeftFunction3.interactable = false;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "";
            panelLeftFunction3.onClick.RemoveListener(PanelLeftHandPiercing);
            ToggleButtonsInteractable(handPiercing, true);
            handFunctionsSelected.Remove("Piercing");
        }
    }

    public void HandThrowing()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction1.interactable = true;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "Throwing";
            panelLeftFunction1.onClick.AddListener(PanelLeftHandThrowing);
            ToggleButtonsInteractable(handThrowing, false);
            handFunctionsSelected.Add("Throwing");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction2.interactable = true;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "Throwing";
            panelLeftFunction2.onClick.AddListener(PanelLeftHandThrowing);
            ToggleButtonsInteractable(handThrowing, false);
            handFunctionsSelected.Add("Throwing");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction3.interactable = true;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "Throwing";
            panelLeftFunction3.onClick.AddListener(PanelLeftHandThrowing);
            ToggleButtonsInteractable(handThrowing, false);
            handFunctionsSelected.Add("Throwing");
        }
        else
        {
            panelFunctionsFilled.SetActive(true);
        }
    }

    public void PanelLeftHandThrowing()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "Throwing")
        {
            panelLeftFunction1.interactable = false;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "";
            panelLeftFunction1.onClick.RemoveListener(PanelLeftHandThrowing);
            ToggleButtonsInteractable(handThrowing, true);
            handFunctionsSelected.Remove("Throwing");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "Throwing")
        {
            panelLeftFunction2.interactable = false;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "";
            panelLeftFunction2.onClick.RemoveListener(PanelLeftHandThrowing);
            ToggleButtonsInteractable(handThrowing, true);
            handFunctionsSelected.Remove("Throwing");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "Throwing")
        {
            panelLeftFunction3.interactable = false;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "";
            panelLeftFunction3.onClick.RemoveListener(PanelLeftHandThrowing);
            ToggleButtonsInteractable(handThrowing, true);
            handFunctionsSelected.Remove("Throwing");
        }
    }

    public void HandPrecisionGripping()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction1.interactable = true;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "Precision gripping";
            panelLeftFunction1.onClick.AddListener(PanelLeftHandPrecisionGripping);
            ToggleButtonsInteractable(handPrecisionGripping, false);
            handFunctionsSelected.Add("Precision gripping");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction2.interactable = true;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "Precision gripping";
            panelLeftFunction2.onClick.AddListener(PanelLeftHandPrecisionGripping);
            ToggleButtonsInteractable(handPrecisionGripping, false);
            handFunctionsSelected.Add("Precision gripping");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction3.interactable = true;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "Precision gripping";
            panelLeftFunction3.onClick.AddListener(PanelLeftHandPrecisionGripping);
            ToggleButtonsInteractable(handPrecisionGripping, false);
            handFunctionsSelected.Add("Precision gripping");
        }
        else
        {
            panelFunctionsFilled.SetActive(true);
        }
    }

    public void PanelLeftHandPrecisionGripping()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "Precision gripping")
        {
            panelLeftFunction1.interactable = false;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "";
            panelLeftFunction1.onClick.RemoveListener(PanelLeftHandPrecisionGripping);
            ToggleButtonsInteractable(handPrecisionGripping, true);
            handFunctionsSelected.Remove("Precision gripping");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "Precision gripping")
        {
            panelLeftFunction2.interactable = false;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "";
            panelLeftFunction2.onClick.RemoveListener(PanelLeftHandPrecisionGripping);
            ToggleButtonsInteractable(handPrecisionGripping, true);
            handFunctionsSelected.Remove("Precision gripping");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "Precision gripping")
        {
            panelLeftFunction3.interactable = false;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "";
            panelLeftFunction3.onClick.RemoveListener(PanelLeftHandPrecisionGripping);
            ToggleButtonsInteractable(handPrecisionGripping, true);
            handFunctionsSelected.Remove("Precision gripping");
        }
    }

    public void HandReaching()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction1.interactable = true;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "Reaching";
            panelLeftFunction1.onClick.AddListener(PanelLeftHandReaching);
            ToggleButtonsInteractable(handReaching, false);
            handFunctionsSelected.Add("Reaching");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction2.interactable = true;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "Reaching";
            panelLeftFunction2.onClick.AddListener(PanelLeftHandReaching);
            ToggleButtonsInteractable(handReaching, false);
            handFunctionsSelected.Add("Reaching");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction3.interactable = true;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "Reaching";
            panelLeftFunction3.onClick.AddListener(PanelLeftHandReaching);
            ToggleButtonsInteractable(handReaching, false);
            handFunctionsSelected.Add("Reaching");
        }
        else
        {
            panelFunctionsFilled.SetActive(true);
        }
    }

    public void PanelLeftHandReaching()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "Reaching")
        {
            panelLeftFunction1.interactable = false;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "";
            panelLeftFunction1.onClick.RemoveListener(PanelLeftHandReaching);
            ToggleButtonsInteractable(handReaching, true);
            handFunctionsSelected.Remove("Reaching");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "Reaching")
        {
            panelLeftFunction2.interactable = false;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "";
            panelLeftFunction2.onClick.RemoveListener(PanelLeftHandReaching);
            ToggleButtonsInteractable(handReaching, true);
            handFunctionsSelected.Remove("Reaching");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "Reaching")
        {
            panelLeftFunction3.interactable = false;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "";
            panelLeftFunction3.onClick.RemoveListener(PanelLeftHandReaching);
            ToggleButtonsInteractable(handReaching, true);
            handFunctionsSelected.Remove("Reaching");
        }
    }

    public void HandGrabbing()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction1.interactable = true;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "Grabbing";
            panelLeftFunction1.onClick.AddListener(PanelLeftHandGrabbing);
            ToggleButtonsInteractable(handGrabbing, false);
            handFunctionsSelected.Add("Grabbing");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction2.interactable = true;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "Grabbing";
            panelLeftFunction2.onClick.AddListener(PanelLeftHandGrabbing);
            ToggleButtonsInteractable(handGrabbing, false);
            handFunctionsSelected.Add("Grabbing");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction3.interactable = true;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "Grabbing";
            panelLeftFunction3.onClick.AddListener(PanelLeftHandGrabbing);
            ToggleButtonsInteractable(handGrabbing, false);
            handFunctionsSelected.Add("Grabbing");
        }
        else
        {
            panelFunctionsFilled.SetActive(true);
        }
    }

    public void PanelLeftHandGrabbing()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "Grabbing")
        {
            panelLeftFunction1.interactable = false;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "";
            panelLeftFunction1.onClick.RemoveListener(PanelLeftHandGrabbing);
            ToggleButtonsInteractable(handGrabbing, true);
            handFunctionsSelected.Remove("Grabbing");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "Grabbing")
        {
            panelLeftFunction2.interactable = false;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "";
            panelLeftFunction2.onClick.RemoveListener(PanelLeftHandGrabbing);
            ToggleButtonsInteractable(handGrabbing, true);
            handFunctionsSelected.Remove("Grabbing");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "Grabbing")
        {
            panelLeftFunction3.interactable = false;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "";
            panelLeftFunction3.onClick.RemoveListener(PanelLeftHandGrabbing);
            ToggleButtonsInteractable(handGrabbing, true);
            handFunctionsSelected.Remove("Grabbing");
        }
    }

    public void HandClawing()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction1.interactable = true;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "Clawing";
            panelLeftFunction1.onClick.AddListener(PanelLeftHandClawing);
            ToggleButtonsInteractable(handClawing, false);
            handFunctionsSelected.Add("Clawing");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction2.interactable = true;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "Clawing";
            panelLeftFunction2.onClick.AddListener(PanelLeftHandClawing);
            ToggleButtonsInteractable(handClawing, false);
            handFunctionsSelected.Add("Clawing");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction3.interactable = true;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "Clawing";
            panelLeftFunction3.onClick.AddListener(PanelLeftHandClawing);
            ToggleButtonsInteractable(handClawing, false);
            handFunctionsSelected.Add("Clawing");
        }
        else
        {
            panelFunctionsFilled.SetActive(true);
        }
    }

    public void PanelLeftHandClawing()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "Clawing")
        {
            panelLeftFunction1.interactable = false;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "";
            panelLeftFunction1.onClick.RemoveListener(PanelLeftHandClawing);
            ToggleButtonsInteractable(handClawing, true);
            handFunctionsSelected.Remove("Clawing");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "Clawing")
        {
            panelLeftFunction2.interactable = false;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "";
            panelLeftFunction2.onClick.RemoveListener(PanelLeftHandClawing);
            ToggleButtonsInteractable(handClawing, true);
            handFunctionsSelected.Remove("Clawing");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "Clawing")
        {
            panelLeftFunction3.interactable = false;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "";
            panelLeftFunction3.onClick.RemoveListener(PanelLeftHandClawing);
            ToggleButtonsInteractable(handClawing, true);
            handFunctionsSelected.Remove("Clawing");
        }
    }

    public void HandTearing()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction1.interactable = true;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "Tearing";
            panelLeftFunction1.onClick.AddListener(PanelLeftHandTearing);
            ToggleButtonsInteractable(handTearing, false);
            handFunctionsSelected.Add("Tearing");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction2.interactable = true;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "Tearing";
            panelLeftFunction2.onClick.AddListener(PanelLeftHandTearing);
            ToggleButtonsInteractable(handTearing, false);
            handFunctionsSelected.Add("Tearing");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "")
        {
            panelLeftFunction3.interactable = true;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "Tearing";
            panelLeftFunction3.onClick.AddListener(PanelLeftHandTearing);
            ToggleButtonsInteractable(handTearing, false);
            handFunctionsSelected.Add("Tearing");
        }
        else
        {
            panelFunctionsFilled.SetActive(true);
        }
    }

    public void PanelLeftHandTearing()
    {
        if (panelLeftFunction1.GetComponentInChildren<Text>().text == "Tearing")
        {
            panelLeftFunction1.interactable = false;
            panelLeftFunction1.GetComponentInChildren<Text>().text = "";
            panelLeftFunction1.onClick.RemoveListener(PanelLeftHandTearing);
            ToggleButtonsInteractable(handTearing, true);
            handFunctionsSelected.Remove("Tearing");
        }
        else if (panelLeftFunction2.GetComponentInChildren<Text>().text == "Tearing")
        {
            panelLeftFunction2.interactable = false;
            panelLeftFunction2.GetComponentInChildren<Text>().text = "";
            panelLeftFunction2.onClick.RemoveListener(PanelLeftHandTearing);
            ToggleButtonsInteractable(handTearing, true);
            handFunctionsSelected.Remove("Tearing");
        }
        else if (panelLeftFunction3.GetComponentInChildren<Text>().text == "Tearing")
        {
            panelLeftFunction3.interactable = false;
            panelLeftFunction3.GetComponentInChildren<Text>().text = "";
            panelLeftFunction3.onClick.RemoveListener(PanelLeftHandTearing);
            ToggleButtonsInteractable(handTearing, true);
            handFunctionsSelected.Remove("Tearing");
        }
    }

    public void HandHabitatGrasslands()
    {
        if (panelLeftHabitat.GetComponentInChildren<Text>().text == "")
        {
            panelLeftHabitat.interactable = true;
            panelLeftHabitat.GetComponentInChildren<Text>().text = "Grassland terrain";
            panelLeftHabitat.onClick.AddListener(PanelLeftHandHabitatGrasslands);
            ToggleButtonsInteractable(handHabitatGrassland, false);
            handHabitatSelected = "Grassland terrain";
        }
        else
        {
            panelHabitatFilled.SetActive(true);
        }
    }

    public void PanelLeftHandHabitatGrasslands()
    {
        panelLeftHabitat.interactable = false;
        panelLeftHabitat.GetComponentInChildren<Text>().text = "";
        panelLeftHabitat.onClick.RemoveListener(PanelLeftHandHabitatGrasslands);
        ToggleButtonsInteractable(handHabitatGrassland, true);
        handHabitatSelected = "";
    }

    public void HandHabitatUnderground()
    {
        if (panelLeftHabitat.GetComponentInChildren<Text>().text == "")
        {
            panelLeftHabitat.interactable = true;
            panelLeftHabitat.GetComponentInChildren<Text>().text = "Underground tunnels";
            panelLeftHabitat.onClick.AddListener(PanelLeftHandHabitatUnderground);
            ToggleButtonsInteractable(handHabitatUnderground, false);
            handHabitatSelected = "Underground tunnels";
        }
        else
        {
            panelHabitatFilled.SetActive(true);
        }
    }

    public void PanelLeftHandHabitatUnderground()
    {
        panelLeftHabitat.interactable = false;
        panelLeftHabitat.GetComponentInChildren<Text>().text = "";
        panelLeftHabitat.onClick.RemoveListener(PanelLeftHandHabitatUnderground);
        ToggleButtonsInteractable(handHabitatUnderground, true);
        handHabitatSelected = "";
    }

    public void HandHabitatTropical()
    {
        if (panelLeftHabitat.GetComponentInChildren<Text>().text == "")
        {
            panelLeftHabitat.interactable = true;
            panelLeftHabitat.GetComponentInChildren<Text>().text = "Tropical forest";
            panelLeftHabitat.onClick.AddListener(PanelLeftHandHabitatTropical);
            ToggleButtonsInteractable(handHabitatTropical, false);
            handHabitatSelected = "Tropical forest";
        }
        else
        {
            panelHabitatFilled.SetActive(true);
        }
    }

    public void PanelLeftHandHabitatTropical()
    {
        panelLeftHabitat.interactable = false;
        panelLeftHabitat.GetComponentInChildren<Text>().text = "";
        panelLeftHabitat.onClick.RemoveListener(PanelLeftHandHabitatTropical);
        ToggleButtonsInteractable(handHabitatTropical, true);
        handHabitatSelected = "";
    }

    private void ToggleButtonsInteractable(Button[] buttons, bool state)
    {
        foreach (Button b in buttons)
        {
            b.interactable = state;
        }
    }

    public void VerifySelections()
    {
        if (pelvis && !hand)
        {
            foreach (string selection in pelvisFunctionsSelected)
            {
                foreach (string answer in pelvisFunctions)
                {
                    if (answer == selection)
                    {
                        matches++;
                    }
                }
            }
            if (pelvisHabitat == pelvisHabitatSelected)
            {
                matches++;
            }
        }
        else if (!pelvis && hand)
        {
            foreach (string selection in handFunctionsSelected)
            {
                foreach (string answer in handFunctions)
                {
                    if (answer == selection)
                    {
                        matches++;
                    }
                }
            }
            if (handHabitat == handHabitatSelected)
            {
                matches++;
            }
        }
        panelVerificationScore.SetActive(true);
        score.text = matches.ToString() + "/4 options verified.";
        if (matches == 4)
        {
            dataNumber.SetActive(true);
            dataNumber.GetComponent<Text>().text = "Data #: CMNHBCD0" + System.DateTime.Now.Month.ToString() + System.DateTime.Now.Day.ToString() + "19_" + dataCounter.ToString("00");
            _oscOut.Send(addressBDC, dataCounter);
            dataCounter++;
            
        }
        
    }

    public void CloseOverlay()
    {
        panelFunctionsFilled.SetActive(false);
        panelHabitatFilled.SetActive(false);
        panelVerificationScore.SetActive(false);
        dataNumber.SetActive(false);
        matches = 0;
    }

    public void CloseOverlayVerify()
    {
        panelFunctionsFilled.SetActive(false);
        panelHabitatFilled.SetActive(false);
        panelVerificationScore.SetActive(false);
        dataNumber.SetActive(false);
        if (matches == 4)
            ResetButton();
        matches = 0;
    }
}
