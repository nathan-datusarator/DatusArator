using DatusArator.Core.Util;
using System;
using System.Collections.Generic;

namespace DatusArator.Core.DotPath {
  public class DotPathFilter {
    internal string fCurrentValue = null;
    internal string fTargetValue = null;
    internal bool? fGreaterThan = null;

    public bool MissingValues { get; private set; } = false;

    public bool IsTrue { get; private set; } = false;
    public bool CheckChangesOverTime { get; private set; } = false;
    public bool CheckMissingOverTime { get; private set; } = false;

    public List<ConditionToken> Tokens { get; private set; }
    public List<ConditionToken> Postfix { get; private set; }

    public DotPathFilter(string condition, object dataSource) {
      if (!string.IsNullOrEmpty(condition)) {
        Tokens = Tokenize(condition);

        try {
          CreatePostfix();
        } catch (Exception) {
        }

        if (dataSource != null)
          IsTrue = Evaluate(dataSource);

        ConditionToken firstConst = null;
        foreach (var token in Tokens) {
          if (token.TokenType == ConditionTokenType.VAR) {
            if (fCurrentValue == null)
              fCurrentValue = token.Value;
            else if (fTargetValue == null)
              fTargetValue = token.Value;
          } else if ((token.TokenType == ConditionTokenType.CONST) && (firstConst == null)) {
            firstConst = token;
          } else if ((token.TokenType == ConditionTokenType.OP)) {
            if ("~".Equals(token.Token))
              CheckChangesOverTime = true;
            else if ("@".Equals(token.Token))
              CheckMissingOverTime = true;

            if (fGreaterThan == null)
              fGreaterThan = (">".Equals(token.Token) || ">=".Equals(token.Token));
          }
        }

        if ((fTargetValue == null) && (firstConst != null))
          fTargetValue = firstConst.Token;
      }

      if (fGreaterThan == null)
        fGreaterThan = false;
    }

    public bool Evaluate(object dataSource) {
      foreach (ConditionToken token in Postfix) {
        if (token.TokenType == ConditionTokenType.VAR) {
          if (dataSource is DotPathObject)
            token.Value = (dataSource as DotPathObject).GetValue(token.Token)?.ToString();
          else
            token.Value = DotPathObject.GetValue(dataSource, token.Token)?.ToString();
        }

        token.MissingValues = false;
      }


      MissingValues = false;
      var stack = new Stack<ConditionToken>();
      foreach (ConditionToken token in Postfix) {
        switch (token.TokenType) {
          case ConditionTokenType.OP:
            ConditionToken a = stack.Pop();
            ConditionToken b = stack.Pop();

            stack.Push(token.Evaluate(b, a));
            MissingValues = MissingValues || token.MissingValues;

            break;
          case ConditionTokenType.FUNC:
            stack.Push(token.Evaluate(stack.Pop(), null));
            MissingValues = MissingValues || token.MissingValues;

            break;
          default:
            stack.Push(token);
            break;
        }

        if (MissingValues)
          return false;
      }

      return StringUtils.IsTrue(stack.Pop().Token);
    }

    #region Create Postfix
    private void CreatePostfix() {
      Postfix = new List<ConditionToken>();
      var stack = new Stack<ConditionToken>();

      foreach (var token in Tokens) {
        switch (token.TokenType) {
          case ConditionTokenType.VAR:
          case ConditionTokenType.CONST:
            Postfix.Add(token);
            break;
          case ConditionTokenType.FUNC:
          case ConditionTokenType.LEFT_PAREN:
            stack.Push(token);
            break;
          case ConditionTokenType.RIGHT_PAREN:
            while ((stack.Count > 0) && (stack.Peek().TokenType != ConditionTokenType.LEFT_PAREN))
              Postfix.Add(stack.Pop());
            stack.Pop();  // Throw away the LEFT PAREN
            break;
          case ConditionTokenType.OP:
            if ((stack.Count == 0) || (stack.Peek().TokenType == ConditionTokenType.LEFT_PAREN))
              stack.Push(token);
            else {
              while ((stack.Count > 0) && (stack.Peek().TokenType != ConditionTokenType.LEFT_PAREN) && IsLowerPrecedence(token, stack.Peek()))
                Postfix.Add(stack.Pop());
              stack.Push(token);
            }
            break;
        }
      }

      while (stack.Count > 0)
        Postfix.Add(stack.Pop());
    }

