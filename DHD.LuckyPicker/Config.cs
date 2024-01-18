using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using System.Xaml;
using System.Xml;
using System.Xml.Linq;

namespace DHD.LuckyPicker
{
    internal class Config
    {

        private static XDocument _xmlDoc = new();
        private static XElement _xmlRoot = new("N11111");
        private static readonly String _configFilePath = AppDomain.CurrentDomain.BaseDirectory + "Config.xml";
        internal static void Load()
        {
            _xmlDoc = XDocument.Load(_configFilePath);
            if (_xmlDoc != null && _xmlDoc.Root != null)
            {
                _xmlRoot = _xmlDoc.Root;
            }
        }

        public static String AppRootPath
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory.Trim('\\') + "\\";
            }
        }

        public static String ImageFolderPath
        {
            get
            {
                return AppRootPath + "Images\\Background\\";
            }
        }

        /// <summary>
        /// 获取奖项设定
        /// </summary>
        /// <returns></returns>
        internal static List<Models.Level> Levels
        {
            get
            {
                List<Models.Level> result = new List<Models.Level>();
                XElement? t = _xmlRoot.Element("Levels");
                if (t != null)
                {
                    IEnumerable<XElement> nodes = t.Elements();
                    foreach (XElement node in nodes)
                    {
                        Models.Level tmp = new Models.Level();
                        tmp.Number = Int32.Parse(node.Attribute("Number").Value);
                        tmp.MaxNumber = Int32.Parse(node.Attribute("MaxNumber").Value);
                        tmp.PerSelect = Int32.Parse(node.Attribute("PerSelect").Value);
                        tmp.Name = node.Attribute("Name").Value;
                        result.Add(tmp);
                    }
                    result = result.OrderByDescending(x => x.Number).ToList();
                }
                return result;
            }
        }

        /// <summary>
        /// 获取已选结果
        /// </summary>
        internal static List<Models.PickResult> PickResults
        {
            get
            {
                List<Models.PickResult> result = new List<Models.PickResult>();

                XElement? baseNode = _xmlRoot.Element("PickResults");
                if (baseNode != null)
                {
                    IEnumerable<XElement> nodes = baseNode.Elements();
                    foreach (XElement node in nodes)
                    {
                        Models.PickResult tmp = new Models.PickResult();

                        // 尝试转换奖项级别数字
                        Int32 tmpLevelNumber = -1;
                        if (!Int32.TryParse(node.Attribute("LevelNumber")?.Value, out tmpLevelNumber))
                        {
                            throw new ApplicationException("读取已抽结果时发生异常，无法将节点的LevelNumber转换为有效的数字，请检查配置文件是否被修改！");
                        }
                        tmp.LevelNumber = tmpLevelNumber;
                        tmp.Names = new List<string>();
                        String userNameStr = node.Value;
                        String[] userNameArr = userNameStr.Split(',');
                        foreach (String userName in userNameArr)
                        {
                            tmp.Names.Add(userName);
                        }
                        result.Add(tmp);
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 获取所有已经选择的人员名单，不分奖项级别
        /// </summary>
        internal static List<String> PickedNames
        {
            get
            {
                List<String> result = new List<string>();
                List<Models.PickResult> picked = PickResults;
                if (picked != null && picked.Count > 0)
                {
                    foreach (Models.PickResult tmp in picked)
                    {
                        if (tmp.Names != null && tmp.Names.Count > 0)
                        {
                            foreach (String name in tmp.Names)
                            {
                                result.Add(name);
                            }
                        }
                    }
                }
                return result;
            }
        }

        /// <summary>
        /// 将抽取名单添加到配置文件的选择结果节点中
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        internal static Boolean AddPickResult(Models.PickResult result)
        {

            XElement? pickResultsNode = _xmlRoot.Element("PickResults");
            XElement? element = null;
            if (pickResultsNode != null)
            {
                element = pickResultsNode.Elements()?.FirstOrDefault(x => x.Attribute("LevelNumber")?.Value == result.LevelNumber.ToString());


                if (element != null)
                {
                    element.SetValue(element.Value + "," + String.Join(',', result.Names));
                }
                else
                {
                    element = new XElement("PickResult");
                    element.SetAttributeValue("LevelNumber", result.LevelNumber);
                    element.SetValue(String.Join(',', result.Names));
                    pickResultsNode.Add(element);
                }
            }
            else
            {
                throw new ApplicationException("配置文件中缺少关键节点：PickResults");
            }

            Save();
            return true;
        }

        /// <summary>
        /// 清空所有已选结果
        /// </summary>
        /// <returns></returns>
        internal static Boolean ClearPickResult()
        {
            XElement? pickResultsNode = _xmlRoot.Element("PickResults");
            if (pickResultsNode != null)
            {
                pickResultsNode.RemoveNodes();
            }
            Save();
            return true;
        }


        /// <summary>
        /// 获取抽奖池所有人员名单
        /// </summary>
        internal static List<String> Users
        {
            get
            {
                XElement? usersNode = _xmlRoot.Element("Users");
                if (usersNode != null)
                {
                    String userStr = usersNode.Value;
                    String[] arr = userStr.Split(',');
                    List<String> result = new List<String>();
                    foreach (String ar in arr)
                    {
                        if (!result.Contains(ar))
                        {
                            result.Add(ar);
                        }
                    }
                    return result;
                }
                else
                {
                    return new List<String>();
                }
            }
        }


        #region 背景图设置

        /// <summary>
        /// 获取界面背景图对象
        /// </summary>
        public static BitmapImage BackgroundImageSource
        {
            get
            {
                try
                {
                    return new BitmapImage(new Uri(".\\Images\\Background\\" + BackgroundImageName, UriKind.Relative));
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("配置中获取背景图对象失败：" + ex.Message);
                }

            }
        }


        /// <summary>
        /// 获取或者设置背景图的名称
        /// </summary>
        public static String BackgroundImageName
        {
            get
            {
                XElement? baseNode = _xmlRoot.Element("BackgroundImage");
                if (baseNode != null)
                {
                    String val = baseNode.Value;
                    return val;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                XElement? baseNode = _xmlRoot.Element("BackgroundImage");
                if (baseNode != null)
                {
                    baseNode.Value = value;
                    Save();
                }
            }
        }

        /// <summary>
        /// 背景图集合
        /// </summary>
        internal static List<String> BackgroundImageNames
        {
            get
            {
                List<String> result = new List<String>();
                String[] files = System.IO.Directory.GetFiles(ImageFolderPath);
                foreach (String file in files)
                {
                    result.Add(System.IO.Path.GetFileName(file));
                }
                return result;
            }
        }

        /// <summary>
        /// 背景图的透明度。默认为0.8
        /// </summary>
        internal static double BackgroundOpacity
        {
            get {
                double result = 0.8;
                XElement? baseNode = _xmlRoot.Element("BackgroundOpacity");
                if (baseNode != null)
                {
                    _ = double.TryParse(baseNode.Value, out result);
                }
                return result;
            }
        }

        #endregion

        #region 其它界面设置

        /// <summary>
        /// 抽奖开始按钮文字
        /// </summary>
        internal static String PickButtonText
        {
            get {
                //PickButtonText
                XElement? usersNode = _xmlRoot.Element("PickButtonText");
                if (usersNode != null)
                {
                    if (!String.IsNullOrEmpty(usersNode.Value))
                    {
                        return usersNode.Value;
                    }
                    else
                    {
                        return "开始";
                    }
                }
                else
                {
                    return "开始";
                }
            }
        }


        #endregion

        /// <summary>
        /// 保存配置
        /// </summary>
        private static void Save()
        {
            _xmlDoc.Save(_configFilePath);
        }




    }
}
