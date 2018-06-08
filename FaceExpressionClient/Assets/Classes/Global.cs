using UnityEngine;
using System.Collections;

public class EmofaniGlobal : MonoBehaviour
{

	protected UdpListener udp;
	protected FaceAnimator faceAnimator;
	protected Camera mainCamera;

	protected UdpListener Listener {
		get {
			if (this.udp == null) {
				GameObject udpObj = GameObject.Find("UDPListener");
				if (udpObj != null) {
					this.udp = udpObj.GetComponent<UdpListener>();
				}
			}
			return this.udp;
		}
	}

	protected FaceAnimator FaceAnim {
		get {
			if (this.faceAnimator == null) {
				GameObject faceObject = GameObject.Find("Face");
				if (faceObject != null) {
					this.faceAnimator = faceObject.GetComponent<FaceAnimator>();
				}
			}
			return this.faceAnimator;
		}
	}

	protected Camera MainCamera {
		get {
			if (this.mainCamera == null) {
				GameObject cameraObject = GameObject.Find("MainCamera");
				if (cameraObject != null) {
					this.mainCamera = cameraObject.GetComponent<Camera>();
				}
			}
			return this.mainCamera;
		}
	}

}