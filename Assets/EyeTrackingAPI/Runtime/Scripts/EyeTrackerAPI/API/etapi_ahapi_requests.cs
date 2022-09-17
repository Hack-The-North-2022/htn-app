﻿// Copyright (c) AdHawk Microsystems Inc.
// All rights reserved.

using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

// disable warnings to do with unassigned and un-used fields
#pragma warning disable CS0649, CS0168, CS0414


// This file holds the EyeTrackerAPI backend request functions

namespace AdhawkApi
{
    public partial class EyeTrackerAPI
    {

        // public TextEditor
        

        /// <summary>
        /// Recenter the calibration using data registered after the original calibration was generated
        /// </summary>
        /// <returns> Coroutine handle for recenter calibration. Can be treated as a void 
        /// or coroutine for yielding until procedure is complete. </returns>
        public Coroutine RecenterCalibration()
        {
            if (DeviceHandler != null)
            {
                return DeviceHandler.RunRecenter();
            }
            else return null;
        }

        /// <summary>
        /// Request current device autotune. Yeilds when request is complete
        /// </summary>
        /// <returns></returns>
        public Coroutine Autotune()
        {
            if (DeviceHandler != null)
            {
                return DeviceHandler.RequestAutotune();
            }
            else return null;
        }

        /// <summary> Remind backend service of the currently selected IPD </summary>
        public Coroutine SetEyeOffsetFromBaseInMeters(float newIPD)
        {
            return StartCoroutine(SetEyeOffsetFromBaseInMetersCoroutine(newIPD));
        }

        private IEnumerator SetEyeOffsetFromBaseInMetersCoroutine(float newIPD)
        {
            float baseIPD = 0.063f; // Oculus quest 2 base IPD

            float newOffsetR = ((newIPD / 2.0f) - (baseIPD / 2.0f)) * 1000.0f;
            float newOffsetL = -newOffsetR;
            
            Vector3 leftOffset = Vector3.zero;
            Vector3 rightOffset = Vector3.zero;

            leftOffset.x = newOffsetR;
            rightOffset.x = newOffsetL;

            Debug.Log("Setting new offsets: L: " + leftOffset.ToString("F4") + ", R: " + rightOffset.ToString("F4"));

            List<byte> data = new List<byte>() { (byte)udpInfo.PropertyType.COMPONENT_OFFSETS };
            data.AddRange(rightOffset.ToBytes());
            data.AddRange(leftOffset.ToBytes());
            yield return udpClient.SendUDPRequest(new UDPRequest(udpInfo.PROPERTY_SET, null, data));
        }

        /// <summary> Add a listener to the error handler callback event. </summary>
        public void RegisterErrorHandler(ErrorMessageHandler Handler)
        {
            if (Handler != null) { OnErrorMessage += Handler; }
        }

        /// <summary> Remove a listener from the error handler callback event. </summary>
        public void UnregisterErrorHandler(ErrorMessageHandler Handler)
        {
            if (Handler != null) { OnErrorMessage -= Handler; }
        }

        /// <summary> Sets the session log mode of backend API service. This is for data collection and analysing algorithms to run
        /// on session data generated by backend. </summary>
        public void SetLogMode(udpInfo.LogMode mode)
        {
            SessionLogMode = mode;
            if (udpClient)
            {
                if (udpClient.ServerConnected)
                {
                    Debug.Log("Setting log mode to: " + SessionLogMode.ToString());
                    udpClient.RestartSession(SessionLogMode, SessionProfileName);
                }
            }
        }

        /// <summary>
        /// Update profile name and tags (tags are simple strings separated by comma) for analytics purposes
        /// </summary>
        public void SetLogTags(string name = "", string tags = "")
        {
            PlayerPrefs.SetString(P_PREFS_SESSION_NAME, name);
            PlayerPrefs.SetString(P_PREFS_SESSION_TAGS, tags);
            SessionProfileName = name;
            SessionTags = tags;
            if (udpClient)
            {
                if (udpClient.ServerConnected)
                {
                    Debug.Log("Setting session tags to: name: " + name + ", tags: " + tags);
                    udpClient.RestartSession(SessionLogMode, name, tags);
                }
            }
        }

