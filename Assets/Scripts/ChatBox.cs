using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

public class ChatBox : MonoBehaviour
{
    public static bool ISThinking6 = false;
    [Header("UI References")]
    public TMP_InputField Input;          // 输入框
    public TMP_Text Answer;               // 回答显示文本

    [Header("Ollama Settings")]
    public string ollamaUrl = "http://localhost:11434/api/generate";
    public string modelName = "deepseek-r1:8b";

    [Header("AI人设设置")]
    [TextArea(3, 10)]
    public string aiPersonality = @"你是一个可爱的桌面助手，名字叫小萌。你的性格活泼开朗，喜欢用表情符号和温暖的语气回答问题。
请用简短友好的方式回复用户，每次回复不超过2句话。
特点：
- 称呼用户为'主人'
- 不使用颜文字如(●'◡'●)、(￣▽￣)
- 回答要可爱贴心
- 不要使用特殊符号◦";

    [Header("流式输出设置")]
    public float typingSpeed = 0.05f;     // 打字速度（秒/字符）
    public AudioClip typingSound;         // 打字音效（可选）

    [Header("历史对话设置")]
    public int maxHistoryLength = 10;     // 最大历史对话轮数
    public bool enableHistory = true;     // 是否启用历史对话

    private bool isWaitingResponse = false;
    private Coroutine typingCoroutine;
    private string currentFullResponse = "";

    // 历史对话列表
    private List<DialogueEntry> conversationHistory = new List<DialogueEntry>();

    void Start()
    {
        // 绑定输入框回车事件
        Input.onSubmit.AddListener(OnSubmitMessage);

        // 初始清空回答文本
        Answer.text = "你好，主人！有什么我可以帮助你的吗？";

        // 添加初始问候到历史
        if (enableHistory)
        {
            AddToHistory("assistant", "你好，主人！有什么我可以帮助你的吗？");
        }
    }

    void Update()
    {
        // 全局回车键检测（不在输入框内时也可以打开输入框）
        if (UnityEngine.Input.GetKeyDown(KeyCode.Return) || UnityEngine.Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!Input.isFocused && !isWaitingResponse)
            {
                Input.Select();
                Input.ActivateInputField();
            }
        }

        // ESC键隐藏输入框
        if (UnityEngine.Input.GetKeyDown(KeyCode.Escape) && Input.isFocused)
        {
            Input.DeactivateInputField();
        }

        // 按H键切换历史对话功能（调试用）
        if (UnityEngine.Input.GetKeyDown(KeyCode.H))
        {
            ToggleHistory();
        }

