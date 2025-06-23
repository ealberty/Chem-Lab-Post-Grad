using Obi;
using UnityEngine;

public class EyeWashStationScript : MonoBehaviour
{
    [Header("Shower")]
    public float showerEmitterSpeed;
    public bool leverPulled;
    public doorScriptXAxis leverDoorScript;
    public ObiEmitter showerHeadEmitter;
    public GameObject pullLever;

    [Header("Eye Wash")]
    public float eyeWashEmitterSpeed;
    public bool handlePushed;
    public doorScriptXAxis handleDoorScript;
    public ObiEmitter eyeEmitterL;
    public ObiEmitter eyeEmitterR;

    
    public AudioSource showerAudioSrc;
    public AudioSource eyeWashAudioSrc;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        showerAudioSrc = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        // Shower head
        leverPulled = !leverDoorScript.doorIsClosed;
        showerHeadEmitter.speed = leverPulled ? showerEmitterSpeed : 0f;
        pullLever.transform.rotation = Quaternion.Euler(new Vector3(0, -90f, 0));

        if (leverPulled)
            if (!showerAudioSrc.isPlaying)
                showerAudioSrc.Play();
                
        if (!leverPulled)
            showerAudioSrc.Stop();

        // Eye Wash
        handlePushed = !handleDoorScript.doorIsClosed;
        eyeEmitterL.speed = handlePushed ? eyeWashEmitterSpeed : 0f;
        eyeEmitterR.speed = handlePushed ? eyeWashEmitterSpeed : 0f;

        if (handlePushed)
            if (!eyeWashAudioSrc.isPlaying)
                eyeWashAudioSrc.Play();
                
        if (!handlePushed)
            eyeWashAudioSrc.Stop();
    }
}