        /// <summary>
        /// Update profile name and tags (Backend session tagging), and update log mode at the same time
        /// </summary>
        public void SetLogModeAndTags(udpInfo.LogMode mode, string name = "", string tags = "")
        {
            PlayerPrefs.SetString(P_PREFS_SESSION_NAME, name);
            PlayerPrefs.SetString(P_PREFS_SESSION_TAGS, tags);
            SessionProfileName = name;
            SessionTags = tags;
            if (udpClient)
            {
                if (udpClient.ServerConnected)
                {
                    Debug.Log("Setting log mode to: " + mode.ToString());
                    Debug.Log("Setting session tags to: name: " + name + ", tags: " + tags);
                    udpClient.RestartSession(mode, name, tags);
                }
            }
        }

        /// <summary>
        /// Save a calibration or multiglint fusion. This is useless and needs to be paired with 
        /// </summary>
        /// <param name="blobType"> Blob type to save </param>
        /// <param name="blobCallBack"> Some anonymous function to call to essentially apply or return data from the coroutine </param>
        public Coroutine SaveBlob(udpInfo.BlobVersion blobType, System.Action<UInt32> blobCallback = null)
        {
            return StartCoroutine(SaveBlobCoroutine(blobType, blobCallback));
        }

        private IEnumerator SaveBlobCoroutine(udpInfo.BlobVersion blobType, Action<UInt32> blobCallback)
        {
            yield return udpClient.SendUDPRequest(
                   request: new UDPRequest(
                       udpInfo.SAVE_BLOB,
                       (byte[] data, UDPRequestStatus status) => {
                           if (status == UDPRequestStatus.AckSuccess)
                           {
                               if (blobCallback != null)
                               {
                                   blobCallback(BitConverter.ToUInt32(data, 1));
                               }
                           }
                       },
                       data: new byte[] { (byte)blobType }
                   ),
                   timeout: 1.0f
               );
        }

        /// <summary>
        /// Command backend to load a saved calibration or multiglint fusion
        /// </summary>
        /// <param name="blobType"> Blob type to load </param>
        /// <param name="blobID"> Saved blob's ID </param>
        /// <returns></returns>
        public Coroutine LoadBlob(udpInfo.BlobVersion blobType, UInt32 blobID)
        {
            return StartCoroutine(LoadBlobCoroutine(blobType, blobID));
        }

        private IEnumerator LoadBlobCoroutine(udpInfo.BlobVersion blobType, UInt32 blobID)
        {
            byte[] payload = new byte[5];
            payload[0] = (byte)blobType;

            BitConverter.GetBytes(blobID).CopyTo(payload, 1);

            yield return udpClient.SendUDPRequest(
                   request: new UDPRequest(
                       udpInfo.SAVE_BLOB,
                       null,
                       data: payload
                   ),
                   timeout: 1.0f
               );
        }

        /// <summary>
        /// Requests backend for the nominal eye offsets - the distance in millimeters that each eye is from the nominal position.
        /// </summary>
        /// <param name="setterCallback">A callback that takes two vector3's for handling the returned data if successful. Order: Right eye, Left eye.</param>
        /// <returns></returns>
        public Coroutine RequestEyeOffsets(System.Action<Vector3, Vector3> setterCallback)
        {
            return StartCoroutine(RequestEyeOffsetsCoroutine(setterCallback));
        }

        private IEnumerator RequestEyeOffsetsCoroutine(System.Action<Vector3, Vector3> setterCallback)
        {
            // data will be ack code byte, property, xyz for right, xyz for left
            UDPRequestCallback handle_data = (byte[] data, UDPRequestStatus status) =>
            {
                if (status == UDPRequestStatus.AckSuccess)
                {
                    int i = 0;

                    byte test;
                    data.ReadNextInt8(ref i, out test);
                    data.ReadNextInt8(ref i, out test);
                    Vector3 rEyeOffset;
                    data.ReadNextVector3(ref i, out rEyeOffset);
                    Vector3 lEyeOffset;
                    data.ReadNextVector3(ref i, out lEyeOffset);
                    setterCallback(rEyeOffset, lEyeOffset);
                }
                else
                {
                    if (data.Length > 0)
                    {
                        Debug.LogError("ERROR WHEN REQUESTING NOMINAL EYE OFFSETS: " + udpInfo.GetAckPacketTypeInfo(data[0]));
                    } else
                    {
                        Debug.LogError("ERROR WHEN REQUESTING NOMINAL EYE OFFSETS: " + status.ToString());
                    }
                }
            };

            yield return udpClient.SendUDPRequest(
                new UDPRequest(
                    udpInfo.PROPERTY_GET,
                    handle_data,
                    new byte[] { (byte)udpInfo.PropertyType.NORMALIZED_EYE_OFFSETS }
                    )
                );

        }

