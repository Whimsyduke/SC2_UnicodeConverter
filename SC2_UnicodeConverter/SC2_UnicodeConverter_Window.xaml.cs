using Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using WH_CommonControlLibrary.Functionality.MultiLanguage;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Xml.Linq;
using ICSharpCode.AvalonEdit.Highlighting;
using System.ComponentModel;

namespace SC2_UnicodeConverter
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SC2_UnicodeConverter_Window : RibbonWindow
    {
        #region 声明

        /// <summary>
        /// 处理附加文本委托
        /// </summary>
        /// <param name="isError">转换出错</param>
        /// <param name="textListInput">输入文本列表</param>
        /// <param name="textListOutput">输出文本列表</param>
        /// <param name="msg">消息</param>
        public delegate void DelegateCodeFuncAdditionalText(ref bool isError, List<string> textListInput, out List<string> textListOutput, ref string msg);

        /// <summary>
        /// 处理正则表达式委托
        /// </summary>
        /// <param name="isError">转换出错</param>
        /// <param name="regular">正则表达式</param>
        /// <param name="textListInput">输入文本列表</param>
        /// <param name="textListOutput">输出文本列表</param>
        /// <param name="msg">消息</param>
        public delegate void DelegateCodeFuncRegularExpression(ref bool isError, string regular, List<string> textListInput, out List<string> textListOutput, ref string msg);

        /// <summary>
        /// 编码过程字符处理委托
        /// </summary>
        /// <param name="isProcessASCII">是否处理ASCII</param>
        /// <param name="isUppercase">是否使用大写字母</param>
        /// <param name="isXML">是否处理XML转码</param>
        /// <param name="convertScale">转码进制</param>
        /// <param name="prefix">前缀文本</param>
        /// <param name="suffix">后缀文本</param>
        /// <param name="textInput">输入文本</param>
        /// <returns>结果文本</returns>
        public delegate string DelegateCodeFuncEncodeFormatCharacters(bool isProcessASCII, bool isUppercase, bool isXML, SC2_StructConvertMode.EnumConvertScale convertScale, string prefix, string suffix, string textInput);
        /// <summary>
        /// 解码过程字符处理委托
        /// </summary>
        /// <param name="isError">转换出错</param>
        /// <param name="textListInput">输入文本列表</param>
        /// <param name="textListOutput">输出文本列表</param>
        /// <param name="msg">消息</param>
        /// <param name="isXML">是否处理XML转码</param>
        /// <param name="convertScale">转码进制</param>
        /// <param name="prefix">前缀文本</param>
        /// <param name="suffix">后缀文本</param>
        public delegate void DelegateCodeFuncDecodeFormatCharacters(ref bool isError, List<string> textListInput, out List<string> textListOutput, ref string msg, bool isXML, SC2_StructConvertMode.EnumConvertScale convertScale, string prefix, string suffix);

        /// <summary>
        /// 处理方式枚举
        /// </summary>
        public enum EnumProcessingMode
        {
            /// <summary>
            /// 编码
            /// </summary>
            Encode,
            /// <summary>
            /// 解码
            /// </summary>
            Decode,
        }
        #endregion

        #region 字段
        // (?<=")[^"\\\r\n]*(?:\\.[^"\\\r\n]*)*(?=")
        // (?<=StringToText\s*\(\s*")[^"\\\r\n]*(?:\\.[^"\\\r\n]*)*(?="\s*\))
        // (?<=TriggerDebugOutput\s*\(\s*\d+,\s*StringToText\s*\(\s*")[^"\\\r\n]*(?:\\.[^"\\\r\n]*)*(?="\s*\)\s*,)
        private readonly Regex RegexDecodeString = new Regex(@"(?<="")[^""\\\r\n]*(?:\\.[^""\\\r\n]*)*(?="")", RegexOptions.Compiled);
        private readonly Regex RegexDecodeText = new Regex(@"(?<=StringToText\s*\(\s*"")[^""\\\r\n]*(?:\\.[^""\\\r\n]*)*(?=""\s*\))", RegexOptions.Compiled);
        private readonly Regex RegexDecodeDebugMsg = new Regex(@"(?<=TriggerDebugOutput\s*\(\s*\d+,\s*StringToText\s*\(\s*"")[^""\\\r\n]*(?:\\.[^""\\\r\n]*)*(?=""\s*\)\s*,)", RegexOptions.Compiled);
        private readonly Regex RegexIsProcessASCII = new Regex("[\\u0000-\\u0009\\u000b\\u000c\\u000e-\\u007f]", RegexOptions.Compiled);
        private readonly Regex RegexRegularEscapeCharacter = new Regex(@"[\$\(\)\*\+\.\[\]\?\\\^\{\}\|#]", RegexOptions.Compiled);
        private Regex RegexTempCodeValue;
        private Encoding EncodingClassOfCodeRuleType;
        private const string ConfigFileName = "Config.json";
        private readonly XDocument BaseHighligthXshd = XDocument.Parse(@"<?xml version=""1.0""?><SyntaxDefinition name = ""Custom Highlighting"" xmlns=""http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008""></SyntaxDefinition>");
        #endregion

        #region 属性

        /// <summary>
        /// 多语言支持
        /// </summary>
        private WH_MultiLanguageSupport MulitiLanguageSupport { get; set; }

        /// <summary>
        /// 处理方式配置
        /// </summary>
        private EnumProcessingMode EnumProcessingModeConfiguration { get; set; }

        /// <summary>
        /// 编码规则配置
        /// </summary>
        private SC2_StructConvertMode.EnumCodeRuleType EnumCodeRuleTypeConfiguration { get; set; }

        /// <summary>
        /// 编码规则配置
        /// </summary>
        private SC2_StructConvertMode.EnumConvertScale EnumConvertScaleConfiguration { get; set; }

        /// <summary>
        /// 附加文本配置
        /// </summary>
        private SC2_StructConvertMode.EnumAdditionType EnumAdditionTypeConfiguration { get; set; }

        /// <summary>
        /// 是否保持UI
        /// </summary>
        public bool IsKeepUI { get; set; }

        /// <summary>
        /// 是否为转换方式修改配置
        /// </summary>
        public bool IsKeepConvertMode { get; set; }

        /// <summary>
        /// 是否设置输入高亮
        /// </summary>
        public bool IsKeepInputHighlight { get; set; }

        /// <summary>
        /// 是否设置输出高亮
        /// </summary>
        public bool IsKeepOutputHighlight { get; set; }

        /// <summary>
        /// 是否为自动切换到自定义
        /// </summary>
        public bool IsAutoChangeToCustom { get; set; }

        /// <summary>
        /// 正则表达式有效
        /// </summary>
        public bool IsRegularExpressionValid { get; private set; }

        /// <summary>
        /// 预处理的前缀正则表达式
        /// </summary>
        private string PreparedPrefixRegexString { get; set; }

        /// <summary>
        /// 预处理的后缀正则表达式
        /// </summary>
        private string PreparedSuffixRegexString { get; set; }

        /// <summary>
        /// 主窗口
        /// </summary>
        public static SC2_UnicodeConverter_Window MainWindow { get; private set; }

        /// <summary>
        /// 使用的转换模式按钮
        /// </summary>
        public static Fluent.ToggleButton SelectConvertModeButton { get; set; }


        /// <summary>
        /// 软件版本信息
        /// </summary>
        public static FileVersionInfo VersionInfo { get; set; }

        /// <summary>
        /// 编码处理函数字典
        /// </summary>
        public Dictionary<EnumProcessingMode, Func<string, string>> FuncCodingInputText { get; private set; }

        /// <summary>
        /// 附加文本处理函数字典
        /// </summary>
        public Dictionary<EnumProcessingMode, Dictionary<SC2_StructConvertMode.EnumAdditionType, DelegateCodeFuncAdditionalText>> FuncAdditionalTextCoding { get; private set; }

        /// <summary>
        /// 正则表达式处理函数字典
        /// </summary>
        public Dictionary<EnumProcessingMode, DelegateCodeFuncRegularExpression> FuncRegularExpressionCoding { get; private set; }

        /// <summary>
        /// 编码过程字符处理函数字典
        /// </summary>
        public Dictionary<SC2_StructConvertMode.EnumCodeRuleType, DelegateCodeFuncEncodeFormatCharacters> FuncEncodeFormatCharactersCoding { get; private set; }

        /// <summary>
        /// 解码过程字符处理函数字典
        /// </summary>
        public Dictionary<SC2_StructConvertMode.EnumCodeRuleType, DelegateCodeFuncDecodeFormatCharacters> FuncDecodeFormatCharactersCoding { get; private set; }

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public SC2_UnicodeConverter_Window()
        {
            InitDelegate();
            IsRegularExpressionValid = true;
            IsKeepUI = true;
            MainWindow = this;
            IsKeepInputHighlight = true;
            IsKeepOutputHighlight = true;
            InitializeComponent();
            IsKeepInputHighlight = false;
            IsKeepOutputHighlight = false;
            VersionInfo = FileVersionInfo.GetVersionInfo(System.Windows.Forms.Application.ExecutablePath);
            MulitiLanguageSupport = new WH_MultiLanguageSupport();
            List<string> languageCultures = new List<string>();
            string currentLanguage = MulitiLanguageSupport.InitializeMultiLanguageSupport("Language\\", ref languageCultures);
            foreach (string select in languageCultures)
            {
                Fluent.ToggleButton item = new ToggleButton
                {
                    Header = MulitiLanguageSupport.UILanguages[select]["LanguageName"],
                    Tag = select,
                    GroupName = "Language",
                    SizeDefinition = new RibbonControlSizeDefinition("Middle"),
                    Icon = Application.Current.Resources["IMAGE_LanguageItem"],
                    Width = 120,
                };
                if (select == currentLanguage)
                {
                    item.IsChecked = true;
                }
                item.Checked += MulitiLanguageItem_Checked;
                Gallery_LanguageList.Items.Add(item);
            }
            ChangeLanguage(currentLanguage);
            IsKeepUI = false;
        }

        #endregion

        #region 方法

        /// <summary>
        /// 初始化委托
        /// </summary>
        private void InitDelegate()
        {
            #region 编码处理

            FuncCodingInputText = new Dictionary<EnumProcessingMode, Func<string, string>>
            {
                {
                    EnumProcessingMode.Encode,
                    delegate (string input)
                    {
                        if (string.IsNullOrEmpty(input)) return input;
                        string errMsg = "";
                        bool isError = false;
                        List<string> strings = new List<string> { input };

                        FuncRegularExpressionCoding[EnumProcessingMode.Encode](ref isError, TextBox_RegularExpression.Text, strings, out strings, ref errMsg);
                        FuncAdditionalTextCoding[EnumProcessingMode.Encode][EnumAdditionTypeConfiguration](ref isError, strings, out strings, ref errMsg);

                        if (isError)
                        {
                            return errMsg;
                        }
                        else
                        {
                            string result = "";
                            foreach (string select in strings)
                            {
                                result += "\r\n" + select;
                            }
                            if (string.IsNullOrEmpty(result))
                            {
                                return "";
                            }
                            else
                            {
                                return result.Substring(2);
                            }
                        }
                    }
                },
                {
                    EnumProcessingMode.Decode,
                    delegate (string input)
                    {
                        if (string.IsNullOrEmpty(input)) return input;
                        string errMsg = "";
                        bool isError = false;
                        List<string> strings = new List<string> { input };

                        FuncAdditionalTextCoding[EnumProcessingMode.Decode][EnumAdditionTypeConfiguration](ref isError, strings, out strings, ref errMsg);
                        FuncDecodeFormatCharactersCoding[EnumCodeRuleTypeConfiguration](ref isError, strings, out strings, ref errMsg, CheckBox_XMLTranscode.IsChecked == true, EnumConvertScaleConfiguration, TextBox_Prefix.Text, TextBox_Suffix.Text);

                        if (isError)
                        {
                            return errMsg;
                        }
                        else
                        {
                            string result = "";
                            foreach (string select in strings)
                            {
                                result += "\r\n" + select;
                            }
                            if (string.IsNullOrEmpty(result))
                            {
                                return "";
                            }
                            else
                            {
                                return result.Substring(2);
                            }
                        }
                    }
                },
            };

            #endregion

            #region 附加文本

            FuncAdditionalTextCoding = new Dictionary<EnumProcessingMode, Dictionary<SC2_StructConvertMode.EnumAdditionType, DelegateCodeFuncAdditionalText>>
            {
                {
                    EnumProcessingMode.Encode,
                    new Dictionary<SC2_StructConvertMode.EnumAdditionType, DelegateCodeFuncAdditionalText>
                    {
                        {
                            SC2_StructConvertMode.EnumAdditionType.DebugMsg,
                            delegate (ref bool isError, List<string> textListInput, out List<string> textListOutput, ref string msg)
                            {
                                if (isError)
                                {
                                    textListOutput = textListInput;
                                    return;
                                }
                                textListOutput = textListInput.Select(r=> "TriggerDebugOutput(1, StringToText(\"" + r + "\"), true);").ToList();
                                return;
                            }
                        },
                        {
                            SC2_StructConvertMode.EnumAdditionType.AsText,
                            delegate (ref bool isError, List<string> textListInput, out List<string> textListOutput, ref string msg)
                            {
                                if (isError)
                                {
                                    textListOutput = textListInput;
                                    return;
                                }
                                textListOutput = textListInput.Select(r=> "StringToText(\"" + r + "\")").ToList();
                                return;
                            }
                        },
                        {
                            SC2_StructConvertMode.EnumAdditionType.AsString,
                            delegate (ref bool isError, List<string> textListInput, out List<string> textListOutput, ref string msg)
                            {
                                if (isError)
                                {
                                    textListOutput = textListInput;
                                    return;
                                }
                                textListOutput = textListInput.Select(r=> "\"" + r + "\"").ToList();
                                return;
                            }
                        },
                        {
                            SC2_StructConvertMode.EnumAdditionType.OrigionalContent,
                            delegate (ref bool isError, List<string> textListInput, out List<string> textListOutput, ref string msg)
                            {
                                textListOutput = textListInput;
                                return;
                            }
                        },
                    }
                },
                {
                    EnumProcessingMode.Decode,
                    new Dictionary<SC2_StructConvertMode.EnumAdditionType, DelegateCodeFuncAdditionalText>
                    {
                        {
                            SC2_StructConvertMode.EnumAdditionType.DebugMsg,
                            delegate (ref bool isError, List<string> textListInput, out List<string> textListOutput, ref string msg)
                            {
                                if (isError)
                                {
                                    textListOutput = textListInput;
                                    return;
                                }
                                textListOutput = new List<string>();
                                foreach (var select in textListInput)
                                {
                                    textListOutput.AddRange(RegexDecodeDebugMsg.Matches(select).OfType<Match>().Select(r=>r.Value).ToList());
                                }
                                if (textListOutput.Count == 0)
                                {
                                    isError = true;
                                    msg = App.CurrentLanguage["ErrorText_Decode_AdditionType_DebugMsg_0"] as string;
                                }
                                return;
                            }
                        },
                        {
                            SC2_StructConvertMode.EnumAdditionType.AsText,
                            delegate (ref bool isError, List<string> textListInput, out List<string> textListOutput, ref string msg)
                            {
                                if (isError)
                                {
                                    textListOutput = textListInput;
                                    return;
                                }
                                textListOutput = new List<string>();
                                foreach (var select in textListInput)
                                {
                                    textListOutput.AddRange(RegexDecodeText.Matches(select).OfType<Match>().Select(r=>r.Value).ToList());
                                }
                                if (textListOutput.Count == 0)
                                {
                                    isError = true;
                                    msg = App.CurrentLanguage["ErrorText_Decode_AdditionType_AsText_0"] as string;
                                }
                                return;
                            }
                        },
                        {
                            SC2_StructConvertMode.EnumAdditionType.AsString,
                            delegate (ref bool isError, List<string> textListInput, out List<string> textListOutput, ref string msg)
                            {
                                if (isError)
                                {
                                    textListOutput = textListInput;
                                    return;
                                }
                                textListOutput = new List<string>();
                                foreach (var select in textListInput)
                                {
                                    textListOutput.AddRange(RegexDecodeString.Matches(select).OfType<Match>().Select(r=>r.Value).ToList());
                                }
                                if (textListOutput.Count == 0)
                                {
                                    isError = true;
                                    msg = App.CurrentLanguage["ErrorText_Decode_AdditionType_AsString_0"] as string;
                                }
                                return;
                            }
                        },
                        {
                            SC2_StructConvertMode.EnumAdditionType.OrigionalContent,
                            delegate (ref bool isError, List<string> textListInput, out List<string> textListOutput, ref string msg)
                            {
                                textListOutput = textListInput;
                                return;
                            }
                        },
                    }
                },
            };

            #endregion

            #region 正则表达式

            FuncRegularExpressionCoding = new Dictionary<EnumProcessingMode, DelegateCodeFuncRegularExpression>
            {
                {
                    EnumProcessingMode.Encode,
                    delegate (ref bool isError, string regular, List<string> textListInput, out List<string> textListOutput, ref string msg)
                    {
                        if (isError)
                        {
                            textListOutput = textListInput;
                            return;
                        }
                        textListOutput = new List<string>();
                        if (!IsRegularExpressionValid)
                        {
                            isError = true;
                            msg = App.CurrentLanguage["ErrorText_Encode_RegularExpression_ErrorSyntex_0"] as string;
                            return;
                        }
                        // 正则表达式为空时不处理
                        if (string.IsNullOrEmpty(regular))
                        {
                            foreach (var select in textListInput)
                        {
                            textListOutput.Add(FuncEncodeFormatCharactersCoding[EnumCodeRuleTypeConfiguration](CheckBox_ProcessASCII.IsChecked == true, CheckBox_UppercaseCharacters.IsChecked == true, CheckBox_XMLTranscode.IsChecked == true, EnumConvertScaleConfiguration, TextBox_Prefix.Text, TextBox_Suffix.Text, select));
                        }
                            return;
                        }

                        Regex regex = new Regex(regular);
                        int count = 0;
                        foreach (var select in textListInput)
                        {
                            if (regex.IsMatch(select)) count++;
                            textListOutput.Add(regex.Replace(select, EncodeMatchEvaluatorFunc));
                        }
                        if (count == 0)
                        {
                            isError = true;
                            msg = App.CurrentLanguage["ErrorText_Encode_RegularExpression_NoMatch_0"] as string;
                        }
                        return;
                    }
                },
                {
                    EnumProcessingMode.Decode,
                    delegate (ref bool isError, string regular, List<string> textListInput, out List<string> textListOutput, ref string msg)
                    {
                        textListOutput = textListInput;
                        return;
                    }
                },
            };

            #endregion

            #region 编码过程字符处理

            FuncEncodeFormatCharactersCoding = new Dictionary<SC2_StructConvertMode.EnumCodeRuleType, DelegateCodeFuncEncodeFormatCharacters>
            {
                {
                    SC2_StructConvertMode.EnumCodeRuleType.Utf8,
                    delegate (bool isProcessASCII, bool isUppercase, bool isXML, SC2_StructConvertMode.EnumConvertScale convertScale, string prefix, string suffix, string textInput)
                        {
                        StringBuilder strBuilder = new StringBuilder("");
                        TextElementEnumerator teEnum = StringInfo.GetTextElementEnumerator(textInput);
                        while (teEnum.MoveNext())
                        {
                            SplitConvertCharacter(teEnum.GetTextElement(), 1, isProcessASCII, isUppercase, isXML,convertScale, prefix, suffix, ref strBuilder, Encoding.UTF8, BitConverterToInt8);
                        }
                        return strBuilder.ToString();
                    }
                },
                {
                    SC2_StructConvertMode.EnumCodeRuleType.Utf16,
                    delegate (bool isProcessASCII, bool isUppercase, bool isXML, SC2_StructConvertMode.EnumConvertScale convertScale, string prefix, string suffix, string textInput)
                    {
                        StringBuilder strBuilder = new StringBuilder("");
                        TextElementEnumerator teEnum = StringInfo.GetTextElementEnumerator(textInput);
                        while (teEnum.MoveNext())
                        {
                            SplitConvertCharacter(teEnum.GetTextElement(), 2, isProcessASCII, isUppercase, isXML,convertScale, prefix, suffix, ref strBuilder, Encoding.Unicode, BitConverterToInt16);
                        }
                        return strBuilder.ToString();
                    }
                },
                {
                    SC2_StructConvertMode.EnumCodeRuleType.Utf32,
                    delegate (bool isProcessASCII, bool isUppercase, bool isXML, SC2_StructConvertMode.EnumConvertScale convertScale, string prefix, string suffix, string textInput)
                    {
                        StringBuilder strBuilder = new StringBuilder("");
                        TextElementEnumerator teEnum = StringInfo.GetTextElementEnumerator(textInput);
                        while (teEnum.MoveNext())
                        {
                            SplitConvertCharacter(teEnum.GetTextElement(), 4, isProcessASCII, isUppercase, isXML,convertScale, prefix, suffix, ref strBuilder, Encoding.UTF32, BitConverterToInt32);
                        }
                        return strBuilder.ToString();
                    }
                },
            };

            #endregion

            #region 解码过程字符处理

            FuncDecodeFormatCharactersCoding = new Dictionary<SC2_StructConvertMode.EnumCodeRuleType, DelegateCodeFuncDecodeFormatCharacters>
            {
                {
                    SC2_StructConvertMode.EnumCodeRuleType.Utf8,
                    delegate (ref bool isError, List<string> textListInput, out List<string> textListOutput, ref string msg, bool isXML, SC2_StructConvertMode.EnumConvertScale convertScale, string prefix, string suffix)
                    {
                        if (isError)
                        {
                            textListOutput = textListInput;
                            return;
                        }

                        Regex regex = PrepareDecodeFormatCharacters(isXML, SC2_StructConvertMode.EnumCodeRuleType.Utf8, convertScale, prefix, suffix);

                        EncodingClassOfCodeRuleType = Encoding.UTF8;

                        textListOutput = new List<string>();
                        if (isXML)
                        {
                            foreach (var select in textListInput)
                            {
                                select.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&").Replace("&apos;", "\"").Replace("&quot;", "'");
                                textListOutput.Add(regex.Replace(select, DecodeMatchEvaluatorFunc));
                                select.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&").Replace("&apos;", "\"").Replace("&quot;", "'");
                            }
                        }
                        else
                        {
                            foreach (var select in textListInput)
                            {
                                textListOutput.Add(regex.Replace(select, DecodeMatchEvaluatorFunc));
                            }
                        }
                        return;
                    }
                },
                {
                    SC2_StructConvertMode.EnumCodeRuleType.Utf16,
                    delegate (ref bool isError, List<string> textListInput, out List<string> textListOutput, ref string msg, bool isXML, SC2_StructConvertMode.EnumConvertScale convertScale, string prefix, string suffix)
                    {
                        if (isError)
                        {
                            textListOutput = textListInput;
                            return;
                        }

                        Regex regex = PrepareDecodeFormatCharacters(isXML, SC2_StructConvertMode.EnumCodeRuleType.Utf16, convertScale, prefix, suffix);

                        EncodingClassOfCodeRuleType = Encoding.Unicode;

                        textListOutput = new List<string>();
                        if (isXML)
                        {
                            foreach (var select in textListInput)
                            {
                                select.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&").Replace("&apos;", "\"").Replace("&quot;", "'");
                                textListOutput.Add(regex.Replace(select, DecodeMatchEvaluatorFunc));
                                select.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&").Replace("&apos;", "\"").Replace("&quot;", "'");
                            }
                        }
                        else
                        {
                            foreach (var select in textListInput)
                            {
                                textListOutput.Add(regex.Replace(select, DecodeMatchEvaluatorFunc));
                            }
                        }
                        return;
                    }
                },
                {
                    SC2_StructConvertMode.EnumCodeRuleType.Utf32,
                    delegate (ref bool isError, List<string> textListInput, out List<string> textListOutput, ref string msg, bool isXML, SC2_StructConvertMode.EnumConvertScale convertScale, string prefix, string suffix)
                    {
                        if (isError)
                        {
                            textListOutput = textListInput;
                            return;
                        }

                        Regex regex = PrepareDecodeFormatCharacters(isXML, SC2_StructConvertMode.EnumCodeRuleType.Utf32, convertScale, prefix, suffix);

                        EncodingClassOfCodeRuleType = Encoding.UTF32;

                        textListOutput = new List<string>();
                        if (isXML)
                        {
                            foreach (var select in textListInput)
                            {
                                select.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&").Replace("&apos;", "\"").Replace("&quot;", "'");
                                textListOutput.Add(regex.Replace(select, DecodeMatchEvaluatorFunc));
                                select.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&").Replace("&apos;", "\"").Replace("&quot;", "'");
                            }
                        }
                        else
                        {
                            foreach (var select in textListInput)
                            {
                                textListOutput.Add(regex.Replace(select, DecodeMatchEvaluatorFunc));
                            }
                        }
                        return;
                    }
                },
            };

            #endregion

        }

        /// <summary>
        /// Byte[]转整数8位
        /// </summary>
        /// <param name="bytes">Byte数据</param>
        /// <param name="startIndex">起始序号</param>
        /// <returns>返回整数</returns>
        private static int BitConverterToInt8(Byte[] bytes, int startIndex) => bytes[startIndex];

        /// <summary>
        /// Byte[]转整数16位
        /// </summary>
        /// <param name="bytes">Byte数据</param>
        /// <param name="startIndex">起始序号</param>
        /// <returns>返回整数</returns>
        private static int BitConverterToInt16(Byte[] bytes, int startIndex) => BitConverter.ToInt16(bytes, startIndex);

        /// <summary>
        /// Byte[]转整数32位
        /// </summary>
        /// <param name="bytes">Byte数据</param>
        /// <param name="startIndex">起始序号</param>
        /// <returns>返回整数</returns>
        private static int BitConverterToInt32(Byte[] bytes, int startIndex) => BitConverter.ToInt32(bytes, startIndex);

        /// <summary>
        /// 处理单个编码字符
        /// </summary>
        /// <param name="str">字符文本</param>
        /// <param name="splitCount">分割计数</param>
        /// <param name="isProcessASCII">是否处理ASCII</param>
        /// <param name="isUppercase">是否使用大写字母</param>
        /// <param name="isXML">是否处理XML转码</param>
        /// <param name="convertScale">转码进制</param>
        /// <param name="prefix">前缀文本</param>
        /// <param name="suffix">后缀文本</param>
        /// <param name="strBuilder">字符串构造器</param>
        /// <param name="encoding">编码方案</param>
        private void SplitConvertCharacter(string str, int splitCount, bool isProcessASCII, bool isUppercase, bool isXML, SC2_StructConvertMode.EnumConvertScale convertScale, string prefix, string suffix, ref StringBuilder strBuilder, Encoding encoding, Func<Byte[], int, int> func)
        {
            if (isXML)
            {
                switch (str)
                {
                    case "<":
                        strBuilder.Append("&lt;");
                        return;
                    case ">":
                        strBuilder.Append("&gt;");
                        return;
                    case "&":
                        strBuilder.Append("&amp;");
                        return;
                    case "\"":
                        strBuilder.Append("&apos;");
                        return;
                    case "'":
                        strBuilder.Append("&quot;");
                        return;
                    default:
                        break;
                }
            }
            if (!isProcessASCII && RegexIsProcessASCII.IsMatch(str))
            {
                strBuilder.Append(str);
            }
            else
            {
                int value = 0;
                char[] chars = str.ToCharArray();
                Byte[] datas = encoding.GetBytes(chars);
                for (int i = 0; i < datas.Length; i += splitCount)
                {
                    value = func(datas, i);
                    if (EnumCodeRuleTypeConfiguration == SC2_StructConvertMode.EnumCodeRuleType.Utf16 && datas.Count() == 4)
                    {
                        value = BitConverter.ToInt32(datas, i);
                        i += 2;
                    }
                    string numText = Convert.ToString(value, SC2_StructConvertMode.ConvertScaleCount[convertScale]);
                    if (convertScale == SC2_StructConvertMode.EnumConvertScale.NumberType2)
                    {
                        numText = numText.PadLeft(splitCount * 8, '0');
                    }
                    else if (convertScale == SC2_StructConvertMode.EnumConvertScale.NumberType16)
                    {
                        numText = numText.PadLeft(splitCount, '0');
                    }
                    if (isUppercase)
                    {
                        strBuilder.Append(prefix + numText.ToUpper() + suffix);
                    }
                    else
                    {
                        strBuilder.Append(prefix + numText.ToLower() + suffix);
                    }
                }
            }
        }

        /// <summary>
        /// 准备解码格式化字符串
        /// </summary>
        /// <param name="isXML">是否处理XML转码</param>
        /// <param name="codeRuleType">编码规则</param>
        /// <param name="convertScale">转码进制</param>
        /// <param name="prefix">前缀文本</param>
        /// <param name="suffix">后缀文本</param>
        /// <returns>分割正则表达式</returns>
        public Regex PrepareDecodeFormatCharacters(bool isXML, SC2_StructConvertMode.EnumCodeRuleType codeRuleType, SC2_StructConvertMode.EnumConvertScale convertScale, string prefix, string suffix)
        {
            string regular = "(" + PreparedPrefixRegexString + SC2_StructConvertMode.ConvertScaleRegularString[codeRuleType][convertScale] + PreparedSuffixRegexString + ")+";
            Regex regex;
            try
            {
                regex = new Regex(regular);
            }
            catch
            {
                throw new Exception("SC2_UnicodeConverter_Window.DecodeFormatCharacters(regex): RegexError!(" + regular + ")");
            }

            regular = "(?<=" + PreparedPrefixRegexString + ")" + SC2_StructConvertMode.ConvertScaleRegularString[codeRuleType][EnumConvertScaleConfiguration] + "(?=" + PreparedSuffixRegexString + ")";
            try
            {
                RegexTempCodeValue = new Regex(regular);
            }
            catch
            {
                throw new Exception("SC2_UnicodeConverter_Window.DecodeFormatCharacters(RegexTempCodeValue): RegexError!(" + regular + ")");
            }

            return regex;
        }

        /// <summary>
        /// 编码过程正则表达式替换处理函数
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>处理结果</returns>
        private string EncodeMatchEvaluatorFunc(Match input)
        {
            return FuncEncodeFormatCharactersCoding[EnumCodeRuleTypeConfiguration](CheckBox_ProcessASCII.IsChecked == true, CheckBox_UppercaseCharacters.IsChecked == true, CheckBox_XMLTranscode.IsChecked == true, EnumConvertScaleConfiguration, TextBox_Prefix.Text, TextBox_Suffix.Text, input.Value);
        }

        /// <summary>
        /// 解码过程正则表达式替换处理函数
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>处理结果</returns>
        private string DecodeMatchEvaluatorFunc(Match input)
        {
            string str = input.Value;

            MatchCollection matchs = RegexTempCodeValue.Matches(str);
            List<byte> buff = new List<byte>();

            for (int i = 0; i < matchs.Count; i++)
            {
                int value = Convert.ToInt32(matchs[i].Value, SC2_StructConvertMode.ConvertScaleCount[EnumConvertScaleConfiguration]);
                switch (EnumCodeRuleTypeConfiguration)
                {
                    case SC2_StructConvertMode.EnumCodeRuleType.Utf8:
                        buff.AddRange(BitConverter.GetBytes(value).Take(1));
                        break;
                    case SC2_StructConvertMode.EnumCodeRuleType.Utf16:
                        if (value >= 0xD800 && value <= 0xDFFF)
                        {
                            buff.AddRange(BitConverter.GetBytes(value));
                        }
                        else
                        {
                            buff.AddRange(BitConverter.GetBytes(value).Take(2));
                        }
                        break;
                    case SC2_StructConvertMode.EnumCodeRuleType.Utf32:
                        buff.AddRange(BitConverter.GetBytes(value));
                        break;
                    default:
                        break;
                }
            }

            return EncodingClassOfCodeRuleType.GetString(buff.ToArray());
        }


        /// <summary>
        /// 正则表达式转义字符处理函数
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>转码结果</returns>
        private static string RegexFormatMatchEvaluatorFunc(Match input) => "\\" + input.ToString();

        /// <summary>
        /// 切换游戏语言
        /// </summary>
        /// <param name="languageName">切换语言</param>
        private void ChangeLanguage(string languageName)
        {
            App.CurrentLanguage = MulitiLanguageSupport.UILanguages[languageName];
            WindowLanguage.MergedDictionaries.Clear();
            WindowLanguage.MergedDictionaries.Add(App.CurrentLanguage);
            Title = App.CurrentLanguage["WindowTitleText"] as string + " v" + VersionInfo.ProductVersion;
            DropDownButton_Language.Header = App.CurrentLanguage["LanguageName"];
        }

        /// <summary>
        /// 添加新转换模式回调函数
        /// </summary>
        /// <param name="name">转码模式名称</param>
        /// <param name="isProcessASCII">是否处理ASCII</param>
        /// <param name="isUppercase">是否使用大写字母</param>
        /// <param name="isXML">是否处理XML转码</param>
        /// <param name="codeRuleType">编码规则</param>
        /// <param name="convertScale">转码进制</param>
        /// <param name="additionType">附加文本</param>
        /// <param name="prefix">前缀</param>
        /// <param name="suffix">后缀</param>
        /// <param name="regular">正则表达式</param>
        private void AddNewConvertMode(string name, bool isProcessASCII, bool isUppercase, bool isXML, SC2_StructConvertMode.EnumCodeRuleType codeRuleType, SC2_StructConvertMode.EnumConvertScale convertScale, SC2_StructConvertMode.EnumAdditionType additionType, string prefix, string suffix, string regular)
        {
            SC2_StructConvertMode mode = new SC2_StructConvertMode(name, "ui-editoricon-general_triggers.dds", false, isProcessASCII, isUppercase, isXML, codeRuleType, convertScale, additionType, prefix, suffix, regular);
            SC2_StructConvertMode.StructConvertModeList.Add(name, mode);
            ToggleButton button = mode.GetToggleButton();
            InRibbonGallery_ConventMode.Items.Insert(4, button);
        }

        /// <summary>
        /// 清理转换模式
        /// </summary>
        private void CleanSelectConvertModeButton()
        {
            if (IsKeepConvertMode || SelectConvertModeButton == SC2_StructConvertMode.StructConvertModeDefaultList["UIText_ToggleButton_CustomConfig_Header"].ModeButton) return;
            IsAutoChangeToCustom = true;
            SC2_StructConvertMode.StructConvertModeDefaultList["UIText_ToggleButton_CustomConfig_Header"].ModeButton.IsChecked = true;
        }

        /// <summary>
        /// 生成正则表达式格式的字符串
        /// </summary>
        /// <param name="input">输入字符串</param>
        /// <returns>格式化结果</returns>
        private string GenRegexFormateString(string input)
        {
            return "(?-x:" + RegexRegularEscapeCharacter.Replace(input, RegexFormatMatchEvaluatorFunc) + ")";
        }

        /// <summary>
        /// 设置输入语法高亮
        /// </summary>
        public void SetInputHightlight()
        {
#if !DEBUG
            try
            {
#endif
            if (IsKeepInputHighlight) return;
            bool isPrefixNull = string.IsNullOrEmpty(TextBox_Prefix.Text);
            bool isSuffixNull = string.IsNullOrEmpty(TextBox_Suffix.Text);
            bool isRegularNull = string.IsNullOrEmpty(TextBox_RegularExpression.Text);
            XNamespace xmlns = "http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008";
            XElement ruleSet = new XElement(xmlns + "RuleSet");
            XElement syntaxDefinition = new XElement(xmlns + "SyntaxDefinition",
                new XAttribute("name", "InputHighlight"),
                ruleSet
                );
            XDocument xshd = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                syntaxDefinition);
            switch (EnumProcessingModeConfiguration)
            {
                case EnumProcessingMode.Encode:
                    XElement rule = new XElement(xmlns + "Rule");
                    SC2_HighlightConfig.ConfigList[SC2_HighlightConfig.EnumConfigTargetType.RegexMath].SetConfig(rule);
                    if (isRegularNull)
                    {
                        rule.Add(@"[\s\S]+");
                    }
                    else
                    {
                        if (IsRegularExpressionValid) rule.Add(TextBox_RegularExpression.Text);
                    }
                    ruleSet.Add(rule);
                    break;
                case EnumProcessingMode.Decode:
                    //SetTranscodeCodeHighlight(xmlns, ruleSet, isPrefixNull, isSuffixNull);
                    SetAdditionalTextHighlight(xmlns, ruleSet);
                    break;
                default:
                    break;

            }
            TextEditor_Input.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(xshd.CreateReader(), HighlightingManager.Instance);
#if !DEBUG
            }
            catch (Exception e)
            {
                //当前堆栈信息
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                string errMsg = e.Message + "\r\n\r\n" + App.CurrentLanguage["ErrorText_ALL_Exception_CallStack_Header"];
                foreach (StackFrame select in st.GetFrames())
                {
                    if (System.Diagnostics.StackFrame.OFFSET_UNKNOWN == select.GetILOffset()) break;
                    errMsg += "\r\n" + select.GetMethod().Name;
                }
                MessageBox.Show(errMsg, App.CurrentLanguage["ErrorText_ALL_Exception_MessageBox_Caption"] as string, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
#endif
        }


        /// <summary>
        /// 设置输出语法高亮
        /// </summary>
        public void SetOutputHighlight()
        {
#if !DEBUG
            try
            {
#endif
            if (IsKeepOutputHighlight) return;
            bool isPrefixNull = string.IsNullOrEmpty(TextBox_Prefix.Text);
            bool isSuffixNull = string.IsNullOrEmpty(TextBox_Suffix.Text);
            bool isRegularNull = string.IsNullOrEmpty(TextBox_RegularExpression.Text);
            XNamespace xmlns = "http://icsharpcode.net/sharpdevelop/syntaxdefinition/2008";
            XElement ruleSet = new XElement(xmlns + "RuleSet");
            XElement syntaxDefinition = new XElement(xmlns + "SyntaxDefinition",
                new XAttribute("name", "OutputHighlight"),
                ruleSet
                );
            XDocument xshd = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                syntaxDefinition);
            switch (EnumProcessingModeConfiguration)
            {
                case EnumProcessingMode.Encode:
                    //SetTranscodeCodeHighlight(xmlns, ruleSet, isPrefixNull, isSuffixNull);
                    SetAdditionalTextHighlight(xmlns, ruleSet);
                    break;
                case EnumProcessingMode.Decode:
                    XElement rule = new XElement(xmlns + "Rule");
                    SC2_HighlightConfig.ConfigList[SC2_HighlightConfig.EnumConfigTargetType.RegexMath].SetConfig(rule);
                    if (isRegularNull)
                    {
                        rule.Add(@"[\s\S]+");
                    }
                    else
                    {
                        if (IsRegularExpressionValid) rule.Add(TextBox_RegularExpression.Text);
                    }
                    ruleSet.Add(rule);
                    break;
                default:
                    break;
            }
            TextEditor_Output.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(xshd.CreateReader(), HighlightingManager.Instance);
#if !DEBUG
            }
            catch (Exception e)
            {
                //当前堆栈信息
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                string errMsg = e.Message + "\r\n\r\n" + App.CurrentLanguage["ErrorText_ALL_Exception_CallStack_Header"];
                foreach (StackFrame select in st.GetFrames())
                {
                    if (System.Diagnostics.StackFrame.OFFSET_UNKNOWN == select.GetILOffset()) break;
                    errMsg += "\r\n" + select.GetMethod().Name;
                }
                MessageBox.Show(errMsg, App.CurrentLanguage["ErrorText_ALL_Exception_MessageBox_Caption"] as string, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
#endif
        }

        /// <summary>
        /// 转码字符高亮设置
        /// </summary>
        /// <param name="xmlns">XML命名空间</param>
        /// <param name="ruleSet">规则集</param>
        /// <param name="isPrefixNull">前缀为空</param>
        /// <param name="isSuffixNull">后缀为空</param>
        private void SetTranscodeCodeHighlight(XNamespace xmlns, XElement ruleSet, bool isPrefixNull, bool isSuffixNull)
        {
            XElement rule;
            string regular;
            string codeRegexString = SC2_StructConvertMode.ConvertScaleRegularString[EnumCodeRuleTypeConfiguration][EnumConvertScaleConfiguration];
            if (!isPrefixNull)
            {
                regular = PreparedPrefixRegexString + "(?=" + codeRegexString + PreparedSuffixRegexString + ")";
                rule = new XElement(xmlns + "Rule");
                SC2_HighlightConfig.ConfigList[SC2_HighlightConfig.EnumConfigTargetType.DecodePrefix].SetConfig(rule);
                rule.Add(regular);
                ruleSet.Add(rule);
            }
            if (!isSuffixNull)
            {
                regular = "(?<=" + PreparedPrefixRegexString + codeRegexString + ")" + PreparedSuffixRegexString;
                rule = new XElement(xmlns + "Rule");
                SC2_HighlightConfig.ConfigList[SC2_HighlightConfig.EnumConfigTargetType.DecodeSuffix].SetConfig(rule);
                rule.Add(regular);
                ruleSet.Add(rule);
            }
            regular = (isPrefixNull ? "" : "(?<=" + PreparedPrefixRegexString + ")") + codeRegexString + (isSuffixNull ? "" : "(?=" + PreparedSuffixRegexString + ")");
            rule = new XElement(xmlns + "Rule");
            SC2_HighlightConfig.ConfigList[SC2_HighlightConfig.EnumConfigTargetType.DecodeCode].SetConfig(rule);
            rule.Add(regular);
            ruleSet.Add(rule);
        }

        /// <summary>
        /// 正则匹配字符高亮设置
        /// </summary>
        /// <param name="xmlns">XML命名空间</param>
        /// <param name="ruleSet">规则集</param>
        private void SetAdditionalTextHighlight(XNamespace xmlns, XElement ruleSet)
        {
            XElement rule = new XElement(xmlns + "Rule");
            SC2_HighlightConfig.ConfigList[SC2_HighlightConfig.EnumConfigTargetType.RegexMath].SetConfig(rule);
            rule.Add(GetAdditionalHighlightRegexString(EnumAdditionTypeConfiguration, TextBox_RegularExpression.Text));
            ruleSet.Add(rule);
        }

        /// <summary>
        /// 获取编码字符匹配正则字符串
        /// </summary>
        /// <param name="additionType">附加文本类型</param>
        /// <param name="baseString">基本字符串</param>
        /// <returns>正则表达式</returns>
        private static string GetAdditionalHighlightRegexString(SC2_StructConvertMode.EnumAdditionType additionType, string baseString)
        {
            string use;
            if (string.IsNullOrEmpty(baseString))
            {
                use =  @"[\s\S]+";
            }
            else
            {
                use = baseString;
            }
            switch (additionType)
            {
                case SC2_StructConvertMode.EnumAdditionType.DebugMsg:
                    return @"(?-x)(?<=TriggerDebugOutput\s*\(\s*\d+,\s*StringToText\s*\(\s*""[^""\\\r\n]*?)" + use + @"(?:[^""\\\r\n]*(?:\\.[^""\\\r\n]*)*?""\s*\)\s*,)(?=[^""\\\r\n]*(?:\\.[^""\\\r\n]*)*?""\s*\)\s*,)";
                case SC2_StructConvertMode.EnumAdditionType.AsText:
                    return @"(?-x)(?<=StringToText\s*\(\s*""[^""\\\r\n]*?)" + use + @"(?:[^""\\\r\n]*(?:\\.[^""\\\r\n]*)*?""\s*\))(?=[^""\\\r\n]*(?:\\.[^""\\\r\n]*)*?""\s*\))";
                case SC2_StructConvertMode.EnumAdditionType.AsString:
                    return @"(?-x)(?<=""[^""\\\r\n]*?)" + use + @"(?:[^""\\\r\n]*(?:\\.[^""\\\r\n]*)*?"")(?=[^""\\\r\n]*(?:\\.[^""\\\r\n]*)*?"")";
                case SC2_StructConvertMode.EnumAdditionType.OrigionalContent:
                    return use;
                default:
                    throw new Exception("Error enum value additionType!");
            }
        }

        /// <summary>
        /// 刷新UI
        /// </summary>
        public void RefreshUI()
        {
            if (IsKeepUI) return;
            CleanSelectConvertModeButton();
#if !DEBUG
            try
            {
#endif
            SC2_StructConvertMode.StructConvertModeDefaultList["UIText_ToggleButton_CustomConfig_Header"].ReadFromUI();
            TextEditor_Output.Text = FuncCodingInputText[EnumProcessingModeConfiguration](TextEditor_Input.Text);
#if !DEBUG
            }
            catch (Exception e)
            {
                //当前堆栈信息
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                string errMsg = e.Message + "\r\n\r\n" + App.CurrentLanguage["ErrorText_ALL_Exception_CallStack_Header"];
                foreach (StackFrame select in st.GetFrames())
                {
                    if (System.Diagnostics.StackFrame.OFFSET_UNKNOWN == select.GetILOffset()) break;
                    errMsg += "\r\n" + select.GetMethod().Name;
                }
                MessageBox.Show(errMsg, App.CurrentLanguage["ErrorText_ALL_Exception_MessageBox_Caption"] as string, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
#endif
        }
        #endregion

        #region 控件事件

        /// <summary>
        /// 窗口初始化事件
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void RibbonWindow_Initialized(object sender, EventArgs e)
        {
#if !DEBUG
            try
            {
#endif
            InRibbonGallery_ConventMode.GroupByAdvanced = SC2_StructConvertMode.AdvancedGroupFunction;
            foreach (var select in SC2_StructConvertMode.StructConvertModeDefaultList)
            {
                InRibbonGallery_ConventMode.Items.Add(select.Value.GetToggleButton());
            }
            JObject config = null;
            if (File.Exists(ConfigFileName))
            {
                StreamReader sr = new StreamReader(ConfigFileName);
                string jsonConfig = sr.ReadToEnd();
                sr.Close();

                try
                {
                    config = JObject.Parse(jsonConfig);
                }
                catch
                {
                    MessageBox.Show(App.CurrentLanguage["ErrorText_ReadConfig_Exception_MessageBox_Text"] as string, App.CurrentLanguage["ErrorText_ALL_Exception_MessageBox_Caption"] as string, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                }
                if (config != null)
                {
                    if (config["CustomModeList"] is JArray modes)
                    {
                        foreach (var slecet in modes)
                        {
                            if (slecet is JObject mode)
                            {
                                SC2_StructConvertMode modeClass = JsonConvert.DeserializeObject<SC2_StructConvertMode>(mode.ToString());
                                if (modeClass != null)
                                {
                                    SC2_StructConvertMode.StructConvertModeList.Add(modeClass.ConvertModeName, modeClass);
                                }
                            }
                        }
                        if (SC2_StructConvertMode.StructConvertModeList.Count != 0)
                        {
                            foreach (var select in SC2_StructConvertMode.StructConvertModeList)
                            {
                                InRibbonGallery_ConventMode.Items.Add(select.Value.GetToggleButton());
                            }
                        }
                    }
                    if (config["CustomMode"] != null)
                    {
                        SC2_StructConvertMode modeClass = JsonConvert.DeserializeObject<SC2_StructConvertMode>(config["CustomMode"].ToString());
                        SC2_StructConvertMode.StructConvertModeDefaultList["UIText_ToggleButton_CustomConfig_Header"].CopyProperty(modeClass);
                    }
                }
            }
            Fluent.MenuItem item = new Fluent.MenuItem
            {
                Width = 180
            };
            item.SetResourceReference(Fluent.MenuItem.IconProperty, "IMAGE_SaveMode");
            item.SetResourceReference(Fluent.MenuItem.HeaderProperty, "UIText_MenuItem_ConventModeSave_Header");
            item.Click += MenuITem_ConventModeSave_Click;
            InRibbonGallery_ConventMode.Items.Add(item);
            if (config != null && config["SelectedMode"] != null)
            {
                string selectModeName = config["SelectedMode"].ToString();
                if (SC2_StructConvertMode.StructConvertModeDefaultList.ContainsKey(selectModeName))
                {
                    SC2_StructConvertMode.StructConvertModeDefaultList[selectModeName].ModeButton.IsChecked = true;
                }
                else if (SC2_StructConvertMode.StructConvertModeList.ContainsKey(selectModeName))
                {
                    SC2_StructConvertMode.StructConvertModeList[selectModeName].ModeButton.IsChecked = true;
                }
                else
                {
                    SC2_StructConvertMode.StructConvertModeDefaultList["UIText_ToggleButton_SC2DefaultConfig_Header"].ModeButton.IsChecked = true;
                }
            }
            else
            {
                SC2_StructConvertMode.StructConvertModeDefaultList["UIText_ToggleButton_SC2DefaultConfig_Header"].ModeButton.IsChecked = true;
            }
            if (config != null && config["ProcessingMode"] != null && config["ProcessingMode"].ToString() == "Decode")
            {
                ToggleButton_ConvetModeDecode.IsChecked = true;
            }
            else
            {
                ToggleButton_ConvetModeEncode.IsChecked = true;
            }

#if !DEBUG
            }
            catch (Exception error)
            {
                //当前堆栈信息
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                string errMsg = error.Message + "\r\n\r\n" + App.CurrentLanguage["ErrorText_ALL_Exception_CallStack_Header"];
                foreach (StackFrame select in st.GetFrames())
                {
                    if (System.Diagnostics.StackFrame.OFFSET_UNKNOWN == select.GetILOffset()) break;
                    errMsg += "\r\n" + select.GetMethod().Name;
                }
                MessageBox.Show(errMsg, App.CurrentLanguage["ErrorText_ALL_Exception_MessageBox_Caption"] as string, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);

            }
#endif
        }

        /// <summary>
        /// 退出窗口保存
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void RibbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SC2_StructConvertMode.StructConvertModeDefaultList["UIText_ToggleButton_CustomConfig_Header"].ReadFromUI();
            JObject config =
                new JObject(
                    new JProperty("SelectedMode", (SelectConvertModeButton.Tag as SC2_StructConvertMode).ConvertModeName),
                        new JProperty("ProcessingMode", Enum.GetName(typeof(EnumProcessingMode), EnumProcessingModeConfiguration)),
                        new JProperty("CustomMode", JObject.Parse(JsonConvert.SerializeObject(SC2_StructConvertMode.StructConvertModeDefaultList["UIText_ToggleButton_CustomConfig_Header"]))),
                        new JProperty("CustomModeList",
                            new JArray(
                                from mode in SC2_StructConvertMode.StructConvertModeList.Values
                                select JObject.Parse(JsonConvert.SerializeObject(mode)
                            )
                        )
                    )
                );
            StreamWriter sw = new StreamWriter(ConfigFileName);
            sw.Write(config.ToString());
            sw.Close();
        }

        /// <summary>
        /// 语言列表选择切换事件
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void MulitiLanguageItem_Checked(object sender, RoutedEventArgs e)
        {
            if (!(sender is Fluent.ToggleButton)) return;
            Fluent.ToggleButton button = sender as Fluent.ToggleButton;
            string select = button.Tag as string;
            ChangeLanguage(select);
        }


        /// <summary>
        /// 处理模式选择事件
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void ToggleButton_ProcessMode_Checked(object sender, RoutedEventArgs e)
        {
            if (!(sender is ToggleButton)) return;
            ToggleButton button = sender as ToggleButton;
            if (button == ToggleButton_ConvetModeEncode)
            {
                EnumProcessingModeConfiguration = EnumProcessingMode.Encode;
            }
            else if (button == ToggleButton_ConvetModeDecode)
            {
                EnumProcessingModeConfiguration = EnumProcessingMode.Decode;
            }
            else
            {
                throw new Exception("SC2_UnicodeConverter_Window.ToggleButton_ProcessMode_Checked():Error Button!");
            }
            IsKeepConvertMode = true;
            SetInputHightlight();
            SetOutputHighlight();
            RefreshUI();
            IsKeepConvertMode = false;
        }

        /// <summary>
        /// 转换模式保存项点击事件
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void MenuITem_ConventModeSave_Click(object sender, RoutedEventArgs e)
        {
            SC2_NewConvertModeWindow window = new SC2_NewConvertModeWindow(App.CurrentLanguage, SC2_StructConvertMode.StructConvertModeList.Keys.ToList(), SC2_StructConvertMode.GetNoDuplicateModeName(App.CurrentLanguage), CheckBox_ProcessASCII.IsChecked == true, CheckBox_UppercaseCharacters.IsChecked == true, CheckBox_XMLTranscode.IsChecked == true, (SC2_StructConvertMode.EnumCodeRuleType)ComboBox_CharacterCodeRuleType.SelectedIndex, (SC2_StructConvertMode.EnumConvertScale)ComboBox_ConvertingScaleType.SelectedIndex, (SC2_StructConvertMode.EnumAdditionType)ComboBox_AdditionTextType.SelectedIndex, TextBox_Prefix.Text, TextBox_Suffix.Text, TextBox_RegularExpression.Text, AddNewConvertMode);
            window.Show();
        }

        /// <summary>
        /// CheckBox_ProcessASCII勾选事件
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void CheckBox_ProcessASCII_Checked(object sender, RoutedEventArgs e)
        {
            RefreshUI();
        }

        /// <summary>
        /// CheckBox_ProcessASCII取消勾选事件
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void CheckBox_ProcessASCII_Unchecked(object sender, RoutedEventArgs e)
        {
            RefreshUI();
        }

        /// <summary>
        /// CheckBox_Uppercase勾选事件
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void CheckBox_UppercaseCharacters_Unchecked(object sender, RoutedEventArgs e)
        {
            RefreshUI();
        }

        /// <summary>
        /// CheckBox_Uppercase取消勾选事件
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void CheckBox_UppercaseCharacters_Checked(object sender, RoutedEventArgs e)
        {
            RefreshUI();
        }

        /// <summary>
        /// CheckBox_XMLTranscode勾选事件
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void CheckBox_XMLTranscode_Checked(object sender, RoutedEventArgs e)
        {
            RefreshUI();
        }

        /// <summary>
        /// CheckBox_XMLTranscode取消勾选事件
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void CheckBox_XMLTranscode_Unchecked(object sender, RoutedEventArgs e)
        {
            RefreshUI();
        }

        /// <summary>
        /// ComboBox_CharacterCodeRuleType选择变化事件
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void ComboBox_CharacterCodeRuleType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnumCodeRuleTypeConfiguration = (SC2_StructConvertMode.EnumCodeRuleType)(ComboBox_CharacterCodeRuleType.SelectedItem as ComboBoxItem).Tag;
            SetInputHightlight();
            SetOutputHighlight();
            RefreshUI();
        }

        /// <summary>
        /// ComboBox_ConvertingScaleType选择变化事件
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void ComboBox_ConvertingScaleType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnumConvertScaleConfiguration = (SC2_StructConvertMode.EnumConvertScale)(ComboBox_ConvertingScaleType.SelectedItem as ComboBoxItem).Tag;
            SetInputHightlight();
            SetOutputHighlight();
            RefreshUI();
        }

        /// <summary>
        /// ComboBox_AdditionTextType选择变化事件
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void ComboBox_AdditionTextType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            EnumAdditionTypeConfiguration = (SC2_StructConvertMode.EnumAdditionType)ComboBox_AdditionTextType.SelectedIndex;
            SetInputHightlight();
            SetOutputHighlight();
            RefreshUI();
        }

        /// <summary>
        /// TextBox_Prefix文本变化事件
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void TextBox_Prefix_TextChanged(object sender, TextChangedEventArgs e)
        {
            PreparedPrefixRegexString = GenRegexFormateString(TextBox_Prefix.Text);
            SetInputHightlight();
            SetOutputHighlight();
            RefreshUI();
        }

        /// <summary>
        /// TextBox_Suffix文本变化事件
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void TextBox_Suffix_TextChanged(object sender, TextChangedEventArgs e)
        {
            PreparedSuffixRegexString = GenRegexFormateString(TextBox_Suffix.Text);
            SetInputHightlight();
            SetOutputHighlight();
            RefreshUI();
        }

        /// <summary>
        /// TextBox_RegularExpression文本变化事件
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void TextBox_RegularExpression_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (!String.IsNullOrEmpty(TextBox_RegularExpression.Text))
                {
                    Regex regex = new Regex(TextBox_RegularExpression.Text);
                }
                IsRegularExpressionValid = true;
            }
            catch
            {
                IsRegularExpressionValid = false;
            }
            SetInputHightlight();
            SetOutputHighlight();
            RefreshUI();
        }
        /// <summary>
        /// 输入文本变化
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void TextEditor_Input_TextChanged(object sender, EventArgs e)
        {
            TextEditor_Output.Select(0, 0);
            IsKeepConvertMode = true;
            RefreshUI();
            IsKeepConvertMode = false;
        }

        #endregion

    }
}
