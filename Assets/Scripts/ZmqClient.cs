using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using NetMQ;
using NetMQ.Sockets;
using Google.Protobuf;
using Sea;

public class ZmqClient : MonoBehaviour
{

    private Thread zmqThread;
    
    public bool isRunning = false;
    
    // Start is called before the first frame update
    void Start()
    {
        StartCommunication();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    

    public void StartCommunication()
    {
        
        Debug.Log("开启反馈线程");
        isRunning = true;
        
        zmqThread = new Thread(() => {
                AsyncIO.ForceDotNet.Force();
                using (var subSocket = new SubscriberSocket())
                {
                    subSocket.Connect("tcp://192.168.1.94:6060");
                    subSocket.Subscribe("");

                    while (isRunning)
                    {
                        try
                        {
                            byte[] msg = subSocket.ReceiveFrameBytes();
                            var feedback = ControlFeedback.Parser.ParseFrom(msg);
                            switch (feedback.FeedbackCase)
                            {
                                case ControlFeedback.FeedbackOneofCase.Status:
                                    var status = feedback.Status;
                                    Debug.Log(status.ToString());
                                    break;
                                case ControlFeedback.FeedbackOneofCase.Config:
                                    var config = feedback.Config;
                                    Debug.Log(config.ToString());
                                    break;
                                case ControlFeedback.FeedbackOneofCase.None:
                                    Debug.LogWarning("Feedback类型错误！");
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("Receive Error: " +e.Message);
                        }
                    }
                }
            }
        );
        
        zmqThread.Start();
        
        
    }

    private void OnDestroy()
    {
        isRunning = false;
        if (zmqThread != null && zmqThread.IsAlive)
        {
            zmqThread.Join();
            zmqThread = null;
        }
        
        NetMQConfig.Cleanup();
        
        
    }
}
