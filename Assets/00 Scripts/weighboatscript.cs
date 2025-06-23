using Obi;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class weighboatscript : MonoBehaviour
{
    public int maxScoops = 10;
    //this references a list in liquidScript
    public int scoopsHeld = 0;
    List<float> densities = new List<float> {1.83f, 2.12f, 1f, 2.66f, 2.7f, 1.5f, 2.672f, 1.76f, 2.42f, 1.75f, 1.57f, 0.789f};
    public List<float> solutionMakeup = new List<float> {0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f};
    public List<char> compoundStates = new List<char> { 'a', 'a', 'l', 'a', 's', 'a', 's', 's', 's', 'a', 's', 'l'};
    public List<float> molarMasses = new List<float> {98.079f, 56.1056f, 18.01528f, 174.259f, 26.982f, 134.12f, 342.14f, 474.39f, 78f, 464.46f, 98.075f, 46.069f};
    public List<string> compoundNames = new List<string> {"H2SO4", "KOH", "H2O", "K2SO4", "Al", "KAl(OH)4", "Al2(SO4)3", "Alum", "Al(OH)3", "KAl(SO4)2*12H2O", "KAlO2", "CH3CH2OH"};
    public doCertainThingWith doCertainThingWithScript;
    public bool isPouring = false;
    public float density;
    public String meshType = "";

    void Start()
    {
        GameObject player = GameObject.Find("Player");
        doCertainThingWithScript = player.GetComponent<doCertainThingWith>();
        Debug.Log("solution makeup length" + solutionMakeup.Count);
        compoundNames = new List<string> {
        "H<sub>2</sub>SO<sub>4</sub>", "KOH", "H<sub>2</sub>O", "K<sub>2</sub>SO<sub>4</sub>", "Al", "KAl(OH)<sub>4</sub>", "Al<sub>2</sub>(SO<sub>4</sub>)<sub>3</sub>", "Alum", "Al(OH)<sub>3</sub>", "KAl(SO<sub>4</sub>)<sub>2</sub>*12H<sub>2</sub>O", "KAlO<sub>2</sub>", "CH<sub>3</sub>CH<sub>2</sub>OH"
    };
    }

    void Update()
    {
        if (transform.name.StartsWith("Paper Towel") && scoopsHeld > 0){
            List<float> liquidSolution = Enumerable.Repeat(0f, 12).ToList();   //separate out liquid solution
            float liquidPercent = 0f;
            for (int i = 0; i < solutionMakeup.Count; i++)
            {
                if (compoundStates[i] == 'l' || compoundStates[i] == 'a')
                {
                    liquidSolution[i] = solutionMakeup[i];
                    liquidPercent += solutionMakeup[i];
                }
                else
                {
                    liquidSolution[i] = 0f;  // Ensure only liquids are transferred
                }
            }

            float liquidVolume = liquidPercent * 0.1852f * scoopsHeld;  // Calculate volume of liquid part

            // Normalize `liquidSolution` to sum to 100%
            for (int i = 0; i < solutionMakeup.Count; i++)
            {
                if (liquidVolume != 0){  //prevent divide by 0
                    liquidSolution[i] = (liquidSolution[i] * 0.1852f * scoopsHeld) / liquidVolume;
                }
                else{
                    liquidSolution[i] = 0;
                }
            }
            float volumeToRemove = liquidVolume / 100f;
            
            List<float> volumes = Enumerable.Repeat(0f, 12).ToList();
            //remove part of liquid Solution
            float totalVolume = 0f;
            for (int i = 0; i < solutionMakeup.Count; i++){
                volumes[i] = solutionMakeup[i] * scoopsHeld * 0.1852f;
                if (compoundStates[i] == 'l' || compoundStates[i] == 'a')
                {
                    volumes[i] = volumes[i] * 0.999f;
                }
                totalVolume += volumes[i];
            }

            for (int i = 0; i < solutionMakeup.Count; i++){
                solutionMakeup[i] = volumes[i] / totalVolume;
            }
            calculateDensity();
        }
    }

    public void addScoop(List<float> compoundType){
        //the weigh boat does not have a compound type set yet or it matches the one in the scoopula
        doCertainThingWithScript.tryingToMixCompoundsInNonLiquidHolder = false;
        if (compoundType.SequenceEqual(solutionMakeup) || solutionMakeup.All(num => num == 0f)){
            solutionMakeup = compoundType; 
            scoopsHeld += 1; 
            //adds mass to the rigidbody to compensate
            calculateDensity();
            GetComponent<Rigidbody>().mass += 0.1852f * density / 1000;
            if (solutionMakeup[4] == 1f){ // if its aluminum
                meshType = "Aluminum";
            }
            else{
                meshType = "scoop";
            }
            foreach (Transform child in transform)
            {
                if(!child.gameObject.activeSelf && child.gameObject.transform.name.StartsWith(meshType)){
                    child.gameObject.SetActive(true);
                    return;
                }

            }
            GetComponent<Rigidbody>().AddForce(Vector3.up * 0.0001f, ForceMode.Impulse);
        } 
        //the weigh boat compound type does not match that of the scoopula
        else{
            doCertainThingWithScript.tryingToMixCompoundsInNonLiquidHolder = true;
        }  
    }

    public void removeScoop(){
        if (scoopsHeld > 0){
            scoopsHeld -= 1;
            GetComponent<Rigidbody>().mass -= 0.1852f * density / 1000;
            foreach (Transform child in transform)
            {
                if(child.gameObject.activeSelf){
                    child.gameObject.SetActive(false);
                    return;
                }

            }
        }
    }

    void calculateDensity()
    {
        float weightedDensitySum = 0f;

        for (int i = 0; i < densities.Count; i++)
        {
            weightedDensitySum += solutionMakeup[i] / densities[i]; // Weighting by fraction
        }

        if (weightedDensitySum > 0)
        {
            density = 1f / weightedDensitySum; // Harmonic mean of densities
        }
        else
        {
            density = 0f; // No valid solution
        }
    }

}
