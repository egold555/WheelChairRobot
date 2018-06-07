using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class UdpListener : EmofaniGlobal
{
	
	private Thread listenerThread;
	private UdpClient listener;
	private int receivePort = 11000;
	private List<string> messages;

	public int ReceivePort {
		get {
			return this.receivePort;
		}
		set {
			if (value != receivePort) {
				receivePort = value;
				if (this.Listening) {
					Close();
					StartListening();
				}
			}
		}
	}

	private bool Listening {
		get {
			return listenerThread != null && listenerThread.IsAlive;
		}
	}

	public void StartListening()
	{
		
		Debug.Log("Starting UDP Listener on Port " + receivePort);
		
		if (listenerThread != null && listenerThread.IsAlive) {
			Close();
		}
		
		listenerThread = new Thread(
			new ThreadStart(Listen));
		
		listenerThread.IsBackground = true;
		listenerThread.Start();
		
	}


	private void Start()
	{
		
		Application.runInBackground = true;
		
		messages = new List<string>();
		
		StartListening();
	}

	private void Update()
	{

		if (messages.Count > 0) {

			try {
				// try block prevents dropping of messages if something goes wrong

				foreach (string message in messages) {
					FaceAnim.HandleMessage(message);
				}

				// clears only if no errors occured
				messages.Clear();
				Debug.Log("Messages cleared");

			} catch (Exception e) {
				Debug.Log(e.Message + ":" + e.StackTrace);
			}
		}

	}

	private void Listen()
	{
		try {
			listener = new UdpClient(receivePort);
			IPEndPoint groupEP = new IPEndPoint(IPAddress.Any, receivePort);

			while (true) {
				byte[] bytes = listener.Receive(ref groupEP);
				string message = Encoding.ASCII.GetString(bytes, 0, bytes.Length);
				Debug.Log(message);
				messages.Add(message);
			}
			
		} catch (Exception e) {
			Debug.Log(e.ToString());
		}

	}

	private void Close()
	{
		try {
			if (listenerThread != null && listenerThread.IsAlive) {
				Debug.Log("Kill listener thread.");
				listenerThread.Abort();
			}
			listener.Close();
		} catch (Exception e) {
			Debug.Log(e.Message);
		}
	}

	private void OnDestroy()
	{
		Close();
	}
    
}
