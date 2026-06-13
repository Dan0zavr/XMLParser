using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XMLParser.SpecialClasses.Tree;

namespace XMLParser.SpecialClasses.DocumentChangers
{
    public static class StyleIntegrator
    {
        public static void IntegrateStylesToTree(TreeNode root, List<TreeNode> styles)
        {
            foreach (TreeNode style in styles)
            {
                if (style.TagName != "formulaContainer" && style.TagName != "globalContainer") root.Children.Add(style);
            }
        }
        // abs = абстрактный
        public static void IntegrateNumberingStylesToTree(TreeNode docRoot, TreeNode numberingRoot, List<TreeNode> myAbstractNodes, TreeNode paragraphStyle)
        {
            List<TreeNode> numPr = docRoot.LongBreadthFirstSearch("w:numPr");
            List<string> lvls = numPr.Select(n => n.Children[0].Attributes["w:val"]).ToList();
            List<string> ids = numPr.Select(n => n.Children[1].Attributes["w:val"]).ToList();

            HashSet<(string id, string lvl)> values = IdAndLvlToHashSet(ids, lvls);

            List<TreeNode> nums = numberingRoot.LongBreadthFirstSearch("w:num");
            List<TreeNode> absNums = numberingRoot.LongBreadthFirstSearch("w:abstractNum");

            foreach((var id, var lvl) in values)
            {
                TreeNode num = BinarySearchById(nums, "w:numId", Convert.ToInt32(id));
                int absId = Convert.ToInt32(num.Children[0].Attributes["w:val"]);
                TreeNode absNum = BinarySearchById(absNums, "w:abstractNumId", absId);
                List<TreeNode> lvlNodes = absNum.LongBreadthFirstSearch("w:lvl");

                int targetLvl = Convert.ToInt32(lvl);
                foreach (var lvlNode in lvlNodes)
                {
                    int currentLvl = Convert.ToInt32(lvlNode.Attributes["w:ilvl"]);
                    if (currentLvl == targetLvl)
                    {
                        string currentNumFmt = lvlNode.QuikBreadthFirstSearch("w:numFmt").First().Attributes["w:val"];
                        foreach (var myAbstractNode in myAbstractNodes)
                        {
                            string myFmt = myAbstractNode.QuikBreadthFirstSearch("w:numFmt").First().Attributes["w:val"];
                            if (currentNumFmt == myFmt)
                            {
                                TreeNode myLvl = myAbstractNode.QuikBreadthFirstSearch("w:lvl").First();
                                AddInd(myLvl, currentLvl, paragraphStyle);
                                TreeNode clone = myLvl.Clone();
                                lvlNode.Children.Clear();
                                lvlNode.Children = clone.Children;
                                SyncLvl(lvlNode);
                                break;
                            }
                        }
                    }
                }
            }
        }

        public static void IntegrateNumberingStylesToTree(TreeNode docRoot, TreeNode numberingRoot, TreeNode paragraphStyle)
        {
            List<TreeNode> numPr = docRoot.LongBreadthFirstSearch("w:numPr");
            List<string> lvls = numPr.Select(n => n.Children[0].Attributes["w:val"]).ToList();
            List<string> ids = numPr.Select(n => n.Children[1].Attributes["w:val"]).ToList();

            HashSet<(string id, string lvl)> values = IdAndLvlToHashSet(ids, lvls);

            List<TreeNode> nums = numberingRoot.LongBreadthFirstSearch("w:num");
            List<TreeNode> absNums = numberingRoot.LongBreadthFirstSearch("w:abstractNum");

            foreach ((var id, var lvl) in values)
            {
                TreeNode num = BinarySearchById(nums, "w:numId", Convert.ToInt32(id));
                int absId = Convert.ToInt32(num.Children[0].Attributes["w:val"]);
                TreeNode absNum = BinarySearchById(absNums, "w:abstractNumId", absId);
                List<TreeNode> lvlNodes = absNum.LongBreadthFirstSearch("w:lvl");

                int targetLvl = Convert.ToInt32(lvl);
                foreach (var lvlNode in lvlNodes)
                {
                    int currentLvl = Convert.ToInt32(lvlNode.Attributes["w:ilvl"]);
                    if (currentLvl == targetLvl)
                    {
                        TreeNode ind = lvlNode.LongBreadthFirstSearch("w:ind").First();
                        TreeNode pInd = paragraphStyle.LongBreadthFirstSearch("w:ind").First();
                        string firstLine = pInd.Attributes["w:firstLine"];
                        ind.Attributes["w:left"] = "0";
                        ind.Attributes.Remove("w:hanging");
                        ind.Attributes.Add("w:firstLine", firstLine);
                        break;
                    }
                }
            }
        }

