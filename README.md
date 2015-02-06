# *Delivering Kinect On-Demand to a Windows Store App with Microsoft Azure Media Services & Notification Hubs* #
This repository contains the source code of my tutorial *'Delivering Kinect On-Demand to a Windows Store App with Microsoft Azure Media Services & Notification Hubs'* that you can read [here](http://www.kinectingforwindows.com/2014/08/25/delivering-kinect-on-demand-to-a-store-app-with-azure-media-services-notification-hubs-tutorial/ "Full blog post").

This tutorial uses the following technologies -

- **Kinect for Windows Gen. II** *(public preview SDK)*
- **Micrsoft Azure Media Services**
- **Microsoft Azure Notification Hubs**
- **Windows 8.1 Store Apps**

![K4W logo](http://www.kinectingforwindows.com/wp-content/themes/twentyten/images/headers/logo.jpg)

## Disclaimer
This tutorial is based on the Kinect for Windows Gen II SDK v2.0.1410.19 NuGet Package.

## Template ##
In order to follow this tutorial I've created a template that contains a **Kinect for Windows application** that displays the camera and a **basic Windows Store App**. You can download the template [here](http://github.com/KinectingForWindows/G2KVOD/tree/Template "Tutorial Template").

## Scenario ##
In this scenario we will develop a Kinect application that enables the user to record a video with a self-describing caption. All the viewers will be notified that there is a new video available so they can watch it on-demand.

Before I start with the tutorial, let me quickly introduce some of the services we will be using in this scenario.


### Kinect client ###
We will develop a WPF client that will orchestrate the communication between the Kinect sensor & the cloud. The WPF client enables the users to start & stop the recording and assign a self-describing caption for the viewers. Upon recording we will save each frame as a JPG-image and render it into an AVI-video at the end. Important to know is that the recording will automatically stop when the Kinect sensor becomes unavailable.

![Kinect Client](http://www.kinectingforwindows.com/wp-content/uploads/2014/07/Demo-Scenario-Kinect.png)

When the recording is done we will have a local video that we will upload as our raw Asset, encode it into MP4 & package it to a Smooth Stream for our viewers app. Last but not least we will send a notification to all our viewers that there is a new video available along with the stream URL & the specified caption

### Video Client ###
The viewers will use a simple Windows Store App that will receive push notifications when a new video is ready. They can then use the stream URL and play the video in from Media Services. The stream URL will also be stored in the local storage so that the video can be watched again later on.

![Kinect Client](http://www.kinectingforwindows.com/wp-content/uploads/2014/07/Demo-Scenario-Client.png)