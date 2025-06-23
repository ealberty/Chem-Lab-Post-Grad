using System;
using System.Collections;
using System.Collections.Generic;
using Obi;
using TMPro;
using Tripolygon.UModelerX.Runtime;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample;
using Unity.Netcode;
// using Unity.Services.Multiplay.Authoring.Core.MultiplayApi;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;

public class doCertainThingWith : MonoBehaviour
{
    const float TONG_GRAB_DISTANCE = 3f;
    const float PIPETTE_GRAB_DISTANCE = 0.55f;
    const float IRON_RING_SNAP_DISTANCE = 0.65f;
    const float IRON_MESH_SNAP_DISTANCE = 1.2f;
    const float SCOOPULA_GRAB_DISTANCE = 1.2f;
    const float FUNNEL_INSERT_DISTANCE = 1.5f;
    const float CAPILLARY_ASSEMBLY_DISTANCE = 2f;
    const float ALUMINUM_DROPOFF_RANGE = 0.8f;
    const float BUCHNER_FUNNEL_ATTATCH_DIST = 2f;const float BUCHNER_FLASK_ATTATCH_DIST = 1.4f;


    public multihandler multiHandlerScript;


    public GameObject itemHeldByTongs; int itemHeldByTongsLayer;

    public GameObject heldPipette; public float pipetteSpeed;
    public GameObject closestIronStand; public GameObject closestIronRing; public GameObject closestIronMesh;
    public GameObject ironMesh;
    private bool flowLock = false;
    private bool scoopulaAnimationPlaying = false;
    pickUpObjects pickUpScript;
    public Vector3 testingOffset;
    public bool funnelIsAttatched = false;
    public GameObject funneledFlask;
    public GameObject filteredFunnel;
    public bool filterIsAttatched = false;
    public bool buchnerfunnelIsAttached = false;
    public GameObject buchnerfunneledFlask;
    public GameObject buchnerfilteredFunnel;
    public bool buchnerfilterIsAttached = false;

    public bool isRodInBeaker = false;
    public bool isSmallRodInBeaker = false;
    public bool isSmallRodInBeaker2 = false;
    public bool isSmallRodInBeaker3 = false;
    public bool isSmallRodInBeaker4 = false;
    public GameObject rodInBeaker = null;
    public GameObject rodInBeaker2 = null;
    public GameObject rodInBeaker3 = null;
    public GameObject rodInBeaker4 = null;
    public bool beginStirring = false;
    public bool beginStirring2 = false;
    public bool beginStirring3 = false; 
    public bool beginStirring4 = false;
    //public Animator stirAnimator;
    public Animator smallStirAnimator;  
    public Animator smallStirAnimator2;
    public Animator smallStirAnimator3;
    public Animator smallStirAnimator4;                     

    public bool tryingToPipetteSolid = false;
    public bool MeshSnapped = false;

    public bool tryingToMixCompoundsInNonLiquidHolder = false;
    private Coroutine pouringCoroutine; // Store reference to coroutine
    public GameObject paperTowelSheet;
    public GameObject combinedApparatusPrefab;
    public GameObject ApparatusCanvasPrefab;
    public bool tryingToPourLiquidOnPaperTowel = false;
    public List<Transform> buchnerFlaskNozzleLocations = new List<Transform>();
    public bool meltingPointToolPlaced = false;
    public GameObject meltingPointBeaker;
    public bool IsNearIronMesh { get; private set; }
    public GameObject currentCanvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        multiHandlerScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<multihandler>();
        pickUpScript = GetComponent<pickUpObjects>();

        //stirAnimator = GameObject.Find("Stir Rod").GetComponent<Animator>();
        //stirAnimator.enabled = false;

        smallStirAnimator = GameObject.Find("Small Stir Rod").GetComponent<Animator>();
        smallStirAnimator.enabled = false;

        smallStirAnimator2 = GameObject.Find("Small Stir Rod 2").GetComponent<Animator>();
        smallStirAnimator2.enabled = false;

        smallStirAnimator3 = GameObject.Find("Small Stir Rod 3").GetComponent<Animator>();
        smallStirAnimator3.enabled = false;

        smallStirAnimator4 = GameObject.Find("Small Stir Rod 4").GetComponent<Animator>();
        smallStirAnimator4.enabled = false;

