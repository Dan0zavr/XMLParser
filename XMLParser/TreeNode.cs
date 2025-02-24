using System;
using System.Collections.Generic;
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
        public Dictionary<string, string> Attributes { get; set; } = new();
        public List<string> Values { get; set; } = new(); // Список текстовых значений
        public List<TreeNode> Children { get; set; } = new();

        public override string ToString()
        {
            return $"<{TagName}> (Атрибутов: {Attributes.Count}, Дочерних элементов: {Children.Count})";
        }

        public TreeNode? CheckChild(TreeNode node, string tagName)
        {
            if (node.Children.Count != 0)
            {
                foreach(TreeNode child in node.Children)
                {
                    if (child.TagName == tagName)
                    {
                        return node;
                    }
                }
            }
            return null;
        }

        public List<TreeNode> BreadthFirstSearch(TreeNode tree, string tagName)
        {
            List<TreeNode> parents = new List<TreeNode>();
            Queue<TreeNode> queue = new Queue<TreeNode>();
            queue.Enqueue(tree);

            while (queue.Count > 0) 
            { 
                TreeNode currentNode = queue.Dequeue();
                if (CheckChild(currentNode, tagName) != null)
                {
                    parents.Add(currentNode);
                }
                else
                {
                    foreach (TreeNode child in currentNode.Children)
                    {
                        queue.Enqueue(child);
                    }
                }

            }
            return parents;
        }

        public void TerminateChildren(List<TreeNode> parents, string tagName)
        {
            foreach (TreeNode currentNode in parents)
            {
                for (int i = 0; i < currentNode.Children.Count; i++) 
                { 
                    if(currentNode.Children[i].TagName == tagName)
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
                    string tagName = token.Substring(1, token.IndexOfAny(new[] { ' ', '>' }) - 1);
                    TreeNode treeNode = new TreeNode { TagName = tagName };

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
