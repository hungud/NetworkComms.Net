﻿using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using System.Net;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.IO;

namespace ExamplesChat.Android
{
    [Activity(Label = "ExamplesChat.Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class Activity1 : Activity
    {
        #region Private Fields
        /// <summary>
        /// An instance of the chat applications
        /// </summary>
        ChatAppAndroid chatApplication;

        /// <summary>
        /// The chat history window. This is where all of the messages get displayed
        /// </summary>
        TextView chatHistory;

        /// <summary>
        /// The send button
        /// </summary>
        Button sendButton;
        
        /// <summary>
        /// The input box where new messages are entered
        /// </summary>
        AutoCompleteTextView input;

        /// <summary>
        /// The texbox containing the master ip address (server)
        /// </summary>
        AutoCompleteTextView ipTextBox;

        /// <summary>
        /// The texbox containing the master port number (server)
        /// </summary>
        AutoCompleteTextView portTextBox;

        /// <summary>
        /// The spinner (drop down) menu for selecting the connection type to use
        /// </summary>
        Spinner connectionTypeSelector;

        /// <summary>
        /// The checkbox which can be used to enable local server mode
        /// </summary>
        CheckBox enableLocalServerCheckBox;
        #endregion

        /// <summary>
        /// Method runs after the application has been launched
        /// </summary>
        /// <param name="bundle"></param>
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            //Get references to user interface controls
            connectionTypeSelector = FindViewById<Spinner>(Resource.Id.connectionTypeSpinner);
            chatHistory = FindViewById<TextView>(Resource.Id.mainText);
            input = FindViewById<AutoCompleteTextView>(Resource.Id.messageTextInput);
            ipTextBox = FindViewById<AutoCompleteTextView>(Resource.Id.ipTextInput);
            portTextBox = FindViewById<AutoCompleteTextView>(Resource.Id.portTextInput);
            sendButton = FindViewById<Button>(Resource.Id.sendButton);
            enableLocalServerCheckBox = FindViewById<CheckBox>(Resource.Id.enableLocalServer);

            //Set the connection type selection drop down options
            ArrayAdapter adapter = ArrayAdapter.CreateFromResource(this, Resource.Array.ConnectionTypes, global::Android.Resource.Layout.SimpleSpinnerItem);
            adapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerDropDownItem);
            connectionTypeSelector.Adapter = adapter;

            //Append the method 'connectionType_Selected' to the connection type selected event
            connectionTypeSelector.ItemSelected += connectionType_Selected;

            //Append the method 'sendButton_Click' to the button click event
            sendButton.Click += sendButton_Click;

            //Append the method 'enableLocalServerCheckBox_CheckedChange' when the enable
            //local server checkbox state is changed
            enableLocalServerCheckBox.CheckedChange += enableLocalServerCheckBox_CheckedChange;

            //Initialise the chat application
            chatApplication = new ChatAppAndroid(this, chatHistory, input);

            //Print the usage instructions
            chatApplication.PrintUsageInstructions();

            //Initialise NetworkComms.Net but without a local server
            chatApplication.RefreshNetworkCommsConfiguration();

            //Uncomment this line to enable logging
            //EnableLogging();
        }

        /// <summary>
        /// Event triggered when the enable local server checkbox is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void enableLocalServerCheckBox_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            //Update the local server enabled state and then refresh the network configuration
            chatApplication.LocalServerEnabled = enableLocalServerCheckBox.Checked;
            chatApplication.RefreshNetworkCommsConfiguration();
        }

        /// <summary>
        /// Event triggered when the send button is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void sendButton_Click(object sender, EventArgs e)
        {
            //Parse the ip address box
            IPAddress newMasterIPAddress;
            if (!IPAddress.TryParse(ipTextBox.Text, out newMasterIPAddress))
                //If the parse failed set the ipTextBox back to the the previous good value
                ipTextBox.Text = chatApplication.ServerIPAddress;
            else
                chatApplication.ServerIPAddress = newMasterIPAddress.ToString();

            //Parse the port number
            int newPort;
            if (!int.TryParse(portTextBox.Text, out newPort) || newPort < 1 || newPort > ushort.MaxValue)
                //If the parse failed we set the portTextBox back to the previous good value
                portTextBox.Text = chatApplication.ServerPort.ToString();
            else
                chatApplication.ServerPort = newPort;

            //Send the text entered in the input box
            chatApplication.SendMessage(input.Text);
        }

        /// <summary>
        /// Checks if the selected connection type has changed. If changed reset the example to use the new connection type.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void connectionType_Selected(object sender, EventArgs e)
        {
            //Parse the connection type
            string selectedItem = connectionTypeSelector.SelectedItem.ToString();
            if (selectedItem == "TCP")
                chatApplication.ConnectionType = NetworkCommsDotNet.ConnectionType.TCP;
            else if (selectedItem == "UDP")
                chatApplication.ConnectionType = NetworkCommsDotNet.ConnectionType.UDP;

            //Update the NetworkComms.Net configuration
            chatApplication.RefreshNetworkCommsConfiguration();
        }

        /// <summary>
        /// Enable NetworkComms.Net logging. Usefull for debugging.
        /// </summary>
        void EnableLogging()
        {
            string sdCardDir = global::Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            string logFileName = Path.Combine(sdCardDir, "NetworkCommsLog.txt");

            chatApplication.AppendLineToChatHistory(System.Environment.NewLine + "Logging enabled to " + logFileName);

            NetworkCommsDotNet.NetworkComms.EnableLogging(logFileName);
        }
    }
}

