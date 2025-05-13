﻿/*using System.Collections.Generic;
using System.Text;

namespace Laba1
{
    
    public enum TokenCode
    {
        Integer = 1,        // целое
        Float = 2,          // вещественное
        Identifier = 3,     // идентификатор
        AssignOp = 4,       // "="
        Minus = 5,          // (если бы мы хотели отдельный токен)
        Comma = 6,          // ","
        LBracket = 7,       // "["
        RBracket = 8,       // "]"
        Semicolon = 9,      // ";"
        StringLiteral = 10, // "..." строка
        Plus = 11,
        String = 12,
        Error = 99
    }

    public class Token
    {
        public TokenCode Code { get; set; }
        public string Type { get; set; }
        public string Lexeme { get; set; }
        public int StartPos { get; set; }
        public int EndPos { get; set; }
        public int Line { get; set; }

        public override string ToString()
        {
            return $"[{Line}:{StartPos}-{EndPos}] ({Code}) {Type} : '{Lexeme}'";
        }
    }

    public class Scanner
    {
        private string _text;
        private int _pos;       // текущая позиция (сквозная по всему тексту)
        private int _line;      // текущая строка
        private int _linePos;   // позиция в текущей строке
        private List<Token> _tokens;

        // Пример набора ключевых слов
        private HashSet<string> _keywords = new HashSet<string> { "List" };

        public Scanner()
        {
            _tokens = new List<Token>();
        }

        public List<Token> Scan(string text)
        {
            _text = text;
            _pos = 0;
            _line = 1;
            _linePos = 1;
            _tokens.Clear();

            while (!IsEnd())
            {
                char ch = CurrentChar();

                switch (ch)
                {
                    // Пропускаем незначащие пробелы, табуляцию и переводы строк
                    case var c when char.IsWhiteSpace(c):
                        Advance();
                        break;

                    // Буква - значит начинаем считывать идентификатор
                    case var c when char.IsLetter(c) &&  c >= 65 && c <= 122:
                        ReadIdentifierOrKeyword();
                        Advance();
                        break;

                    // Минус: может быть частью числа (например, -5 или -2.3)
                    case '-':
                        AddToken(TokenCode.Minus, " знак минус ", "-");
                        Advance();
                        break;

                    case '+':
                        AddToken(TokenCode.Plus, " знак плюс ", "+");
                        Advance();
                        break;

                    // Цифра - читаем число (целое или вещественное)
                    case var c when char.IsDigit(c):
                        ReadNumber();
                        break;

                    // Оператор присваивания
                    case '=':
                        AddToken(TokenCode.AssignOp, " оператор присваивания ", "=");
                        Advance();
                        break;

                    // Открывающая скобка
                    case '[':
                        AddToken(TokenCode.LBracket, " открывающая скобка ", "[");
                        Advance();
                        break;

                    // Закрывающая скобка
                    case ']':
                        AddToken(TokenCode.RBracket, " закрывающая скобка ", "]");
                        Advance();
                        break;

                    // Запятая
                    case ',':
                        AddToken(TokenCode.Comma, " запятая ", ",");
                        Advance();
                        break;

                    // Точка с запятой
                    case ';':
                        AddToken(TokenCode.Semicolon, "конец оператора", ";");
                        Advance();
                        break;

                    // Строка (начинается на ")
                    case '"':
                        AddToken(TokenCode.String, " кавычка ", " '' ");
                        ReadStringLiteral();
                        Advance();
                        break;

                    // По умолчанию - недопустимый символ
                    default:
                        AddToken(TokenCode.Error, " недопустимый символ ", ch.ToString());
                        Advance();
                        break;
                }
            }

            return _tokens;
        }


        /// <summary>
        /// Считывание идентификатора
        /// </summary>
        private void ReadIdentifierOrKeyword()
        {
            int startPos = _linePos;
            var sb = new StringBuilder();

            // Разрешаем только латинские буквы
            while (!IsEnd() && IsLatinLetter(CurrentChar()))
            {
                sb.Append(CurrentChar());
                Advance();
            }

            string lexeme = sb.ToString();
            if (_keywords.Contains(lexeme))
                AddToken(TokenCode.Identifier, " ключевое слово ", lexeme, startPos, _linePos - 1, _line);
            else
            {
                AddToken(TokenCode.Identifier, " идентификатор ", lexeme, startPos, _linePos - 1, _line);
            }
        }

        // Метод для проверки, является ли символ латинской буквой
        private bool IsLatinLetter(char ch)
        {
            return (ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z');
        }


        /// <summary>
        /// Считывание числа (целое или вещественное, может быть с минусом)
        /// </summary>
        private void ReadNumber()
        {
            int startPos = _linePos;
            bool hasDot = false;

            StringBuilder sb = new StringBuilder();

            // Может начинаться с минуса
            if (CurrentChar() == '-')
            {
                sb.Append(CurrentChar());
                Advance();
            }

            // Основной цикл
            while (!IsEnd())
            {
                char ch = CurrentChar();
                if (char.IsDigit(ch))
                {
                    sb.Append(ch);
                    Advance();
                }
                else if (ch == '.')
                {
                    if (hasDot)
                    {
                        // Повторная точка - прерываем, либо помечаем ошибку
                        break;
                    }
                    else
                    {
                        hasDot = true;
                        sb.Append(ch);
                        Advance();
                    }
                }
                else
                {
                    // Не цифра и не точка - конец числа
                    break;
                }
            }

            string numberLexeme = sb.ToString();

            if (hasDot)
            {
                AddToken(TokenCode.Float, " вещественное число ", numberLexeme, startPos, _linePos - 1, _line);
            }
            else
            {
                AddToken(TokenCode.Integer, " целое число ", numberLexeme, startPos, _linePos - 1, _line);
            }
        }

        /// <summary>
        /// Считывание строкового литерала (открывающая кавычка уже считана)
        /// </summary>
        private void ReadStringLiteral()
        {
            int startPos = _linePos;  // позиция открывающей кавычки
            StringBuilder sb = new StringBuilder();

            // Пропускаем открывающую кавычку
            Advance(); // уходим за '"'
            bool closed = false;

            while (!IsEnd())
            {
                char ch = CurrentChar();
                if (ch == '"')
                {
                    // нашли закрывающую кавычку
                    closed = true;
                    AddToken(TokenCode.String, " закрывающая кавычка ", " '' ");
                    Advance(); // пропускаем закрывающую кавычку
                    break;
                }
                else
                {
                    // добавляем символ в строку (включая пробелы)
                    sb.Append(ch);
                    Advance();
                }
            }

            string strValue = sb.ToString();
            if (!closed)
            {
                // Строка не закрылась
                AddToken(TokenCode.Error, " незакрытая строка ", strValue, startPos, _linePos - 1, _line);
            }
            else
            {
                // Закрытая строка
                AddToken(TokenCode.StringLiteral, " строка ", strValue, startPos, _linePos - 1, _line);
            }
        }

        #region Вспомогательные методы

        private bool IsEnd()
        {
            return _pos >= _text.Length;
        }

        private char CurrentChar()
        {
            if (IsEnd()) return '\0';
            return _text[_pos];
        }

        private void Advance()
        {
            // Если встретили перевод строки, переходим на следующую строку
            if (CurrentChar() == '\n')
            {
                _line++;
                _linePos = 0;
            }
            _pos++;
            _linePos++;
        }

        private void AddToken(TokenCode code, string type, string lexeme)
        {
            AddToken(code, type, lexeme, _linePos, _linePos, _line);
        }

        private void AddToken(TokenCode code, string type, string lexeme, int startPos, int endPos, int line)
        {
            var token = new Token
            {
                Code = code,
                Type = type,
                Lexeme = lexeme,
                StartPos = startPos,
                EndPos = endPos,
                Line = line
            };
            _tokens.Add(token);
        }

        #endregion
    }
}*/