        ironMesh = GameObject.Find("Iron Mesh");
        Rigidbody rb = ironMesh.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

    }

    
    // Update is called once per frame
    void Update()
    {

        checkForInput();

        

        if (heldPipette)
        {
            if (heldPipette == pickUpScript.other)
            {
                pipetteSpeed = heldPipette.GetComponent<pipetteScript>().flowSpeed;
            }
        }
        if (pickUpScript.other){
            if (pickUpScript.other.transform.name.StartsWith("Beaker") || pickUpScript.other.transform.name == "Weigh Boat" || pickUpScript.other.transform.name.StartsWith("Erlenmeyer Flask") || pickUpScript.other.transform.name == "Paper Cone" || pickUpScript.other.transform.name == "Pipette" || pickUpScript.other.transform.name == "Graduated Cylinder" || pickUpScript.other.transform.name.StartsWith("Paper Towel") || pickUpScript.other.transform.name.StartsWith("Buchner Flask")){
                lightUpBeaker();
                if (pickUpScript.other.GetComponent<liquidScript>()){
                    if (pickUpScript.other.GetComponent<liquidScript>().isPouring){
                        turnOffBeakers();
                    }
                }
            }
        }

        if (pickUpScript.other && pickUpScript.other.name == "Iron Ring") // Snap ring to stand
            CheckForIronStandNearby(pickUpScript.other);

        if (pickUpScript.other && pickUpScript.other.name == "Iron Mesh") // Snap mesh to ring
            CheckForIronRingNearby(pickUpScript.other);

        if (pickUpScript.other && pickUpScript.other.CompareTag("LiquidHolder")) // Snap liquid to iron mesh
            CheckForIronMeshNearby(pickUpScript.other);

    }

    void LateUpdate()
    {
        if (pickUpScript.other && pickUpScript.other.name == "Buchner Flask") // If we can snap buchner flask tell user
            CheckForSinkSetupForBuchnerFlaskNearby(pickUpScript.other);

        if (itemHeldByTongs)
            handleTongObject();
    }

    void checkForInput()
    {
        if (Input.GetMouseButtonDown(1))
            findObjectAndPerformAction();

        if (Input.GetMouseButton(1))
            findObjectAndPerformHeldAction();

        if (Input.GetMouseButtonUp(1))
        {
            findObjectAndPerformLiftedMouseAction();
        }

        checkForKeyOrScrollInputs();
    }

    void findObjectAndPerformAction()       // Right click once
    {
        tryingToPourLiquidOnPaperTowel = false;
        if (pickUpScript.other != null)
        {
            GameObject obj = pickUpScript.other;

            if (obj.name == "Fire extinguisher")
                ShootFoam();

            if (obj.name == "Tongs")
                GrabFlaskByNeck(obj);

            if (obj.name == "Matchbox")
                LightMatchAndTossForward(obj);

            if (obj.name == "Iron Ring")
                SnapIronRingToStand();

            if (obj.name == "Iron Mesh")
                SnapIronMeshToRing();

            if (obj.CompareTag("LiquidHolder"))
            {
                SnapLiquidHolderToIronMesh();
            }

            if (obj.name == "Bunsen Burner")
                faceItemAwayFromPlayer();

            if (obj.name == "Scoopula")
                GatherAluminumPelletsFromContainerOrDropThem();

            if (obj.name == "Glass Funnel")
                insertFunnel(obj);

            if (obj.name == "Paper Cone")
                insertFilter(obj);

            if (obj.name == "Buchner Funnel")
                insertBuchnerFunnel(obj);

            if (obj.name.StartsWith("Buchner Flask"))
                tryToAttachToTableNozzle();


            //if (obj.name == "Buchner Paper Cone")
            //    insertBuchnerFilter(obj);

            if (obj.name.StartsWith("Small Stir Rod"))
                putStirRodInBeaker(obj);

            if (obj.name == "Capilary tube")
                insertMeltingPointApparatus(obj);

            if (obj.name == "Melting Point Tool")
                placeMeltingPointApparatus(obj);
        }
    }

    void findObjectAndPerformHeldAction()  // Held right click
    {
        if (beginStirring == false) {
            if (isSmallRodInBeaker) {
                beginStirring = true;
                handleSmallStirringAnims();
            }
        }
        if (beginStirring2 == false) {
            if (isSmallRodInBeaker2) {
                beginStirring2 = true;
                handleSmallStirringAnims2();
            }
        }
        if (beginStirring3 == false) {
            if (isSmallRodInBeaker3) {
                beginStirring3 = true;
                handleSmallStirringAnims3();
            }
        }
        if (beginStirring4 == false) {
            if (isSmallRodInBeaker4) {
                beginStirring4 = true;
                handleSmallStirringAnims4();
            }
        }

        if (pickUpScript.other != null)
        {
            GameObject obj = pickUpScript.other;

            if (obj.name == "Pipette")
            {
                if (!heldPipette)
                {
                    heldPipette = obj;
                    heldPipette.GetComponent<pipetteScript>().pipetteFlowStatus = true;
                    pipetteSpeed = heldPipette.GetComponent<pipetteScript>().flowSpeed;
                }
                if (flowLock == false)
                {
                    if (heldPipette.GetComponent<pipetteScript>().pipetteVolume > 0 && heldPipette.GetComponent<pipetteScript>().pipetteFlowStatus == true) //is the pipette flowing?
                    {
                        heldPipette.GetComponent<pipetteScript>().pipetteFlowing = true;
                        heldPipette.GetComponent<pipetteScript>().pipetteFlowStatus = false;
                    }
                    else
                    {
                        heldPipette.GetComponent<pipetteScript>().pipetteExtracting = true;
                        heldPipette.GetComponent<pipetteScript>().pipetteFlowStatus = true;
                    }

                    flowLock = true;
                }
                SetPippetteSpeed(obj, pipetteSpeed);
            }

            if (obj.name.StartsWith("Beaker")){
                BringObjectCloser(-1.5f);
                if (obj.name == "Beaker 800mL")
                    BringObjectCloser(-1.74f);
                if (obj.name == "Beaker 400mL")
                    BringObjectCloser(-1.83f);
                if (obj.name == "Beaker 250mL")
                    BringObjectCloser(-1.9f);
                if (obj.name == "Beaker 150mL")
                    BringObjectCloser(-1.95f);
                if (obj.name == "Beaker 100mL")
                    BringObjectCloser(-2f);
                if (obj.name == "Beaker 50mL")
                    BringObjectCloser(-2.06f);
            }

            if (obj.name == "Graduated Cylinder")
                BringObjectCloser(-1.5f);

            if (obj.name == "Weigh Boat")
                BringObjectCloser(-1.5f);

            if (obj.name.StartsWith("Erlenmeyer Flask"))
                BringObjectCloser(-1.5f);
            
            if (obj.name.StartsWith("Paper Cone")){
                BringObjectCloser(-1.5f);
            }

            if (obj.name == "Bunsen Burner")
                manipulateBunsenBurner();
        }
    }


    void findObjectAndPerformLiftedMouseAction()  // Lifted Right Click
    {
        if (isRodInBeaker == true){
            //beginStirring = false;
            //Debug.Log("Stir animator: " + stirAnimator);
            //if (stirAnimator != null) {
                //stirAnimator.enabled = false;
                //stirAnimator.SetBool("IsStirring", false);
                //stirAnimator.SetBool("is800", false);
                //stirAnimator.SetBool("is400", false);
            //}
        }

        if (isSmallRodInBeaker == true) {
            beginStirring = false;
            if (smallStirAnimator != null) {
                smallStirAnimator.enabled = false;
                smallStirAnimator.SetBool("currentlyStirring", false);
                smallStirAnimator.SetBool("is50", false);
                smallStirAnimator.SetBool("is100", false);
                smallStirAnimator.SetBool("is150", false);
                smallStirAnimator.SetBool("is250", false);
                smallStirAnimator.SetBool("is400", false);
                smallStirAnimator.SetBool("is800", false);
            }
        }

        if (isSmallRodInBeaker2 == true) {
            beginStirring2 = false;
            if (smallStirAnimator2 != null) {
                smallStirAnimator2.enabled = false;
                smallStirAnimator2.SetBool("currentlyStirring", false);
                smallStirAnimator2.SetBool("is50", false);
                smallStirAnimator2.SetBool("is100", false);
                smallStirAnimator2.SetBool("is150", false);
                smallStirAnimator2.SetBool("is250", false);
                smallStirAnimator2.SetBool("is400", false);
                smallStirAnimator2.SetBool("is800", false);
            }
        }

        if (isSmallRodInBeaker3 == true) {
            beginStirring3 = false;
            if (smallStirAnimator3 != null) {
                smallStirAnimator3.enabled = false;
                smallStirAnimator3.SetBool("currentlyStirring", false);
                smallStirAnimator3.SetBool("is50", false);
                smallStirAnimator3.SetBool("is100", false);
                smallStirAnimator3.SetBool("is150", false);
                smallStirAnimator3.SetBool("is250", false);
                smallStirAnimator3.SetBool("is400", false);
                smallStirAnimator3.SetBool("is800", false);
            }
        }

        if (isSmallRodInBeaker4 == true) {
            beginStirring4 = false;
            if (smallStirAnimator4 != null) {
                smallStirAnimator4.enabled = false;
                smallStirAnimator4.SetBool("currentlyStirring", false);
                smallStirAnimator4.SetBool("is50", false);
                smallStirAnimator4.SetBool("is100", false);
                smallStirAnimator4.SetBool("is150", false);
                smallStirAnimator4.SetBool("is250", false);
                smallStirAnimator4.SetBool("is400", false);
                smallStirAnimator4.SetBool("is800", false);
            }
        }

        if (pickUpScript.other != null)
        {
            GameObject obj = pickUpScript.other;

            if (obj.name == "Pipette")
            {
                SetPippetteSpeed(obj, 0f);
                heldPipette.GetComponent<pipetteScript>().pipetteFlowing = false;
                heldPipette.GetComponent<pipetteScript>().pipetteExtracting = false;
                flowLock = false;
            }

            if (obj.name == "Bunsen Burner")
                resetRotationOffset();
        }
    }

    void checkForKeyOrScrollInputs()
    {
        if (pickUpScript.other != null)
        {
            GameObject obj = pickUpScript.other;

            if (obj.name == "Bunsen Burner")
                if (Input.GetMouseButton(1))
                {
                    obj.GetComponent<bunsenBurnerScript>().AdjustAirflowBasedOnInput(Input.mouseScrollDelta.y * 2f);
                    obj.GetComponent<bunsenBurnerScript>().AdjustGearRotation(Input.mouseScrollDelta.y * 2f);
                }

            if (obj.name.StartsWith("Beaker") || obj.name.StartsWith("Erlenmeyer Flask")|| obj.name == "Paper Cone" || obj.name == "Graduated Cylinder" || obj.name == "Buchner Flask")
            {
                if (obj.transform.Find("Melting Point Tool") == null){
                    if (Input.GetKey(KeyCode.P)) // While "P" is held
                    {
                        if (!obj.GetComponent<liquidScript>().isPouring) // Only start pouring if it's not already pouring
                        {
                            startPour();
                        }
                    }
                    else if (Input.GetKeyUp(KeyCode.P)) // When "P" is released
                    {
                        stopPour();
                    }
                }
            }
            if (obj.name == "Weigh Boat" || obj.name.StartsWith("Paper Towel")){
                if (Input.GetKey(KeyCode.P)) // While "P" is held
                {
                    if (!obj.GetComponent<weighboatscript>().isPouring) // Only start pouring if it's not already pouring
                    {
                        startWeighBoatPour();
                    }
                }
                else if (Input.GetKeyUp(KeyCode.P)) // When "P" is released
                {
                    stopWeighBoatPour();
                }
            }
        }
    }

    void BringObjectCloser(float dist)
    {
        pickUpScript.distOffset = dist;
    }

    void CheckForSinkSetupForBuchnerFlaskNearby(GameObject buchnerFlask){
        float minDist = Mathf.Infinity;
        Transform closestNozzle = null;

        foreach (Transform nozzle in buchnerFlaskNozzleLocations){
            if (Vector3.Distance(buchnerFlask.transform.position, nozzle.position) < minDist) {
                minDist = Vector3.Distance(buchnerFlask.transform.position, nozzle.position);
                closestNozzle = nozzle;
            }
        }

        if (closestNozzle && minDist <= BUCHNER_FLASK_ATTATCH_DIST){
            multiHandlerScript.setHelpText("Right Click to attatch Buchner Flask to Sink.");
        };
    }
    void tryToAttachToTableNozzle(){
            float minDist = Mathf.Infinity;
            Transform closestNozzle = null;

            foreach (Transform nozzle in buchnerFlaskNozzleLocations){
                if (Vector3.Distance(pickUpScript.other.transform.position, nozzle.position) < minDist) {
                    minDist = Vector3.Distance(pickUpScript.other.transform.position, nozzle.position);
                    closestNozzle = nozzle;
                }
            }
            if (closestNozzle && minDist <= BUCHNER_FLASK_ATTATCH_DIST){
                closestNozzle.parent.Find("Hanging Hose").gameObject.SetActive(false);
                closestNozzle.parent.Find("Buchner Hose").gameObject.SetActive(true);
                GameObject buchnerFlask = pickUpScript.other;
                pickUpScript.DropItem();
                buchnerFlask.transform.position = closestNozzle.parent.TransformPoint(new Vector3(-0.0427360535f, 0.0219124556f, 0.633605659f));
                buchnerFlask.transform.localEulerAngles = new Vector3(0f, 270f, 0f);
                buchnerFlask.GetComponent<Rigidbody>().isKinematic = true;
                buchnerFlask.GetComponent<liquidScript>().buchnerFaucet = closestNozzle.transform.parent.gameObject;
            }
        }

        public void disconnectBuchnerFlask(GameObject flask){

            float minDist = Mathf.Infinity;
            Transform closestNozzle = null;
            foreach (Transform nozzle in buchnerFlaskNozzleLocations){
                if (Vector3.Distance(flask.transform.position, nozzle.position) < minDist) {
                    minDist = Vector3.Distance(flask.transform.position, nozzle.position);
                    closestNozzle = nozzle;
                }
            }
            // print("DISCONNECT");
            closestNozzle.parent.Find("Hanging Hose").gameObject.SetActive(true);
            closestNozzle.parent.Find("Buchner Hose").gameObject.SetActive(false);

            pickUpScript.PickUpItem(flask);
            // buchnerFlask.transform.position = closestNozzle.parent.TransformPoint(new Vector3(-0.0427360535f, 0.0219124556f, 0.633605659f));

            flask.transform.position += Vector3.up * 0.1f;
            flask.GetComponent<Rigidbody>().isKinematic = false;
            flask.gameObject.layer = LayerMask.NameToLayer("Default");

            flask.GetComponent<liquidScript>().buchnerFaucet = null;
        }

    void startPour()
    {
        liquidScript LS = pickUpScript.other.GetComponent<liquidScript>();

        if (LS.isPouring) return;

        Debug.Log("ispouring");
        LS.isPouring = true;

        float minDist = Mathf.Infinity;
        GameObject closestBeakerOrFlask = null;

        foreach (GameObject currentObject in FindObjectsOfType<GameObject>())
        {
            if ((currentObject.tag == "LiquidHolder" || currentObject.name.StartsWith("Paper Towel") || currentObject.name.StartsWith("Weigh Boat")) && currentObject != pickUpScript.other)
            {
                // Retrieve the Liquid script attached to the current object
                liquidScript liquidScriptComponent = currentObject.GetComponent<liquidScript>();

                // Skip objects that are in the drawer
                if (liquidScriptComponent != null && liquidScriptComponent.InDrawer)
                    continue;

                var pipetteTip = pickUpScript.other.transform.position;
                pipetteTip.y = 0f;
                var beakerOrFlask = currentObject.transform.position;
                beakerOrFlask.y = 0f;

                float distFromTip = Vector3.Distance(pipetteTip, beakerOrFlask);

                if (distFromTip < minDist)
                {
                    minDist = distFromTip;
                    closestBeakerOrFlask = currentObject;
                }
            }
        }

        if (closestBeakerOrFlask == null)
        {
            Debug.LogWarning("No liquid container found nearby!");
            return;
        }

        float maxPourDistance = PIPETTE_GRAB_DISTANCE * 5f;
        float distance = Vector3.Distance(pickUpScript.other.transform.position, closestBeakerOrFlask.transform.position);

        if (distance > maxPourDistance) // Stop pouring if too far
        {
            return; // Exit the function
        }

        if (closestBeakerOrFlask.name.StartsWith("Paper Towel") || closestBeakerOrFlask.name.StartsWith("Weigh Boat"))
        {
            if (LS.liquidPercent < 0.2f)
            {
                pickUpScript.other.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Keep beaker tilted
                Debug.Log(closestBeakerOrFlask.transform.name);
                pouringCoroutine = StartCoroutine(PourToWeighBoatContinuously(LS, closestBeakerOrFlask.transform));
            }
            else
            {
                tryingToPourLiquidOnPaperTowel = true;
            }
        }
        else
        {
            Transform pourPos = closestBeakerOrFlask.transform.Find("pourPos");
            pickUpScript.other.transform.position = pourPos.position;

            pickUpScript.other.transform.localRotation = pourPos.transform.localRotation;
            pickUpScript.other.transform.rotation = Quaternion.Euler(90f, 90f, 0f); // Keep beaker tilted

            Debug.Log("Pouring into: " + closestBeakerOrFlask.name);

            // Store the coroutine reference
            pouringCoroutine = StartCoroutine(PourContinuously(LS, closestBeakerOrFlask.transform));
        }
    }


    void stopPour()
    {
        liquidScript LS = pickUpScript.other.GetComponent<liquidScript>();
        LS.isPouring = false;
        pickUpScript.other.transform.rotation = Quaternion.identity;
        //pickUpScript.other.GetComponent<MeshCollider>().isTrigger = false;

        // Stop only this specific coroutine
        if (pouringCoroutine != null)
        {
            StopCoroutine(pouringCoroutine);
            pouringCoroutine = null;
        }
    }
    IEnumerator PourContinuously(liquidScript LS, Transform targetContainer)
    {
        float maxPourDistance = PIPETTE_GRAB_DISTANCE * 5f; // Set the max allowed distance for pouring

        while (LS.isPouring)
        {
            // Check if target container is still within range
            if(!targetContainer){
                yield break;
            }
            float distance = Vector3.Distance(pickUpScript.other.transform.position, targetContainer.position);

            if (distance > maxPourDistance) // Stop pouring if too far
            {
                stopPour();
                yield break; // Exit the coroutine
            }

            if(targetContainer.GetComponent<liquidScript>().currentVolume_mL + 1f < targetContainer.GetComponent<liquidScript>().totalVolume_mL){
                LS.filterSolution(LS.solutionMakeup, 150 * Time.deltaTime, targetContainer); // Pour 1 unit per frame
            }
            targetContainer.GetComponent<Rigidbody>().AddForce(Vector3.up * 0.0001f, ForceMode.Impulse);
            yield return new WaitForSeconds(0.1f); // Controls pour speed

        }
    }

    IEnumerator PourToWeighBoatContinuously(liquidScript LS, Transform targetContainer)
    {
        float maxPourDistance = PIPETTE_GRAB_DISTANCE; // Set the max allowed distance for pouring

        while (LS.isPouring)
        {
            // Check if target container is still within range
            float distance = Vector3.Distance(pickUpScript.other.transform.position, targetContainer.position);

            if (distance > maxPourDistance) // Stop pouring if too far
            {
                stopPour();
                yield break; // Exit the coroutine
            }
            if (LS.currentVolume_mL > 0.4f){
                LS.currentVolume_mL -= 0.1852f;
                targetContainer.GetComponent<weighboatscript>().addScoop(LS.solutionMakeup);
            }
        
            yield return new WaitForSeconds(0.1f); // Controls pour speed
        }
    }

    void startWeighBoatPour()
    {
        weighboatscript WBS = pickUpScript.other.GetComponent<weighboatscript>();

        if (WBS.isPouring) return;

        WBS.isPouring = true;
        GameObject closestBeakerOrFlask = findClosestItemWithTag("LiquidHolder", pickUpScript.other);

        if (closestBeakerOrFlask == null)
        {
            Debug.LogWarning("No liquid container found nearby!");
            return;
        }

        float maxPourDistance = PIPETTE_GRAB_DISTANCE * 8f;
        float distance = Vector3.Distance(pickUpScript.other.transform.position, closestBeakerOrFlask.transform.position);

        if (distance > maxPourDistance) // Stop pouring if too far
        {
            return; // Exit the function
        }
        pickUpScript.other.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Keep beaker tilted

        //pickUpScript.other.transform.eulerAngles = new Vector3(90f, 0f, 0f);
        Debug.Log("Pouring into: " + closestBeakerOrFlask.name);

        // Store the coroutine reference
        pouringCoroutine = StartCoroutine(PourWeighBoatContinuously(WBS, closestBeakerOrFlask.transform));
    }

    void stopWeighBoatPour()
    {
        weighboatscript WBS = pickUpScript.other.GetComponent<weighboatscript>();
        WBS.isPouring = false;
        pickUpScript.other.transform.rotation = Quaternion.identity;

        // Stop only this specific coroutine
        if (pouringCoroutine != null)
        {
            StopCoroutine(pouringCoroutine);
            pouringCoroutine = null;
        }
    }
    IEnumerator PourWeighBoatContinuously(weighboatscript WBS, Transform targetContainer)
    {
        float maxPourDistance = PIPETTE_GRAB_DISTANCE* 3f; // Set the max allowed distance for pouring

        while (WBS.isPouring)
        {
            // Check if target container is still within range
            float distance = Vector3.Distance(pickUpScript.other.transform.position, targetContainer.position);

            if (distance > maxPourDistance) // Stop pouring if too far
            {
                stopWeighBoatPour();
                yield break; // Exit the coroutine
            }

            if (WBS.scoopsHeld > 0){
                Debug.Log("Made it here");
                WBS.removeScoop();
                Debug.Log("Made it after remove scoop");
                targetContainer.GetComponent<liquidScript>().addSolution(new List<float> { 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f }, 0.1852f);
                
            }
            yield return new WaitForSeconds(0.1f); // Controls pour speed
        }
    }

    void insertFunnel(GameObject funnel)
    {
        float minDist = Mathf.Infinity;
        GameObject closestFlask = null;
        Transform flaskOpening = null;

        // Find the closest Flask
        foreach (GameObject currentObject in FindObjectsOfType<GameObject>())
        {
            if (currentObject.name.StartsWith("Erlenmeyer Flask"))
            {
                float distFromFunnel = Vector3.Distance(funnel.transform.position, currentObject.transform.position);

                if (distFromFunnel < minDist)
                {
                    minDist = distFromFunnel;
                    closestFlask = currentObject;

                    // Find the flask's top position
                    flaskOpening = closestFlask.transform.Find("FlaskTop");
                }
            }
        }

        // Attach the funnel to the flask if within range
        if (closestFlask && flaskOpening && minDist <= FUNNEL_INSERT_DISTANCE)
        {
            pickUpScript.DropItem();

            // Disable physics and collisions so it stays attached
            Physics.IgnoreCollision(funnel.GetComponent<Collider>(), closestFlask.GetComponent<Collider>(), true);

            // Attach funnel to flask
            funnel.transform.position = flaskOpening.position;
            funnel.transform.rotation = flaskOpening.rotation;

            // Make it a child so it follows movement
            funnel.transform.SetParent(closestFlask.transform);

            Rigidbody rb = funnel.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.isKinematic = true;
            }
            funneledFlask = closestFlask;
            funneledFlask.tag = "Untagged";
            funnelIsAttatched = true;
        }
    }

    public void DetachFunnel(GameObject funnel)
    {
        // Remove parent so it no longer follows the flask
        funnel.transform.SetParent(null);

        // Re-enable physics and collisions
        Physics.IgnoreCollision(funnel.GetComponent<Collider>(), funneledFlask.GetComponent<Collider>(), false);

        Rigidbody rb = funnel.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
        }
        funneledFlask.tag = "LiquidHolder";
        funneledFlask = null;
        funnelIsAttatched = false;
    }

    void insertFilter(GameObject filter)
    {
        float minDist = Mathf.Infinity;
        GameObject closestFunnel = null;
        Transform funnelOpening = null;

        // Find the closest Flask
        foreach (GameObject currentObject in FindObjectsOfType<GameObject>())
        {
            if ((currentObject.name == "Glass Funnel" || currentObject.name == "Buchner Funnel") && currentObject.transform.parent != null)
            {
                if (currentObject.transform.parent.name.StartsWith("Erlenmeyer Flask") || currentObject.transform.parent.name.StartsWith("Buchner Flask"))
                {
                    float distFromFilter = Vector3.Distance(filter.transform.position, currentObject.transform.position);

                    if (distFromFilter < minDist)
                    {
                        minDist = distFromFilter;
                        closestFunnel = currentObject;

                        // Find the flask's top position
                        funnelOpening = closestFunnel.transform.Find("FunnelTop");
                    }
                }
            }
        }

        // Attach the funnel to the flask if within range
        if (closestFunnel && funnelOpening && minDist <= FUNNEL_INSERT_DISTANCE)
        {
            // Disable physics and collisions so it stays attached
            Physics.IgnoreCollision(filter.GetComponent<MeshCollider>(), closestFunnel.GetComponent<Collider>(), true);
            pickUpScript.DropItem();

            // Attach funnel to flask
            filter.transform.position = funnelOpening.position;
            filter.transform.rotation = funnelOpening.rotation;

            // Make it a child so it follows movement
            filter.transform.SetParent(closestFunnel.transform);

            Rigidbody rb = filter.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.isKinematic = true;
            }

            if (closestFunnel.name == "Glass Funnel")
            {
                filteredFunnel = closestFunnel;
                filterIsAttatched = true;
            }
            else
            {
                buchnerfilteredFunnel = closestFunnel;
                buchnerfilterIsAttached = true;
            }
        }
    }

    public void DetachFilter(GameObject filter)
    {
        Debug.Log(filter.transform.name);

        Rigidbody rb = filter.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
        }

        Debug.Log(filter.transform.parent.name);
        if (filter.transform.parent.name == "Glass Funnel")
        {
            // Re-enable physics and collisions
            Physics.IgnoreCollision(filter.GetComponent<MeshCollider>(), filteredFunnel.GetComponent<Collider>(), false);
            filteredFunnel = null;
            filterIsAttatched = false;
        }
        else
        {
            // Re-enable physics and collisions
            Physics.IgnoreCollision(filter.GetComponent<MeshCollider>(), buchnerfilteredFunnel.GetComponent<Collider>(), false);
            buchnerfilteredFunnel = null;
            buchnerfilterIsAttached = false;
        }
        // Remove parent so it no longer follows the flask
        filter.transform.SetParent(null);
    }

    void insertBuchnerFunnel(GameObject funnel)
    {
        float minDist = Mathf.Infinity;
        GameObject closestFlask = null;
        Transform flaskOpening = null;

        // Find the closest Flask
        foreach (GameObject currentObject in FindObjectsOfType<GameObject>())
        {
            if (currentObject.name == "Buchner Flask")
            {
                float distFromFunnel = Vector3.Distance(funnel.transform.position, currentObject.transform.position);

                if (distFromFunnel < minDist)
                {
                    minDist = distFromFunnel;
                    closestFlask = currentObject;

                    // Find the flask's top position
                    flaskOpening = closestFlask.transform.Find("FlaskTop");
                }
            }
        }

        // Attach the funnel to the flask if within range
        if (closestFlask && flaskOpening && minDist <= FUNNEL_INSERT_DISTANCE)
        {
            pickUpScript.DropItem();

            // Attach funnel to flask
            funnel.transform.position = flaskOpening.position;
            funnel.transform.rotation = flaskOpening.rotation;

            // Make it a child so it follows movement
            funnel.transform.SetParent(closestFlask.transform);

            // Disable physics and collisions so it stays attached
            Physics.IgnoreCollision(funnel.GetComponent<Collider>(), closestFlask.GetComponent<Collider>(), true);

            Rigidbody rb = funnel.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.isKinematic = true;
            }
            buchnerfunneledFlask = closestFlask;
            buchnerfunneledFlask.tag = "Untagged";
            buchnerfunnelIsAttached = true;
        }
    }

    public void DetachBuchnerFunnel(GameObject funnel)
    {
        // Remove parent so it no longer follows the flask
        Debug.Log("detatching funnel");
        funnel.transform.SetParent(null);

        // Re-enable physics and collisions
        Physics.IgnoreCollision(funnel.GetComponent<Collider>(), buchnerfunneledFlask.GetComponent<Collider>(), false);

        Rigidbody rb = funnel.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
        }
        buchnerfunneledFlask.tag = "LiquidHolder";
        buchnerfunneledFlask = null;
        buchnerfunnelIsAttached = false;
    }
    void insertMeltingPointApparatus(GameObject capillaryTube)
    {   
        float minDistTherm = Mathf.Infinity;
        float minDistRubber = Mathf.Infinity;
        GameObject thermometer = null;
        GameObject rubberBand = null;
        foreach (GameObject currentObject in FindObjectsOfType<GameObject>())
        {
            if (currentObject.name == "Therometer")
            {
                float distFromTherm = Vector3.Distance(capillaryTube.transform.position, currentObject.transform.position);

                if (distFromTherm < minDistTherm)
                {
                    minDistTherm = distFromTherm;
                    thermometer = currentObject;
                }
            }

            if (currentObject.name == "RubberBand")
            {
                float distFromTherm = Vector3.Distance(capillaryTube.transform.position, currentObject.transform.position);

                if (distFromTherm < minDistRubber)
                {
                    minDistRubber = distFromTherm;
                    rubberBand = currentObject;
                }
            }
        }
        

        if (thermometer == null || rubberBand == null)
        {
            Debug.Log("Required components not found in the scene.");
            return;
        }

        float distThermometer = Vector3.Distance(capillaryTube.transform.position, thermometer.transform.position);
        float distRubberBand = Vector3.Distance(capillaryTube.transform.position, rubberBand.transform.position);

        Debug.Log("Thermometer distance: " + distThermometer);
        Debug.Log("Rubber Band distance: " + distRubberBand);

        // Check if all components are within range
        if (distThermometer <= CAPILLARY_ASSEMBLY_DISTANCE && distRubberBand <= CAPILLARY_ASSEMBLY_DISTANCE)
        {
            Debug.Log("All components within assembly range. Creating assembled apparatus...");

            // Instantiate the assembled apparatus at the thermometer's position
            GameObject newApparatus = Instantiate(combinedApparatusPrefab, thermometer.transform.position, thermometer.transform.rotation);
            newApparatus.name = combinedApparatusPrefab.name; // Remove "(Clone)"



            // Destroy individual components
            pickUpScript.DropItem();
            Destroy(thermometer);
            Destroy(rubberBand);
            Transform capillaryTubeHolder = newApparatus.transform.Find("Capilary tube Prefab");
            if (capillaryTubeHolder != null)
            {
                capillaryTube.transform.SetParent(capillaryTubeHolder, false);
                capillaryTube.transform.localPosition = Vector3.zero;
                capillaryTube.transform.localRotation = Quaternion.identity;
                capillaryTube.transform.localScale = Vector3.one;
                capillaryTube.GetComponent<CapsuleCollider>().isTrigger = true;
                capillaryTube.GetComponent<Rigidbody>().isKinematic = true;
                capillaryTube.GetComponent<liquidScript>().CapilaryAttached = true;

            }
            else
            {
                Debug.LogWarning("Capilary tube not found in new apparatus.");
            }
            Debug.Log("Melting Point Apparatus Assembled Successfully!");
        }
        else
        {
            Debug.Log("Assembly failed. Components may be out of range.");
            //time to gather our sample
            GameObject closestBeakerOrFlask = findClosestItemWithTag("LiquidHolder", capillaryTube);
            float maxSampleDistance = PIPETTE_GRAB_DISTANCE * 2f;
            float distance = Vector3.Distance(capillaryTube.transform.position, closestBeakerOrFlask.transform.position);

            if (distance > maxSampleDistance) // Stop pouring if too far
            {
                return; // Exit the function
            }

            liquidScript LS = capillaryTube.GetComponent<liquidScript>();
            liquidScript CBLS = closestBeakerOrFlask.GetComponent<liquidScript>();

            LS.addSolution(CBLS.solutionMakeup, 1f); // FILL IT UP
        }
    }

    void placeMeltingPointApparatus(GameObject MeltingPointTool)
    {
        Debug.Log("Trying");
        // Find all beakers in the scene
        List<GameObject> filteredBeakers = new List<GameObject>();
        foreach (GameObject beaker in GameObject.FindGameObjectsWithTag("LiquidHolder"))
        {
            if (beaker.name != "Capilary tube")
            {
                filteredBeakers.Add(beaker);
            }
        }
        GameObject[] beakers = filteredBeakers.ToArray();


        GameObject closestBeaker = null;
        float closestDistance = Mathf.Infinity;

        // Loop through all beakers to find the closest one
        foreach (GameObject beaker in beakers)
        {
            float distBeaker = Vector3.Distance(MeltingPointTool.transform.position, beaker.transform.position);
            if (distBeaker < closestDistance)
            {
                closestDistance = distBeaker;
                closestBeaker = beaker;
            }
        }

        // If no closest beaker was found, return
        if (closestBeaker == null)
        {
            Debug.Log("No closest beaker found.");

        }

        // Get the liquidScript from the closest beaker
        liquidScript beakerLiquid = closestBeaker.GetComponent<liquidScript>();
        if (beakerLiquid == null)
        {
            Debug.Log("Closest beaker has no Liquid script attached.");
        }

        if (beakerLiquid.percentH2SO4 == 0 &&
            beakerLiquid.percentKOH == 0 &&
            beakerLiquid.percentH2O == 1 &&
            beakerLiquid.percentK2SO4 == 0 &&
            beakerLiquid.percentAl == 0 &&
            beakerLiquid.percentKAlOH4 == 0 &&
            beakerLiquid.percentAl2SO43 == 0 &&
            beakerLiquid.percentAlum == 0 &&
            beakerLiquid.percentAlOH3 == 0 &&
            beakerLiquid.percentKAlSO42 == 0 &&
            beakerLiquid.percentKAlO2 == 0)
        {
            float distBeaker = Vector3.Distance(MeltingPointTool.transform.position, closestBeaker.transform.position);
            Debug.Log("Distance to beaker: " + distBeaker);

            if (distBeaker <= CAPILLARY_ASSEMBLY_DISTANCE)
            {
                Debug.Log("Melting Point Apparatus successfully placed near the beaker.");
                pickUpScript.DropItem();
                Rigidbody rb = MeltingPointTool.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }

                BoxCollider collider = MeltingPointTool.GetComponent<BoxCollider>();
                if (collider != null)
                {
                    collider.isTrigger = true; // Makes the collider a trigger
                }
                // Set as a child of the beaker
                MeltingPointTool.transform.SetParent(closestBeaker.transform);

                // Apply local position, rotation, and scale
                MeltingPointTool.transform.localPosition = new Vector3(-0.0810000002f,-0.125f,-0.0160000008f);
                
                MeltingPointTool.transform.localRotation = Quaternion.Euler(0f, 0f, -25.826f);
                currentCanvas = Instantiate(ApparatusCanvasPrefab, MeltingPointTool.transform.position, Quaternion.identity);
                currentCanvas.name = ApparatusCanvasPrefab.name;

                currentCanvas.transform.SetParent(closestBeaker.transform, false);
                currentCanvas.transform.localPosition = Vector3.zero;
                currentCanvas.transform.localRotation = Quaternion.identity;
                currentCanvas.transform.localScale = Vector3.one;
                //MeltingPointTool.transform.localScale = new Vector3(6.905945f, 5.176058f, 7.692307f);

                Debug.Log("Melting Point Apparatus placed successfully with specified transform settings.");
            }
            else
            {
                Debug.Log("Beaker is out of range. Cannot place Melting Point Apparatus.");
            }
        }
        else
        {
            Debug.Log("Beaker does not contain pure water. Cannot place Melting Point Apparatus.");
            //Debug.Log(beakerLiquid.percentH2O);
            //Debug.Log((beakerLiquid.percentH2SO4, beakerLiquid.percentKOH, beakerLiquid.percentH2O, beakerLiquid.percentK2SO4, beakerLiquid.percentAl, beakerLiquid.percentKAlOH4, beakerLiquid.percentAl2SO43, beakerLiquid.percentAlum, beakerLiquid.percentAlOH3, beakerLiquid.percentKAlSO42, beakerLiquid.percentKAlO2));
        }
        meltingPointToolPlaced = true;
        meltingPointBeaker = closestBeaker;
    }

    public void DetachMeltingPointTool(GameObject tool)
    {
        if (tool == null)
        {
            return;
        }

        Rigidbody rb = tool.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
        }
        tool.GetComponent<BoxCollider>().isTrigger = false;
        tool.transform.SetParent(null);
        meltingPointToolPlaced = false;
        if (currentCanvas != null)
        {
            Destroy(currentCanvas);
            currentCanvas = null;
        }
    }
    void GrabFlaskByNeck(GameObject tongs)
    {

        // Find Closest Flask in the room
        float minDist = Mathf.Infinity;
        GameObject closestFlask = null;

        foreach (GameObject currentObject in FindObjectsOfType<GameObject>())
        {

            if (currentObject.name.StartsWith("Erlenmeyer Flask") || currentObject.name.StartsWith("Buchner Flask"))
            {

                float distFromTip = Vector3.Distance(tongs.transform.Find("Tip").transform.position, currentObject.transform.position);

                if (distFromTip < minDist)
                {
                    minDist = distFromTip;
                    closestFlask = currentObject;
                }
            }
        }

        // If we have a flask held, drop it
        if (itemHeldByTongs)
        {
            tongs.transform.Find("Open").gameObject.SetActive(true);
            tongs.transform.Find("Closed").gameObject.SetActive(false);
            dropItemFromTongsCorrectly();
            return;
        }

        if (!closestFlask || minDist > TONG_GRAB_DISTANCE) // If we cannot pick up flask make sure meshes are good
        {
            itemHeldByTongs = null;
            tongs.transform.Find("Open").gameObject.SetActive(true);
            tongs.transform.Find("Closed").gameObject.SetActive(false);
            return;
        }

        if (closestFlask && minDist <= TONG_GRAB_DISTANCE) // Now we have closest Flask
        {
            tongs.transform.Find("Closed").gameObject.SetActive(true); // Turn on closed mesh
            tongs.transform.Find("Open").gameObject.SetActive(false); // Turn off open mesh
            itemHeldByTongs = closestFlask;
            itemHeldByTongs.GetComponent<Rigidbody>().isKinematic = true;
            itemHeldByTongs.GetComponent<Rigidbody>().useGravity = true;
            itemHeldByTongsLayer = itemHeldByTongs.layer;
            itemHeldByTongs.layer = LayerMask.NameToLayer("HeldByOther");
            var rot = itemHeldByTongs.transform.localEulerAngles;
            itemHeldByTongs.transform.localEulerAngles = new Vector3(0f, rot.y, 0f);
        }
    }

    void putStirRodInBeaker(GameObject stirRod)
    {   //Debug.Log("Stir Rod was put in beaker");

        float minDist = Mathf.Infinity;
        GameObject closestBeaker = null;
        Transform stirPosition = null;

        // locate closest beaker
        foreach (GameObject currentObject in FindObjectsOfType<GameObject>())
        {
            /*if (pickUpScript.other && pickUpScript.other.name == "Stir Rod" && (currentObject.name == "Beaker 800mL" || currentObject.name == "Beaker 400mL")) {
                float distFromBeaker = Vector3.Distance(stirRod.transform.position, currentObject.transform.position);

                if (distFromBeaker < minDist) {
                    minDist = distFromBeaker;
                    closestBeaker = currentObject;

                    // designated position
                    stirPosition = closestBeaker.transform.Find("StirPos");
                }
                Debug.Log("This is a larger beaker");
            } */

            if (pickUpScript.other && pickUpScript.other.name.StartsWith("Small Stir Rod") && (currentObject.name.StartsWith("Beaker"))) {
                float distFromBeaker = Vector3.Distance(stirRod.transform.position, currentObject.transform.position);

                if (distFromBeaker < minDist) {
                    minDist = distFromBeaker;
                    closestBeaker = currentObject;

                    // designated position
                    stirPosition = closestBeaker.transform.Find("StirPos");
                    //Debug.Log("Starting rod at position: " + stirPosition.localPosition);
                }
                Debug.Log("This is a smaller beaker");
            }
        }

        // teleport stir rod to animation position if in range
        if (closestBeaker && stirPosition && minDist <= FUNNEL_INSERT_DISTANCE)
        {
            pickUpScript.DropItem();

            stirRod.transform.position = stirPosition.position;
            stirRod.transform.rotation = stirPosition.rotation;
            stirRod.transform.SetParent(closestBeaker.transform, true);
            
            Debug.Log("stirposition" + stirRod.transform.localPosition);
            Debug.Log("stirrotation " + stirRod.transform.localRotation);
            Debug.Log("stirposition setting to " + stirPosition.transform.localPosition);
            Debug.Log("stirposition setting to " + stirPosition.transform.localRotation);

            // Disable physics and collisions so it stays attached
            Physics.IgnoreCollision(stirRod.GetComponent<Collider>(), closestBeaker.GetComponent<Collider>(), true);

            Rigidbody rb = stirRod.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.isKinematic = true;
            }

            if (stirRod.transform.name == "Small Stir Rod")
            {
                rodInBeaker = closestBeaker;
                rodInBeaker.tag = "Untagged";
                isSmallRodInBeaker = true;
            }
            else if (stirRod.transform.name == "Small Stir Rod 2")
            {
                rodInBeaker2 = closestBeaker;
                rodInBeaker2.tag = "Untagged";
                isSmallRodInBeaker2 = true;
            }
            else if (stirRod.transform.name == "Small Stir Rod 3")
            {
                rodInBeaker3 = closestBeaker;
                rodInBeaker3.tag = "Untagged";
                isSmallRodInBeaker3 = true;
            }
            else if (stirRod.transform.name == "Small Stir Rod 4")
            {
                rodInBeaker4 = closestBeaker;
                rodInBeaker4.tag = "Untagged";
                isSmallRodInBeaker4 = true;
            }
        }
    }

    public void removeStirRod(GameObject stirRod)
    {
        // Remove parent so it no longer follows the flask
        stirRod.transform.SetParent(null);
        GameObject usedRodInBeaker = null;
        if (stirRod.transform.name == "Small Stir Rod"){
            usedRodInBeaker = rodInBeaker;
        }
        else if (stirRod.transform.name == "Small Stir Rod 2"){
            usedRodInBeaker = rodInBeaker2;
        }
        else if (stirRod.transform.name == "Small Stir Rod 3"){
            usedRodInBeaker = rodInBeaker3;
        }
        else if (stirRod.transform.name == "Small Stir Rod 4"){
            usedRodInBeaker = rodInBeaker4;
        }
        // Re-enable physics and collisions
        Physics.IgnoreCollision(stirRod.GetComponent<Collider>(), usedRodInBeaker.GetComponent<Collider>(), false);

        Rigidbody rb = stirRod.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
        }

        if (stirRod.transform.name == "Small Stir Rod")
        {
            rodInBeaker.tag = "LiquidHolder";
            rodInBeaker = null;
            isSmallRodInBeaker = false;
        }
        else if (stirRod.transform.name == "Small Stir Rod 2")
        {
            rodInBeaker2.tag = "LiquidHolder";
            rodInBeaker2 = null;
            isSmallRodInBeaker2 = false;
        }
        else if (stirRod.transform.name == "Small Stir Rod 3")
        {
            rodInBeaker3.tag = "LiquidHolder";
            rodInBeaker3 = null;
            isSmallRodInBeaker3 = false;
        }
        else if (stirRod.transform.name == "Small Stir Rod 4")
        {
            rodInBeaker4.tag = "LiquidHolder";
            rodInBeaker4 = null;
            isSmallRodInBeaker4 = false;
        }
        Debug.Log("Because we're picking up small stir rod, boolean is set to false again");
    }

    void handleTongObject()
    {    // Here Tongs are pos.other
        // pickUpScript.other.transform.TransformDirection(testingOffset);
        Vector3 offset = Vector3.zero;

        if (itemHeldByTongs.name == "Erlenmeyer Flask 250")
            offset = pickUpScript.other.transform.TransformDirection(0f,-0.317f,0.1056f);

        if (itemHeldByTongs.name == "Buchner Flask")
            offset = pickUpScript.other.transform.TransformDirection(0f,-0.317f,0.1056f);

        if (itemHeldByTongs.name == "Erlenmeyer Flask 500")
            offset = pickUpScript.other.transform.TransformDirection(0f,-0.39f,0.1056f);



        itemHeldByTongs.transform.position = pickUpScript.other.transform.Find("Tip").position + offset;
    }

    public void dropItemFromTongsCorrectly()
    {
        if (itemHeldByTongs)
        {
            itemHeldByTongs.GetComponent<Rigidbody>().isKinematic = false;
            itemHeldByTongs.GetComponent<Rigidbody>().useGravity = true;
            itemHeldByTongs.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            itemHeldByTongs.layer = itemHeldByTongsLayer;
        }

        pickUpScript.other.transform.Find("Open").gameObject.SetActive(true);
        pickUpScript.other.transform.Find("Closed").gameObject.SetActive(false);
        itemHeldByTongs = null;
    }

    public void dropIronRingCorrectly()
    {
        pickUpScript.other.transform.Find("Screw").gameObject.SetActive(true);
        pickUpScript.other.transform.Find("Ring").gameObject.SetActive(true);
        pickUpScript.other.transform.Find("Ghost").gameObject.SetActive(false);
        pickUpScript.other.GetComponent<BoxCollider>().enabled = true;
        pickUpScript.other.tag = "IronRing";
    }


    public void dropIronMeshCorrectly()
    {
        pickUpScript.other.transform.Find("Real").gameObject.SetActive(true);
        pickUpScript.other.transform.Find("Ghost").gameObject.SetActive(false);
        pickUpScript.other.GetComponent<BoxCollider>().enabled = true;
        pickUpScript.other.tag = "IronMesh";
    }

    public void dropLiquidHolderCorrectly()
    {
        GameObject liquidHolder = pickUpScript.other;
        if (liquidHolder.GetComponent<MeshCollider>())
            liquidHolder.GetComponent<MeshCollider>().enabled = true;
        else
            liquidHolder.GetComponent<Collider>().enabled = true;

        liquidHolder.tag = "LiquidHolder";
    }


    public void SetPippetteSpeed(GameObject pipette, float speed)
    {

        // First find the closest beaker/flask below you
        GameObject pipetteEnd = pipette.transform.Find("Tip")?.gameObject;
        GameObject closestBeakerOrFlask = findClosestItemWithTag("LiquidHolder", pipetteEnd);
        var pipetteTip = pipette.transform.Find("Tip").transform.position; pipetteTip.y = 0f;
        var beakerOrFlask = closestBeakerOrFlask.transform.position; beakerOrFlask.y = 0f;

        float distFromTip = Vector3.Distance(pipetteTip, beakerOrFlask);

        if (closestBeakerOrFlask && distFromTip <= PIPETTE_GRAB_DISTANCE)
        { // We have a beaker or flask within range

            pipette.transform.Find("Tip").GetComponent<ObiEmitter>().speed = 0f;

            // Add or subtract liquid from beaker based on volume within pipette
            if (closestBeakerOrFlask.transform.Find("Liquid"))
            {
                pipetteScript PS = heldPipette.GetComponent<pipetteScript>();
                float realFlowRate = PS.flowRateML;
                liquidScript LS = closestBeakerOrFlask.GetComponent<liquidScript>();
                float amountToAddOrExtract = realFlowRate * Time.deltaTime;

                if (PS.pipetteFlowing) // stop adding liquid if the pipette runs out
                {
                    tryingToPipetteSolid = false;
                    //adds liquid to the beaker and extracts from pipette
                    float pipetteAmountAfterAdding = PS.pipetteVolume - amountToAddOrExtract;
                    if (pipetteAmountAfterAdding > 0)  //makes sure that the pipette does not give more than it has
                    {
                        //transfers liquid from the pipette to the beaker
                        PS.pipetteVolume -= amountToAddOrExtract;
                        LS.addSolution(PS.pipetteSolution, amountToAddOrExtract);
                        closestBeakerOrFlask.GetComponent<Rigidbody>().AddForce(Vector3.up * 0.0001f, ForceMode.Impulse);
                    }
                    else
                    {
                        //transfers remaining liquid from pipette to beaker
                        LS.addSolution(PS.pipetteSolution, PS.pipetteVolume);
                        PS.pipetteVolume = 0f;
                        closestBeakerOrFlask.GetComponent<Rigidbody>().AddForce(Vector3.up * 0.0001f, ForceMode.Impulse);
                    }
                }
                else if (PS.pipetteExtracting)
                {
                    if (LS.liquidPercent > 0.5){
                        tryingToPipetteSolid = false;
                        //Sets the liquid type in the pipette to the liquid type in the beaker (the liquid type will not change inside the pipette)
                        PS.pipetteSolution = LS.solutionMakeup;
                        heldPipette.GetComponent<liquidScript>().solutionMakeup = LS.solutionMakeup;

                        //Extracts liquid from the beaker into the pipette
                        float beakerAmountAfterExtracting = LS.currentVolume_mL - amountToAddOrExtract;
                        if (beakerAmountAfterExtracting > 0f && PS.pipetteMaxVolume > PS.pipetteVolume + amountToAddOrExtract) //checks if the beaker has liquid and the pipette has room
                        {
                            //transfers liquid from the beaker to the pipette
                            LS.currentVolume_mL -= amountToAddOrExtract;
                            PS.pipetteVolume += amountToAddOrExtract;
                            closestBeakerOrFlask.GetComponent<Rigidbody>().AddForce(Vector3.up * 0.0001f, ForceMode.Impulse);
                        }
                        else
                        {
                            // transfers remaining liquid from the beaker to the pipette
                            float amountToFillPipette = PS.pipetteMaxVolume - PS.pipetteVolume;
                            if (LS.currentVolume_mL > (amountToFillPipette)) // checks to see what the limiting factor is: the beaker or the pipette
                            {
                                LS.currentVolume_mL -= amountToFillPipette;
                                PS.pipetteVolume += amountToFillPipette;
                                closestBeakerOrFlask.GetComponent<Rigidbody>().AddForce(Vector3.up * 0.0001f, ForceMode.Impulse);
                            }
                        }
                    }
                    else{
                        tryingToPipetteSolid = true;
                    }
                }
            }
        }

        // We arent within range of a liquid holder
        else
        {
            pipette.transform.Find("Tip").GetComponent<ObiEmitter>().speed = speed;
            heldPipette.GetComponent<pipetteScript>().pipetteVolume -= Time.deltaTime;
            tryingToPipetteSolid = false;
        }
    }

    void lightUpBeaker()
    {
        GameObject pipetteEnd = pickUpScript.other;
        if (pickUpScript.other.name == "Pipette" || pickUpScript.other.name == "Scoopula")
        {
            pipetteEnd = pickUpScript.other.transform.Find("Tip")?.gameObject;
        }

        GameObject closestBeakerOrFlask = findClosestItemWithTag("LiquidHolder", pipetteEnd);
        if (closestBeakerOrFlask == null) return;

        // Get positions and zero out Y-axis
        Vector3 pipetteTip = pickUpScript.other.transform.position;
        if (pickUpScript.other.name == "Pipette" || pickUpScript.other.name == "Scoopula")
        {
            pipetteTip = pickUpScript.other.transform.Find("Tip").position;
        }
        pipetteTip.y = 0f;
        Vector3 beakerOrFlask = closestBeakerOrFlask.transform.position;
        beakerOrFlask.y = 0f;

        float distFromTip = Vector3.Distance(pipetteTip, beakerOrFlask);

        // Find "allLiquidHolders" object
        GameObject allLiquidHolders = GameObject.Find("allLiquidHolders");
        if (allLiquidHolders == null) return; // Avoid errors if it's missing

        float distanceAllowed = PIPETTE_GRAB_DISTANCE;
        if (pickUpScript.other.name != "Pipette")
        {
            distanceAllowed = distanceAllowed * 5f;
        }

        foreach (Transform liquidHolder in allLiquidHolders.transform)
        {
            // Retrieve the Liquid script to check the Indrawer flag
            liquidScript liquidScript = liquidHolder.GetComponent<liquidScript>();
            if (liquidScript != null && liquidScript.InDrawer) continue; // Skip if the liquid holder is in the drawer

            if (liquidHolder.name.StartsWith("Erlenmeyer Flask"))
            {
                foreach (Transform flaskChild in liquidHolder.transform)
                {
                    if (flaskChild.name.StartsWith("Glass Funnel"))
                    {
                        foreach (Transform funnelChild in flaskChild.transform)
                        {
                            if (funnelChild.name.StartsWith("Paper Cone"))
                            {
                                GameObject whiteOutline = funnelChild.GetChild(0).gameObject;
                                bool isClosest = funnelChild.gameObject == closestBeakerOrFlask && distFromTip <= distanceAllowed;
                                whiteOutline.SetActive(isClosest);
                            }
                        }
                    }
                }
            }

            if (liquidHolder.name.StartsWith("Buchner Flask"))
            {
                foreach (Transform flaskChild in liquidHolder.transform)
                {
                    if (flaskChild.name.StartsWith("Buchner Funnel"))
                    {
                        foreach (Transform funnelChild in flaskChild.transform)
                        {
                            if (funnelChild.name.StartsWith("Paper Cone"))
                            {
                                GameObject whiteOutline = funnelChild.GetChild(0).gameObject;
                                bool isClosest = funnelChild.gameObject == closestBeakerOrFlask && distFromTip <= distanceAllowed;
                                whiteOutline.SetActive(isClosest);
                            }
                        }
                    }
                }
            }

            if (liquidHolder.childCount > 0) // Ensure it has children
            {
                GameObject whiteOutline = liquidHolder.GetChild(0).gameObject;
                bool isClosest = liquidHolder.gameObject == closestBeakerOrFlask && distFromTip <= distanceAllowed;
                whiteOutline.SetActive(isClosest);
            }
        }
    }

    public void turnOffBeakers()
    {
        GameObject allLiquidHolders = GameObject.Find("allLiquidHolders");
        if (allLiquidHolders == null) return; // Avoid errors if it's missing

        foreach (Transform liquidHolder in allLiquidHolders.transform)
        {
            if (liquidHolder.childCount > 0) // Ensure it has children
            {
                GameObject whiteOutline = liquidHolder.GetChild(0).gameObject;
                whiteOutline.SetActive(false);
            }
        }
    }


    GameObject findClosestItemWithTag(string Tag, GameObject pipette)
    {
        float minDist = Mathf.Infinity;
        GameObject closestObject = null;

        foreach (GameObject currentObject in FindObjectsOfType<GameObject>())
        {
            if (currentObject.tag == Tag && currentObject != pipette)
            {

                var pipetteTip = pipette.transform.position; pipetteTip.y = 0f;
                var beakerOrFlask = currentObject.transform.position; beakerOrFlask.y = 0f;

                float distFromTip = Vector3.Distance(pipetteTip, beakerOrFlask);

                if (distFromTip < minDist)
                {
                    minDist = distFromTip;
                    closestObject = currentObject;
                }
            }
        }
        return closestObject;
    }

    public void CheckForIronStandNearby(GameObject ironRing)
    {
        float minDist = float.MaxValue;
        closestIronStand = null;

        foreach (GameObject currentObject in GameObject.FindGameObjectsWithTag("IronStand"))
        {
            var ironRingPos = ironRing.transform.position; ironRingPos.y = 0f;
            var ironStandPos = currentObject.transform.position; ironStandPos.y = 0f;

            float distFromRing = Vector3.Distance(ironRingPos, ironStandPos);

            if (distFromRing < minDist)
            {
                minDist = distFromRing;
                closestIronStand = currentObject;
            }
        }

        float yDist = Vector3.Distance(ironRing.transform.Find("Pivot").position, closestIronStand?.transform.Find("Base").position ?? Vector3.zero);

        if (closestIronStand == null || minDist > IRON_RING_SNAP_DISTANCE || yDist > 1.35f)
        {
            closestIronStand = null;
            ironRing.transform.Find("Screw").gameObject.SetActive(true);
            ironRing.transform.Find("Ring").gameObject.SetActive(true);
            ironRing.transform.Find("Ghost").gameObject.SetActive(false);
            ironRing.GetComponent<BoxCollider>().enabled = true;
        }
        else
        {
            ironRing.transform.Find("Screw").gameObject.SetActive(false);
            ironRing.transform.Find("Ring").gameObject.SetActive(false);
            ironRing.transform.Find("Ghost").gameObject.SetActive(true);
            ironRing.GetComponent<BoxCollider>().enabled = false;

            var ghostRestingPoint = closestIronStand.transform.Find("Base").position;
            ghostRestingPoint += closestIronStand.transform.up * yDist;

            ironRing.transform.Find("Ghost").gameObject.transform.position = ghostRestingPoint;
        }
    }

    void SnapIronRingToStand()
    {
        GameObject ironRing = pickUpScript.other;

        if (ironRing.transform.Find("Ghost").gameObject.activeInHierarchy) // Ready to snap and right-clicked
        {
            Vector3 realMeshOffset = ironRing.transform.Find("Ghost").Find("Ghost Screw").position - ironRing.transform.Find("Screw").position;


            ironRing.transform.Find("Screw").gameObject.SetActive(true);
            ironRing.transform.Find("Ring").gameObject.SetActive(true);
            ironRing.transform.Find("Ghost").gameObject.SetActive(false);
            ironRing.GetComponent<BoxCollider>().enabled = true;

            ironRing.GetComponent<Rigidbody>().isKinematic = true; // Set to kinematic

            GameObject temp = pickUpScript.other;
            pickUpScript.DropItem(); // Probably the only way
            temp.tag = "IronRing";

            ironRing.transform.SetParent(closestIronStand.transform);
            ironRing.transform.Find("Screw").position += realMeshOffset;
            ironRing.transform.Find("Ring").position += realMeshOffset;
            ironRing.GetComponent<BoxCollider>().center = ironRing.transform.Find("Ring").localPosition;
            
        }
    }

    public void CheckForIronRingNearby(GameObject ironMesh)
    {
        float minDist = float.MaxValue;
        closestIronRing = null;

        foreach (GameObject currentObject in GameObject.FindGameObjectsWithTag("IronRing"))
        {
            Vector3 ironMeshPos = ironMesh.transform.position;
            Vector3 ironRingPos = currentObject.transform.position;

            float distFromMesh = Vector3.Distance(ironMeshPos, ironRingPos);

            if (distFromMesh < minDist)
            {
                minDist = distFromMesh;
                closestIronRing = currentObject;
            }
        }

        // if (closestIronRing.transform.parent == null)
        //     return;

        if (closestIronRing == null || minDist > IRON_RING_SNAP_DISTANCE)
        {
            closestIronRing = null;
            ironMesh.transform.Find("Real").gameObject.SetActive(true);
            ironMesh.transform.Find("Ghost").gameObject.SetActive(false);
            ironMesh.GetComponent<BoxCollider>().enabled = false;
        }
        else if (closestIronRing.transform.parent != null && closestIronRing.transform.parent.name != "Science Gear")
        {
            ironMesh.transform.Find("Real").gameObject.SetActive(false);
            ironMesh.transform.Find("Ghost").gameObject.SetActive(true);
            ironMesh.GetComponent<BoxCollider>().enabled = true;

            Vector3 ghostRestingPoint = closestIronRing.transform.Find("Ring/Center").position;

            ironMesh.transform.Find("Ghost").position = ghostRestingPoint;
            ironMesh.transform.Find("Ghost").localEulerAngles = Vector3.zero;
        }
    }

    public void SnapIronMeshToRing()
    {
        GameObject ironMesh = pickUpScript.other;
        
        if (ironMesh.transform.Find("Ghost").gameObject.activeInHierarchy)
        {
            ironMesh.transform.Find("Real").gameObject.SetActive(true);
            ironMesh.transform.Find("Ghost").gameObject.SetActive(false);
            ironMesh.GetComponent<BoxCollider>().enabled = true;
            ironMesh.GetComponent<Rigidbody>().isKinematic = true;

            Vector3 localPosition = closestIronRing.transform.Find("Ring").localPosition +
                                    closestIronRing.transform.Find("Ring/Center").localPosition;

            GameObject temp = pickUpScript.other;
            pickUpScript.DropItem(); // Probably the only way
            temp.tag = "IronMesh";

            // Set the position of the iron mesh
            ironMesh.transform.position = closestIronRing.transform.Find("Ring/Center").position;
            ironMesh.transform.SetParent(closestIronRing.transform);

            Debug.Log("Iron Mesh Snapped to Ring");
            Debug.Log($"Iron Mesh Position: {ironMesh.transform.localPosition}");
            MeshSnapped = true;
        }
    }

    public void CheckForIronMeshNearby(GameObject liquidHolder)
    {
        float minDist = float.MaxValue;
        closestIronMesh = null;
        IsNearIronMesh = false;

        foreach (GameObject currentObject in GameObject.FindGameObjectsWithTag("IronMesh"))
        {
            Vector3 liquidHolderPos = liquidHolder.transform.position;
            Vector3 ironMeshPos = currentObject.transform.position;

            float distFromHolder = Vector3.Distance(liquidHolderPos, ironMeshPos);

            if (distFromHolder < minDist)
            {
                minDist = distFromHolder;
                closestIronMesh = currentObject;
            }
        }

        if (closestIronMesh == null || minDist > IRON_MESH_SNAP_DISTANCE)
        {
            closestIronMesh = null;
            IsNearIronMesh = false;
        }
        else
        {
            IsNearIronMesh = true;
        }
    }
    void SnapLiquidHolderToIronMesh()
    {
        GameObject liquidHolder = pickUpScript.other;

        if (MeshSnapped == true)
        {
            if (liquidHolder == null)
            {
                return;
            }
            if (closestIronMesh == null)
            {
                return;
            }

            // Find the snap point on the IronMesh
            Transform attachmentPoint = closestIronMesh.transform.Find("White");

            // Default Y offset
            float yOffset = 0.14f;

            // Adjust Y offset based on the name of the LiquidHolder (e.g., beaker size, flask type)
            string holderName = liquidHolder.name.ToLower();

            if (holderName.Contains("graduated cylinder"))
                return;

            if (holderName.Contains("buchner flask"))
                return;

            if (holderName.Contains("Capilary tube"))
                return;

            if (holderName.Contains("beaker"))
            {
                
                if (holderName.Contains("400ml"))
                    yOffset = 0.14f;  
                else if (holderName.Contains("250ml"))
                    yOffset = 0.115f; 
                else if (holderName.Contains("150ml"))
                    yOffset = 0.1f; 
                else if (holderName.Contains("100ml"))
                    yOffset = 0.09f; 
                else if (holderName.Contains("800ml"))
                    yOffset = 0.18f;
                else if (holderName.Contains("50ml"))
                    yOffset = 0.07f;
            }
            else if (holderName.Contains("erlenmeyer flask"))
            {
                if (holderName.Contains("500"))
                    yOffset = 0.005f;
                else if (holderName.Contains("250"))
                    yOffset = 0.005f; 
            }

            // Adjust position and rotation based on attachment point
            if (attachmentPoint != null)
            {
                Debug.Log("1");
                liquidHolder.transform.position = new Vector3(attachmentPoint.position.x, attachmentPoint.position.y + yOffset, attachmentPoint.position.z);
                liquidHolder.transform.rotation = attachmentPoint.rotation;
            }
            else
            {
                Debug.Log("2");
                liquidHolder.transform.position = new Vector3(closestIronMesh.transform.position.x, closestIronMesh.transform.position.y + yOffset, closestIronMesh.transform.position.z);
                liquidHolder.transform.rotation = Quaternion.identity;
            }

            liquidHolder.transform.SetParent(closestIronMesh.transform);

            // Drop from pickup script so it's no longer "held"
            pickUpScript.DropItem();

            Rigidbody rb = liquidHolder.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Debug.Log("3");
                rb.isKinematic = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            Debug.Log("LiquidHolder snapped to IronMesh!");
        }
    }


    void faceItemAwayFromPlayer()
    {
        pickUpScript.targetRotation.y = transform.localEulerAngles.y;           // When right clicked, face item away from player
    }

    void manipulateBunsenBurner()
    {
        pickUpScript.targetX = 85f;
        pickUpScript.canZoomIn = false;
        pickUpScript.canRotateItem = false;
    }

    void resetRotationOffset()
    {
        pickUpScript.targetX = 0f;
        pickUpScript.canZoomIn = true;
        pickUpScript.canRotateItem = true;
    }

