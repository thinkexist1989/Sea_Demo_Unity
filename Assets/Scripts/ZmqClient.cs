using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using Google.Protobuf;
using Sea;
using TMPro;
using Unity.Collections;
using UnityEditor;
using UnityEngine.UI;


public class ZmqClient : MonoBehaviour
{
    // 连接
    public TMP_InputField serverAddr;
    public Button connectButton;
    public Image statusImage;
    
    // 速度模式
    public Button setVelocityModeButton;
    public Button startVelButton;
    public Button stopVelButton;
    public TMP_InputField targetVel;
    
    // 位置模式
    public Button setPositionModeButton;
    public Button moveToPosButton;
    public TMP_InputField targetPos;
    public TMP_InputField maxVel;
    public TMP_InputField maxAcc;
    
    // 阻抗模式
    public Button setImpedanceModeButton;
    public TMP_InputField stiffness;
    public TMP_InputField damping;
    
    // 零力模式
    public Button setZeroforceModeButton;
    
    
    
    
    
    
    // private Thread zmqThread;
    private RequestSocket socket;
    
    public bool is_connected = false;
    
    // Start is called before the first frame update
    void Start()
    {
        AsyncIO.ForceDotNet.Force();
        
        // 连接
        connectButton.onClick.AddListener(() =>
        {
            if (is_connected) // 如果已经连接，则断开连接
            {
                StopCommunication();
                connectButton.GetComponentInChildren<TextMeshProUGUI>().text = "未连接";
            }
            else // 如果未连接，则开始连接
            {
                if (StartCommunication())
                {
                    connectButton.GetComponentInChildren<TextMeshProUGUI>().text = "已连接";
                }
            }
        });
        
        // 速度模式
        setVelocityModeButton.onClick.AddListener(() =>
        {
            if (is_connected)
            {
                SetWorkMode(WorkMode.Velocity);
            }
            else
            {
                Debug.LogWarning("请先连接到服务器");
            }
        });
        
        startVelButton.onClick.AddListener(() =>
        {
            if (is_connected)
            {
                float vel;
                if (float.TryParse(targetVel.text, out vel))
                {
                    SetVelocity(vel);
                }
                else
                {
                    Debug.LogWarning("请输入有效的速度值");
                }
            }
            else
            {
                Debug.LogWarning("请先连接到服务器");
            }
        });
        
        
        
        
        
        
        
        
        
    }
    
    public bool StartCommunication()
    {
        
        Debug.Log("开始连接到服务器: " + serverAddr.text);

        socket = new RequestSocket();
        string address = $"tcp://{serverAddr.text}";

        try
        {
            socket.Connect(address);
        }
        catch (Exception e)
        {
            Debug.LogError("连接失败: " + e.Message);
            return false;
        }

        is_connected = true;
        // 更新连接状态图标
        statusImage.sprite = Resources.Load<Sprite>("Images/green_circle");
        
        
        return true;


        // isRunning = true;
        
        // zmqThread = new Thread(() => {
        //         using (var socket = new RequestSocket())
        //         {
        //             string address = $"tcp://{serverAddr.text}";
        //
        //             try
        //             {
        //                 socket.Connect(address);
        //             }
        //             catch (Exception e)
        //             {
        //                 Debug.LogError("连接失败: " + e.Message);
        //                 return;
        //             }
        //
        //             while (true)
        //             {
        //                 try
        //                 {
        //             
        //                     var command = new ControlCommand
        //                     {
        //                         SetWorkMode = new SetWorkModeCommand()
        //                         {
        //                             WorkMode = WorkMode.Impedance
        //                         }
        //                     };
        //             
        //                     byte[] data = command.ToByteArray();
        //                     socket.SendFrame(data);
        //                     
        //                     
        //                     byte[] msg;
        //                     if (socket.TryReceiveFrameBytes(TimeSpan.FromMilliseconds(100), out msg))
        //                     {
        //                         var feedback = ControlFeedback.Parser.ParseFrom(msg);
        //                         switch (feedback.FeedbackCase)
        //                         {
        //                             case ControlFeedback.FeedbackOneofCase.Status:
        //                                 var status = feedback.Status;
        //                                 Debug.Log(status.ToString());
        //                                 break;
        //                             case ControlFeedback.FeedbackOneofCase.Config:
        //                                 var config = feedback.Config;
        //                                 Debug.Log(config.ToString());
        //                                 break;
        //                             case ControlFeedback.FeedbackOneofCase.None:
        //                                 Debug.LogWarning("Feedback类型错误！");
        //                                 break;
        //                         }
        //                     }
        //                     else
        //                     {
        //                         Debug.LogWarning("未收到返回数据帧");
        //                     }
        //                     
        //                     
        //             
        //                 }
        //                 catch (Exception e)
        //                 {
        //                     Debug.LogError("Receive Error: " +e.Message);
        //                 }
        //             }
        //         }
        //     }
        // );
        
        // zmqThread.Start();
        
    }

    public void StopCommunication()
    {
        socket.Disconnect($"tcp://{serverAddr.text}");
        
        socket.Close();
        
        is_connected = false;
        statusImage.sprite = Resources.Load<Sprite>("Images/red_circle");

    }

    public bool SetWorkMode(WorkMode mode)
    {
        try
        {
            var command = new ControlCommand
            {
                SetWorkMode = new SetWorkModeCommand
                {
                    WorkMode = mode
                }
            };
        
            byte[] data = command.ToByteArray();
            socket.SendFrame(data);
            
            byte[] msg;
            if (socket.TryReceiveFrameBytes(TimeSpan.FromSeconds(1), out msg))
            {
                var feedback = ControlFeedback.Parser.ParseFrom(msg);
                if (feedback.FeedbackCase == ControlFeedback.FeedbackOneofCase.SetWorkMode)
                {
                    Debug.Log($"工作模式设置成功: {feedback.FeedbackCase}");
                    return false;

                }
                
                Debug.Log($"收到错误反馈数据: {feedback.FeedbackCase}");
                return false;
            }
            else
            {
                Debug.LogWarning("未收到返回数据帧");
                return false;
                
            }
            
        }
        catch (Exception e)
        {
            Debug.LogError("Receive Error: " +e.Message);
            return false;
        }
    }

    public bool SetVelocity(float vel)
    {
        try
        {
            var command = new ControlCommand
            {
                SetVelocity = new SetVelocityCommand
                {
                    Vel = vel
                }
            };
        
            byte[] data = command.ToByteArray();
            socket.SendFrame(data);
            
            byte[] msg;
            if (socket.TryReceiveFrameBytes(TimeSpan.FromMilliseconds(1000), out msg))
            {
                var feedback = ControlFeedback.Parser.ParseFrom(msg);
                if (feedback.FeedbackCase == ControlFeedback.FeedbackOneofCase.SetVelocity)
                {
                    Debug.Log($"速度设置成功: {feedback.FeedbackCase}");
                    return false;

                }
                
                Debug.Log($"收到错误反馈数据: {feedback.FeedbackCase}");
                return false;
            }
            else
            {
                Debug.LogWarning("未收到返回数据帧");
                return false;
                
            }
            
        }
        catch (Exception e)
        {
            Debug.LogError("Receive Error: " +e.Message);
            return false;
        }
    }
    
    
    // Update is called once per frame
    void Update()
    {
        
    }
    
    private void OnDestroy()
    {
        if(is_connected)
        {
            StopCommunication();
        }
        
        NetMQConfig.Cleanup();
        
        
    }
}
