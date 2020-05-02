﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FanControl
{
    public partial class MainForm : Form
    {
        private List<Label> mSensorLabelList = new List<Label>();
        private List<TextBox> mSensorNameTextBoxList = new List<TextBox>();
        private List<Label> mFanLabelList = new List<Label>();
        private List<TextBox> mFanNameTextBoxList = new List<TextBox>();
        private List<TextBox> mControlTextBoxList = new List<TextBox>();
        private List<Label> mControlLabelList = new List<Label>();
        private List<TextBox> mControlNameTextBoxList = new List<TextBox>();        

        private FanControlForm mFanControlForm = null;
        private KrakenForm mKrakenForm = null;

        public MainForm()
        {
            InitializeComponent();
            this.localizeComponent();

            this.FormClosing += onClosing;

            mTrayIcon.Visible = true;
            mTrayIcon.MouseDoubleClick += onTrayIconDBClicked;
            mTrayIcon.ContextMenuStrip = mTrayMenuStrip;

            if (OptionManager.getInstance().read() == false)
            {
                OptionManager.getInstance().write();
            }

            if (OptionManager.getInstance().Interval < 100)
            {
                OptionManager.getInstance().Interval = 100;
            }
            else if (OptionManager.getInstance().Interval > 5000)
            {
                OptionManager.getInstance().Interval = 5000;
            }

            if (OptionManager.getInstance().IsMinimized == true)
            {
                this.Visible = false;
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            var hardwareManager = HardwareManager.getInstance();
            var controlManager = ControlManager.getInstance();

            hardwareManager.onUpdateCallback += onUpdate;
            hardwareManager.start();

            // name
            controlManager.setNameCount(0, hardwareManager.getSensorCount());
            controlManager.setNameCount(1, hardwareManager.getFanCount());
            controlManager.setNameCount(2, hardwareManager.getControlCount());

            for(int i = 0; i < hardwareManager.getSensorCount(); i++)
            {
                var temp = hardwareManager.getSensor(i);
                controlManager.setName(0, i, true, temp.Name);
                controlManager.setName(0, i, false, temp.Name);
            }

            for (int i = 0; i < hardwareManager.getFanCount(); i++)
            {
                var temp = hardwareManager.getFan(i);
                controlManager.setName(1, i, true, temp.Name);
                controlManager.setName(1, i, false, temp.Name);
            }

            for (int i = 0; i < hardwareManager.getControlCount(); i++)
            {
                var temp = hardwareManager.getControl(i);
                controlManager.setName(2, i, true, temp.Name);
                controlManager.setName(2, i, false, temp.Name);
            }

            controlManager.read();
            if (controlManager.checkData() == false)
            {
                MessageBox.Show(StringLib.Not_Match);
            }

            this.createComponent();
            this.ActiveControl = mFanControlButton;
            mKrakenButton.Visible = (hardwareManager.getKrakenX() != null);

            mEnableToolStripMenuItem.Checked = controlManager.IsEnable;
            mNormalToolStripMenuItem.Checked = (controlManager.ModeIndex == 0);
            mSilenceToolStripMenuItem.Checked = (controlManager.ModeIndex == 1);
            mPerformanceToolStripMenuItem.Checked = (controlManager.ModeIndex == 2);
            mGameToolStripMenuItem.Checked = (controlManager.ModeIndex == 3);

            // startUpdate
            hardwareManager.startUpdate();
        }

        private void localizeComponent()
        {
            System.Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            this.Text = StringLib.Title + " v" + version.Major + "." + version.Minor + "." + version.Build;
            mTrayIcon.Text = StringLib.Title;
            mTempGroupBox.Text = StringLib.Temperature;
            mFanGroupBox.Text = StringLib.Fan_speed;
            mControlGroupBox.Text = StringLib.Fan_control;
            mOptionButton.Text = StringLib.Option;
            mFanControlButton.Text = StringLib.Auto_Fan_Control;
            mKrakenButton.Text = StringLib.Kraken_Setting;
            mMadeLabel.Text = StringLib.Made;

            mEnableToolStripMenuItem.Text = StringLib.Enable_automatic_fan_control;
            mNormalToolStripMenuItem.Text = StringLib.Normal;
            mSilenceToolStripMenuItem.Text = StringLib.Silence;
            mPerformanceToolStripMenuItem.Text = StringLib.Performance;
            mGameToolStripMenuItem.Text = StringLib.Game;
            mShowToolStripMenuItem.Text = StringLib.Show;
            mExitToolStripMenuItem.Text = StringLib.Exit;
        }

        private void onClosing(object sender, FormClosingEventArgs e)
        {
            if (mFanControlForm != null)
            {
                mFanControlForm.Close();
                mFanControlForm = null;
            }

            if(mKrakenForm != null)
            {
                mKrakenForm.Close();
                mKrakenForm = null;
            }

            this.Visible = false;
            e.Cancel = true;
        }

        private void onTrayIconDBClicked(object sender, MouseEventArgs e)
        {
            this.ShowInTaskbar = true;
            this.Visible = true;
            this.Activate();

            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;
        }

        private void onTrayMenuEnableClick(object sender, EventArgs e)
        {
            ControlManager.getInstance().IsEnable = !ControlManager.getInstance().IsEnable;
            mEnableToolStripMenuItem.Checked = ControlManager.getInstance().IsEnable;
            ControlManager.getInstance().write();
        }

        private void onTrayMenuNormalClick(object sender, EventArgs e)
        {
            ControlManager.getInstance().ModeIndex = 0;
            mNormalToolStripMenuItem.Checked = (ControlManager.getInstance().ModeIndex == 0);
            mSilenceToolStripMenuItem.Checked = (ControlManager.getInstance().ModeIndex == 1);
            mPerformanceToolStripMenuItem.Checked = (ControlManager.getInstance().ModeIndex == 2);
            mGameToolStripMenuItem.Checked = (ControlManager.getInstance().ModeIndex == 3);
            ControlManager.getInstance().write();
        }

        private void onTrayMenuSilenceClick(object sender, EventArgs e)
        {
            ControlManager.getInstance().ModeIndex = 1;
            mNormalToolStripMenuItem.Checked = (ControlManager.getInstance().ModeIndex == 0);
            mSilenceToolStripMenuItem.Checked = (ControlManager.getInstance().ModeIndex == 1);
            mPerformanceToolStripMenuItem.Checked = (ControlManager.getInstance().ModeIndex == 2);
            mGameToolStripMenuItem.Checked = (ControlManager.getInstance().ModeIndex == 3);
            ControlManager.getInstance().write();
        }

        private void onTrayMenuPerformanceClick(object sender, EventArgs e)
        {
            ControlManager.getInstance().ModeIndex = 2;
            mNormalToolStripMenuItem.Checked = (ControlManager.getInstance().ModeIndex == 0);
            mSilenceToolStripMenuItem.Checked = (ControlManager.getInstance().ModeIndex == 1);
            mPerformanceToolStripMenuItem.Checked = (ControlManager.getInstance().ModeIndex == 2);
            mGameToolStripMenuItem.Checked = (ControlManager.getInstance().ModeIndex == 3);
            ControlManager.getInstance().write();
        }

        private void onTrayMenuGameClick(object sender, EventArgs e)
        {
            ControlManager.getInstance().ModeIndex = 3;
            mNormalToolStripMenuItem.Checked = (ControlManager.getInstance().ModeIndex == 0);
            mSilenceToolStripMenuItem.Checked = (ControlManager.getInstance().ModeIndex == 1);
            mPerformanceToolStripMenuItem.Checked = (ControlManager.getInstance().ModeIndex == 2);
            mGameToolStripMenuItem.Checked = (ControlManager.getInstance().ModeIndex == 3);
            ControlManager.getInstance().write();
        }

        private void onTrayMenuShow(object sender, EventArgs e)
        {            
            this.ShowInTaskbar = true;
            this.Visible = true;
            this.Activate();

            if (this.WindowState == FormWindowState.Minimized)
                this.WindowState = FormWindowState.Normal;
        }

        private void onTrayMenuExit(object sender, EventArgs e)
        {
            if (mFanControlForm != null)
            {
                mFanControlForm.Close();
                mFanControlForm = null;
            }

            if(mKrakenForm != null)
            {
                mKrakenForm.Close();
                mKrakenForm = null;
            }

            HardwareManager.getInstance().stop();
            
            mTrayIcon.Visible = false;

            Application.ExitThread();
            Application.Exit();
        }        

        private void createComponent()
        {
            var hardwareManager = HardwareManager.getInstance();
            var controlManager = ControlManager.getInstance();

            // temperature
            for (int i = 0; i < hardwareManager.getSensorCount(); i++)
            {
                var label = new Label();
                label.Location = new System.Drawing.Point(10, 25 + i * 25);
                label.Name = "sensorLabel" + i.ToString();
                label.Size = new System.Drawing.Size(33, 23);
                label.Text = "";
                mTempGroupBox.Controls.Add(label);
                mSensorLabelList.Add(label);

                var textBox = new TextBox();
                textBox.Location = new System.Drawing.Point(label.Left + label.Width + 5, label.Top - 5);
                textBox.Name = "sensorName" + i.ToString();
                textBox.Size = new System.Drawing.Size(mTempGroupBox.Width - 60, 23);
                textBox.Multiline = false;
                textBox.MaxLength = 40;
                textBox.Text = controlManager.getName(0, i, false);
                textBox.Leave += onSensorNameTextBoxLeaves;
                mTempGroupBox.Controls.Add(textBox);
                mSensorNameTextBoxList.Add(textBox);

                if (i < hardwareManager.getSensorCount() - 1)
                {
                    mTempGroupBox.Height = mTempGroupBox.Height + 25;
                }
            }

            // fan
            for (int i = 0; i < hardwareManager.getFanCount(); i++)
            {
                var label = new Label();
                label.Location = new System.Drawing.Point(10, 25 + i * 25);
                label.Name = "fanLabel" + i.ToString();
                label.Size = new System.Drawing.Size(60, 23);
                label.Text = "";
                mFanGroupBox.Controls.Add(label);
                mFanLabelList.Add(label);

                var textBox = new TextBox();
                textBox.Location = new System.Drawing.Point(label.Left + label.Width + 5, label.Top - 5);
                textBox.Name = "fanName" + i.ToString();
                textBox.Size = new System.Drawing.Size(mFanGroupBox.Width - 85, 23);
                textBox.Multiline = false;
                textBox.MaxLength = 40;
                textBox.Text = controlManager.getName(1, i, false);
                textBox.Leave += onFanNameTextBoxLeaves;
                mFanGroupBox.Controls.Add(textBox);
                mFanNameTextBoxList.Add(textBox);

                if (i < hardwareManager.getFanCount() - 1)
                {
                    mFanGroupBox.Height = mFanGroupBox.Height + 25;
                }
            }

            // set groupbox height
            if (mFanGroupBox.Height > mTempGroupBox.Height)
                mTempGroupBox.Height = mFanGroupBox.Height;
            else
                mFanGroupBox.Height = mTempGroupBox.Height;

            // control
            for (int i = 0; i < hardwareManager.getControlCount(); i++)
            {
                var textBox = new TextBox();
                textBox.Location = new System.Drawing.Point(10, 20 + i * 25);
                textBox.Name = "controlTextBox" + i.ToString();
                textBox.Size = new System.Drawing.Size(40, 23);
                textBox.Multiline = false;
                textBox.MaxLength = 3;
                textBox.Text = "" + hardwareManager.getControl(i).Value;
                textBox.KeyPress += onControlTextBoxKeyPress;
                textBox.TextChanged += onControlTextBoxChanges;
                mControlGroupBox.Controls.Add(textBox);
                mControlTextBoxList.Add(textBox);

                int minValue = hardwareManager.getControl(i).getMinSpeed();
                int maxValue = hardwareManager.getControl(i).getMaxSpeed();
                var tooltipString = minValue + " ≤  value ≤ " + maxValue;
                mToolTip.SetToolTip(textBox, tooltipString);

                var label = new Label();
                label.Location = new System.Drawing.Point(textBox.Left + textBox.Width + 2, 25 + i * 25);
                label.Name = "controlLabel" + i.ToString();
                label.Size = new System.Drawing.Size(15, 23);
                label.Text = "%";
                mControlGroupBox.Controls.Add(label);
                mControlLabelList.Add(label);

                var textBox2 = new TextBox();
                textBox2.Location = new System.Drawing.Point(label.Left + label.Width + 5, label.Top - 5);
                textBox2.Name = "controlName" + i.ToString();
                textBox2.Size = new System.Drawing.Size(mControlGroupBox.Width - 85, 23);
                textBox2.Multiline = false;
                textBox2.MaxLength = 40;
                textBox2.Text = controlManager.getName(2, i, false);
                textBox2.Leave += onFanControlNameTextBoxLeaves;
                mControlGroupBox.Controls.Add(textBox2);
                mControlNameTextBoxList.Add(textBox2);

                if (i < hardwareManager.getControlCount() - 1)
                {
                    mControlGroupBox.Height = mControlGroupBox.Height + 25;
                }
            }

            // set groupbox height
            if (mFanGroupBox.Height > mControlGroupBox.Height)
            {
                mControlGroupBox.Height = mFanGroupBox.Height;
            }
            else
            {
                mTempGroupBox.Height = mControlGroupBox.Height;
                mFanGroupBox.Height = mControlGroupBox.Height;
            }            

            // position
            mOptionButton.Top = mFanGroupBox.Top + mFanGroupBox.Height + 6;            
            mFanControlButton.Top = mFanGroupBox.Top + mFanGroupBox.Height + 6;
            mKrakenButton.Top = mFanGroupBox.Top + mFanGroupBox.Height + 6;
            mMadeLabel.Top = mFanGroupBox.Top + mFanGroupBox.Height + 20;
            this.Height = mFanGroupBox.Height + mOptionButton.Height + 70;
        }

        private void onControlTextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar) == false)
            {
                e.Handled = true;
            }
        }

        private void onControlTextBoxChanges(object sender, EventArgs e)
        {
            var textBox = (TextBox)sender;
            if (textBox.Focused == false)
                return;

            var hardwareManager = HardwareManager.getInstance();
            int value = int.Parse(textBox.Text);
            for (int i = 0; i < mControlTextBoxList.Count; i++)
            {
                if (mControlTextBoxList[i].Equals(sender) == true)
                {
                    int minValue = hardwareManager.getControl(i).getMinSpeed();
                    int maxValue = hardwareManager.getControl(i).getMaxSpeed();

                    if(value >= minValue && value <= maxValue)
                    {
                        int changeValue = hardwareManager.addChangeValue(value, hardwareManager.getControl(i));
                        if (changeValue != value)
                        {
                            textBox.Text = changeValue.ToString();
                        }
                    }
                    break;
                }
            }
        }

        private void onSensorNameTextBoxLeaves(object sender, EventArgs e)
        {
            this.onNameTextBoxLeaves((TextBox)sender, 0, ref mSensorNameTextBoxList);
        }

        private void onFanNameTextBoxLeaves(object sender, EventArgs e)
        {
            this.onNameTextBoxLeaves((TextBox)sender, 1, ref mFanNameTextBoxList);
        }

        private void onFanControlNameTextBoxLeaves(object sender, EventArgs e)
        {
            this.onNameTextBoxLeaves((TextBox)sender, 2, ref mControlNameTextBoxList);
        }

        private void onNameTextBoxLeaves(TextBox textBox, int type, ref List<TextBox> nameTextBoxList)
        {
            var controlManager = ControlManager.getInstance();
            int index = -1;
            int num = 2;
            string name = textBox.Text;

            while (true)
            {
                bool isExist = false;
                for (int i = 0; i < nameTextBoxList.Count; i++)
                {
                    if (nameTextBoxList[i] == textBox)
                    {
                        if (name.Length == 0)
                        {
                            textBox.Text = controlManager.getName(type, i, false);
                            return;
                        }

                        index = i;
                        continue;
                    }

                    else if (nameTextBoxList[i].Text.Equals(name) == true)
                    {
                        isExist = true;
                        break;
                    }
                }

                if (isExist == true)
                {
                    name = textBox.Text + " #" + num++;
                    continue;
                }

                textBox.Text = name;
                controlManager.setName(type, index, false, name);
                break;
            }
            controlManager.write();
        }

        private void onUpdate()
        {
            if (this.Visible == false)
                return;

            this.BeginInvoke(new Action(delegate ()
            {
                var hardwareManager = HardwareManager.getInstance();
                
                for (int i = 0; i < hardwareManager.getSensorCount(); i++)
                {
                    var sensor = hardwareManager.getSensor(i);
                    if (sensor == null)
                        break;

                    mSensorLabelList[i].Text = sensor.getString();
                }

                for (int i = 0; i < hardwareManager.getFanCount(); i++)
                {
                    var fan = hardwareManager.getFan(i);
                    if (fan == null)
                        break;

                    mFanLabelList[i].Text = fan.getString();
                }

                for (int i = 0; i < hardwareManager.getControlCount(); i++)
                {
                    var control = hardwareManager.getControl(i);
                    if (control == null)
                        break;

                    if (mControlTextBoxList[i].Focused == false)
                    {
                        mControlTextBoxList[i].Text = control.Value.ToString();
                    }
                }

                if (mFanControlForm != null)
                    mFanControlForm.onUpdateTimer();
            }));
        }
        

        private void onOptionButtonClick(object sender, EventArgs e)
        {
            var form = new OptionForm();
            form.OnExitHandler += onTrayMenuExit;
            if (form.ShowDialog() == DialogResult.OK)
            {
                HardwareManager.getInstance().restartTimer(OptionManager.getInstance().Interval);
            }
        }

        private void onFanControlButtonClick(object sender, EventArgs e)
        {
            if(mFanControlForm != null)
            {
                mFanControlForm.Focus();
                return;
            }

            for(int i = 0; i < mSensorNameTextBoxList.Count; i++)
                mSensorNameTextBoxList[i].Enabled = false;

            for (int i = 0; i < mFanNameTextBoxList.Count; i++)
                mFanNameTextBoxList[i].Enabled = false;

            for (int i = 0; i < mControlNameTextBoxList.Count; i++)
                mControlNameTextBoxList[i].Enabled = false;

            mFanControlForm = new FanControlForm();
            mFanControlForm.onCloseCallback += (sender2, e2) =>
            {
                mFanControlForm = null;

                for (int i = 0; i < mSensorNameTextBoxList.Count; i++)
                    mSensorNameTextBoxList[i].Enabled = true;

                for (int i = 0; i < mFanNameTextBoxList.Count; i++)
                    mFanNameTextBoxList[i].Enabled = true;

                for (int i = 0; i < mControlNameTextBoxList.Count; i++)
                    mControlNameTextBoxList[i].Enabled = true;
            };
            mFanControlForm.onApplyCallback += (sender2, e2) =>
            {
                mEnableToolStripMenuItem.Checked = ControlManager.getInstance().IsEnable;
                mNormalToolStripMenuItem.Checked = (ControlManager.getInstance().ModeIndex == 0);
                mSilenceToolStripMenuItem.Checked = (ControlManager.getInstance().ModeIndex == 1);
                mPerformanceToolStripMenuItem.Checked = (ControlManager.getInstance().ModeIndex == 2);
                mGameToolStripMenuItem.Checked = (ControlManager.getInstance().ModeIndex == 3);
            };
            mFanControlForm.StartPosition = FormStartPosition.Manual;
            mFanControlForm.Location = new Point(this.Location.X + 100, this.Location.Y + 100);
            mFanControlForm.Show(this);
        }

        private void onKrakenButtonClick(object sender, EventArgs e)
        {
            if(mKrakenForm != null)
            {
                mKrakenForm.Focus();
                return;
            }

            mKrakenForm = new KrakenForm(HardwareManager.getInstance().getKrakenX());
            mKrakenForm.onCloseCallback += (sender2, e2) =>
            {
                mKrakenForm = null;
            };
            mKrakenForm.StartPosition = FormStartPosition.Manual;
            mKrakenForm.Location = new Point(this.Location.X + 100, this.Location.Y + 100);
            mKrakenForm.Show(this);
        }        
    }
}
