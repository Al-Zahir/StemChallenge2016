#pragma strict

function Start () {

((GetComponent.<Animation>() as Animation)["run"] as AnimationClip).wrapMode = WrapMode.Loop;
((GetComponent.<Animation>() as Animation)["run"] as AnimationClip).frameRate = 2;

}

function Update () {

}