using System;
using System.Collections.Generic;
using System.Text;
using XMLParser.DocumentPipeline;
using XMLParser.SpecialClasses.InputOutput;
using XMLParser.SpecialClasses.Tree;
using XMLParser.Styles;
using static XMLParser.SpecialClasses.Tree.Tokenizator;

namespace XMLParser.SpecialClasses.DocumentChangers
{
    public class ColontitulService
    {
        private PiplineContext _context;
        private GlobalStyle _globalStyle;

        public ColontitulService(PiplineContext context)
        {
            _context = context;
            _globalStyle = _context.Template.GlobalStyle;
        }

        public void ClearSectPr()
        {
            TreeNode body = _context.DocumentRoot.LongBreadthFirstSearch("w:body").FirstOrDefault();

            bool isLastSectPrPassed = false;
            for(int i = body.Children.Count - 1; i >= 0; --i)
            {
                if (body.Children[i].TagName == "w:sectPr")
                {
                    if (isLastSectPrPassed)
                    {
                        body.Children.RemoveAt(i);
                    }
                    else
                    {
                        isLastSectPrPassed = true;
                    }
                }
            }
        }

        private TreeNode CreateSectPrNastedNodes()
        {
            List<TreeNode> nastedNodes = new List<TreeNode>();

            TreeNode margin = new TreeNode
            {
                TagName = "w:pgMar",
                Attributes = { {"w:top", $"{CmToTwips(_globalStyle.TopMargin)}" },
                               {"w:bottom", $"{CmToTwips(_globalStyle.BottomMargin)}" },
                               {"w:left", $"{CmToTwips(_globalStyle.LeftMargin)}" },
                               {"w:right", $"{CmToTwips(_globalStyle.RightMargin)}" },
                               {"w:header", "708" },
                               {"w:footer", "708" },
                               {"w:gutter", "0" } }
            };

            nastedNodes.Add(margin);

            return new TreeNode
            {
                TagName = "container",
                Children = nastedNodes
            };
        }

        private int CmToTwips(double cm)
        {
            return (int)Math.Round(cm * 1440 / 2.54);
        }

        public void AddNoNumberingSectPr(int[] targetPages)
        {
            if (_globalStyle.LastNoNumberingPage == null)
                return;

            TreeNode body = _context.DocumentRoot.LongBreadthFirstSearch("w:body").First();

            Stash stash = new Stash(body);

            int insertIndex = stash.FindLastElementIndexOfPages(_context.ForColontitulPagesWords, targetPages);

            if (insertIndex < 0)
                return;

            TreeNode p = new TreeNode
            {
                TagName = "w:p",
                Children = {
                    new TreeNode
                    {
                        TagName = "w:pPr",
                        CloseTag = true,
                        Children = { 
                            new TreeNode {
                                TagName = "w:sectPr",
                                CloseTag = true
                            }
                        }
                    }
                },
                CloseTag = true
            };

            body.Children.Insert(insertIndex + 1, p);
        }

        public void ApplyFields()
        {
            TreeNode root = _context.DocumentRoot;
            List<TreeNode> sectPrs = root.LongBreadthFirstSearch("w:sectPr");
            TreeNode style = CreateSectPrNastedNodes();

            if (sectPrs.Count == 0) throw new Exception("Не найден конец документа");

            foreach (var sectPr in sectPrs) {

                TreeNode oldMar = sectPr.LongBreadthFirstSearch("w:pgMar").FirstOrDefault();

                if (oldMar != null) 
                {
                    oldMar.Attributes = style.LongBreadthFirstSearch("w:pgMar").First().Attributes;
                }
                else
                {
                    sectPr.Children.Add(style.LongBreadthFirstSearch("w:pgMar").First().Clone());
                }
            }
        }

        public void ApplyFooter() // это надо поменять, я делал это в спешке и с большим нежеланием
        {
            (TreeNode numberingFooter, List<string> numberingTokens) = XMLRead.ReadXMLDocument(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "numberingFooterReference.xml"));
            (TreeNode specilaFooter, List<string> specialFooterTokens) = XMLRead.ReadXMLDocument(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "specialFooterReference.xml"));

            TreeNode crutchStyleLink = new TreeNode // Я не хочу думать, как переносить ссылку на стиль, поэтому просто предполагаем, что такой стиль есть, т.к. стиль текста обязателен
            {
                TagName = "w:rStyle",
                Attributes = { { "w:val", "WordRegSimpleStyle" } }
            };

