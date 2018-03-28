using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SC2_UnicodeConverter
{
    /// <summary>
    /// 高亮颜色配置
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class SC2_HighlightConfig
    {
        #region 声明

        /// <summary>
        /// 目标类型
        /// </summary>
        public enum EnumConfigTargetType
        {
            /// <summary>
            /// 输入编码
            /// </summary>
            InputEncode,
            /// <summary>
            /// 输入编码前缀
            /// </summary>
            InputDecodePrefix,
            /// <summary>
            /// 输入编码后缀
            /// </summary>
            InputDecodeSuffix,
            /// <summary>
            /// 输入编码
            /// </summary>
            InputDecodeCode,
        }

        /// <summary>
        /// 字体样式枚举
        /// </summary>
        public enum EnumFontStyle
        {
            italic,
            normal,
            oblique,
        }

        #endregion

        #region 属性
        /// <summary>
        /// 前景色
        /// </summary>
        [JsonProperty]
        public string ForeGround { get; set; }

        /// <summary>
        /// 背景色
        /// </summary>
        [JsonProperty]
        public string BackGround { get; set; }

        /// <summary>
        /// 字体粗细
        /// </summary>
        [JsonProperty]
        public string FontWeight { get; set; }

        /// <summary>
        /// 字体样式
        /// </summary>
        [JsonProperty, JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public EnumFontStyle FontStyle { get; set; }

        /// <summary>
        /// 下划线
        /// </summary>
        [JsonProperty]
        public bool UnderLine { get; set; }

        /// <summary>
        /// 目标类型
        /// </summary>
        [JsonProperty, JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        private EnumConfigTargetType TargetType { get; set; }

        public static Dictionary<EnumConfigTargetType, SC2_HighlightConfig> ConfigList { get; set; }

        #endregion

        #region 构造函数

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static SC2_HighlightConfig() => ConfigList = new Dictionary<EnumConfigTargetType, SC2_HighlightConfig>()
        {
            {
                EnumConfigTargetType.InputEncode,
                new SC2_HighlightConfig("Red", "", "bold", EnumFontStyle.normal, false)
            },
            {
                EnumConfigTargetType.InputDecodePrefix,
                new SC2_HighlightConfig("Yellow", "Green", "", EnumFontStyle.normal, false)
            },
            {
                EnumConfigTargetType.InputDecodeSuffix,
                new SC2_HighlightConfig("Yellow", "Blue", "", EnumFontStyle.normal, false)
            },
            {
                EnumConfigTargetType.InputDecodeCode,
                new SC2_HighlightConfig("Red", "Yellow", "bold", EnumFontStyle.normal, false)
            },
        };

        /// <summary>
        /// 构造函数
        /// </summary>
        public SC2_HighlightConfig()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="foreground">前景色</param>
        /// <param name="background">背景色</param>
        /// <param name="fontWeight">加粗</param>
        /// <param name="fontStyle">斜体</param>
        /// <param name="underline">下划线</param>
        public SC2_HighlightConfig(string foreground, string background, string fontWeight, EnumFontStyle fontStyle, bool underline)
        {
            ForeGround = foreground;
            BackGround = background;
            FontWeight = fontWeight;
            FontStyle = fontStyle;
            UnderLine = underline;
        }
        #endregion

        #region 方法
        /// <summary>
        /// 使用配置
        /// </summary>
        /// <param name="target">使用对象</param>
        public void SetConfig(XElement target)
        {
            if (!string.IsNullOrEmpty(ForeGround))
            {
                target.Add(new XAttribute("foreground", ForeGround));
            }
            if (!string.IsNullOrEmpty(BackGround))
            {
                target.Add(new XAttribute("background", BackGround));
            }
            if (!string.IsNullOrEmpty(FontWeight))
            {
                target.Add(new XAttribute("fontWeight", FontWeight));
            }
            if (FontStyle != EnumFontStyle.normal)
            {
                target.Add(new XAttribute("fontStyle", Enum.GetName(typeof(EnumFontStyle), FontStyle)));
            }
            if (UnderLine)
            {
                target.Add(new XAttribute("underline", true));
            }
        }
        #endregion
    }
}