    private bool IsLowerPrecedence(ConditionToken token, ConditionToken stackToken) {
      int precedence = MapPrecedence(token);
      int stackPrecedence = MapPrecedence(stackToken);

      return precedence >= stackPrecedence;
    }

    private int MapPrecedence(ConditionToken token) {
      if (token.TokenType == ConditionTokenType.FUNC)
        return 0;

      switch (token.Token) {
        case "?":
          return 1;
        case "*":
        case "/":
          return 1;
        case "+":
        case "-":
          return 2;
        case "&":
        case "|":
          return 4;
        default:
          return 3;
      }
    }
    #endregion

    #region Tokenize
    List<ConditionToken> Tokenize(String dataCondition) {
      var result = new List<ConditionToken>();

      char tokenDelim = '\0';
      String token = "";
      for (int i = 0; i < dataCondition.Length; i++) {
        char c = dataCondition[i] ;

        // Finished the current delimited token
        if (c == tokenDelim) {
          AddToken(result, token, c == ']' ? ConditionTokenType.VAR : ConditionTokenType.CONST);
          token = "";
          tokenDelim = '\0';

          continue;
          // Still on a delimited token
        } else if (tokenDelim != '\0') {
          token += c;
          continue;
        }

        switch (c) {
          case ' ':
            AddToken(result, token, ConditionTokenType.CONST);
            token = "";
            break;

          case '(':
            AddToken(result, token, ConditionTokenType.FUNC);
            token = "";
            AddToken(result, "(", ConditionTokenType.LEFT_PAREN);
            break;
          case ')':
            AddToken(result, token, ConditionTokenType.CONST);
            token = "";
            AddToken(result, ")", ConditionTokenType.RIGHT_PAREN);
            break;
          case '-':
            AddToken(result, token, ConditionTokenType.CONST);
            token = "";

            if ((result.Count > 0) && (result[result.Count - 1].TokenType == ConditionTokenType.OP))
              AddToken(result, c.ToString(), ConditionTokenType.FUNC);
            else
              AddToken(result, c.ToString(), ConditionTokenType.OP);
            break;
          case '+':
          case '*':
          case '/':
          case '~':
          case '?':
          case '@':
          case '=':
            AddToken(result, token, ConditionTokenType.CONST);
            token = "";
            AddToken(result, c.ToString(), ConditionTokenType.OP);
            break;

          case '&':
          case '|':
            AddToken(result, token, ConditionTokenType.CONST);
            token = "";

            if (((i + 1) < dataCondition.Length) && (dataCondition[i + 1] == c)) {
              i += 1;
            }

            AddToken(result, c.ToString(), ConditionTokenType.OP);
            break;

          case '<':
          case '>':
          case '!':
            AddToken(result, token, ConditionTokenType.CONST);
            token = "";

            String op = c.ToString();
            if (((i + 1) < dataCondition.Length) && (dataCondition[i + 1] == '=')) {
              op += '=';
              i += 1;
            }

            AddToken(result, op, "!".Equals(op) ? ConditionTokenType.FUNC : ConditionTokenType.OP);

            break;

          // Delimited Words / phrases
          case '[':
            AddToken(result, token, ConditionTokenType.CONST);
            token = "";
            tokenDelim = ']';
            break;
          case '\'':
          case '"':
            AddToken(result, token, ConditionTokenType.CONST);
            token = "";
            tokenDelim = c;
            break;

          default:
            token += c;
            break;
        }
      }

      AddToken(result, token, ConditionTokenType.CONST);

      return result;
    }

    void AddToken(List<ConditionToken> result, String token, ConditionTokenType tokenType) {
      if (token == null)
        return;

      token = token.Trim();

      if (!"".Equals(token))
        result.Add(new ConditionToken(token, tokenType));
    }
    #endregion
  }

  public class ConditionToken {
    public ConditionTokenType TokenType { get; private set; }
    public string Token { get; private set; }