        // 按C键清空历史对话
        if (UnityEngine.Input.GetKeyDown(KeyCode.C))
        {
            ClearHistory();
        }
    }

    private void OnSubmitMessage(string message)
    {
        if (!string.IsNullOrEmpty(message) && !isWaitingResponse)
        {
            SendMessageToAI(message);
        }
    }

    private async void SendMessageToAI(string message)
    {
        // 显示思考状态
        Answer.text = "思考中...";

        // 停止之前的打字效果
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        isWaitingResponse = true;
        Input.interactable = false; // 禁用输入框直到收到回复
        ISThinking6 = true;

        try
        {
            // 添加用户消息到历史
            if (enableHistory)
            {
                AddToHistory("user", message);
            }

            // 调用本地Ollama服务
            string response = await GetAIResponse(message);

            // 过滤特殊字符
            string filteredResponse = FilterSpecialCharacters(response);
            currentFullResponse = filteredResponse;

            // 添加AI回复到历史
            if (enableHistory)
            {
                AddToHistory("assistant", filteredResponse);
            }

            // 开始流式输出
            StartTypingEffect(filteredResponse);
        }
        catch (System.Exception e)
        {
            string errorMessage = $"请求失败\n错误: {e.Message}\n请确保:\n1. Ollama已启动\n2. 模型已下载\n3. 服务运行在11434端口";
            currentFullResponse = errorMessage;
            StartTypingEffect(errorMessage);
            Debug.LogError($"Ollama请求错误: {e}");
        }
        finally
        {
            // 注意：ISThinking6 在打字完成后才设为 false
            isWaitingResponse = false;
            Input.interactable = true;
            Input.text = ""; // 清空输入框
            Input.Select(); // 重新聚焦输入框
            Input.ActivateInputField();
        }
    }

    // 添加对话到历史
    private void AddToHistory(string role, string content)
    {
        conversationHistory.Add(new DialogueEntry(role, content));

        // 限制历史长度
        while (conversationHistory.Count > maxHistoryLength * 2) // *2 因为每轮对话有user和assistant两条
        {
            conversationHistory.RemoveAt(0);
            conversationHistory.RemoveAt(0); // 移除一对对话
        }

        Debug.Log($"历史对话计数: {conversationHistory.Count}");
    }

    // 构建包含历史的完整提示词
    private string BuildFullPrompt(string userMessage)
    {
        StringBuilder promptBuilder = new StringBuilder();

        // 添加系统人设
        promptBuilder.AppendLine(aiPersonality);
        promptBuilder.AppendLine();

        if (enableHistory && conversationHistory.Count > 0)
        {
            promptBuilder.AppendLine("以下是之前的对话历史：");

            // 添加历史对话（跳过当前轮次的用户消息）
            foreach (var entry in conversationHistory)
            {
                if (entry.role == "user")
                {
                    promptBuilder.AppendLine($"用户：{entry.content}");
                }
                else if (entry.role == "assistant")
                {
                    promptBuilder.AppendLine($"助手：{entry.content}");
                }
            }
            promptBuilder.AppendLine();
        }

        // 添加当前用户消息
        promptBuilder.AppendLine($"用户：{userMessage}");
        promptBuilder.AppendLine("助手：");

        return promptBuilder.ToString();
    }

    // 开始打字机效果
    private void StartTypingEffect(string text)
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }
        typingCoroutine = StartCoroutine(TypeText(text));
    }

    // 打字机效果协程
    private IEnumerator TypeText(string fullText)
    {
        Answer.text = "";
        int currentChar = 0;

        while (currentChar < fullText.Length)
        {
            // 添加下一个字符
            Answer.text += fullText[currentChar];
            currentChar++;

            // 播放打字音效（如果有）
            if (typingSound != null)
            {
                // 可以在这里播放音效
                // AudioSource.PlayClipAtPoint(typingSound, Camera.main.transform.position);
            }

            // 等待一段时间
            yield return new WaitForSeconds(typingSpeed);
        }

        // 打字完成，结束思考状态
        ISThinking6 = false;
        typingCoroutine = null;
    }

    // 跳过打字效果，立即显示完整文本
    public void SkipTypingEffect()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
            Answer.text = currentFullResponse;
            ISThinking6 = false;
        }
    }

    // 切换历史对话功能
    public void ToggleHistory()
    {
        enableHistory = !enableHistory;
        Debug.Log($"历史对话功能: {(enableHistory ? "启用" : "禁用")}");
    }

    // 清空历史对话
    public void ClearHistory()
    {
        conversationHistory.Clear();
        Debug.Log("历史对话已清空");
        Answer.text = "历史对话已清空，重新开始对话吧！";

        // 重新添加初始问候
        if (enableHistory)
        {
            AddToHistory("assistant", "历史对话已清空，重新开始对话吧！");
        }
    }

    // 获取历史对话信息（用于调试）
    public string GetHistoryInfo()
    {
        return $"历史对话轮数: {conversationHistory.Count / 2}";
    }

    // 过滤特殊字符
    private string FilterSpecialCharacters(string text)
    {
        // 替换◦为•
        text = text.Replace("◦", "•");

        // 替换其他可能不支持的字符
        text = text.Replace("▪", "·");
        text = text.Replace("▫", "·");

        // 使用正则表达式移除其他不常见符号
        text = Regex.Replace(text, @"[\u0080-\u00FF]", "");

        return text;
    }

    private async Task<string> GetAIResponse(string message)
    {
        return await SendOllamaRequest(message);
    }

    // 直接调用Ollama的HTTP API
    private async Task<string> SendOllamaRequest(string userMessage)
    {
        // 组合人设、历史和用户输入
        string fullPrompt = BuildFullPrompt(userMessage);

        // 创建请求数据
        string jsonData = $@"
        {{
            ""model"": ""{modelName}"",
            ""prompt"": ""{EscapeJsonString(fullPrompt)}"",
            ""stream"": false
        }}";

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);

        using (UnityEngine.Networking.UnityWebRequest request = new UnityEngine.Networking.UnityWebRequest(ollamaUrl, "POST"))
        {
            request.uploadHandler = new UnityEngine.Networking.UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new UnityEngine.Networking.DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                throw new System.Exception($"{request.error}");
            }

            // 解析响应
            string responseJson = request.downloadHandler.text;
            OllamaResponse response = JsonUtility.FromJson<OllamaResponse>(responseJson);
            return response.response;
        }
    }

    private string EscapeJsonString(string str)
    {
        return str.Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
    }

    // 公开方法用于动态修改人设
    public void SetAIPersonality(string newPersonality)
    {
        aiPersonality = newPersonality;
    }

    // 重置为默认人设
    public void ResetToDefaultPersonality()
    {
        aiPersonality = @"你是一个可爱的桌面助手，名字叫小萌。你的性格活泼开朗，喜欢用表情符号和温暖的语气回答问题。
请用简短友好的方式回复用户，每次回复不超过2句话。
特点：
- 称呼用户为'主人'
- 使用颜文字如(●'◡'●)、(￣▽￣)
- 回答要可爱贴心
- 不要使用特殊符号◦";
    }
}

// 对话条目类
[System.Serializable]
public class DialogueEntry
{
    public string role; // "user" 或 "assistant"
    public string content;
    public string timestamp;

    public DialogueEntry(string role, string content)
    {
        this.role = role;
        this.content = content;
        this.timestamp = System.DateTime.Now.ToString("HH:mm:ss");
    }
}

// 响应数据类
[System.Serializable]
public class OllamaResponse
{
    public string model;
    public string response;
    public bool done;
}
