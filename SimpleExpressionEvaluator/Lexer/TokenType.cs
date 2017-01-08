﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleExpressionEvaluator.Lexer
{ 
    public enum TokenType
    {
        EOF = -1,                
        OPENBRACKET = 1,
        CLOSEBRACKET = 2,
        POINT = 3, 
        PLUS = 4,  
        MINUS = 5,
        MUL = 6,  
        DIV = 7,
        PROZ = 8,        
        EQUAL = 9,
        UNEQUAL = 10,
        GREATERTHEN = 11,
        GREATERTHENOREQUAL = 12,
        SMALLERTHEN = 13,
        SMALLERTHENOREQUAL = 14,
        AND = 15,
        OR = 16,              
        TRUE = 17,
        FALSE = 18,
        IDENT = 19,
        INTEGER = 20,
        DOUBLE = 21,
        STRING = 22,
        MOD = 23,
        BOOL = 24,
        SET = 25,
        THEN = 26,
        ELSE = 27,
        IS = 28,
        COMMA = 29,
        NULL = 30,
        LIKE = 31
    }
}
