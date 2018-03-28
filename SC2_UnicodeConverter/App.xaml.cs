using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SC2_UnicodeConverter
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        #region 属性

        /// <summary>
        /// 当前语言
        /// </summary>
        public static ResourceDictionary CurrentLanguage { get; set; }

        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数
        /// </summary>
        public App()
        {
            InitializeComponent();

#if !DEBUG
            //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DispatcherUnhandledException += new System.Windows.Threading.DispatcherUnhandledExceptionEventHandler(Application_DispatcherUnhandledException);
#endif
        }

        #endregion

        #region 方法

        /// <summary>
        /// 全局UI异常捕捉
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true;
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                string errMsg = e.Exception.Message + "\r\n\r\n" + CurrentLanguage["ErrorText_ALL_Exception_CallStack_Header"];
                foreach (StackFrame select in st.GetFrames())
                {
                    if (System.Diagnostics.StackFrame.OFFSET_UNKNOWN == select.GetILOffset()) break;
                    errMsg += "\r\n" + select.GetMethod().Name;
                }
                MessageBox.Show(errMsg, CurrentLanguage["ErrorText_ALL_Exception_MessageBox_Caption"] as string, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
            catch
            {

            }
        }

        /// <summary>
        /// 全局非UI异常捕捉
        /// </summary>
        /// <param name="sender">响应对象</param>
        /// <param name="e">响应参数</param>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception error = e.ExceptionObject as Exception;
                System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace();
                string errMsg = error.Message + "\r\n\r\n" + CurrentLanguage["ErrorText_ALL_Exception_CallStack_Header"];
                foreach (StackFrame select in st.GetFrames())
                {
                    if (System.Diagnostics.StackFrame.OFFSET_UNKNOWN == select.GetILOffset()) break;
                    errMsg += "\r\n" + select.GetMethod().Name;
                }
                MessageBox.Show(errMsg, CurrentLanguage["ErrorText_ALL_Exception_MessageBox_Caption"] as string, MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
            catch
            {

            }
        }

        #endregion
    }
}
