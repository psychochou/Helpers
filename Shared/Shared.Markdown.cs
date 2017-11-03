using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Markdig;
using HtmlAgilityPack;
using System.Text.RegularExpressions;

namespace Shared
{
    static class HelperMarkdownFormat
    {
        private static MarkdownPipeline _sMarkdownPipeline;

        public static MarkdownPipeline GetMarkdownPipeline() => _sMarkdownPipeline ?? (_sMarkdownPipeline = new MarkdownPipelineBuilder().UsePipeTables().UseCustomContainers().UseEmphasisExtras().UseAutoLinks().UseGenericAttributes().Build());
        public static string FormatMarkdown(this string value)
        {
            var retVal = Markdown.ToHtml(value, GetMarkdownPipeline());
            var index = 0;
            retVal = Regex.Replace(retVal, "<(h[23])[^>]*?>", new MatchEvaluator((m) =>
            {

                return $"<{m.Groups[1].Value} id=\"section-{(++index)}\">";

            }));
            return retVal;
        }
        public static string FormatHeading(this string value)
        {
            if (value.TrimStart().StartsWith("#"))
            {
                return $"#{value.TrimStart()}";
            }
            return $"# {value.TrimStart()}";

        }

        public static string FormatHr(this string value)
        {
            return $"{value}{Environment.NewLine}***";
        }
        public static string FormatStrong(this string value)
        {
            return $" **{value.Trim()}** ";
        }
        public static string FormatEm(this string value)
        {
            return $" *{value.Trim()}* ";
        }
        public static string FormatCode(this string value)
        {
            if (value.Contains('\n'))
            {
                return $"{Environment.NewLine}```{Environment.NewLine}{WebUtility.HtmlDecode(value.Trim())}{Environment.NewLine}```{Environment.NewLine}";
            }
            return $" `{WebUtility.HtmlDecode(value.Trim())}` ";
        }
        public static string FormatImage(this string value, string src)
        {

            return $"![{value}]({src})";

        }
        public static string FormatLink(this string value, string src)
        {

            return $"[{value}]({src})";

        }
        public static string FormatUl(this string value)
        {
            var ls = value.ToLines();
            var sb = new StringBuilder();
            foreach (var item in ls)
            {

                sb.AppendLine($"- {item.TrimStart(new char[] { ' ', '-' })}");

            }
            return sb.ToString();
        }
        public static string FormatHtml(this string value)
        {
            return Regex.Replace(value, "<([a-zA-Z0-9]+)\\s [^>]*?>", "<$1>");
        }
        public static string FormatTab(this string value)
        {
            if (value.IsVacuum())
                return "    ";
            return string.Join(Environment.NewLine, value.Split('\n').Select(i => "    " + i.TrimEnd()));
        }

        public static (string, string, string) FormatArticle(this string value)
        {
            var p = value.IndexOf("---\r\n");
            if (p > -1)
            {
                var start = p;
                p = value.IndexOf("---\r\n", p + 5);
                if (p > -1)
                    return (value.Substring(0, start), value.Substring(start, p + 5 - start), value.Substring(p + 5));
            }
            var splited = value.Trim().Split(new char[] { '\n' }, 2);
            if (splited.Count() == 1)
                return (splited[0].TrimEnd(), string.Empty, splited[0].TrimEnd());

            return (splited[0].TrimEnd(), string.Empty, splited[1].TrimEnd());

        }
    }
    class HelperHtmToMarkdown
    {

        private readonly string[] _block = new[] { "div", "p" };
        public HelperHtmToMarkdown()
        {

        }
        private static string FormatForInline(string value)
        {
            return WebUtility.HtmlDecode(Regex.Replace(Regex.Replace(value, "[\r\n]+", " "), "\\s{2,}", " "));
        }
        private void ParseNode(HtmlNode n, StringBuilder sb)
        {
            if (n.NodeType == HtmlNodeType.Text)
            {
                sb.Append(FormatForInline(n.InnerText));
                return;
            }
            var nn = n.Name;

            if (nn == "body" || nn == "div" || nn == "p" || nn == "ul" || nn == "ol" || nn == "span" || nn == "article"||nn=="section" || nn=="aside")
            {
                if (nn != "span")
                    sb.AppendLine().AppendLine();
                foreach (var item in n.ChildNodes)
                {
                    ParseNode(item, sb);
                }
            }
            else if (nn == "pre")
            {
                sb.AppendLine()
                    .Append('`', 3)
                    .AppendLine()
                    .AppendLine(WebUtility.HtmlDecode(n.InnerText).Trim())
                    .Append('`', 3)
                    .AppendLine()
                    ;
            }
            else if (nn.StartsWith("h"))
            {
                sb.Append('#', nn.ConvertToInt(0)+1).Append(' ');
                if (n.ChildNodes.Count == 1 && n.NodeType == HtmlNodeType.Text)
                {
                    sb.Append(FormatForInline(n.InnerText));

                    return;
                }
                foreach (var item in n.ChildNodes)
                {
                    ParseNode(item, sb);
                }
            }
            else if (nn == "code")
            {
                if (n.InnerText.Contains("`"))
                {
                    sb.Append(n.OuterHtml);
                    return;
                }
                sb.Append(" `" + WebUtility.HtmlDecode(n.InnerText).Trim() + "` ");
            }
            else if (nn == "strong" || nn == "b")
            {
                sb.Append(" **" + FormatForInline(n.InnerText) + "** ");
            }
            else if (nn == "em" || nn == "i")
            {
                sb.Append(" *" + WebUtility.HtmlDecode(n.InnerText).Trim() + "* ");
            }
            else if (nn == "sub")
            {
                sb.Append(" ~" + WebUtility.HtmlDecode(n.InnerText).Trim() + "~ ");
            }
            else if (nn == "sup")
            {
                sb.Append(" ^" + WebUtility.HtmlDecode(n.InnerText).Trim() + "^ ");
            }
            else if (nn == "mark")
            {
                sb.Append(" ==" + WebUtility.HtmlDecode(n.InnerText).Trim() + "== ");
            }
            else if (nn == "ins")
            {
                sb.Append(" ++" + WebUtility.HtmlDecode(n.InnerText).Trim() + "++ ");
            }
            else if (nn == "del")
            {
                sb.Append(" ~~" + WebUtility.HtmlDecode(n.InnerText).Trim() + "~~ ");
            }
            else if (nn == "br")
            {
                sb.AppendLine();
            }
            else if (nn == "a")
            {
                var str = n.GetAttributeValue("href", "");
                if (!str.IsVacuum())
                {
                    sb.Append($"[{WebUtility.HtmlDecode(n.InnerText)}]({str}) ");
                }
            }
            else if (nn == "img")
            {
                var str = n.GetAttributeValue("src", "");
                if (!str.IsVacuum())
                {
                    sb.Append($"![{n.GetAttributeValue("alt","")}]({str}) ");
                }
            }
            else if (nn == "li")
            {
                sb.AppendLine();
                sb.Append("- ");
                if(n.ChildNodes.Count==1&& n.FirstChild.Name == "p")
                {
                    sb.Append(FormatForInline(n.InnerText));
                }
                else
                {
                    foreach (var item in n.ChildNodes)
                    {
                        ParseNode(item, sb);
                    }
                }
               
            }
            else
            {

                sb.AppendLine(n.OuterHtml);
            }
        }
        public string Parse(string value)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(value);

            var children = doc.DocumentNode.ChildNodes;
            var sb = new StringBuilder();
            foreach (var item in children)
            {
                ParseNode(item, sb);

            }
            return sb.ToString();
        }
    }
}
