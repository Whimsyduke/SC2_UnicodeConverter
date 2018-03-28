using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Fluent;
using System.Windows.Data;
using System.Windows;

namespace SC2_UnicodeConverter
{
    /// <summary>
    /// 转换模式配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class SC2_StructConvertMode
    {
        #region 声明
        /// <summary>
        /// 编码规则
        /// </summary>
        public enum EnumCodeRuleType
        {
            /// <summary>
            /// 使用Utf-8
            /// </summary>
            Utf8,
            /// <summary>
            /// 使用Utf-16
            /// </summary>
            Utf16,
            /// <summary>
            /// 使用Utf-32
            /// </summary>
            Utf32,
        }

        /// <summary>
        /// 转码进制
        /// </summary>
        public enum EnumConvertScale
        {
            /// <summary>
            /// 使用16进制
            /// </summary>
            NumberType16,
            /// <summary>
            /// 使用10进制
            /// </summary>
            NumberType10,
            /// <summary>
            /// 使用8进制
            /// </summary>
            NumberType8,
            /// <summary>
            /// 使用2进制
            /// </summary>
            NumberType2,
        }

        /// <summary>
        /// 附加文本类型
        /// </summary>
        public enum EnumAdditionType
        {
            /// <summary>
            /// 输出调试信息
            /// </summary>
            DebugMsg,
            /// <summary>
            /// 文本
            /// </summary>
            AsText,
            /// <summary>
            /// 字符串
            /// </summary>
            AsString,
            /// <summary>
            /// 原文本
            /// </summary>
            OrigionalContent,
        }
        #endregion

        #region 属性

        /// <summary>
        /// 转码进制整数字典
        /// </summary>
        public static Dictionary<EnumConvertScale, int> ConvertScaleCount { get; private set; }

        /// <summary>
        /// 转码进制正则表达式
        /// </summary>
        public static Dictionary<EnumCodeRuleType, Dictionary<EnumConvertScale, string>> ConvertScaleRegularString { get; private set; }

        /// <summary>
        /// 转码模式名称
        /// </summary>
        [JsonProperty]
        public string ConvertModeName { get; set; }

        /// <summary>
        /// 是否为默认项
        /// </summary>
        [JsonProperty]
        public bool? IsDefault { get; set; }

        /// <summary>
        /// 图标路径
        /// </summary>
        [JsonProperty]
        public string IconPath { get; set; }

        /// <summary>
        /// 是否处理ASCII
        /// </summary>
        [JsonProperty]
        public bool IsProcessingASCII { get; set; }
        /// <summary>
        /// 是否使用大写字母
        /// </summary>
        [JsonProperty]
        public bool IsUppercaseChars { get; set; }
        /// <summary>
        /// 是否处理XML转码
        /// </summary>
        [JsonProperty]
        public bool IsXMLTranscoding { get; set; }

        /// <summary>
        /// 编码规则
        /// </summary>
        [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
        public EnumCodeRuleType CodeRuleType { get; set; }

        /// <summary>
        /// 转码进制
        /// </summary>
        [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
        public EnumConvertScale ConvertScale { get; set; }

        /// <summary>
        /// 附加文本
        /// </summary>
        [JsonProperty, JsonConverter(typeof(StringEnumConverter))]
        public EnumAdditionType AdditionType { get; set; }

        /// <summary>
        /// 前缀
        /// </summary>
        [JsonProperty]
        public string Prefix { get; set; }

        /// <summary>
        /// 后缀
        /// </summary>
        [JsonProperty]
        public string Suffix { get; set; }

        /// <summary>
        /// 正则表达式
        /// </summary>
        [JsonProperty]
        public string Regular { get; set; }

        /// <summary>
        /// 默认转换模式列表
        /// </summary>
        public static Dictionary<string, SC2_StructConvertMode> StructConvertModeDefaultList { get; set; }
        /// <summary>
        /// 转换模式列表
        /// </summary>
        public static Dictionary<string, SC2_StructConvertMode> StructConvertModeList { get; set; }

        /// <summary>
        /// 模式按钮
        /// </summary>
        public ToggleButton ModeButton { get; private set; }

        #endregion

        #region 构造函数

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static SC2_StructConvertMode() => StaticInit();
        /// <summary>
        /// 构造函数
        /// </summary>
        public SC2_StructConvertMode()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="name">转码模式名称</param>
        /// <param name="icon">图标路径</param>
        /// <param name="isDefault">是否处理ASCII</param>
        /// <param name="isProcessASCII">是否处理ASCII</param>
        /// <param name="isUppercase">是否使用大写字母</param>
        /// <param name="isXML">是否处理XML转码</param>
        /// <param name="codeRuleType">编码规则</param>
        /// <param name="convertScale">转码进制</param>
        /// <param name="additionType">附加文本</param>
        /// <param name="prefix">前缀</param>
        /// <param name="suffix">后缀</param>
        /// <param name="regular">正则表达式</param>
        public SC2_StructConvertMode(string name, string icon, bool isDefault, bool isProcessASCII, bool isUppercase, bool isXML, EnumCodeRuleType codeRuleType, EnumConvertScale convertScale, EnumAdditionType additionType, string prefix, string suffix, string regular)
        {
            ConvertModeName = name;
            IconPath = icon;
            IsDefault = isDefault;
            IsProcessingASCII = isProcessASCII;
            IsUppercaseChars = isUppercase;
            IsXMLTranscoding = isXML;
            CodeRuleType = codeRuleType;
            ConvertScale = convertScale;
            AdditionType = additionType;
            Prefix = prefix;
            Suffix = suffix;
            Regular = regular;
        }
        #endregion

        #region 方法

        /// <summary>
        /// 静态构造函数初始化
        /// </summary>
        private static void StaticInit()
        {
            ConvertScaleCount = new Dictionary<EnumConvertScale, int>
            {
                { EnumConvertScale.NumberType16, 16 },
                { EnumConvertScale.NumberType10, 10 },
                { EnumConvertScale.NumberType8, 8 },
                { EnumConvertScale.NumberType2, 2 },
            };
            ConvertScaleRegularString = new Dictionary<EnumCodeRuleType, Dictionary<EnumConvertScale, string>>
            {
                {
                    EnumCodeRuleType.Utf8,
                    new Dictionary<EnumConvertScale, string>
                    {
                        {EnumConvertScale.NumberType16, "[0-9a-fA-F]{1,2}" },
                        {EnumConvertScale.NumberType10, @"-?\d{1,3}" },
                        {EnumConvertScale.NumberType8, @"[0-7]{1,3}" },
                        {EnumConvertScale.NumberType2, @"[01]{1,8}" },
                    }
                },
                {
                    EnumCodeRuleType.Utf16,
                    new Dictionary<EnumConvertScale, string>
                    {
                        {EnumConvertScale.NumberType16, "[0-9a-fA-F]{1,8}" },
                        {EnumConvertScale.NumberType10, @"-?\d{1,11}" },
                        {EnumConvertScale.NumberType8, @"[0-7]{1,11}" },
                        {EnumConvertScale.NumberType2, @"[01]{1,32}" },
                    }
                },
                {
                    EnumCodeRuleType.Utf32,
                    new Dictionary<EnumConvertScale, string>
                    {
                        {EnumConvertScale.NumberType16, "[0-9a-fA-F]{1,8}" },
                        {EnumConvertScale.NumberType10, @"-?\d{1,11}" },
                        {EnumConvertScale.NumberType8, @"[0-7]{1,11}" },
                        {EnumConvertScale.NumberType2, @"[01]{1,32}" },
                    }
                },
            };
            StructConvertModeDefaultList = new Dictionary<string, SC2_StructConvertMode>
            {
                { "UIText_ToggleButton_SC2DefaultConfig_Header", new SC2_StructConvertMode("UIText_ToggleButton_SC2DefaultConfig_Header", "ui-editoricon-scripteditor_launchsc2.dds", true, false, false, false, EnumCodeRuleType.Utf8, EnumConvertScale.NumberType16, EnumAdditionType.OrigionalContent, "\\x", "", "") },
                { "UIText_ToggleButton_SC2UIXmlConfig_Header", new SC2_StructConvertMode("UIText_ToggleButton_SC2UIXmlConfig_Header", "ui-editoricon-general_ui.dds", true, false, false, true, EnumCodeRuleType.Utf32, EnumConvertScale.NumberType16, EnumAdditionType.OrigionalContent, "&#x", ";", "(?<=[\\s]val=\")[\\s\\S]+?(?=\"[\\s]*/>)") },
                { "UIText_ToggleButton_EmptyConfig_Header", new SC2_StructConvertMode("UIText_ToggleButton_EmptyConfig_Header", "ui-editoricons-layers_regions.dds", true, true, true, false, EnumCodeRuleType.Utf8, EnumConvertScale.NumberType16, EnumAdditionType.OrigionalContent, "", "", "") } ,
                { "UIText_ToggleButton_CustomConfig_Header", new SC2_StructConvertMode("UIText_ToggleButton_CustomConfig_Header", "ui-editoricon-general_ai.dds", true, false, false, false, EnumCodeRuleType.Utf8, EnumConvertScale.NumberType16, EnumAdditionType.OrigionalContent, "\\x", "", "") }
            };
            StructConvertModeList = new Dictionary<string, SC2_StructConvertMode>();
        }

        /// <summary>
        /// 分组方式
        /// </summary>
        /// <param name="obj">分组对象</param>
        /// <returns>分组结果</returns>
        public static string AdvancedGroupFunction(object obj)
        {
            ToggleButton button = obj as ToggleButton;
            if (button == null) return "UIText_GalleryGroupFilter_Other_Header";
            SC2_StructConvertMode mode = button.Tag as SC2_StructConvertMode;
            if (mode.IsDefault == true)
            {
                return "UIText_GalleryGroupFilter_Default_Header";
            }
            else if (mode.IsDefault == false)
            {
                return "UIText_GalleryGroupFilter_Custom_Header";
            }
            else
            {
                return "UIText_GalleryGroupFilter_Current_Header";
            }
        }
        /// <summary>
        /// 获取模式按钮
        /// </summary>
        /// <returns>模式按钮</returns>
        public ToggleButton GetToggleButton()
        {
            Uri uri = new Uri("pack://application:,,,/Assets/Image/" + IconPath);
            ModeButton = new ToggleButton
            {
                Icon = uri,
                LargeIcon = uri,
                Width = 60,
                GroupName = "ConvertMode",
                SizeDefinition = "Large",
                Tag = this,
            };
            ModeButton.Checked += ToggleButton_ConvertMode_Checked;
            if (IsDefault == true)
            {
                ModeButton.SetResourceReference(ToggleButton.HeaderProperty, ConvertModeName);
            }
            else
            {
                ModeButton.Header = ConvertModeName;
                Fluent.MenuItem item = new MenuItem
                {
                    Width = 100,
                    Tag = ModeButton,
                };
                item.SetResourceReference(MenuItem.IconProperty, "IMAGE_DeleteMode");
                item.SetResourceReference(MenuItem.HeaderProperty, "UIText_MenuItem_DelectConvertModeItem_Header");
                item.Click += MenuItem_DelectConvertModeItem_Click;
                ContextMenu menu = new ContextMenu();
                ModeButton.ContextMenu = menu;
                menu.Items.Add(item);
            }
            return ModeButton;
        }

        /// <summary>
        /// 获取不重复模式名称
        /// </summary>
        /// <returns>模式名称</returns>
        public static string GetNoDuplicateModeName(ResourceDictionary language)
        {
            string baseName = language["NameText_Constant_BaseConvertModeName_Prefix"] as string;
            int i = 0;
            while (StructConvertModeList.ContainsKey(baseName + i)) i++;
            return baseName + i;
        }

        /// <summary>
        /// 从UI读取数据
        /// </summary>
        public void ReadFromUI()
        {
            if (SC2_UnicodeConverter_Window.MainWindow.IsKeepUI) return;
            IsProcessingASCII = SC2_UnicodeConverter_Window.MainWindow.CheckBox_ProcessASCII.IsChecked == true;
            IsUppercaseChars = SC2_UnicodeConverter_Window.MainWindow.CheckBox_UppercaseCharacters.IsChecked == true;
            IsXMLTranscoding = SC2_UnicodeConverter_Window.MainWindow.CheckBox_XMLTranscode.IsChecked == true;
            CodeRuleType = (EnumCodeRuleType)SC2_UnicodeConverter_Window.MainWindow.ComboBox_CharacterCodeRuleType.SelectedIndex;
            ConvertScale = (EnumConvertScale)SC2_UnicodeConverter_Window.MainWindow.ComboBox_ConvertingScaleType.SelectedIndex;
            AdditionType = (EnumAdditionType)SC2_UnicodeConverter_Window.MainWindow.ComboBox_AdditionTextType.SelectedIndex;
            Prefix = SC2_UnicodeConverter_Window.MainWindow.TextBox_Prefix.Text;
            Suffix = SC2_UnicodeConverter_Window.MainWindow.TextBox_Suffix.Text;
            Regular = SC2_UnicodeConverter_Window.MainWindow.TextBox_RegularExpression.Text;
        }

        #endregion

        #region 控件事件

        /// <summary>
        /// 删除方案点击事件
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void MenuItem_DelectConvertModeItem_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem)) return;
            MenuItem item = sender as MenuItem;
            ToggleButton button = item.Tag as ToggleButton;
            if (button == null) return;
            SC2_UnicodeConverter_Window.MainWindow.InRibbonGallery_ConventMode.Items.Remove(button);
            StructConvertModeList.Remove(ConvertModeName);
        }

