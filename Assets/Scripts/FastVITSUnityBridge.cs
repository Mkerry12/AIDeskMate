using System.Collections;
using UnityEngine;
using System.Diagnostics;
using System.IO;

public class FastVITSFinalTest : MonoBehaviour
{
    public string fastVITSFolderPath = @"E:\steam\steamapps\workshop\content\2488350\3010132930";

    void Start()
    {
        UnityEngine.Debug.Log("FastVITS 最终测试");
        StartCoroutine(TestWithCommonParameters());
    }

    private IEnumerator TestWithCommonParameters()
    {
        string exePath = Path.Combine(fastVITSFolderPath, "FastVITS.exe");
        string configPath = Path.Combine(fastVITSFolderPath, "config.json");
        string modelPath = Path.Combine(fastVITSFolderPath, "SoraGinko.pth");

        // 常见的 FastVITS 参数组合
        string[][] testCases = {
            new[] { "-i \"测试文本\" -o test.wav", "输入输出参数" },
            new[] { "--input \"测试\" --output test.wav --model \"" + modelPath + "\"", "完整参数" },
            new[] { "--config \"" + configPath + "\" --model \"" + modelPath + "\" --text \"测试\" --out test.wav", "配置文件参数" },
            new[] { "-s 0 -t \"测试文本\"", "说话人和文本" },
            new[] { "--speaker 0 --text \"测试\" --output test.wav", "标准参数" },
            new[] { "0 \"测试文本\" test.wav", "位置参数" }
        };

        foreach (var testCase in testCases)
        {
            string args = testCase[0];
            string description = testCase[1];

            UnityEngine.Debug.Log($"测试: {description}");
            UnityEngine.Debug.Log($"命令: FastVITS.exe {args}");

            yield return StartCoroutine(RunProcessTest(exePath, args, description));
            yield return new WaitForSeconds(2f);
        }
    }

    private IEnumerator RunProcessTest(string exePath, string arguments, string description)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = exePath,
            Arguments = arguments,
            WorkingDirectory = Path.GetDirectoryName(exePath),
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        try
        {
            using (Process process = new Process())
            {
                process.StartInfo = startInfo;
                process.Start();

                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                process.WaitForExit(5000);

                if (!string.IsNullOrEmpty(output))
                    UnityEngine.Debug.Log($"{description} 输出: {output}");
                if (!string.IsNullOrEmpty(error))
                    UnityEngine.Debug.LogError($"{description} 错误: {error}");

                // 检查是否生成了文件
                CheckGeneratedFiles();
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"{description} 异常: {e.Message}");
        }

        yield return null; // 添加这行来修复返回值问题
    }

    private void CheckGeneratedFiles()
    {
        string[] possibleFiles = { "test.wav", "output.wav", "out.wav", "*.wav" };

        foreach (string filePattern in possibleFiles)
        {
            try
            {
                string[] files = Directory.GetFiles(fastVITSFolderPath, filePattern);
                foreach (string file in files)
                {
                    UnityEngine.Debug.Log($"发现生成的文件: {file}");
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning($"检查文件 {filePattern} 时出错: {e.Message}");
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartCoroutine(TestWithCommonParameters());
        }
    }
}