        //Для нумерованных списков, заменяет lvlText
        private static void SyncLvl(TreeNode lvlNode)
        {
            if (lvlNode.QuikBreadthFirstSearch("w:numFmt").First().Attributes["w:val"] != "decimal") return;

            TreeNode lvlText = lvlNode.QuikBreadthFirstSearch("w:lvlText").First();
            string text = lvlText.Attributes["w:val"];
            string level = lvlNode.Attributes["w:ilvl"];
            int lvl = Convert.ToInt32(level);

            StringBuilder sb = new StringBuilder(text);

            for (int i = 0; i < text.Length; i++)
            {
                if (int.TryParse(text[i].ToString(), out int number))
                {
                    sb[i] = (char)((lvl + 1) + '0');
                }
            }

            lvlText.Attributes["w:val"] = sb.ToString();
        }

        private static void AddInd(TreeNode myLvlNode, int currentLvl, TreeNode paragraphStyle)
        {
            TreeNode ind = paragraphStyle.QuikBreadthFirstSearch("w:ind").FirstOrDefault();

            double left = 0;
            double firstLine = 0;

            if (ind != null)
            {
                if (ind.Attributes.ContainsKey("w:left"))
                    left = Convert.ToDouble(ind.Attributes["w:left"].Replace('.', ','));

                if (ind.Attributes.ContainsKey("w:firstLine"))
                    firstLine = Convert.ToDouble(ind.Attributes["w:firstLine"].Replace('.', ','));
            }

            TreeNode pPr = myLvlNode.QuikBreadthFirstSearch("w:pPr").First();

            TreeNode absNumInd = pPr.LongBreadthFirstSearch("w:ind").FirstOrDefault();
            if (absNumInd == null)
            {
                absNumInd = new TreeNode
                {
                    TagName = "w:ind"
                };
                pPr.Children.Add(absNumInd);
            }

            double step = 720;

            double newLeft = left + currentLvl * step;

            absNumInd.Attributes["w:left"] = newLeft.ToString().Replace(',', '.');
            absNumInd.Attributes["w:firstLine"] = firstLine.ToString().Replace(',', '.');

            absNumInd.Attributes.Remove("w:hanging");
        }

        private static TreeNode BinarySearchById(List<TreeNode> nodes, string attributeKey, int target )
        {
            int i = 0;
            int j = nodes.Count - 1;

            while (i <= j)
            {
                int mid = (i + j) / 2;
                int hypotesis = Convert.ToInt32(nodes[mid].Attributes[attributeKey]);

                if (hypotesis == target) 
                    return nodes[mid];
                else if (hypotesis < target) i = mid + 1;
                else j = mid - 1;
            }

            throw new Exception("Numbering Id Not Found");
        }

        private static HashSet<(string id, string lvl)> IdAndLvlToHashSet(List<string> ids, List<string> lvls)
        {
            if (ids.Count != lvls.Count) throw new Exception("Невозможно преобразовать ids и lvls во множество");

            HashSet<(string id, string lvl)> result = new HashSet<(string id, string lvl)>();

            for (int i = 0; i < ids.Count; i++)
            {
                result.Add((ids[i], lvls[i]));
            }

            return result;
        }
    }
}
