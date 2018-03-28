using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SC2_UnicodeConverter
{
    /// <summary>
    /// 新模式保存窗口
    /// </summary>
    public partial class SC2_NewConvertModeWindow : Window
    {
        #region 声明
        /// <summary>
        /// 新建模式委托
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
        public delegate void DelegateStructConvertMode(string name, bool isProcessASCII, bool isUppercase, bool isXML, SC2_UnicodeConverter.SC2_StructConvertMode.EnumCodeRuleType codeRuleType, SC2_UnicodeConverter.SC2_StructConvertMode.EnumConvertScale convertScale, SC2_UnicodeConverter.SC2_StructConvertMode.EnumAdditionType additionType, string prefix, string suffix, string regular);
        #endregion

        #region 属性
        private ResourceDictionary LanguageDictionary { get; set; }
        /// <summary>
        /// 已有模式名称列表
        /// </summary>
        private List<string> ExistModeNameList { get; set; }

        /// <summary>
        /// 回调委托
        /// </summary>
        private DelegateStructConvertMode CallBack { get; set; }
        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public SC2_NewConvertModeWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="language">使用语言资源</param>
        /// <param name="existNames">已有名称</param>
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
        /// <param name="callback">结束回调</param>
        public SC2_NewConvertModeWindow(ResourceDictionary language, List<string> existNames, string name, bool isProcessASCII, bool isUppercase, bool isXML, SC2_StructConvertMode.EnumCodeRuleType codeRuleType, SC2_StructConvertMode.EnumConvertScale convertScale, SC2_StructConvertMode.EnumAdditionType additionType, string prefix, string suffix, string regular, DelegateStructConvertMode callback)
        {
            InitializeComponent();
            WindowLanguage.MergedDictionaries.Clear();
            WindowLanguage.MergedDictionaries.Add(language);
            LanguageDictionary = language;

            CallBack = callback;

            ExistModeNameList = existNames;
            TextBox_ModeName.Text = name;

            CheckBox_ProcessASCII.IsChecked = isProcessASCII;
            CheckBox_UppercaseCharacters.IsChecked = isUppercase;
            CheckBox_XMLTranscode.IsChecked = isXML;

            ComboBox_CharacterCodeRuleType.SelectedIndex = (int)codeRuleType;
            ComboBox_ConvertingScaleType.SelectedIndex = (int)convertScale;
            ComboBox_AdditionTextType.SelectedIndex = (int)additionType;

            TextBox_Prefix.Text = prefix;
            TextBox_Suffix.Text = suffix;
            TextBox_RegularExpression.Text = regular;
        }

        #endregion

        #region 控件事件

        /// <summary>
        /// 点击确认事件
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void Button_Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (ExistModeNameList.Contains(TextBox_ModeName.Text))
            {
                MessageBox.Show(LanguageDictionary["Message_SC2_NewConvertModeWindow_DuplicateName_Text0"] as string, LanguageDictionary["Message_SC2_NewConvertModeWindow_DuplicateName_Cation"] as string, MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
                return;
            }
            CallBack(TextBox_ModeName.Text, CheckBox_ProcessASCII.IsChecked == true, CheckBox_UppercaseCharacters.IsChecked == true, CheckBox_XMLTranscode.IsChecked == true, (SC2_StructConvertMode.EnumCodeRuleType)ComboBox_CharacterCodeRuleType.SelectedIndex, (SC2_StructConvertMode.EnumConvertScale)ComboBox_ConvertingScaleType.SelectedIndex, (SC2_StructConvertMode.EnumAdditionType)ComboBox_AdditionTextType.SelectedIndex, TextBox_Prefix.Text, TextBox_Suffix.Text, TextBox_RegularExpression.Text);
            Close();
        }

        /// <summary>
        /// 点击取消事件
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void Button_Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion
    }
}
