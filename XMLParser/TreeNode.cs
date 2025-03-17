using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace XMLParser
{
    public class TreeNode
    {
        public string TagName { get; set; }
        public bool CloseTag { get; set; } = false;
        public Dictionary<string, string> Attributes { get; set; } = new();
        public List<string> Values { get; set; } = new();
        public List<TreeNode> Children { get; set; } = new();

        public List<string> TagsForSave = new List<string>() { "w:b", "w:i", "w:u", "w:strike", "w:color", "w:vertAlign", "w:rPr", "w:numPr" };

        public TreeNode? CheckChild(TreeNode node, string tagName)
        {
            if (node.Children.Count != 0)
            {
                foreach(TreeNode child in node.Children)
                {
                    if (child.TagName == tagName)
                    {
                        return child;
                    }
                }
            }
            return null;
        }

        public List<TreeNode> QuikBreadthFirstSearch(TreeNode tree, string tagName)
        {
            List<TreeNode> tags = new List<TreeNode>();
            Queue<TreeNode> queue = new Queue<TreeNode>();
            queue.Enqueue(tree);

            while (queue.Count > 0) 
            { 
                TreeNode currentNode = queue.Dequeue();
                if (CheckChild(currentNode, tagName) != null)
                {
                    tags.Add(CheckChild(currentNode, tagName));
                }
                else
                {
                    foreach (TreeNode child in currentNode.Children)
                    {
                        queue.Enqueue(child);
                    }
                }

            }
            return tags;
        }

        public List<TreeNode> LongBreadthFirstSearch(TreeNode tree, string tagName)
        {
            List<TreeNode> tags = new List<TreeNode>();
            Queue <TreeNode> queue = new Queue<TreeNode>();
            queue.Enqueue(tree);

            while (queue.Count > 0) 
            {
                TreeNode currentNode = queue.Dequeue();
                foreach (TreeNode child in currentNode.Children)
                {
                    if(child.TagName == tagName)
                    {
                        tags.Add(child);
                    }
                    queue.Enqueue(child);
                }
            }
            return tags;
        }

        public void TerminateChildren(List<TreeNode> tags)
        {
            foreach (TreeNode currentNode in tags)
            {
                // Проходим по детям в обратном порядке, чтобы избежать проблем с удалением элементов
                for (int i = currentNode.Children.Count - 1; i >= 0; i--)
                {
                    TreeNode childNode = currentNode.Children[i];
                    // Проверяем, совпадает ли имя тега с любым из TagForSave
                    if (!TagsForSave.Contains(childNode.TagName))
                    {
                        currentNode.Children.RemoveAt(i);
                    }
                }
            }
        }

        public TreeNode BuildTree(List<string> tokens)
        {
            Stack<TreeNode> stack = new Stack<TreeNode>();
            TreeNode root = null;

            foreach (var token in tokens)
            {
                if (token.EndsWith("/>")) // Самозакрывающийся тег
                {
                    string tagName = token.Substring(1, token.IndexOfAny(new[] { ' ', '/' }) - 1);
                    TreeNode treeNode = new TreeNode { TagName = tagName };
                    treeNode.CloseTag = false;

                    // Обработка атрибутов
                    int start = token.IndexOf(' ');
                    if (start != -1)
                    {
                        string newAttribute = token.Substring(start + 1, token.Length - start - 3); // Учитываем "/>"
                        var attributes = newAttribute.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        foreach (var item in attributes)
                        {
                            var parts = item.Split('=');
                            if (parts.Length == 2)
                            {
                                treeNode.Attributes[parts[0]] = parts[1].Trim('"');
                            }
                        }
                    }

                    if (stack.Count > 0)
                    {
                        stack.Peek().Children.Add(treeNode); // Добавляем к текущему родителю
                    }
                }
                else
                {
                    if (token.StartsWith('<') && !token.StartsWith("</"))
                    {
                        string tagName = token.Substring(1, token.IndexOfAny(new[] { ' ', '>' }) - 1);
                        TreeNode treeNode = new TreeNode { TagName = tagName };
                        treeNode.CloseTag = true;

                        // Обработка атрибутов
                        int start = token.IndexOf(' ');
                        if (start != -1)
                        {
                            string newAttribute = token.Substring(start + 1, token.Length - start - 2);
                            var attributes = newAttribute.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                            foreach (var item in attributes)
                            {
                                var parts = item.Split('=');
                                if (parts.Length == 2)
                                {
                                    treeNode.Attributes[parts[0]] = parts[1].Trim('"');
                                }
                            }
                        }

                        if (stack.Count > 0)
                        {
                            stack.Peek().Children.Add(treeNode); // Добавляем к текущему родителю
                        }
                        stack.Push(treeNode); // Добавляем узел в стек

                        if (root == null)
                        {
                            root = treeNode; // Первый элемент — корень дерева
                        }
                    }
                    else if (token.StartsWith("</"))
                    {
                        if (stack.Count > 0)
                        {
                            stack.Pop(); // Убираем узел из стека
                        }
                    }
                    else
                    {
                        if (stack.Count > 0)
                        {
                            stack.Peek().Values.Add(token); // Добавляем текст к текущему узлу
                        }
                    }
                }
            }

            return root;
        }

        public void AddChildren(List<TreeNode> parents, List<TreeNode> children)
        {
            for(int i = 0; i < parents.Count; i++)
            {
                for (int j = 0; j < children.Count; j++) 
                {
                    AddChild(parents[i], children[j]);
                }
            }
        }

        public void AddChild(TreeNode parent, TreeNode child)
        {
            parent.Children.Add(child);
        }

        public void PrintTree(TreeNode node, int indent = 0)
        {
            Console.WriteLine(new string(' ', indent * 2) + node);

            // Вывод текстовых значений
            if (node.Values != null && node.Values.Count > 0)
            {
                Console.WriteLine(new string(' ', (indent + 1) * 2) + "Values: " + string.Join(", ", node.Values));
            }

            // Рекурсивный обход дочерних элементов
            foreach (var child in node.Children)
            {
                PrintTree(child, indent + 1);
            }
        } 
    }
}