        /// <summary>
        /// Register a validation point for analytics purposes
        /// </summary>
        /// <param name="pointVector">point to register (where we think the gaze vector _should_ be.</param>
        /// <param name="callback">callback can be null. callback is invoked as part of the acknowledgement in the UDPRequest</param>
        /// <param name="timeout">time in seconds to wait for an achnowledgement.</param>
        /// <returns>A couroutine handler.</returns>
        public Coroutine RegisterValidationPoint(Vector3 pointVector, UDPRequestCallback callback, float timeout = 1.0f)
        {
            return StartCoroutine(RegisterValidationPointCoroutine(pointVector, callback, timeout));
        }

        private IEnumerator RegisterValidationPointCoroutine(Vector3 pointVector, UDPRequestCallback callback, float timeout = 1.0f)
        {
            yield return udpClient.SendUDPRequest(
                request: new UDPRequest(
                    udpInfo.REGISTER_VALIDATION_POINT,
                    callback,
                    data: pointVector.ToBytes()
                ),
                timeout: 1.0f
            );
            yield return null;
        }

        /// <summary>
        /// Registers a new center point for the calibration to align the projected gaze with the users current gaze in case of slip (tracker movement over time)
        /// </summary>
        public Coroutine RegisterRecenterPoint(Vector3 pointVector, UDPRequestCallback callback = null, float timeout = 1.0f)
        {
            return StartCoroutine(RegisterRecenterPointCoroutine(pointVector, callback, timeout));
        }

        private IEnumerator RegisterRecenterPointCoroutine(Vector3 pointVector, UDPRequestCallback callback = null, float timeout = 1.0f)
        {
            yield return udpClient.SendUDPRequest(
                request: new UDPRequest(
                    udpInfo.START_RECENTER_PROCEDURE,
                    callback,
                    data: pointVector.ToBytes()
                ),
                timeout: 1.0f
            );
            yield return null;
        }

        /// <summary>
        /// Tell the eytracking service to start listening for validation points.
        /// </summary>
        public Coroutine StartValidationSession()
        {
            return StartCoroutine(SendRequestWaitForAck(udpInfo.START_VALIDATION, 1.0f, "Requesting to start validation session."));
        }

        /// <summary>
        /// Tell the eyetracking service to stop listening for validation points.
        /// </summary>
        public Coroutine StopValidationSession()
        {
            return StartCoroutine(SendRequestWaitForAck(udpInfo.STOP_VALIDATION, 1.0f, "Requesting to stop validation session."));
        }

        private IEnumerator SendRequestWaitForAck(byte packetType, float timeout, string info = "")
        {
            yield return udpClient.SendUDPRequest(
                request: new UDPRequest(
                    packetType: packetType,
                    ackCallback: (data, result) => {
                        if (result == UDPRequestStatus.Timeout)
                        {
                            if (ErrorCallback != null)
                            {
                                //Debug.LogError(string.Concat("UDP Request Timeout! \ninfo: ",info)); 
                                CallError(string.Concat("UDP Request Timeout! \ninfo: ", info));
                            }
                        }
                        else if (result == UDPRequestStatus.AckError)
                        {
                            if (ErrorCallback != null)
                            {
                                //Debug.LogError(string.Concat("UDP Request Error! \ninfo: ",info));
                                CallError(string.Concat("UDP Request Error! \ninfo: ", info));
                            }
                        }
                    },
                    data: EMPTY_BYTE
                ),
                timeout: 1.0f
            );
            
        }
        /// <summary>
        /// Message backend that we want to start a calibration, so that it can start listening.
        /// yields after ack is recieved or timeout is hit.
        /// </summary>
        /// <param name="successCallback">Called when request ack is recieved as successful</param>
        public Coroutine QueryBeginCalibration(float timeout = 1, UDPRequestCallback successCallback = null)
        {
            return StartCoroutine(MessageBeginCalibrationCoroutine(timeout, successCallback));
        }
        public Coroutine QueryBeginCalibration()
        {
            return StartCoroutine(QueryBeginCalibrationCoroutine());
        }
        private IEnumerator MessageBeginCalibrationCoroutine(float timeout, UDPRequestCallback successCallback)
        {

            Calibrating = true;
            yield return udpClient.SendUDPRequest(
                request: new UDPRequest(
                    udpInfo.START_CALIBRATION,
                    successCallback,
                    EMPTY_BYTE
                )
            );
        }
        private IEnumerator QueryBeginCalibrationCoroutine()
        {

            Calibrating = true;
            yield return udpClient.SendUDPRequest(
                    request: new UDPRequest(
                        packetType: udpInfo.START_CALIBRATION,
                        ackCallback: null,
                        data: EMPTY_BYTE
                    )
                );
        }

