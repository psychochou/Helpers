using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using Shared;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Helpers
{
    public partial class mainForm : Form
    {
        private readonly string _dataPath;
        private string _defaultDatabase;
        private Article _article;
        public mainForm()
        {
            /*
             Install-Package Markdig
             Install-Package sqlite-net-pcl
             Install-Package HtmlAgilityPack -Pre
             Install-Package DotNetZip -Version 1.10.1
             Install-Package Microsoft.jQuery.Unobtrusive.Ajax -Version 3.2.3
             Install-Package SSH.NET -Version 2016.1.0-beta1 -Pre
             Install-Package Newtonsoft.Json
             Install-Package System.ValueTuple -Version 4.3.1
             */
            InitializeComponent();

            _dataPath = "datas".GetCommandPath();
            _dataPath.CreateDirectoryIfNotExists();
            _defaultDatabase = _dataPath.Combine("db.dat");

            if (!_defaultDatabase.FileExists())
                HelperSqlite.GetInstance(_defaultDatabase);

            databaseBox.Items.AddRange(_dataPath.GetFiles("*.dat").Select(i => i.GetFileName()).ToArray());
            //databaseBox.SelectedIndex = 0;
        }

        private void 复制ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textBox.SelectedText.IsVacuum())
            {
                textBox.SelectLine(true);
            }
            textBox.Copy();
        }

        private void 粘贴ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.Paste();
        }

        private void textBox_TextChanged(object sender, EventArgs e)
        {
            if (!this.Text.EndsWith("*"))
                this.Text += " *";
        }

        private void 保留正则表达式ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (findBox.Text.IsReadable())
                textBox.Text = textBox.Text.Matches(findBox.Text).ToLine();

        }

        private void 删除_Click(object sender, EventArgs e)
        {
            textBox.Delete();
        }

        private void 翻译_Click(object sender, EventArgs e)
        {
            textBox.SelectLine(true);

            var val = textBox.SelectedText;
            if (val.IsVacuum()) return;
            var json = HelperTranslator.GetInstance().QueryEnglish(val);

            var obj = Newtonsoft.Json.Linq.JObject.Parse(json);
            Newtonsoft.Json.Linq.JToken jtoken;

            if (!obj.TryGetValue("sentences", out jtoken)) return;

            var sb = new StringBuilder();
            foreach (var item in jtoken)
            {
                sb.AppendLine(item["trans"].ToString()).AppendLine(item["orig"].ToString());
            }
            textBox.Text = sb.ToString();
        }

        private void 程序_Click(object sender, EventArgs e)
        {

            Process.Start("".GetCommandPath());
        }

        private void databaseBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _defaultDatabase = _dataPath.Combine(databaseBox.Text);
            HelperSqlite.GetInstance(_defaultDatabase);
            UpdateList();

        }
        private void UpdateList()
        {

            listBox.Items.Clear();
            listBox.Items.AddRange(HelperSqlite.GetInstance().GetTitleList().ToArray());
        }
        private void 保存_Click(object sender, EventArgs e)
        {
            if (textBox.Text.IsVacuum()) return;
            if (_article == null)
            {
                var title = textBox.Text.GetFirstReadable().TrimStart(new char[] { ' ', '#' });
                _article = new Article
                {
                    Title = title,
                    Content = textBox.Text,
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow,
                };
                HelperSqlite.GetInstance().Insert(_article);
                _article = HelperSqlite.GetInstance().GetArticle(title);
                UpdateList();
                this.Text = title;
            }
            else
            {
                var title = textBox.Text.GetFirstReadable().TrimStart(new char[] { ' ', '#' });
                var updateList = false;
                if (_article.Title != title)
                {
                    updateList = true;
                }
                _article.Title = title;
                _article.Content = textBox.Text;
                _article.UpdateAt = DateTime.UtcNow;

                HelperSqlite.GetInstance().Update(_article);
                if (updateList)
                {
                    UpdateList();

                }
                this.Text = title;
            }
        }

        private void listBox_DoubleClick(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex == -1) return;
            var title = listBox.SelectedItem.ToString();
            _article = HelperSqlite.GetInstance().GetArticle(title);

            this.Text = _article.Title;
            textBox.Text = _article.Content;
        }

        private void 压缩目录加密ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnDirectoy((dir) =>
            {
                var dstDir = @"c:\psycho\.RAR";
                dstDir.CreateDirectoryIfNotExists();
                HelperZip.CompressDirectoryEncrypt(dir, dstDir);
            });
        }

        private void 新建_Click(object sender, EventArgs e)
        {
            _article = null;
            if (textBox.Text.IsReadable())
            {
                var (title, head, content) = textBox.Text.FormatArticle();
                var prefix = title;//textBox.Text.Split(new char[] { '\n' }, 2).First().Trim();
                if (prefix.Contains(" - "))
                {
                    prefix = prefix.Substring(0, prefix.IndexOf("-") + 2);

                }
                else
                if (prefix.Contains("："))
                {
                    prefix = prefix.Substring(0, prefix.IndexOf("：") + 1);

                }
                textBox.Text = prefix + Environment.NewLine + Environment.NewLine + head + Environment.NewLine + Environment.NewLine;
            }
            else

                textBox.Text = string.Empty;

            this.Text = string.Empty;

        }

        private void 中文_Click(object sender, EventArgs e)
        {
            textBox.SelectLine(true);

            var val = textBox.SelectedText;
            if (val.IsVacuum()) return;
            var json = HelperTranslator.GetInstance().QueryChinese(val);

            var obj = Newtonsoft.Json.Linq.JObject.Parse(json);
            Newtonsoft.Json.Linq.JToken jtoken;

            if (!obj.TryGetValue("sentences", out jtoken)) return;

            var sb = new StringBuilder();
            foreach (var item in jtoken)
            {
                sb.AppendLine(item["trans"].ToString()).AppendLine(item["orig"].ToString());
            }
            textBox.Text = sb.ToString();
        }

        private void 获取当前鼠标坐标ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var (x, y) = HelperSimulator.GetCursorPosition();
            textBox.Text = $"{x},{y}";
        }

        private void 模拟上传百度云ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var handle = HelperSimulator.GetBaiduNetHandle().First();
            Win32.SetForegroundWindow(handle);
            Thread.Sleep(1000);
            foreach (var item in textBox.Text.ToLines())
            {
                try
                {
                    Clipboard.SetText(item);


                    HelperSimulator.MouseLeftClick(526, 123);
                    //    Thread.Sleep(500);
                    HelperSimulator.MouseLeftClick(773, 483);
                    Thread.Sleep(500);


                    // HelperSimulator.KeyPressWithCtrl(handle_, Keys.V);
                    SendKeys.SendWait("^{v}");

                    SendKeys.Flush();
                    Thread.Sleep(1000);
                    HelperSimulator.MouseLeftClick(1164, 637);
                    Thread.Sleep(1000);
                    HelperSimulator.MouseLeftClick(1227, 332);
                }
                catch
                {

                }
            }
        }

        private void 获取当前鼠标窗口句柄ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.Text += Win32.GetHandleOfWindowMouseIsOver();

        }

        private void 保留行正则表达式ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (findBox.Text.IsReadable())
                textBox.Text = textBox.Text.ToLines().Where(i => Regex.IsMatch(i, findBox.Text)).ToLine();

        }

        private void 代码_Click(object sender, EventArgs e)
        {
            textBox.SelectedText = textBox.SelectedText.FormatCode();
        }

        private void 粗体_Click(object sender, EventArgs e)
        {
            textBox.SelectedText = textBox.SelectedText.FormatStrong();
        }

        private void 标题_Click(object sender, EventArgs e)
        {
            var start = textBox.SelectionStart;

            while (start - 1 > -1 && textBox.Text[start - 1] != '\n')
            {
                start--;
            }
            textBox.SelectionStart = start;
            textBox.SelectionLength = 0;
            if (textBox.Text[start] == '#')
                textBox.SelectedText = "#";
            else
                textBox.SelectedText = "# ";

        }

        private void 斜体_Click(object sender, EventArgs e)
        {
            textBox.SelectedText = textBox.SelectedText.FormatEm();

        }

        private void 图片_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "所有支持的图片|*.jpg;*.jpeg;*.gif;*.svg;*.png";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var dir = "assets\\images".GetCommandPath();
                dir.CreateDirectoryIfNotExists();
                var fileName = dir.GetUniqueImageRandomFileName();

                dlg.FileName.FileCopy(dir.Combine(fileName + dlg.FileName.GetExtension()));
                dlg.FileName.FileMove(dlg.FileName.GetDirectoryName().Combine(fileName + dlg.FileName.GetExtension()));
                textBox.SelectedText = textBox.SelectedText.FormatImage("../images/" + fileName + dlg.FileName.GetExtension()) + "   " + fileName + dlg.FileName.GetExtension();
            }
        }

        private void 文件到数组ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnFile((path) =>
            {
                textBox.Text = "byte[] buffer=new byte[]{" + path.ReadAllBytes().Select(i => i.ToString()).ToLine(",") + "};";
            });
        }

        private void StringBuilderButton_ButtonClick(object sender, EventArgs e)
        {
            HelperApplication.ClipboardAction((path) =>
            {
                return path.ConvertToStringBuilder();
            });
        }

        private void HTMLS_Click(object sender, EventArgs e)
        {


            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\u003C!doctype html\u003E");
            sb.AppendLine("\u003Chtml class=\u0022no-js\u0022 lang=\u0022zh-hans\u0022 dir=\u0022ltr\u0022\u003E");
            sb.AppendLine("");
            sb.AppendLine("\u003Chead\u003E");
            sb.AppendLine("    \u003Cmeta charset=\u0022utf-8\u0022\u003E");
            sb.AppendLine("    \u003Cmeta http-equiv=\u0022x-ua-compatible\u0022 content=\u0022ie=edge\u0022\u003E");
            sb.AppendLine("    \u003Ctitle\u003E");
            sb.AppendLine(HtmlEncoder.Default.Encode(textBox.Text.GetFirstReadable().TrimStart("# ".ToCharArray())));
            sb.AppendLine("    \u003C/title\u003E");
            sb.AppendLine("    \u003Cmeta name=\u0022viewport\u0022 content=\u0022width=device-width, initial-scale=1\u0022\u003E");
            sb.AppendLine("    \u003Clink rel=\u0022stylesheet\u0022 href=\u0022../stylesheets/markdown.css\u0022\u003E");
            sb.AppendLine("\u003C/head\u003E");
            sb.AppendLine("\u003Cbody\u003E");
            sb.AppendLine(textBox.Text.FormatMarkdown());

            sb.AppendLine("\u003C/body\u003E");
            sb.AppendLine("\u003C/html\u003E");
            var fileName = @"assets\htmls".GetCommandPath().Combine(textBox.Text.GetFirstReadable().TrimStart('#').TrimStart().GetValidFileName('-') + ".htm");
            fileName.WriteAllText(sb.ToString());

            System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe", $"\"{ fileName}\"");
        }

        private void 压缩CSharp目录加密ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnDirectoy((dir) =>
            {
                var dstDir = @"c:\psycho\.RAR";
                dstDir.CreateDirectoryIfNotExists();
                HelperZip.CompressCSharpDirectoryEncrypt(dir, dstDir);
            });
        }

        private void UL_Click(object sender, EventArgs e)
        {
            textBox.SelectedText = textBox.SelectedText.FormatUl();
        }

        private void mainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            "settings.txt".GetCommandPath().WriteAllText(databaseBox.Text);
        }

        private void mainForm_Load(object sender, EventArgs e)
        {
            if ("settings.txt".GetCommandPath().FileExists())
            {
                var value = "settings.txt".GetCommandPath().ReadAllText();
                if (value.IsReadable())
                {
                    databaseBox.SelectedItem = value.Trim();
                }
            }
        }

        private void Markdowns_Click(object sender, EventArgs e)
        {
            textBox.SelectedText = new HelperHtmToMarkdown().Parse(Clipboard.GetText()) + Environment.NewLine + Environment.NewLine + Environment.NewLine;
        }

        private void 粘贴代码ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            var replacer = JavaScriptEncoder.Default.Encode("`");

            textBox.SelectedText = $"{textBox.SelectedText}\r\n```\r\n{Clipboard.GetText().Replace("`", replacer)}\r\n```\r\n";
        }

        private void 格式化换行符ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.Text = Regex.Replace(textBox.Text, "(?<!\r)\n", "\r\n");
        }

        private void textBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\t')
            {
                e.Handled = true;

                textBox.SelectedText = textBox.SelectedText.FormatTab();
            }
        }

        private void 执行命令无窗口ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!textBox.SelectedText.IsReadable())
            {
                textBox.SelectLine(true);
            }
            HelperApplication.ExecuteCommand(textBox.SelectedText.Trim(), false);

        }

        private void 格式化成一行ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardAction((path) =>
            {
                return Regex.Replace(path, "[\r\n]+", "");

            });
        }

        private void 逃逸JavaScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardAction((path) =>
            {
                return $"{JavaScriptEncoder.Default.Encode(Regex.Replace(path.Trim(), "[\r\n]+", ""))}";

            });
        }

        private void 导入服务器ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "*.json|*.json";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var r = HelperWebServer.GetInstance().ImportAll(dlg.FileName);
                MessageBox.Show(r);
            }
        }

        private void 逃逸JavaScript数组ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardAction((path) =>
            {
                return path.ConvertToJavaScriptArray();

            });
        }

        private void 字符串到数组ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardAction((path) =>
            {
                return string.Join(",", FileExtensions.sUTF8Encoding.GetBytes(path).Select(i => i.ToString()));

            });
        }

        private void 排序ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.SelectedText = string.Join(Environment.NewLine, textBox.SelectedText.Split('\n').Where(i => i.IsReadable()).Select(i => i.Trim()).Distinct().OrderBy(i => i));
        }

        private void 刷选行正则表达式ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (findBox.Text.IsReadable())
                textBox.Text = textBox.Text.ToLines().Where(i => Regex.IsMatch(i, findBox.Text)).Distinct((i) =>
                {

                    return Regex.Match(i, "[sS][0-9]+[eE][0-9]+").Value;
                }).ToLine();
        }

        private void 插入元数据ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var (title, head, content) = textBox.Text.FormatArticle();
            var sb = new StringBuilder();
            sb.AppendLine("---")
                .AppendLine("id:")
                .AppendLine("image:BynIKNWF.gif")
                .AppendLine("tags:外公是个老中医")

                .AppendLine("---");
            textBox.Text = title + Environment.NewLine + sb.ToString() + content;
        }



        private void 随机字符串ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(12.GetRandomString());
        }

        private void 插入新文档ToolStripMenuItem_Click(object sender, EventArgs e)
        {


            var (title, head, content) = textBox.Text.FormatArticle();

            if (title.IsVacuum() || head.IsVacuum())
            {
                return;
            }
            var image = string.Empty;
            var imageMatch = Regex.Match(head, "^image:([^\n]*?)\n", RegexOptions.Multiline);
            if (imageMatch.Success)
            {
                image = imageMatch.Groups[1].Value.Trim();
            }
            var tags = new List<string>();
            var tagsMatch = Regex.Match(head, "^tags:([^\n]*?)\n", RegexOptions.Multiline);
            if (tagsMatch.Success)
            {

                var value = tagsMatch.Groups[1].Value;
                if (value.IsReadable())
                {
                    var splited = value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim());

                    tags.AddRange(splited);
                }
            }
            var obj = new Dictionary<string, dynamic>();

            obj.Add("title", title.TrimStart("# ".ToCharArray()).Trim());
            obj.Add("content", (title + content).FormatMarkdown());
            if (image.IsReadable())
                obj.Add("image", image);
            obj.Add("password", "izm9ZVf_6KCW");

            if (tags.Count > 0)
            {
                obj.Add("tags", tags);
            }
            var message = HelperWebServer.GetInstance().Insert(obj);

            MessageBox.Show(message);


            //var token = tokenTextBox.Text;

            //if (token.IsVacuum())
            //{
            //    return;
            //}

            //var (title, head, content) = textBox.Text.FormatArticle();

            //if (title.IsVacuum() || head.IsVacuum())
            //{
            //    return;
            //}
            //var image = string.Empty;
            //var imageMatch = Regex.Match(head, "^image:([^\n]*?)\n", RegexOptions.Multiline);
            //if (imageMatch.Success)
            //{
            //    image = imageMatch.Groups[1].Value.Trim();
            //}

            //var obj = new Dictionary<string, dynamic>();

            //obj.Add("title", title.TrimStart("# ".ToCharArray()));
            //obj.Add("content", (title + content).FormatMarkdown());
            //if (image.IsReadable())
            //    obj.Add("image", image);

            //var message = HelperWebServer.GetInstance().Insert(token, obj);

            //MessageBox.Show(message);
        }

        private void 删除文档ToolStripMenuItem_Click(object sender, EventArgs e)
        {


            var (title, head, content) = textBox.Text.FormatArticle();

            if (head.IsVacuum())
            {
                return;
            }
            var id = string.Empty;
            var idMatch = Regex.Match(head, "^id:([^\n]*?)\n", RegexOptions.Multiline);
            if (idMatch.Success)
            {
                id = idMatch.Groups[1].Value.Trim();
            }

            var obj = new Dictionary<string, dynamic>();
            if (id.IsVacuum()) return;
            obj.Add("id", id);

            //var message = HelperWebServer.GetInstance().Delete(token, obj);

            //MessageBox.Show(message);
        }

        private void 更新文档ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //var token = tokenTextBox.Text;

            //if (token.IsVacuum())
            //{
            //    return;
            //}

            var (title, head, content) = textBox.Text.FormatArticle();

            if (head.IsVacuum())
            {
                return;
            }
            var id = string.Empty;
            var idMatch = Regex.Match(head, "^id:([^\n]*?)\n", RegexOptions.Multiline);
            if (idMatch.Success)
            {
                id = idMatch.Groups[1].Value.Trim();
            }
            if (id.IsVacuum()) return;

            var obj = new Dictionary<string, dynamic>();
            obj.Add("id", id);

            obj.Add("title", title.TrimStart("# ".ToCharArray()));
            obj.Add("content", (title + content).FormatMarkdown());

            var image = string.Empty;
            var imageMatch = Regex.Match(head, "^image:([^\n]*?)\n", RegexOptions.Multiline);
            if (imageMatch.Success)
            {
                image = imageMatch.Groups[1].Value.Trim();
            }




            if (image.IsReadable())
                obj.Add("image", image);
            var tags = new List<string>();
            var tagsMatch = Regex.Match(head, "^tags:([^\n]*?)\n", RegexOptions.Multiline);
            if (tagsMatch.Success)
            {

                var value = tagsMatch.Groups[1].Value;
                if (value.IsReadable())
                {
                    var splited = value.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(i => i.Trim());

                    tags.AddRange(splited);
                }
            }
            obj.Add("password", "izm9ZVf_6KCW");
            if (tags.Count > 0)
            {
                obj.Add("tags", tags);
            }
            var message = HelperWebServer.GetInstance().Update(obj);

            MessageBox.Show(message);
        }

        private void 排序ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardAction((value) =>
            {
                var ls = value.ConvertToBlocks().OrderBy(i => Regex.Match(i, " [^\\(]*?\\(").Value);
                return string.Join(Environment.NewLine, ls);
            });
        }

        private void 格式化_ButtonClick(object sender, EventArgs e)
        {
            textBox.Format();
            //textBox.Text = WebUtility.HtmlDecode(textBox.Text);
        }

        private void 顶端_Click(object sender, EventArgs e)
        {
            textBox.SelectionStart = 0;
            textBox.ScrollToCaret();
        }

        private void 底端_Click(object sender, EventArgs e)
        {
            textBox.SelectionStart = textBox.Text.IsVacuum() ? 0 : textBox.Text.Length - 1;
            textBox.ScrollToCaret();
        }

        private void 复制表格ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.SelectTableCell();
            textBox.Copy();
        }

        private void 文件SHA1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnFile((path) =>
            {
                Clipboard.SetText(path.GetFileSha1());
            });
        }

        private void 压缩JavaScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnFile((path) =>
            {

                path.ChangeFileName(path.GetFileNameWithoutExtension() + ".min").WriteAllText(NUglify.Uglify.Js(path.ReadAllText()).Code);
            });
        }

        private void javaScript模板ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardAction((value) =>
            {
                var ls = NUglify.Uglify.Html(value).Code.ConvertToJavaScriptTemplate();
                return string.Join("," + Environment.NewLine, ls.Select(i => "\"" + JavaScriptEncoder.Default.Encode(i) + "\""));
            });
        }

        private void 压缩HTMToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardAction((value) =>
            {

                return NUglify.Uglify.Html(value).Code;
            });
        }

        private void 创建文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardAction((path) =>
            {
                HelpersSafari.CreateDirectory(path);
                return "";

            });

        }

        private void 创建目录ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            HelpersSafari.CreateTableContents();
        }

        private void 离线文档ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var dir = dlg.FileName.GetDirectoryName();

                var ls = System.IO.Directory.GetDirectories(dir);
                foreach (var item in ls)
                {
                    HelpersSafari.ProcessForOffline(item);
                }
            }
        }

        private void 转换成HTM文档ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var dir = dlg.FileName.GetDirectoryName();

                var ls = System.IO.Directory.GetDirectories(dir);
                foreach (var item in ls)
                {
                    HelpersSafari.FormatHTML(item);
                }
            }
        }

        private void 下载图片ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new OpenFileDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var dir = dlg.FileName.GetDirectoryName();

                var ls = System.IO.Directory.GetDirectories(dir);
                foreach (var item in ls)
                {
                    HelpersSafari.DoExtractImages(item);
                }
            }
        }


        private void 生成未下载文件列表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //var dlg = new OpenFileDialog();
            //if (dlg.ShowDialog() == DialogResult.OK)
            //{
            //    var dir = dlg.FileName.GetDirectoryName();

            //    HelpersSafari.GenerateUnDownloadFileListFile(dir, dlg.FileName);
            //}
            HelperApplication.ClipboardActionOnDirectoy((path) =>
            {
                var ls = System.IO.Directory.GetDirectories(path);

                foreach (var item in ls)
                {


                    HelpersSafari.GenerateUnDownloadFileListFile(item, item.Combine("links.txt"));

                }
            });
        }

        private void 压缩子目录加密ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnDirectoy((dir) =>
            {

                HelperZip.CompressDirectoriesEncrypt(dir, dir);
            });
        }

        private void H3_Click(object sender, EventArgs e)
        {
            var start = textBox.SelectionStart;

            while (start - 1 > -1 && textBox.Text[start - 1] != '\n')
            {
                start--;
            }
            var end = start;
            while (end + 1 < textBox.Text.Length && textBox.Text[end + 1] == '#')
            {
                end++;
            }
            textBox.SelectionStart = start;
            textBox.SelectionLength = end - start;
            textBox.SelectedText = "### ";
        }

        private void H2_Click(object sender, EventArgs e)
        {
            var start = textBox.SelectionStart;

            while (start - 1 > -1 && textBox.Text[start - 1] != '\n')
            {
                start--;
            }
            var end = start;
            while (end + 1 < textBox.Text.Length && textBox.Text[end + 1] == '#')
            {
                end++;
            }
            textBox.SelectionStart = start;
            textBox.SelectionLength = end - start;
            textBox.SelectedText = "## ";
        }

        private void 目录_Click(object sender, EventArgs e)
        {
            var matches = textBox.Text.MatchesMultiline("^#{2,3} [^\n]*?\n");
            var count = 0;
            var sb = new StringBuilder();
            sb.AppendLine().AppendLine("<nav class=\"article-nav\">").AppendLine();
            sb.AppendLine("- [目录](#top_of_page){article-nav-head}");
            foreach (var item in matches)
            {
                var (head, tail) = item.SplitTwo(' ');
                tail = Regex.Replace(tail, "\\[([^\\]]*?)\\]\\(.+\\)", "$1");
                sb.Append(' ', (item.CountStart('#') - 2) * 2).Append("- [").Append(tail.Trim()).Append("](").Append("#section-" + (++count)).Append(")").AppendLine();
            }
            sb.AppendLine().AppendLine("</nav>").AppendLine();
            textBox.SelectedText = sb.ToString();
        }

        private void 粘贴格式_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardAction((value) =>
            {

                textBox.SelectedText = new HelperHtmToMarkdown().Parse(value);

                //var doc = new HtmlAgilityPack.HtmlDocument();
                //doc.LoadHtml(value);

                //var children = doc.DocumentNode.Descendants();
                //var sb = new StringBuilder();
                //foreach (var item in children)
                //{
                //    if (item.Name == "h1")
                //    {
                //        sb.AppendLine("## " + item.InnerText.Trim()).AppendLine();

                //    }
                //    if (item.Name == "h2")
                //    {
                //        sb.AppendLine("### " + item.InnerText.Trim()).AppendLine();
                //    }
                //    else if (item.Name == "pre")
                //    {
                //        sb.Append(item.InnerText.FormatCode()).AppendLine();
                //    }
                //}
                //textBox.SelectedText = sb.ToString();
                return "";
            });
        }

        private void 压缩子目录ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnDirectoy((dir) =>
            {

                HelperZip.CompressDirectories(dir, dir);
            });
        }

        private void qRDecoderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            HelperApplication.ClipboardActionOnFile((path) =>
            {

                // create a barcode reader instance
                ZXing.IBarcodeReader reader = new ZXing.BarcodeReader();
                // load a bitmap
                var barcodeBitmap = (Bitmap)Bitmap.FromFile(path);
                // detect and decode the barcode inside the bitmap
                var result = reader.Decode(barcodeBitmap);
                // do something with the result
                if (result != null)
                {
                    textBox.SelectedText = result.BarcodeFormat.ToString() + Environment.NewLine + result.Text;

                }
            });*/
        }

        private void 压缩单个文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnFile((dir) =>
            {

                HelperZip.CompressFile(dir, @"C:\psycho\.RAR");
            });
        }

        private void 重启NginxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperSSH.ExecuteCommand("find /var/psycho/temp/cache -type f -delete && nginx -s reload");
        }

        private void 字符串到字节数组多行ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardAction((path) =>
            {
                var ls = path.Split('\n').Where(i => i.IsReadable()).Select(i => i.TrimEnd());

                var sb = new StringBuilder();

                foreach (var item in ls)
                {
                    sb.Append('{').Append(string.Join(",", FileExtensions.sUTF8Encoding.GetBytes(item).Select(i => i.ToString()))).Append("};").AppendLine();

                }
                return sb.ToString();

            });
        }

        private void 重启服务器ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperSSH.ExecuteCommand("systemctl  restart everstore.service");

        }

        private void javascriptsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnFiles((files) =>
            {
                HelperSSH.UploadFiles(files, "/var/psycho/public/javascripts");
            });
        }

        private void stylesheetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnFiles((files) =>
            {
                HelperSSH.UploadFiles(files, "/var/psycho/public/stylesheets");
            });

        }

        private void layoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnFiles((files) =>
            {
                HelperSSH.UploadFiles(files, "/root/go/_layout");
            });
        }

        private void rootgoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnFiles((files) =>
            {
                HelperSSH.UploadFiles(files, "/root/go");
            });

        }

        private void 解压EverStoreZipToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperSSH.ExecuteCommand("cd /root/go && unzip -o /root/go/EverStore.zip");

        }

        private void imagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnFiles((files) =>
            {
                HelperSSH.UploadFiles(files, "/var/psycho/public/images");
            });
        }

        private void CookBook_ButtonClick(object sender, EventArgs e)
        {
            textBox.Text = CookBook.Text + "：" + textBox.Text.TrimStart();
        }

        private void 置顶Button_Click(object sender, EventArgs e)
        {
            this.TopMost = !this.TopMost;
        }
        private void ExportDocument(string title, string content)
        {


            StringBuilder sb = new StringBuilder();
            sb.AppendLine("\u003C!doctype html\u003E");
            sb.AppendLine("\u003Chtml class=\u0022no-js\u0022 lang=\u0022zh-hans\u0022 dir=\u0022ltr\u0022\u003E");
            sb.AppendLine("");
            sb.AppendLine("\u003Chead\u003E");
            sb.AppendLine("    \u003Cmeta charset=\u0022utf-8\u0022\u003E");
            sb.AppendLine("    \u003Cmeta http-equiv=\u0022x-ua-compatible\u0022 content=\u0022ie=edge\u0022\u003E");
            sb.AppendLine("    \u003Ctitle\u003E");
            sb.AppendLine(HtmlEncoder.Default.Encode(title.TrimStart("# ".ToCharArray())));
            sb.AppendLine("    \u003C/title\u003E");
            sb.AppendLine("    \u003Cmeta name=\u0022viewport\u0022 content=\u0022width=device-width, initial-scale=1\u0022\u003E");
            sb.AppendLine("    \u003Clink rel=\u0022stylesheet\u0022 href=\u0022../stylesheets/markdown.css\u0022\u003E");
            sb.AppendLine("\u003C/head\u003E");
            sb.AppendLine("\u003Cbody\u003E");
            sb.AppendLine(content.FormatMarkdown());

            sb.AppendLine("\u003C/body\u003E");
            sb.AppendLine("\u003C/html\u003E");
            var fileName = @"assets\htmls".GetCommandPath().Combine(title.TrimStart('#').TrimStart().GetValidFileName('-') + ".htm");
            fileName.WriteAllText(sb.ToString());

            //System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe", $"\"{ fileName}\"");
        }
        private void 导出全部ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ls = HelperSqlite.GetInstance().GetTitleContentList();
            foreach (var item in ls)
            {
                ExportDocument(item.Title, item.Content);
            }
        }

        private void 查找_ButtonClick(object sender, EventArgs e)
        {
            var value = textBox.Text;
            var findValue = textBox.SelectedText.IsVacuum() ? findBox.Text : textBox.SelectedText;
            var selectedLength = findValue.Length;
            var indexStart = textBox.SelectionStart + selectedLength;
            var position = value.IndexOf(findValue, indexStart);

            if (position != -1)
            {
                textBox.SelectionStart = position;
                textBox.SelectionLength = selectedLength;
                textBox.ScrollToCaret();
            }
        }

        private void 生成未下载文件列表imagesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnDirectoy((path) =>
            {
                var ls = System.IO.Directory.GetDirectories(path);

                foreach (var item in ls)
                {
                    var dir = item.Combine("images");

                    HelpersSafari.GenerateUnDownloadFileListFile(dir, dir.Combine("img-links.txt"));

                }
            });
        }

        private void 提取链接列表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardAction((value) =>
            {
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(value);
                var sb = new StringBuilder();
                var ls = doc.DocumentNode.SelectNodes("//a");

                foreach (var item in ls)
                {
                    sb.AppendFormat("- [{0}]({1})\r\n", item.InnerText.Trim(), item.GetAttributeValue("href", ""));
                }
                textBox.SelectedText = sb.ToString();
                return "";
            });
        }

        private void H4_Click(object sender, EventArgs e)
        {
            var start = textBox.SelectionStart;

            while (start - 1 > -1 && textBox.Text[start - 1] != '\n')
            {
                start--;
            }
            var end = start;
            while (end + 1 < textBox.Text.Length && textBox.Text[end + 1] == '#')
            {
                end++;
            }
            textBox.SelectionStart = start;
            textBox.SelectionLength = end - start;
            textBox.SelectedText = "#### ";
        }

        private void programmingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.Text = programmingToolStripMenuItem.Text + "：" + textBox.Text.TrimStart();
        }

        private void 创建EpubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnDirectoy((dir) =>
            {
                var ls = System.IO.Directory.GetDirectories(dir);

                var targetDirectory = @"C:\Users\Administrator\Desktop\Safari\EPUBS";
                targetDirectory.CreateDirectoryIfNotExists();
                foreach (var item in ls)
                {
                    targetDirectory.Combine(item.GetFileName()).CreateDirectoryIfNotExists();
                    HelperEpubCreator.CreateEpub(item, targetDirectory.Combine(item.GetFileName()));
                }

            });
        }

        private void 解压子目录加密ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnDirectoy((dir) =>
            {

                HelperZip.DeCompressDirectoriesEncrypt(dir, dir);
            });
        }

        private void 压缩IntellijAndroidToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnDirectoy((dir) =>
            {
                var dstDir = @"c:\psycho\.RAR";
                dstDir.CreateDirectoryIfNotExists();
                HelperZip.CompressAndroidIntellijDirectoryEncrypt(dir, dstDir);
            });
        }

        private void 词典_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardAction((value) =>
            {

                textBox.SelectedText = HelperTranslatorMerriam.GetInstance().QueryChinese(value.Split(new char[] { ' ' }, 2).First().Trim().ToLower());
                return string.Empty;
            });
        }

        private void 导入ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardAction((value) =>
            {
                var ls = Regex.Split(value, "[\\W_]+").Where(i => i.Length > 0 && Regex.IsMatch(i, "[a-zA-Z]")).Select(i => i.ToLower()).Distinct();
                var collection = ls.Except(HelperTranslatorMerriam.GetInstance().ListAllKey()).ToArray();
                //var a = ls.ToArray();
                textBox.Text = collection.Count().ToString() + Environment.NewLine + string.Join(",", collection);
                SynchronizationContext context = SynchronizationContext.Current;
                Task.Run(() =>
                {
                    var length = collection.Length;

                    for (int iv = 0; iv < length; iv++)
                    {
                        HelperTranslatorMerriam.GetInstance().QueryChinese(collection[iv]);
                        context.Post((v) =>
                        {
                            this.Text = iv + " " + collection[iv];
                        }, null);
                    }




                    MessageBox.Show("OK");

                });
                return string.Empty;
            });
        }

        private void 检查词典数据库完整性ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SynchronizationContext context = SynchronizationContext.Current;

            Task.Run(() =>
            {


                HelperTranslatorMerriam.GetInstance().QueryEmpty(context, (v) =>
                 {
                     this.Text = v;

                 });

            });
        }

        private void 导入文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //new HelperTranslatorImporter().ImportPsychoDatabase();
            HelperApplication.ClipboardActionOnDirectoy((value) =>
            {
                var ls = new List<string>();
                var fs = System.IO.Directory.GetFiles(value, "*.txt");
                foreach (var item in fs)
                {
                    ls.AddRange(Regex.Split(item.ReadAllText(), "[\\W_]+").Where(i => i.Length > 0 && !Regex.IsMatch(i, "[0-9]")).Select(i => i.ToLower()).Distinct());

                }

                var importer = new HelperTranslatorImporter();

                var databaseList = importer.ListAllKey().ToArray();
                var collection = ls.Except(databaseList).Except("disable.txt".GetCommandPath().ReadAllText().Split("\r\n".ToArray(), StringSplitOptions.RemoveEmptyEntries)).ToArray();
                //var a = ls.ToArray();
                textBox.Text = collection.Count().ToString() + Environment.NewLine + string.Join(",", collection);
                SynchronizationContext context = SynchronizationContext.Current;
                Task.Run(async () =>
                {
                    // 
                    //var length = collection.Length;

                    //for (int iv = 0; iv < length; iv++)
                    //{
                    //    try
                    //    {
                    //        await importer.ImportEnglish(collection[iv]);
                    //    }
                    //    catch
                    //    {

                    //    }
                    //    context.Post((v) =>
                    //    {
                    //        if (iv < length)
                    //            this.Text = iv + " " + collection[iv];
                    //    }, null);
                    //}


                    await importer.Query(collection, (i) =>
                     {
                         context.Post((v) =>
                         {

                             this.Text = i.ToString();
                         }, null);
                     });


                    importer.Close();
                    MessageBox.Show("OK");

                });

            });
        }

        private void 字符串到数组简单ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardAction((path) =>
            {
                var sb = new StringBuilder();

                sb.AppendLine("new string[]{");

                var ls = path.Split('\n');
                foreach (var item in ls)
                {
                    if (item.IsReadable())
                    {
                        sb.AppendLine("\"" + item.TrimEnd().Replace("\"", "\\\"") + "\",");

                    }
                }
                sb.AppendLine("}");
                return sb.ToString();
            });
        }

        private void 转化为Kindle字典ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new HelperTranslatorToKindle().ExportEnglish("Dictionary", "psycho.db");
        }

        private void 导入文件夹MerriamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnDirectoy((value) =>
            {
                var ls = new List<string>();
                var fs = System.IO.Directory.GetFiles(value, "*.txt");
                foreach (var item in fs)
                {
                    ls.AddRange(Regex.Split(item.ReadAllText(), "[\\W_]+").Where(i => i.Length > 0 && !Regex.IsMatch(i, "[0-9]")).Select(i => i.ToLower()).Distinct());

                }
                var databaseList = HelperTranslatorMerriam.GetInstance().ListAllKey().ToArray();
                var collection = ls.Except(databaseList).ToArray();
                //var a = ls.ToArray();
                textBox.Text = collection.Count().ToString() + Environment.NewLine + string.Join(",", collection);
                SynchronizationContext context = SynchronizationContext.Current;
                Task.Run(() =>
                {
                    var length = collection.Length;

                    for (int iv = 0; iv < length; iv++)
                    {
                        try
                        {
                            HelperTranslatorMerriam.GetInstance().QueryChinese(collection[iv]);
                        }
                        catch
                        {

                        }
                        context.Post((v) =>
                        {
                            if (iv < length)
                                this.Text = iv + " " + collection[iv];
                        }, null);
                    }



                    HelperTranslatorMerriam.GetInstance().Close();
                    MessageBox.Show("OK");

                });

            });
        }

        private void 转化为Kindle字典MerriamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new HelperTranslatorToKindle().ExportEnglish();
        }

        private void 整理Epub文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnDirectoy((value) =>
            {
                var epubFiles = System.IO.Directory.GetFiles(value, "*.epub", System.IO.SearchOption.AllDirectories).Where(i => !i.Contains(".EPUB"));
                var targetDirectory = value;
                foreach (var item in epubFiles)
                {
                    HelperEpubRename.ReNameEpub(item, targetDirectory);
                }

            });
        }

        private void 标题2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardAction((v) =>
            {
                textBox.SelectedText += $"\r\n\r\n## {v.Trim()}\r\n\r\n";
                return "";
            });
        }

        private void 粘贴为代码块ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            var replacer = JavaScriptEncoder.Default.Encode("`");

            textBox.SelectedText = $"{textBox.SelectedText}\r\n```\r\n{Clipboard.GetText().Replace("`", replacer)}\r\n```\r\n";
        }

        private void 格式化C代码ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardAction((v) =>
            {

                return CodeGenerator.FormatCSharpCode(v);
            });
        }
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                var strArray = Clipboard.GetText().Split('\n').Select(i => i.Trim());
                var sb = new StringBuilder();

                foreach (var item in strArray)
                {
                    if (item.IsReadable())
                    {
                        var endCharacter = item[item.Length - 1];
                        if (".!?\"".Contains(endCharacter))
                            sb.Append(item).Append("\r\n\r\n");
                        else
                            sb.Append(item).Append(' ');

                    }
                }

                textBox.SelectedText = sb.Append("\r\n\r\n\r\n").ToString();
                // textBox.SelectedText = Environment.NewLine + Regex.Replace(textBox.SelectedText, "[\r\n]+", " ").Trim() + Environment.NewLine;

                return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
        private void pdfToTextToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnFile((f) =>
            {

                if (f.EndsWith(".pdf"))
                {
                    var str = HelperPdf.ExtractTextFromPdf(f);
                    f.ChangeExtension("txt").WriteAllText(str);
                }
            });
        }

        private async void toolStripButton1_Click(object sender, EventArgs e)
        {
            await HelperTranslatorImporter.QueryOnBing("good", new System.Net.Http.HttpClient(), new HtmlAgilityPack.HtmlDocument());
        }

        private void magnetAria2cButton_Click(object sender, EventArgs e)
        {
            try
            {

                var dir = "C:\\psycho\\aria2c";
                dir.CreateDirectoryIfNotExists();
                Process.Start(new ProcessStartInfo
                {
                    WorkingDirectory = dir,
                    FileName = "aria2c",
                    Arguments = "\"" + Clipboard.GetText() + "\""
                });

            }
            catch (Exception exception)
            {

                MessageBox.Show(exception.Message);
            }
        }

        private void magnetToTorrentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {

                var magnet = Clipboard.GetText().Trim();

                if (!magnet.StartsWith("magnet:"))
                {
                    return;

                }
                var dir = "C:\\psycho\\aria2c";
                dir.CreateDirectoryIfNotExists();

                Process.Start(new ProcessStartInfo
                {
                    WorkingDirectory = dir,
                    FileName = "aria2c",
                    Arguments = "--bt-metadata-only=true --bt-save-metadata=true \"" + magnet + "\""
                });

            }
            catch (Exception exception)
            {

                MessageBox.Show(exception.Message);
            }
        }

        private void 整理Mobi文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnDirectoy((value) =>
            {
                HelperMobi.ReName(value);
            });
        }

        private void codeSplitButton_ButtonClick(object sender, EventArgs e)
        {
            HelperApplication.ClipboardAction((value) =>
            {
                var sb = new StringBuilder();
                var cacheSb = new StringBuilder();

                sb.Append("/*\r\n\r\n");
                foreach (var item in value.Split(new char[] { '\n' }))
                {
                    if (item.IsReadable())
                    {
                        foreach (var l in item.Split(' '))
                        {
                            if (l.IsReadable())
                            {

                                cacheSb.Append(l.Trim()).Append(' ');
                                if (cacheSb.Length > 50)
                                {
                                    sb.Append(cacheSb).AppendLine();
                                    cacheSb.Clear();
                                }
                            }
                        }
                        if (cacheSb.Length > 0)
                        {
                            sb.Append(cacheSb).AppendLine().AppendLine();
                            cacheSb.Clear();
                        }

                    }
                }
                sb.Append("*/\r\n");
                return sb.ToString();
            });
        }

        private void 导入文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HelperApplication.ClipboardActionOnFile((path) =>
            {

                var content = HelperMarkdownFormat.FormatCode(path.ReadAllText());

                var title = $"文件：{path.GetFileName()}";
                var article = new Article
                {
                    Title = title,
                    Content = title + Environment.NewLine + Environment.NewLine + content,
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow,
                };
                Article tmp = null;
                if ((tmp = HelperSqlite.GetInstance().GetArticle(title)) != null)
                {
                    article.Id = tmp.Id;
                    HelperSqlite.GetInstance().Update(article);
                    return;
                }
                HelperSqlite.GetInstance().Insert(article);
                UpdateList();
            });
        }

        private void 导出文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listBox.SelectedIndex == -1) return;
            var title = listBox.SelectedItem.ToString();
            var article = HelperSqlite.GetInstance().GetArticle(title);
            var pos = article.Content.IndexOf("```");
            if (pos > -1)
            {

                var fileName = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\" + article.Title.Split('：').Last();

                fileName.WriteAllText(article.Content.Substring(pos + 3).Trim().TrimEnd('`'));
            }
        }

        private void formatButton_Click(object sender, EventArgs e)
        {
            textBox.SelectedText = string.Join(Environment.NewLine + Environment.NewLine, Clipboard.GetText().Split('\n').Where(i => i.IsReadable()).Select(i => i.TrimEnd()))+ Environment.NewLine + Environment.NewLine;
        }

        private void h1Button_Click(object sender, EventArgs e)
        {

            var start = textBox.SelectionStart;

            while (start - 1 > -1 && textBox.Text[start - 1] != '\n')
            {
                start--;
            }
            var end = start;
            while (end + 1 < textBox.Text.Length && textBox.Text[end + 1] == '#')
            {
                end++;
            }
            textBox.SelectionStart = start;
            textBox.SelectionLength = end - start;
            textBox.SelectedText = "# ";
        }

        private void 格式化ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            textBox.SelectedText = Regex.Replace(textBox.SelectedText, "[\r\n]+", "").Trim() + Environment.NewLine + Environment.NewLine;
        }
        #region 
        private void 替换所选内容ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (findBox.Text.IsReadable() && textBox.SelectedText.IsReadable())
            {
                textBox.SelectedText = Regex.Replace(textBox.SelectedText, findBox.Text, replaceBox.Text.Replace("\\n", Environment.NewLine));
            }
        }
        #endregion

        private void 剪切ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (textBox.SelectedText.IsVacuum())
            {
                textBox.SelectLine(true);
            }
            textBox.Cut();
        }
    }
}
