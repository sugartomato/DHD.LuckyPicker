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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DHD.LuckyPicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private List<String> _Users = new List<string>();
        System.Threading.Timer? _Timer = null;

        /// <summary>
        /// 标记是否正在随机选择中
        /// </summary>
        private Boolean IsDoRandom = false;

        private List<TextBlock> _NameControls = new List<TextBlock>();
        private Models.Level? _CurrentLevel = null;

        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            SetBackImage();

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _Users = Config.Users;
            // 从人名单中移除已获取奖项的人
            List<String> picked = Config.PickedNames;
            if (picked != null && picked.Count > 0)
            {
                foreach (String tmp in picked)
                {
                    _Users.Remove(tmp);
                }
            }

            // 加载奖项选择列表
            CTRL_DROPDOWN_LEVEL.ItemsSource = Config.Levels;
            CTRL_DROPDOWN_LEVEL.DisplayMemberPath = "Name";
            CTRL_DROPDOWN_LEVEL.SelectedValuePath = "Number";
            CTRL_DROPDOWN_LEVEL.SelectedIndex = 0;

            // 加载背景图选择列表
            CTRL_DROPDOWN_BackImage.ItemsSource = Config.BackgroundImageNames;
            CTRL_DROPDOWN_BackImage.SelectedIndex = Config.BackgroundImageNames.IndexOf(Config.BackgroundImageName);

            // 设置抽奖按钮文字
            CTRL_BUTTON_DOPICK.Content = Config.PickButtonText;

        }






        #region 操作处理

        /// <summary>
        /// 关闭按钮处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClick_Close(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确认要退出吗？", "提醒", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                this.Close();
            }
        }

        private void OnClick_MaxWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Maximized;
        }

        private void OnClick_MinWindow(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// 复位。清空所有已经抽取的奖项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClick_Reset(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确认要复位吗？复位之后，将清空已获奖数据！", "提醒", MessageBoxButton.OKCancel, MessageBoxImage.Question) == MessageBoxResult.OK)
            {
                Config.ClearPickResult();
                _Users = Config.Users;
                CTRL_TXT_PICKED.Text = "";
                UpdatePickNumberOfUI();
                MessageBox.Show("已复位！");
            }
        }

        private void OnClick_ShowResult(object sender, RoutedEventArgs e)
        {
            Window_PickResult wd = new Window_PickResult();
            wd.Show();
        }

        #endregion

        #region 抽奖相关业务处理

        /// <summary>
        /// 设置区域奖项选择切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChange_Level(object sender, SelectionChangedEventArgs e)
        {
            CTRL_TXT_LEVEL_NAME.Text = "";
            CTRL_TXT_PICKED.Text = "";

            // 切换奖项时，清空当前选择区域生成的内容
            CTRL_SHOWREGION_A.Children.Clear();
            CTRL_SHOWREGION_B.Children.Clear();

            if (CTRL_DROPDOWN_LEVEL.SelectedValue != null && !String.IsNullOrEmpty(CTRL_DROPDOWN_LEVEL.SelectedValue.ToString()))
            {
                Int32 levelNumber = -1;
                Int32.TryParse(CTRL_DROPDOWN_LEVEL.SelectedValue.ToString(), out levelNumber);

                _CurrentLevel = Config.Levels.FirstOrDefault(x => x.Number == levelNumber);
                if (_CurrentLevel != null)
                {
                    CTRL_CurrentLevelName.Text = _CurrentLevel.Name; // 修改奖项选择区域顶部标题

                    CTRL_TXT_LEVEL_NAME.Text = _CurrentLevel.Name + ":";
                    if (Config.PickResults != null && Config.PickResults.Count > 0)
                    {
                        Models.PickResult? currentPick = Config.PickResults.FirstOrDefault(x => x.LevelNumber == _CurrentLevel.Number);
                        if (currentPick != null && currentPick.Names != null && currentPick.Names.Count > 0)
                        {
                            CTRL_TXT_PICKED.Text = String.Join(',', currentPick.Names);
                        }
                    }

                    UpdatePickNumberOfUI();
                }
            }
        }

        /// <summary>
        /// 获取当前奖项可以抽取的人数
        /// </summary>
        /// <returns></returns>
        private Int32 GetPickNumber()
        {
            if (_CurrentLevel == null) return 0;
            Models.PickResult? pickResult = Config.PickResults.FirstOrDefault(x => x.LevelNumber == _CurrentLevel.Number);
            if (pickResult == null)
            {
                return _Users.Count > _CurrentLevel.PerSelect ? _CurrentLevel.PerSelect : _Users.Count;
            }
            else
            {
                // 如果有该奖项的已抽取数据，对比已抽奖人数
                if (pickResult.Names != null && pickResult.Names.Count > 0)
                {
                    // 得到剩余抽取人数
                    Int32 restNumber = _CurrentLevel.MaxNumber - pickResult.Names.Count;
                    Int32 pickNumber = restNumber > _CurrentLevel.PerSelect ? _CurrentLevel.PerSelect : restNumber;

                    // 如果除去将要抽取的人数，剩余1人，则将剩余1人添加到当前抽取数量中。
                    Int32 perReset = _CurrentLevel.PerSelect - restNumber;
                    if (Math.Abs(perReset) == 1)
                    {
                        pickNumber += 1;
                    }
                    return _Users.Count > pickNumber ? pickNumber : _Users.Count;
                }
                else
                {
                    return _Users.Count > _CurrentLevel.PerSelect ? _CurrentLevel.PerSelect : _Users.Count;
                }
            }
        }

        /// <summary>
        /// 更新界面上的获奖统计数字
        /// </summary>
        private void UpdatePickNumberOfUI()
        {

            // 总人数池剩余人数
            if (_Users != null)
            {
                CTRL_TXT_POOL_REMAIN.Text = $"【人数池剩余：{_Users.Count}人。】";
            }

            // 当前奖项已选人数和剩余人数
            if (_CurrentLevel != null)
            {
                if (Config.PickResults != null)
                {
                    Models.PickResult? result = Config.PickResults.FirstOrDefault(x => x.LevelNumber == _CurrentLevel.Number);
                    if (result != null && result.Names != null)
                    {
                        CTRL_TXT_LEVEL_PICK_COUNT.Text = $"（当前奖项总额[{_CurrentLevel.MaxNumber}]个，已抽取[{result.Names.Count}]个，剩余[{_CurrentLevel.MaxNumber - result.Names.Count}]个）";
                    }
                    else
                    {
                        CTRL_TXT_LEVEL_PICK_COUNT.Text = $"（当前奖项总额[{_CurrentLevel.MaxNumber}]个，已抽取[0]个，剩余[{_CurrentLevel.MaxNumber}]个）";
                    }
                }
                else
                {
                    CTRL_TXT_LEVEL_PICK_COUNT.Text = $"（当前奖项总额[{_CurrentLevel.MaxNumber}]个，已抽取[0]个，剩余[{_CurrentLevel.MaxNumber}]个）";
                }
            }

        }

        private void UpdatePickNamesToUI(List<String> names)
        {
            for (Int32 i = 0; i < names.Count; i++)
            {
                _NameControls[i].Text = names[i];
            }
        }

        /// <summary>
        /// 界面抽奖按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClick_Pick(object sender, RoutedEventArgs e)
        {
            if (_CurrentLevel == null)
            {
                MessageBox.Show("请先选择奖项！");
                return;
            }

            if (IsDoRandom)
            {
                // 随机选择中，点击进行停止，
                IsDoRandom = !IsDoRandom;
                CTRL_BUTTON_DOPICK.Content = Config.PickButtonText;
                List<String> names = new List<string>();
                foreach (TextBlock ctrl in _NameControls)
                {
                    _Users.Remove(ctrl.Text);
                    CTRL_TXT_PICKED.Text = CTRL_TXT_PICKED.Text + ctrl.Text + ",";
                    names.Add(ctrl.Text);
                }

                Config.AddPickResult(new Models.PickResult()
                {
                    LevelNumber = _CurrentLevel.Number,
                    Names = names
                });

                UpdatePickNumberOfUI();
            }
            else
            {

                CTRL_SHOWREGION_A.Children.Clear();
                CTRL_SHOWREGION_B.Children.Clear();

                _NameControls = new List<TextBlock>();

                Int32 pickNumber = GetPickNumber();
                if (pickNumber == 0)
                {
                    MessageBox.Show("该奖项的可抽奖人数为0！");
                    return;
                }
                // 生成控件
                for (Int32 i = 0; i < pickNumber; i++)
                {
                    TextBlock tmpOBj = new TextBlock();
                    tmpOBj.Style = FindResource("TextBlock_UserName") as Style;
                    tmpOBj.Text = "";
                    if (i < 3)
                    {
                        CTRL_SHOWREGION_A.Children.Add(tmpOBj);
                    }
                    else
                    {
                        CTRL_SHOWREGION_B.Children.Add(tmpOBj);
                    }
                    //Canvas.SetLeft(tmpOBj, 100 * (i + 1));
                    //Canvas.SetTop(tmpOBj, CTRL_SHOWREGION.Height / 2 - tmpOBj.Height);
                    _NameControls.Add(tmpOBj);
                }


                // 获取当前选择的奖项设置
                // 获取当前奖项已经获奖人员名单
                // 生成所需控件
                // 绘制控件

                // 启动计时器，随机刷新
                Task.Factory.StartNew(new Action(() =>
                {
                    Task t = new Task(() => { DoRandomPicker(); });
                    t.Start();
                }));
                IsDoRandom = !IsDoRandom;
                CTRL_BUTTON_DOPICK.Content = "停止";
            }
        }

        /// <summary>
        /// 循环随机显示名单池中名单
        /// </summary>
        private void DoRandomPicker()
        {
            while (IsDoRandom)
            {
                List<String> result = new List<string>();
                while (true)
                {
                    for (Int32 i = 0; i < GetPickNumber(); i++)
                    {
                        String tmp = _Users[new Random().Next(0, _Users.Count)];
                        if (!result.Contains(tmp))
                        {
                            result.Add(tmp);
                        }
                    }
                    break;
                }
                Action<List<String>> action = new Action<List<String>>(UpdatePickNamesToUI);
                this.Dispatcher.BeginInvoke(action, result);
                System.Threading.Thread.Sleep(20);
            }
        }


        #endregion

        private void OnMouseDown_WindowControlBar(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void OnDoubleClick_WindowControlBar(object sender, MouseButtonEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }



        #region 背景图处理


        private void SetBackImage()
        {
            ImageBrush backBrush = new ImageBrush();
            backBrush.ImageSource = Config.BackgroundImageSource;
            backBrush.Opacity = Config.BackgroundOpacity;
            this.Background = backBrush;
        }

        private void OnChange_BackImage(object sender, SelectionChangedEventArgs e)
        {

            ComboBox ctrl = CTRL_DROPDOWN_BackImage;

            if (ctrl.SelectedValue != null && !String.IsNullOrEmpty(ctrl.SelectedValue.ToString()))
            {
                String imgName = ctrl.SelectedValue.ToString();
                Config.BackgroundImageName = imgName;
                SetBackImage();
            }
        }


        #endregion

    }
}
