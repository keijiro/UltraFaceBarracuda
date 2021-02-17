UltraFaceBarracuda
==================

![gif](https://i.imgur.com/OQeWbDn.gif)
![gif](https://i.imgur.com/Xxiw25F.gif)

**UltraFaceBarracuda** is a Unity sample project that shows how to run the
[UltraFace] face detection neural network model on the Unity [Barracuda].

[UltraFace]:
  https://github.com/Linzaer/Ultra-Light-Fast-Generic-Face-Detector-1MB
[Barracuda]: https://docs.unity3d.com/Packages/com.unity.barracuda@latest

For the details of the UltraFace ("Ultra-Light-Fast-Generic-Face-Detector-1MB")
model, please see the [original repository][UltraFace].

System requirements
-------------------

- Unity 2020.2
- Barracuda 1.3.0

Sample scenes
-------------

### StaticImageTest

**StaticImageTest** runs the face detection model with a given single image. It
visualizes the bounding boxes using the Unity UI system, which shows how to use
the detection results with simple C# scripting.

### WebcamTest

**WebcamTest** runs the face detection model with a video stream from a
UVC-compatible video capture device (webcam, HDMI capture, etc.). It draws
bounding boxes using indirect drawing -- In other words, it visualizes the
detection results without GPU-to-CPU readback so that it runs in a performant
way.

You can also set a texture to decorate the bounding boxes.