        /// <summary>
        /// Message backend that we want to start a validation, so that it can start listening.
        /// yields after ack is recieved or timeout is hit.
        /// </summary>
        /// <param name="successCallback">Called when request ack is recieved as successful</param>
        public Coroutine QueryBeginValidation(float timeout = 1, UDPRequestCallback successCallback = null)
        {
            return StartCoroutine(MessageBeginValidationCoroutine(timeout, successCallback));
        }
        public Coroutine QueryBeginValidation()
        {
            return StartCoroutine(QueryBeginValidationCoroutine());
        }
        private IEnumerator MessageBeginValidationCoroutine(float timeout, UDPRequestCallback successCallback)
        {

            Calibrating = true;
            yield return udpClient.SendUDPRequest(
                request: new UDPRequest(
                    udpInfo.START_VALIDATION,
                    successCallback,
                    EMPTY_BYTE
                ),
                timeout: 1.0f
            );
        }
        private IEnumerator QueryBeginValidationCoroutine()
        {

            Calibrating = true;
            yield return udpClient.SendUDPRequest(
                    request: new UDPRequest(
                        packetType: udpInfo.START_VALIDATION,
                        ackCallback: null,
                        data: EMPTY_BYTE
                    ),
                    timeout: 1.0f
                );
        }

        /// <summary>
        /// Aborts the currently active calibration procedure (requires a message to backend). Discards already used points and reverts to an uncalibrated state.
        /// </summary>
        public Coroutine AbortCalibration()
        {
            return StartCoroutine(AbortCalibrationCoroutine());
        }

        private IEnumerator AbortCalibrationCoroutine()
        {
            yield return udpClient.SendUDPRequest(
                request: new UDPRequest(
                    udpInfo.ABORT_CALIBRATION,
                    null,
                    EMPTY_BYTE 
                ),
                timeout: 1.0f
            );
            Calibrating = false;
        }

        /// <summary>
        /// Message backend that we want to stop calibration session, so it will stop listening and run
        /// a calculation based on the points recieved.
        /// yields after ack is recieved or timeout is hit.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="successCallback"></param>
        /// <returns></returns>
        public Coroutine QueryEndCalibration(float timeout = 1, UDPRequestCallback successCallback = null)
        {
            return StartCoroutine(MessageEndCalibrationCoroutine(timeout, successCallback));
        }

        private IEnumerator MessageEndCalibrationCoroutine(float timeout, UDPRequestCallback successCallback)
        {

            UDPRequestCallback calresult = (byte[] data, UDPRequestStatus status) =>
            {
                Debug.Log("Status " + status);
                if (status == UDPRequestStatus.AckSuccess)
                {
                    Streams.Gaze.Start();
                    Calibrated = true;
                }
                else
                {
                    if (data.Length > 0)
                    {
                        Debug.Log(string.Format("Failed Calibration with code: 0x{0:X2}", data[0]));
                    }
                    else
                    {
                        Debug.Log(string.Format("Failed Calibration with no code."));
                    }

                }
            };

            calresult += successCallback;

            yield return udpClient.SendUDPRequest(
                request: new UDPRequest(
                    udpInfo.STOP_CALIBRATION,
                    calresult,
                    EMPTY_BYTE
                ),
                timeout: 1.0f
            );

            Calibrating = false;
        }

        /// <summary>
        /// Message backend that we want to stop calibration session, so it will stop listening and run
        /// a calculation based on the points recieved.
        /// yields after ack is recieved or timeout is hit.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="successCallback"></param>
        /// <returns></returns>
        public Coroutine QueryEndValidation(float timeout = 1, UDPRequestCallback successCallback = null)
        {
            return StartCoroutine(MessageEndValidationCoroutine(timeout, successCallback));
        }
        private IEnumerator MessageEndValidationCoroutine(float timeout, UDPRequestCallback successCallback)
        {

            UDPRequestCallback valresult = (byte[] data, UDPRequestStatus status) =>
            {
                if (status != UDPRequestStatus.AckSuccess)
                {
                    if (data.Length > 0)
                    {
                        Debug.Log(string.Format("Failed Validation with code: 0x{0:X2}", data[0]));
                    }
                    else
                    {
                        Debug.Log(string.Format("Failed Validation with no code."));
                    }

                }
            };

            valresult += successCallback;

            yield return udpClient.SendUDPRequest(
                request: new UDPRequest(
                    udpInfo.STOP_VALIDATION,
                    valresult,
                    EMPTY_BYTE
                ),
                timeout: 1.0f
            );
            Debug.Log("Status " + false);
            Calibrating = false;
        }