/// <summary>
/// //////As pertains to the scoopula: I just want yall to know that I know that this code is bad and not efficient but it works like a charm so dont touch it pls :)
/// </summary>/
    void GatherAluminumPelletsFromContainerOrDropThem()
    {
        GameObject scoopula = pickUpScript.other;

        // Find Closest Aluminum Container in the room
        float minDist = Mathf.Infinity;
        GameObject closestScoopableObject = null;

        foreach (GameObject currentObject in FindObjectsOfType<GameObject>())
        {
            if (currentObject.name == "Aluminum Container" || currentObject.name == "Paper Cone" || currentObject.name.StartsWith("Beaker") || currentObject.name == "Weigh Boat" || currentObject.name.StartsWith("Paper Towel"))
            {
                float distFromTip = Vector3.Distance(scoopula.transform.position, currentObject.transform.position);

                if (distFromTip < minDist)
                {
                    minDist = distFromTip;
                    closestScoopableObject = currentObject;
                }
            } 
        }

        if (closestScoopableObject && minDist <= SCOOPULA_GRAB_DISTANCE) // Now we have closest object
        {
            if (!scoopulaAnimationPlaying)
            {
                if ((scoopula.transform.Find("Aluminum").gameObject.activeInHierarchy || scoopula.transform.Find("Sugar Powder").gameObject.activeInHierarchy) && closestScoopableObject.name != "Aluminum Container"){ //drop the aluminum if there is aluminum to drop and the target container is not the aluminum container
                    StartCoroutine(scoopulaDip(scoopula, closestScoopableObject));
                }
                else if (closestScoopableObject.name == "Aluminum Container"){ //get aluminum out of aluminum container (with unscrewing)
                    StartCoroutine(getAluminumUsingScoopula(closestScoopableObject));
                }
                else if (closestScoopableObject.GetComponent<weighboatscript>()){ // get aluminum out of weighboat
                    if (closestScoopableObject.GetComponent<weighboatscript>().scoopsHeld > 0){
                        //Debug.Log("retrieving aluminum from weighboat");
                        if (closestScoopableObject.GetComponent<weighboatscript>().solutionMakeup.SequenceEqual(new List<float> { 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f })){  // get aluminum
                            //Debug.Log("for real");
                            StartCoroutine(pickUpAluminum(closestScoopableObject, scoopula));
                        }
                        else if (closestScoopableObject.GetComponent<weighboatscript>().solutionMakeup != new List<float> { 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f }){  //get arbitrary substance
                            StartCoroutine(pickUpArbitrarySolid(closestScoopableObject, scoopula));
                        }
                    }
                }
                else if (closestScoopableObject.GetComponent<liquidScript>()){
                    if (closestScoopableObject.GetComponent<liquidScript>().liquidPercent < 0.1f){
                        StartCoroutine(pickUpArbitrarySolidFromArbitraryContainer(closestScoopableObject, scoopula));
                    }
                }
            }
        }
    }

    IEnumerator pickUpAluminum(GameObject closestScoopableObject, GameObject scoopula){
        float speedMult = 1 / 2f;

        //scoopula dips down
        StartCoroutine(LerpValue(value => GetComponent<pickUpObjects>().targetX = value, 0f, 30f, 1.2f * speedMult));
        yield return new WaitForSeconds(1.2f * speedMult);
        //scoopula picks up aluminum
        scoopula.transform.Find("Aluminum").gameObject.SetActive(true);
        //scoopula.transform.Find("Sugar Powder").gameObject.SetActive(false);
        //closestWeighBoat.GetComponent<liquidScript>().addSolution(new List<float> { 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f }, 0.7407f);  // Add 0.37 mL of Aluminum
        Debug.Log(scoopula.GetComponent<ScoopulaScript>().solutionMakeup);
        Debug.Log(closestScoopableObject.GetComponent<weighboatscript>().solutionMakeup);
        scoopula.GetComponent<ScoopulaScript>().solutionMakeup = closestScoopableObject.GetComponent<weighboatscript>().solutionMakeup;
        closestScoopableObject.GetComponent<weighboatscript>().removeScoop();
        closestScoopableObject.GetComponent<Rigidbody>().AddForce(Vector3.up * 0.0001f, ForceMode.Impulse);
        //scoopula returns to original position
        yield return new WaitForSeconds(0.7f * speedMult);
        StartCoroutine(LerpValue(value => GetComponent<pickUpObjects>().targetX = value, 30f, 0f, 0.5f * speedMult));
        yield return new WaitForSeconds(0.8f * speedMult);
    }

    IEnumerator pickUpArbitrarySolid(GameObject closestScoopableObject, GameObject scoopula){
        float speedMult = 1 / 4f;

        //scoopula dips down
        StartCoroutine(LerpValue(value => GetComponent<pickUpObjects>().targetX = value, 0f, 30f, 1.2f * speedMult));
        yield return new WaitForSeconds(1.2f * speedMult);
        //scoopula picks up aluminum
        scoopula.transform.Find("Aluminum").gameObject.SetActive(false);
        scoopula.transform.Find("Sugar Powder").gameObject.SetActive(true);

        scoopula.GetComponent<ScoopulaScript>().solutionMakeup = closestScoopableObject.GetComponent<weighboatscript>().solutionMakeup;
        //closestWeighBoat.GetComponent<liquidScript>().addSolution(new List<float> { 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f }, 0.7407f);  // Add 0.37 mL of Aluminum
        closestScoopableObject.GetComponent<weighboatscript>().removeScoop();
        closestScoopableObject.GetComponent<Rigidbody>().AddForce(Vector3.up * 0.0001f, ForceMode.Impulse);
        //scoopula returns to original position
        yield return new WaitForSeconds(0.7f * speedMult);
        StartCoroutine(LerpValue(value => GetComponent<pickUpObjects>().targetX = value, 30f, 0f, 0.5f * speedMult));
        yield return new WaitForSeconds(0.8f * speedMult);
    }

    IEnumerator pickUpArbitrarySolidFromArbitraryContainer(GameObject closestScoopableObject, GameObject scoopula){
        float speedMult = 1 / 4f;

        //scoopula dips down
        StartCoroutine(LerpValue(value => GetComponent<pickUpObjects>().targetX = value, 0f, 30f, 1.2f * speedMult));
        yield return new WaitForSeconds(1.2f * speedMult);
        //scoopula picks up aluminum
        scoopula.transform.Find("Aluminum").gameObject.SetActive(false);
        scoopula.transform.Find("Sugar Powder").gameObject.SetActive(true);
        //closestWeighBoat.GetComponent<liquidScript>().addSolution(new List<float> { 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f }, 0.7407f);  // Add 0.37 mL of Aluminum
        scoopula.GetComponent<ScoopulaScript>().solutionMakeup = closestScoopableObject.GetComponent<liquidScript>().solutionMakeup;
        closestScoopableObject.GetComponent<liquidScript>().currentVolume_mL -= 0.1852f;
        closestScoopableObject.GetComponent<Rigidbody>().AddForce(Vector3.up * 0.0001f, ForceMode.Impulse);
        //scoopula returns to original position
        yield return new WaitForSeconds(0.7f * speedMult);
        StartCoroutine(LerpValue(value => GetComponent<pickUpObjects>().targetX = value, 30f, 0f, 0.5f * speedMult));
        yield return new WaitForSeconds(0.8f * speedMult);
    }

    IEnumerator getAluminumUsingScoopula(GameObject container)
    {
        scoopulaAnimationPlaying = true;
        
        // Reference to scoopula
        GameObject scoopula = GetComponent<pickUpObjects>().other;
    
        // If scoopula is null (dropped), stop coroutine
        if (scoopula == null)
            yield break;
    
        Transform cap = container.transform.Find("Cap");
    
        GetComponent<playerMovement>().canMove = false;
        GetComponent<playerMovement>().canTurn = false;
        GetComponent<pickUpObjects>().canRotateItem = false;
    
        if (cap == null)
            yield break; // Stop the coroutine if cap is not found
    
        float speedMult = 1 / 5f;
        Vector3 startPos = cap.position;
        Vector3 targetPos = startPos + cap.parent.up * 0.08f;
        Vector3 leftPos = targetPos - cap.parent.right * 0.14f;
        float duration = 0.8f;
        float rotationSpeed = 240f;
    
        // First movement check
        if (scoopula == null) yield break;
        yield return StartCoroutine(MoveAndRotateOverTime(cap, startPos, targetPos, duration * speedMult, -rotationSpeed, Vector3.zero));
    
        if (scoopula == null) yield break;
        yield return new WaitForSeconds(duration * speedMult);
    
        Vector3 tiltRotation = new Vector3(0f, 0f, 70f);
        
        if (scoopula == null) yield break;
        yield return StartCoroutine(MoveAndRotateOverTime(cap, targetPos, leftPos, duration * speedMult, 0, tiltRotation));
    
        if (scoopula == null) yield break;
        StartCoroutine(LerpValue(value => GetComponent<pickUpObjects>().targetX = value, 0f, 30f, 1.2f * speedMult));
        yield return new WaitForSeconds(1.2f * speedMult);
    
        if (scoopula == null) yield break;
        scoopula.transform.Find("Aluminum").gameObject.SetActive(true);
        scoopula.GetComponent<ScoopulaScript>().solutionMakeup = new List<float> {0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f};
    
        if (scoopula == null) yield break;
        yield return new WaitForSeconds(0.7f * speedMult);
        
        if (scoopula == null) yield break;
        StartCoroutine(LerpValue(value => GetComponent<pickUpObjects>().targetX = value, 30f, 0f, 0.5f * speedMult));
        yield return new WaitForSeconds(0.8f * speedMult);
    
        Vector3 tiltRotationBack = new Vector3(0f, 0f, -70f);
        
        if (scoopula == null) yield break;
        yield return StartCoroutine(MoveAndRotateOverTime(cap, leftPos, targetPos, duration * speedMult, 0, tiltRotationBack));
    
        if (scoopula == null) yield break;
        yield return new WaitForSeconds(duration * speedMult);
    
        if (scoopula == null) yield break;
        yield return StartCoroutine(MoveAndRotateOverTime(cap, targetPos, startPos, duration * speedMult, rotationSpeed, Vector3.zero));
    
        if (scoopula == null) yield break;
        yield return new WaitForSeconds(duration * speedMult);
    
        // Reset animation state
        scoopulaAnimationPlaying = false;
        cap.localPosition = new Vector3(0f, 0.3299f, 0f);
        cap.localEulerAngles = new Vector3(0f, 0f, 0f);
        GetComponent<playerMovement>().canMove = true;
        GetComponent<playerMovement>().canTurn = true;
        GetComponent<pickUpObjects>().canRotateItem = true;
    }

    IEnumerator scoopulaDip(GameObject scoopula, GameObject closestWeighBoat){
        float speedMult = 1 / 4f;
        tryingToMixCompoundsInNonLiquidHolder = false;
        //scoopula dips down
        StartCoroutine(LerpValue(value => GetComponent<pickUpObjects>().targetX = value, 0f, 30f, 1.2f * speedMult));
        yield return new WaitForSeconds(1.2f * speedMult);

        //scoopula drops pellets
        if (scoopula.transform.Find("Aluminum").gameObject.activeInHierarchy){
            if (closestWeighBoat.GetComponent<weighboatscript>()){ //object is a weigh boat
                for (int i = 0; i < scoopula.GetComponent<ScoopulaScript>().solutionMakeup.Count; i++){
                    Debug.Log(scoopula.GetComponent<ScoopulaScript>().solutionMakeup[i]);
                }
                if (scoopula.GetComponent<ScoopulaScript>().solutionMakeup.SequenceEqual(closestWeighBoat.GetComponent<weighboatscript>().solutionMakeup) || closestWeighBoat.GetComponent<weighboatscript>().solutionMakeup.All(num => num == 0f))
                    closestWeighBoat.GetComponent<weighboatscript>().addScoop(new List<float> { 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f});
                else{
                    tryingToMixCompoundsInNonLiquidHolder = true;
                }
            }
            else{ //object is a beaker or other
                    closestWeighBoat.GetComponent<liquidScript>().addSolution(new List<float> { 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, 0f}, 0.1852f);  // Add 0.37 mL of Aluminum
            }
            if (!tryingToMixCompoundsInNonLiquidHolder){
                scoopula.transform.Find("Aluminum").gameObject.SetActive(false);
            }
        }

        if (scoopula.transform.Find("Sugar Powder").gameObject.activeInHierarchy){
            if (closestWeighBoat.GetComponent<weighboatscript>()){ //object is a weigh boat
                if (scoopula.GetComponent<ScoopulaScript>().solutionMakeup.SequenceEqual(closestWeighBoat.GetComponent<weighboatscript>().solutionMakeup) || closestWeighBoat.GetComponent<weighboatscript>().solutionMakeup.All(num => num == 0f)){
                    closestWeighBoat.GetComponent<weighboatscript>().addScoop(scoopula.GetComponent<ScoopulaScript>().solutionMakeup);
                }
                else{
                    tryingToMixCompoundsInNonLiquidHolder = true;
                }
            }
            else{ //object is a beaker or other
                closestWeighBoat.GetComponent<liquidScript>().addSolution(scoopula.GetComponent<ScoopulaScript>().solutionMakeup, 0.1852f);  // Add 0.37 mL of arbitrary solution
            }
            scoopula.transform.Find("Sugar Powder").gameObject.SetActive(false);
        }

        closestWeighBoat.GetComponent<Rigidbody>().AddForce(Vector3.up * 0.0001f, ForceMode.Impulse);

        //scoopula returns to original position
        yield return new WaitForSeconds(0.7f * speedMult);
        StartCoroutine(LerpValue(value => GetComponent<pickUpObjects>().targetX = value, 30f, 0f, 0.5f * speedMult));
        yield return new WaitForSeconds(0.8f * speedMult);
    }

    IEnumerator MoveAndRotateOverTime(Transform obj, Vector3 start, Vector3 end, float duration, float rotationSpeed, Vector3 targetRotation)
    {
        float elapsedTime = 0f;
        Quaternion startRotation = obj.rotation;
        Quaternion endRotation = obj.rotation * Quaternion.Euler(targetRotation);

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            obj.position = Vector3.Lerp(start, end, t);

            // Rotate smoothly if a rotation is applied
            if (rotationSpeed != 0)
                obj.Rotate(obj.transform.parent.up, rotationSpeed * Time.deltaTime);
            else
                obj.rotation = Quaternion.Lerp(startRotation, endRotation, t); // Tilt smoothly

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        obj.position = end; // Ensure final position is set
        obj.rotation = endRotation; // Ensure final rotation is set
    }

    IEnumerator LerpValue(System.Action<float> setValue, float start, float end, float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            setValue(Mathf.Lerp(start, end, t));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        setValue(end); // Ensure it reaches the exact target value
    }


    public void givePlayerPaperTowelSheet(Transform paperTowelRoll){
        Debug.Log("Give Paper Towel");
        paperTowelRoll.gameObject.GetComponent<AudioSource>().Play();
        GameObject sheet = Instantiate(paperTowelSheet, paperTowelRoll.position - transform.forward * 0.15f, transform.rotation);
        GetComponent<pickUpObjects>().PickUpItem(sheet);


    }

    void ShootFoam()
    {
        if (pickUpScript.other != null && pickUpScript.other.name == "Fire extinguisher")
        {
            ParticleSystem foam = pickUpScript.other.transform.Find("Foam").GetComponent<ParticleSystem>();

            if (!foam.isPlaying)
            {
                TriggerFoam(pickUpScript.other);
                // Extinguish Fires
                StartCoroutine(extinguishFiresConstantly());
            }
        }
    }

    IEnumerator extinguishFiresConstantly(){
        if (!pickUpScript.other)
            yield break;
        for (int i = 0; i < 80; i++)
        {
            ExtinguishFires(pickUpScript.other.transform);
            yield return new WaitForSeconds(0.1f);
        }
    }

    void ExtinguishFires(Transform extinguisher)
    {
        float extinguishRadius = 5f;           // Radius of fire extinguishing
        float extinguishAngle = 45f;           // Angle in front of the extinguisher that will be affected
        LayerMask fireLayer = LayerMask.GetMask("Fire");  // Make sure your fires are on the "Fire" layer

        // Get all fires within the radius
        Collider[] hitColliders = Physics.OverlapSphere(extinguisher.position, extinguishRadius, fireLayer);

        foreach (Collider hit in hitColliders)
        {
            // Check if the hit object is within the extinguishing cone angle
            Vector3 directionToFire = (hit.transform.position - extinguisher.position).normalized;
            float angle = Vector3.Angle(extinguisher.forward, directionToFire);

            if (angle <= extinguishAngle)
            {
                Destroy(hit.gameObject);  // Destroy the fire object immediately
            }
        }
    }

    private void TriggerFoam(GameObject extinguisher)
    {

        ParticleSystem foam = extinguisher.transform.Find("Foam").GetComponent<ParticleSystem>();
        foam.Play();

        StartCoroutine(OnOrOffForDelay(extinguisher.transform.Find("Spraying").gameObject, foam.main.duration));
        StartCoroutine(OnOrOffForDelay(extinguisher.transform.Find("Not Spraying").gameObject, foam.main.duration, false));
    }

    IEnumerator OnOrOffForDelay(GameObject obj, float delayTime, bool initialState = true)
    {
        obj.SetActive(initialState);
        yield return new WaitForSeconds(delayTime);
        obj.SetActive(!initialState);
    }

    void LightMatchAndTossForward(GameObject obj)
    {

        matchBoxScript matchScript = obj.GetComponent<matchBoxScript>();

        if (!matchScript.animationPlaying)
        {

            matchScript.LightMatch();
        }
    }

    /*public void handleBigStirringAnims() {
        if (stirAnimator != null) {
            stirAnimator.enabled = true;
            stirAnimator.SetBool("IsStirring", true);
            if (rodInBeaker != null && rodInBeaker.name == "Beaker 800mL") {
                stirAnimator.SetBool("is800", true);
            } else if (rodInBeaker != null && rodInBeaker.name == "Beaker 400mL") {
                stirAnimator.SetBool("is400", true);
            } else {
                Debug.Log("Big stir animator is null");
            }
        }
    } */

    public void handleSmallStirringAnims() {
        if (smallStirAnimator != null) {
            smallStirAnimator.enabled = true;
            smallStirAnimator.SetBool("currentlyStirring", true);
            if (rodInBeaker != null && rodInBeaker.name == "Beaker 800mL") {
                smallStirAnimator.SetBool("is50", false);
                smallStirAnimator.SetBool("is100", false);
                smallStirAnimator.SetBool("is150", false);
                smallStirAnimator.SetBool("is250", false);
                smallStirAnimator.SetBool("is400", false);
                smallStirAnimator.SetBool("is800", true);
            } else if (rodInBeaker != null && rodInBeaker.name == "Beaker 400mL") {
                smallStirAnimator.SetBool("is50", false);
                smallStirAnimator.SetBool("is100", false);
                smallStirAnimator.SetBool("is150", false);
                smallStirAnimator.SetBool("is250", false);
                smallStirAnimator.SetBool("is800", false);
                smallStirAnimator.SetBool("is400", true);
            } else if (rodInBeaker != null && rodInBeaker.name == "Beaker 250mL") {
                smallStirAnimator.SetBool("is50", false);
                smallStirAnimator.SetBool("is100", false);
                smallStirAnimator.SetBool("is150", false);
                smallStirAnimator.SetBool("is400", false);
                smallStirAnimator.SetBool("is800", false);
                smallStirAnimator.SetBool("is250", true);
            } else if (rodInBeaker != null && rodInBeaker.name == "Beaker 150mL") {
                smallStirAnimator.SetBool("is50", false);
                smallStirAnimator.SetBool("is100", false);
                smallStirAnimator.SetBool("is250", false);
                smallStirAnimator.SetBool("is400", false);
                smallStirAnimator.SetBool("is800", false);
                smallStirAnimator.SetBool("is150", true);
            } else if (rodInBeaker != null && rodInBeaker.name == "Beaker 100mL") {
                smallStirAnimator.SetBool("is50", false);
                smallStirAnimator.SetBool("is150", false);
                smallStirAnimator.SetBool("is250", false);
                smallStirAnimator.SetBool("is400", false);
                smallStirAnimator.SetBool("is800", false);
                smallStirAnimator.SetBool("is100", true);
            } else if (rodInBeaker != null && rodInBeaker.name == "Beaker 50mL") {
                smallStirAnimator.SetBool("is100", false);
                smallStirAnimator.SetBool("is150", false);
                smallStirAnimator.SetBool("is250", false);
                smallStirAnimator.SetBool("is400", false);
                smallStirAnimator.SetBool("is800", false);
                smallStirAnimator.SetBool("is50", true);
            } else {
                Debug.Log("Small stir animator is null");
            }
        }
    }
    public void handleSmallStirringAnims2() {
        if (smallStirAnimator2 != null) {
            smallStirAnimator2.enabled = true;
            smallStirAnimator2.SetBool("currentlyStirring", true);
            if (rodInBeaker2 != null && rodInBeaker2.name == "Beaker 800mL") {
                smallStirAnimator2.SetBool("is50", false);
                smallStirAnimator2.SetBool("is100", false);
                smallStirAnimator2.SetBool("is150", false);
                smallStirAnimator2.SetBool("is250", false);
                smallStirAnimator2.SetBool("is400", false);
                smallStirAnimator2.SetBool("is800", true);
            } else if (rodInBeaker2 != null && rodInBeaker2.name == "Beaker 400mL") {
                smallStirAnimator2.SetBool("is50", false);
                smallStirAnimator2.SetBool("is100", false);
                smallStirAnimator2.SetBool("is150", false);
                smallStirAnimator2.SetBool("is250", false);
                smallStirAnimator2.SetBool("is800", false);
                smallStirAnimator2.SetBool("is400", true);
            } else if (rodInBeaker2 != null && rodInBeaker2.name == "Beaker 250mL") {
                smallStirAnimator2.SetBool("is50", false);
                smallStirAnimator2.SetBool("is100", false);
                smallStirAnimator2.SetBool("is150", false);
                smallStirAnimator2.SetBool("is400", false);
                smallStirAnimator2.SetBool("is800", false);
                smallStirAnimator2.SetBool("is250", true);
            } else if (rodInBeaker2 != null && rodInBeaker2.name == "Beaker 150mL") {
                smallStirAnimator2.SetBool("is50", false);
                smallStirAnimator2.SetBool("is100", false);
                smallStirAnimator2.SetBool("is250", false);
                smallStirAnimator2.SetBool("is400", false);
                smallStirAnimator2.SetBool("is800", false);
                smallStirAnimator2.SetBool("is150", true);
            } else if (rodInBeaker2 != null && rodInBeaker2.name == "Beaker 100mL") {
                smallStirAnimator2.SetBool("is50", false);
                smallStirAnimator2.SetBool("is150", false);
                smallStirAnimator2.SetBool("is250", false);
                smallStirAnimator2.SetBool("is400", false);
                smallStirAnimator2.SetBool("is800", false);
                smallStirAnimator2.SetBool("is100", true);
            } else if (rodInBeaker2 != null && rodInBeaker2.name == "Beaker 50mL") {
                smallStirAnimator2.SetBool("is100", false);
                smallStirAnimator2.SetBool("is150", false);
                smallStirAnimator2.SetBool("is250", false);
                smallStirAnimator2.SetBool("is400", false);
                smallStirAnimator2.SetBool("is800", false);
                smallStirAnimator2.SetBool("is50", true);
            } else {
                Debug.Log("Small stir animator is null");
            }
        }
    }
    public void handleSmallStirringAnims3() {
        if (smallStirAnimator3 != null) {
            smallStirAnimator3.enabled = true;
            smallStirAnimator3.SetBool("currentlyStirring", true);
            if (rodInBeaker3 != null && rodInBeaker3.name == "Beaker 800mL") {
                smallStirAnimator3.SetBool("is50", false);
                smallStirAnimator3.SetBool("is100", false);
                smallStirAnimator3.SetBool("is150", false);
                smallStirAnimator3.SetBool("is250", false);
                smallStirAnimator3.SetBool("is400", false);
                smallStirAnimator3.SetBool("is800", true);
            } else if (rodInBeaker3 != null && rodInBeaker3.name == "Beaker 400mL") {
                smallStirAnimator3.SetBool("is50", false);
                smallStirAnimator3.SetBool("is100", false);
                smallStirAnimator3.SetBool("is150", false);
                smallStirAnimator3.SetBool("is250", false);
                smallStirAnimator3.SetBool("is800", false);
                smallStirAnimator3.SetBool("is400", true);
            } else if (rodInBeaker3 != null && rodInBeaker3.name == "Beaker 250mL") {
                smallStirAnimator3.SetBool("is50", false);
                smallStirAnimator3.SetBool("is100", false);
                smallStirAnimator3.SetBool("is150", false);
                smallStirAnimator3.SetBool("is400", false);
                smallStirAnimator3.SetBool("is800", false);
                smallStirAnimator3.SetBool("is250", true);
            } else if (rodInBeaker3 != null && rodInBeaker3.name == "Beaker 150mL") {
                smallStirAnimator3.SetBool("is50", false);
                smallStirAnimator3.SetBool("is100", false);
                smallStirAnimator3.SetBool("is250", false);
                smallStirAnimator3.SetBool("is400", false);
                smallStirAnimator3.SetBool("is800", false);
                smallStirAnimator3.SetBool("is150", true);
            } else if (rodInBeaker3 != null && rodInBeaker3.name == "Beaker 100mL") {
                smallStirAnimator3.SetBool("is50", false);
                smallStirAnimator3.SetBool("is150", false);
                smallStirAnimator3.SetBool("is250", false);
                smallStirAnimator3.SetBool("is400", false);
                smallStirAnimator3.SetBool("is800", false);
                smallStirAnimator3.SetBool("is100", true);
            } else if (rodInBeaker3 != null && rodInBeaker3.name == "Beaker 50mL") {
                smallStirAnimator3.SetBool("is100", false);
                smallStirAnimator3.SetBool("is150", false);
                smallStirAnimator3.SetBool("is250", false);
                smallStirAnimator3.SetBool("is400", false);
                smallStirAnimator3.SetBool("is800", false);
                smallStirAnimator3.SetBool("is50", true);
            } else {
                Debug.Log("Small stir animator is null");
            }
        }
    }
    public void handleSmallStirringAnims4() {
        if (smallStirAnimator4 != null) {
            smallStirAnimator4.enabled = true;
            smallStirAnimator4.SetBool("currentlyStirring", true);
            if (rodInBeaker4 != null && rodInBeaker4.name == "Beaker 800mL") {
                smallStirAnimator4.SetBool("is50", false);
                smallStirAnimator4.SetBool("is100", false);
                smallStirAnimator4.SetBool("is150", false);
                smallStirAnimator4.SetBool("is250", false);
                smallStirAnimator4.SetBool("is400", false);
                smallStirAnimator4.SetBool("is800", true);
            } else if (rodInBeaker4 != null && rodInBeaker4.name == "Beaker 400mL") {
                smallStirAnimator4.SetBool("is50", false);
                smallStirAnimator4.SetBool("is100", false);
                smallStirAnimator4.SetBool("is150", false);
                smallStirAnimator4.SetBool("is250", false);
                smallStirAnimator4.SetBool("is800", false);
                smallStirAnimator4.SetBool("is400", true);
            } else if (rodInBeaker4 != null && rodInBeaker4.name == "Beaker 250mL") {
                smallStirAnimator4.SetBool("is50", false);
                smallStirAnimator4.SetBool("is100", false);
                smallStirAnimator4.SetBool("is150", false);
                smallStirAnimator4.SetBool("is400", false);
                smallStirAnimator4.SetBool("is800", false);
                smallStirAnimator4.SetBool("is250", true);
            } else if (rodInBeaker4 != null && rodInBeaker4.name == "Beaker 150mL") {
                smallStirAnimator4.SetBool("is50", false);
                smallStirAnimator4.SetBool("is100", false);
                smallStirAnimator4.SetBool("is250", false);
                smallStirAnimator4.SetBool("is400", false);
                smallStirAnimator4.SetBool("is800", false);
                smallStirAnimator4.SetBool("is150", true);
            } else if (rodInBeaker4 != null && rodInBeaker4.name == "Beaker 100mL") {
                smallStirAnimator4.SetBool("is50", false);
                smallStirAnimator4.SetBool("is150", false);
                smallStirAnimator4.SetBool("is250", false);
                smallStirAnimator4.SetBool("is400", false);
                smallStirAnimator4.SetBool("is800", false);
                smallStirAnimator4.SetBool("is100", true);
            } else if (rodInBeaker4 != null && rodInBeaker4.name == "Beaker 50mL") {
                smallStirAnimator4.SetBool("is100", false);
                smallStirAnimator4.SetBool("is150", false);
                smallStirAnimator4.SetBool("is250", false);
                smallStirAnimator4.SetBool("is400", false);
                smallStirAnimator4.SetBool("is800", false);
                smallStirAnimator4.SetBool("is50", true);
            } else {
                Debug.Log("Small stir animator is null");
            }
        }
    }
}