    private string fValue;
    public string Value {
      get { return TokenType == ConditionTokenType.VAR ? fValue : Token; }
      set { fValue = value; }
    }

    public bool MissingValues { get; set; } = false;

    public ConditionToken(string token, ConditionTokenType tokenType) {
      TokenType = tokenType;
      Token = token;
      fValue = null;
    }

    public ConditionToken Evaluate(ConditionToken a, ConditionToken b) {
      Object result = null;

      switch (Token) {
        case "ABS":
          if (CheckValues(a, null))
            result = Math.Abs(Double.Parse(a.Value));
          break;
        case "!":
          if (CheckValues(a, null))
            result = !StringUtils.IsTrue(a.Value);
          break;
        case "/":
          if (CheckValues(a, b))
            result = Double.Parse(a.Value) / Double.Parse(b.Value);
          break;
        case "*":
          if (CheckValues(a, b))
            result = Double.Parse(a.Value) * Double.Parse(b.Value);
          break;
        case "+":
          if (CheckValues(a, b))
            result = Double.Parse(a.Value) + Double.Parse(b.Value);
          break;
        case "-":
          if (b == null) {
            if (CheckValues(a, null))
              result = -1.0 * Double.Parse(a.Value);
          } else if (CheckValues(a, b))
            result = Double.Parse(a.Value) - Double.Parse(b.Value);
          break;
        case ">":
          if (CheckValues(a, b))
            result = Double.Parse(a.Value) > Double.Parse(b.Value);
          break;
        case ">=":
          if (CheckValues(a, b))
            result = Double.Parse(a.Value) >= Double.Parse(b.Value);
          break;
        case "<":
          if (CheckValues(a, b))
            result = Double.Parse(a.Value) < Double.Parse(b.Value);
          break;
        case "<=":
          if (CheckValues(a, b))
            result = Double.Parse(a.Value) <= Double.Parse(b.Value);
          break;
        case "!=":
        case "=":
          if (CheckValues(a, b)) {
            double? dVal1 = StringUtils.SafeStrToDouble(a.Value, null);
            double? dVal2 = StringUtils.SafeStrToDouble(b.Value, null);

            bool value;
            if ((dVal1 != null) && (dVal2 != null))
              value = ((dVal1 != null) && (dVal2 != null)) ? (Math.Abs(dVal1.Value - dVal2.Value) < 0.0001) : a.Value.Equals(b.Value, StringComparison.CurrentCultureIgnoreCase);
            else
              value = a.Value.ToString().Equals(b.Value.ToString(), StringComparison.InvariantCultureIgnoreCase);

            result = (Token == "!=") ? !value : value;
          }
          break;
        case "&":
          if (CheckValues(a, b))
            result = StringUtils.IsTrue(a.Value) && StringUtils.IsTrue(b.Value);
          break;
        case "|":
          if (CheckValues(a, b))
            result = StringUtils.IsTrue(a.Value) || StringUtils.IsTrue(b.Value);
          break;
        case "?":
          if (a.Value == null) {
            if (CheckValues(b, null))
              result = b.Value;
          } else {
            result = a.Value;
          }
          break;
        case "@":
        case "~":
          CheckValues(a, b);  // Need to still check missing for further up
          result = false;  // Just return false.  Next level needs to have a secondary check
          break;
        default:
          result = "?";
          Console.WriteLine("Unkown Operator: " + Token);
          break;
      }

      return (result != null) ? new ConditionToken(result.ToString(), ConditionTokenType.CONST) : null;
    }

    private bool CheckValues(ConditionToken a, ConditionToken b) {
      if (a.Value == null) {
        MissingValues = true;
        return false;
      }

      if ((b != null) && (b.Value == null)) {
        MissingValues = true;
        return false;
      }

      return true;
    }

    public override String ToString() {
      return Token + "[" + TokenType + "]" + (TokenType == ConditionTokenType.VAR ? "(" + Value + ")" : "");
    }

  }

  public enum ConditionTokenType { VAR, CONST, OP, FUNC, LEFT_PAREN, RIGHT_PAREN }
}
