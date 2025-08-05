using Entropek.Collections;
using Godot;
using System;
using System.Collections.Generic;

public partial class AudioPlayer : Node2D{
    private SwapbackList<AudioInstance> audioInstances = new SwapbackList<AudioInstance>();
    private SwapbackList<FMOD.Studio.EVENT_CALLBACK> callbacks = new SwapbackList<FMOD.Studio.EVENT_CALLBACK>();

    public override void _ExitTree(){
        base._ExitTree();
        foreach (var audioInstance in audioInstances) {
            audioInstance.EventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            audioInstance.EventInstance.release();
        }

        audioInstances.Clear();
        callbacks.Clear();
    }


    public void PlaySound(string eventName, bool oneshot = true){
        TrackEventInstanceLifetime(AudioManager.Singleton.PlayEvent(eventName, oneshot));
    }

    public void PlaySound(string eventName, Vector2 globalPosition, bool oneshot = true){
        TrackEventInstanceLifetime(AudioManager.Singleton.PlayEvent(eventName, globalPosition, oneshot));
    }

    public void PauseState(){
        for(int i = 0; i < audioInstances.Count; i++){
            audioInstances[i].EventInstance.setPaused(true);
        }
    }

    public void ResumeState(){
        for(int i = 0; i < audioInstances.Count; i++){
            audioInstances[i].EventInstance.setPaused(false);
        }    
    }

    public bool StopSound(string eventName, bool immediate = false){
        for(int i = audioInstances.Count-1; i >=     0; i--){
            if(audioInstances[i].Name == eventName){
                audioInstances[i].EventInstance.stop(immediate == false? FMOD.Studio.STOP_MODE.ALLOWFADEOUT : FMOD.Studio.STOP_MODE.IMMEDIATE);
                return true;
            }
        }
        return false;
    }

    private void TrackEventInstanceLifetime(AudioInstance audioInstance){
        audioInstances.Add(audioInstance);

        FMOD.Studio.EVENT_CALLBACK callback = null;
        callback = (FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr paramPtr) => {
            
            // stop tracking and release the event when the sound has stopped playing.

            if(type == FMOD.Studio.EVENT_CALLBACK_TYPE.STOPPED){
                FMOD.Studio.EventInstance stoppedInstance = new FMOD.Studio.EventInstance(instancePtr);
                stoppedInstance.getDescription(out FMOD.Studio.EventDescription description);
                audioInstances.Remove(audioInstance);
                stoppedInstance.release();
                callbacks.Remove(callback); 
            }
            
            return FMOD.RESULT.OK;
        };

        audioInstance.EventInstance.setCallback(callback, FMOD.Studio.EVENT_CALLBACK_TYPE.STOPPED);

        callbacks.Add(callback);
    }
}