using System;
using System.Collections.Generic;

namespace TetradApp
{
    public enum TokenType
    {
        Plus, Assign, Minus, Mul, Div, LParen, RParen, Identifier, End
    }

    public class Token
    {
        public TokenType Type { get; }
        public string Lexeme { get; }
        public int Position { get; }

        public Token(TokenType type, string lexeme, int pos)
        {
            Type = type;
            Lexeme = lexeme;
            Position = pos;
        }
    }

    public class LexicalError
    {
        public string Message { get; }
        public int Position { get; }
        public int Length { get; }

        public LexicalError(string msg, int pos, int len)
        {
            Message = msg;
            Position = pos;
            Length = len;
        }
    }

    public class Lexer
    {
        private readonly string _input;
        private int _idx;

        public List<Token> Tokens { get; } = new List<Token>();
        public List<LexicalError> Errors { get; } = new List<LexicalError>();

        public Lexer(string input)
        {
            _input = input;
            _idx = 0;
        }

        public void Tokenize()
        {
            while (_idx < _input.Length)
            {
                char c = _input[_idx];
                if (char.IsWhiteSpace(c))
                {
                    _idx++;
                    continue;
                }
                if (char.IsLetter(c))
                {
                    int start = _idx;
                    while (_idx < _input.Length && char.IsLetter(_input[_idx]))
                        _idx++;
                    string lex = _input.Substring(start, _idx - start);
                    Tokens.Add(new Token(TokenType.Identifier, lex, start));
                    continue;
                }

                switch (c)
                {
                    case '+': Tokens.Add(new Token(TokenType.Plus, "+", _idx)); break;
                    case '-': Tokens.Add(new Token(TokenType.Minus, "-", _idx)); break;
                    case '*': Tokens.Add(new Token(TokenType.Mul, "*", _idx)); break;
                    case '/': Tokens.Add(new Token(TokenType.Div, "/", _idx)); break;
                    case '(': Tokens.Add(new Token(TokenType.LParen, "(", _idx)); break;
                    case ')': Tokens.Add(new Token(TokenType.RParen, ")", _idx)); break;
                    case '=': Tokens.Add(new Token(TokenType.Assign, "=", _idx)); break;  // <-- добавили
                    default:
                        Errors.Add(new LexicalError($"Недопустимый символ '{c}'", _idx, 1));
                        break;
                }
                _idx++;
            }
            Tokens.Add(new Token(TokenType.End, string.Empty, _idx));
        }
    }
}

