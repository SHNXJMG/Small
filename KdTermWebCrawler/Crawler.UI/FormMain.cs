using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Collections;
using System.Net.Mail;
using System.Threading;
using System.Xml;
using Crawler.Instance;

namespace Crawler
{
    public partial class FormMain : Form
    {
        protected log4net.ILog _logger;
        /// <summary>
        /// 日志记录对象
        /// </summary> 
        public log4net.ILog Logger
        {
            get
            {
                if (_logger == null)
                    _logger = log4net.LogManager.GetLogger(this.GetType());
                return _logger;
            }
        }

        public System.Timers.Timer timer = new System.Timers.Timer();
        public Dictionary<string, List<WebSiteCrawller>> dictTimerCrawllers = new Dictionary<string, List<WebSiteCrawller>>();

        #region 构造及初始化
        /// <summary>
        /// 构造方法
        /// </summary>
        public FormMain()
        {
            InitializeComponent();

            InitTreeView();

            SetCurrCrawllerDetails();
        }

        public void InitTreeView()
        {
            AppendText("初始化信息抓取选项数据......");

            ToolComm.InitCrawller(treCrawlers);

            BindSertchList();

            AppendText("初始化完成。");
        }
        #endregion

        #region 按钮事件
        /// <summary>
        /// 启动定时抓取程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBegin_Click(object sender, EventArgs e)
        {
            dictTimerCrawllers.Clear();

            int count = 0;
            Dictionary<string, WebSiteCrawller> crawllers = ToolComm.GetEnabledCrawllers(treCrawlers);
            foreach (string key in crawllers.Keys)
            {
                count++;
                WebSiteCrawller crawller = crawllers[key];

                if (dictTimerCrawllers.ContainsKey(crawller.PlanTime))
                {
                    dictTimerCrawllers[crawller.PlanTime].Add(crawller);
                }
                else
                {
                    List<WebSiteCrawller> list = new List<WebSiteCrawller>();
                    list.Add(crawller);
                    dictTimerCrawllers.Add(crawller.PlanTime, list);
                }
            }

            if (count == 0)
            {
                MessageBox.Show("请选择要抓取的选项！");
                return;
            }

            ToolComm.SaveConfigs(treCrawlers);

            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Interval = 60 * 1000; //1分钟执行一次
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Tick);
            AppendText("开始抓取数据......");
            BtnBegin.Enabled = false;
            grpCurr.Enabled = false;
        }

        /// <summary>
        /// 清除日志
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClear_Click(object sender, EventArgs e)
        {
            this.txtLog.Clear();
        }

