using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System;

public class FaceAnimator : EmofaniGlobal
{

	public const int fallbackSendPort = 11001;
	public string hostname = "";
	private float arousal, pleasure, blush, gazeXEyes, gazeYEyes, gazeZEyes, gazeXHead, gazeYHead, gazeZHead,
		eyeRandX, eyeRandY, idleTime, idleChangeTime, verticalHeadMovement = .5f, horizontalHeadMovement = .5f;
	private int targetArousal, targetPleasure, targetBlush, targetGazeX, targetGazeY, targetGazeZ = 250, sendPort = -1;
	private long lastInputId;
	private bool talking, idle, mirrorGaze;
	private Vector3 lookAtEyes;
	private Transform leftEyeBone, rightEyeBone, headBone, focusPoint;
	private Dictionary<string,Expression> expressions;
	private Animator anim;
    private Material material;

	public bool MirrorGaze {
		get {
			return this.mirrorGaze;
		}
		set {
			mirrorGaze = value;
		}
	}

	public float VerticalHeadMovement {
		get {
			return this.verticalHeadMovement;
		}
		set {
			verticalHeadMovement = value;
		}
	}

	public float HorizontalHeadMovement {
		get {
			return this.horizontalHeadMovement;
		}
		set {
			horizontalHeadMovement = value;
		}
	}
	
	public float BreathingWeight {
		get {
			return anim.GetLayerWeight(anim.GetLayerIndex("Breathing"));
		}
		set {
			anim.SetLayerWeight(anim.GetLayerIndex("Breathing"), value);
		}
	}

	public long LastInputId {
		get {
			return this.lastInputId;
		}
	}


	public void HandleMessage(string message)
	{

		Debug.Log ("Input:" + message);

		try {

			long timestamp = 0;
			string inputError = "";

			// transform message string to dictionary
			Dictionary<string,string> input = transformMessage(message);

			// check if port is set
			if (input.ContainsKey("p")) {
				sendPort = int.Parse(input ["p"]);
			} else {
				inputError += "p (port to answer to) is missing. ";
			}

			// check if host is set
			if (input.ContainsKey("s")) {
				hostname = input ["s"];
			} else {
				inputError += "s (hostname to answer to) is missing. ";
			}

			// validate timestamp
			if (input.ContainsKey("t")) {
				timestamp = long.Parse(input ["t"]);
			} else {
				inputError += "t (message id or timestamp) is missing. ";
			}

			// messages arriving late will be dropped
			if (timestamp < lastInputId) {
				inputError += "Dropped because t is smaller than previous timestamp. ";
			}

			// check if data is set
			if (!input.ContainsKey("d")) {
				inputError += "d (data with key=value) is missing. ";
			}

			// if we detect an input error we still try to determine the port and host to send an error status to
			if (inputError != "") {
				if (sendPort == -1) {
					sendPort = fallbackSendPort;
				}
				if (hostname == "") {
					hostname = IPAddress.Loopback.ToString();
				}
				throw new Exception(inputError);
			}

			// Interpret data part of the message
			string[] nameValue = input ["d"].Split('=');
			string key = nameValue [0];
			string value = nameValue [1];

			// idle will be set to false for every message to exit idle mode if a new parameter is sent. 
			// it will only be activated if the key is "idle" and the value "true"
			Idle = false;

			switch (key) {
				case "arousal":
					targetArousal = int.Parse(value);
					break;
				case "pleasure":
					targetPleasure = int.Parse(value);
					break;
				case "gazex":
					targetGazeX = int.Parse(value);
					break;
				case "gazey":
					targetGazeY = int.Parse(value);
					break;
				case "gazez":
					targetGazeZ = int.Parse(value);
					break;
                case "blush":
                    targetBlush = int.Parse(value);
                    break;
				case "expression":
					SetExpression(value);
					break;
				case "talking":
					Talking = bool.Parse(value);
					break;
				case "idle":
					Idle = bool.Parse(value);
					break;
				default:
					throw new Exception("Unknown parameter \"" + key + "\"");
			}

			// everything was OK. Set new timestamp and send OK status.
			lastInputId = timestamp;
			SendOkStatus();

		} catch (Exception e) {
			// something went wrong. Send an error message with the problem.
			SendErrorStatus(e.Message);
		}
	}

	public void SetExpression(string expressionInput)
	{
		try {

			// try to read expression and intensity from string
			string[] expressionStrength = expressionInput.Split('%');
			string expression = expressionStrength [0];
			float instensity = float.Parse(expressionStrength [1]) / 100;

			// read expression's arousal and pleasure values and apply intensity
			// set values as target
			Expression ex = expressions [expression];
			targetArousal = Mathf.RoundToInt(ex.Arousal * instensity);
			targetPleasure = Mathf.RoundToInt(ex.Pleasure * instensity);

		} catch (Exception e) {

			throw new Exception("Expression error: " + e.Message + " ");

		}
	}

	protected void Awake()
	{

		anim = GetComponent<Animator>();

		expressions = new Dictionary<string, Expression>();
		expressions.Add("neutral", new Expression(0, 0));
		expressions.Add("happy", new Expression(85, 12));
		expressions.Add("sad", new Expression(-71, -36));
		expressions.Add("attentive", new Expression(-0, 91));
		expressions.Add("sleepy", new Expression(5, -97));
		expressions.Add("frustrated", new Expression(-56, 44));
		expressions.Add("excited", new Expression(63, 72));
		expressions.Add("relaxed", new Expression(70, -64));

		leftEyeBone = GameObject.Find("Face/FaceRig/Chest/Neck/Head/Eye_L").transform;
		rightEyeBone = GameObject.Find("Face/FaceRig/Chest/Neck/Head/Eye_R").transform;
		headBone = GameObject.Find("Face/FaceRig/Chest/Neck/Head").transform;
        material = GetComponentInChildren<Renderer>().materials[0];

		// this is just a little helper in the editor (I used it during debugging and left it in)
		focusPoint = new GameObject("FocusPoint").transform;
	}