        /// <summary>
        /// 复制属性
        /// </summary>
        /// <param name="mode">模式</param>
        public void CopyProperty(SC2_StructConvertMode mode)
        {

            IsProcessingASCII = mode.IsProcessingASCII;
            IsUppercaseChars = mode.IsUppercaseChars;
            IsXMLTranscoding = mode.IsXMLTranscoding;

            CodeRuleType = mode.CodeRuleType;
            ConvertScale = mode.ConvertScale;
            AdditionType = mode.AdditionType;

            Prefix = mode.Prefix;
            Suffix = mode.Suffix;
            Regular = mode.Regular;
        }

        /// <summary>
        /// 转换方案选择事件
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void ToggleButton_ConvertMode_Checked(object sender, RoutedEventArgs e)
        {
            if (SC2_UnicodeConverter_Window.MainWindow == null) return;
            if (!(sender is ToggleButton)) return;
            ToggleButton button = sender as ToggleButton;
            SC2_UnicodeConverter_Window.SelectConvertModeButton = button;
            if (SC2_UnicodeConverter_Window.MainWindow.IsAutoChangeToCustom)
            {
                SC2_UnicodeConverter_Window.MainWindow.IsAutoChangeToCustom = false;
                StructConvertModeDefaultList["UIText_ToggleButton_CustomConfig_Header"].ReadFromUI();
                return;
            }
            SC2_UnicodeConverter_Window.MainWindow.IsKeepUI = true;
            SC2_UnicodeConverter_Window.MainWindow.IsKeepConvertMode = true;
            SC2_UnicodeConverter_Window.MainWindow.IsKeepInputHighlight = true;
            SC2_UnicodeConverter_Window.MainWindow.IsKeepOutputHighlight = true;
            SC2_UnicodeConverter_Window.MainWindow.CheckBox_ProcessASCII.IsChecked = IsProcessingASCII;
            SC2_UnicodeConverter_Window.MainWindow.CheckBox_UppercaseCharacters.IsChecked = IsUppercaseChars;
            SC2_UnicodeConverter_Window.MainWindow.CheckBox_XMLTranscode.IsChecked = IsXMLTranscoding;

            SC2_UnicodeConverter_Window.MainWindow.ComboBox_CharacterCodeRuleType.SelectedIndex = (int)CodeRuleType;
            SC2_UnicodeConverter_Window.MainWindow.ComboBox_ConvertingScaleType.SelectedIndex = (int)ConvertScale;
            SC2_UnicodeConverter_Window.MainWindow.ComboBox_AdditionTextType.SelectedIndex = (int)AdditionType;

            SC2_UnicodeConverter_Window.MainWindow.TextBox_Prefix.Text = Prefix;
            SC2_UnicodeConverter_Window.MainWindow.TextBox_Suffix.Text = Suffix;
            SC2_UnicodeConverter_Window.MainWindow.TextBox_RegularExpression.Text = Regular;
            SC2_UnicodeConverter_Window.MainWindow.IsKeepUI = false;
            SC2_UnicodeConverter_Window.MainWindow.IsKeepInputHighlight = false;
            SC2_UnicodeConverter_Window.MainWindow.IsKeepOutputHighlight = false;
            SC2_UnicodeConverter_Window.MainWindow.RefreshUI();
            SC2_UnicodeConverter_Window.MainWindow.SetInputHightlight();
            SC2_UnicodeConverter_Window.MainWindow.IsKeepConvertMode = false;
            StructConvertModeDefaultList["UIText_ToggleButton_CustomConfig_Header"].ReadFromUI();
        }
        #endregion
    }
}