        /// <summary>
        /// Register a calibration point with backend
        /// </summary>
        /// <param name="calibrationPointRelativeToCamera"></param>
        /// <param name="callback"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public Coroutine RegisterCalibrationPoint(Vector3 calibrationPointRelativeToCamera, UDPRequestCallback callback, float timeout = 4.0f)
        {
            return StartCoroutine(RegisterCalibrationPointCoroutine(calibrationPointRelativeToCamera, callback, timeout));
        }
        private IEnumerator RegisterCalibrationPointCoroutine(Vector3 calibrationPointRelativeToCamera, UDPRequestCallback callback, float timeout = 4.0f)
        {
            yield return udpClient.SendUDPRequest(
                request: new UDPRequest(
                    packetType: udpInfo.REGISTER_CALIBRATION_POINT,
                    ackCallback: callback,
                    data: calibrationPointRelativeToCamera.InvertZ().ToBytes()
                ),
                timeout: timeout
            );
        }

        /// <summary> 
        /// Message backend to start the autotune procedure
        /// </summary>
        public Coroutine RequestAutotune(float timeout = 4.0f)
        {
            return StartCoroutine(RequestAutotuneCoroutine(timeout));
        }
        private IEnumerator RequestAutotuneCoroutine(float timeout = 4.0f)
        {
            double curLastTrackerReadyTime = lastTrackerReadySignalTime;
            Debug.Log("Requesting autotune");
            if (RunningAutotune)
            {
                yield break;
            }
            RunningAutotune = true;
            // start by sending the autotune request.
            yield return udpClient.SendUDPRequest(
                new UDPRequest(
                    packetType: udpInfo.START_RANGING,
                    ackCallback: (data, result) => {
                        if (result != UDPRequestStatus.AckSuccess)
                        {
                            if (data.Length > 0)
                            {
                                CallError(udpInfo.GetAckPacketTypeInfo(data[0]));
                            }
                            else
                            {
                                CallError(result.ToString());
                            }
                        }
                    },
                    data: new byte[0]
                ),
                timeout: timeout
            );
            Debug.Log("Autotune request sent.");
            float time_waited = 0;
            float tracker_ready_timeout = 8.0f;
            yield return new WaitUntil(() =>
            {
                time_waited += Time.deltaTime;
                if (time_waited > tracker_ready_timeout)
                {
                    Debug.LogError("Timeout when waiting for tracker ready signal");
                    return true;
                } 
                if (curLastTrackerReadyTime != lastTrackerReadySignalTime)
                {
                    Debug.Log("Tracker ready signal recieved, autotune complete.");
                    return true;
                }
                return false;
            });

            RunningAutotune = false;
        }

        /// <summary>
        /// Requests tracker ready state from backend.
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public Coroutine RequestTrackerStatus(UDPRequestCallback callback = null)
        {
            return StartCoroutine(RequestTrackerStatusCoroutine(callback));
        }

        private IEnumerator RequestTrackerStatusCoroutine(UDPRequestCallback callback = null)
        {
            // return self._handler.request(PacketType.TRACKER_STATE, callback=callback)
            yield return udpClient.SendUDPRequest(new UDPRequest(
                udpInfo.GET_TRACKER_STATE,
                callback,
                new byte[] {}));
        }
        
        /// <summary>
        /// Will return a coroutine handle and yield when completed,
        /// calling resultCallback with the resulting data
        /// </summary>
        /// <param name="resultCallback"></param>
        public Coroutine RequestDeviceSerial(Action<string> resultCallback)
        {
            return StartCoroutine(RequestDeviceSerialCoroutine(resultCallback));
        }

        private IEnumerator RequestDeviceSerialCoroutine(Action<string> resultCallback)
        {
            yield return udpClient.SendUDPRequest(new UDPRequest(
                udpInfo.GET_SYSTEM_INFO,
                (byte[] data, UDPRequestStatus status) =>
                {
                    // think about validating sys-info request type, in case we send several at once.
                    if (status == UDPRequestStatus.AckSuccess || status == UDPRequestStatus.Recieved)
                    {
                        resultCallback(System.Text.Encoding.UTF8.GetString(data, 2, data.Length - 2));
                    }
                    else if (status == UDPRequestStatus.Timeout)
                    {
                        resultCallback("");
                    }
                },
                new byte[] { (byte)udpInfo.SystemInfo.DEVICE_SERIAL }
            ));
        }

    }
}