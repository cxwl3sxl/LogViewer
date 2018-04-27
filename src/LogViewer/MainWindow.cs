//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Windows.Controls;
//using System.Windows.Documents;
//using System.Windows.Media;

//namespace LogViewer
//{
//    partial class MainWindow
//    {
//        /// <summary>
//        /// 改变在文章中用户搜索的关键字的字体颜色
//        /// </summary>
//        /// <param name="keyword"></param>
//        public void ChangeSeachKeyWordsColor(string keyword)
//        {
//            ChangeColorWithResout(keyword);
//        }

//        /// <summary>
//        /// 改变关键字的字体颜色
//        /// </summary>
//        /// <param name="keyword">用户搜索关键字</param>
//        /// <returns></returns>
//        private List<TextRange> ChangeColorWithResout(string keyword)
//        {
//            if (!string.IsNullOrEmpty(m_ProSearchkey))
//            {
//                ChangeColor(CommonColorsHelp.DefaultFontColor, m_ProSearchkey);
//                ReSetBackGroundAll();
//            }
//            m_ProSearchkey = keyword;

//            return ChangeColor(CommonColorsHelp.KeyFontColor, keyword);
//        }



//        /// <summary>
//        /// 设置背景色
//        /// </summary>
//        /// <param name="l"></param>
//        /// <param name="textRange"></param>
//        public void SetBackGround(Color l, TextRange textRange)
//        {
//            //textRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(l));
//            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.GreenYellow));
//        }
//        /// <summary>
//        /// 重新设置背景色
//        /// </summary>
//        /// <param name="textRange">关键字的的TextRange</param>
//        /// <param name="isCurrKeyWord">是否是当前的关键字</param>
//        public void ReSetBackGround(TextRange textRange, bool isCurrKeyWord)
//        {
//            textRange.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.White));
//            if (isCurrKeyWord)
//            {
//                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(CommonColorsHelp.KeyFontColor));
//            }
//            else
//            {
//                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(CommonColorsHelp.DefaultFontColor));
//            }

//        }

//        /// <summary>
//        /// 上一处
//        /// </summary>
//        public void SetUpBackGround()
//        {
//            if (m_TextList != null && m_TextList.Count > 0)
//            {
//                ReSetBackGround(m_TextList[currNumber], true);
//                currNumber--;
//                if (currNumber < 0)
//                {
//                    currNumber = m_TextList.Count - 1;
//                }
//                SetBackGround(CommonColorsHelp.SelectedKeyBackGroundColor, m_TextList[currNumber]);
//            }

//        }
//        /// <summary>
//        /// 下一处
//        /// </summary>
//        public void SetDownBackGround()
//        {
//            if (m_TextList != null && m_TextList.Count > 0)
//            {
//                ReSetBackGround(m_TextList[currNumber], true);
//                currNumber++;
//                if (currNumber >= m_TextList.Count)
//                {
//                    currNumber = 0;
//                }
//                RichTextBoxLogs.Document.MoveFocus( Tr)
//                SetBackGround(CommonColorsHelp.SelectedKeyBackGroundColor, m_TextList[currNumber]);
//            }
//        }
//        /// <summary>
//        /// 改变关键字的具体实现
//        /// </summary>
//        /// <param name="l"></param>
//        /// <param name="richTextBox1"></param>
//        /// <param name="selectLength"></param>
//        /// <param name="tpStart"></param>
//        /// <param name="tpEnd"></param>
//        /// <returns></returns>
//        private TextPointer selecta(Color l, RichTextBox richTextBox1, int selectLength, TextPointer tpStart, TextPointer tpEnd)
//        {
//            TextRange range = richTextBox1.Selection;
//            range.Select(tpStart, tpEnd);
//            //高亮选择
//            range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(l));
//            return tpEnd.GetNextContextPosition(LogicalDirection.Forward);
//        }
//        /// <summary>
//        /// 把所有背景恢复到默认
//        /// </summary>
//        private void ReSetBackGroundAll()
//        {
//            if (m_TextList != null)
//            {
//                foreach (TextRange textRange in m_TextList)
//                {
//                    ReSetBackGround(textRange, false);
//                }
//            }
//        }
//        /// <summary>
//        /// 当前第几处关键字被选中
//        /// </summary>
//        private int currNumber = 0;
//        /// <summary>
//        /// 改变关键字字体颜色
//        /// </summary>
//        /// <param name="l">颜色</param>
//        /// <param name="keyword">关键字</param>
//        /// <returns></returns>
//        private List<TextRange> ChangeColor(Color l, string keyword)
//        {
//            m_TextList = new List<TextRange>();
//            //设置文字指针为Document初始位置           
//            //richBox.Document.FlowDirection           
//            TextPointer position = RichTextBoxLogs.Document.ContentStart;
//            while (position != null)
//            {
//                //向前搜索,需要内容为Text       
//                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
//                {
//                    //拿出Run的Text        
//                    string text = position.GetTextInRun(LogicalDirection.Forward);
//                    //可能包含多个keyword,做遍历查找           
//                    int index = 0;
//                    index = text.IndexOf(keyword, 0);
//                    if (index != -1)
//                    {
//                        TextPointer start = position.GetPositionAtOffset(index);
//                        TextPointer end = start.GetPositionAtOffset(keyword.Length);
//                        m_TextList.Add(new TextRange(start, end));
//                        position = selecta(l, RichTextBoxLogs, keyword.Length, start, end);
//                    }
//                }
//                //文字指针向前偏移   
//                position = position.GetNextContextPosition(LogicalDirection.Forward);
//            }

//            if (m_TextList != null && m_TextList.Count > 0)
//            {
//                //重置
//                currNumber = 0;
                
//                SetBackGround(CommonColorsHelp.SelectedKeyBackGroundColor, m_TextList[currNumber]);
//            }
            
//            return m_TextList;
//        }
//        /// <summary>
//        /// 当前关键字共搜索出的结果集合
//        /// </summary>
//        private List<TextRange> m_TextList;
//    }
//}