        /// <summary>
        /// 关闭窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        #region 其他事件
        /// <summary>
        /// 定时执行方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer_Tick(object sender, EventArgs e)
        {
            if (dictTimerCrawllers != null && dictTimerCrawllers.Count > 0)
            {
                foreach (KeyValuePair<string, List<WebSiteCrawller>> kvp in dictTimerCrawllers)
                {
                    string[] dataArray = kvp.Key.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    if (dataArray != null && dataArray.Length > 0)
                    {
                        foreach (string dateOne in dataArray)
                        {
                            int year = -1, month = -1, day = -1, hour = -1, min = -1;
                            try
                            {
                                string tempHour = string.Empty;
                                if (dateOne.Trim().IndexOf(" ") != -1)  //有日期
                                {
                                    string[] tempDayArray = dateOne.Trim().Substring(0, dateOne.Trim().IndexOf(" ")).Trim().Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                                    switch (tempDayArray.Length)
                                    {
                                        case 3:
                                            year = int.Parse(tempDayArray[0]);
                                            month = int.Parse(tempDayArray[1]);
                                            day = int.Parse(tempDayArray[2]);
                                            break;
                                        case 2:
                                            month = int.Parse(tempDayArray[0]);
                                            day = int.Parse(tempDayArray[1]);
                                            break;
                                        case 1:
                                            day = int.Parse(tempDayArray[0]);
                                            break;
                                    }
                                    tempHour = dateOne.Trim().Substring(dateOne.Trim().IndexOf(" ") + 1).Trim();
                                }
                                else
                                {
                                    tempHour = dateOne.Trim();
                                }
                                if (!string.IsNullOrEmpty(tempHour))
                                {
                                    hour = int.Parse(tempHour.Substring(0, tempHour.IndexOf(":")));
                                    min = int.Parse(tempHour.Substring(tempHour.IndexOf(":") + 1));
                                }
                            }
                            catch (Exception) { }
                            DateTime dtTemp = DateTime.Now;
                            if ((year == -1 || dtTemp.Year == year)
                                && (month == -1 || dtTemp.Month == month)
                                && (day == -1 || dtTemp.Day == day)
                                && (hour == -1 || dtTemp.Hour == hour)
                                && (min == -1 || dtTemp.Minute == min))
                            {
                                foreach (WebSiteCrawller crawller in kvp.Value)
                                {
                                    AppendText("开始抓取【" + crawller.Title + "】数据......");
                                    try
                                    {
                                        AsyncMethodCaller caller = new AsyncMethodCaller(ToolComm.DealEntity);
                                        IAsyncResult callResult = caller.BeginInvoke(crawller, new AsyncCallback(CallbackMethod), caller);
                                    }
                                    catch (Exception ex)
                                    {

                                        Logger.Error(ex.ToString());
                                        AppendText("进行【" + crawller.Title + "】数据抓取时出现异常，详见日志文件！");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void treCrawlers_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node != null)
            {
                WebSiteCrawller crawller = e.Node.Tag as WebSiteCrawller;
                if (crawller != null)
                {
                    crawller.Enabled = e.Node.Checked;
                }

                foreach (TreeNode child in e.Node.Nodes)
                {
                    child.Checked = e.Node.Checked;
                    WebSiteCrawller childCrawller = child.Tag as WebSiteCrawller;
                    if (childCrawller != null)
                    {
                        childCrawller.Enabled = child.Checked;
                    }
                }

            }
        }
        TreeNode currNode = null;
        WebSiteCrawller currCrawller = null;

        /// <summary>
        /// 给窗体控件赋值
        /// </summary>
        private void SetCurrCrawllerDetails()
        {
            this.btnCurrCrawl.Enabled = this.btnCurrSave.Enabled = currCrawller != null;
            if (grpCurr.Enabled == false)
                this.btnCurrSave.Enabled = false;

            if (currCrawller != null)
            {
                //txtCrawller.Text = ToolComm.GetCrawlerInfo(currCrawller);

                lblCurrKey.Text = "应用程序类：" + currCrawller.Key;
                txtCurrTitle.Text = currCrawller.Title;
                txtCurrDescription.Text = currCrawller.Description;
                txtCurrSiteUrl.Text = currCrawller.SiteUrl;
                txtCurrGroup.Text = currCrawller.Group;
                txtCurrPlanTime.Text = currCrawller.PlanTime;
                txtCurrMaxCount.Text = currCrawller.MaxCount.ToString();
                txtCurrExistCompareFields.Text = currCrawller.ExistCompareFields;
                txtMaxEndTime.Text = currCrawller.MaxEndTime.ToString();
                lblCurrLastCrawlStart.Text = currCrawller.LastCrawlStart.ToString("yyyy-MM-dd HH:mm:ss");
                lblCurrLastCrawlEnd.Text = currCrawller.LastCrawlEnd.ToString("yyyy-MM-dd HH:mm:ss");
                chkCurrCrawlAll.Checked = currCrawller.IsCrawlAll;
                chkCurrEnabled.Checked = currCrawller.Enabled;
                chkCurrExistsUpdate.Checked = currCrawller.ExistsUpdate;
                chkUpdateCtx.Checked = currCrawller.ExistsHtlCtx;
                chkUpdateAttach.Checked = currCrawller.ExistsUpdateAttach;
            }
            else
            {
                lblCurrKey.Text =
                txtCurrTitle.Text =
                txtCurrDescription.Text =
                txtCurrSiteUrl.Text =
                txtCurrGroup.Text =
                txtCurrPlanTime.Text =
                txtCurrMaxCount.Text =
                txtCurrExistCompareFields.Text =
                lblCurrLastCrawlStart.Text =
                lblCurrLastCrawlEnd.Text =
                txtCurrMaxCount.Text = string.Empty;

                chkUpdateAttach.Checked =
                chkCurrCrawlAll.Checked =
                chkCurrEnabled.Checked =
                chkUpdateCtx.Checked =
                chkCurrExistsUpdate.Checked = false;
            }
        }

        private void btnCurrSave_Click(object sender, EventArgs e)
        {
            if (currCrawller != null)
            {
                currCrawller.Title = txtCurrTitle.Text;
                currCrawller.Description = txtCurrDescription.Text;
                currCrawller.SiteUrl = txtCurrSiteUrl.Text;
                currCrawller.Group = txtCurrGroup.Text;
                currCrawller.PlanTime = txtCurrPlanTime.Text;
                currCrawller.MaxCount = int.Parse(txtCurrMaxCount.Text);
                currCrawller.ExistCompareFields = txtCurrExistCompareFields.Text;
                if (!string.IsNullOrWhiteSpace(txtMaxEndTime.Text))
                    currCrawller.MaxEndTime = int.Parse(txtMaxEndTime.Text);
                currCrawller.IsCrawlAll = chkCurrCrawlAll.Checked;
                currCrawller.Enabled = chkCurrEnabled.Checked;
                currCrawller.ExistsUpdate = chkCurrExistsUpdate.Checked;
                currCrawller.ExistsHtlCtx = chkUpdateCtx.Checked;
                currCrawller.ExistsUpdateAttach = chkUpdateAttach.Checked;
                ToolComm.UpdateNodeConfigs(currNode, false);

                ToolComm.SaveConfigs(treCrawlers);
            }
        }

        private void treCrawlers_AfterSelect(object sender, TreeViewEventArgs e)
        {
            currNode = e.Node;
            currCrawller = currNode.Tag as WebSiteCrawller;
            SetCurrCrawllerDetails();
        }
        #endregion

        #region 窗体事件
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult result = MessageBox.Show("数据正在抓取中，您确定要退出吗", "退出确认", MessageBoxButtons.OKCancel);
            e.Cancel = (result == DialogResult.Cancel);
        }
        #endregion

        #region 公共方法
        /// <summary>
        /// 写日志到文件和文本框中
        /// </summary>
        /// <param name="text"></param>
        private void AppendText(string text)
        {
            if (!this.InvokeRequired)
            {
                Logger.Error(text);
                txtLog.AppendText(DateTime.Now.ToString("MM-dd HH:mm:ss") + "：" + text + "\r\n");
            }
            else
                this.Invoke(new Action<string>(AppendText), text);
        }

        /// <summary>
        /// 异步调用方法
        /// </summary>
        /// <param name="ar"></param>
        private void CallbackMethod(IAsyncResult ar)
        {
            try
            {
                AsyncMethodCaller caller = (AsyncMethodCaller)ar.AsyncState;
                string result = (string)caller.EndInvoke(ar);
                AppendText(result);
                SetCurrCrawllerDetails();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }
        #endregion

        #region 抓取单个程序
        /// <summary>
        /// 抓取单个程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCurrCrawl_Click(object sender, EventArgs e)
        {
            if (currCrawller != null)
            {
                AppendText("开始（单独）抓取【" + currCrawller.Title + "】数据......");

                try
                {
                    AsyncMethodCaller caller = new AsyncMethodCaller(ToolComm.DealEntity);
                    //ToolComm.DealEntity(currCrawller);
                    IAsyncResult callResult = caller.BeginInvoke(currCrawller, new AsyncCallback(CallbackMethod), caller);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    AppendText("进行【" + currCrawller.Title + "】数据抓取时出现异常，详见日志文件！（单个数据）");
                }
                SetCurrCrawllerDetails();
            }
        }

        #endregion


        private void CbxLogExist_CheckedChanged(object sender, EventArgs e)
        {
            ToolCoreDb.LogExistCompareSQL = CbxLogExist.Checked;
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            ToolComm.SaveConfigs(treCrawlers);
        }

        private void btnTreeMin_Click(object sender, EventArgs e)
        {
            treCrawlers.CollapseAll();
        }

        private void btnTreeMax_Click(object sender, EventArgs e)
        {
            treCrawlers.ExpandAll();
        }

        #region 树节点搜索
        private void btnSearch_Click(object sender, EventArgs e)
        {
            ClearSelect();
            if (txtSertch.Text.Trim().Length < 1)
                return;
            string selectText = comBoxSertch.SelectedItem.ToString();
            if (selectText.Equals("全部"))
            {
                foreach (TreeNode tn in treCrawlers.Nodes)
                {
                    if (tn.Text.Contains(txtSertch.Text.Trim()))
                    {
                        tn.ForeColor = Color.Red;
                    }
                    if (tn.Nodes.Count > 1)
                    {
                        foreach (TreeNode node in tn.Nodes)
                        {
                            if (node.Text.Contains(txtSertch.Text.Trim()))
                            {
                                tn.Expand();
                                node.ForeColor = Color.Red;
                            }
                        }
                    }
                }

            }
            else
            {
                TreeNode tn = treCrawlers.Nodes[comBoxSertch.SelectedItem.ToString()];
                if (tn.Nodes.Count > 1)
                {
                    tn.Expand();
                    foreach (TreeNode node in tn.Nodes)
                    {
                        if (node.Text.Contains(txtSertch.Text.Trim()))
                        {
                            node.ForeColor = Color.Red;
                        }
                    }
                }
            }
        }

        private void BindSertchList()
        {
            comBoxSertch.Items.Add("全部");
            foreach (TreeNode tn in treCrawlers.Nodes)
            {
                comBoxSertch.Items.Add(tn.Text);
            }
            comBoxSertch.SelectedIndex = 0;
        }

        private void comBoxSertch_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            string selectText = comBoxSertch.SelectedItem.ToString();
            if (string.IsNullOrEmpty(selectText))
                return;

            treCrawlers.CollapseAll();
            if (!selectText.Equals("全部"))
            {
                TreeNode tn = treCrawlers.Nodes[selectText];
                tn.Expand();
            }
        }

        private void ClearSelect()
        {
            foreach (TreeNode tn in treCrawlers.Nodes)
            {
                if (tn.Text.Contains(txtSertch.Text.Trim()))
                {
                    tn.ForeColor = Color.Blue;
                }
                if (tn.Nodes.Count > 1)
                {
                    foreach (TreeNode node in tn.Nodes)
                    {
                        WebSiteCrawller crawller = node.Tag as WebSiteCrawller;
                        if (crawller != null)
                        {
                            node.ForeColor = crawller.Enabled ? Color.Blue : Color.Gray;
                        }

                    }
                }
            }
        }

        private void btnClearSertch_Click(object sender, EventArgs e)
        {
            ClearSelect();
        }

        #endregion


    }
}