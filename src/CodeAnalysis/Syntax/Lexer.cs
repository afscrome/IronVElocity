
using System;
using IronVelocity.CodeAnalysis.Text;

namespace IronVelocity.CodeAnalysis.Syntax
{
    public class Lexer
    {
        private readonly SourceText _text;
        private int _position;

        public Lexer(string text)
        {
            _text = new SourceText(text);
        }

        private char Current => Peek(0);
        private char LookAhead => Peek(1);

        private char Peek(int offset)
        {
            var index = _position + offset;

            if (index >= _text.Length)
                return '\0';

            return _text[index];
        }

        public SyntaxToken NextToken()
        {
            switch (Current)
            {
                case '\0':
                    return BasicToken(SyntaxKind.EndOfFileToken, "\0");

                case '$':
                    return BasicToken(SyntaxKind.DollarToken, "$");

                case '#' when LookAhead == '#':
                    return SingleLineComment();
                case '#' when LookAhead == '*':
                    return BlockComment();
                case '#' when LookAhead == '[' && Peek(2) == '[':
                    return Literal();
                case '#':
                    return BasicToken(SyntaxKind.HashToken, "#");
               
                case ' ':
                case '\t':
                    return HorizontalWhitesapce();

                case '\r':
                case '\n':
                    return VerticalWhitesapce();

                case '+':
                    return BasicToken(SyntaxKind.PlusToken, "+");
                case '-':
                    return BasicToken(SyntaxKind.MinusToken, "-");
                case '*':
                    return BasicToken(SyntaxKind.StarToken, "*");
                case '/':
                    return BasicToken(SyntaxKind.SlashToken, "/");
                case '%':
                    return BasicToken(SyntaxKind.ModuloToken, "%");

                case '0': case '1': case '2': case '3': case '4':
                case '5': case '6': case '7': case '8': case '9':
                    return Number();

                default:
                    return BasicToken(SyntaxKind.BadToken, Current.ToString());
            }
        }

        private SyntaxToken BasicToken(SyntaxKind kind, string text)
        {
            int startPosition = _position;
            _position += text.Length;
            return new SyntaxToken(kind, startPosition, text);
        }


        private SyntaxToken TokenSincePosition(SyntaxKind kind, int start)
        {
            var length = _position - start;

            var commentText = _text.Substring(start, length);

            return new SyntaxToken(kind, start, commentText);
        }

        private SyntaxToken Number()
        {
            int start = _position;

            while (Current >= '0' && Current <='9')
            {
                _position++;
            }

            return TokenSincePosition(SyntaxKind.NumberToken, start);
        }

        private SyntaxToken Literal()
        {
            int start = _position;
            _position += 3;
            while (!(Current == ']' && LookAhead == ']'))
            {
                _position++;
            }

            _position += 2;

            return TokenSincePosition(SyntaxKind.LiteralToken, start);
        }

        private SyntaxToken SingleLineComment()
        {
            int start = _position;
            _position += 2;
            while (Current != '\r' && Current != '\n' && Current != '\0')
            {
                _position++;
            }

            return TokenSincePosition(SyntaxKind.SingleLineComment, start);
        }


        private SyntaxToken BlockComment()
        {
            int start = _position;
            _position += 2;
            while (!(Current == '*' && LookAhead == '#'))
            {
                _position++;
            }

            _position += 2;

            return TokenSincePosition(SyntaxKind.BlockComment, start);
        }

        private SyntaxToken HorizontalWhitesapce()
        {
            int start = _position;
            _position++;
            while (Current == ' ' || Current == '\t')
            {
                _position++;
            }

            return TokenSincePosition(SyntaxKind.HorizontalWhitespaceToken, start);
        }

        private SyntaxToken VerticalWhitesapce()
        {
            int start = _position;
            _position++;
            while (Current == '\n' || Current == '\r')
            {
                _position++;
            }

            return TokenSincePosition(SyntaxKind.VerticalWhitespaceToken, start);
        }
    }

}