            //Устанавливаем стили текста для колонтитулов
            List<TreeNode> rPr = numberingFooter.LongBreadthFirstSearch("w:rPr");
            foreach (var elem in rPr) 
            {
                elem.Children.Add(crutchStyleLink);
            }

            rPr.Clear();
            rPr = specilaFooter.LongBreadthFirstSearch("w:rPr");
            foreach (var elem in rPr)
            {
                elem.Children.Add(crutchStyleLink);
            }

            //Устанавливаем центрирование
            TreeNode jc = numberingFooter.LongBreadthFirstSearch("w:jc").First();
            jc.Attributes["w:val"] = _context.Template.GlobalStyle.Alignment;

            jc = specilaFooter.LongBreadthFirstSearch("w:jc").First();
            jc.Attributes["w:val"] = _context.Template.GlobalStyle.Alignment;

            if (_context.Template.GlobalStyle.SpecialColontitul != null) // Добавление значения спец. колонтитула
            {
                TreeNode text = specilaFooter.LongBreadthFirstSearch("w:t").First();
                text.Values = new List<string>();
                text.Values.Add(_context.Template.GlobalStyle.SpecialColontitul);
            }

            (TreeNode refsRoot, List<string> refsTokens) = XMLRead.ReadXMLDocument(Path.Combine(_context.TempDocumentDirectory, "word", "_rels", "document.xml.rels"));

            List<TreeNode> sectPrs = _context.DocumentRoot.LongBreadthFirstSearch("w:sectPr");

            Dictionary<string, string> footers = new Dictionary<string, string>();
            string numberingFooterName = "NumberingFooter.xml";
            string specilaFooterName = "SpecialColontitul.xml";
            string emptyFooterName = "EmptyFooter.xml";

            string numberingId = "mId02";
            string specialId = "mId01";
            string emptyId = "mId03";

            footers.Add(numberingId, numberingFooterName);
            footers.Add(specialId, specilaFooterName);
            footers.Add(emptyId, emptyFooterName);


            TreeNode relation = new TreeNode
            {
                TagName = "Relationship",
                Attributes = { {"Id", "mId" }, {"Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/footer" }, {"Target", "name"} }
            };

            foreach (var item in footers)
            {
                TreeNode clone = relation.Clone();
                clone.Attributes["Id"] = item.Key;
                clone.Attributes["Target"] = item.Value;
                refsRoot.Children.Add(clone);
            }

            TreeNode referenceNode = new TreeNode
            {
                TagName = "w:footerReference",
                Attributes = { {"w:type", "name"}, {"r:id", "id" } }
            };

