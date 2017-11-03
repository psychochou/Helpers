using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Shared
{
    public static class CodeGenerator
    {





        public static string FormatCSharpCode(string value)
        {

            var s = new StringBuilder();

            var rootNode = CSharpSyntaxTree.ParseText(value).GetRoot();

            var namespace_ = rootNode.DescendantNodes().OfType<NamespaceDeclarationSyntax>();

            if (namespace_.Any())
            {

                s.Append(namespace_.First().NamespaceKeyword.Text).Append(' ').Append(namespace_.First().Name).Append('{');
            }

            var using_ = rootNode.DescendantNodes().OfType<UsingDirectiveSyntax>();
            if (using_.Any())
            {

                using_ = using_.OrderBy(i => i.Name.ToString());//.Distinct(i => i.Name.GetText());

                foreach (var item in using_)
                {
                    s.Append(item.ToFullString());
                }
            }
            var enum_ = rootNode.DescendantNodes().OfType<EnumDeclarationSyntax>();
            if (enum_.Any())
            {
                foreach (var item in enum_)
                {
                    enum_ = enum_.OrderBy(i => i.Identifier.ToFullString());
                    s.Append(item.ToFullString());
                }
            }
            var struct_ = rootNode.DescendantNodes().OfType<StructDeclarationSyntax>();
            if (struct_.Any())
            {
                foreach (var item in struct_)
                {
                    struct_ = struct_.OrderBy(i => i.Identifier.ToFullString());
                    s.Append(item.ToFullString());
                }
            }
            var class_ = rootNode.DescendantNodes().OfType<ClassDeclarationSyntax>();

            if (class_.Any())
            {
                class_ = class_.OrderBy(i => i.Identifier.ValueText);

                foreach (var item in class_)
                {
                    s.Append(item.Modifiers.ToFullString()).Append(" class ").Append(item.Identifier.ValueText).Append('{');
                    var field_ = item.DescendantNodes().OfType<FieldDeclarationSyntax>();
                    if (field_.Any())
                    {
                        field_ = field_.OrderBy(i => i.Declaration.Variables.First().ToFullString());

                        foreach (var itemField in field_)
                        {

                            s.Append(itemField.ToFullString().Trim() + '\n');
                        }
                    }

                    var constructor_ = item.DescendantNodes().OfType<ConstructorDeclarationSyntax>();
                    if (constructor_.Any())
                    {
                        constructor_ = constructor_.OrderBy(i => i.Identifier.ValueText);//.OrderBy(i => i.Identifier.ValueText).ThenBy(i=>i.Modifiers.ToFullString());
                        foreach (var itemMethod in constructor_)
                        {


                            s.Append(itemMethod.ToFullString());
                        }

                    }
                    var method_ = item.DescendantNodes().OfType<MethodDeclarationSyntax>();

                    if (method_.Any())
                    {
                        method_ = method_.OrderBy(i => i.Modifiers.ToFullString().Trim() + i.Identifier.ValueText.Trim());//.OrderBy(i => i.Identifier.ValueText).ThenBy(i=>i.Modifiers.ToFullString());
                        foreach (var itemMethod in method_)
                        {


                            s.Append(itemMethod.ToFullString());
                        }

                    }
                    s.Append('}');
                }

            }
            s.Append('}');
            return s.ToString();

        }

        public static IEnumerable<string> ConvertToJavaScriptTemplate(this string value)
        {
            var start = 0;
            var offset = 0;
            var ls = new List<string>();
            var count = 0;
            while ((offset = value.IndexOf("{{", start)) != -1)
            {
                ls.Add(value.Substring(start, offset - start));
                count++;
                start = offset;
                offset = value.IndexOf("}}", offset);
                ls.Add(count + ":" + value.Substring(start, offset - start + 2));
                start = offset + 2;
                count++;
            }

            if (start + 1 < value.Length)
            {
                ls.Add(value.Substring(start));
            }
            return ls;
        }
        public static IEnumerable<string> ConvertToBlocks(this string value)
        {
            var ls = new List<string>();
            var sb = new StringBuilder();
            var count = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '{')
                {
                    count++;
                }
                else if (value[i] == '}')
                {
                    count--;

                    if (count == 0)
                    {
                        sb.Append(value[i]);
                        ls.Add(sb.ToString());
                        sb.Clear();
                        continue;
                    }
                }
                sb.Append(value[i]);
            }

            ls.Add(sb.ToString());
            return ls;
        }

        public static string ConvertToJavaScriptArray(this string value)
        {

            var ls = value.Split('\n').Select(i => i.TrimEnd());
            StringBuilder sb = new StringBuilder();
            sb.Append('[');
            foreach (var item in ls)
            {
                sb.Append('"').Append(JavaScriptEncoder.Default.Encode(item)).Append('"').Append(',');
            }
            sb.Append(']');
            return sb.ToString();
        }
        public static string ConvertToStringBuilder(this string value)
        {

            var ls = value.Split('\n').Select(i => i.TrimEnd());
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("StringBuilder sb = new StringBuilder();");
            foreach (var item in ls)
            {
                sb.AppendLine("sb.AppendLine(@\"" + JavaScriptEncoder.Default.Encode(item.TrimEnd()) + "\");");
            }
            return sb.ToString();
        }
    }
}
