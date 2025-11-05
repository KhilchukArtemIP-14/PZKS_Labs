using BuildingBlocks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZKS_Lab2.Nodes
{
    public class ExpressionNode
    {
        public string Value { get; set; }
        public TokenType Type { get; set; }

        public ExpressionNode Left { get; set; }
        public ExpressionNode Right { get; set; }

        public ExpressionNode(Token token)
        {
            this.Value = token.Value;
            this.Type = token.TokenType;
            this.Left = null;
            this.Right = null;
        }
        public override string ToString()
        {
            var builder = new StringBuilder();
            BuildStringRecursive(builder, "", true);
            return builder.ToString();
        }
        private void BuildStringRecursive(StringBuilder builder, string indent, bool isLast)
        {
            if (builder == null) return;

            var marker = isLast ? "└──" : "├──";
            builder.Append(indent);
            builder.Append(marker);

            if (Type == TokenType.OP || Type == TokenType.FN)
            {
                builder.AppendLine($"({Value})");
            }
            else
            {
                builder.AppendLine(Value);
            }

            indent += isLast ? "    " : "│   ";

            var children = new List<ExpressionNode>();
            if (Left != null) children.Add(Left);
            if (Right != null) children.Add(Right);

            for (int i = 0; i < children.Count; i++)
            {
                children[i].BuildStringRecursive(builder, indent, i == children.Count - 1);
            }
        }
    }
}