	protected void Update()
	{

		if (Idle) {
			// IdleLoop() will randomy set arousal, pleasure and gaze parameters from time to time
			IdleLoop();
		}

		// set time variables to use with Mathf.Lerp() function. Differs for expression, eyes and head
		float expT = Time.deltaTime * 3; // time to use for expression blending
		float gazeTEyes = Time.deltaTime * 6; // time to use for eye lookAt position blending
		float gazeTHead = Time.deltaTime; // time to use for head lookAt position blending

		/*
         * interpolate between current values and target values
         */

        // Arousal (Animation Controller)
		arousal = Mathf.Lerp(GetFloat("arousal"), (float)targetArousal, expT);
		SetFloat("arousal", arousal);

        // Pleasure (Animation Controller)
		pleasure = Mathf.Lerp(GetFloat("pleasure"), (float)targetPleasure, expT);
		SetFloat("pleasure", pleasure);

        // Blush (Shader Property)
        blush = Mathf.Lerp(blush, (float)targetBlush, expT);
        material.SetFloat("_BlushIntensity", (float)blush/100);

        // Gaze (Bone-Rotation updated in LateUpdate())
        gazeXEyes = Mathf.Lerp(gazeXEyes, (float)targetGazeX, gazeTEyes);
		gazeYEyes = Mathf.Lerp(gazeYEyes, (float)targetGazeY, gazeTEyes);
		gazeZEyes = Mathf.Lerp(gazeZEyes, (float)targetGazeZ, gazeTEyes);
		gazeXHead = Mathf.Lerp(gazeXHead, (float)targetGazeX, gazeTHead);
		gazeYHead = Mathf.Lerp(gazeYHead, (float)targetGazeY, gazeTHead);
		gazeZHead = Mathf.Lerp(gazeZHead, (float)targetGazeZ, gazeTHead);

	}

	protected void LateUpdate()
	{
		SetGaze();
	}

	protected bool Idle {
		get {
			return this.idle;
		}
		set {
			if (value) {
				Talking = false;
				idle = true;
			} else {
				idle = false;
			}
		}
	}

	protected bool Talking {
		get {
			return this.talking;
		}
		set {
			talking = value;
			SetBool("talking", value);
		}
	}

	protected void IdleLoop()
	{

		idleTime += Time.deltaTime;

		if (idleTime > idleChangeTime) {

			idleTime = 0;
			idleChangeTime = UnityEngine.Random.Range(2f, 6f);

			targetArousal = UnityEngine.Random.Range(-25, 25);
			targetPleasure = UnityEngine.Random.Range(-25, 25);

			targetGazeX = UnityEngine.Random.Range(-50, 50);
			targetGazeY = UnityEngine.Random.Range(-25, 25);
			targetGazeZ = UnityEngine.Random.Range(200, 350);

			SendOkStatus(); // in case a client application is listening -> let it know about the change
		}
	}

	private void SetGaze()
	{
		int xMod = (MirrorGaze) ? -1 : 1;

		headBone.LookAt(new Vector3(gazeXHead * xMod * horizontalHeadMovement,
		                            gazeYHead * verticalHeadMovement,
		                            -gazeZHead));

		// set eye gaze
		lookAtEyes = new Vector3(gazeXEyes * xMod, gazeYEyes, -gazeZEyes);
		leftEyeBone.LookAt(lookAtEyes);
		rightEyeBone.LookAt(lookAtEyes);

		// update the helper empty for debugging in the editor
		focusPoint.position = lookAtEyes;
	}

	private Dictionary<string,string> transformMessage(string message)
	{

		Dictionary<string,string> output = new Dictionary<string, string>();

		string[] segments = message.Split(';');

		foreach (string segment in segments) {
			string[] keyValue = segment.Split(':');
			output.Add(keyValue [0], keyValue [1]);
		}

		return output;
	}

	private void SetBool(string param, bool value)
	{
		anim.SetBool(Animator.StringToHash(param), value);
	}

	private bool GetBool(string param)
	{
		return anim.GetBool(Animator.StringToHash(param));
	}

	private void SetFloat(string param, float value)
	{
		anim.SetFloat(Animator.StringToHash(param), value);
	}

	private float GetFloat(string param)
	{
		return anim.GetFloat(Animator.StringToHash(param));
	}

	private void SendOkStatus()
	{

		string message = "status:OK;";
		message += "arousal:" + targetArousal + ";";
		message += "pleasure:" + targetPleasure + ";";
        message += "blush:" + targetBlush + ";";
        message += "gazex:" + targetGazeX + ";";
		message += "gazey:" + targetGazeY + ";";
		message += "gazez:" + targetGazeZ + ";";
		message += "talking:" + talking.ToString().ToLower() + ";";
		message += "idle:" + idle.ToString().ToLower();

		SendStatus(message);
	}

	private void SendErrorStatus(string error)
	{

		string message = "status:ERROR;";
		message += "error:\"" + error + "\"";

		SendStatus(message);
	}

	private void SendStatus(string message)
	{
		Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

		IPAddress serverAddr = IPAddress.Parse(hostname);

		IPEndPoint endPoint = new IPEndPoint(serverAddr, sendPort);

		byte[] send_buffer = Encoding.ASCII.GetBytes(message);

		sock.SendTo(send_buffer, endPoint);

	}

}
