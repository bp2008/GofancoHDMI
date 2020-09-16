# GofancoHDMI
A simple Windows Service with an embedded web server that assists in controlling a Gofanco PRO-Matrix44-SC (4x4 HDMI matrix) over HTTP.

![PRO-Matrix44-SC](https://i.imgur.com/c8fEyQs.jpg)

![versions](https://i.imgur.com/TNR975W.png)

## Purpose

The [Gofanco PRO-Matrix44-SC](http://amzn.com/B07P765D45) has a rudimentary web interface allowing device control over TCP/IP.  However, all useful requests require the use of HTTP POST, and to complicate matters, the device does not implement the HTTP protocol correctly when it sends responses.  This service application aims to offer control of the HDMI matrix using standard HTTP GET requests.

## Supported Features

* Read the device configuration, including power state, input/output status, input names, output names, and mapping names (a "mapping" is basically a preset that remembers the state of all inputs and outputs).
* Change the input assigned to each output.
* Set the names of inputs, outputs, and mappings.

## Features Not Implemented

The official "Matrix Manager" application can do most of these things via IP, if you don't have a serial connection available.  The default IP for one of these devices is `192.168.1.70`.

* EDID control
* Network settings
* Cloud settings
* Reading software/firmware versions
* Firmware updating
