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
    [Header("连接")]
    public TMP_InputField serverAddr;
    public int serverPort = 6060;
    
    public Button connectButton;
    public Image statusImage;
    
    [Header("下使能")]
    public Button returnButton;
    
    
    [Header("速度模式")]
    public Button setVelocityModeButton;
    public Button startVelButton;
    public Button stopVelButton;
    public TMP_InputField targetVel;
    
    [Header("位置模式")]
    public Button setPositionModeButton;
    public Button moveToPosButton;
    public TMP_InputField targetPos;
    public TMP_InputField maxVel;
    public TMP_InputField maxAcc;
    
    [Header("阻抗模式")]
    public Button setImpedanceModeButton;
    public TMP_InputField stiffness;
    public TMP_InputField damping;
    public Button setStiffnessButton;
    public Button setDampingButton;
    
    [Header("零力模式")]
    public Button setZeroforceModeButton;
    
    
    [Header("状态显示")]
    public Button runStateButton;
    private TMP_Text stateText;
    public TMP_Text currentWorkModeText;
    public TMP_Text currentPosText;
    public TMP_Text currentVelText;
    public TMP_Text encoder1Text;
    public TMP_Text encoder2Text;
    public TMP_Text externalForceText;
    public TwistDeformer twistDeformer;
    
    [Header("配置显示")]
    public TMP_Text encoder1ResText;
    public TMP_Text encoder2ResText;
    public TMP_Text springStiffnessText;
    
    
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
        
        
        // 下使能
        returnButton.onClick.AddListener(() =>
        {
            Stop();
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
        
        stopVelButton.onClick.AddListener(() =>
        {
            if (is_connected)
            {
                SetVelocity(0.0f);
            }
            else
            {
                Debug.LogWarning("请先连接到服务器");
            }
        });
        
        // 位置模式
        setPositionModeButton.onClick.AddListener(() =>
        {
            if (is_connected)
            {
                
            }
        });

        
        // 阻抗模式
        setImpedanceModeButton.onClick.AddListener(() =>
        {
            if (is_connected)
            {
                // 刚进阻抗给一个默认值
                SetStiffness(float.Parse(stiffness.text));
                SetDamping(float.Parse(damping.text));

                
                SetWorkMode(WorkMode.Impedance);
            }
            else
            {
                Debug.LogWarning("请先连接到服务器");
            }
        });
        
        setStiffnessButton.onClick.AddListener(() =>
        {
            if (is_connected)
            {
                SetStiffness(float.Parse(stiffness.text));
            }
            else
            {
                Debug.LogWarning("请先连接到服务器");
            }
        } );
        
        setDampingButton.onClick.AddListener(() =>
        {
            if (is_connected)
            {
                SetDamping(float.Parse(damping.text));
            }
            else
            {
                Debug.LogWarning("请先连接到服务器");
            }
        });
        
        
        
        
        // 运行状态&重置Reset
        stateText = runStateButton.GetComponentInChildren<TextMeshProUGUI>();
        runStateButton.onClick.AddListener(() =>
        {
            if (is_connected)
            {
                Reset();
            }
            else
            {
                Debug.LogWarning("请先连接到服务器");
            }
        });



        StartCoroutine(UpdateStateLoop());

    }
    
    public bool StartCommunication()
    {
        
        Debug.Log($"开始连接到服务器: {serverAddr.text}:{serverPort}");

        socket = new RequestSocket();
        string address = $"tcp://{serverAddr.text}:{serverPort}";

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

        GetConfig();
        
        return true;
        
    }

    public void StopCommunication()
    {
        socket.Disconnect($"tcp://{serverAddr.text}:{serverPort}");
        
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
            if (socket.TryReceiveFrameBytes(TimeSpan.FromSeconds(2), out msg))
            {
                var feedback = ControlFeedback.Parser.ParseFrom(msg);
                if (feedback.FeedbackCase == ControlFeedback.FeedbackOneofCase.SetWorkMode)
                {
                    Debug.Log($"工作模式设置成功: {feedback.FeedbackCase}");
                    return true;

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

    /// <summary>
    /// 速度模式下速度设置
    /// </summary>
    /// <param name="vel">单位是°</param>
    /// <returns></returns>
    public bool SetVelocity(float vel)
    {
        float vel_rad = vel * Mathf.Deg2Rad;
        
        try
        {
            var command = new ControlCommand
            {
                SetVelocity = new SetVelocityCommand
                {
                    Vel = vel_rad
                }
            };
        
            byte[] data = command.ToByteArray();
            socket.SendFrame(data);
            
            byte[] msg;
            if (socket.TryReceiveFrameBytes(TimeSpan.FromSeconds(2), out msg))
            {
                var feedback = ControlFeedback.Parser.ParseFrom(msg);
                if (feedback.FeedbackCase == ControlFeedback.FeedbackOneofCase.SetVelocity)
                {
                    Debug.Log($"速度设置成功: {feedback.FeedbackCase}");
                    return true;

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

    public bool Stop()
    {
        try
        {
            var command = new ControlCommand
            {
                Stop = new StopCommand
                {
                }
            };
            
            byte[] data = command.ToByteArray();
            socket.SendFrame(data);
            
            byte[] msg;
            if (socket.TryReceiveFrameBytes(TimeSpan.FromSeconds(2), out msg))
            {
                var feedback = ControlFeedback.Parser.ParseFrom(msg);
                if (feedback.FeedbackCase == ControlFeedback.FeedbackOneofCase.Stop)
                {
                    Debug.Log($"停止运动成功: {feedback.FeedbackCase}");
                    return true;

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

    public bool GetConfig()
    {
        try
        {
            var command = new ControlCommand
            {
                GetConfig = new GetConfigCommand
                {
                }
            };
        
            byte[] data = command.ToByteArray();
            socket.SendFrame(data);
            
            byte[] msg;
            if (socket.TryReceiveFrameBytes(TimeSpan.FromSeconds(2), out msg))
            {
                var feedback = ControlFeedback.Parser.ParseFrom(msg);
                if (feedback.FeedbackCase == ControlFeedback.FeedbackOneofCase.Config)
                {
                    Debug.Log($"获取配置成功: {feedback.FeedbackCase}");
                    // 处理配置数据
                    UpdateConfigUI(feedback.Config);
                    
                    return true;
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
    
    public bool GetStatus()
    {
        try
        {
            var command = new ControlCommand
            {
                GetStatus = new GetStatusCommand
                {
                }
            };
        
            byte[] data = command.ToByteArray();
            socket.SendFrame(data);
            
            byte[] msg;
            if (socket.TryReceiveFrameBytes(TimeSpan.FromSeconds(2), out msg))
            {
                var feedback = ControlFeedback.Parser.ParseFrom(msg);
                if (feedback.FeedbackCase == ControlFeedback.FeedbackOneofCase.Status)
                {
                    // Debug.Log($"获取状态成功: {feedback.FeedbackCase}");
                    // 处理状态数据
                    
                    UpdateStatusUI(feedback.Status);
                    
                    return true;

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


    public bool Reset()
    {
        try
        {
            var command = new ControlCommand
            {
                Reset = new ResetCommand
                {
                }
            };
            
            byte[] data = command.ToByteArray();
            socket.SendFrame(data);
            
            byte[] msg;
            if (socket.TryReceiveFrameBytes(TimeSpan.FromSeconds(2), out msg))
            {
                var feedback = ControlFeedback.Parser.ParseFrom(msg);
                if (feedback.FeedbackCase == ControlFeedback.FeedbackOneofCase.Reset)
                {
                    Debug.Log($"重置成功: {feedback.FeedbackCase}");
                    
                    return true;

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
    
    
    public bool SetStiffness(float stiffnessValue)
    {
        try
        {
            var command = new ControlCommand
            {
                SetStiffness = new SetStiffnessCommand
                {
                    Stiffness = stiffnessValue / Mathf.Deg2Rad // 转换为弧度
                }
            };
        
            byte[] data = command.ToByteArray();
            socket.SendFrame(data);
            
            byte[] msg;
            if (socket.TryReceiveFrameBytes(TimeSpan.FromSeconds(2), out msg))
            {
                var feedback = ControlFeedback.Parser.ParseFrom(msg);
                if (feedback.FeedbackCase == ControlFeedback.FeedbackOneofCase.SetStiffness)
                {
                    Debug.Log($"刚度设置成功: {feedback.FeedbackCase}");
                    return true;

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
    
    public bool SetDamping(float dampingValue)
    {
        try
        {
            var command = new ControlCommand
            {
                SetDamping = new SetDampingCommand
                {
                    Damping = dampingValue / Mathf.Deg2Rad // 转换为弧度
                }
            };
        
            byte[] data = command.ToByteArray();
            socket.SendFrame(data);
            
            byte[] msg;
            if (socket.TryReceiveFrameBytes(TimeSpan.FromSeconds(2), out msg))
            {
                var feedback = ControlFeedback.Parser.ParseFrom(msg);
                if (feedback.FeedbackCase == ControlFeedback.FeedbackOneofCase.SetDamping)
                {
                    Debug.Log($"阻尼设置成功: {feedback.FeedbackCase}");
                    return true;

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


    void UpdateConfigUI(ConfigFeedback feedback)
    {
        // 编码器1分辨率
        encoder1ResText.text = $"{feedback.Encoder1Resolution} cnt/r"; // Encoder1分辨率
        // 编码器2分辨率
        encoder2ResText.text = $"{feedback.Encoder2Resolution} cnt/r"; // Encoder2分辨率
        // 弹簧刚度
        springStiffnessText.text = $"{feedback.SpringStiffness / Mathf.Rad2Deg} N·m/°"; // 扭簧刚度
    }
    
    
    
    void UpdateStatusUI(StatusFeedback feedback)
    {
        // 运行状态
        stateText.text = feedback.RunState.ToString();

        // 当前模式
        currentWorkModeText.text = feedback.WorkMode.ToString();
        // 当前位置
        double currentPosDeg = feedback.CurrentPosition * Mathf.Rad2Deg;
        currentPosText.text = currentPosDeg.ToString("F2") + " °"; // 保留两位小数
        // 当前速度
        double currentVelDeg = feedback.CurrentVelocity * Mathf.Rad2Deg;
        currentVelText.text = currentVelDeg.ToString("F2") + " °/s"; // 保留两位小数
        // 编码器1折算的角度
        double encoder1Deg = feedback.Encoder1Feedback * Mathf.Rad2Deg;
        encoder1Text.text = encoder1Deg.ToString("F2") + " °"; // 保留两位小数
        // 编码器2折算的角度
        double encoder2Deg = feedback.Encoder2Feedback * Mathf.Rad2Deg;
        encoder2Text.text = encoder2Deg.ToString("F2") + " °"; // 保留两位小数
        // 外力
        double externalForce = feedback.ExternalForce;
        externalForceText.text = externalForce.ToString("F2")  + " Nm"; // 保留两位小数

        twistDeformer.value = (float)feedback.SpringAngle * Mathf.Rad2Deg;;
        
        
    }
    
    
    
    
    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator UpdateStateLoop()
    {
        while (true)
        {
            if (is_connected)
            {
                if (!GetStatus())
                {
                    Debug.LogWarning("获取状态失败");
                }
            }
            
            yield return new WaitForSeconds(0.1f); // 每秒更新一次状态
        }
    }
    

    private void OnDestroy()
    {
        // if(is_connected)
        // {
        //     StopCommunication();
        // }
        
        NetMQConfig.Cleanup(false);
        
        
    }
}
