﻿using Koubot.Tool.Extensions;
using Koubot.Tool.General;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Koubot.Tool.Interfaces;

namespace Koubot.Tool.Maths
{
    /// <summary>
    /// 表达式计算器
    /// </summary>
    public class ExpressionCalculator : IKouErrorMsg
    {
        /// <summary>
        /// 计算结果,如果表达式出错则返回空（调用ToString即是结果）
        /// </summary>
        /// <param name="statement"></param>
        /// <returns></returns>
        public object? Calculate(string statement)
        {
            if (statement != null && statement.Trim() != string.Empty)
            {
                try
                {
                    var evaluator = new ExpressionCalculator();
                    return evaluator.GetFormulaResult(statement);
                }
                catch (Exception e)
                {
                    ErrorMsg = e.Message;
                    return null;
                }

            }

            return null;
        }


        private object? GetFormulaResult(string originExpression)
        {
            if (originExpression == "")
            {
                return null;
            }
            var postfixExpression = BuildingRPN(originExpression);//转化为后缀表达式

            var tmp = "";//临时存放转换运算数
            var operandStack = new Stack(); //这个栈只存放运算的数字

            var operand = new StringBuilder();//运算数 用来转换字符型数字到double型数字
            double x, y;//两个参与运算的运算数，现在仅支持双目运算或单目运算
            foreach (var nowChar in postfixExpression)
            {
                //added c==',' for germany culture 德国文化？
                if (char.IsDigit(nowChar) || nowChar is '.' or ',')
                {
                    //数据值收集.
                    operand.Append(nowChar);
                }
                else switch (nowChar)
                {
                    case ' ' when operand.Length > 0:

                        #region 运算数转换 将字符型数字处理成double型可供运算的数字
                        try
                        {
                            //七奏：你都已经处理成！了就不需要了罢
                            //tmp = Operand.ToErrorString();
                            //if (tmp.StartsWith("-"))//负数的转换一定要小心...它不被直接支持.
                            //{
                            //    //现在我的算法里这个分支可能永远不会被执行.
                            //    operandStack.Push(-((double)Convert.ToDouble(tmp.Substring(1, tmp.Length - 1))));
                            //}
                            //else
                            //{
                            //    operandStack.Push(Convert.ToDouble(tmp));
                            //}
                            tmp = operand.ToString();
                            operandStack.Push(Convert.ToDouble(tmp));
                        }
                        catch(Exception e)
                        {
                            ErrorMsg = $"{e.Message}：{tmp}";
                            return null;
                        }
                        operand = new StringBuilder();
                        #endregion

                        break;
                    case '+' or '-' or '*' or '/' or '%' or '^':
                    {
                        #region 双目运算
                        if (operandStack.Count > 0)/*如果输入的表达式根本没有包含运算符.或是根本就是空串.这里的逻辑就有意义了.    七奏：可是没包含运算符也进不来啊 可能是说运算数*/
                        {
                            y = (double)operandStack.Pop();
                        }
                        else
                        {
                            return null;
                            //operandStack.Push(0);
                            //break;
                        }
                        if (operandStack.Count > 0)
                            x = (double)operandStack.Pop();
                        else
                        {
                            return null;//双目运算如果运算数不够说明语法有误。
                            //operandStack.Push(y);
                            //break;
                        }
                        switch (nowChar)
                        {
                            case '+':
                                operandStack.Push(x + y);
                                break;
                            case '-':
                                operandStack.Push(x - y);
                                break;
                            case '*':
                                //if (y == 0) 
                                //{
                                //    operandStack.Push(x * 1);
                                //}
                                //else
                                //{
                                //    operandStack.Push(x * y);
                                //}
                                operandStack.Push(x * y);
                                break;
                            case '/':
                                operandStack.Push(x / y);
                                break;
                            case '%':
                                operandStack.Push(x % y);
                                break;
                            case '^':
                                if (x > 0)
                                {
                                    //我原本还想,如果被计算的数是负数,又要开真分数次方时如何处理的问题.后来我想还是算了吧.
                                    operandStack.Push(Math.Pow(x, y));
                                }
                                else
                                {
                                    var t = y;
                                    var ts = "";
                                    t = 1 / (2 * t);
                                    ts = t.ToString();
                                    if (ts.ToUpper().LastIndexOf('E') > 0)
                                    {

                                    }
                                }
                                break;
                        }
                        #endregion

                        break;
                    }
                    //单目取反
                    case '!':
                        operandStack.Push(-((double)operandStack.Pop()));
                        break;
                    default:
                    {
                        if (IsInSupportedArray(nowChar)) //单目函数运算
                        {
                            if (operandStack.Count > 0)
                            {
                                y = (double)operandStack.Pop();
                                switch (nowChar) //这里改对应函数映射关系
                                {
                                    case 'a':
                                        operandStack.Push(Math.Abs(y));
                                        break;
                                    case 'b':
                                        operandStack.Push(Math.Acos(y));
                                        break;
                                    case 'c':
                                        operandStack.Push(Math.Asin(y));
                                        break;
                                    case 'd':
                                        operandStack.Push(Math.Atan(y));
                                        break;
                                    case 'e':
                                        operandStack.Push(Math.Floor(y));
                                        break;
                                    case 'f':
                                        operandStack.Push(Math.Sqrt(y));
                                        break;
                                    case 'g':
                                        operandStack.Push(Math.Log(y));
                                        break;
                                    case 'h':
                                        operandStack.Push(Math.Sin(y));
                                        break;
                                    case 'i':
                                        operandStack.Push(Math.Cos(y));
                                        break;
                                    case 'j':
                                        operandStack.Push(Math.Cos(y) / Math.Sin(y));
                                        break;
                                    case 'k':
                                        operandStack.Push(Math.Tan(y));
                                        break;
                                    case 'l':
                                        operandStack.Push(Math.Log10(y));
                                        break;
                                    case 'm':
                                        operandStack.Push(Math.Ceiling(y));
                                        break;
                                    case 'n':
                                        operandStack.Push(Math.Log(y, Math.E));
                                        break;
                                    default:
                                        break;
                                }
                            }
                            else
                            {
                                return null;//语法有误
                                //operandStack.Push(0);
                                //break;
                            }
                        }

                        break;
                    }
                }
            }
            switch (operandStack.Count)
            {
                case > 1:
                case 0:
                    return null;
                default:
                    return operandStack.Pop();
            }
        }

