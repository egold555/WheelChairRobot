package org.golde.java.robot.facialexpressionserver;

import java.awt.Container;
import java.awt.Cursor;
import java.awt.Dimension;
import java.awt.GridBagConstraints;
import java.awt.GridBagLayout;
import java.awt.GridLayout;
import java.awt.event.MouseEvent;
import java.awt.event.MouseListener;
import java.util.Hashtable;

import javax.swing.*;

public class MainWindow extends JFrame {

	private static final long serialVersionUID = 1;
	private static MainWindow instance;

	private static JTextArea textarea;
	private static JTextField hostField, sendPortField, receivePortField,
			timeoutField, talkField;

	private JSlider arousalSlider, pleasureSlider, blushSlider, gazeXSlider, gazeYSlider,
			gazeZSlider, expressionInstensitySlider;
	private JToggleButton talkingButton, idleButton;

	private MainWindow() {

		Dimension size = new Dimension(800, 600);
		this.setSize(size);
		this.setMinimumSize(size);

		this.addWindowListener(new java.awt.event.WindowAdapter() {
			@Override
			public void windowClosing(java.awt.event.WindowEvent windowEvent) {
				System.exit(0);
			}
		});

		Container cpane = this.getContentPane();
		cpane.setLayout(new GridLayout(1, 2));
		cpane.add(this.getControlsPanel());
		cpane.add(this.getCommunicatorPanel());

		this.setVisible(true);
	}

	public static void create() {
		MainWindow.getInstance();
	}

	public static MainWindow getInstance() {
		if (instance == null) {
			instance = new MainWindow();
		}
		return instance;
	}

	public void print(String message) {
		if (textarea != null) {
			textarea.append(message + "\n");
		}
	}

	public int getExpressionInstensity() {
		return expressionInstensitySlider.getValue();
	}

	public String getTalkMessage() {
		return talkField.getText();
	}

	public void update(String data) {

		Hashtable<String, String> keyValues = new Hashtable<String, String>();

		try {

			String[] params = data.split(";");
			for (String param : params) {
				String[] keyValue = param.split(":");
				keyValues.put(keyValue[0], keyValue[1]);
			}

		} catch (Exception e) {
			print("Error while interpreting a received message.");
		}

		try {

			arousalSlider.setValue(Integer.parseInt(keyValues.get("arousal")));
			pleasureSlider
					.setValue(Integer.parseInt(keyValues.get("pleasure")));
			gazeXSlider.setValue(Integer.parseInt(keyValues.get("gazex")));
			gazeYSlider.setValue(Integer.parseInt(keyValues.get("gazey")));
			gazeZSlider.setValue(Integer.parseInt(keyValues.get("gazez")));
			System.out.println("Incoming:-" + keyValues.get("talking") + "-");
			talkingButton.setSelected(keyValues.get("talking")
					.equalsIgnoreCase("true"));
			idleButton.setSelected(keyValues.get("idle").equalsIgnoreCase(
					"true"));

		} catch (Exception e) {
			print("Received message was missing at least one parameter: "
					+ e.getMessage());
		}
	}

	public String getHost() {
		return hostField.getText();
	}

	public int getTimeout() {
		try {
			return Integer.parseInt(timeoutField.getText());
		} catch (Exception e) {
			print("Timeout Input Error: " + e.getMessage());
			return 0;
		}
	}

	public int getSendPort() {
		try {
			return Integer.parseInt(sendPortField.getText());
		} catch (Exception e) {
			print("Port Input Error: " + e.getMessage());
			return -1;
		}
	}

	public int getReceivePort() {
		try {
			return Integer.parseInt(receivePortField.getText());
		} catch (Exception e) {
			print("Port Input Error: " + e.getMessage());
			return -1;
		}
	}

	private JPanel getControlsPanel() {
		JPanel panel = new JPanel();
		panel.setLayout(new BoxLayout(panel, BoxLayout.Y_AXIS));
		panel.add(this.getExpressionsPanel());
		panel.add(this.getSliderPanel());
		return panel;
	}

	private JPanel getCommunicatorPanel() {
		JPanel panel = new JPanel();
		panel.setLayout(new GridBagLayout());

		GridBagConstraints c = new GridBagConstraints();
		c.weightx = 1;
		c.fill = GridBagConstraints.BOTH;
		c.gridx = GridBagConstraints.REMAINDER;

		c.weighty = 24;
		panel.add(getUdpLogPanel(), c);

		c.weighty = 1;
		panel.add(getTargetPanel(), c);

		return panel;
	}

	private JPanel getTargetPanel() {
		JPanel panel = new JPanel();
		panel.setBorder(BorderFactory.createTitledBorder("Target"));

		panel.setLayout(new GridLayout(1, 2));

		hostField = new JTextField("localhost", 16);
		hostField.setBorder(BorderFactory.createTitledBorder("Host"));
		panel.add(hostField);

		sendPortField = new JTextField("11000", 16);
		sendPortField
				.setBorder(BorderFactory.createTitledBorder("Output Port"));
		panel.add(sendPortField);

		receivePortField = new JTextField("11001", 16);
		receivePortField.setBorder(BorderFactory
				.createTitledBorder("Input Port"));
		panel.add(receivePortField);

		timeoutField = new JTextField("50", 3);
		timeoutField.setBorder(BorderFactory.createTitledBorder("Timeout"));
		panel.add(timeoutField);

		return panel;
	}

	private JScrollPane getUdpLogPanel() {

		textarea = new JTextArea();
		textarea.setEditable(false);

		JScrollPane panel = new JScrollPane(textarea);
		panel.setBorder(BorderFactory.createTitledBorder("UDP Log"));

		return panel;
	}

