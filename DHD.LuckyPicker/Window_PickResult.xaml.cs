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

namespace DHD.LuckyPicker
{
    /// <summary>
    /// Window_PickResult.xaml 的交互逻辑
    /// </summary>
    public partial class Window_PickResult : Window
    {
        public Window_PickResult()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
            this.Topmost = true;
            ImageBrush backBrush = new ImageBrush();
            backBrush.Opacity = Config.BackgroundOpacity;
            backBrush.ImageSource = Config.BackgroundImageSource;
            this.Background = backBrush;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var rowDefs = CTRL_ResultGrid.RowDefinitions;
                Int32 rowIndex = 0;
                // 获取所有奖项设置
                List<Models.Level> levels = Config.Levels;
                if (levels != null && levels.Count > 0)
                {
                    levels = levels.OrderBy(x => x.Number).ToList();

                    // 遍历奖项设置，创建对应的行，追加到Grid中
                    foreach (Models.Level level in levels)
                    {
                        var rowDefTitle = new RowDefinition();
                        rowDefTitle.Height = new GridLength(80);
                        rowDefs.Add(rowDefTitle);

                        // 创建标题行
                        StackPanel titlePanel = new StackPanel();
                        Border titleBorder = new Border();
                        titleBorder.Background = new SolidColorBrush() { Opacity = 0.3, Color = Colors.LightPink };
                        TextBlock titleText = new TextBlock();
                        titleText.Text = level.Name;
                        titleText.Style = (Style)FindResource("LevelName");
                        titleBorder.Child = titleText;
                        titlePanel.Children.Add(titleBorder);

                        // 标题行添加到Grid容器中
                        CTRL_ResultGrid.Children.Add(titlePanel);
                        titlePanel.SetValue(Grid.RowProperty, rowIndex);

                        rowIndex += 1;

                        // 创建并添加获奖名单行
                        var rowDefContent = new RowDefinition();
                        rowDefContent.Height = new GridLength(130);
                        rowDefs.Add(rowDefContent);

                        TextBlock contentText = new TextBlock();
                        List<Models.PickResult> pickResults = Config.PickResults;
                        contentText.Text = "";
                        if (pickResults != null && pickResults.Count > 0)
                        {
                            var pickResult = pickResults.FirstOrDefault(x => x.LevelNumber == level.Number);
                            if (pickResult != null)
                            {
                                contentText.Text = String.Join(",  ", pickResult.Names);
                            }
                        }

                        contentText.Style = (Style)FindResource("LevelResult");
                        CTRL_ResultGrid.Children.Add(contentText);
                        contentText.SetValue(Grid.RowProperty, rowIndex);

                        rowIndex += 1;
                    }
                }


                // 20240118 以下为原始处理方法，已弃用
                // 获取所有中奖数据，按照奖项级别排序
                List<Models.PickResult> result = Config.PickResults;
                if (result != null && result.Count > 0)
                {

                    result = result.OrderBy(x => x.LevelNumber).ToList();

                    foreach (Models.PickResult pickTemp in result)
                    {
                        if (pickTemp.LevelNumber == 1 && pickTemp.Names != null && pickTemp.Names.Count > 0)
                        {
                            CTRL_TXT_RESULT_LEVEL1.Text = String.Join(',', pickTemp.Names);
                        }
                        if (pickTemp.LevelNumber == 2 && pickTemp.Names != null && pickTemp.Names.Count > 0)
                        {
                            CTRL_TXT_RESULT_LEVEL2.Text = String.Join(',', pickTemp.Names);
                        }
                        if (pickTemp.LevelNumber == 3 && pickTemp.Names != null && pickTemp.Names.Count > 0)
                        {
                            CTRL_TXT_RESULT_LEVEL3.Text = String.Join(',', pickTemp.Names);
                        }
                    }
                }
                else
                {
                    //TextBlock txtNoResult = new TextBlock();
                    //txtNoResult.Text = "还没有人中奖哦！";
                    //CTRL_GRID_SHOW_PANEL.Children.Add(txtNoResult);
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show($"窗口启动异常：{ex.Message}");
            }
        }

        // 标题行拖拽移动窗口和放大缩小窗口
        private void CTRL_HeaderText_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ClickCount >= 2)
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
                else
                {
                    this.DragMove();
                }
            }
            catch (Exception)
            {
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
