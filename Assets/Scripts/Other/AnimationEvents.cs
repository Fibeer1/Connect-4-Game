using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    private AnimationSequencer sequencer;

    private void Awake()
    {
        sequencer = FindFirstObjectByType<AnimationSequencer>();
    }

    //Since animation events only work with scripts which are attached to the same object as the animator,
    //This script handles events to other scripts

    public void DeathEndGame()
    {
        sequencer.DeathEndGame();
    }

    public void JumpscareStartSound()
    {
        sequencer.JumpscareStartSound();
    }

    public void JumpscareEndSound()
    {
        sequencer.JumpscareEndSound();
    }
}