        private static readonly HashSet<char> _supportedChars = new()
            {'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n'};

        private static bool IsInSupportedArray(char testChar) => _supportedChars.Contains(testChar);

        private static readonly Dictionary<string, string> _supportFunction = new()
        {
            {"abs", "a"},
            {"arccos", "b"},
            {"acos", "b"},
            {"arcsin", "c"},
            {"asin", "c"},
            {"arctan", "d"},
            {"atan", "d"},
            {"floor", "e"},
            {"sqrt", "f"},
            {"log", "g"},
            {"sin", "h"},
            {"sine", "h"},
            {"cos", "i"},
            {"cosine", "i"},
            {"cot", "j"},
            {"cotangent", "j"},
            {"tan", "k"},
            {"tangent", "k"},
            {"tg", "k"},
            {"lg", "l"},
            {"ceiling", "m"},
            {"ln","n" },
            {"pi", Math.PI.ToString(CultureInfo.InvariantCulture)},
            {"e", Math.E.ToString(CultureInfo.InvariantCulture)}
        };

        /// <summary>
        /// 算术逆波兰表达式生成（也叫后缀表达式）Reverse Polish Notation
        /// </summary>
        /// 首先构造一个运算符栈，此运算符在栈内遵循越往栈顶优先级越高的原则
        /// 如果当前字符为变量或者为数字，则压栈，如果是运算符，则将栈顶两个元素弹出作相应运算，结果再入栈，最后当表达式扫描完后，栈里的就是结果。
        /// 
        ///参考 https://bbs.csdn.net/topics/200010856
        private string BuildingRPN(string originExpression)
        {
            originExpression = originExpression.ToLower();//仅支持小写
            #region 先把所有支持的函数名替换为单个字符
            originExpression = StringTool.ToHalfWidth(originExpression);
            if (originExpression.IsMatch("[a-z]"))
            {
                originExpression = originExpression.ReplaceBasedOnDict(_supportFunction);
            }
            #endregion

            var originStr = new StringBuilder(originExpression);//存入要转成的字符串为stringBuilder 
            var operatorStack = new Stack(); //运算符栈
            var parsingStr = new StringBuilder();//存放处理后的结果

            var nowChar = ' '; //遍历用当前字符

            //这一步遍历是挑出有用的字符，去除一些空格或无用字符等干扰因素
            for (var i = 0;
                i < originStr.Length;
                i++)
            {
                nowChar = originStr[i];
                //added c==',' for german culture
                if (char.IsDigit(nowChar) || nowChar == ',')//数字当然要了.
                {
                    parsingStr.Append(nowChar);
                }
                else if (char.IsWhiteSpace(nowChar))
                {
                    continue;
                }
                else if (char.IsLetter(nowChar))//注意可能会有用户乱输入的字母 还未处理
                {
                    parsingStr.Append(nowChar);
                }
                else
                {
                    switch (nowChar)//如果是其它字符...列出的要,没有列出的不要.
                    {
                        case '+':
                        case '-':
                        case '*':
                        case '/':
                        case '%':
                        case '^':
                        case '!':
                        case '(':
                        case ')':
                        case '.':
                            parsingStr.Append(nowChar);
                            break;
                        default:
                            continue;
                    }
                }
            }
            originStr = new StringBuilder(parsingStr.ToString());
            #region 对负号进行预转义处理.负号变单目运算符求反.
            for (var i = 0; i < originStr.Length - 1; i++)
                if (originStr[i] == '-' && (i == 0 || originStr[i - 1] == '('))//这里是支持开头为负号的数字，但中间有数字负号必须用括号括起来
                    originStr[i] = '!';
            //字符转义.
            #endregion
            #region 将中缀表达式变为后缀表达式.
            parsingStr = new StringBuilder();
            for (var i = 0; i < originStr.Length; i++)
            {
                if (char.IsDigit(originStr[i]) || originStr[i] == '.')//如果是数值.
                {
                    parsingStr.Append(originStr[i]);
                    //加入后缀式
                }
                else if (originStr[i] == '+'
                    || originStr[i] == '-'
                    || originStr[i] == '*'
                    || originStr[i] == '/'
                    || originStr[i] == '%'
                    || originStr[i] == '^'
                    || originStr[i] == '!'
                    || IsInSupportedArray(originStr[i]))
                {
                    #region 运算符处理 主要按照优先级刷新运算顺序
                    while (operatorStack.Count > 0) //栈不为空时
                    {
                        nowChar = (char)operatorStack.Pop();
                        //将栈中的操作符弹出.
                        if (nowChar == '(') //如果发现左括号.停.
                        {
                            operatorStack.Push(nowChar);
                            //将弹出的左括号压回.因为还有右括号要和它匹配.
                            break;
                            //中断.
                        }
                        else
                        {
                            if (Power(nowChar) < Power(originStr[i]))//如果优先级比上次的高,则压栈.
                            {
                                operatorStack.Push(nowChar);
                                break;
                            }
                            else
                            {
                                parsingStr.Append(' ');
                                parsingStr.Append(nowChar);
                            }
                            //如果不是左括号,那么将操作符加入后缀式中.
                        }
                    }
                    operatorStack.Push(originStr[i]);
                    //把新操作符入栈.
                    parsingStr.Append(' ');
                    #endregion
                }
                else if (originStr[i] == '(')//基本优先级提升
                {
                    operatorStack.Push('(');
                    parsingStr.Append(' ');
                }
                else if (originStr[i] == ')')//基本优先级下调
                {
                    while (operatorStack.Count > 0) //栈不为空时
                    {
                        nowChar = (char)operatorStack.Pop();
                        //pop Operator
                        if (nowChar != '(')
                        {
                            parsingStr.Append(' ');
                            parsingStr.Append(nowChar);
                            //加入空格主要是为了防止不相干的数据相临产生解析错误.
                            parsingStr.Append(' ');
                        }
                        else
                            break;
                    }
                }
                else
                    parsingStr.Append(originStr[i]);
            }
            while (operatorStack.Count > 0)//这是最后一个弹栈啦.
            {
                parsingStr.Append(' ');
                parsingStr.Append(operatorStack.Pop());
            }
            #endregion
            parsingStr.Append(' ');
            return FormatSpace(parsingStr.ToString());
            //在这里进行一次表达式格式化.这里就是后缀式了.  
        }

        /// <summary>  
        /// 优先级别测试函数.  
        /// </summary>  
        /// <param name="opr"></param>  
        /// <returns></returns>  
        private static int Power(char opr)
        {
            if (IsInSupportedArray(opr))
            {
                return 3;
            }

            switch (opr)
            {
                case '+':
                case '-':
                    return 1;
                case '*':
                case '/':
                    return 2;
                case '%':
                case '^':
                case '!':
                    return 3;
                default:
                    return 0;
            }

        }

        // 规范化逆波兰表达式.
        private static string FormatSpace(string s) //暂时不知道有啥用
        {
            var ret = new StringBuilder();
            for (var i = 0;
                i < s.Length;
                i++)
            {
                if (!(s.Length > i + 1 && s[i] == ' ' && s[i + 1] == ' '))
                    ret.Append(s[i]);
                else //这俩不是一样吗
                    ret.Append(s[i]);
            }
            return ret.ToString();
            //.Replace( '!','-' );
        }

        public string? ErrorMsg { get; set; }
    }
}