	private JPanel getSliderPanel() {

		JPanel emotionPanel = new JPanel();
		emotionPanel.setBorder(BorderFactory
				.createTitledBorder("Emotion Parameters"));
		emotionPanel.setLayout(new BoxLayout(emotionPanel, BoxLayout.Y_AXIS));

		arousalSlider = addParamSlider("arousal", -100, 100, emotionPanel);
		pleasureSlider = addParamSlider("pleasure", -100, 100, emotionPanel);
		blushSlider = addParamSlider("blush", 0, 100, emotionPanel);

		JPanel gazePanel = new JPanel();
		gazePanel
				.setBorder(BorderFactory.createTitledBorder("Gaze Parameters"));
		gazePanel.setLayout(new BoxLayout(gazePanel, BoxLayout.Y_AXIS));
		gazeXSlider = addParamSlider("gazex", -200, 200, gazePanel);
		gazeYSlider = addParamSlider("gazey", -200, 200, gazePanel);
		gazeZSlider = addParamSlider("gazez", 0, 400, gazePanel);

		JPanel panel = new JPanel();
		panel.setLayout(new BoxLayout(panel, BoxLayout.Y_AXIS));
		panel.add(emotionPanel);
		panel.add(gazePanel);

		return panel;
	}

	private JPanel getExpressionsPanel() {
		JPanel panel = new JPanel();
		panel.setBorder(BorderFactory.createTitledBorder("Expressions"));
		panel.setSize(new Dimension(100, 100));
		panel.setLayout(new BoxLayout(panel, BoxLayout.Y_AXIS));

		expressionInstensitySlider = addNiceSlider("intensity", 0, 100, panel);
		expressionInstensitySlider.setValue(100);

		JPanel buttons = new JPanel();
		buttons.setLayout(new GridLayout(3, 3));
		buttons.add(getExpressionButton("neutral"));
		buttons.add(getExpressionButton("happy"));
		buttons.add(getExpressionButton("sad"));
		buttons.add(getExpressionButton("attentive"));
		buttons.add(getExpressionButton("excited"));
		buttons.add(getExpressionButton("relaxed"));
		buttons.add(getExpressionButton("sleepy"));
		buttons.add(getExpressionButton("frustrated"));
		buttons.add(talkingButton = getToggleButton("talking"));
		buttons.add(idleButton = getToggleButton("idle"));

		panel.add(buttons);

		return panel;
	}

	private JSlider addNiceSlider(String name, int min, int max,
			JPanel targetPanel) {
		JPanel panel = new JPanel();

		panel.setLayout(new BoxLayout(panel, BoxLayout.Y_AXIS));

		JLabel label = new JLabel(name);
		label.setAlignmentX(CENTER_ALIGNMENT);
		JSlider slider = new JSlider();

		slider.setName(name);
		slider.setCursor(Cursor.getPredefinedCursor(Cursor.HAND_CURSOR));
		slider.setMaximum(max);
		slider.setMinimum(min);
		slider.setValue(0);

		int range = max - min;
		if (range <= 40) {
			slider.setMajorTickSpacing(10);
			slider.setMinorTickSpacing(1);
		} else {
			slider.setMajorTickSpacing(range / 4);
		}
		slider.setPaintLabels(true);
		slider.setPaintTicks(true);

		panel.add(label);
		panel.add(slider);

		targetPanel.add(panel);

		return slider;
	}

	private JSlider addParamSlider(String name, int min, int max,
			JPanel targetPanel) {

		JSlider slider = addNiceSlider(name, min, max, targetPanel);

		slider.addMouseListener(new MouseListener() {

			@Override
			public void mouseReleased(MouseEvent e) {
				JSlider source = (JSlider) e.getSource();
				String value = String.valueOf(source.getValue());
				Communicator.getInstance().send(source.getName(), value);
			}

			@Override
			public void mousePressed(MouseEvent e) {

			}

			@Override
			public void mouseExited(MouseEvent e) {

			}

			@Override
			public void mouseEntered(MouseEvent e) {

			}

			@Override
			public void mouseClicked(MouseEvent e) {

			}
		});

		return slider;
	}

	private JButton getExpressionButton(String expression) {

		JButton button = new JButton(expression);

		button.addMouseListener(new MouseListener() {

			@Override
			public void mouseReleased(MouseEvent e) {
				JButton button = (JButton) e.getSource();
				MainWindow mw = MainWindow.getInstance();
				Communicator.getInstance().send("expression",
						button.getText() + "%" + mw.getExpressionInstensity());

			}

			@Override
			public void mouseClicked(MouseEvent e) {
				
			}

			@Override
			public void mouseEntered(MouseEvent e) {
				

			}

			@Override
			public void mouseExited(MouseEvent e) {
				

			}

			@Override
			public void mousePressed(MouseEvent e) {
				

			}

		});

		return button;
	}

	private JToggleButton getToggleButton(String name) {

		JToggleButton button = new JToggleButton(name);

		button.addMouseListener(new MouseListener() {

			@Override
			public void mouseReleased(MouseEvent e) {
				JToggleButton button = (JToggleButton) e.getSource();
				System.out.println(button.isSelected());
				Communicator.getInstance().send(button.getText(),
						String.valueOf(button.isSelected()));
			}

			@Override
			public void mousePressed(MouseEvent e) {
				

			}

			@Override
			public void mouseExited(MouseEvent e) {
				

			}

			@Override
			public void mouseEntered(MouseEvent e) {
				

			}

			@Override
			public void mouseClicked(MouseEvent e) {
				

			}
		});

		return button;

	}

}
