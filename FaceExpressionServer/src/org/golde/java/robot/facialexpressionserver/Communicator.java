package org.golde.java.robot.facialexpressionserver;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.SocketTimeoutException;
import java.net.UnknownHostException;

public class Communicator {

	private static Communicator instance;
	private DatagramSocket server = null;

	private Communicator() {			

	}

	public static Communicator getInstance() {
		if (instance == null){
			instance = new Communicator();
		}
		return instance;
	}

	public void listen(){
		MainWindow mw = MainWindow.getInstance();
		try {
			DatagramPacket packet = new DatagramPacket(new byte[1024],1024);
			server = new DatagramSocket(mw.getReceivePort());
			server.setSoTimeout(mw.getTimeout());
			server.receive(packet);
			String data = new String(packet.getData()).trim();
			mw.print("In: \"" + data + "\"");
			mw.update(data);
		} 
		catch (SocketTimeoutException e) {
			mw.print("Timeout: RobotFace didn't answer in time.");
		}
		catch (IOException e) {
			mw.print("Error: " + e.getMessage());
		} 
		finally {
			server.close();
		}
	}

	public void send(String param, String value){

		MainWindow mw = MainWindow.getInstance();

		// message format: 
		// t:<timestamp>;s:<source>;p:<port>;d:<parameter>=<value>

		//Ex: Communicator.getInstance().send("expression", "happy%100");
		//Ex: Communicator.getInstance().send("gazex", "130");

		String message = "t:" + System.currentTimeMillis() + ";";

		try {
			message += "s:" + InetAddress.getLocalHost().getHostAddress() + ";";
		} catch (UnknownHostException e1) {
			message += "s:localhost;";
		}
		message += "p:" + mw.getReceivePort() + ";";
		message += "d:" + param + "=" + value;
		DatagramSocket s = null;

		try {
			DatagramPacket p = new DatagramPacket(
					message.getBytes(),
					message.length(),
					InetAddress.getByName(mw.getHost()),
					mw.getSendPort());
			s = new DatagramSocket();
			s.send(p);
			mw.print("Out: \"" + message + "\" to " + mw.getHost() + " on port " + mw.getSendPort());
		} catch (IOException e) {
			mw.print("Error: " + e.getMessage());
		} finally {
			try {
				s.close();
			} catch (Exception e) {
				mw.print("Error: " + e.getMessage()); 
			}
		}

		// listen for OK
		listen();

	}

	protected void finalize() {
		server.close();
	}

}
