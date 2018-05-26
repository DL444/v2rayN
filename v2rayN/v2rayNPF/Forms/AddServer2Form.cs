﻿using System;
using System.Windows.Forms;
using v2rayNPF.Handler;
using v2rayNPF.Mode;

namespace v2rayNPF.Forms
{
    public partial class AddServer2Form : BaseForm
    {
        public int EditIndex { get; set; }
        VmessItem vmessItem;

        public AddServer2Form()
        {
            InitializeComponent();
        }

        private void AddServer2Form_Load(object sender, EventArgs e)
        {
            if (EditIndex >= 0)
            {
                BindingServer();
            }
            else
            {
                ClearServer();
            }
        }

        /// <summary>
        /// 绑定数据
        /// </summary>
        private void BindingServer()
        {
            vmessItem = config.vmess[EditIndex];
            txtRemarks.Text = vmessItem.remarks;
            txtAddress.Text = vmessItem.address;
            txtAddress.ReadOnly = true;
        }


        /// <summary>
        /// 清除设置
        /// </summary>
        private void ClearServer()
        {
            txtRemarks.Text = "";
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string remarks = txtRemarks.Text;
            if (Utils.IsNullOrEmpty(remarks))
            {
                UI.Show("请填写备注");
                return;
            }
            vmessItem.remarks = remarks;

            if (ConfigHandler.EditCustomServer(ref config, vmessItem, EditIndex) == 0)
            {
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                UI.Show("操作失败，请检查重试");
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