            if (sectPrs.Count > 1)  // если есть страницы без нумерации
            {
                TreeNode first = sectPrs.Last();
                TreeNode last = sectPrs.First();
                sectPrs.Clear();
                sectPrs.Add(first);
                sectPrs.Add(last);
                if (_context.Template.GlobalStyle.SpecialColontitul != null)
                {
                    TreeNode clone = referenceNode.Clone();
                    clone.Attributes["w:type"] = "first";
                    clone.Attributes["r:id"] = specialId;

                    List<TreeNode> nodes = sectPrs[0].LongBreadthFirstSearch("w:footerReference");
                    bool isFirstFound = false;
                    foreach (var node in nodes)
                    {
                        if (node.Attributes.TryGetValue("w:type", out string o) && o == "first")
                        {
                            node.Attributes["id"] = clone.Attributes["id"];
                            isFirstFound = true;
                        }
                    }

                    if (!isFirstFound)
                    {
                        sectPrs[0].Children.Insert(0, clone);
                    }

                    TreeNode titelPage = new TreeNode
                    {
                        TagName = "w:titlePg"
                    };
                    sectPrs[0].Children.Add(titelPage);
                }

                if (_context.Template.GlobalStyle.LastNoNumberingPage > 0)
                {
                    TreeNode clone2 = referenceNode.Clone();
                    clone2.Attributes["w:type"] = "default";
                    clone2.Attributes["r:id"] = emptyId;

                    List<TreeNode> nodes2 = sectPrs[0].LongBreadthFirstSearch("w:footerReference");
                    bool isDefaultFound = false;
                    foreach (var node in nodes2)
                    {
                        if (node.Attributes.TryGetValue("w:type", out string o) && o == "default")
                        {
                            node.Attributes["id"] = clone2.Attributes["id"];
                            isDefaultFound = true;
                        }
                    }

                    if (!isDefaultFound)
                    {
                        sectPrs[0].Children.Insert(0, clone2);
                    }
                }

                if (_context.Template.GlobalStyle.LastNoNumberingPage > 0)
                {
                    //для остального документа
                    TreeNode clone3 = referenceNode.Clone();
                    clone3.Attributes["w:type"] = "default";
                    clone3.Attributes["r:id"] = numberingId;

                    List<TreeNode> nodes3 = sectPrs[sectPrs.Count - 1].LongBreadthFirstSearch("w:footerReference");
                    bool isDefaultFound2 = false;
                    foreach (var node in nodes3)
                    {
                        if (node.Attributes.TryGetValue("w:type", out string o) && o == "default")
                        {
                            node.Attributes["id"] = clone3.Attributes["id"];
                            isDefaultFound2 = true;
                        }
                    }

                    if (!isDefaultFound2)
                    {
                        sectPrs[sectPrs.Count - 1].Children.Insert(0, clone3);
                    }
                }

                if (_context.Template.GlobalStyle.LastNoNumberingPage != null)
                {
                    TreeNode start = new TreeNode
                    {
                        TagName = "w:pgNumType",
                        Attributes = { { "w:start", (_context.Template.GlobalStyle.LastNoNumberingPage).ToString() } }
                    };

                    sectPrs[sectPrs.Count - 1].Children.Add(start);
                }
            }
            else // если все страницы с нумерацией
            {
                if (_context.Template.GlobalStyle.SpecialColontitul != null)
                {
                    TreeNode clone = referenceNode.Clone();
                    clone.Attributes["w:type"] = "first";
                    clone.Attributes["r:id"] = specialId;

                    List<TreeNode> nodes = sectPrs[sectPrs.Count - 1].LongBreadthFirstSearch("w:footerReference");
                    bool isFirstFound = false;
                    foreach (var node in nodes)
                    {
                        if (node.Attributes.TryGetValue("w:type", out string o) && o == "first")
                        {
                            node.Attributes["id"] = clone.Attributes["id"];
                            isFirstFound = true;
                        }
                    }

                    if (!isFirstFound)
                    {
                        sectPrs[sectPrs.Count - 1].Children.Insert(0, clone);
                    }

                    TreeNode titelPage = new TreeNode
                    {
                        TagName = "w:titlePg"
                    };
                    sectPrs[sectPrs.Count - 1].Children.Add(titelPage);
                }

                if (_context.Template.GlobalStyle.LastNoNumberingPage > 0)
                {
                    TreeNode clone2 = referenceNode.Clone();
                    clone2.Attributes["w:type"] = "default";
                    clone2.Attributes["r:id"] = numberingId;

                    List<TreeNode> nodes2 = sectPrs[sectPrs.Count - 1].LongBreadthFirstSearch("w:footerReference");
                    bool isDefaultFound = false;
                    foreach (var node in nodes2)
                    {
                        if (node.Attributes.TryGetValue("w:type", out string o) && o == "default")
                        {
                            node.Attributes["id"] = clone2.Attributes["id"];
                            isDefaultFound = true;
                        }
                    }

                    if (!isDefaultFound)
                    {
                        sectPrs[sectPrs.Count - 1].Children.Insert(0, clone2);
                    }
                }
            }

            XMLWrite.TreeToXMLDocument(numberingFooter, numberingTokens, numberingFooterName, Path.Combine(_context.TempDocumentDirectory, "word"));
            XMLWrite.TreeToXMLDocument(specilaFooter, specialFooterTokens, specilaFooterName, Path.Combine(_context.TempDocumentDirectory, "word"));
            File.Copy(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "emptyFooterReference.xml"), Path.Combine(_context.TempDocumentDirectory, "word", emptyFooterName));

            XMLWrite.TreeToXMLDocument(refsRoot, refsTokens, "document.xml.rels", Path.Combine(_context.TempDocumentDirectory, "word", "_rels"));

            (TreeNode contentTypes, List<string> contentTokens) = XMLRead.ReadXMLDocument(Path.Combine(_context.TempDocumentDirectory, "[Content_Types].xml"));

            TreeNode overrideNode = new TreeNode
            {
                TagName = "Override",
                Attributes = { {"PartName", ""},{"ContentType", "application/vnd.openxmlformats-officedocument.wordprocessingml.footer+xml" } }
            };

            foreach(var item in footers)
            {
                TreeNode clone = overrideNode.Clone();
                clone.Attributes["PartName"] = $"/word/{item.Value}";
                contentTypes.Children.Add(clone);
            }

            XMLWrite.TreeToXMLDocument(contentTypes, contentTokens, "[Content_Types].xml", _context.TempDocumentDirectory);
        }

    }
}

