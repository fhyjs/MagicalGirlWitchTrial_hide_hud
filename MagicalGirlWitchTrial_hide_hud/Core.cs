using Il2CppNaninovel;
using Il2CppNaninovel.UI;
using MelonLoader;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

[assembly: MelonInfo(typeof(MagicalGirlWitchTrial_hide_hud.Core), "MagicalGirlWitchTrial_hide_hud", "1.0.0", "hanana", null)]
[assembly: MelonGame("Re,AER", "manosaba")]

namespace MagicalGirlWitchTrial_hide_hud
{
    public class Core : MelonMod
    {
        private GameObject naninovelUI;
        private bool hidden;
        private bool inited;
        // Toast 消息类
        private class ToastMessage
        {
            public string Text;
            public float StartTime;
            public float Duration;
        }
       
        private readonly List<ToastMessage> toasts = new();
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Initialized.");
        }
        // 添加 toast
        private void ShowToast(string msg, float duration = 3f)
        {
            var toast = new ToastMessage
            {
                Text = msg,
                StartTime = Time.time,
                Duration = duration
            };
            toasts.Add(toast);

            // 只在第一次 toast 时注册渲染函数
            if (toasts.Count == 1)
                MelonEvents.OnGUI.Subscribe(DrawToasts, 100);
        }
        public override void OnSceneWasInitialized(int buildIndex, string sceneName)
        {
            LoggerInstance.Msg($"Scene {sceneName} with build index {buildIndex} has been loaded!");
        }
        public override void OnUpdate()
        {
            base.OnUpdate();
            if (inited != Il2CppNaninovel.Engine.Initialized)
            {
                inited = Il2CppNaninovel.Engine.Initialized;
                ShowToast("HANANA的【魔法少女的魔女审判】隐藏HUD模组（已经注入成功）");
                ShowToast("在游戏内按下[F6]切换隐藏状态");
            }
            if (Keyboard.current != null && Keyboard.current.f6Key.wasPressedThisFrame)
            {
                DumpAllNaninovelUIs();
                if ((!IsAutoToggleVisible()|| IsTitleLogoVisible()) && !hidden)
                {
                    ShowToast("不在游戏内，无法切换HUD,强制切换会故障");
                    return;
                }
                LoggerInstance.Msg("F6:HANANA模组->魔法少女的魔女审判->隐藏HUD->转换");
                if (naninovelUI == null)
                {
                    var root = GameObject.Find("Naninovel<Runtime>");
                    if (root != null)
                    {
                        var ui = root.transform.Find("UI");
                        if (ui != null)
                            naninovelUI = ui.gameObject;
                    }
                }

                if (naninovelUI != null)
                {
                    hidden = !hidden;
                    naninovelUI.SetActive(!hidden);
                    LoggerInstance.Msg("HUD 切换: " + hidden);
                    ShowToast("HUD 切换: " + hidden);
                }
            }
        }
        private void DumpAllNaninovelUIs()
        {
            if (!Engine.Initialized)
            {
                MelonLogger.Msg("Naninovel 未初始化");
                return;
            }

            var uiManager = Engine.GetService<IUIManager>();
            if (uiManager == null)
            {
                MelonLogger.Msg("UIManager 获取失败");
                return;
            }

            // 创建一个 ICollection<IManagedUI>
            var managedUIs = new Il2CppSystem.Collections.Generic.List<IManagedUI>();


            uiManager.GetManagedUIs(managedUIs.Cast < Il2CppSystem.Collections.Generic.ICollection<Il2CppNaninovel.UI.IManagedUI>>());

            foreach (var ui in managedUIs)
            {
                if (ui == null) continue;
                MelonLogger.Msg($"UI: {ui.GetType().ToString} | Visible: {ui.Visible}");
            }
        }
        private bool IsTitleLogoVisible()
        {
            if (!Engine.Initialized)
            {
                MelonLogger.Msg("Naninovel 未初始化");
                return true;
            }

            var uiManager = Engine.GetService<IUIManager>();
            if (uiManager == null)
            {
                MelonLogger.Msg("UIManager 获取失败");
                return true;
            }

            // 创建一个 ICollection<IManagedUI>
            var managedUIs = new Il2CppSystem.Collections.Generic.List<IManagedUI>();


            uiManager.GetManagedUIs(managedUIs.Cast<Il2CppSystem.Collections.Generic.ICollection<Il2CppNaninovel.UI.IManagedUI>>());
            int num=0;
            foreach (var ui in managedUIs)
            {
                if (ui == null) continue;
                num++;
            }
            return num < 3;
        }

        private bool IsAutoToggleVisible()
        {
            var autoToggle = GameObject.Find("AutoToggle");
            if (autoToggle == null || !autoToggle.activeInHierarchy)
                return false;

            // 检查 CanvasGroup alpha
            var canvasGroup = autoToggle.GetComponent<CanvasGroup>();
            if (canvasGroup != null && canvasGroup.alpha <= 0f)
                return false;

            // 检查 CanvasRenderer alpha
            var renderer = autoToggle.GetComponent<CanvasRenderer>();
            if (renderer != null && renderer.GetAlpha() <= 0f)
                return false;

            return true; // 实际可见
        }
        private void DrawToasts()
        {
            float yOffset = 10f;

            for (int i = toasts.Count - 1; i >= 0; i--)
            {
                var t = toasts[i];
                if (Time.time - t.StartTime > t.Duration)
                {
                    toasts.RemoveAt(i);
                    continue;
                }

                float width = 500f;
                float height = 180f;
                float x = Screen.width - width - 10f; // 右上角偏移
                float y = yOffset;

                // 半透明黑背景
                Color bgColor = new Color(0f, 0f, 0.3f, 0.8f);
                GUI.color = bgColor;
                GUI.Box(new Rect(x, y, width, height), GUIContent.none);

                // 白色文字
                GUI.color = Color.red;
                GUI.Label(new Rect(x + 10f, y, width - 20f, height), $"<size=40><b>{t.Text}</b></size>");

                yOffset += height + 5f;
            }

            if (toasts.Count == 0)
                MelonEvents.OnGUI.Unsubscribe(DrawToasts);
        }

        private void DumpSceneHierarchy()
        {
            Scene scene = SceneManager.GetActiveScene();

            MelonLogger.Msg("====== 场景: " + scene.name + " ======");

            var roots = scene.GetRootGameObjects();

            foreach (var root in roots)
            {
                PrintTransform(root.transform, 0);
            }
        }

        private void PrintTransform(Transform t, int indent)
        {
            string prefix = new string(' ', indent * 2);

            MelonLogger.Msg(prefix + "- " + t.name);

            for (int i = 0; i < t.childCount; i++)
            {
                PrintTransform(t.GetChild(i), indent + 1);
            }
        }
    }
}