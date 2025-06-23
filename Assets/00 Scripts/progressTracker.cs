using System.Collections;
using Unity.VisualScripting;

//using Tripolygon.UModeler.UI.Controls;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class progessTracker : MonoBehaviour
{
    // Current state of the lab progress
    public enum LabState
    {
        safetyCheck, 
        Step1, 
        Step2, 
        Step3, 
        Step4, 
        Step5, 
        Step6, 
        Step7, 
        Step8,
        Step9, 
        Step10,
        Finished,
        meltingPoint
    }
    public LabState currentState;
    public Button nextButton;
    private bool nextButtonClicked = false; 
    public GameObject popUpPanel;
    public TextMeshProUGUI content;
    public GameObject player;
    public GameObject scrollContent;
    public GameObject step1Erlenmeyer; 
    public GameObject Pause;
    public GameObject InGame;
    public CompletionScreen completionScreen;
    public float massOfAluminumUsed;
    public float mp;
    public bool foundMP = false;
    public multihandler theMULTIHANDLER;
    public GameObject giveWalterSolutionText;
    public GameObject teacher;

    // Initialize the first state
    private void Start()
    {
        GameObject Canvases = GameObject.Find("Canvases");
        completionScreen = GameObject.Find("Completion Canvas").GetComponentInChildren<CompletionScreen>();
        InGame = Canvases.transform.Find("In Game Canvas").gameObject;
        popUpPanel = InGame.transform.Find("Pop Up Panel").gameObject;
        nextButton = popUpPanel.transform.Find("Next Button").GetComponent<Button>();
        nextButton.onClick.AddListener(nextButtonClick);
        content = popUpPanel.transform.Find("content").GetComponent<TextMeshProUGUI>();
        player = GameObject.Find("Player");
        Pause = Canvases.transform.Find("Pause Canvas").gameObject;
        GameObject Notebook = Pause.transform.Find("Notebook").gameObject;
        GameObject Scroll = Notebook.transform.Find("Scroll View").gameObject;
        GameObject View = Scroll.transform.Find("Viewport").gameObject;
        scrollContent = View.transform.Find("Content").gameObject;
        theMULTIHANDLER = GameObject.FindGameObjectWithTag("GameController").GetComponent<multihandler>();

        currentState = LabState.safetyCheck;
        DisplayCurrentState();
        StartCoroutine(Intro());


    }

    // Transition to the next state
    public void TransitionToNextState()
    {
        switch (currentState)
        {
            case LabState.safetyCheck:
                GameObject title = scrollContent.transform.Find("Safety Check").gameObject;
                GameObject check = title.transform.Find("Check").gameObject;
                check.SetActive(true);
                StartCoroutine(Step1());
                currentState = LabState.Step1;
                break;

            case LabState.Step1:
                GameObject title1 = scrollContent.transform.Find("Step 1 Title").gameObject;
                GameObject check1 = title1.transform.Find("Check").gameObject;
                check1.SetActive(true);
                StartCoroutine(Step2());
                currentState = LabState.Step2;
                break;

            case LabState.Step2:
                GameObject title2 = scrollContent.transform.Find("Step 2 Title").gameObject;
                GameObject check2 = title2.transform.Find("Check").gameObject;
                check2.SetActive(true);
                StartCoroutine(Step3());
                currentState = LabState.Step3;
                break;

            case LabState.Step3:
                GameObject title3 = scrollContent.transform.Find("Step 3 Title").gameObject;
                GameObject check3 = title3.transform.Find("Check").gameObject;
                check3.SetActive(true);
                StartCoroutine(Step4());
                currentState = LabState.Step4;
                break;

            case LabState.Step4:
                GameObject title4 = scrollContent.transform.Find("Step 4 Title").gameObject;
                GameObject check4 = title4.transform.Find("Check").gameObject;
                check4.SetActive(true);
                StartCoroutine(Step5());
                currentState = LabState.Step5;
                break;

            case LabState.Step5:
                GameObject title5 = scrollContent.transform.Find("Step 5 Title").gameObject;
                GameObject check5 = title5.transform.Find("Check").gameObject;
                check5.SetActive(true);
                StartCoroutine(Step6());
                currentState = LabState.Step6;
                break;

            case LabState.Step6:
                GameObject title6 = scrollContent.transform.Find("Step 6 Title").gameObject;
                GameObject check6 = title6.transform.Find("Check").gameObject;
                GameObject title7 = scrollContent.transform.Find("Step 7 Title").gameObject;
                GameObject check7 = title7.transform.Find("Check").gameObject;
                check6.SetActive(true);
                if (step1Erlenmeyer.GetComponent<liquidScript>().liquidPercent > 0.95f){
                    check7.SetActive(true);
                    Debug.Log("Skipping step 7");
                    StartCoroutine(Step8());
                    currentState = LabState.Step8;
                }
                else{
                    StartCoroutine(Step7());
                    currentState = LabState.Step7;
                }
                break;

            case LabState.Step7:
                title7 = scrollContent.transform.Find("Step 7 Title").gameObject;
                check7 = title7.transform.Find("Check").gameObject;
                check7.SetActive(true);
                StartCoroutine(Step8());
                currentState = LabState.Step8;
                break;

            case LabState.Step8:
                GameObject title8 = scrollContent.transform.Find("Step 8 Title").gameObject;
                GameObject check8 = title8.transform.Find("Check").gameObject;
                check8.SetActive(true);
                StartCoroutine(Step9());
                currentState = LabState.Step9;
                break;

            case LabState.Step9:
                GameObject title9 = scrollContent.transform.Find("Step 9 Title").gameObject;
                GameObject check9 = title9.transform.Find("Check").gameObject;
                check9.SetActive(true);
                StartCoroutine(Step10());
                currentState = LabState.Step10;
                break;

            case LabState.Step10:
                GameObject title10 = scrollContent.transform.Find("Step 10 Title").gameObject;
                GameObject check10 = title10.transform.Find("Check").gameObject;
                check10.SetActive(true);
                StartCoroutine(Finished());
                currentState = LabState.Finished;
                break;

            case LabState.Finished:
                Debug.Log("Lab is complete!");
                StartCoroutine(foundMeltingPoint());
                currentState = LabState.meltingPoint;
                break; // Do not transition further
            
            case LabState.meltingPoint:
                return;
        }

        DisplayCurrentState();
    }

    // Display the current state in the Unity Console
    private void DisplayCurrentState()
    {
        Debug.Log($"Current State: {currentState}");
        // You can also update UI elements here based on the state, such as changing text or images
    }

    void Update()
    {
        PerformStateActions();
        if (Pause.activeInHierarchy || popUpPanel.activeInHierarchy){
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else{
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // You could implement additional methods to perform actions in each state
    private void PerformStateActions()
    {
        switch (currentState)
        {
            //check if safety goggles have been donned
            case LabState.safetyCheck:
                if (player.GetComponent<interactWithObjects>().gogglesOn){
                    TransitionToNextState();
                }
                break;

            //check if 1 g of aluminum has been placed alone in a 250 mL Erlenmeyer
            case LabState.Step1:
                GameObject[] liquidHolders = GameObject.FindGameObjectsWithTag("LiquidHolder");

                foreach (GameObject obj in liquidHolders)
                {
                    if (obj.transform.name.StartsWith("Erlenmeyer Flask 250")){
                        if (obj.GetComponent<liquidScript>().currentVolume_mL < 0.375f && obj.GetComponent<liquidScript>().currentVolume_mL > 0.365f && obj.GetComponent<liquidScript>().percentAl >= 0.95f){
                            step1Erlenmeyer = obj;
                            TransitionToNextState();
                        }
                    }
                }
                break;

            case LabState.Step2:
                GameObject[] liquidHolders1 = GameObject.FindGameObjectsWithTag("LiquidHolder");

                foreach (GameObject obj in liquidHolders1)
                {
                    if (obj.transform.name.StartsWith("Graduated")){
                        if(obj.GetComponent<liquidScript>().currentVolume_mL > 24f && obj.GetComponent<liquidScript>().currentVolume_mL < 26f && obj.GetComponent<liquidScript>().percentKOH > 0.18f && obj.GetComponent<liquidScript>().percentKOH < 0.22f){
                            step1Erlenmeyer = obj;
                            TransitionToNextState();
                        }
                    }
                }
                
                break;

            case LabState.Step3:
                GameObject[] liquidHolders2 = GameObject.FindGameObjectsWithTag("LiquidHolder");
                if (Input.GetKeyDown(KeyCode.C))
                    TransitionToNextState();

                foreach (GameObject obj in liquidHolders2)
                {
                    if (obj.transform.name.StartsWith("Erlenmeyer Flask 250")){
                        if (obj.GetComponent<liquidScript>().liquidTemperature > 343.15f && obj.GetComponent<liquidScript>().currentVolume_mL > 20f && obj.GetComponent<liquidScript>().percentKAlOH4 > 0.3f){
                            step1Erlenmeyer = obj;
                            TransitionToNextState();
                        }
                    }
                }
                break;

            case LabState.Step4:
                GameObject[] liquidHolders3 = GameObject.FindGameObjectsWithTag("LiquidHolder");
                if (Input.GetKeyDown(KeyCode.C))
                    TransitionToNextState();

                foreach (GameObject obj in liquidHolders3)
                {
                    if (obj.transform.name.StartsWith("Erlenmeyer Flask")){
                        if (obj.GetComponent<liquidScript>().liquidTemperature <= 295.15f && obj.GetComponent<liquidScript>().currentVolume_mL > 15f && obj.GetComponent<liquidScript>().percentKAlOH4 > 0.43f && obj.GetComponent<liquidScript>().percentAl <= 0.01f){
                            step1Erlenmeyer = obj;
                            TransitionToNextState();
                        }
                    }
                }
                break;

            case LabState.Step5:
                if (Input.GetKeyDown(KeyCode.C))
                        TransitionToNextState();
                GameObject[] liquidHolders4 = GameObject.FindGameObjectsWithTag("LiquidHolder");

                foreach (GameObject obj in liquidHolders4)
                {
                    if (obj.GetComponent<liquidScript>().currentVolume_mL > 35f && obj.GetComponent<liquidScript>().percentKAlSO42 > 0.15f){ //&& player.GetComponent<doCertainThingWith>().beginStirring add this when stirring is good to go
                        step1Erlenmeyer = obj;
                        TransitionToNextState();
                    }
                }
                break;

            case LabState.Step6:
                if (Input.GetKeyDown(KeyCode.C))
                        TransitionToNextState();
                GameObject[] liquidHolders5 = GameObject.FindGameObjectsWithTag("LiquidHolder");

                foreach (GameObject obj in liquidHolders5)
                {
                    if (obj.GetComponent<liquidScript>().liquidTemperature > 330f && obj.GetComponent<liquidScript>().currentVolume_mL > 4f && obj.GetComponent<liquidScript>().percentKAlSO42 > 0.15f){ 
                        step1Erlenmeyer = obj;
                        TransitionToNextState();
                    }
                }
                break;

            case LabState.Step7:
                if (Input.GetKeyDown(KeyCode.C))
                        TransitionToNextState();
                GameObject[] liquidHolders6 = GameObject.FindGameObjectsWithTag("LiquidHolder");

                foreach (GameObject obj in liquidHolders6)
                {
                    if (obj.GetComponent<liquidScript>().liquidPercent > 0.95f && obj.GetComponent<liquidScript>().currentVolume_mL > 20f && obj.GetComponent<liquidScript>().percentKAlSO42 > 0.3f){ 
                        step1Erlenmeyer = obj;
                        TransitionToNextState();
                    }
                }
                break;

            case LabState.Step8:
                if (Input.GetKeyDown(KeyCode.C))
                        TransitionToNextState();
                GameObject[] liquidHolders7 = GameObject.FindGameObjectsWithTag("LiquidHolder");

                foreach (GameObject obj in liquidHolders7)
                {
                    if (obj.GetComponent<liquidScript>().currentVolume_mL > 20f && obj.GetComponent<liquidScript>().percentAlum > 0.3f){ 
                        step1Erlenmeyer = obj;
                        TransitionToNextState();
                    }
                }
                break;

            case LabState.Step9:
                if (Input.GetKeyDown(KeyCode.C))
                        TransitionToNextState();
                GameObject[] liquidHolders8 = GameObject.FindGameObjectsWithTag("LiquidHolder");

                foreach (GameObject obj in liquidHolders8)
                {
                    if (obj.transform.name == "Paper Cone" && obj.GetComponent<liquidScript>().percentAlum > 0.9f){ 
                        step1Erlenmeyer = obj;
                        TransitionToNextState();
                    }
                }
                break;

            case LabState.Step10:
                if (Input.GetKeyDown(KeyCode.C))
                        TransitionToNextState();
                // GameObject[] liquidHolders9 = GameObject.FindGameObjectsWithTag("LiquidHolder");
                bool playerHoldingLiquidCont = player.GetComponent<pickUpObjects>().other && player.GetComponent<pickUpObjects>().other.tag == "LiquidHolder";

                giveWalterSolutionText.SetActive(Vector3.Distance(player.transform.position, teacher.transform.position) < 10f && playerHoldingLiquidCont);

                if (Vector3.Distance(player.transform.position, teacher.transform.position) < 10f && playerHoldingLiquidCont){
                    
                    if (Input.GetKeyDown(KeyCode.H)){
                        print("Check solution");
                        TransitionToNextState();
                        giveWalterSolutionText.SetActive(false);
                    }
                }

                break;

            case LabState.Finished:
                if (Input.GetKeyDown(KeyCode.C))
                    TransitionToNextState();
                // Mark the lab as completed, maybe show results or feedback
                Debug.Log("checking melting point.");
                if (player.GetComponent<doCertainThingWith>().meltingPointToolPlaced == true){
                    Debug.Log("melting point is assembleddd");
                    GameObject mpBeaker = player.GetComponent<doCertainThingWith>().meltingPointBeaker;
                    GameObject mpTool = mpBeaker.transform.Find("Melting Point Tool").gameObject;
                    GameObject capTube = mpTool.transform.Find("Capilary tube Prefab").gameObject;
                    GameObject realCapTube = capTube.transform.Find("Capilary tube").gameObject;
                    Debug.Log("Melting Point Beaker" + realCapTube.GetComponent<liquidScript>().GetMeltingPoint());
                    if (mpBeaker.GetComponent<liquidScript>().liquidTemperature >= realCapTube.GetComponent<liquidScript>().GetMeltingPoint()){
                        TransitionToNextState();
                        if (!foundMP){
                            mp = mpBeaker.GetComponent<liquidScript>().liquidTemperature;
                            foundMP = true;
                        }
                    }
                }
                break;
        }
    }

    IEnumerator waitBeforeTransitioning(){
        yield return new WaitForSeconds(5f);
        TransitionToNextState();
    }

    IEnumerator Intro(){
        // give them a couple of seconds to view the lab
        yield return new WaitForSeconds(1.8f);
        popUpPanel.SetActive(true);
        GetComponent<multihandler>().ToggleCursor();
        content.text = "Hello There! My name is Walter. Welcome to the Synthesis of Alum Lab! If you have any questions, press T on your keyboard and I would be happy to assist you.";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "You can use left click to pick up or drop objects or right click to use the objects in your hand. Most of the equipment and glassware you need can be found in the drawers in the desks.";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "Before starting any lab, make sure that you put on your safety goggles! You can find them hanging on the wall in the lab.";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        popUpPanel.SetActive(false);
        GetComponent<multihandler>().ToggleCursor();
    }

    IEnumerator Step1(){
        yield return new WaitForSeconds(1f);
        popUpPanel.SetActive(true);
        GetComponent<multihandler>().ToggleCursor();
        content.text = "Congratulations! Your eyes are now protected from hazardous chemicals. Now to begin the experiment. Weigh out about 1 g of aluminum metal onto a weigh boat by placing the weigh boat on the scale and clicking the tare button to set the scale to zero. Then grab the scoopula and right click near the jar of aluminum pellets and pour the aluminum from the weigh boat into the 250 mL Erlenmeyer flask. The flasks can be found in the drawers of the desks. ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        popUpPanel.SetActive(false);
        theMULTIHANDLER.PauseOrUnpause();
        GetComponent<multihandler>().ToggleCursor();
    }

    IEnumerator Step2(){
        yield return new WaitForSeconds(1f);
        popUpPanel.SetActive(true);
        GetComponent<multihandler>().ToggleCursor();
        float aluminumVol = step1Erlenmeyer.GetComponent<liquidScript>().currentVolume_mL;
        float aluminumGrams = aluminumVol * 2.7f;
        massOfAluminumUsed = aluminumGrams;
        content.text = "Nice Work! You have measured out " + aluminumGrams + " grams of aluminum into the 250 mL Erlenmeyer Flask. ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "CAREFULLY add 25 mL of 20% potassium hydroxide (KOH) to the graduated cylinder. This can be found in the hood next to the shower. If you pick it up, you can hold right click to inspect and view its contents. You can pour from this beaker by holding 'P'. ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "While you are holding the graduated cylinder, you can hold right click to inspect to see the exact volume of its contents. Once you get close to the right amount, you can use a pipette to measure out the exact amount that you need. ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        popUpPanel.SetActive(false);
        GetComponent<multihandler>().ToggleCursor();
    }

    IEnumerator Step3()
    {
        yield return new WaitForSeconds(1f);
        popUpPanel.SetActive(true);
        GetComponent<multihandler>().ToggleCursor();
        Debug.Log(step1Erlenmeyer.transform.name);
        float KOHvol = step1Erlenmeyer.GetComponent<liquidScript>().currentVolume_mL;
        content.text = "Awesome! You have measured out " + KOHvol + " mL of potassium hydroxide (KOH). ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "Take the KOH in the graduated cylinder and pour it into the 250 mL flask with the Aluminum that you collected in step 1. Then, set up the iron stand which can be found in the left cabinet at every desk. Grab the iron ring and iron mesh from the drawer and snap them into place on the iron mesh for the flask to be snapped on top of.";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "Then manipulate the ceiling vent to get it into position over the iron stand and flip the handle to turn it on. Place the bunsen burner underneath the flask, turn on the gas and light the match. Wait for the solution to heat up and react to an acceptable level.";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        popUpPanel.SetActive(false);
        GetComponent<multihandler>().ToggleCursor();
    }

    IEnumerator Step4(){
        yield return new WaitForSeconds(1f);
        popUpPanel.SetActive(true);
        GetComponent<multihandler>().ToggleCursor();
        content.text = "Great! The reaction has proceded as expected. Now it's time to filter out the remaining solid waste. When you are trying to filter out solids to get a liquid product, gravity filtering is the prefered method. ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "The flask is very hot, so make sure you use tongs when removing it from the iron stand and allow it to cool until the flask is no longer red. While you are waiting, you can assemble the gravity filter apparatus. This consists of a 250 mL Erlenmeyer flask, a glass funnel and a paper filter cone. They should snap into place. ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "Slowly pour the solution into the filter apparatus and watch as the liquid filters to the bottom and the solid remains at the top. Remove the paper cone and glass funnel when you are finished.";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        popUpPanel.SetActive(false);
        GetComponent<multihandler>().ToggleCursor();
    }

    IEnumerator Step5(){
        yield return new WaitForSeconds(1f);
        popUpPanel.SetActive(true);
        GetComponent<multihandler>().ToggleCursor();
        content.text = "Good work! The filtering looks like it went well. Now, measure out 30 mL of sulfuric acid or H<sub>2</sub>SO<sub>4</sub> into a graduated cylinder as you did in step 2 and add this to an empty beaker. Add your filtered solution to this same beaker. Then, you can stir the solution to break down solids and drive it faster. ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        popUpPanel.SetActive(false);
        GetComponent<multihandler>().ToggleCursor();
    }

    IEnumerator Step6(){
        yield return new WaitForSeconds(1f);
        popUpPanel.SetActive(true);
        GetComponent<multihandler>().ToggleCursor();
        content.text = "Nice Job! The reaction proceeded as expected. As a precaution, gently heat the reaction to ensure that the Aluminum Hydroxide is dissolved.";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        popUpPanel.SetActive(false);
        GetComponent<multihandler>().ToggleCursor();
    }

    IEnumerator Step7(){
        yield return new WaitForSeconds(1f);
        popUpPanel.SetActive(true);
        GetComponent<multihandler>().ToggleCursor();
        content.text = "You're really in your element! It looks like there may be some solid impurities in your solution. Use the gravity filter again with a clean peice of filter paper to remove these impurities.";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        popUpPanel.SetActive(false);
        GetComponent<multihandler>().ToggleCursor();
    }

    IEnumerator Step8(){
        yield return new WaitForSeconds(1f);
        popUpPanel.SetActive(true);
        GetComponent<multihandler>().ToggleCursor();
        content.text = "It looks like you're ready to start crystalization. For this step, place the beaker containing your solution into the ice bath. This will allow the solution to cool and begin to crystalize. ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        popUpPanel.SetActive(false);
        GetComponent<multihandler>().ToggleCursor();
    }

    IEnumerator Step9(){
        yield return new WaitForSeconds(1f);
        popUpPanel.SetActive(true);
        GetComponent<multihandler>().ToggleCursor();
        content.text = "Congratulations! Crystalization is complete. Now it is time to use the Buchner funnel to isolate the crystals. First, assemble the Buchner Funnel in the same way that you assembled the glass funnel. Then, right click to attach the hose to the correct position on the sink.";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "Turn on the sink to draw air out of the Buchner flask and create a vacuum. This vacuum will draw any fluids out of the paper filter cone into the flask, essentially drying the crystals. Pour ethanol over the solution in the paper cone as you filter it to wash the crystals and draw out impurities.";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        popUpPanel.SetActive(false);
        GetComponent<multihandler>().ToggleCursor();
    }

    IEnumerator Step10(){
        yield return new WaitForSeconds(1f);
        popUpPanel.SetActive(true);
        GetComponent<multihandler>().ToggleCursor();
        content.text = "Good Job! Tare out a beaker on the scale and then pour the contents from the paper cone into the tared beaker. This is the mass of your final product. Then, take the solution to Walter and press 'H' to turn it in and have him analyze it. ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        popUpPanel.SetActive(false);
        GetComponent<multihandler>().ToggleCursor();
    }

    IEnumerator Finished(){
        yield return new WaitForSeconds(1f);
        popUpPanel.SetActive(true);
        GetComponent<multihandler>().ToggleCursor();
        GameObject finalBeaker = player.GetComponent<pickUpObjects>().other;
        float highestProductAmount = finalBeaker.GetComponent<liquidScript>().currentVolume_mL * finalBeaker.GetComponent<liquidScript>().percentAlum;
        float AlMols = 1 / 26.98f;
        float massAlumTheoretical = 40f;
        float percentYield = highestProductAmount / massAlumTheoretical * 100f;
        completionScreen.percentYield = percentYield;
        Debug.Log(massAlumTheoretical);
        content.text = "You did it! You have succesfully synthesized Alum! Your percent yield is " + percentYield + "% !";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "Now it is time to find your melting point. Find the capillary tube and right click near your alum product to collect a sample. Then, find the thermometer and rubber band and right click to assemble the melting point apparatus. From here, you can right click near a beaker of water to insert the melting point apparatus into the water. ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        content.text = "Take the beaker and heat it gently with the bunsen burner. Keep a close eye on the panel above it and watch as the temperature increaces. When the sample in the capilary tube melts and turns into a liquid, this will be reflected here. When this happens, note the temperature and record it. This is your melting point. This number is used to help you determine the purity of your product. ";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        popUpPanel.SetActive(false);
        GetComponent<multihandler>().ToggleCursor();
    }

    IEnumerator foundMeltingPoint(){
        yield return new WaitForSeconds(1f);
        popUpPanel.SetActive(true);
        GetComponent<multihandler>().ToggleCursor();
        float mpc = mp - 273.15f;
        completionScreen.meltingPoint = mpc;
        content.text = "Congrats! You found your melting point! It is " + mpc + " Celcius. That's a pretty pure product!";
        while (!nextButtonClicked){
            yield return null;
        }
        nextButtonClicked = false;
        popUpPanel.SetActive(false);
        completionScreen.ShowCompletionScreen();
        GetComponent<multihandler>().ToggleCursor();
    }

    private void nextButtonClick()
    {
        // Set the flag to true when the button is clicked
        nextButtonClicked = true;
    }
}