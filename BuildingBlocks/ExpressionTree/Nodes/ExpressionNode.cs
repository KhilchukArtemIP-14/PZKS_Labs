using BuildingBlocks.Extensions;
using BuildingBlocks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.ExpressionTree.Nodes
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

        public string ToInfixString()
        {
            // 1. Якщо це змінна або число
            if (Type == TokenType.VAR || Type == TokenType.CONST)
            {
                return Value;
            }

            // 2. Якщо це функція, наприклад cos(x)
            if (Type == TokenType.FN)
            {
                // Використовуємо "?." щоб уникнути помилки, якщо Left == null
                return $"{Value}({Left?.ToInfixString() ?? ""})";
            }

            // 3. Якщо це оператор (+, -, *, /)
            if (Type == TokenType.OP)
            {
                string leftStr = Left?.ToInfixString() ?? "";
                string rightStr = Right?.ToInfixString() ?? "";

                // Лівий операнд: якщо його пріоритет менший, беремо в дужки
                if (ShouldAddParentheses(Left))
                {
                    leftStr = $"({leftStr})";
                }

                // Правий операнд: складніша перевірка
                // Додаємо дужки, якщо пріоритет менший АБО
                // якщо операція некомутативна (-, /) і пріоритети рівні (наприклад, a - (b - c))
                bool isSamePrecedence = Right != null &&
                                      Right.Type == TokenType.OP &&
                                      GetOpPrecedence(Right.Value) == GetOpPrecedence(this.Value);

                if (ShouldAddParentheses(Right) || (IsNonCommutative() && isSamePrecedence))
                {
                    rightStr = $"({rightStr})";
                }

                return $"{leftStr}{Value}{rightStr}";
            }

            return Value;
        }

        private bool ShouldAddParentheses(ExpressionNode child)
        {
            if (child == null || child.Type != TokenType.OP) return false;

            int parentPrec = GetOpPrecedence(this.Value);
            int childPrec = GetOpPrecedence(child.Value);

            return childPrec < parentPrec;
        }

        private bool IsNonCommutative()
        {
            return Value == "-" || Value == "/";
        }

        private int GetOpPrecedence(string opValue)
        {
            return opValue switch
            {
                "+" or "-" => 1,
                "*" or "/" => 2,
                _ => 0,
            };
        }
        public ExpressionNode Clone()
        {
            var newToken = new Token { Value = this.Value, TokenType = this.Type };
            var newNode = new ExpressionNode(newToken);

            if (this.Left != null)
            {
                newNode.Left = this.Left.Clone();
            }

            if (this.Right != null)
            {
                newNode.Right = this.Right.Clone();
            }

            return newNode;
        }
    }
}